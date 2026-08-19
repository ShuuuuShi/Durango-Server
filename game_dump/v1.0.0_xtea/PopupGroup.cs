using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupGroup : UIBase
{
	[SerializeField]
	private LoadingIconControl _loadingIcon;

	[SerializeField]
	private TextInputWidget _textInput;

	[SerializeField]
	private PlayerSearchWidget _playerSearch;

	[SerializeField]
	private AlarmControl _alarm;

	[SerializeField]
	private UIWidget _screenRotateButton;

	[SerializeField]
	private NewsAlarmWidget _newsAlarm;

	private Dictionary<Type, TooltipBase> _tooltipDict = new Dictionary<Type, TooltipBase>();

	private float _loadingIconActiveTime;

	private bool _isLoading;

	public PlayerSearchWidget PlayerSearch => _playerSearch;

	public TextInputWidget TextInput => _textInput;

	public AlarmControl Alarm => _alarm;

	public NewsAlarmWidget NewsAlarm => _newsAlarm;

	public bool IsLoading
	{
		get
		{
			return _isLoading;
		}
		set
		{
			_isLoading = value;
			TweenAlpha component = ((Component)_loadingIcon).GetComponent<TweenAlpha>();
			if (value)
			{
				if (!((Component)_loadingIcon).gameObject.activeSelf)
				{
					((Component)_loadingIcon).gameObject.SetActive(true);
				}
				component.delay = 0f;
				component.PlayForward();
				_loadingIconActiveTime = Time.time;
			}
			else
			{
				float num = Time.time - _loadingIconActiveTime;
				if (num < 0.5f)
				{
					component.delay = 0.5f - num;
				}
				else
				{
					component.delay = 0f;
				}
				component.PlayReverse();
				component.ResetToBeginning();
			}
		}
	}

	private void Awake()
	{
		_alarm.Init();
		_textInput.Init();
		_playerSearch.Init();
		((Component)_loadingIcon).GetComponent<TweenAlpha>().SetOnFinished(OnLoadingIconTweenFinished);
		((Component)_loadingIcon).gameObject.SetActive(false);
	}

	public T Tooltip<T>() where T : TooltipBase
	{
		T val;
		if (_tooltipDict.TryGetValue(typeof(T), out var value))
		{
			val = value as T;
		}
		else
		{
			val = ((Component)this).GetComponentInChildren<T>(true);
			_tooltipDict.Add(typeof(T), val);
		}
		if ((Object)(object)val != (Object)null)
		{
			val.Hide(instant: true);
		}
		return val;
	}

	private void OnLoadingIconTweenFinished()
	{
		TweenAlpha component = ((Component)_loadingIcon).GetComponent<TweenAlpha>();
		if (component.tweenFactor >= 1f)
		{
			component.delay = 5f;
			component.PlayReverse();
			component.ResetToBeginning();
		}
		else
		{
			((Component)_loadingIcon).gameObject.SetActive(false);
		}
	}
}
