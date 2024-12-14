using System.Collections;
using System.Linq.Expressions;
using System.Reflection;


namespace ThatDeveloperDad.iFX.CollectionUtilities.Operators;

public abstract class OperatorBase
{
    protected OperatorKinds _operatorKind;
    protected OperatorBase(OperatorKinds operatorKind)
    {
        _operatorKind = operatorKind;
    }

    public abstract Expression AsExpression(MemberExpression member, ConstantExpression constant);

    public abstract Type[] SupportedTypes { get; }   

    public virtual bool RequiresCollection => false;
}

public class EqualOperator : OperatorBase
{

    public EqualOperator():base(OperatorKinds.Equals) {}

    public override BinaryExpression AsExpression(MemberExpression member, ConstantExpression constant)
    {
        return Expression.Equal(member, constant);
    }

    public override Type[] SupportedTypes 
        => [
                typeof(object),
                typeof(string),
                typeof(int),
                typeof(long),
                typeof(decimal),
                typeof(float),
                typeof(double),
                typeof(DateTime),
                typeof(bool)
            ];
}

public class LessThanOperator : OperatorBase
{
    public LessThanOperator() : base(OperatorKinds.LessThan)
    {
    }

    public override Type[] SupportedTypes 
        => [
                typeof(int),
                typeof(long),
                typeof(decimal),
                typeof(float),
                typeof(double),
                typeof(DateTime)
        ];

    public override BinaryExpression AsExpression(MemberExpression member, ConstantExpression constant)
    {
        return Expression.LessThan(member, constant);
    }
}

public class GreaterThanOperator : OperatorBase
{
    public GreaterThanOperator() : base(OperatorKinds.GreaterThan)
    {
    }

    public override Type[] SupportedTypes 
        => [
                typeof(int),
                typeof(long),
                typeof(decimal),
                typeof(float),
                typeof(double),
                typeof(DateTime)
        ];

    public override BinaryExpression AsExpression(MemberExpression member, ConstantExpression constant)
    {
        return Expression.GreaterThan(member, constant);
    }
}

public class GreaterOrEqualsOperator : OperatorBase
{
    public GreaterOrEqualsOperator() : base(OperatorKinds.GreaterOrEquals)
    {
    }

    public override Type[] SupportedTypes 
        => [
                typeof(int),
                typeof(long),
                typeof(decimal),
                typeof(float),
                typeof(double),
                typeof(DateTime)
        ];

    public override BinaryExpression AsExpression(MemberExpression member, ConstantExpression constant)
    {
        return Expression.GreaterThanOrEqual(member, constant);
    }
}

public class LessOrEqualsOperator : OperatorBase
{
    public LessOrEqualsOperator() : base(OperatorKinds.LessOrEquals)
    {
    }

    public override Type[] SupportedTypes 
        => [
                typeof(int),
                typeof(long),
                typeof(decimal),
                typeof(float),
                typeof(double),
                typeof(DateTime)
        ];

    public override BinaryExpression AsExpression(MemberExpression member, ConstantExpression constant)
    {
        return Expression.LessThanOrEqual(member, constant);
    }
}

public class NotEqualsOperator : OperatorBase
{
    public NotEqualsOperator() : base(OperatorKinds.NotEquals)
    {
    }

    public override Type[] SupportedTypes 
        => [
                typeof(object),
                typeof(string),
                typeof(int),
                typeof(long),
                typeof(decimal),
                typeof(float),
                typeof(double),
                typeof(DateTime),
                typeof(bool)
            ];

    public override BinaryExpression AsExpression(MemberExpression member, ConstantExpression constant)
    {
        return Expression.NotEqual(member, constant);
    }
}

/// <summary>
/// This operator allows us to ensure that a string contains a pre-defined substring.
/// </summary>
public class ContainsOperator : OperatorBase
{
    public ContainsOperator() : base(OperatorKinds.Contains)
    {
    }

    public override Type[] SupportedTypes 
        => [
                typeof(string),
                typeof(IEnumerable)
            ];

    /// <summary>
    /// This one's actually an inverse of the other operators.  We're going to use the
    /// we need to 
    /// </summary>
    /// <param name="member"></param>
    /// <param name="constant"></param>
    /// <returns></returns>
    public override Expression AsExpression(MemberExpression member, ConstantExpression constant)
    {
        if(member.Type != typeof(string))
        {
            throw new NotSupportedException("The Contains operator is only supported for string properties.");
        }
        // This will work great for strings, but it'd be nice to have something we can apply
        // to other collection types.
        Type exprType = typeof(string);
        // Gonna use the ! null-forgiving operator on this next line, because DUH!
        MethodInfo containsMethod = exprType.GetMethod(nameof(string.Contains), new[] { exprType })!;

        // This next line turns:  Object.StringProperty.Contains("expected")
        // Into a LINQ expression that can be combined with other expressions
        // and compiled into a dynamically built predicate function AT RUN TIME.
        return Expression.Call(member, containsMethod, constant);
    }
}

public class IsContainedInOperator : OperatorBase
{
    public IsContainedInOperator() : base(OperatorKinds.IsContainedIn) { }

    public override bool RequiresCollection=> true;

    public override Type[] SupportedTypes 
        => [
                typeof(string),
                typeof(IEnumerable),
                typeof(Array)
            ];

    public override Expression AsExpression(MemberExpression member, ConstantExpression constant)
    {
        //First, let's make sure the MemberExpression and COnstantExpression types are appropriate.
        Type memberType = member.Type;
        Type constantType = constant.Type;

        // Make sure the "Expected" is some kind of collection or array.
        GuardConstantIsCollection(constantType);
        // Make sure the "Expected" thing contains values that are assignable to the Member type.
        GuardExpressionsAreTypeCompatible(memberType, constantType);

        MethodInfo containsMethod = GetContainsMethod(memberType);
        
        MethodCallExpression operation = Expression.Call(
            instance: null,
            method: containsMethod,
            arg0: constant,
            arg1: member);

        return operation;
    }

    private static MethodInfo GetContainsMethod(Type elementType)
    {
        MethodInfo? baseMethod = typeof(Enumerable)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == nameof(Enumerable.Contains)
                                     && m.GetParameters().Length == 2);
        // baseMethod now holds an abstract, NOT TYPED version of:  
        // Enumerable.Contains<T>(IEnumerable<T> collection, T value)
        // where T is some Type that we aren't yet aware of.

        MethodInfo? containsMethod = baseMethod?.MakeGenericMethod(elementType);
        // By calling "MakeGenericMethod" on baseMethod, we're getting the REAL
        // Contains function that we want to work with.
        // For example, if elementType is an int, then our constainsMethod will
        // actually be:
        // Enumerable.Contains<int>(IEnumerable<int> collection, int value)
        // Same concept holds true for other types, including custom, complex types.

        if (containsMethod == null)
        {
            throw new ArgumentException("The expected value's type does not include a Contains operator.");
        }
        return containsMethod;
    }

    private static void GuardExpressionsAreTypeCompatible(Type memberType, Type constantType)
    {
        if (constantType.GetGenericArguments().Any(t => t.IsAssignableTo(memberType) == false))
        {
            throw new ArgumentException("The expected value for this criterion must contain values that are compatible with the Property type.");
        }
    }

    private static void GuardConstantIsCollection(Type constantType)
    {
        if (constantType.IsAssignableTo(typeof(IEnumerable)) == false
                    && constantType.IsArray == false)
        {
            throw new ArgumentException("The expected value for this criterion must be an Enumerable or Array.");
        }
    }
}