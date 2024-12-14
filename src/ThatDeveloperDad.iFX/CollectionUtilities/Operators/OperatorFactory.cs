namespace ThatDeveloperDad.iFX.CollectionUtilities.Operators;

internal class OperatorFactory
{
    /// <summary>
    /// Returns an instance of OperatorBase that implements the
    /// OperatorKind provided.
    /// </summary>
    /// <param name="opKind"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static OperatorBase GetOperator(OperatorKinds opKind)
    {
        OperatorBase op = opKind switch
        {
            OperatorKinds.Equals => new EqualOperator(),
            OperatorKinds.GreaterThan => new GreaterThanOperator(),
            OperatorKinds.LessThan => new LessThanOperator(),
            OperatorKinds.GreaterOrEquals => new GreaterOrEqualsOperator(),
            OperatorKinds.LessOrEquals => new LessOrEqualsOperator(),
            OperatorKinds.Contains => new ContainsOperator(),
            OperatorKinds.IsContainedIn => new IsContainedInOperator(),
            OperatorKinds.NotEquals => new NotEqualsOperator(),
            _ => throw new NotImplementedException()
        };

        return op;
    }
}
