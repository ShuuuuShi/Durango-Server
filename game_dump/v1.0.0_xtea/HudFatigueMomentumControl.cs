using System;
using System.Collections.Generic;
using System.Text;
using FatigueData;
using Shared.Survival;
using StatusEffectData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class HudFatigueMomentumControl : MonoBehaviour
{
	[SerializeField]
	private HudFatigueMomentum _baseIcon;

	[SerializeField]
	private int _iconMargin;

	private List<HudFatigueMomentum> _icons = new List<HudFatigueMomentum>();

	private Stack<HudFatigueMomentum> _pool = new Stack<HudFatigueMomentum>();

	private void Awake()
	{
		((Component)_baseIcon).gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		GameSystem<FatigueSystem>.Instance().FatigueUpdated += OnUpdateFatigue;
		OnUpdateFatigue();
	}

	private void OnDisable()
	{
		GameSystem<FatigueSystem>.Instance().FatigueUpdated -= OnUpdateFatigue;
	}

	private void OnUpdateFatigue()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		List<FatigueVelocity> fatigueVelocities = GameSystem<FatigueSystem>.Instance().FatigueVelocities;
		int i = 0;
		for (int count = _icons.Count; i < count; i++)
		{
			_icons[i].Valid = false;
		}
		for (int j = 0; j < fatigueVelocities.Count; j++)
		{
			FatigueVelocity velocity = fatigueVelocities[j];
			HudFatigueMomentum hudFatigueMomentum = Get(velocity.Category, make: true);
			hudFatigueMomentum.Valid = true;
			hudFatigueMomentum.Set(velocity);
			hudFatigueMomentum.Index = j;
			if (!((Component)hudFatigueMomentum).gameObject.activeSelf)
			{
				hudFatigueMomentum.AnimWidget.SetPosition(GetPosition(j), useTween: false);
				hudFatigueMomentum.AnimWidget.SetAlpha(0f, useTween: false);
				((Component)hudFatigueMomentum).gameObject.SetActive(true);
				hudFatigueMomentum.AnimWidget.Alpha = 1f;
			}
		}
		for (int num = _icons.Count - 1; num >= 0; num--)
		{
			if (!_icons[num].Valid)
			{
				_icons[num].AnimWidget.Alpha = 0f;
				_icons.RemoveAt(num);
			}
		}
		UpdatePosition();
	}

	private HudFatigueMomentum Get(Shared.Survival.FatigueCategory key, bool make)
	{
		int i = 0;
		for (int count = _icons.Count; i < count; i++)
		{
			if (_icons[i].Key == key)
			{
				return _icons[i];
			}
		}
		HudFatigueMomentum hudFatigueMomentum = null;
		if (make)
		{
			if (_pool.Count > 0)
			{
				hudFatigueMomentum = _pool.Pop();
			}
			else
			{
				GameObject val = ((Component)((Component)_baseIcon).transform.parent).gameObject.AddChild(((Component)_baseIcon).gameObject);
				hudFatigueMomentum = val.GetComponent<HudFatigueMomentum>();
				hudFatigueMomentum.Disabled = OnIconDisable;
				UIEventListener.Get(val).onClick = OnClickMomentum;
			}
			_icons.Add(hudFatigueMomentum);
		}
		return hudFatigueMomentum;
	}

	private void OnIconDisable(HudFatigueMomentum icon)
	{
		_pool.Push(icon);
	}

	private void UpdatePosition()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int count = _icons.Count; i < count; i++)
		{
			_icons[i].AnimWidget.Position = GetPosition(i);
		}
	}

	private Vector3 GetPosition(int index)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)_baseIcon).transform.localPosition + Vector3.left * (float)index * (float)_iconMargin;
	}

	private void OnClickMomentum(GameObject obj)
	{
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		HudFatigueMomentum component = obj.GetComponent<HudFatigueMomentum>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		Yaml.FatigueCategory fatigueCategory = SingletonDict<Shared.Survival.FatigueCategory, Yaml.FatigueCategory>.Get(component.Key);
		if (fatigueCategory == null)
		{
			return;
		}
		IList<StatusEffect> statusEffects = GameSystem<PlayerStatusEffectSystem>.Instance().StatusEffects;
		string title = fatigueCategory.name;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(fatigueCategory.description).AppendLine();
		int i = 0;
		for (int count = statusEffects.Count; i < count; i++)
		{
			if (statusEffects[i].Effects.TryGetValue(component.Key.ToString().ToLower(), out var value))
			{
				value *= 60f;
				if (Math.Abs(value) > float.Epsilon)
				{
					stringBuilder.AppendFormat("[{0}:1.5] ", statusEffects[i].Template.icon).Append(statusEffects[i].Name).AppendFormat(" {2}{1}{0:0.#}[-]", Mathf.Abs(value), (!(value < 0f)) ? "+" : "-", (!(value < 0f)) ? UIManager.ColorBBCode(component.BadColor) : UIManager.ColorBBCode(component.GoodColor))
						.AppendLine();
				}
			}
		}
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Set(title, stringBuilder.ToString().Trim(), 400);
		widgetTooltipControl.Show(component.AnimWidget.Widget, Vector2.zero, 10f);
	}
}
