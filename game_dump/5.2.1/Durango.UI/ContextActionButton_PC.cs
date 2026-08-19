using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ContextActionButton_PC : ContextActionButtonBase
{
	[SerializeField]
	private UISprite _bg;

	[SerializeField]
	private UISprite _line;

	[SerializeField]
	private UISprite _colorOverlay;

	[SerializeField]
	private Color _cooltimeTextColor;

	[SerializeField]
	private Color _cooltimeLineColor;

	[SerializeField]
	private Color _cooltimeIconColor;

	[SerializeField]
	private string _normalBGName;

	[SerializeField]
	private string _cooltimeBGName;

	[SerializeField]
	private TweenerPlayer _tweenCooltimeEffect;

	[SerializeField]
	private GameObject _cooltimeEffect;

	[SerializeField]
	private UILabel _shortcut;

	protected override void Start()
	{
		base.Start();
		base.CooltimeEnded += OnEndedCooltime;
	}

	protected override void SetState(State state)
	{
		if (_state != state)
		{
			switch (state)
			{
			case State.Normal:
				_bg.spriteName = _normalBGName;
				_cooltime.gameObject.SetActive(value: false);
				break;
			case State.Hovered:
				_bg.spriteName = _normalBGName;
				_cooltime.gameObject.SetActive(value: false);
				break;
			case State.Pressed:
				_bg.spriteName = _normalBGName;
				_cooltime.gameObject.SetActive(value: false);
				break;
			case State.Cooltime:
				_text.color = _cooltimeTextColor;
				_icon.color = _cooltimeIconColor;
				_bg.spriteName = _cooltimeBGName;
				_line.color = _cooltimeLineColor;
				_colorOverlay.gameObject.SetActive(value: false);
				_cooltime.gameObject.SetActive(value: true);
				break;
			}
			base.SetState(state);
		}
	}

	protected override void OnHover(bool hover)
	{
		SetState((!hover) ? State.Normal : State.Hovered);
		base.OnHover(hover);
	}

	private void OnEndedCooltime()
	{
		_cooltimeEffect.gameObject.SetActive(value: true);
		_tweenCooltimeEffect.Play(forward: true, OnFinishedCooltimeEffect);
	}

	private void OnFinishedCooltimeEffect()
	{
		_cooltimeEffect.gameObject.SetActive(value: false);
	}

	public void SetShortcut(KeyCode shortcut)
	{
		_shortcut.gameObject.SetActive(shortcut != KeyCode.None);
		_shortcut.text = InputKeyboard.KeyToCaption(shortcut);
	}
}
