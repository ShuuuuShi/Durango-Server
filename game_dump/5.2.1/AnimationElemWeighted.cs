using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationElemWeighted : AnimationElemBase
{
	[SerializeField]
	public List<WeightedMotion> elems = new List<WeightedMotion>();

	public override void CollectClips(List<AnimationClip> clips)
	{
		foreach (WeightedMotion elem in elems)
		{
			clips.Add(elem.Clip);
		}
	}

	public override void CreateNew(string frameworkName)
	{
		elems.Clear();
		elems.Add(new WeightedMotion());
	}

	public override bool TryMoveNext(int index, out AnimationSequenceClip res)
	{
		if (index == 0)
		{
			WeightedMotion motion = WeightedMotion.GetMotion(elems);
			if (motion != null)
			{
				res = new AnimationSequenceClip(motion.Clip, Mathf.Lerp(motion.duration.x, motion.duration.y, UnityEngine.Random.value));
				return true;
			}
		}
		return base.TryMoveNext(index, out res);
	}

	public override void AutoFill(List<string> animFbxFiles)
	{
	}
}
