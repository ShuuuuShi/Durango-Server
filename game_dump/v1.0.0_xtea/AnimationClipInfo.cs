using UnityEngine;

public struct AnimationClipInfo
{
	public string Name;

	public float AnimTime;

	public float Length;

	public bool IsLoop;

	public float PlaybackRate;

	public AnimationClip Clip;

	public float OriginalTime
	{
		get
		{
			if (PlaybackRate == 0f)
			{
				return AnimTime;
			}
			return AnimTime / PlaybackRate;
		}
	}
}
