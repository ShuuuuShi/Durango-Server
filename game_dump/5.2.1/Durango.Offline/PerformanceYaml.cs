using System.Collections.Generic;
using Durango.Utils;
using Newtonsoft.Json;

namespace Durango.Offline;

public static class PerformanceYaml
{
	public class Performance
	{
		[JsonProperty("add_on")]
		public Dictionary<string, Dictionary<string, AddOn>> AddOnDict;

		[JsonProperty("weapon")]
		public Dictionary<string, Dictionary<string, Weapon>> WeaponDict;

		[JsonProperty("armor")]
		public Dictionary<string, Dictionary<string, Armor>> ArmorDict;

		[JsonProperty("instrument")]
		public Dictionary<string, Dictionary<string, Instrument>> InstrumentDict;

		[JsonProperty("reins")]
		public Dictionary<string, Dictionary<string, Rein>> ReinsDict;

		[JsonProperty("food")]
		public Dictionary<string, Dictionary<string, Food>> FoodDict;

		[JsonProperty("pet_food")]
		public Dictionary<string, Dictionary<string, PetFood>> PetFoodDict;
	}

	public class AddOn
	{
		[JsonProperty("add_on_model_key")]
		public string AddOnModelKey;
	}

	public class Weapon
	{
		[JsonProperty("weapon_framework")]
		public string WeaponFramework;

		[JsonProperty("model")]
		public string Model;

		[JsonProperty("slot")]
		public string Slot;

		[JsonProperty("attack")]
		public string Attack;

		[JsonProperty("battle_speed")]
		public string BattleSpeed;

		[JsonProperty("attack_type")]
		public string AttackType;

		[JsonProperty("icon")]
		public string Icon;

		[JsonProperty("attack_rating")]
		public string AttackRating;

		[JsonProperty("accuracy")]
		public string Accuracy;
	}

	public class Armor
	{
		[JsonProperty("emotional_motions")]
		public string[] EmotionalMotions;

		[JsonProperty("female_model")]
		public string FemaleModel;

		[JsonProperty("male_model")]
		public string MaleModel;

		[JsonProperty("slot")]
		public string Slot;
	}

	public class Instrument
	{
		[JsonProperty("timbre")]
		public string Timbre;
	}

	public class Rein
	{
		[JsonProperty("pet_name")]
		public Gettext PetName;

		[JsonProperty("pet_entity_type")]
		public int PetEntityType;

		[JsonProperty("playback_rate")]
		public float PlaybackRate;

		[JsonProperty("speed")]
		public float Speed;

		[JsonProperty("size")]
		public int Size;

		[JsonProperty("capacity")]
		public float Capacity;
	}

	public class Food
	{
		[JsonProperty("eat_motion")]
		public string EatMotion;

		[JsonProperty("effect_on")]
		public string EffectOn;

		[JsonProperty("modifier_effect_time")]
		public float EffectTime;
	}

	public class PetFood
	{
		[JsonProperty("vigor")]
		public string Vigor;

		[JsonProperty("inventory_plus_10")]
		public string InventoryPlus;

		[JsonProperty("accuracy_plus_10")]
		public float AccuracyPlus;
	}

	private static Performance _performances;

	private static Performance Performances
	{
		get
		{
			if (_performances == null)
			{
				_performances = Json.ReadFromFile<Performance>("offline/assets/performance");
			}
			return _performances;
		}
	}

	public static bool TryGetAddOnModelKey(string prototypeId, out string modelKey)
	{
		if (Performances.AddOnDict.TryGetValue(prototypeId, out var value))
		{
			using Dictionary<string, AddOn>.Enumerator enumerator = value.GetEnumerator();
			if (enumerator.MoveNext())
			{
				modelKey = enumerator.Current.Value.AddOnModelKey;
				return true;
			}
		}
		modelKey = null;
		return false;
	}

	public static Armor GetArmor(string prototypeId)
	{
		if (string.IsNullOrEmpty(prototypeId))
		{
			return null;
		}
		if (Performances.ArmorDict.TryGetValue(prototypeId, out var value))
		{
			using Dictionary<string, Armor>.Enumerator enumerator = value.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current.Value;
			}
		}
		return null;
	}

	public static Weapon GetWeapon(string prototypeId)
	{
		if (string.IsNullOrEmpty(prototypeId))
		{
			return null;
		}
		if (Performances.WeaponDict.TryGetValue(prototypeId, out var value))
		{
			using Dictionary<string, Weapon>.Enumerator enumerator = value.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current.Value;
			}
		}
		return null;
	}

	public static Instrument GetInstrument(string prototypeId)
	{
		if (string.IsNullOrEmpty(prototypeId))
		{
			return null;
		}
		if (Performances.InstrumentDict.TryGetValue(prototypeId, out var value))
		{
			using Dictionary<string, Instrument>.Enumerator enumerator = value.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current.Value;
			}
		}
		return null;
	}

	public static Rein GetRein(string prototypeId)
	{
		if (string.IsNullOrEmpty(prototypeId))
		{
			return null;
		}
		if (Performances.ReinsDict.TryGetValue(prototypeId, out var value))
		{
			using Dictionary<string, Rein>.Enumerator enumerator = value.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current.Value;
			}
		}
		return null;
	}

	public static Food GetFood(string prototypeId)
	{
		if (string.IsNullOrEmpty(prototypeId))
		{
			return null;
		}
		if (Performances.FoodDict.TryGetValue(prototypeId, out var value))
		{
			using Dictionary<string, Food>.Enumerator enumerator = value.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current.Value;
			}
		}
		return null;
	}

	public static PetFood GetPetFood(string prototypeId)
	{
		if (string.IsNullOrEmpty(prototypeId))
		{
			return null;
		}
		if (Performances.PetFoodDict.TryGetValue(prototypeId, out var value))
		{
			using Dictionary<string, PetFood>.Enumerator enumerator = value.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current.Value;
			}
		}
		return null;
	}
}
