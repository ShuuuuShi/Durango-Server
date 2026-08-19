using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBlendingController : MonoBehaviour
{
	[SerializeField]
	private AnimationBlendingInfo _animationBlendingInfo;

	public AnimationBlendingInfo Info
	{
		get
		{
			return _animationBlendingInfo;
		}
		set
		{
			_animationBlendingInfo = value;
		}
	}

	public Dictionary<string, AnimationBlendingInfo.Data> Clips => (!(Info != null)) ? null : Info.Clips;

	public bool IsLoaded()
	{
		return Info != null;
	}

	public float GetFadeTime(string fadeInClip, string fadeOutClip)
	{
		if (string.IsNullOrEmpty(fadeInClip) || string.IsNullOrEmpty(fadeOutClip) || Clips == null)
		{
			return 0.3f;
		}
		AnimationBlendingInfo.Data data = Clips.Get(fadeOutClip);
		AnimationBlendingInfo.Data data2 = Clips.Get(fadeOutClip);
		float val = data?.FadeOutTime ?? 0.3f;
		float num = data2?.FadeInTime ?? 0.3f;
		return (!(num <= -1f)) ? Math.Max(val, num) : 0f;
	}
}
