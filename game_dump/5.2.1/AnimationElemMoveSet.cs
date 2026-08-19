using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationElemMoveSet : AnimationElemBase
{
	public List<MoveSet> elems = new List<MoveSet>();

	public override void CollectClips(List<AnimationClip> clips)
	{
		foreach (MoveSet elem in elems)
		{
			elem.CollectClips(clips);
		}
	}

	public override void CreateNew(string frameworkName)
	{
		elems.Clear();
		MoveSet item = new MoveSet(frameworkName + "_move_set");
		elems.Add(item);
	}

	public override bool TryMoveNext(int index, out AnimationSequenceClip res)
	{
		if (index == 0)
		{
			foreach (MoveSet elem in elems)
			{
				MoveMotionInfo moveMotion = elem.GetMoveMotion();
				if (moveMotion != null)
				{
					res = new AnimationSequenceClip(moveMotion.ClipMove);
					return true;
				}
			}
		}
		return base.TryMoveNext(index, out res);
	}

	public override void AutoFill(List<string> animFbxFiles)
	{
		foreach (MoveSet elem in elems)
		{
			elem.AutoFill(animFbxFiles);
		}
	}
}
