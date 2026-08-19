using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Durango.Logic;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class StatusEffectsControl : MonoBehaviour
{
	public enum Direction
	{
		Left,
		Right
	}

	private enum StatusType
	{
		Player,
		Target,
		EventBuff
	}

	[SerializeField]
	private StatusType _statusType;

	[CanBeNull]
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private StatusEffectIcon _statusEffectBase;

	[SerializeField]
	private int _countPerLine;

	[SerializeField]
	private float _alertRemainTime;

	[SerializeField]
	private bool _alwaysShowDescription;

	[SerializeField]
	private Direction _positionDirection = Direction.Right;

	[SerializeField]
	private float _zoomScale = 1.4f;

	[SerializeField]
	private bool _widgetResizing;

	[SerializeField]
	private TooltipBase.TriggerType _tooltipTriggerType;

	private readonly List<StatusEffectIcon> _statusEffectIcons = new List<StatusEffectIcon>();

	private readonly Queue<StatusEffectIcon> _statusEffectPool = new Queue<StatusEffectIcon>();

	private StatusEffectIcon _selectedIcon;

	private UIBase _parent;

	private string _target;

	protected List<StatusEffectIcon> StatusEffectIcons => _statusEffectIcons;

	public event Action ListUpdated;

	protected virtual void Awake()
	{
		_statusEffectBase.gameObject.SetActive(value: false);
		_parent = UIUtility.FindComponentInParent<UIBase>(base.gameObject);
		if (_scrollView != null)
		{
			_scrollView.Padding += (int)((float)_statusEffectBase.width * 0.5f);
		}
	}

	protected virtual void Start()
	{
		GameSystem<StatusEffectSystem>.Instance().StatusEffectsUpdated += RefreshStatusEffect;
		if (_statusType == StatusType.Target)
		{
			Observable<DamageableEntity> target = GameSystem<CombatSystem>.Instance().Target;
			target.Changed = (Action<DamageableEntity>)Delegate.Combine(target.Changed, new Action<DamageableEntity>(OnTargetChanged));
		}
		ClearEffectIcons();
		RefreshStatusEffect(instant: true);
	}

	protected virtual void Update()
	{
		UpdateStatusEffect();
	}

	private void ClearEffectIcons()
	{
		foreach (StatusEffectIcon statusEffectIcon in _statusEffectIcons)
		{
			StatusEffectIconPush(statusEffectIcon);
		}
		_statusEffectIcons.Clear();
	}

	protected virtual void RefreshStatusEffect(StatusEffects effects)
	{
		bool flag = false;
		switch (_statusType)
		{
		case StatusType.EventBuff:
			flag = effects.EntityId == GameManager.PlayerId;
			break;
		case StatusType.Target:
			flag = effects.EntityId == _target;
			break;
		case StatusType.Player:
			flag = effects.EntityId == GameManager.PlayerId || effects.EntityId == Singleton<PetManager>.Instance().GetPlayerPetId();
			break;
		}
		if (flag)
		{
			RefreshStatusEffect(instant: false);
		}
	}

	private void OnTargetChanged(DamageableEntity target)
	{
		string text = ((!(target == null)) ? target.GetEntityId() : null);
		if (!(_target == text))
		{
			ClearEffectIcons();
			_target = text;
			RefreshStatusEffect(instant: true);
		}
	}

	protected virtual void RefreshStatusEffect(bool instant)
	{
		if (PlayerBehavior.LocalPlayer == null)
		{
			return;
		}
		bool tween = !instant && _statusType != StatusType.Target;
		bool anim = !instant;
		if (UIManager.IsLoadingCurtain)
		{
			tween = false;
			anim = false;
		}
		BeginLoad();
		int index = 0;
		switch (_statusType)
		{
		case StatusType.EventBuff:
		{
			StatusEffects statusEffects4 = GameSystem<StatusEffectSystem>.Instance().GetStatusEffects();
			for (int num4 = statusEffects4.List.Count - 1; num4 >= 0; num4--)
			{
				StatusEffect statusEffect2 = statusEffects4.List[num4];
				if (statusEffect2.Template.Service)
				{
					SetStatusEffect(statusEffect2, ref index, tween);
				}
			}
			break;
		}
		case StatusType.Target:
		{
			StatusEffects statusEffects3 = GameSystem<StatusEffectSystem>.Instance().GetStatusEffects(_target);
			if (statusEffects3 != null)
			{
				for (int num3 = statusEffects3.List.Count - 1; num3 >= 0; num3--)
				{
					StatusEffect status2 = statusEffects3.List[num3];
					SetStatusEffect(status2, ref index, tween);
				}
			}
			break;
		}
		case StatusType.Player:
		{
			StatusEffects statusEffects = GameSystem<StatusEffectSystem>.Instance().GetStatusEffects();
			for (int num = statusEffects.List.Count - 1; num >= 0; num--)
			{
				StatusEffect statusEffect = statusEffects.List[num];
				if (!statusEffect.Template.Service)
				{
					SetStatusEffect(statusEffect, ref index, tween);
				}
			}
			StatusEffects statusEffects2 = GameSystem<StatusEffectSystem>.Instance().GetStatusEffects(Singleton<PetManager>.Instance().GetPlayerPetId());
			if (statusEffects2 != null)
			{
				for (int num2 = statusEffects2.List.Count - 1; num2 >= 0; num2--)
				{
					StatusEffect status = statusEffects2.List[num2];
					SetStatusEffect(status, PresetColor.UILightGreen, ref index, tween);
				}
			}
			break;
		}
		}
		EndLoad(index, anim);
		if (_widgetResizing)
		{
			RefreshWidgetSize();
		}
		if (_scrollView != null)
		{
			KWidgetScrollView scrollView = _scrollView;
			scrollView.Widgets.First().width = _statusEffectBase.width * GetIconColumnCount();
			scrollView.Reposition();
		}
		if (this.ListUpdated != null)
		{
			this.ListUpdated();
		}
	}

	private StatusEffectIcon StatusEffectIconPop()
	{
		StatusEffectIcon statusEffectIcon;
		if (_statusEffectPool.Count == 0)
		{
			statusEffectIcon = _statusEffectBase.transform.parent.gameObject.AddChild(_statusEffectBase.gameObject).GetComponent<StatusEffectIcon>();
			statusEffectIcon.FadeEffectFinished += StatusEffectIcon_FadeEffectFinished;
			UIEventListener uIEventListener = UIEventListener.Get(statusEffectIcon.gameObject);
			if (_tooltipTriggerType == TooltipBase.TriggerType.Click)
			{
				uIEventListener.onClick = StatusEffectIcon_OnClick;
			}
			else
			{
				uIEventListener.onHover = StatusEffectIcon_OnHover;
			}
		}
		else
		{
			statusEffectIcon = _statusEffectPool.Dequeue();
		}
		statusEffectIcon.Index = -1;
		statusEffectIcon.Groups.Clear();
		statusEffectIcon.gameObject.SetActive(value: true);
		statusEffectIcon.transform.localScale = Vector3.one;
		return statusEffectIcon;
	}

	private void StatusEffectIconPush(StatusEffectIcon icon)
	{
		_statusEffectPool.Enqueue(icon);
		icon.gameObject.SetActive(value: false);
	}

	private void StatusEffectIcon_FadeEffectFinished(StatusEffectIcon se)
	{
		if (se.Index < 0)
		{
			StatusEffectIconPush(se);
		}
		else if (_alwaysShowDescription && _selectedIcon == null)
		{
			ShowStatusEffectDescription(se, 6f);
		}
	}

	private void BeginLoad()
	{
		foreach (StatusEffectIcon statusEffectIcon in _statusEffectIcons)
		{
			statusEffectIcon.Groups.Clear();
		}
	}

	private void EndLoad(int count, bool anim)
	{
		for (int num = _statusEffectIcons.Count - 1; num >= 0; num--)
		{
			StatusEffectIcon statusEffectIcon = _statusEffectIcons[num];
			statusEffectIcon.SetStackCount(statusEffectIcon.Groups.Count);
			int index = statusEffectIcon.Index;
			if (index >= 0 && index < count)
			{
				Vector3 vector = CalcStatusEffectIconPosition(index);
				if (anim && statusEffectIcon.IsRepositionRequired && !statusEffectIcon.IsPlayingEffect)
				{
					TweenPosition.Begin(statusEffectIcon.gameObject, 0.3f, vector);
				}
				else
				{
					statusEffectIcon.Position = vector;
				}
			}
			else
			{
				if (anim)
				{
					statusEffectIcon.PlayFadeOut();
				}
				else
				{
					StatusEffectIconPush(statusEffectIcon);
				}
				_statusEffectIcons.Remove(statusEffectIcon);
			}
		}
	}

	private Vector3 CalcStatusEffectIconPosition(int index)
	{
		int width = _statusEffectBase.width;
		int height = _statusEffectBase.height;
		Vector3 vector = ((_positionDirection != Direction.Right) ? Vector3.left : Vector3.right);
		if (_countPerLine > 0)
		{
			return vector * (index % _countPerLine) * width + Vector3.down * (index / _countPerLine) * height;
		}
		return vector * index * width;
	}

	private StatusEffectIcon FindStatusEffectGroupIcon(string group)
	{
		foreach (StatusEffectIcon statusEffectIcon in _statusEffectIcons)
		{
			if (statusEffectIcon.Data != null && statusEffectIcon.Data.Template.UIGroup == group)
			{
				return statusEffectIcon;
			}
		}
		return null;
	}

	private StatusEffectIcon FindStatusEffectIcon(string id)
	{
		foreach (StatusEffectIcon statusEffectIcon in _statusEffectIcons)
		{
			if (statusEffectIcon.Data != null && statusEffectIcon.Data.Id == id)
			{
				return statusEffectIcon;
			}
		}
		return null;
	}

	private StatusEffectIcon FindStatusEffectIcon(int index)
	{
		foreach (StatusEffectIcon statusEffectIcon in _statusEffectIcons)
		{
			if (statusEffectIcon.Index == index)
			{
				return statusEffectIcon;
			}
		}
		return null;
	}

	private void SetStatusEffect(StatusEffect status, ref int index, bool tween)
	{
		SetStatusEffect(status, Color.white, ref index, tween);
	}

	private void SetStatusEffect(StatusEffect status, Color col, ref int index, bool tween)
	{
		StatusEffectIcon statusEffectIcon = FindStatusEffectIcon(index);
		if (statusEffectIcon != null)
		{
			statusEffectIcon.Index = -1;
		}
		bool num = !string.IsNullOrEmpty(status.Template.UIGroup);
		StatusEffectIcon statusEffectIcon2 = ((!num) ? FindStatusEffectIcon(status.Id) : FindStatusEffectGroupIcon(status.Template.UIGroup));
		if (statusEffectIcon2 == null)
		{
			statusEffectIcon2 = StatusEffectIconPop();
			_statusEffectIcons.Add(statusEffectIcon2);
			Vector3 vector = CalcStatusEffectIconPosition(index);
			if (tween)
			{
				statusEffectIcon2.PlayFadeIn(vector);
			}
			else
			{
				statusEffectIcon2.Position = vector;
			}
		}
		else
		{
			statusEffectIcon2.gameObject.SetActive(value: true);
		}
		if (num)
		{
			statusEffectIcon2.SetGroup(status, col);
			if (statusEffectIcon2.Index == -1 || statusEffectIcon2.Index >= index)
			{
				statusEffectIcon2.Index = index;
				index++;
			}
		}
		else
		{
			statusEffectIcon2.Set(status, col);
			statusEffectIcon2.Index = index;
			index++;
		}
	}

	private void UpdateStatusEffect()
	{
		foreach (StatusEffectIcon statusEffectIcon in _statusEffectIcons)
		{
			if (statusEffectIcon.isActiveAndEnabled)
			{
				statusEffectIcon.UpdateEffect(_alertRemainTime);
			}
		}
	}

	private void StatusEffectIcon_OnClick(GameObject go)
	{
		StatusEffectIcon component = go.GetComponent<StatusEffectIcon>();
		if (!(component == null))
		{
			if (component.Groups.Count == 0)
			{
				ShowStatusEffectDescription(component, 6f);
				return;
			}
			StatusEffectGroupPopup statusEffectGroupPopup = UIManager.Popup.Tooltip<StatusEffectGroupPopup>();
			statusEffectGroupPopup.Set(component.Groups);
			statusEffectGroupPopup.Show();
		}
	}

	private void StatusEffectIcon_OnHover(GameObject go, bool state)
	{
		StatusEffectIcon component = go.GetComponent<StatusEffectIcon>();
		if (!(component == null))
		{
			if (state)
			{
				ShowStatusEffectDescription(component, float.MaxValue);
			}
			else
			{
				UIManager.Popup.Tooltip<WidgetTooltipControl>().Hide();
			}
		}
	}

	private void ShowStatusEffectDescription(StatusEffectIcon status, float duration)
	{
		if (!(status == null) && status.Data != null && _parent.Visible && !(duration <= 0f))
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			StatusEffect se = status.Data;
			string text2 = ((GameSystem<StatusEffectSystem>.Instance().GetStatusEffects().GetStatusEffect(se.Id) == null) ? string.Format("[{1}]{0}[-]", se.Name, NGUIText.EncodeColor24(PresetColor.UIPet)) : $"<em>{se.Name}</em>");
			StringBuilder stringBuilder = new StringBuilder(se.Description);
			string value = ((se.EffectDetails != null) ? StatusEffect.EffectsText(se.EffectDetails.GetEnumerator()) : null);
			if (!string.IsNullOrEmpty(value))
			{
				stringBuilder.Append("\n\n<em>");
				stringBuilder.Append(value);
				stringBuilder.Append("</em>");
			}
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Sign = -1;
			SyncString subtitle = ((se.Until <= 0.0) ? ((SyncString)string.Empty) : new SyncString(delegate(out string text, out float period)
			{
				float remainTime = se.GetRemainTime();
				text = TimedeltaFormatter.Format(remainTime);
				period = remainTime % (float)TimedeltaFormatter.CurrentMinUnit();
			}));
			widgetTooltipControl.Set(text2, subtitle, stringBuilder.ToString().Trim(), 500);
			widgetTooltipControl.AutoPosition = false;
			widgetTooltipControl.Show((se.Until <= 0.0) ? duration : Mathf.Min(duration, status.Data.GetRemainTime()));
			Vector3[] localCorners = status.localCorners;
			Vector3 position = Vector3.Lerp(localCorners[0], localCorners[3], 0.5f);
			position = status.transform.TransformPoint(position);
			position = widgetTooltipControl.transform.parent.InverseTransformPoint(position);
			Vector3 pos = position + new Vector3(50f, -20f);
			pos.x = Mathf.Max((0f - (float)UIManager.SafeWidth) * 0.5f + 10f + (float)widgetTooltipControl.Widget.width, pos.x);
			widgetTooltipControl.Widget.SetPosition(pos, 1f, 1f);
			widgetTooltipControl.UpdateArrowPosition(position);
			widgetTooltipControl.AddOnFinished(StatusEffectDescriptionEnded);
			TweenScale.Begin(status.gameObject, 0.2f, Vector3.one * _zoomScale);
			StatusEffectDescriptionEnded();
			_selectedIcon = status;
		}
	}

	private void StatusEffectDescriptionEnded()
	{
		if (_selectedIcon != null)
		{
			TweenScale.Begin(_selectedIcon.gameObject, 0.2f, Vector3.one);
			_selectedIcon = null;
		}
	}

	private void RefreshWidgetSize()
	{
		UIWidget component = GetComponent<UIWidget>();
		if (!(component == null))
		{
			if (_statusEffectIcons.Count > 0)
			{
				component.SetDimensions(_statusEffectBase.width * GetIconColumnCount(), _statusEffectBase.height * GetIconRowCount());
			}
			else
			{
				component.SetDimensions(2, _statusEffectBase.height);
			}
		}
	}

	private int GetIconColumnCount()
	{
		if (_countPerLine > 0)
		{
			return Mathf.Min(_statusEffectIcons.Count, _countPerLine);
		}
		return _statusEffectIcons.Count;
	}

	private int GetIconRowCount()
	{
		if (_countPerLine > 0)
		{
			return Mathf.CeilToInt((float)_statusEffectIcons.Count / (float)_countPerLine);
		}
		return 1;
	}
}
