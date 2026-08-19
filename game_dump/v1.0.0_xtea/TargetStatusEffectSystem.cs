using System;
using System.Collections.Generic;
using Messages;
using StatusEffectData;
using Yaml;

public class TargetStatusEffectSystem : GameSystem<TargetStatusEffectSystem>, IStatusEffectSystem
{
	private List<StatusEffectData.StatusEffect> _statusEffects = new List<StatusEffectData.StatusEffect>();

	public IList<StatusEffectData.StatusEffect> StatusEffects => _statusEffects;

	public event Action StatusEffectsUpdated;

	public void SetStatusEffects(Messages.StatusEffect[] statusEffectMsgs)
	{
		int i = 0;
		for (int count = _statusEffects.Count; i < count; i++)
		{
			_statusEffects[i].IsValid = false;
		}
		int j = 0;
		for (int num = statusEffectMsgs.Length; j < num; j++)
		{
			Messages.StatusEffect statusEffect = statusEffectMsgs[j];
			string effectId = statusEffect.EffectId;
			int level = statusEffect.Level;
			bool flag = false;
			bool flag2 = false;
			int num2 = 0;
			StatusEffectData.StatusEffect statusEffect2 = GetStatusEffect(effectId, level);
			if (statusEffect2 == null)
			{
				StatusEffectTemplate statusEffectTemplate = StatusEffectTemplateYaml.GetStatusEffectTemplate(effectId, level);
				if (statusEffectTemplate == null)
				{
					continue;
				}
				statusEffect2 = new StatusEffectData.StatusEffect(effectId, level, statusEffectTemplate);
				_statusEffects.Add(statusEffect2);
				flag = true;
			}
			statusEffect2.ReceiveIndex = j;
			int stacked = statusEffect.Stacked;
			if (statusEffect2.Stack != stacked)
			{
				num2 = statusEffect2.Stack;
				statusEffect2.Stack = stacked;
				flag2 = true;
			}
			statusEffect2.Since = statusEffect.Since;
			statusEffect2.Until = statusEffect.Until;
			statusEffect2.Effects = new Dictionary<string, float>(statusEffect.Effects);
			statusEffect2.IsValid = true;
			statusEffect2.RefreshText();
		}
		RefreshStatusEffectList();
	}

	private void RefreshStatusEffectList()
	{
		for (int num = _statusEffects.Count - 1; num >= 0; num--)
		{
			StatusEffectData.StatusEffect statusEffect = _statusEffects[num];
			if (!statusEffect.IsValid)
			{
				_statusEffects.RemoveAt(num);
			}
		}
		if (this.StatusEffectsUpdated != null)
		{
			this.StatusEffectsUpdated();
		}
	}

	private StatusEffectData.StatusEffect GetStatusEffect(string id, int level = -1)
	{
		int i = 0;
		for (int count = _statusEffects.Count; i < count; i++)
		{
			if (_statusEffects[i].Id == id && (level == -1 || _statusEffects[i].Level == level))
			{
				return _statusEffects[i];
			}
		}
		return null;
	}
}
