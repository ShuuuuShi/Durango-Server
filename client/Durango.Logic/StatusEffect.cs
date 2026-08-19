using System.Collections.Generic;
using System.Text;
using Durango.Network;
using Durango.Utils;
using L10N;
using Messages;
using Shared.StatusEffect;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Logic;

public class StatusEffect
{
	public bool IsValid;

	private readonly StatusEffectTemplate _template;

	public string Id { get; private set; }

	public int Level { get; private set; }

	public int Stack { get; private set; }

	public double Since { get; private set; }

	public double Until { get; private set; }

	public string Name { get; private set; }

	public IList<Messages.EffectDetail> EffectDetails { get; private set; }

	public string FloatingIcon => _template.FloatingIcon;

	public string Icon => _template.Icon;

	public string IconColor => _template.IconColor;

	public string Description => _template.Description;

	public StatusEffectTemplate Template => _template;

	public DailyContents? DailyContents { get; private set; }

	public StatusEffect(Messages.StatusEffect msg, StatusEffectTemplate template)
	{
		_template = template;
		Set(msg);
	}

	public StatusEffect(string id, string titleName, string desc, string icon)
	{
		_template = new StatusEffectTemplate();
		_template.Name = titleName;
		_template.Description = desc;
		_template.Icon = icon;
		Set(new Messages.StatusEffect
		{
			EffectId = id
		});
	}

	public void Set(Messages.StatusEffect msg)
	{
		Id = msg.EffectId;
		Level = msg.Level;
		Stack = msg.Stacked;
		Since = msg.Since;
		if (_template.ExpirationExtendable)
		{
			Until = 0.0;
		}
		else
		{
			Until = msg.Until;
		}
		EffectDetails = msg.Effects;
		Name = ((!string.IsNullOrEmpty(msg.NameGettext)) ? msg.NameGettext : _template.Name.ToString());
		if (Stack > 1)
		{
			Name = $"{Name} ({Stack})";
		}
		DailyContents = msg.DailyContents;
		IsValid = true;
	}

	public float GetRemainTime()
	{
		return Mathf.Max(0f, (float)(Until - Connections.Frontend.GetPredictedServerTime()));
	}

	public float GetDetail(EffectType type, string key)
	{
		int i = 0;
		for (int size = KUtility.GetSize(EffectDetails); i < size; i++)
		{
			Messages.EffectDetail effectDetail = EffectDetails[i];
			if (effectDetail.Type == type && effectDetail.Key == key)
			{
				return effectDetail.Value;
			}
		}
		return 0f;
	}

	public static string EffectsText(IEnumerator<Messages.EffectDetail> details)
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		EffectsText(reusable.Value, details);
		return reusable.Value.ToString();
	}

	public static void EffectsText(StringBuilder str, IEnumerator<Messages.EffectDetail> details)
	{
		if (details == null)
		{
			return;
		}
		int num = 0;
		EffectType effectType = EffectType.Invalid;
		while (details.MoveNext())
		{
			Messages.EffectDetail current = details.Current;
			string value = null;
			switch (current.Type)
			{
			case EffectType.Survival:
				value = SurvivalText(current.Key, current.Value);
				break;
			case EffectType.Modifier:
			{
				SkillModifier skillModifier = SingletonDict<string, SkillModifier>.Get(current.Key);
				if (skillModifier != null)
				{
					value = string.Format("{0} {1}", skillModifier.Name, skillModifier.GetValueString(current.Value, null, T._("{0} 증가"), T._("{0} 감소")));
				}
				break;
			}
			}
			if (string.IsNullOrEmpty(value))
			{
				continue;
			}
			if (num > 0)
			{
				str.AppendLine();
				if (effectType != current.Type)
				{
					str.AppendLine();
				}
			}
			str.Append(value);
			num++;
			effectType = current.Type;
		}
	}

	private static string SurvivalText(string key, float value)
	{
		string text = LocalizeSystem.Get("#survival_" + key);
		string result = null;
		if (key != "fatigue")
		{
			if (value > 0f)
			{
				result = T._("{0} 초당 {1:N2} 증가", text, Mathf.Abs(value));
			}
			else if (value < 0f)
			{
				result = T._("{0} 초당 {1:N2} 감소", text, Mathf.Abs(value));
			}
		}
		else if (value > 0f)
		{
			result = T._("{0} 분당 {1:N2} 증가", text, Mathf.Abs(value * 60f));
		}
		else if (value < 0f)
		{
			result = T._("{0} 분당 {1:N2} 감소", text, Mathf.Abs(value * 60f));
		}
		return result;
	}
}
