using Durango.Utils;
using NCalc;
using Newtonsoft.Json;

namespace Yaml;

public struct DurabilityResult
{
	[JsonProperty(PropertyName = "failure", Required = Required.Always)]
	public string Failure;

	private bool _failureFlag;

	private Expression _failureExpression;

	public float GetFailureDurability(float maxDurability)
	{
		if (!_failureFlag)
		{
			_failureFlag = true;
			_failureExpression = ExpressionParser.Parse(Failure);
		}
		if (_failureExpression == null)
		{
			return 0f;
		}
		_failureExpression.Parameters["max_durability"] = maxDurability;
		return _failureExpression.ToSingle();
	}
}
