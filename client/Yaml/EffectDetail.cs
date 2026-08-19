using System;
using Durango.Utils;
using NCalc;
using Newtonsoft.Json;
using Shared.StatusEffect;

namespace Yaml;

public class EffectDetail
{
	[JsonProperty(PropertyName = "type")]
	public EffectType Type;

	[JsonProperty(PropertyName = "key")]
	public string Key;

	private Expression _expression;

	[JsonProperty(PropertyName = "value")]
	public string Value { private get; set; }

	public float GetValue(int level)
	{
		if (_expression == null)
		{
			_expression = ExpressionParser.Parse(Value);
		}
		_expression.Parameters["level"] = level;
		return Convert.ToSingle(_expression.Evaluate());
	}
}
