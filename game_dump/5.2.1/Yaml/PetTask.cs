using System.Collections.Generic;
using Durango.Utils;
using NCalc;
using Newtonsoft.Json;
using Shared.Animal;

namespace Yaml;

public class PetTask
{
	[JsonProperty(PropertyName = "type")]
	public PetTaskType Type;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "duration")]
	public float Duration;

	[JsonProperty(PropertyName = "exp")]
	public int Exp;

	[JsonProperty(PropertyName = "hungry_required")]
	public float HungryRequired;

	[JsonProperty(PropertyName = "produced_prototype")]
	public Dictionary<string, float[]> ProducedPrototype;

	[JsonProperty(PropertyName = "random_prototype")]
	public Dictionary<string, float[]> RandomPrototype;

	[JsonProperty(PropertyName = "unlock_level")]
	public int UnlockLevel;

	private Expression _productQuantityExpresion;

	[JsonProperty(PropertyName = "product_quantity")]
	public string ProductQuantity
	{
		set
		{
			_productQuantityExpresion = ExpressionParser.Parse(value);
		}
	}

	public int GetProductQuantity(float quantity)
	{
		if (_productQuantityExpresion == null)
		{
			return 0;
		}
		_productQuantityExpresion.Parameters["animal_product_quantity"] = quantity;
		return _productQuantityExpresion.ToInt32();
	}
}
