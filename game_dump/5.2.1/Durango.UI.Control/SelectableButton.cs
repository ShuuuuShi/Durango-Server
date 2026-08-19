using System;
using System.Collections.Generic;
using Durango.System;
using L10N;
using UnityEngine;

namespace Durango.UI.Control;

[ExecuteInEditMode]
public class SelectableButton : Selectable, RectLayout.ICompatible
{
	[Serializable]
	private struct Padding
	{
		public int Top;

		public int Bottom;

		public int Left;

		public int Right;
	}

	public Action SubClicked;

	[SerializeField]
	[LocalizableString]
	private string _initText;

	[SerializeField]
	private int _fontSize;

	[SerializeField]
	private SpriteData _initIcon;

	[SerializeField]
	private int _iconSize;

	[SerializeField]
	private Padding _padding;

	[SerializeField]
	private PresetButton.Style _style = PresetButton.Style.Border;

	[SerializeField]
	private bool _isIcon;

	[HideInInspector]
	[SerializeField]
	private PresetButton _button;

	[HideInInspector]
	[SerializeField]
	private Selectable _subButton;

	[Tooltip("텍스트/아이콘 크기에 따라서 버튼 크기를 자동 변경")]
	[SerializeField]
	private bool _toPreferredSize;

	[SerializeField]
	private int _minWidth;

	[SerializeField]
	private int _maxWidth;

	[SerializeField]
	private int _minHeight;

	[SerializeField]
	private int _maxHeight;

	[SerializeField]
	private InputCommand _shortcutCommand;

	private PresetButton.Effect _effect;

	private EffectWidget _effectObject;

	private string _text;

	private string _icon;

	private Color _color = Color.white;

