using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBlendingController : MonoBehaviour
{
	public class BlendingInfo
	{
		public float FadeOutTime { get; set; }

		public float FadeInTime { get; set; }

		public BlendingInfo()
		{
			FadeInTime = 0.3f;
			FadeOutTime = 0.3f;
		}
	}

	public const float DefaultMotionFadeTime = 0.3f;

	[SerializeField]
	private TextAsset _clipJson;

	public TextAsset ClipJson => _clipJson;

	public Dictionary<string, BlendingInfo> AnimClips { get; private set; }

	public bool ReadClipJson()
	{
		if (AnimClips != null)
		{
			return true;
		}
		if ((Object)(object)ClipJson == (Object)null)
		{
			return false;
		}
		AnimClips = KUtility.ParseJson<Dictionary<string, BlendingInfo>>(ClipJson.text);
		return true;
	}

	public void ClearClipList()
	{
		AnimClips = null;
	}

	public float GetFadeTime(string fadeInClip, string fadeOutClip)
	{
		if (string.IsNullOrEmpty(fadeInClip) || string.IsNullOrEmpty(fadeOutClip))
		{
			return 0.3f;
		}
		if (AnimClips == null || !AnimClips.ContainsKey(fadeInClip) || !AnimClips.ContainsKey(fadeOutClip))
		{
			return 0.3f;
		}
		float fadeOutTime = AnimClips[fadeOutClip].FadeOutTime;
		float fadeInTime = AnimClips[fadeInClip].FadeInTime;
		return (!(fadeInTime <= -1f)) ? Math.Max(fadeOutTime, fadeInTime) : 0f;
	}
}
