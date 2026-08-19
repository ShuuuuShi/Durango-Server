using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class AlarmNotifyWidget : MonoBehaviour
{
	public Action<AlarmNotifyWidget> ShowFinished;

	public Action<AlarmNotifyWidget> HideFinished;

	[SerializeField]
	private UISprite _typeIcon;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UISpriteLabel _textLable;

	[SerializeField]
	private UIWidget _expandArrow;

	[SerializeField]
	private SoundEventType _sound;

	[SerializeField]
	private int[] _heightByLineCount;

	private AnimationWidget _animWidget;

	private Action _viewMoreAction;

	private float _duration;

	private bool _isActive;

	private bool _isInit;

	public AnimationWidget AnimWidget => (!(_animWidget != null)) ? (_animWidget = GetComponent<AnimationWidget>()) : _animWidget;

	public string Key { get; private set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
			uIEventListener.onClick = OnClick_ViewMore;
			uIEventListener.onDrag = DragAlarm;
			TweenAlpha tweener = AnimWidget.GetTweener<TweenAlpha>();
			tweener.AddOnFinished(OnFinishAnimation);
		}
	}

	private void Update()
	{
		if (_duration > 0f)
		{
			_duration -= Time.deltaTime;
		}
		else
		{
			Hide();
		}
	}

	private void DragAlarm(GameObject go, Vector2 drag)
	{
		Hide();
		UIManager.SetCurrentUITouchEvent(enable: false);
	}

	private void OnClick_ViewMore(GameObject go)
	{
		if (_viewMoreAction != null)
		{
			_viewMoreAction();
		}
		Hide();
	}

	public void Set(string key, string text, string typeIcon, Action viewMoreAction, Color32 iconColor)
	{
		_typeIcon.gameObject.SetActive(value: true);
		_portraitTexture.gameObject.SetActive(value: false);
		_typeIcon.spriteName = typeIcon;
		_typeIcon.color = iconColor;
		Set(key, text, viewMoreAction);
	}

	public void Set(string key, string text, PortraitBuilder.Argument portrait, Action viewMoreAction)
	{
		_typeIcon.gameObject.SetActive(value: false);
		_portraitTexture.gameObject.SetActive(value: true);
		PortraitBuilder.Set(portrait, _portraitTexture);
		Set(key, text, viewMoreAction);
	}

	private void Set(string key, string text, Action viewMoreAction)
	{
		Init();
		Key = key;
		_viewMoreAction = viewMoreAction;
		bool active = _viewMoreAction != null;
		_expandArrow.gameObject.SetActive(active);
		_textLable.text = text;
		int value = Mathf.Max(_textLable.height / _textLable.fontSize, 1);
		AnimWidget.Widget.height = _heightByLineCount[Mathf.Clamp(value, 0, _heightByLineCount.Length - 1)];
		UIUtility.UpdateAnchors(base.transform);
	}

	public void Show(float duration, Vector3 tweenOffset)
	{
		base.gameObject.SetActive(value: true);
		SoundManager.PlayEvent(_sound);
		SetVisibleDuration(duration);
		AnimWidget.SetAlpha(0f, useTween: false);
		AnimWidget.Alpha = 1f;
		Vector3 localPosition = base.transform.localPosition;
		AnimWidget.SetPosition(localPosition + tweenOffset, useTween: false);
		AnimWidget.Position = localPosition;
		_isActive = true;
	}

	public void SetVisibleDuration(float duration)
	{
		_duration = duration;
	}

	public void Hide()
	{
		if (_isActive)
		{
			Key = null;
			AnimWidget.Alpha = 0f;
			_isActive = false;
		}
	}

	public int GetHeight()
	{
		return AnimWidget.Widget.height;
	}

	private void OnFinishAnimation()
	{
		if (AnimWidget.Widget.alpha > 0f)
		{
			OnFinishShowAnimationTweener();
		}
		else
		{
			OnFinishedHideAnimationTweener();
		}
	}

	private void OnFinishShowAnimationTweener()
	{
		if (ShowFinished != null)
		{
			ShowFinished(this);
		}
	}

	private void OnFinishedHideAnimationTweener()
	{
		base.gameObject.SetActive(value: false);
		if (HideFinished != null)
		{
			HideFinished(this);
		}
	}
}
