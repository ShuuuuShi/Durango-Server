using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBlendingInfo : ScriptableObject, ISerializationCallbackReceiver
{
	[Serializable]
	public class SaveData : Data
	{
		[SerializeField]
		public string AnimationKey;
	}

	[Serializable]
	public class Data
	{
		[SerializeField]
		public float FadeOutTime;

		[SerializeField]
		public float FadeInTime;

		public Data()
		{
			FadeOutTime = 0.3f;
			FadeInTime = 0.3f;
		}
	}

	public const float DefaultMotionFadeTime = 0.3f;

	public Dictionary<string, Data> Clips = new Dictionary<string, Data>();

	[HideInInspector]
	[SerializeField]
	private List<SaveData> _savedClips = new List<SaveData>();

	public void OnBeforeSerialize()
	{
		_savedClips.Clear();
		foreach (KeyValuePair<string, Data> clip in Clips)
		{
			_savedClips.Add(new SaveData
			{
				AnimationKey = clip.Key,
				FadeInTime = clip.Value.FadeInTime,
				FadeOutTime = clip.Value.FadeOutTime
			});
		}
	}

	public void OnAfterDeserialize()
	{
		Clips.Clear();
		bool flag = false;
		for (int i = 0; i < _savedClips.Count; i++)
		{
			string animationKey = _savedClips[i].AnimationKey;
			if (string.IsNullOrEmpty(animationKey))
			{
				if (!flag)
				{
					flag = true;
				}
			}
			else
			{
				Clips.Add(animationKey, new Data
				{
					FadeInTime = _savedClips[i].FadeInTime,
					FadeOutTime = _savedClips[i].FadeOutTime
				});
			}
		}
	}
}
