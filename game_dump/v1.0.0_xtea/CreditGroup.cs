using System;
using UnityEngine;

public class CreditGroup : UIBase
{
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private UILabel _credits;

	[SerializeField]
	private GameObject _touchBox;

	[SerializeField]
	private UIWidget _closeButton;

	[SerializeField]
	private float _scrollSpeed = 100f;

	[SerializeField]
	private float _waitStartDuration = 1f;

	[SerializeField]
	private float _waitFinishDuration = 3f;

	[SerializeField]
	private float _touchPauseDuration = 1f;

	private bool _isScrolling;

	private float _startTimer;

	private float _finishTimer;

	private float _pauseTimer;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_touchBox);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (_closeButton.alpha > 0f)
			{
				Close();
			}
			else
			{
				TweenAlpha component = ((Component)_closeButton).GetComponent<TweenAlpha>();
				component.tweenFactor = 0f;
				component.PlayForward();
			}
		});
		UIScrollView scrollView = _scrollView.ScrollView;
		scrollView.onDragStarted = (UIScrollView.OnDragNotification)Delegate.Combine(scrollView.onDragStarted, new UIScrollView.OnDragNotification(OnScrollViewDragStart));
		UIScrollView scrollView2 = _scrollView.ScrollView;
		scrollView2.onStoppedMoving = (UIScrollView.OnDragNotification)Delegate.Combine(scrollView2.onStoppedMoving, new UIScrollView.OnDragNotification(OnScrollViewStopMoving));
		((Component)this).gameObject.SetActive(false);
	}

	protected override bool OnOpen()
	{
		TextAsset val = Resources.Load<TextAsset>("credit");
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		((Component)this).gameObject.SetActive(true);
		_closeButton.alpha = 0f;
		((Behaviour)((Component)_closeButton).GetComponent<TweenAlpha>()).enabled = false;
		TweenAlpha tweenAlpha = TweenAlpha.Begin(((Component)this).gameObject, 0.3f, 1f);
		tweenAlpha.from = 0f;
		_credits.text = val.text;
		_startTimer = _waitStartDuration;
		_finishTimer = _waitFinishDuration;
		_scrollView.Reposition(resetPosition: true, tween: false);
		return true;
	}

	protected override bool OnClose()
	{
		TweenAlpha tweenAlpha = TweenAlpha.Begin(((Component)this).gameObject, 0.3f, 0f);
		tweenAlpha.SetOnFinished(OnTweenFinish);
		return true;
	}

	private void OnScrollViewDragStart()
	{
		_isScrolling = true;
		_startTimer = 0f;
	}

	private void OnScrollViewStopMoving()
	{
		if (_isScrolling)
		{
			_isScrolling = false;
			_pauseTimer = _touchPauseDuration;
		}
	}

	private void OnTweenFinish()
	{
		float alpha = ((Component)this).GetComponent<UIRect>().alpha;
		if (alpha < 1f)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (!base.IsOpen || _isScrolling)
		{
			return;
		}
		if (_startTimer > 0f)
		{
			_startTimer -= Time.deltaTime;
			return;
		}
		if (_pauseTimer > 0f)
		{
			_pauseTimer -= Time.deltaTime;
			return;
		}
		float currentOffset = _scrollView.CurrentOffset;
		if (currentOffset >= _scrollView.MaxOffset)
		{
			if (_finishTimer > 0f)
			{
				_finishTimer -= Time.deltaTime;
			}
			else
			{
				Close();
			}
		}
		else
		{
			currentOffset += Time.deltaTime * _scrollSpeed;
			_scrollView.MoveTo(currentOffset, instant: true);
		}
	}
}
