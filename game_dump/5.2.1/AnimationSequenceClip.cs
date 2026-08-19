using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AnimationSequenceClip
{
	public struct Enumerator : IEnumerator<AnimationSequenceClip>, IDisposable, IEnumerator
	{
		private readonly IClipEnumerator _parent;

		private int _index;

		private AnimationSequenceClip _current;

		object IEnumerator.Current => Current;

		public AnimationSequenceClip Current => _current;

		public Enumerator(IClipEnumerator parent)
		{
			_parent = parent;
			_index = 0;
			_current = default(AnimationSequenceClip);
		}

		public bool MoveNext()
		{
			if (_parent.TryMoveNext(_index, out var clip))
			{
				_index++;
				_current = clip;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			_index = 0;
		}

		public void Dispose()
		{
		}
	}

	public float? Duration;

	public string Clip;

	public AnimationSequenceClip(string clip)
	{
		Clip = clip;
		Duration = null;
	}

	public AnimationSequenceClip(string clip, float duration)
	{
		Clip = clip;
		Duration = duration;
	}

	public AnimationSequenceClip(AnimationClip clip)
	{
		Clip = ((!(clip == null)) ? clip.name : null);
		Duration = null;
	}

	public AnimationSequenceClip(AnimationClip clip, float duration)
	{
		Clip = ((!(clip == null)) ? clip.name : null);
		Duration = duration;
	}
}
