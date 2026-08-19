using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Play Idle Animations")]
public class PlayIdleAnimations : MonoBehaviour
{
	private Animation mAnim;

	private AnimationClip mIdle;

	private List<AnimationClip> mBreaks = new List<AnimationClip>();

	private float mNextBreak;

	private int mLastIndex;

	private void Start()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		mAnim = ((Component)this).GetComponentInChildren<Animation>();
		if ((Object)(object)mAnim == (Object)null)
		{
			Object.Destroy((Object)(object)this);
			return;
		}
		foreach (AnimationState item in mAnim)
		{
			AnimationState val = item;
			if (((Object)val.clip).name == "idle")
			{
				val.layer = 0;
				mIdle = val.clip;
				mAnim.Play(((Object)mIdle).name);
			}
			else if (((Object)val.clip).name.StartsWith("idle"))
			{
				val.layer = 1;
				mBreaks.Add(val.clip);
			}
		}
		if (mBreaks.Count == 0)
		{
			Object.Destroy((Object)(object)this);
		}
	}

	private void Update()
	{
		if (!(mNextBreak < Time.time))
		{
			return;
		}
		if (mBreaks.Count == 1)
		{
			AnimationClip val = mBreaks[0];
			mNextBreak = Time.time + val.length + Random.Range(5f, 15f);
			mAnim.CrossFade(((Object)val).name);
			return;
		}
		int num = Random.Range(0, mBreaks.Count - 1);
		if (mLastIndex == num)
		{
			num++;
			if (num >= mBreaks.Count)
			{
				num = 0;
			}
		}
		mLastIndex = num;
		AnimationClip val2 = mBreaks[num];
		mNextBreak = Time.time + val2.length + Random.Range(2f, 8f);
		mAnim.CrossFade(((Object)val2).name);
	}
}
