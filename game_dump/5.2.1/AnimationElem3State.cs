using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationElem3State : AnimationElemBase
{
	[SerializeField]
	public string begin;

	[SerializeField]
	public string during;

	[SerializeField]
	public string end;

	[SerializeField]
	private AnimationClip _clipObjBegin;

	[SerializeField]
	private AnimationClip _clipObjLooping;

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

	public AnimationClip ClipLooping
	{
		get
		{
			return _clipObjLooping;
		}
		set
		{
			_clipObjLooping = value;
			during = ((!(value != null)) ? string.Empty : value.name);
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

	public override void CollectClips(List<AnimationClip> clips)
	{
		clips.Add(ClipBegin);
		clips.Add(ClipLooping);
		clips.Add(ClipEnd);
	}

	public override void CreateNew(string frameworkName)
	{
	}

	public override bool TryMoveNext(int index, out AnimationSequenceClip res)
	{
		switch (index)
		{
		case 0:
			res = new AnimationSequenceClip(ClipBegin);
			return true;
		case 1:
			res = new AnimationSequenceClip(ClipLooping);
			return true;
		case 2:
			res = new AnimationSequenceClip(ClipEnd);
			return true;
		default:
			return base.TryMoveNext(index, out res);
		}
	}

	public override void AutoFill(List<string> animFbxFiles)
	{
		ClipBegin = AnimalFrameworkUtils.AutoFillInternal(key, "_begin", animFbxFiles);
		ClipLooping = AnimalFrameworkUtils.AutoFillInternal(key, "_looping", animFbxFiles);
		ClipEnd = AnimalFrameworkUtils.AutoFillInternal(key, "_end", animFbxFiles);
	}
}
