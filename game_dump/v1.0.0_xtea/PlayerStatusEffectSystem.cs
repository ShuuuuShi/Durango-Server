using System;
using System.Collections.Generic;
using Messages;
using StatusEffectData;
using Yaml;

public class PlayerStatusEffectSystem : GameSystem<PlayerStatusEffectSystem>, IStatusEffectSystem
{
	private readonly List<StatusEffectData.StatusEffect> _statusEffects = new List<StatusEffectData.StatusEffect>();

	private HashSet<string> _visualEffectSet = new HashSet<string>();

	public HashSet<string> VisualEffects => _visualEffectSet;

	public IList<StatusEffectData.StatusEffect> StatusEffects => _statusEffects;

	public event Action<string> OnAddStatusEffect;

	public event Action StatusEffectsUpdated;

	private void Awake()
	{
		KSingleton<GameManager>.Instance().Ready += delegate
		{
			Connections.Frontend.Send(default(GetStatusEffects));
		};
	}

	public StatusEffectData.StatusEffect GetStatusEffect(string id, int level = -1)
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

	public void AddStatusEffectPrologue(StatusEffectData.StatusEffect effect)
	{
		_statusEffects.Add(effect);
		StatusEffect_OnAdd(effect);
		RefreshStatusEffectList();
	}

	public void ClearStatusEffectPrologue()
	{
		_statusEffects.Clear();
		RefreshStatusEffectList();
	}

	public void RemoveStatusEffectPrologue(string id)
	{
		StatusEffectData.StatusEffect statusEffect = GetStatusEffect(id);
		if (statusEffect != null)
		{
			statusEffect.IsValid = false;
		}
		RefreshStatusEffectList();
	}

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
			int prevStack = 0;
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
				prevStack = statusEffect2.Stack;
				statusEffect2.Stack = stacked;
				flag2 = true;
			}
			statusEffect2.Since = statusEffect.Since;
			statusEffect2.Until = statusEffect.Until;
			statusEffect2.Effects = new Dictionary<string, float>(statusEffect.Effects);
			statusEffect2.IsValid = true;
			statusEffect2.RefreshText();
			if (flag)
			{
				StatusEffect_OnAdd(statusEffect2);
			}
			if (flag2)
			{
				StatusEffect_OnStackChange(statusEffect2, prevStack);
			}
		}
		CheckRestEffect();
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
				StatusEffect_OnFinished(statusEffect);
			}
		}
		_visualEffectSet.Clear();
		int i = 0;
		for (int count = _statusEffects.Count; i < count; i++)
		{
			StatusEffectData.StatusEffect statusEffect2 = _statusEffects[i];
			if (!string.IsNullOrEmpty(statusEffect2.Template.visual_effect))
			{
				_visualEffectSet.Add(statusEffect2.Template.visual_effect);
			}
		}
		if (this.StatusEffectsUpdated != null)
		{
			this.StatusEffectsUpdated();
		}
	}

	private void CheckRestEffect()
	{
		if (!KSingleton<PlayerController>.HasInstance())
		{
			return;
		}
		bool flag = false;
		int i = 0;
		for (int count = _statusEffects.Count; i < count; i++)
		{
			StatusEffectData.StatusEffect statusEffect = _statusEffects[i];
			if (statusEffect.IsValid)
			{
				flag |= statusEffect.Template.motion == "rest";
			}
		}
		if (PlayerBehavior.LocalPlayer.IsRest != flag)
		{
			PlayerController playerController = KSingleton<PlayerController>.Instance();
			playerController.MotionParam("IsRest", flag ? 1 : 0);
			playerController.Motion("Stand");
		}
	}

	private void StatusEffect_OnAdd(StatusEffectData.StatusEffect effect)
	{
		if (this.OnAddStatusEffect != null)
		{
			this.OnAddStatusEffect(effect.Id);
		}
	}

	private void StatusEffect_OnStackChange(StatusEffectData.StatusEffect status, int prevStack)
	{
	}

	private void StatusEffect_OnFinished(StatusEffectData.StatusEffect se)
	{
	}

	public bool IsActivated(string id)
	{
		return GetStatusEffect(id) != null;
	}

	public string[] GetValidStatusStringArray()
	{
		string[] array = new string[_statusEffects.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _statusEffects[i].Id;
		}
		return array;
	}
}
