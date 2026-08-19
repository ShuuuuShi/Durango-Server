using System;
using UnityEngine;

public class AlarmWidget : MonoBehaviour
{
	private const int Padding = 10;

	public Action<AlarmWidget> ShowFinished;

	public Action<AlarmWidget> HideFinished;

	[SerializeField]
	private UISprite _typeIcon;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UISpriteLabel _textLable;

	[SerializeField]
	private UIWidget _expandArrow;

	private AnimationWidget _animWidget;

	private Action _viewMoreAction;

	private float _duration;

	private bool _isActive;

	private int _defaultTextWidth;

	private bool _isInit;

	public AnimationWidget AnimWidget => (!((Object)(object)_animWidget != (Object)null)) ? (_animWidget = ((Component)this).GetComponent<AnimationWidget>()) : _animWidget;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIEventListener uIEventListener = UIEventListener.Get(((Component)this).gameObject);
			uIEventListener.onClick = OnClick_ViewMore;
			uIEventListener.onDrag = DragAlarm;
			TweenAlpha tweener = AnimWidget.GetTweener<TweenAlpha>();
			tweener.AddOnFinished(OnFinishAnimation);
			_defaultTextWidth = _textLable.Label.width;
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

	public void Set(string text, string typeIcon, Action viewMoreAction)
	{
		((Component)_typeIcon).gameObject.SetActive(true);
		((Component)_portraitTexture).gameObject.SetActive(false);
		_typeIcon.spriteName = typeIcon;
		Set(text, viewMoreAction);
	}

	public void Set(string text, PortraitBuilder.Argument portrait, Action viewMoreAction)
	{
		((Component)_typeIcon).gameObject.SetActive(false);
		((Component)_portraitTexture).gameObject.SetActive(true);
		PortraitBuilder.Set(portrait, _portraitTexture);
		Set(text, viewMoreAction);
	}

	private void Set(string text, Action viewMoreAction)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		Init();
		_viewMoreAction = viewMoreAction;
		bool flag = _viewMoreAction != null;
		((Component)_expandArrow).gameObject.SetActive(flag);
		_textLable.Label.width = _defaultTextWidth - (flag ? _expandArrow.width : 0);
		_textLable.text = text;
		int num = Mathf.Max((!((Component)_typeIcon).gameObject.activeSelf) ? _portraitTexture.height : _typeIcon.height, _textLable.Label.height);
		num += 20;
		Vector3 localPosition = ((Component)_textLable).transform.localPosition;
		localPosition.y = (float)num * 0.5f;
		((Component)_textLable).transform.localPosition = localPosition;
		AnimWidget.Widget.height = num;
		UIUtility.UpdateAnchors(((Component)this).transform);
		AnimWidget.SetAlpha(0f, useTween: false);
		NGUITools.UpdateWidgetCollider(((Component)AnimWidget).gameObject);
	}

	public void Show(float duration)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		_duration = duration;
		AnimWidget.Alpha = 1f;
		Vector3 localPosition = ((Component)this).transform.localPosition;
		AnimWidget.SetPosition(localPosition + Vector3.down * 10f, useTween: false);
		AnimWidget.Position = localPosition;
		_isActive = true;
	}

	public void Hide()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (_isActive)
		{
			AnimWidget.Alpha = 0f;
			AnimWidget.Position += Vector3.right * 10f;
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
		((Component)this).gameObject.SetActive(false);
		if (HideFinished != null)
		{
			HideFinished(this);
		}
	}
}
