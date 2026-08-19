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
		IncreaseType increaseType = IncreaseType;
		if (increaseType == IncreaseType.Ratio)
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
			return (!string.IsNullOrEmpty(replaceFormat)) ? string.Format(replaceFormat, value.ToString(valueFormat)) : value.ToString(valueFormat);
		}
		valueFormat = Mathf.Abs(value).ToString(valueFormat);
		if ((value > 0f && !Inverse) || (value < 0f && Inverse))
		{
			return (!string.IsNullOrEmpty(positiveFormat)) ? string.Format(positiveFormat, valueFormat) : valueFormat;
		}
		if ((value < 0f && !Inverse) || (value > 0f && Inverse))
		{
			return (!string.IsNullOrEmpty(negativeFormat)) ? string.Format(negativeFormat, valueFormat) : valueFormat;
		}
		return value.ToString(valueFormat);
	}
}
