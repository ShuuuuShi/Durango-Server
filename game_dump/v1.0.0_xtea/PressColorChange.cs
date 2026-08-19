using System;
using UnityEngine;

public class PressColorChange : MonoBehaviour
{
	[Serializable]
	private struct ObjectColorStruct
	{
		public UIWidget Widget;

		public Color Color;

		public Color SelectColor;

		public Color DisableColor;

		private Color _origin;

		private Color _tint;

		private State State { get; set; }

		public void Init()
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Widget == (Object)null)
			{
				_origin = Color.white;
			}
			else
			{
				_origin = Widget.color;
				TweenColor component = ((Component)Widget).GetComponent<TweenColor>();
				TweenAlpha component2 = ((Component)Widget).GetComponent<TweenAlpha>();
				if ((Object)(object)component != (Object)null)
				{
					_origin = component.to;
				}
				if ((Object)(object)component2 != (Object)null)
				{
					_origin.a = component2.to;
				}
			}
			_tint = Color.white;
		}

		public void SetColor(Color col)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_origin = col;
			Refresh();
		}

		public void SetState(State state)
		{
			State = state;
			Refresh();
		}

		private void Refresh()
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Widget != (Object)null)
			{
				Color val = (Color)(State switch
				{
					State.Normal => _origin, 
					State.Press => Color, 
					State.Disable => DisableColor, 
					State.Select => SelectColor, 
					_ => _origin, 
				});
				Widget.color = val * _tint;
			}
		}

		public void SetTint(Color col)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_tint = col;
			Refresh();
		}
	}

	private enum State
	{
		Normal,
		Press,
		Disable,
		Select
	}

	[SerializeField]
	private ObjectColorStruct[] _objects;

	[SerializeField]
	private bool _useSelectColor;

	[SerializeField]
	private bool _useDisableColor;

	private bool _isSelect;

	private bool _isDisable;

	private bool _isPress;

	private bool _isInit;

	private void Awake()
	{
		Init();
	}

	private void OnDisable()
	{
		_isSelect = false;
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			int i = 0;
			for (int num = ((_objects != null) ? _objects.Length : 0); i < num; i++)
			{
				_objects[i].Init();
			}
		}
	}

	private void OnPress(bool press)
	{
		Press(press);
	}

	public void Press(bool press)
	{
		_isPress = press;
		Refresh();
	}

	public void Select(bool select)
	{
		_isSelect = select;
		Refresh();
	}

	public void Disable(bool disable)
	{
		_isDisable = disable;
		Refresh();
	}

	public void SetTint(Color col)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Init();
		int i = 0;
		for (int num = ((_objects != null) ? _objects.Length : 0); i < num; i++)
		{
			_objects[i].SetTint(col);
		}
	}

	private void Refresh()
	{
		Init();
		State state = State.Normal;
		if (_useDisableColor && _isDisable)
		{
			state = State.Disable;
		}
		else if (_isSelect)
		{
			state = ((!_useSelectColor) ? State.Press : State.Select);
		}
		else if (_isPress)
		{
			state = State.Press;
		}
		int i = 0;
		for (int num = ((_objects != null) ? _objects.Length : 0); i < num; i++)
		{
			_objects[i].SetState(state);
		}
	}

	public void SetColor(UIWidget widget, Color color)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int num = ((_objects != null) ? _objects.Length : 0); i < num; i++)
		{
			if ((Object)(object)_objects[i].Widget == (Object)(object)widget)
			{
				_objects[i].SetColor(color);
				return;
			}
		}
		widget.color = color;
	}

	public void SetOriginColor(Color color)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int num = ((_objects != null) ? _objects.Length : 0); i < num; i++)
		{
			_objects[i].SetColor(color);
		}
	}
}
