using System;
using System.Collections;
using UnityEngine;

public class DefaultSelectableButton : Selectable
{
	public enum ButtonStyle
	{
		Invalid,
		Yellow,
		Gray,
		Black,
		WhiteGray,
		Orange,
		LightYellow
	}

	[SerializeField]
	private ButtonStyle _style;

	[SerializeField]
	private bool _glitterWhenEnable;

	[SerializeField]
	private bool _rotateDotWhenGlittering;

	[SerializeField]
	private UISprite _border;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private GameObject _container;

	private DefaultSelectableButtonStyle.StyleMeta _meta;

	private UISpriteLabel _spriteLabel;

	private Vector3 _baseTextPos;

	private Vector3 _baseIconPos;

	private Vector3 _baseContainerPos;

	private UIWidget _widget;

	private bool _prevSelect;

	private GlitteringDots _glitteringDots;

	private UISprite _glitterBg;

	private bool _isGlitter;

	public ButtonStyle Style => _style;

	public UILabel TextLabel => _text;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public string Text
	{
		get
		{
			Init();
			if ((Object)(object)_spriteLabel != (Object)null)
			{
				return _spriteLabel.text;
			}
			if ((Object)(object)_text != (Object)null)
			{
				return _text.text;
			}
			return string.Empty;
		}
		set
		{
			Init();
			if ((Object)(object)_spriteLabel != (Object)null)
			{
				_spriteLabel.text = value;
			}
			else if ((Object)(object)_text != (Object)null)
			{
				_text.text = value;
			}
		}
	}

	public string Icon
	{
		set
		{
			Init();
			if (!((Object)(object)_icon == (Object)null))
			{
				string text = value;
				if (string.IsNullOrEmpty(text))
				{
					text = "icon_question";
				}
				_icon.spriteName = text;
			}
		}
	}

	public string Value { get; set; }

	private GlitteringDots GlitteringDots
	{
		get
		{
			if ((Object)(object)_glitteringDots == (Object)null)
			{
				_glitteringDots = ((Component)this).GetComponent<GlitteringDots>();
				if ((Object)(object)_glitteringDots == (Object)null)
				{
					_glitteringDots = ((Component)this).gameObject.AddComponent<GlitteringDots>();
					_glitteringDots.SetSprite(KSingleton<UIManager>.Instance().UIAtlas, "img_quickslot_circle");
				}
				int depth = Widget.depth;
				if ((Object)(object)_icon != (Object)null)
				{
					depth = _icon.depth;
				}
				else if ((Object)(object)_text != (Object)null)
				{
					depth = _text.depth;
				}
				else if ((Object)(object)_border != (Object)null)
				{
					depth = _border.depth;
				}
				_glitteringDots.SetDepth(depth + 2);
			}
			return _glitteringDots;
		}
	}

	private UISprite Glitter
	{
		get
		{
			if ((Object)(object)_glitterBg == (Object)null)
			{
				_glitterBg = ((Component)this).gameObject.AddChild<UISprite>();
				_glitterBg.atlas = KSingleton<UIManager>.Instance().AdditiveAtlas;
				_glitterBg.spriteName = "effect_button";
				_glitterBg.type = UIBasicSprite.Type.Sliced;
				_glitterBg.SetAnchor(((Component)Widget).gameObject, -16, -12, 12, 16);
				int depth = Widget.depth;
				if ((Object)(object)_border != (Object)null)
				{
					depth = _border.depth;
				}
				_glitterBg.depth = depth + 1;
				((Component)_glitterBg).gameObject.SetActive(false);
			}
			return _glitterBg;
		}
	}

