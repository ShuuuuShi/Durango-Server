using Durango.Utils;
using NCalc;
using Newtonsoft.Json;

namespace Yaml;

public class PetExpTable
{
	private Expression _requiredExpExpresion;

	[JsonProperty(PropertyName = "required_exp")]
	public string RequiredExp
	{
		set
		{
			_requiredExpExpresion = ExpressionParser.Parse(value);
		}
	}

	public int GetRequiredExp(int level)
	{
		if (_requiredExpExpresion == null)
		{
			return 0;
		}
		_requiredExpExpresion.Parameters["level"] = (double)level;
		return _requiredExpExpresion.ToInt32();
	}
}
