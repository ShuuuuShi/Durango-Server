using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Weighted3StateMotion : IClipCollectable, IClipEnumerator
{
	[SerializeField]
	public float weight = 1f;

	[SerializeField]
	public string begin;

	[SerializeField]
	public List<WeightedMotion> loopings = new List<WeightedMotion>
	{
		new WeightedMotion()
	};

	[SerializeField]
	public string end;

	[SerializeField]
	private AnimationClip _clipObjBegin;

	[SerializeField]
	private AnimationClip _clipObjEnd;

	public AnimationClip ClipBegin
	{
		get
		{
			return _clipObjBegin;
		}
		set
		{
			_clipObjBegin = value;
			begin = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public AnimationClip ClipEnd
	{
		get
		{
			return _clipObjEnd;
		}
		set
		{
			_clipObjEnd = value;
			end = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public void CollectClips(List<AnimationClip> clips)
	{
		clips.Add(ClipBegin);
		foreach (WeightedMotion looping in loopings)
		{
			clips.Add(looping.Clip);
		}
		clips.Add(ClipEnd);
	}

	public bool TryMoveNext(int index, out AnimationSequenceClip res)
	{
		switch (index)
		{
		case 0:
			res = new AnimationSequenceClip(ClipBegin);
			return true;
		case 1:
		{
			WeightedMotion motion = WeightedMotion.GetMotion(loopings);
			res = new AnimationSequenceClip(motion.Clip, Mathf.Lerp(motion.duration.x, motion.duration.y, UnityEngine.Random.value));
			return true;
		}
		case 2:
			res = new AnimationSequenceClip(ClipEnd);
			return true;
		default:
			res = default(AnimationSequenceClip);
			return false;
		}
	}
}