	public void SetStyle(ButtonStyle style)
	{
		if (style != 0)
		{
			_style = style;
			_meta = DefaultSelectableButtonStyle.Get(_style);
			Refresh(select: false);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Refresh();
		if (_glitterWhenEnable)
		{
			Glittering(!base.Disable);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Glittering(enable: false);
	}

	protected override void OnInit()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (_style == ButtonStyle.Invalid)
		{
			_style = ButtonStyle.Yellow;
		}
		_meta = DefaultSelectableButtonStyle.Get(_style);
		BoxCollider component = ((Component)this).GetComponent<BoxCollider>();
		if ((Object)(object)component == (Object)null)
		{
			component = ((Component)this).gameObject.AddComponent<BoxCollider>();
			NGUITools.UpdateWidgetCollider(component, considerInactive: false);
		}
		if ((Object)(object)_text != (Object)null)
		{
			_text.color = Color.white;
			_text.supportEncoding = true;
			_spriteLabel = ((Component)_text).GetComponent<UISpriteLabel>();
		}
		base.Select = false;
	}

	protected override void Refresh(bool select)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		if (!_prevSelect)
		{
			if ((Object)(object)_container != (Object)null)
			{
				_baseContainerPos = _container.transform.localPosition;
			}
			if ((Object)(object)_text != (Object)null)
			{
				_baseTextPos = ((Component)_text).transform.localPosition;
			}
			if ((Object)(object)_icon != (Object)null)
			{
				_baseIconPos = ((Component)_icon).transform.localPosition;
			}
		}
		_prevSelect = select;
		bool flag = _meta.DisableBorderColor.a > 0f && _meta.DisableContentsColor.a > 0f;
		if ((Object)(object)_border != (Object)null)
		{
			string spriteName = ((!select) ? _meta.UnselectBorder.sprite : _meta.SelectBorder.sprite);
			Color color = ((!flag || !base.Disable) ? ((!select) ? _meta.UnselectBorderColor : _meta.SelectBorderColor) : _meta.DisableBorderColor);
			_border.spriteName = spriteName;
			_border.color = color;
		}
		Color color2 = ((!flag || !base.Disable) ? ((!select) ? _meta.UnselectContentsColor : _meta.SelectContentsColor) : _meta.DisableContentsColor);
		if ((Object)(object)_icon != (Object)null)
		{
			_icon.color = color2;
		}
		if ((Object)(object)_text != (Object)null)
		{
			_text.color = color2;
		}
		Vector3 val = (Vector3)((!select) ? Vector3.zero : new Vector3(4f, -4f));
		if ((Object)(object)_container != (Object)null)
		{
			_container.transform.localPosition = _baseContainerPos + val;
		}
		else
		{
			if ((Object)(object)_text != (Object)null)
			{
				((Component)_text).transform.localPosition = _baseTextPos + val;
			}
			if ((Object)(object)_icon != (Object)null)
			{
				((Component)_icon).transform.localPosition = _baseIconPos + val;
			}
		}
		if (!flag)
		{
			Widget.alpha = ((!base.Disable) ? 1f : 0.5f);
		}
	}

	protected override void OnSelectDisable(bool disable)
	{
		base.OnSelectDisable(disable);
		if (disable)
		{
			Glittering(enable: false);
		}
		else if (_glitterWhenEnable)
		{
			Glittering(enable: true);
		}
	}

	private void OnPress(bool press)
	{
		if (base.AsyncState == State.Normal)
		{
			Refresh(press || base.Select);
		}
	}

	public void Glittering(bool enable)
	{
		Glittering(enable, _rotateDotWhenGlittering);
	}

	public void Glittering(bool enable, bool rotateDot)
	{
		if (enable)
		{
			if (!_isGlitter && ((Component)this).gameObject.activeInHierarchy)
			{
				((MonoBehaviour)this).StartCoroutine(CoGlitterButton(rotateDot));
			}
		}
		else
		{
			_isGlitter = false;
		}
	}

	public void ToggleGlittering()
	{
		Glittering(!_isGlitter);
	}

	private IEnumerator CoGlitterButton(bool rotateDot)
	{
		_isGlitter = true;
		if (rotateDot)
		{
			GlitteringDots.InitPreset(GlitteringDots.PresetShape.Rect);
			GlitteringDots.Show();
		}
		UISprite glitter = Glitter;
		((Component)glitter).gameObject.SetActive(true);
		float timer = 0f;
		while (_isGlitter)
		{
			float alpha = (Mathf.Sin(timer * (float)Math.PI * 2f) + 1f) * 0.5f;
			glitter.alpha = alpha;
			timer += Time.deltaTime;
			yield return null;
		}
		((Component)glitter).gameObject.SetActive(false);
		if (rotateDot)
		{
			GlitteringDots.Hide();
		}
		_isGlitter = false;
	}
}
