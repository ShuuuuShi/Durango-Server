using System;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class BattleActionButton : MonoBehaviour
{
	public enum ButtonState
	{
		Activate,
		Cooldown,
		Locked,
		Reserved,
		Empty,
		Prohibited,
		Emphasis,
		Sealed,
		Unusable
	}

	[SerializeField]
	private UISprite _cooltime;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private TweenerPlayer _startTweener;

	[SerializeField]
	private TweenerPlayer _clickEffectTweener;

	[SerializeField]
	private GameObject _cooldownFinishTweener;

	[SerializeField]
	private GameObject _releaseTweener;

	[SerializeField]
	private UISound.ClickType _clickSound;

	[SerializeField]
	[WidgetStates.Type(typeof(ButtonState))]
	private WidgetStates _stateSet;

	private double _timerSince;

	private double _timerUntil;

	private bool _isPressed;

	public string Id { get; private set; }

	public float ShowDelay { get; set; }

	public ButtonState State { get; private set; }

	public bool IsClickable
	{
		get
		{
			ButtonState state = State;
			if (state == ButtonState.Activate || state == ButtonState.Emphasis)
			{
				return true;
			}
			return false;
		}
	}

	public event Action<BattleActionButton, bool> Pressed;

	private void OnEnable()
	{
		_startTweener.Play(ShowDelay);
	}

	private void OnDisable()
	{
		_clickEffectTweener.ResetToLast();
		_releaseTweener.SetActive(value: false);
		_cooldownFinishTweener.SetActive(value: false);
	}

	private void SetButtonState(ButtonState state)
	{
		if (State != state)
		{
			if (state == ButtonState.Activate && State == ButtonState.Cooldown)
			{
				_cooldownFinishTweener.SetActive(value: true);
			}
			_stateSet.Apply((int)state);
			State = state;
		}
	}

	public void SetEmpty()
	{
		Set(string.Empty, ButtonState.Empty);
	}

	public void Set(string id, ButtonState state)
	{
		Id = id;
		SetButtonState(state);
	}

	public void SetIcon(string icon)
	{
		SetIcon(icon, Color.white);
	}

	public void SetIcon(string icon, Color color)
	{
		if (string.IsNullOrEmpty(icon))
		{
			_icon.gameObject.SetActive(value: false);
			return;
		}
		_icon.gameObject.SetActive(value: true);
		_icon.spriteName = icon;
		_icon.color = color.WithA(_icon.alpha);
	}

	public void SetTimer(double since, double until)
	{
		double serverTime = GetServerTime();
		_timerSince = ((!(since > 0.0)) ? serverTime : since);
		_timerUntil = until;
	}

	private double GetServerTime()
	{
		return Connections.Frontend.GetPredictedServerTime();
	}

	private void Update()
	{
		double serverTime = GetServerTime();
		double timerSince = _timerSince;
		double timerUntil = _timerUntil;
		float fillAmount;
		if (timerUntil <= serverTime)
		{
			fillAmount = 1f;
		}
		else if (timerSince >= serverTime)
		{
			fillAmount = 0f;
		}
		else
		{
			float num = (float)(timerUntil - timerSince);
			fillAmount = ((!(num > 0f)) ? 0f : ((float)(serverTime - timerSince) / num));
		}
		_cooltime.fillAmount = fillAmount;
	}

	private void OnPress(bool isPress)
	{
		ShowPressEffect(isPress);
		if (this.Pressed != null)
		{
			this.Pressed(this, isPress);
		}
	}

	public void ShowClickEffect()
	{
		ShowPressEffect(isPress: true);
		ShowPressEffect(isPress: false);
	}

	public void ShowPressEffect(bool isPress)
	{
		if (_isPressed || IsClickable)
		{
			if (isPress)
			{
				UISound.PlayClick(_clickSound);
				_clickEffectTweener.ResetToFirst();
			}
			else
			{
				_clickEffectTweener.Play();
				_releaseTweener.SetActive(value: true);
			}
			_isPressed = isPress;
		}
	}
}
