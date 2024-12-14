using System.Linq.Expressions;

namespace ThatDeveloperDad.iFX.CollectionUtilities.Operators;

internal static class OperatorKindsExtensions
{
    /// <summary>
    /// Uses member and constant expressions to create
    /// a BinaryExpression based on the OperatorKind.
    /// </summary>
    /// <param name="opKind"></param>
    /// <param name="member"></param>
    /// <param name="constant"></param>
    /// <returns></returns>
    public static Expression AsExpression(
        this OperatorKinds opKind,
        MemberExpression member,
        ConstantExpression constant)
        {
            Expression expr;

            OperatorBase op = OperatorFactory.GetOperator(opKind);
            expr = op.AsExpression(member, constant);

            return expr;
        } 
}
