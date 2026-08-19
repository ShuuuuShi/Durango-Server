using System;
using System.Collections.Generic;
using System.Text;
using L10N;
using Shared.StatusEffect;
using StatusEffectData;
using TimerData;
using UnityEngine;

public class StatusEffectsControl : MonoBehaviour
{
	private enum StatusType
	{
		Player,
		Target
	}

	[SerializeField]
	private StatusType _statusType;

	[SerializeField]
	private StatusEffectIcon _statusEffectBase;

	[SerializeField]
	private int _countPerLine;

	[SerializeField]
	private float _descriptionVisibleTime;

	[SerializeField]
	private float _alertRemainTime;

	[SerializeField]
	private bool _alwaysShowDescription;

	private List<StatusEffectIcon> _statusEffectIcons = new List<StatusEffectIcon>();

	private Queue<StatusEffectIcon> _statusEffectPool = new Queue<StatusEffectIcon>();

	private StatusEffectIcon _selectedIcon;

	private UIBase _parent;

	private IStatusEffectSystem _system;

	private IStatusEffectSystem StatusEffectSystem
	{
		get
		{
			if (_system == null)
			{
				object system;
				if (_statusType == StatusType.Player)
				{
					IStatusEffectSystem statusEffectSystem = GameSystem<PlayerStatusEffectSystem>.Instance();
					system = statusEffectSystem;
				}
				else
				{
					system = GameSystem<TargetStatusEffectSystem>.Instance();
				}
				_system = (IStatusEffectSystem)system;
			}
			return _system;
		}
	}

	private void Awake()
	{
		((Component)_statusEffectBase).gameObject.SetActive(false);
		_parent = ((Component)this).GetComponentInParent<UIBase>();
	}

	private void OnEnable()
	{
		StatusEffectSystem.StatusEffectsUpdated += OnUpdateStatusEffect;
		RefreshStatusEffect(anim: false);
	}

	private void OnDisable()
	{
		StatusEffectSystem.StatusEffectsUpdated -= OnUpdateStatusEffect;
	}

	private void Update()
	{
		UpdateStatusEffect();
	}

	private void OnUpdateStatusEffect()
	{
		RefreshStatusEffect(!UIManager.IsLoadingCurtain);
	}

	private void RefreshStatusEffect(bool anim)
	{
		int num = 0;
		IList<StatusEffect> statusEffects = StatusEffectSystem.StatusEffects;
		int i = 0;
		for (int num2 = statusEffects?.Count ?? 0; i < num2; i++)
		{
			StatusEffect status = statusEffects[i];
			SetStatusEffect(status, num, anim);
			num++;
		}
		StatusEffectReposition(num, anim);
	}

