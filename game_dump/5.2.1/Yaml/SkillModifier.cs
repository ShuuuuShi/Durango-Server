using Newtonsoft.Json;
using Shared.Ability;
using UnityEngine;

namespace Yaml;

public class SkillModifier
{
	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "reduce_type")]
	public string ReduceType;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "increase_type")]
	public IncreaseType IncreaseType;

	[JsonProperty(PropertyName = "apply_type")]
	public ApplyType ApplyType;

	[JsonProperty(PropertyName = "inverse")]
	public bool Inverse;

	public string GetValueFormat()
	{
		if (IncreaseType == IncreaseType.Ratio)
		{
			return "0.##%";
		}
		return "0.#";
	}

	public string GetValueString(float value, string replaceFormat = "={0}", string positiveFormat = "+{0}", string negativeFormat = "-{0}")
	{
		string valueFormat = GetValueFormat();
		if (ApplyType == ApplyType.Replace)
		{
			if (string.IsNullOrEmpty(replaceFormat))
			{
				return value.ToString(valueFormat);
			}
			return string.Format(replaceFormat, value.ToString(valueFormat));
		}
		valueFormat = Mathf.Abs(value).ToString(valueFormat);
		if ((value > 0f && !Inverse) || (value < 0f && Inverse))
		{
			if (string.IsNullOrEmpty(positiveFormat))
			{
				return valueFormat;
			}
			return string.Format(positiveFormat, valueFormat);
		}
		if ((value < 0f && !Inverse) || (value > 0f && Inverse))
		{
			if (string.IsNullOrEmpty(negativeFormat))
			{
				return valueFormat;
			}
			return string.Format(negativeFormat, valueFormat);
		}
		return value.ToString(valueFormat);
	}
}