	private int _prevStyle = -1;

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
			_icon = null;
			if (_button != null)
			{
				string text = _text;
				if (Platform.Instance.UsePCUI && _shortcutCommand != 0)
				{
					string keyCaption = GameSystem<InputSystem>.Instance().Keyboard.GetKeyCaption(_shortcutCommand);
					if (!string.IsNullOrEmpty(keyCaption))
					{
						text = "[preset=keycode_box_on_button?" + keyCaption + "] " + text;
					}
				}
				_button.SetText(text, _fontSize);
			}
			if (_toPreferredSize)
			{
				ToPreferredSize();
			}
		}
	}

	public string Icon
	{
		get
		{
			return _icon;
		}
		set
		{
			_icon = value;
			_text = null;
			if (_button != null)
			{
				_button.SetIcon(_icon, _iconSize);
			}
			if (_toPreferredSize)
			{
				ToPreferredSize();
			}
		}
	}

	public Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
			if (_button != null)
			{
				_button.SetTint(_color);
			}
		}
	}

	public InputCommand ShortcutCommand
	{
		get
		{
			return _shortcutCommand;
		}
		set
		{
			if (_shortcutCommand != value)
			{
				_shortcutCommand = value;
				Text = Text;
			}
		}
	}

	public string Value { get; set; }

	public int MinWidth
	{
		get
		{
			return _minWidth;
		}
		set
		{
			_minWidth = value;
		}
	}

	public int MaxWidth
	{
		get
		{
			return _maxWidth;
		}
		set
		{
			_maxWidth = value;
		}
	}

	public int MinHeight
	{
		get
		{
			return _minHeight;
		}
		set
		{
			_minHeight = value;
		}
	}

	public int MaxHeight
	{
		get
		{
			return _maxHeight;
		}
		set
		{
			_maxHeight = value;
		}
	}

	public Selectable SubButton => _subButton;

	public PresetButton.Style GetStyle()
	{
		return _style;
	}

	public void SetStyle(PresetButton.Style style)
	{
		if (_style != style)
		{
			_style = style;
			if (Application.isPlaying)
			{
				MakeButton();
			}
		}
	}

	public void ClearEffect()
	{
		SetEffect(PresetButton.Effect.None);
	}

	public void SetEffect(PresetButton.Effect effect)
	{
		if (_effect == effect)
		{
			return;
		}
		_effect = effect;
		if (_effectObject != null)
		{
			UnityEngine.Object.Destroy(_effectObject.gameObject);
		}
		EffectWidget effect2 = SelectableButtonStyle.GetEffect(effect);
		if (!(effect2 == null))
		{
			_effectObject = UnityEngine.Object.Instantiate(effect2.gameObject, base.transform).GetComponent<EffectWidget>();
			Transform obj = _effectObject.transform;
			obj.localPosition = Vector3.zero;
			obj.localRotation = Quaternion.identity;
			obj.localScale = Vector3.one;
			if (_effectObject.gameObject.layer != base.gameObject.layer)
			{
				NGUITools.SetLayer(_effectObject.gameObject, base.gameObject.layer);
			}
			_effectObject.SetAnchor(base.gameObject, _padding.Left, _padding.Bottom, _padding.Right, _padding.Top);
			_effectObject.depth = base.Widget.depth + 100;
		}
	}

	public void ShowLoadingRing(bool show, Vector3? offset = null)
	{
		if (show)
		{
			UIManager.Popup.LoadingRing.AttachToWidget(base.gameObject, offset);
		}
		else
		{
			UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
		}
	}

	public Point2 GetPreferredSize()
	{
		Init();
		return _button.GetPreferredSize(MinWidth, MaxWidth, MinHeight, MaxHeight);
	}

	public Point2 ToPreferredSize()
	{
		Point2 preferredSize = GetPreferredSize();
		SetDimensions(preferredSize);
		return preferredSize;
	}

	public void SetDimensions(Point2 size)
	{
		SetDimensions(size.x, size.y);
	}

	public void SetDimensions(int width, int height)
	{
		base.Widget.SetDimensions(width, height);
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void OnInit()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (_icon == null && _text == null)
		{
			if (_isIcon)
			{
				if (!string.IsNullOrEmpty(_initIcon.sprite))
				{
					_icon = _initIcon.sprite;
					_text = null;
				}
			}
			else if (!string.IsNullOrEmpty(_initText))
			{
				_text = T._(_initText);
				_icon = null;
			}
		}
		MakeButton();
	}

	protected override void OnRefresh(State state)
	{
		if (!(_button == null))
		{
			_button.SetState(state);
			if (state != State.Pressed && _subButton != null)
			{
				_subButton.SetState(state);
			}
		}
	}

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			ShowPreview(show: true);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (!Application.isPlaying)
		{
			ShowPreview(show: false);
		}
	}

	private void OnPress(bool isPress)
	{
		base.Pressed = isPress;
	}

	private void OnSubButtonClick()
	{
		Selectable.Current = this;
		if (SubClicked != null)
		{
			SubClicked();
		}
		Selectable.Current = null;
	}

	private void DestroyButton()
	{
		if (_button != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_button.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_button.gameObject);
			}
		}
		if (_subButton != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_subButton.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_subButton.gameObject);
			}
		}
		_prevStyle = -1;
	}

	private void MakeButton()
	{
		int style = (int)_style;
		if (style != _prevStyle)
		{
			DestroyButton();
			PresetButton style2 = SelectableButtonStyle.GetStyle(_style);
			_prevStyle = style;
			if (style2 == null)
			{
				return;
			}
			_button = UnityEngine.Object.Instantiate(style2.gameObject, base.transform).GetComponent<PresetButton>();
			Transform obj = _button.transform;
			obj.localPosition = Vector3.zero;
			obj.localRotation = Quaternion.identity;
			obj.localScale = Vector3.one;
			if (_button.gameObject.layer != base.gameObject.layer)
			{
				NGUITools.SetLayer(_button.gameObject, base.gameObject.layer);
			}
			_button.Widget.SetAnchor(base.gameObject, _padding.Left, _padding.Bottom, _padding.Right, _padding.Top);
			Transform transform = _button.transform.Find("Sub");
			if (transform != null)
			{
				_subButton = transform.GetComponent<Selectable>();
				_subButton.Clicked = OnSubButtonClick;
			}
			int depth = base.Widget.depth;
			UIWidget[] componentsInChildren = _button.GetComponentsInChildren<UIWidget>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].depth += depth;
			}
			ClickSound = style2.ClickSoundType;
		}
		if (Application.isPlaying)
		{
			_button.Init();
			if (!string.IsNullOrEmpty(_icon))
			{
				Icon = _icon;
			}
			else
			{
				Text = Text;
			}
			_button.SetTint(_color);
			UIUtility.ResetAndUpdateAnchors(_button.transform);
			Refresh();
			return;
		}
		if (_isIcon)
		{
			string icon = ((!string.IsNullOrEmpty(_initIcon.sprite)) ? _initIcon.sprite : "icon_question");
			Icon = icon;
		}
		else
		{
			string text = ((!string.IsNullOrEmpty(_initText)) ? _initText : "Text");
			Text = text;
		}
		_button.SetTint(_color);
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(_button.transform);
		while (stack.Count > 0)
		{
			Transform transform2 = stack.Pop();
			transform2.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			int j = 0;
			for (int childCount = transform2.childCount; j < childCount; j++)
			{
				stack.Push(transform2.GetChild(j));
			}
		}
	}

	public void ShowPreview(bool show)
	{
		if (show)
		{
			MakeButton();
		}
		else
		{
			DestroyButton();
		}
	}

	public Vector2 UpdateLayout(float? x, float? y)
	{
		Init();
		UIWidget widget = base.Widget;
		int num = (x.HasValue ? ((int)x.Value) : widget.width);
		int num2 = (y.HasValue ? ((int)y.Value) : widget.height);
		widget.SetDimensions(num, num2);
		return new Vector2(num, num2);
	}
}