	private StatusEffectIcon StatusEffectIconPop()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		StatusEffectIcon statusEffectIcon = null;
		if (_statusEffectPool.Count == 0)
		{
			GameObject val = ((Component)((Component)_statusEffectBase).transform.parent).gameObject.AddChild(((Component)_statusEffectBase).gameObject);
			statusEffectIcon = val.GetComponent<StatusEffectIcon>();
			statusEffectIcon.OnFinishedFadeEffect += StatusEffectIcon_OnFinishedFadeEffect;
			UIEventListener uIEventListener = UIEventListener.Get(((Component)statusEffectIcon).gameObject);
			uIEventListener.onClick = StatusEffectIcon_OnClick;
		}
		else
		{
			statusEffectIcon = _statusEffectPool.Dequeue();
		}
		((Component)statusEffectIcon).gameObject.SetActive(true);
		((Component)statusEffectIcon).transform.localScale = Vector3.one;
		return statusEffectIcon;
	}

	private void StatusEffectIconPush(StatusEffectIcon icon)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_statusEffectPool.Enqueue(icon);
		icon.Index = -1;
		icon.Position = Vector3.zero;
		((Component)icon).gameObject.SetActive(false);
	}

	private void StatusEffectIcon_OnFinishedFadeEffect(StatusEffectIcon se)
	{
		if (se.Index < 0)
		{
			StatusEffectIconPush(se);
		}
		else if (_alwaysShowDescription && (Object)(object)_selectedIcon == (Object)null)
		{
			ShowStatusEffectDescription(se);
		}
	}

	private void StatusEffectReposition(int count, bool anim)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		for (int num = _statusEffectIcons.Count - 1; num >= 0; num--)
		{
			int index = _statusEffectIcons[num].Index;
			if (index >= 0 && index < count)
			{
				Vector3 val = CalcStatusEffectIconPosition(index);
				if (anim && _statusEffectIcons[num].IsRequireReposition && !_statusEffectIcons[num].IsPlayingEffect)
				{
					_statusEffectIcons[num].Tweener.from = _statusEffectIcons[num].Position;
					_statusEffectIcons[num].Tweener.to = val;
					_statusEffectIcons[num].Tweener.tweenFactor = 0f;
					_statusEffectIcons[num].Tweener.PlayForward();
				}
				else
				{
					_statusEffectIcons[num].Position = val;
				}
			}
			else
			{
				if (anim)
				{
					_statusEffectIcons[num].PlayFadeOut();
				}
				else
				{
					StatusEffectIconPush(_statusEffectIcons[num]);
				}
				_statusEffectIcons.Remove(_statusEffectIcons[num]);
			}
		}
	}

	private Vector3 CalcStatusEffectIconPosition(int index)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		int width = _statusEffectBase.Width;
		int height = _statusEffectBase.Height;
		return Vector3.right * (float)(index % _countPerLine) * (float)width + Vector3.down * (float)(index / _countPerLine) * (float)height;
	}

	private StatusEffectIcon FindStatusEffectIcon(string id)
	{
		int count = _statusEffectIcons.Count;
		for (int i = 0; i < count; i++)
		{
			if (_statusEffectIcons[i].Data.Id == id)
			{
				return _statusEffectIcons[i];
			}
		}
		return null;
	}

	private StatusEffectIcon FindStatusEffectIcon(int index)
	{
		int count = _statusEffectIcons.Count;
		for (int i = 0; i < count; i++)
		{
			if (_statusEffectIcons[i].Index == index)
			{
				return _statusEffectIcons[i];
			}
		}
		return null;
	}

	private void SetStatusEffect(StatusEffect status, int index, bool tween)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		StatusEffectIcon statusEffectIcon = FindStatusEffectIcon(index);
		if ((Object)(object)statusEffectIcon != (Object)null)
		{
			statusEffectIcon.Index = -1;
		}
		StatusEffectIcon statusEffectIcon2 = FindStatusEffectIcon(status.Id);
		if ((Object)(object)statusEffectIcon2 == (Object)null)
		{
			statusEffectIcon2 = StatusEffectIconPop();
			_statusEffectIcons.Add(statusEffectIcon2);
			Vector3 val = CalcStatusEffectIconPosition(index);
			if (tween)
			{
				statusEffectIcon2.PlayFadeIn(val);
			}
			else
			{
				statusEffectIcon2.Position = val;
			}
		}
		statusEffectIcon2.Set(status);
		statusEffectIcon2.Index = index;
	}

	private void UpdateStatusEffect()
	{
		int i = 0;
		for (int count = _statusEffectIcons.Count; i < count; i++)
		{
			StatusEffectIcon statusEffectIcon = _statusEffectIcons[i];
			float remainTime = statusEffectIcon.Data.GetRemainTime();
			if (!statusEffectIcon.IsPlayingEffect)
			{
				if (remainTime > _alertRemainTime)
				{
					statusEffectIcon.Widget.alpha = 1f;
					continue;
				}
				float num = remainTime / _alertRemainTime;
				float num2 = Mathf.Cos(num * 12f * (float)Math.PI);
				statusEffectIcon.Widget.alpha = num2 * 0.25f + 0.5f;
			}
		}
	}

	private void StatusEffectIcon_OnClick(GameObject go)
	{
		StatusEffectIcon component = go.GetComponent<StatusEffectIcon>();
		if (!((Object)(object)component == (Object)null))
		{
			ShowStatusEffectDescription(component);
		}
	}

	private void ShowStatusEffectDescription(StatusEffectIcon status)
	{
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)status != (Object)null && _parent.Visible)
		{
			StatusEffect se = status.Data;
			string text2 = $"<em>{se.Name}</em>";
			string text3 = se.Description;
			if (se.Template.type == EffectType.Modifier && se.Effects.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder(text3);
				stringBuilder.Append("\n\n<em>");
				stringBuilder.Append(ModifiersText(se.Effects));
				stringBuilder.Append("</em>");
				text3 = stringBuilder.ToString();
			}
			else if (se.Template.type == EffectType.Survival && se.Effects.Count > 0)
			{
				StringBuilder stringBuilder2 = new StringBuilder(text3);
				stringBuilder2.Append("\n\n<em>");
				stringBuilder2.Append(SurvivalEffectText(se.Effects));
				stringBuilder2.Append("</em>");
				text3 = stringBuilder2.ToString();
			}
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Sign = -1;
			SyncString subtitle = ((!(se.Until > 0.0)) ? ((SyncString)string.Empty) : new SyncString(delegate(out string text, out float period)
			{
				text = TimerSystem.TimeToString(se.GetRemainTime(), TimePeriod.Sec, 2);
				period = 1f;
			}));
			widgetTooltipControl.Set(text2, subtitle, text3, 500);
			widgetTooltipControl.Show(status.Widget, Vector2.zero, Mathf.Min(6f, status.Data.GetRemainTime()));
			widgetTooltipControl.AddOnFinished(StatusEffectDescriptionEnded);
			TweenScale.Begin(((Component)status).gameObject, 0.2f, Vector3.one * 1.4f);
			StatusEffectDescriptionEnded();
			_selectedIcon = status;
		}
	}

	public static string ModifiersText(Dictionary<string, float> Modifiers)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (KeyValuePair<string, float> Modifier in Modifiers)
		{
			string text = LocalizeUtil.ModifierIncreaseText(Modifier.Key, Modifier.Value);
			if (text != null)
			{
				if (num > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(text);
				num++;
			}
		}
		return stringBuilder.ToString();
	}

	public static string SurvivalEffectText(Dictionary<string, float> effects)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (KeyValuePair<string, float> effect in effects)
		{
			string text = LocalizeSystem.Get("#survival_" + effect.Key);
			string text2 = null;
			if (effect.Key != "fatigue")
			{
				if (effect.Value > 0f)
				{
					text2 = T._("{0} 초당 {1} 증가", text, Mathf.Abs(effect.Value));
				}
				else if (effect.Value < 0f)
				{
					text2 = T._("{0} 초당 {1} 감소", text, Mathf.Abs(effect.Value));
				}
			}
			else
			{
				float num2 = effect.Value * 60f;
				if (num2 > 0f)
				{
					text2 = T._("{0} 분당 {1} 증가", text, Mathf.Abs(num2));
				}
				else if (num2 < 0f)
				{
					text2 = T._("{0} 분당 {1} 감소", text, Mathf.Abs(num2));
				}
			}
			if (text2 != null)
			{
				if (num > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(text2);
				num++;
			}
		}
		return stringBuilder.ToString();
	}

	private void StatusEffectDescriptionEnded()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_selectedIcon != (Object)null)
		{
			TweenScale.Begin(((Component)_selectedIcon).gameObject, 0.2f, Vector3.one);
			_selectedIcon = null;
		}
	}
}
