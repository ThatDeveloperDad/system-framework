using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using ThatDeveloperDad.iFX.CollectionUtilities.Operators;

namespace ThatDeveloperDad.iFX.CollectionUtilities;

public abstract class Filter
{
    protected readonly List<FilterCriteria> _filterCriteria;
    protected readonly Type _filterSubjectType;
    protected readonly PropertyInfo[] _subjectProperties;

    protected string _materializedFilter = string.Empty;

    public Filter(Type filterSubjectType)
    {
        _filterSubjectType = filterSubjectType;
        _subjectProperties = _filterSubjectType.GetProperties();
        _filterCriteria = new List<FilterCriteria>();
    }
    
    public void AddCriteria(string propertyName, OperatorKinds operatorKind, object? expectedValue)
    {
        // Validate first.  If anything's wrong here,
        // let's throw up all over the place. XD
        // Sure would be nice if we had some way to do this
        // so that it would validate in the IDE, wouldn't it?
        Type? propertyType = GetPropertyTypeOrThrow(propertyName);
        GuardPropertyValueCompatibility(expectedValue, propertyType);
        GuardPropertyOperatorCompatibility(operatorKind, propertyType);

        FilterCriteria newCriteria = new();
        newCriteria.PropertyName = propertyName;
        newCriteria.Operator = operatorKind;
        newCriteria.ExpectedValue = expectedValue;
        newCriteria.PropertyType = propertyType;
        _filterCriteria.Add(newCriteria);

        _materializedFilter = string.Empty;
        BuildFilter();
    }

    private Type GetPropertyTypeOrThrow(string propertyName)
    {
        Type? propertyType = _subjectProperties.SingleOrDefault(p => p.Name == propertyName)?.PropertyType;
        if (propertyType == null)
        {
            throw new ArgumentException($"Property {propertyName} not found on {_filterSubjectType.Name}");
        }
        return propertyType;
    }

    private void GuardPropertyValueCompatibility(object? expectedValue, Type propertyType)
    {
        if (expectedValue == null)
        {
            return;
        }

        if(expectedValue.GetType() == propertyType)
        {
            return;
        }

        if(IsCollection(propertyType) || IsCollection(expectedValue))
        {
            GuardCollectionCompatibility(expectedValue, propertyType);
            return;
        }
        
        try
        {
            Convert.ChangeType(expectedValue, propertyType);
        }
        catch
        {
            throw new ArgumentException($"Expected value {expectedValue} cannot be converted to and instance of {propertyType.Name}");
        }
    }

    private bool IsCollection(object? obj)
    {
        Type objType = obj?.GetType() ?? typeof(object);
        bool isCollection = objType.IsAssignableTo(typeof(IEnumerable<>))
                          || objType.IsAssignableTo(typeof(Array));
        return isCollection;
    }

    private void GuardCollectionCompatibility(object? expectedValue, Type propertyType)
    {
        //First, let's make sure the MemberExpression and COnstantExpression types are appropriate.
        Type propType = propertyType;
        Type constantType = expectedValue?.GetType()??typeof(object);

        Type collectionType = IsCollection(propType) ? propType : constantType;
        Type memberType = IsCollection(propType) ? constantType : propType;
        
        // Make sure the "Expected" thing contains values that are assignable to the Member type.
        if(collectionType.GetGenericArguments().Any(t => t.IsAssignableTo(memberType) == false))
        {
            throw new ArgumentException("The expected value for this criterion must contain values that are compatible with the Property type.");
        }
    }

    private void GuardPropertyOperatorCompatibility(OperatorKinds operatorKind, Type propertyType)
    {
        OperatorBase op = OperatorFactory.GetOperator(operatorKind);
        if(op.RequiresCollection)
        {
            // We have to be more sophisticated from collection operations.
            return;
        }

        if (!op.SupportedTypes.Contains(propertyType))
        {
            throw new ArgumentException($"Operator {operatorKind} is not supported for properties of type {propertyType.Name}");
        }
    }

    protected abstract void BuildFilter();

    /// <summary>
    /// Returns the Filter and its criteria as a simple string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder sb = new($"Filter for {_filterSubjectType.Name}: ");
        sb.AppendLine();
        int lineNumber = 0;
        foreach (var criteria in _filterCriteria)
        {
            string spacer = lineNumber == 0 ? "    " : "AND ";
            string criteriaDescription = 
                $"{spacer}{criteria.PropertyName} {criteria.Operator} {criteria.ExpectedValue}";
            sb.AppendLine(criteriaDescription);
            lineNumber++;
        }
        sb.AppendLine();
        sb.AppendLine($"Materialized Filter: {_materializedFilter}");

        return sb.ToString();
    }
}

public class Filter<T> : Filter
    where T: class
{
    private Func<T, bool>? _filterQuery;
    private ParameterExpression _collectionTypeExpression;

    internal List<FilterCriteria> Criteria => _filterCriteria;

    public Filter() : base(typeof(T))
    {
        _collectionTypeExpression = Expression.Parameter(typeof(T), "x");
    }

    protected override void BuildFilter()
    {
        _filterQuery = BuildExpressionTree();
    }

    public IEnumerable<T> ApplyFilter(IEnumerable<T> collection)
    {
        var filtered = new List<T>();

        var query = _filterQuery??BuildExpressionTree();
        filtered.AddRange(collection.Where(query));

        return filtered;
    }

    private Func<T, bool> BuildExpressionTree()
    {
        Func<T, bool> query = (T t) => true;
        // If there are no criteria, then we return an expression that always returns true.
        if(_filterCriteria.Count == 0)
        {
            return query;
        }

        // Reference: https://learn.microsoft.com/en-us/dotnet/csharp/linq/how-to-build-dynamic-queries
        // Scroll about halfway down to "Construct a full query at run time".
        List<Expression> filterClauses = new();
        
        // AsExpressionFor is a method on FilterCriteria that returns that Criteria
        // as an Expression we can add to a LINQ query.
        filterClauses = _filterCriteria
            .Select(fc=> fc.AsExpressionFor(_collectionTypeExpression))
            .ToList();

        // For now, we'll combine all expressions with AND.
        // We can get fancy later on and create CriteriaGroups that can
        // be combined in all kinds of ways if we need to.
        Expression composedFilterBody = filterClauses
            .Aggregate((prev, current) => Expression.AndAlso(prev, current));
        
        // Now we can create the Lambda.
        Expression<Func<T, bool>> lambda = Expression
            .Lambda<Func<T, bool>>(
                composedFilterBody, 
                _collectionTypeExpression);
        
        query = lambda.Compile();
        // Store the stringified version for logging and debugging.
        _materializedFilter = lambda.ToString();
        return query;
    }
}