using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class AnimationEventData : KSingleton<AnimationEventData>
{
	public const float FramesPerSecond = 30f;

	private readonly Dictionary<string, Dictionary<string, List<AnimationEventInfo>>> _objectsAnimEvents = new Dictionary<string, Dictionary<string, List<AnimationEventInfo>>>();

	public void Remove([CanBeNull] AnimationEventResource animationEventFile, [CanBeNull] AnimationEventResource animationEventFileShared)
	{
		if (!((Object)(object)animationEventFile == (Object)null))
		{
			string key = FindAnimEventFileKey(animationEventFile, animationEventFileShared);
			if (_objectsAnimEvents.ContainsKey(key))
			{
				_objectsAnimEvents.Remove(key);
			}
		}
	}

	private string FindAnimEventFileKey([NotNull] AnimationEventResource animationEventFile, [CanBeNull] AnimationEventResource animationEventFileShared)
	{
		string text = ((Object)animationEventFile).name;
		if ((Object)(object)animationEventFileShared != (Object)null)
		{
			text = text + "+" + ((Object)animationEventFileShared).name;
		}
		return text;
	}

	public Dictionary<string, List<AnimationEventInfo>> LoadAnimationEvent([CanBeNull] AnimationEventResource animationEventFile, [CanBeNull] AnimationEventResource animationEventFileShared)
	{
		if ((Object)(object)animationEventFile == (Object)null)
		{
			return null;
		}
		string key = FindAnimEventFileKey(animationEventFile, animationEventFileShared);
		if (_objectsAnimEvents.ContainsKey(key))
		{
			return _objectsAnimEvents[key];
		}
		Dictionary<string, List<AnimationEventInfo>> dictionary = animationEventFile.ToDictionary();
		if ((Object)(object)animationEventFileShared != (Object)null)
		{
			MergeAnimationEvents(dictionary, animationEventFileShared);
		}
		PostLoadEvent(dictionary);
		_objectsAnimEvents.Add(key, dictionary);
		return dictionary;
	}

	private static void MergeAnimationEvents([NotNull] Dictionary<string, List<AnimationEventInfo>> dest, [NotNull] AnimationEventResource sharedEvents)
	{
		int num = sharedEvents.AnimationEventPairs.Length;
		for (int i = 0; i < num; i++)
		{
			AnimationEventResource.AnimationEventPair animationEventPair = sharedEvents.AnimationEventPairs[i];
			string name = animationEventPair.Name;
			if (!dest.TryGetValue(name, out var value))
			{
				value = new List<AnimationEventInfo>();
				dest.Add(name, value);
			}
			AnimationEventInfo[] infos = animationEventPair.Infos;
			for (int j = 0; j < infos.Length; j++)
			{
				infos[j].shared = true;
				value.Add(infos[j]);
			}
		}
		Sort(dest);
	}

	private static void Sort([NotNull] Dictionary<string, List<AnimationEventInfo>> animationEvents)
	{
		int count = animationEvents.Count;
		for (int i = 0; i < count; i++)
		{
			List<AnimationEventInfo> list = animationEvents.Values.ElementAt(i);
			list.Sort();
		}
	}

	private static void PostLoadEvent(Dictionary<string, List<AnimationEventInfo>> animationEvents)
	{
		Dictionary<string, List<AnimationEventInfo>>.Enumerator enumerator = animationEvents.GetEnumerator();
		while (enumerator.MoveNext())
		{
			List<AnimationEventInfo> value = enumerator.Current.Value;
			int count = value.Count;
			for (int i = 0; i < count; i++)
			{
				AnimationEventInfo animationEventInfo = value[i];
				switch (animationEventInfo.animEventCmd)
				{
				case AnimEventCmd.Particle:
					ParticleManager.Cache(animationEventInfo.gameObjectPath);
					break;
				case AnimEventCmd.Sound:
					SoundManager.Cache(animationEventInfo.gameObjectPath, delayedCache: true);
					break;
				case AnimEventCmd.IntegratedEffect:
					IntegratedEffect.Precache(animationEventInfo.gameObjectPath);
					break;
				}
			}
		}
	}
}
