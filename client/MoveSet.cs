using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveSet
{
	[SerializeField]
	public string name = "default_move_set";

	[SerializeField]
	public List<MoveMotionInfo> motions = new List<MoveMotionInfo>
	{
		new MoveMotionInfo()
	};

	public MoveSet(string moveSetName)
	{
		name = moveSetName;
	}

	public MoveSet()
	{
	}

	public void CollectClips(List<AnimationClip> clips)
	{
		int count = motions.Count;
		for (int i = 0; i < count; i++)
		{
			motions[i].CollectClips(clips);
		}
	}

	public void AutoFill(List<string> animFbxFiles)
	{
		int count = motions.Count;
		for (int i = 0; i < count; i++)
		{
			motions[i].AutoFill(animFbxFiles);
		}
	}

	public MoveMotionInfo GetMoveMotion(float moveSpeed = float.MaxValue)
	{
		MoveMotionInfo moveMotionInfo = null;
		foreach (MoveMotionInfo motion in motions)
		{
			if (string.IsNullOrEmpty(motion.conditions.flag) && (moveMotionInfo == null || Mathf.Abs(moveSpeed - motion.base_move_speed) < Mathf.Abs(moveSpeed - moveMotionInfo.base_move_speed)))
			{
				moveMotionInfo = motion;
			}
		}
		return moveMotionInfo;
	}
}
