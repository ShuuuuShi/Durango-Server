using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Rank;

namespace Yaml;

public struct Season2
{
	public enum Quantity
	{
		Few,
		Several,
		Lots
	}

	public struct WeatherInfo
	{
		[JsonProperty(PropertyName = "weather_id")]
		public string WeatherId;
	}

	[JsonProperty(PropertyName = "alpha_stone")]
	public ushort AlphaStoneType;

	[JsonProperty(PropertyName = "bravo_stone")]
	public ushort BravoStoneType;

	[JsonProperty(PropertyName = "charlie_stone")]
	public ushort CharlieStoneType;

	[JsonProperty(PropertyName = "entree_level_limit")]
	public int EntreeLevelLimit;

	[JsonProperty(PropertyName = "resource_quantity")]
	public Dictionary<string, float> ResourceQuantity;

	[JsonProperty(PropertyName = "voucher")]
	public Season2Voucher Voucher;

	[JsonProperty(PropertyName = "ranking")]
	public List<Category> Rankings;

	[JsonProperty(PropertyName = "storm_sign_time")]
	public int StormSignTime;

	[JsonProperty(PropertyName = "weathers")]
	public Dictionary<int, WeatherInfo> Weathers;

	public Quantity CalcQuantity(float resource)
	{
		if (resource > ResourceQuantity["lots"])
		{
			return Quantity.Lots;
		}
		if (resource > ResourceQuantity["several"])
		{
			return Quantity.Several;
		}
		return Quantity.Few;
	}
}
