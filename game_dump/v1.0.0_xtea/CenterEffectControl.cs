using System;
using UnityEngine;

public class CenterEffectControl : MonoBehaviour
{
	public Action Closing;

	public Action Closed;

	[SerializeField]
	private UISprite _bg;

	[SerializeField]
	private UISprite _mainIcon;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private UISprite _labelBG;

	[SerializeField]
	private UIAtlas[] _atlas;

	private UISprite[] _outline;

	private readonly Vector3[] OutlineDir = (Vector3[])(object)new Vector3[4]
	{
		Vector3.up,
		Vector3.down,
		Vector3.right,
		Vector3.left
	};

	private TweenerPlayer _effectPlayer;

	private AnimationWidget _animWidget;

	private float _labelBgMinWidth;

	private TweenerPlayer EffectPlayer
	{
		get
		{
			if ((Object)(object)_effectPlayer == (Object)null)
			{
				_effectPlayer = ((Component)this).GetComponent<TweenerPlayer>();
			}
			return _effectPlayer;
		}
	}

	private AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	private void Awake()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		_labelBgMinWidth = _labelBG.width;
		_outline = new UISprite[4];
		int i = 0;
		for (int num = _outline.Length; i < num; i++)
		{
			_outline[i] = ((Component)((Component)_mainIcon).transform.parent).gameObject.AddChild(((Component)_mainIcon).gameObject).GetComponent<UISprite>();
			_outline[i].color = Color.black;
			_outline[i].depth = _mainIcon.depth - (i + 1);
			((Component)_outline[i]).transform.localEulerAngles = ((Component)_mainIcon).transform.localEulerAngles;
		}
	}

	public void Play(string text, string icon, int iconSize, int outlineSize, Color iconColor, Color bgColor, Color textColor, EventDelegate.Callback finish = null)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		AnimWidget.SetAlpha(1f, useTween: false);
		_text.text = text;
		float x = _text.printedSize.x;
		int width = (int)Mathf.Max(x + 40f, _labelBgMinWidth);
		AnimWidget.Widget.width = width;
		_labelBG.width = width;
		int i = 0;
		for (int num = _atlas.Length; i < num; i++)
		{
			if (_atlas[i].GetSprite(icon) != null)
			{
				_mainIcon.atlas = _atlas[i];
				_mainIcon.spriteName = icon;
				break;
			}
		}
		UIUtility.ResizeToSquare(_mainIcon, iconSize);
		_mainIcon.color = iconColor;
		_bg.color = bgColor;
		_text.color = textColor;
		int j = 0;
		for (int num2 = _outline.Length; j < num2; j++)
		{
			((Component)_outline[j]).gameObject.SetActive(outlineSize > 0);
			if (outlineSize > 0)
			{
				((Component)_outline[j]).transform.localPosition = ((Component)_mainIcon).transform.localPosition + OutlineDir[j] * (float)outlineSize;
				_outline[j].width = _mainIcon.width;
				_outline[j].height = _mainIcon.height;
				_outline[j].atlas = _mainIcon.atlas;
				_outline[j].spriteName = _mainIcon.spriteName;
			}
		}
		EffectPlayer.Play(finish);
	}

	public void Close()
	{
		AnimWidget.Alpha = 0f;
		if (Closing != null)
		{
			Closing();
			Closing = null;
		}
	}

	private void OnDrag(Vector2 delta)
	{
		UIManager.SetCurrentUITouchEvent(enable: false);
	}

	private void OnPress(bool press)
	{
		Close();
	}

	private void OnDisable()
	{
		if (Closed != null)
		{
			Closed();
			Closed = null;
		}
	}
}
