using System.Collections.Generic;
using System.IO;
using Durango.Render.Effect;
using Durango.Render.Particle;
using JetBrains.Annotations;
using UnityEngine;

public static class AnimationEventContainer
{
	public const float FramesPerSecond = 30f;

	private static readonly Dictionary<string, Dictionary<string, List<AnimationEventInfo>>> EventsDict;

	static AnimationEventContainer()
	{
		EventsDict = new Dictionary<string, Dictionary<string, List<AnimationEventInfo>>>();
		GameManager.Reset += delegate
		{
			EventsDict.Clear();
		};
	}

	public static void Remove([CanBeNull] AnimationEventResource animationEventFile, [CanBeNull] AnimationEventResource animationEventFileShared)
	{
		if (!(animationEventFile == null))
		{
			string key = FindAnimEventFileKey(animationEventFile, animationEventFileShared);
			if (EventsDict.ContainsKey(key))
			{
				EventsDict.Remove(key);
			}
		}
	}

	private static string FindAnimEventFileKey([NotNull] AnimationEventResource animationEventFile, [CanBeNull] AnimationEventResource animationEventFileShared)
	{
		string text = animationEventFile.name;
		if (animationEventFileShared != null)
		{
			text = text + "+" + animationEventFileShared.name;
		}
		return text;
	}

	public static Dictionary<string, List<AnimationEventInfo>> LoadAnimationEvent([CanBeNull] AnimationEventResource resource, [CanBeNull] AnimationEventResource animationEventFileShared)
	{
		if (resource == null)
		{
			return null;
		}
		string key = FindAnimEventFileKey(resource, animationEventFileShared);
		Dictionary<string, List<AnimationEventInfo>> dictionary = EventsDict.Get(key);
		if (dictionary != null)
		{
			return dictionary;
		}
		dictionary = resource.ToDictionary();
		if (animationEventFileShared != null)
		{
			MergeAnimationEvents(dictionary, animationEventFileShared);
		}
		PostLoadEvent(dictionary);
		EventsDict.Add(key, dictionary);
		return dictionary;
	}

	private static void MergeAnimationEvents([NotNull] Dictionary<string, List<AnimationEventInfo>> dest, [NotNull] AnimationEventResource sharedEvents)
	{
		int num = sharedEvents.AnimationEventPairs.Length;
		for (int i = 0; i < num; i++)
		{
			AnimationEventResource.AnimationEventPair obj = sharedEvents.AnimationEventPairs[i];
			string name = obj.Name;
			if (!dest.TryGetValue(name, out var value))
			{
				value = new List<AnimationEventInfo>();
				dest.Add(name, value);
			}
			AnimationEventInfo[] infos = obj.Infos;
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
		foreach (KeyValuePair<string, List<AnimationEventInfo>> animationEvent in animationEvents)
		{
			animationEvent.Value.Sort();
		}
	}

	private static void PostLoadEvent(Dictionary<string, List<AnimationEventInfo>> animationEvents)
	{
		foreach (KeyValuePair<string, List<AnimationEventInfo>> animationEvent in animationEvents)
		{
			List<AnimationEventInfo> value = animationEvent.Value;
			int count = value.Count;
			for (int i = 0; i < count; i++)
			{
				AnimationEventInfo animationEventInfo = value[i];
				switch (animationEventInfo.animEventCmd)
				{
				case AnimEventCmd.LegacyParticle:
				case AnimEventCmd.NewParticle:
					if (Application.isPlaying)
					{
						ParticleManager.Cache(animationEventInfo.gameObjectPath);
					}
					break;
				case AnimEventCmd.Sound:
					if (Application.isPlaying && animationEventInfo.gameObjectPath != null)
					{
						SoundManager.PrepareEvent(Path.GetFileNameWithoutExtension(animationEventInfo.gameObjectPath));
					}
					break;
				case AnimEventCmd.IntegratedEffect:
					if (Application.isPlaying)
					{
						IntegratedEffect.Precache(animationEventInfo.gameObjectPath);
					}
					break;
				case AnimEventCmd.SoundEvent:
					if (Application.isPlaying)
					{
						SoundManager.PrepareEvent(animationEventInfo.gameObjectPath);
					}
					break;
				}
			}
		}
	}
}
