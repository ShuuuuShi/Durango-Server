using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AnimationEventResource : ScriptableObject
{
	[Serializable]
	public class AnimationEventPair
	{
		public string Name;

		public AnimationEventInfo[] Infos;
	}

	[SerializeField]
	public AnimationEventPair[] AnimationEventPairs;

	public Dictionary<string, List<AnimationEventInfo>> ToDictionary()
	{
		Dictionary<string, List<AnimationEventInfo>> dictionary = new Dictionary<string, List<AnimationEventInfo>>();
		for (int i = 0; i < AnimationEventPairs.Length; i++)
		{
			AnimationEventPair animationEventPair = AnimationEventPairs[i];
			dictionary.Add(animationEventPair.Name, new List<AnimationEventInfo>(animationEventPair.Infos));
		}
		return dictionary;
	}
}
