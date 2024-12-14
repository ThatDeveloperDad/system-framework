using System.Linq.Expressions;
using ThatDeveloperDad.iFX.CollectionUtilities.Operators;

namespace ThatDeveloperDad.iFX.CollectionUtilities;

public class FilterCriteria
{
    public string PropertyName { get; set; } = string.Empty;

    public Type PropertyType { get; set; } = typeof(object);

    public OperatorKinds Operator { get; set; } = OperatorKinds.Equals;

    public object? ExpectedValue { get; set; } = default;

    /// <summary>
    /// Converts the FilterCriteria into a Linq Expression against the given ParameterExpression
    /// </summary>
    /// <param name="paramEx"></param>
    /// <returns></returns>
    public Expression AsExpressionFor(ParameterExpression paramEx)
    {
        MemberExpression member = Expression.Property(paramEx, PropertyName);
        ConstantExpression constant = CreateConstantExpression();
        
        Expression body = Operator.AsExpression(member, constant);
        return body;
    }

    private ConstantExpression CreateConstantExpression()
    {
        // Some of our Operators involve a Collection Type as one of the operands,
        // and in those cases, we won't be able to create a ConstantExpression
        // directly.

        if(OperatorFactory.GetOperator(Operator).RequiresCollection)
        {
            Type collectionType = ExpectedValue?.GetType() ?? typeof(object);
            return Expression.Constant(ExpectedValue, collectionType);
        }

        return Expression.Constant(ExpectedValue, PropertyType);
    }
}
