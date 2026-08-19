using System;
using System.Collections.Generic;
using Durango.Utils;
using Messages;
using NCalc;
using Newtonsoft.Json;

namespace Yaml;

public class StatusEffectTemplate
{
	[JsonProperty(PropertyName = "min_level")]
	public int MinLevel;

	[JsonProperty(PropertyName = "max_level")]
	public int MaxLevel;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "floating_icon")]
	public string FloatingIcon;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "icon_color")]
	public string IconColor;

	[JsonProperty(PropertyName = "skin_effect")]
	public string SkinEffect;

	[JsonProperty(PropertyName = "expiration_extendable")]
	public bool ExpirationExtendable;

	[JsonProperty(PropertyName = "service")]
	public bool Service;

	[JsonProperty(PropertyName = "ui_group_icon")]
	public string UIGroup;

	[JsonProperty(PropertyName = "screen_effect")]
	public string ScreenEffectName;

	[JsonProperty(PropertyName = "effects")]
	public EffectDetail[] Effects;

	private Expression _expression;

	[JsonProperty(PropertyName = "duration")]
	public string Duration { private get; set; }

	public float GetDuration(int level)
	{
		if (_expression == null)
		{
			_expression = ExpressionParser.Parse(Duration);
		}
		_expression.Parameters["level"] = level;
		return Convert.ToSingle(_expression.Evaluate());
	}

	public IEnumerator<Messages.EffectDetail> GetEffects(int level)
	{
		int i = 0;
		for (int iMax = KUtility.GetSize(Effects); i < iMax; i++)
		{
			EffectDetail e = Effects[i];
			yield return new Messages.EffectDetail
			{
				Type = e.Type,
				Key = e.Key,
				Value = e.GetValue(level)
			};
		}
	}
}
