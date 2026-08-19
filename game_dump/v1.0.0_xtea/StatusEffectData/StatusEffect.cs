using System.Collections.Generic;
using Shared.StatusEffect;
using UnityEngine;
using Yaml;

namespace StatusEffectData;

public class StatusEffect
{
	public string Id;

	public int Level;

	public StatusEffectTemplate Template;

	public string Name;

	public string Description;

	public int Stack;

	public int ReceiveIndex;

	public double Since;

	public double Until;

	public Dictionary<string, float> Effects;

	public bool IsValid;

	public StatusEffect(string id, int level, StatusEffectTemplate template)
	{
		Id = id;
		Level = level;
		Template = template;
		IsValid = true;
	}

	public StatusEffect(string id, string name, string desc, string icon)
	{
		Id = id;
		Level = 0;
		Template = new StatusEffectTemplate();
		Template.name = name;
		Template.description = desc;
		Template.icon = icon;
		Template.type = EffectType.None;
		IsValid = true;
	}

	public void RefreshText()
	{
		if (Stack <= 1)
		{
			Name = Template.name;
		}
		else
		{
			Name = $"{Template.name} ({Stack})";
		}
		Description = Template.description;
	}

	public float GetRemainTime()
	{
		if (Until <= 0.0)
		{
			return float.MaxValue;
		}
		return Mathf.Max(0f, (float)(Until - Connections.Frontend.GetPredictedServerTime()));
	}
}
