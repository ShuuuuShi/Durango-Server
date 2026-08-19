using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActionMeta
{
	[SerializeField]
	public string motion;

	[SerializeField]
	public bool bound_enemy;

	[SerializeField]
	public float rot_speed;

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

	public void CollectClips(List<AnimationClip> clips)
	{
		clips.Add(Clip);
	}

	public void AutoFill(string key, List<string> animFbxFiles)
	{
		if (!(Clip != null))
		{
			Clip = AnimalFrameworkUtils.AutoFillInternal(key, string.Empty, animFbxFiles);
		}
	}

	public ActionMeta Clone()
	{
		return (ActionMeta)MemberwiseClone();
	}
}
