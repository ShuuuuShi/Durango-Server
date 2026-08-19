using Durango.Utils;
using NCalc;
using Newtonsoft.Json;

namespace Yaml;

public class FactionSupport
{
	private Expression _supportLevelExpression;

	[JsonProperty(PropertyName = "support_level_step", Required = Required.Always)]
	public int SupportLevelStep;

	[JsonProperty(PropertyName = "support_level", Required = Required.Always)]
	public string SupportLevel
	{
		set
		{
			_supportLevelExpression = ExpressionParser.Parse(value);
		}
	}

	public int GetSupportLevel(int level)
	{
		_supportLevelExpression.Parameters.Clear();
		_supportLevelExpression.Parameters["support_level_step"] = SupportLevelStep;
		_supportLevelExpression.Parameters["level"] = level;
		return _supportLevelExpression.ToInt32();
	}
}
