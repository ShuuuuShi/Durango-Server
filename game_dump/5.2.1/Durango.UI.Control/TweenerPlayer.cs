using System;
using System.Collections.Generic;
using Durango.Development;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class TweenerPlayer : MonoBehaviour
{
	[SerializeField]
	[SortedUnityObjectList]
	private GameObject[] _tweeners;

	[SerializeField]
	private bool _playWhenEnable;

	[SerializeField]
	private bool _playWhenPress;

	[SerializeField]
	private bool _playWhenClick;

	[SerializeField]
	private bool _deactiveWhenFinish;

	[SerializeField]
	private bool _loop;

	private Action _onAllTweenerFinished;

	private bool _isInitTweeners;

	private List<UITweener> _cachedTweeners;

	private bool _isPlaying;

	private bool _isForward;

	private float _reservedAt;

	public event Action Played;

	private void OnEnable()
	{
		if (_playWhenEnable)
		{
			Play();
		}
	}

	private void Update()
	{
		if (_reservedAt > 0f && _reservedAt < Time.time)
		{
			PlayTweeners();
		}
	}

	[UsedImplicitly]
	private void OnPress(bool press)
	{
		if (press && _playWhenPress)
		{
			Play();
		}
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (_playWhenClick)
		{
			Play();
		}
	}

	private void InitTweeners()
	{
		if (_isInitTweeners)
		{
			return;
		}
		_isInitTweeners = true;
		_cachedTweeners = new List<UITweener>();
		int i = 0;
		for (int size = KUtility.GetSize(_tweeners); i < size; i++)
		{
			UITweener[] array = ((!(_tweeners[i] == null)) ? _tweeners[i].GetComponents<UITweener>() : null);
			if (array != null)
			{
				_cachedTweeners.AddRange(array);
			}
		}
		_cachedTweeners.Sort((UITweener t1, UITweener t2) => t1.delay.CompareTo(t2.delay));
		if (Application.isPlaying)
		{
			for (int j = 0; j < _cachedTweeners.Count; j++)
			{
				_cachedTweeners[j].SetOnFinished(OnFinishTweener);
			}
		}
	}

	private void ResetToBeginning()
	{
		if (_isForward)
		{
			ResetToFirst();
		}
		else
		{
			ResetToLast();
		}
	}

	public void SetDeactiveWhenFinish(bool isDeactivate)
	{
		_deactiveWhenFinish = isDeactivate;
	}

	public void ResetToFirst()
	{
		InitTweeners();
		for (int num = _cachedTweeners.Count - 1; num >= 0; num--)
		{
			UITweener uITweener = _cachedTweeners[num];
			uITweener.tweenFactor = 0f;
			uITweener.Sample(0f, isFinished: false);
			uITweener.enabled = false;
		}
	}

	public void ResetToLast()
	{
		InitTweeners();
		for (int i = 0; i < _cachedTweeners.Count; i++)
		{
			UITweener uITweener = _cachedTweeners[i];
			uITweener.tweenFactor = 1f;
			uITweener.Sample(1f, isFinished: false);
			uITweener.enabled = false;
		}
	}

	public void Play(float delay = 0f, int tweenGroup = 0, float duration = 0f)
	{
		Play(forward: true, null, delay, tweenGroup, duration);
	}

	public void Play(Action finishCallback, float delay = 0f, int tweenGroup = 0, float duration = 0f)
	{
		Play(forward: true, finishCallback, delay, tweenGroup, duration);
	}

	public void Play(bool forward, Action finishCallback, float delay = 0f, int tweenGroup = 0, float duration = 0f)
	{
		_isForward = forward;
		ResetToBeginning();
		_onAllTweenerFinished = finishCallback;
		if (delay > 0f)
		{
			_reservedAt = Time.time + delay;
		}
		else
		{
			PlayTweeners(tweenGroup, duration);
		}
		if (this.Played != null)
		{
			this.Played();
		}
	}

	private void PlayTweeners(int tweenGroup = 0, float duration = 0f)
	{
		_reservedAt = 0f;
		for (int i = 0; i < _cachedTweeners.Count; i++)
		{
			UITweener uITweener = _cachedTweeners[i];
			if (uITweener.tweenGroup == tweenGroup)
			{
				if (duration > 0f)
				{
					uITweener.duration = duration;
				}
				if (Application.isPlaying)
				{
					uITweener.Play(_isForward);
					continue;
				}
				uITweener.Play(_isForward);
				uITweener.ResetToBeginning();
				uITweener.enabled = true;
				EditorUpdateLoop.Play(uITweener, OnFinishTweener);
			}
		}
		_isPlaying = true;
	}

	[ExposedInEditor("Play")]
	private void EditorPlay()
	{
		_isInitTweeners = false;
		Play();
	}

	[ExposedInEditor(null)]
	public void Stop()
	{
		_isPlaying = false;
		_reservedAt = 0f;
		int i = 0;
		for (int size = KUtility.GetSize(_cachedTweeners); i < size; i++)
		{
			UITweener uITweener = _cachedTweeners[i];
			if (uITweener != null)
			{
				uITweener.enabled = false;
			}
		}
	}

	public List<UITweener> GetTweeners()
	{
		InitTweeners();
		return _cachedTweeners;
	}

	private void OnFinishTweener()
	{
		if (PlayingTweenerCount() > 0)
		{
			return;
		}
		if (_isPlaying && _loop)
		{
			if (NGUITools.GetActive(this))
			{
				Play();
			}
		}
		else
		{
			OnFinish();
		}
	}

	private void OnFinish()
	{
		Stop();
		if (_onAllTweenerFinished != null)
		{
			_onAllTweenerFinished();
			_onAllTweenerFinished = null;
		}
		if (_deactiveWhenFinish && Application.isPlaying)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private int PlayingTweenerCount()
	{
		if (!_isInitTweeners)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < _cachedTweeners.Count; i++)
		{
			if (NGUITools.GetActive(_cachedTweeners[i]))
			{
				num++;
			}
		}
		return num;
	}
}
