using System;
using UnityEngine;

namespace Durango.UI.Control;

[Serializable]
public class ButtonState
{
	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private State _base;

	[SerializeField]
	private State _press;

	[SerializeField]
	private State _select;

	[SerializeField]
	private State _disable;

	[SerializeField]
	private State _hover;

	private Selectable.State _colorState = Selectable.State.Pressed;

	private Selectable.State _scaleState = Selectable.State.Pressed;

	private Selectable.State _tweenerState = Selectable.State.Pressed;

	private Selectable.State _tweenerModState = Selectable.State.Pressed;

	private Selectable.State _activeState = Selectable.State.Pressed;

	private bool _tintChanged;

	private UITweener[] _tweeners;

	private Color _tint = Color.white;

	public Color Tint
	{
		get
		{
			return _tint;
		}
		set
		{
			if (!(_tint == value))
			{
				_tintChanged = true;
				_tint = value;
			}
		}
	}

	public void SetState(Selectable.State state)
	{
		if (TryGetState(state, UseState.Color, out var res) && (!Application.isPlaying || _tintChanged || _colorState != res))
		{
			_colorState = res;
			_tintChanged = false;
			SetColor(GetState(res).Color * _tint);
		}
		if (TryGetState(state, UseState.Scale, out var res2) && (!Application.isPlaying || _scaleState != res2))
		{
			_scaleState = res2;
			State state2 = GetState(res2);
			SetScale(((state2.Use & UseState.Scale) != 0) ? state2.Scale : Vector3.one);
		}
		if (TryGetState(state, UseState.Tweener, out var res3) && (!Application.isPlaying || _tweenerState != res3))
		{
			_tweenerState = res3;
			State state3 = GetState(res3);
			TweenerOn(((state3.Use & UseState.Tweener) != 0) ? state3.GetTweeners(_widget) : null);
		}
		if (TryGetState(state, UseState.TweenerMod, out var res4) && (!Application.isPlaying || _tweenerModState != res4))
		{
			_tweenerModState = res4;
			State state4 = GetState(res4);
			TweenerModOn(((state4.Use & UseState.TweenerMod) != 0) ? state4.GetTweeners(_widget) : null, state4.IsForward);
		}
		if (TryGetState(state, UseState.Active, out var res5) && (!Application.isPlaying || _activeState != res5))
		{
			_activeState = res5;
			State state5 = GetState(res5);
			if ((state5.Use & UseState.Active) != 0)
			{
				_widget.gameObject.SetActive(state5.IsActive);
			}
		}
	}

	private void SetColor(Color color)
	{
		TweenerOff();
		_widget.color = color;
	}

	private void SetScale(Vector3 scale)
	{
		TweenerOff();
		_widget.transform.localScale = scale;
	}

	private void TweenerOn(UITweener[] tweeners)
	{
		_tweeners = tweeners;
		int i = 0;
		for (int size = KUtility.GetSize(tweeners); i < size; i++)
		{
			tweeners[i].ResetToBeginning();
			tweeners[i].PlayForward();
		}
	}

	private void TweenerOff()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_tweeners); i < size; i++)
		{
			_tweeners[i].enabled = false;
		}
		_tweeners = null;
	}

	private void TweenerModOn(UITweener[] tweeners, bool isPlayForward)
	{
		_tweeners = tweeners;
		if (isPlayForward)
		{
			int i = 0;
			for (int size = KUtility.GetSize(tweeners); i < size; i++)
			{
				tweeners[i].tweenFactor = 0f;
				tweeners[i].Sample(0f, isFinished: false);
				tweeners[i].PlayForward();
			}
		}
		else
		{
			int j = 0;
			for (int size2 = KUtility.GetSize(tweeners); j < size2; j++)
			{
				tweeners[j].Sample(1f, isFinished: false);
				tweeners[j].PlayReverse();
			}
		}
	}

	private bool TryGetState(Selectable.State state, UseState use, out Selectable.State res)
	{
		switch (state)
		{
		case Selectable.State.Selected:
			if ((_select.Use & use) != 0)
			{
				res = Selectable.State.Selected;
				return true;
			}
			if ((_press.Use & use) != 0)
			{
				res = Selectable.State.Pressed;
				return true;
			}
			break;
		case Selectable.State.Pressed:
			res = Selectable.State.Pressed;
			return (_press.Use & use) != 0;
		case Selectable.State.Disabled:
			if ((_disable.Use & use) != 0)
			{
				res = Selectable.State.Disabled;
				return true;
			}
			break;
		case Selectable.State.Hovered:
			if ((_hover.Use & use) != 0)
			{
				res = Selectable.State.Hovered;
				return true;
			}
			break;
		}
		res = Selectable.State.Normal;
		return true;
	}

	private State GetState(Selectable.State state)
	{
		return state switch
		{
			Selectable.State.Selected => _select, 
			Selectable.State.Pressed => _press, 
			Selectable.State.Disabled => _disable, 
			Selectable.State.Hovered => _hover, 
			_ => _base, 
		};
	}

	public static ButtonState Make(UIWidget widget)
	{
		if (widget == null)
		{
			return null;
		}
		ButtonState buttonState = new ButtonState();
		buttonState._widget = widget;
		Color color = widget.color;
		Vector3 localScale = widget.transform.localScale;
		buttonState._base = new State
		{
			Use = UseState.Color,
			Color = color,
			Scale = localScale,
			IsForward = true,
			IsActive = true
		};
		buttonState._press = new State
		{
			Use = UseState.Color,
			Color = color,
			Scale = localScale,
			IsForward = true,
			IsActive = true
		};
		buttonState._select = new State
		{
			Use = (UseState)0,
			Color = color,
			Scale = localScale,
			IsForward = true,
			IsActive = true
		};
		buttonState._disable = new State
		{
			Use = (UseState)0,
			Color = color,
			Scale = localScale,
			IsForward = true,
			IsActive = true
		};
		buttonState._hover = new State
		{
			Use = (UseState)0,
			Color = color,
			Scale = localScale,
			IsForward = true,
			IsActive = true
		};
		return buttonState;
	}
}
