using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Messages;
using Yaml;

namespace Durango.Logic;

public class StatusEffects
{
	private readonly List<StatusEffect> _list = new List<StatusEffect>();

	private readonly List<KeyValuePair<string, bool>> _changedList = new List<KeyValuePair<string, bool>>();

	public string EntityId { get; private set; }

	[NotNull]
	public List<StatusEffect> List => _list;

	public event Action<StatusEffects> StatusEffectsUpdated;

	public event Action<string, string> StatusEffectAdded;

	public event Action<string, string> StatusEffectRemoved;

	public StatusEffects(string id)
	{
		EntityId = id;
	}

	public void SetStatusEffects(Messages.StatusEffects effects)
	{
		if (effects.EntityId != EntityId)
		{
			return;
		}
		Messages.StatusEffect[] statusEffects = effects._StatusEffects;
		_changedList.Clear();
		int i = 0;
		for (int count = _list.Count; i < count; i++)
		{
			_list[i].IsValid = false;
		}
		int j = 0;
		for (int size = KUtility.GetSize(statusEffects); j < size; j++)
		{
			Messages.StatusEffect msg = statusEffects[j];
			string effectId = msg.EffectId;
			int level = msg.Level;
			StatusEffect statusEffect = GetStatusEffect(effectId, level);
			if (statusEffect == null)
			{
				StatusEffectTemplate statusEffectTemplate = StatusEffectTemplateYaml.GetStatusEffectTemplate(effectId, level);
				// Some asset sets do not contain a template for the server-owned rest effect.
				// Do not drop the packet: the player must still see the rest buff and icon.
				if (statusEffectTemplate == null && string.Equals(effectId, "rest", StringComparison.OrdinalIgnoreCase))
				{
					statusEffectTemplate = new StatusEffectTemplate
					{
						MinLevel = 1,
						MaxLevel = int.MaxValue,
						Name = "Resting",
						Description = "Resting at a shelter",
						Icon = "icon_se_rest",
						Effects = new Yaml.EffectDetail[0]
					};
				}
				if (statusEffectTemplate != null)
				{
					statusEffect = new StatusEffect(msg, statusEffectTemplate);
					_list.Add(statusEffect);
					_changedList.Add(new KeyValuePair<string, bool>(msg.EffectId, value: true));
				}
			}
			else
			{
				statusEffect.Set(msg);
			}
		}
		for (int num = _list.Count - 1; num >= 0; num--)
		{
			StatusEffect statusEffect2 = _list[num];
			if (!statusEffect2.IsValid)
			{
				_changedList.Add(new KeyValuePair<string, bool>(statusEffect2.Id, value: false));
				_list.RemoveAt(num);
			}
		}
		OnChanged();
	}

	public void SetStatusEffects(List<StatusEffect> effects)
	{
		_changedList.Clear();
		for (int i = 0; i < _list.Count; i++)
		{
			_list[i].IsValid = false;
		}
		int j = 0;
		for (int size = KUtility.GetSize(effects); j < size; j++)
		{
			StatusEffect statusEffect = effects[j];
			statusEffect.IsValid = true;
			string id = statusEffect.Id;
			int level = statusEffect.Level;
			int num = IndexOf(id, level);
			if (num == -1)
			{
				_list.Add(statusEffect);
				_changedList.Add(new KeyValuePair<string, bool>(statusEffect.Id, value: true));
			}
			else
			{
				_list[num] = statusEffect;
			}
		}
		for (int num2 = _list.Count - 1; num2 >= 0; num2--)
		{
			StatusEffect statusEffect2 = _list[num2];
			if (!statusEffect2.IsValid)
			{
				_changedList.Add(new KeyValuePair<string, bool>(statusEffect2.Id, value: false));
				_list.RemoveAt(num2);
			}
		}
		OnChanged();
	}

	private void OnChanged()
	{
		if (this.StatusEffectAdded != null)
		{
			for (int i = 0; i < _changedList.Count; i++)
			{
				if (_changedList[i].Value)
				{
					this.StatusEffectAdded(EntityId, _changedList[i].Key);
				}
			}
		}
		if (this.StatusEffectRemoved != null)
		{
			for (int j = 0; j < _changedList.Count; j++)
			{
				if (!_changedList[j].Value)
				{
					this.StatusEffectRemoved(EntityId, _changedList[j].Key);
				}
			}
		}
		_changedList.Clear();
		if (this.StatusEffectsUpdated != null)
		{
			this.StatusEffectsUpdated(this);
		}
	}

	public StatusEffect GetStatusEffect(string id, int? level = null)
	{
		int num = IndexOf(id, level);
		if (num == -1)
		{
			return null;
		}
		return _list[num];
	}

	private int IndexOf(string id, int? level)
	{
		int i = 0;
		for (int count = _list.Count; i < count; i++)
		{
			if (_list[i].Id == id && (!level.HasValue || _list[i].Level == level.Value))
			{
				return i;
			}
		}
		return -1;
	}
}
