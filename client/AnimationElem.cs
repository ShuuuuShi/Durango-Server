using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationElem : AnimationElemBase
{
	[SerializeField]
	public string motion;

	[SerializeField]
	private AnimationClip _clipObj;

	public AnimationClip Clip
	{
		get
		{
			return _clipObj;
		}
		set
		{
			_clipObj = value;
			motion = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public override void CollectClips(List<AnimationClip> clips)
	{
		clips.Add(Clip);
	}

	public override void CreateNew(string frameworkName)
	{
	}

	public override bool TryMoveNext(int index, out AnimationSequenceClip res)
	{
		if (index == 0)
		{
			res = new AnimationSequenceClip(_clipObj);
			return true;
		}
		return base.TryMoveNext(index, out res);
	}

	public override void AutoFill(List<string> animFbxFiles)
	{
		if (!(Clip != null))
		{
			Clip = AnimalFrameworkUtils.AutoFillInternal(key, string.Empty, animFbxFiles);
		}
	}
}
