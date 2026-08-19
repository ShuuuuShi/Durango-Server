using System;
using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

[Uri("Credit")]
public class CreditGroup : UIBase
{
	[Serializable]
	private struct CreditFile
	{
		public string Locale;

		public string File;
	}

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

	[SerializeField]
	private List<CreditFile> _creditFiles;

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
				TweenAlpha component = _closeButton.GetComponent<TweenAlpha>();
				component.tweenFactor = 0f;
				component.PlayForward();
			}
		});
		UIScrollView scrollView = _scrollView.ScrollView;
		scrollView.onDragStarted = (UIScrollView.OnDragNotification)Delegate.Combine(scrollView.onDragStarted, new UIScrollView.OnDragNotification(OnScrollViewDragStart));
		UIScrollView scrollView2 = _scrollView.ScrollView;
		scrollView2.onStoppedMoving = (UIScrollView.OnDragNotification)Delegate.Combine(scrollView2.onStoppedMoving, new UIScrollView.OnDragNotification(OnScrollViewStopMoving));
		base.gameObject.SetActive(value: false);
	}

	protected override bool TryOpen()
	{
		TextAsset creaditFile = GetCreaditFile();
		if (creaditFile == null)
		{
			return false;
		}
		base.gameObject.SetActive(value: true);
		_closeButton.alpha = 0f;
		_closeButton.GetComponent<TweenAlpha>().enabled = false;
		GetComponent<UIRect>().alpha = 0f;
		TweenAlpha.Begin(base.gameObject, 0.3f, 1f);
		_credits.text = creaditFile.text;
		_startTimer = _waitStartDuration;
		_finishTimer = _waitFinishDuration;
		_scrollView.Reposition(resetPosition: true, tween: false);
		return true;
	}

	protected override bool TryClose()
	{
		TweenAlpha.Begin(base.gameObject, 0.3f, 0f).SetOnFinished(OnTweenFinish);
		return true;
	}

	private TextAsset GetCreaditFile()
	{
		int index = 0;
		for (int i = 0; i < _creditFiles.Count; i++)
		{
			if (_creditFiles[i].Locale == LocalizeSystem.Locale)
			{
				index = i;
				break;
			}
		}
		return Resources.Load<TextAsset>(_creditFiles[index].File);
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
		if (GetComponent<UIRect>().alpha < 1f)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (!base.IsOpened || _isScrolling)
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
