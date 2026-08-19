using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public abstract class Selectable : MonoBehaviour
{
	public enum State
	{
		Normal,
		Selected,
		Pressed,
		Disabled,
		Hovered
	}

	public static Selectable Current;

	public Action Clicked;

	public Action RightClicked;

	public Action LongPressed;

	public Action DoubleClicked;

	public Action<bool> OnHovered;

	public Action<Selectable, State> StateUpdated;

	protected UISound.ClickType ClickSound = UISound.ClickType.ButtonDefault;

	protected string OverrideSound;

	private State _state;

	private bool _initialized;

	private bool _disabled;

	private bool _selected;

	private bool _pressed;

	private bool _hovered;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public bool CanClickWhenDisabled { get; set; }

	public bool IsChangDisabled { get; private set; }

	public bool IsChangeSelected { get; private set; }

	public bool IsChangePressed { get; private set; }

	public bool IsChangeHovered { get; private set; }

	public bool Disabled
	{
		get
		{
			return _disabled;
		}
		set
		{
			Init();
			IsChangDisabled = _disabled != value;
			_disabled = value;
			OnChangeState();
		}
	}

	public bool Selected
	{
		get
		{
			return _selected;
		}
		set
		{
			Init();
			IsChangeSelected = _selected != value;
			_selected = value;
			OnChangeState();
		}
	}

	public bool Pressed
	{
		get
		{
			return _pressed;
		}
		protected set
		{
			Init();
			IsChangePressed = _pressed != value;
			_pressed = value;
			OnChangeState();
		}
	}

	public bool Hovered
	{
		get
		{
			return _hovered;
		}
		protected set
		{
			Init();
			IsChangeHovered = _hovered != value;
			_hovered = value;
			OnChangeState();
		}
	}

	protected abstract void OnInit();

	protected abstract void OnRefresh(State state);

	public void SetClickSound(UISound.ClickType type)
	{
		Init();
		ClickSound = type;
		OverrideSound = null;
	}

	public void SetClickSound(string sound)
	{
		Init();
		OverrideSound = sound;
	}

	private void Awake()
	{
		Init();
	}

	protected virtual void OnDisable()
	{
		Pressed = false;
		Hovered = false;
	}

	public void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			_pressed = false;
			_disabled = false;
			_selected = false;
			_hovered = false;
			OnInit();
			IsChangDisabled = true;
			IsChangeSelected = true;
			IsChangePressed = true;
			IsChangeHovered = true;
			OnChangeState();
		}
	}

	public State GetState()
	{
		Init();
		if (_disabled)
		{
			return State.Disabled;
		}
		if (_pressed)
		{
			return State.Pressed;
		}
		if (_selected)
		{
			return State.Selected;
		}
		if (_hovered)
		{
			return State.Hovered;
		}
		return State.Normal;
	}

	public void SetState(State state)
	{
		Init();
		bool pressed = _pressed;
		bool disabled = _disabled;
		bool selected = _selected;
		bool hovered = _hovered;
		switch (state)
		{
		case State.Normal:
			_pressed = false;
			_disabled = false;
			_selected = false;
			_hovered = false;
			break;
		case State.Selected:
			_pressed = false;
			_disabled = false;
			_selected = true;
			_hovered = false;
			break;
		case State.Pressed:
			_pressed = true;
			_disabled = false;
			_selected = false;
			_hovered = false;
			break;
		case State.Disabled:
			_pressed = false;
			_disabled = true;
			_selected = false;
			_hovered = false;
			break;
		case State.Hovered:
			_pressed = false;
			_disabled = false;
			_selected = false;
			_hovered = true;
			break;
		}
		IsChangePressed = pressed != _pressed;
		IsChangDisabled = disabled != _disabled;
		IsChangeSelected = selected != _selected;
		IsChangeHovered = hovered != _hovered;
		OnChangeState();
	}

	private void OnChangeState()
	{
		State state = GetState();
		OnRefresh(state);
		if (_state != state && StateUpdated != null)
		{
			Current = this;
			StateUpdated(this, state);
			Current = null;
		}
		_state = state;
		IsChangDisabled = false;
		IsChangeSelected = false;
		IsChangePressed = false;
		IsChangeHovered = false;
	}

	public void Refresh()
	{
		OnRefresh(_state);
	}

	[UsedImplicitly]
	protected virtual void OnClick()
	{
		Current = this;
		bool flag = CanClickWhenDisabled || !Disabled;
		if (flag)
		{
			if (string.IsNullOrEmpty(OverrideSound))
			{
				UISound.PlayClick(ClickSound);
			}
			else
			{
				SoundManager.PlayEvent(OverrideSound);
			}
		}
		if (Clicked != null && flag)
		{
			Clicked();
		}
		Current = null;
	}

	[UsedImplicitly]
	protected virtual void OnRightClick()
	{
		Current = this;
		bool flag = CanClickWhenDisabled || !Disabled;
		if (flag)
		{
			if (string.IsNullOrEmpty(OverrideSound))
			{
				UISound.PlayClick(ClickSound);
			}
			else
			{
				SoundManager.PlayEvent(OverrideSound);
			}
		}
		if (RightClicked != null && flag)
		{
			RightClicked();
		}
		Current = null;
	}

	[UsedImplicitly]
	protected virtual void OnDoubleClick()
	{
		Current = this;
		bool flag = CanClickWhenDisabled || !Disabled;
		if (flag)
		{
			if (string.IsNullOrEmpty(OverrideSound))
			{
				UISound.PlayClick(ClickSound);
			}
			else
			{
				SoundManager.PlayEvent(OverrideSound);
			}
		}
		if (DoubleClicked != null && flag)
		{
			DoubleClicked();
		}
		Current = null;
	}

	[UsedImplicitly]
	protected virtual void OnLongPress()
	{
		Current = this;
		bool flag = CanClickWhenDisabled || !Disabled;
		if (flag)
		{
			if (string.IsNullOrEmpty(OverrideSound))
			{
				UISound.PlayClick(ClickSound);
			}
			else
			{
				SoundManager.PlayEvent(OverrideSound);
			}
		}
		if (LongPressed != null && flag)
		{
			LongPressed();
		}
		Current = null;
	}

	[UsedImplicitly]
	protected virtual void OnHover(bool isHover)
	{
		Hovered = isHover;
		if (OnHovered != null)
		{
			OnHovered(Hovered);
		}
	}
}
