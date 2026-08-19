using System.Collections.Generic;
using UnityEngine;

public class TimeLimitWidget : MonoBehaviour
{
	public enum VisibleState
	{
		Wait,
		FadeIn,
		Show,
		FadeOut,
		Hide
	}

	protected List<EventDelegate> onFinished = new List<EventDelegate>();

	private VisibleState _state;

	private UIWidget _widget;

	[SerializeField]
	private bool _alwaysShowFade = true;

	[SerializeField]
	private float _fadeoutTime = 0.3f;

	[SerializeField]
	private float _fadeinDelayTime = 0.1f;

	[SerializeField]
	private float _fadeinTime = 0.3f;

	private float _startTime;

	private float _endTime;

	private float _visibleDuration;

	private bool _ignoreTimeScale = true;

	public VisibleState State
	{
		get
		{
			return _state;
		}
		private set
		{
			if (_state != value)
			{
				_state = value;
				OnChangeState();
			}
		}
	}

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

	public bool IsVisible => ((Component)this).gameObject.activeSelf;

	private float GetTime()
	{
		return (!_ignoreTimeScale) ? Time.time : RealTime.time;
	}

	private float GetDeltaTime()
	{
		return (!_ignoreTimeScale) ? Time.deltaTime : RealTime.deltaTime;
	}

	public void Visible(float duration)
	{
		if (IsVisible)
		{
			OnFinish();
			EventDelegate.Execute(onFinished);
			if (_alwaysShowFade)
			{
				_startTime = GetTime();
				Widget.alpha = 0f;
			}
		}
		else
		{
			_startTime = GetTime();
			Widget.alpha = 0f;
			((Component)this).gameObject.SetActive(true);
		}
		_visibleDuration = duration;
		_endTime = GetTime() + duration;
	}

	public void VisibleTimeReset()
	{
		_endTime = GetTime() + _visibleDuration;
	}

	public void Hide(bool instant = false)
	{
		Hide((!instant) ? _fadeoutTime : 0f);
	}

	public void Hide(float delay)
	{
		if (delay > 0f)
		{
			if (!(GetTime() - (_endTime - _visibleDuration) < GetDeltaTime()))
			{
				_endTime = Mathf.Min(GetTime() + delay, _endTime);
			}
		}
		else if (IsVisible)
		{
			_endTime = 0f;
			Update();
		}
	}

	private void Update()
	{
		if (_endTime < GetTime())
		{
			((Component)this).gameObject.SetActive(false);
			State = VisibleState.Hide;
			OnFinish();
			EventDelegate.Execute(onFinished);
			return;
		}
		float time = GetTime();
		float num = time - _startTime;
		float num2 = _endTime - time;
		if (num < _fadeinDelayTime)
		{
			State = VisibleState.Wait;
			Widget.alpha = 0f;
		}
		else if (num < _fadeinTime + _fadeinDelayTime)
		{
			State = VisibleState.FadeIn;
			Widget.alpha = (num - _fadeinDelayTime) / _fadeinTime;
		}
		else if (_fadeoutTime > num2)
		{
			State = VisibleState.FadeOut;
			Widget.alpha = Mathf.Clamp01(num2 / _fadeoutTime);
		}
		else
		{
			State = VisibleState.Show;
			Widget.alpha = 1f;
		}
		OnUpdate();
	}

	public void AddOnFinished(EventDelegate.Callback func)
	{
		EventDelegate.Add(onFinished, func, oneShot: true);
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnFinish()
	{
	}

	protected virtual void OnChangeState()
	{
	}
}
