using System;
using System.Linq;
using Durango.Logic.Item;
using Durango.Network;
using InteractionData;
using UnityEngine;

namespace Durango.UI;

public class ContextActionButtonBase : MonoBehaviour
{
	public enum State
	{
		None,
		Normal,
		Hovered,
		Pressed,
		Cooltime
	}

	protected State _state;

	[SerializeField]
	protected UILabel _text;

	[SerializeField]
	protected UISprite _icon;

	[SerializeField]
	protected UISprite _cooltime;

	private UITweener[] _effectTweeners;

	private TweenAlpha _hideTweener;

	private double _since;

	private double _until;

	private bool _isShow;

	private bool _isInit;

	public InteractionMenuData Menu { get; private set; }

	public string Description { get; private set; }

	public event Action<ContextActionButtonBase> Clicked;

	public event Action<ContextActionButtonBase, bool> Pressed;

	public event Action<ContextActionButtonBase, bool> Hovered;

	protected event Action CooltimeEnded;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_effectTweeners = (from t in GetComponents<UITweener>()
			where t.tweenGroup == 0
			select t).ToArray();
		for (int i = 0; i < _effectTweeners.Length; i++)
		{
			_hideTweener = _effectTweeners[i] as TweenAlpha;
			if (_hideTweener != null)
			{
				break;
			}
		}
		SetState(State.Normal);
		base.gameObject.SetActive(value: false);
	}

	protected virtual void Start()
	{
		Init();
	}

	public void UpdateRoutine()
	{
		if (!(_until > 0.0))
		{
			return;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double num = (predictedServerTime - _since) / (_until - _since);
		float num2 = Mathf.Clamp01((float)num);
		_cooltime.fillAmount = num2;
		if (num2 >= 1f)
		{
			_since = 0.0;
			_until = 0.0;
			SetState(State.Normal);
			if (this.CooltimeEnded != null)
			{
				this.CooltimeEnded();
			}
		}
		else
		{
			SetState(State.Cooltime);
		}
	}

	public void Show(InteractionMenuData menu)
	{
		Init();
		bool flag = !_isShow || Menu.Action != menu.Action;
		_isShow = true;
		Set(menu);
		if (flag)
		{
			base.gameObject.SetActive(value: true);
			_hideTweener.SetOnFinished((EventDelegate.Callback)null);
			for (int i = 0; i < _effectTweeners.Length; i++)
			{
				_effectTweeners[i].tweenFactor = 0f;
				_effectTweeners[i].PlayForward();
			}
		}
	}

	public void Hide()
	{
		if (!_isShow)
		{
			return;
		}
		_isShow = false;
		for (int i = 0; i < _effectTweeners.Length; i++)
		{
			if (!(_effectTweeners[i] == _hideTweener))
			{
				_effectTweeners[i].enabled = false;
			}
		}
		_hideTweener.PlayReverse();
		_hideTweener.SetOnFinished(TweenerFinished);
	}

	private void TweenerFinished()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetCooltime(double since, double until)
	{
		_since = since;
		_until = until;
		if (until > 0.0)
		{
			UpdateRoutine();
		}
		else
		{
			_cooltime.fillAmount = 1f;
		}
	}

	protected virtual void SetState(State state)
	{
		_state = state;
	}

	private void Set(InteractionMenuData menu)
	{
		Menu = menu;
		Description = LocalizeSystem.Get("#context_action_desc_" + menu.Action);
		ItemIcon icon = menu.Icon;
		bool flag = false;
		if (string.IsNullOrEmpty(icon.Main))
		{
			_text.text = menu.Name;
			flag = true;
		}
		else
		{
			_icon.spriteName = icon.Main;
			UIUtility.ResizeToSquare(_icon);
		}
		_icon.gameObject.SetActive(!flag);
		_text.gameObject.SetActive(flag);
		SetState(State.Normal);
	}

	public void OnPress(bool press)
	{
		if (_isShow)
		{
			SetState((!press) ? State.Normal : State.Pressed);
			if (this.Pressed != null)
			{
				this.Pressed(this, press);
			}
		}
	}

	private void OnClick()
	{
		if (_isShow)
		{
			UISound.PlayClick(UISound.ClickType.ActionButtonDefault);
			if (this.Clicked != null)
			{
				this.Clicked(this);
			}
		}
	}

	protected virtual void OnHover(bool hover)
	{
		if (this.Hovered != null)
		{
			this.Hovered(this, hover);
		}
	}
}
