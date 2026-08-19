using System.Collections;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class InteractionMenuWidget_PC : InteractionMenuWidgetBase
{
	private const int MenuCount = 6;

	private const float LongPressDuration = 0.5f;

	[SerializeField]
	private InteractionMenuPresetActivator _presetActivator;

	[SerializeField]
	private InteractionMenuQueueWidget _queueLabel;

	[SerializeField]
	private ProgressGauge _longPressGauge;

	[SerializeField]
	private float _pressGaugeWaitTime;

	[SerializeField]
	private float _pressGaugeDuration;

	[SerializeField]
	private GameObject _bgNormal;

	[SerializeField]
	private GameObject _bgDisabled;

	[SerializeField]
	private UILabel _infoTooltipLabel;

	[EnumList(typeof(State), false, 0, -1)]
	[SerializeField]
	private Color[] _iconColors;

	[SerializeField]
	private float _emptyAlpha;

	private ICoroutineBinder _keyPressBinder;

	private bool _isWarning;

	protected override void OnDisable()
	{
		base.OnDisable();
		_queueLabel.SetCount(0, isCurrentlyGathering: false);
		this.StopCoroutine(_keyPressBinder);
	}

	protected override void OnInit()
	{
		base.OnInit();
		_infoTooltipLabel.text = T._("모두 채집하면 본체가 사라집니다");
	}

	public override void SetReservedQueueList(List<Pair<int, ItemIcon>> items)
	{
		int count = items?.Count ?? 0;
		bool flag = InteractionSystem.CurrentMenu.IsEqualKey(base.Data);
		if (flag && base.Data.Timer != null && base.Data.Timer.IsInterrupt)
		{
			flag = false;
		}
		_queueLabel.SetCount(count, flag);
		UpdateDescription();
	}

	protected override void OnSet()
	{
		if (base.Data.Timer == null)
		{
			if (InteractionSystem.CurrentMenu.IsEqualKey(base.Data))
			{
				_queueLabel.SetCount(0, isCurrentlyGathering: false);
			}
		}
		else if (_queueLabel.Count == 0)
		{
			_queueLabel.SetCount(0, isCurrentlyGathering: true);
		}
		UpdateDescription();
		Refresh();
	}

	protected override void RefreshIconTextureColor()
	{
		State state = ((!_isWarning) ? GetState() : State.Disabled);
		if (base.Data.Color != Color.white && (state == State.Normal || state == State.Hovered))
		{
			IconTexture.color = base.Data.Color;
		}
		else
		{
			IconTexture.color = _iconColors[(int)state];
		}
	}

	public override void UpdateUIPosition()
	{
		if (_presetActivator != null && base.Index % 6 >= 0)
		{
			_presetActivator.Activate(base.Index % 6);
		}
		NameLabel.pivot = ((GetSign() <= 0) ? UIWidget.Pivot.Right : UIWidget.Pivot.Left);
		NameLabel.transform.localPosition = Vector3.zero;
		UIUtility.UpdateAnchors(base.transform);
	}

	public void UpdateShortcut()
	{
		HoverShortcutViewer component = GetComponent<HoverShortcutViewer>();
		if (base.Index == -1)
		{
			component.Set(InputCommand.None);
		}
		else
		{
			component.Set((InputCommand)(49 + base.Index % 6));
		}
	}

	public void SetEmpty()
	{
		base.Disabled = true;
		base.Valid = false;
		base.Empty = true;
		base.Name = string.Empty;
		base.Description = string.Empty;
		SetDurationText(-1f);
		SetInfoText(string.Empty, emphasis: false);
		SetWaringText(string.Empty, emphasis: false);
		IconTexture.SetIcon(string.Empty);
		ProgressGauge.Timer = null;
		base.Alpha = _emptyAlpha;
		base.transform.localScale = Vector3.one * InteractionMenuListWidgetBase.MajorScale;
	}

	public void SetClick()
	{
		OnClick();
		StopPressGauge();
	}

	public void SetRightClick()
	{
		OnRightClick();
		StopPressGauge();
	}

	public void SetPress(bool isPress, bool isShortcut = false)
	{
		base.Pressed = isPress;
		if (base.Pressed && !string.IsNullOrEmpty(base.Description))
		{
			PlayPressGauge();
		}
		else
		{
			StopPressGauge();
		}
		if (isShortcut)
		{
			if (isPress)
			{
				this.StartCoroutine(ref _keyPressBinder, CoKeyPress());
			}
			else
			{
				this.StopCoroutine(_keyPressBinder);
			}
		}
	}

	public void SetLongPress()
	{
		OnLongPress();
		StopPressGauge();
	}

	public void SetHovered(bool isHover)
	{
		base.Hovered = isHover;
		if (OnHovered != null)
		{
			OnHovered(base.Hovered);
		}
	}

	public void PlayPressGauge()
	{
		_longPressGauge.Play(new Timer(Time.time + _pressGaugeWaitTime, Time.time + _pressGaugeWaitTime + _pressGaugeDuration));
	}

	public void StopPressGauge()
	{
		_longPressGauge.Timer = null;
	}

	public override bool IsWarning()
	{
		return _isWarning;
	}

	protected override void SetWaringText(string text, bool emphasis)
	{
		_isWarning = !string.IsNullOrEmpty(text);
		_bgDisabled.SetActive(_isWarning);
		_bgNormal.SetActive(!_isWarning);
	}

	private void UpdateDescription()
	{
		int num = base.Data.Count - _queueLabel.Count;
		base.Description = ((num > 0) ? num.ToString() : string.Empty);
	}

	private IEnumerator CoKeyPress()
	{
		yield return new WaitForSeconds(0.5f);
		SetLongPress();
	}
}
