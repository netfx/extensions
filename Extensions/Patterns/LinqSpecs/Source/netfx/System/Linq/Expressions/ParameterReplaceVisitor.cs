using System.Linq.Expressions;

// Code copied from http://stackoverflow.com/questions/26439718/specification-patternmultitype-composite-specifications
internal class ParameterReplaceVisitor : ExpressionVisitor
{
	private readonly ParameterExpression _parameter;
	private readonly Expression _replacementExpression;

	public ParameterReplaceVisitor(ParameterExpression parameter, Expression replacementExpression)
	{
		_parameter = parameter;
		_replacementExpression = replacementExpression;
	}

	protected override Expression VisitParameter(ParameterExpression node)
	{
		return (node == _parameter) ? _replacementExpression : node;
	}
}