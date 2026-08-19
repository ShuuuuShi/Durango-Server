using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationElemDirectional : AnimationElemBase
{
	[SerializeField]
	public string front;

	[SerializeField]
	public string back;

	[SerializeField]
	public string left;

	[SerializeField]
	public string right;

	[SerializeField]
	private AnimationClip _clipObjFront;

	[SerializeField]
	private AnimationClip _clipObjBack;

	[SerializeField]
	private AnimationClip _clipObjLeft;

	[SerializeField]
	private AnimationClip _clipObjRight;

	public AnimationClip ClipFront
	{
		get
		{
			return _clipObjFront;
		}
		set
		{
			_clipObjFront = value;
			front = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public AnimationClip ClipBack
	{
		get
		{
			return _clipObjBack;
		}
		set
		{
			_clipObjBack = value;
			back = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public AnimationClip ClipLeft
	{
		get
		{
			return _clipObjLeft;
		}
		set
		{
			_clipObjLeft = value;
			left = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public AnimationClip ClipRight
	{
		get
		{
			return _clipObjRight;
		}
		set
		{
			_clipObjRight = value;
			right = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public override void CollectClips(List<AnimationClip> clips)
	{
		clips.Add(ClipFront);
		clips.Add(ClipBack);
		clips.Add(ClipLeft);
		clips.Add(ClipRight);
	}

	public override void CreateNew(string frameworkName)
	{
	}

	public override void AutoFill(List<string> animFbxFiles)
	{
		ClipFront = AnimalFrameworkUtils.AutoFillInternal(key, "_s", animFbxFiles);
		ClipBack = AnimalFrameworkUtils.AutoFillInternal(key, "_n", animFbxFiles);
		ClipLeft = AnimalFrameworkUtils.AutoFillInternal(key, "_e", animFbxFiles);
		ClipRight = AnimalFrameworkUtils.AutoFillInternal(key, "_w", animFbxFiles);
	}
}
