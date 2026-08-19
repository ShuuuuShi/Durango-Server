using System;
using System.Collections.Generic;
using Durango.Model;
using JetBrains.Annotations;
using UnityEngine;

public class AnimationSequence
{
	[CanBeNull]
	private Animation _animation;

	[CanBeNull]
	private IMotionPlayable _playable;

	private IEnumerable<AnimationSequenceClip> _enumerable;

	private IEnumerator<AnimationSequenceClip> _enumerator;

	private float? _nextAt;

	private bool _loop;

	private float _playbackRatio;

	private float? _finishAt;

	private Action _onFinished;

	public bool IsPlaying { get; private set; }

	public AnimationSequence()
	{
		Reset();
	}

	public void Set([NotNull] IMotionPlayable playable, IEnumerable<AnimationSequenceClip> enumerable, bool loop = false, float? duration = null, float playbackRatio = 1f, Action onFinished = null)
	{
		_animation = null;
		_playable = playable;
		Set(enumerable, loop, duration, playbackRatio, onFinished);
	}

	public void Set([NotNull] Animation animation, IEnumerable<AnimationSequenceClip> enumerable, bool loop = false, float? duration = null, float playbackRatio = 1f, Action onFinished = null)
	{
		_animation = animation;
		_playable = null;
		Set(enumerable, loop, duration, playbackRatio, onFinished);
	}

	private void Set(IEnumerable<AnimationSequenceClip> enumerable, bool loop, float? duration, float playbackRatio, Action onFinished)
	{
		_enumerable = enumerable;
		_nextAt = 0f;
		_loop = loop;
		_playbackRatio = playbackRatio;
		_onFinished = onFinished;
		if (!duration.HasValue)
		{
			_finishAt = null;
		}
		else
		{
			_finishAt = ((!duration.HasValue) ? null : new float?(Time.time + duration.GetValueOrDefault()));
		}
		_enumerator = enumerable?.GetEnumerator();
		IsPlaying = true;
	}

	public void Reset()
	{
		_animation = null;
		_enumerable = null;
		_enumerator = null;
		_nextAt = null;
		_loop = false;
		_playbackRatio = 1f;
		_onFinished = null;
		_finishAt = null;
		IsPlaying = false;
	}

	public void ToLast()
	{
		_nextAt = null;
		if (_enumerator == null)
		{
			return;
		}
		while (_enumerator.MoveNext())
		{
		}
		if (_animation != null)
		{
			if (_animation.Play(_enumerator.Current.Clip))
			{
				AnimationState animationState = _animation[_enumerator.Current.Clip];
				animationState.normalizedTime = 1f;
			}
		}
		else if (_playable != null)
		{
			_playable.Play(_enumerator.Current.Clip, loop: false);
			AnimationState curAnimState = _playable.GetCurAnimState();
			if (curAnimState != null)
			{
				curAnimState.normalizedTime = 1f;
			}
		}
	}

	public void Update()
	{
		if (!IsPlaying)
		{
			return;
		}
		float time = Time.time;
		if (_enumerator != null)
		{
			if (_finishAt.HasValue)
			{
				float? finishAt = _finishAt;
				if (finishAt.HasValue && finishAt.GetValueOrDefault() < time)
				{
					goto IL_0052;
				}
			}
			float? nextAt = _nextAt;
			if (!nextAt.HasValue || time + Time.deltaTime < _nextAt.Value)
			{
				return;
			}
			if (!_enumerator.MoveNext())
			{
				if (!_loop)
				{
					OnFinished();
					return;
				}
				if (_enumerable == null)
				{
					_enumerator.Reset();
				}
				else
				{
					_enumerator = _enumerable.GetEnumerator();
				}
				_enumerator.MoveNext();
			}
			AnimationSequenceClip current = _enumerator.Current;
			float? num = current.Duration;
			if (_animation != null)
			{
				_animation.CrossFade(current.Clip);
				AnimationState animationState = _animation[current.Clip];
				if (animationState != null)
				{
					animationState.speed = _playbackRatio;
					animationState.normalizedTime = 0f;
					if (!num.HasValue)
					{
						WrapMode wrapMode = animationState.wrapMode;
						if (wrapMode != WrapMode.Loop && wrapMode != WrapMode.PingPong)
						{
							num = animationState.length;
						}
					}
				}
			}
			else if (_playable != null)
			{
				WrapMode wrapMode2 = _playable.GetWrapMode(current.Clip);
				IMotionPlayable playable = _playable;
				string clip = current.Clip;
				bool loop = wrapMode2 == WrapMode.Loop;
				playable.CrossFade(clip, -1f, loop);
				AnimationState curAnimState = _playable.GetCurAnimState();
				if (curAnimState != null)
				{
					curAnimState.speed = _playbackRatio;
					curAnimState.normalizedTime = 0f;
					if (!num.HasValue && wrapMode2 != WrapMode.Loop && wrapMode2 != WrapMode.PingPong)
					{
						num = curAnimState.length;
					}
				}
			}
			if (!num.HasValue)
			{
				_nextAt = null;
			}
			else
			{
				_nextAt = time + num.Value * _playbackRatio;
			}
			return;
		}
		goto IL_0052;
		IL_0052:
		OnFinished();
	}

	private void OnFinished()
	{
		IsPlaying = false;
		if (_onFinished != null)
		{
			Action onFinished = _onFinished;
			_onFinished = null;
			onFinished();
		}
	}
}
