using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationElemAttack : AnimationElemBase
{
	[SerializeField]
	public ActionMeta meta = new ActionMeta();

	[SerializeField]
	public List<AttackInfo> attack_info = new List<AttackInfo>();

	[SerializeField]
	public bool SameAbove;

	public string OriginalElemKey { get; set; }

	public override void CollectClips(List<AnimationClip> clips)
	{
		meta.CollectClips(clips);
	}

	public override void CreateNew(string frameworkName)
	{
		attack_info.Clear();
		attack_info.Add(new AttackInfo
		{
			frame = 0,
			sub_action_id = key + "_0"
		});
	}

	public override bool TryMoveNext(int index, out AnimationSequenceClip res)
	{
		if (index == 0)
		{
			res = new AnimationSequenceClip(meta.Clip);
			return true;
		}
		return base.TryMoveNext(index, out res);
	}

	public override void AutoFill(List<string> animFbxFiles)
	{
		meta.AutoFill(key, animFbxFiles);
	}

	public void CopyFrom(AnimationElemAttack obj)
	{
		meta = obj.meta.Clone();
		attack_info.Clear();
		foreach (AttackInfo item in obj.attack_info)
		{
			attack_info.Add(item.Clone());
		}
	}
}
