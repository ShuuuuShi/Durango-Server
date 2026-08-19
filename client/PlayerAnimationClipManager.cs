using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Durango.Player.Animation;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerAnimationClipManager : Singleton<PlayerAnimationClipManager>
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct PlayerAnimationClipTagComparer : IEqualityComparer<PlayerAnimationClipTag>
	{
		public bool Equals(PlayerAnimationClipTag x, PlayerAnimationClipTag y)
		{
			return x == y;
		}

		public int GetHashCode(PlayerAnimationClipTag x)
		{
			return (int)x;
		}
	}

	public const float DefaultTransitionTime = 0.1f;

	public const string StatePath = "player_animation_state";

	public const string ClipPath = "player_animation_clips";

	public const string TagLevelPath = "player_animation_tag_level";

	private List<PlayerAnimationStateInfo> _animStates;

	private List<PlayerAnimationClipInfo> _animClips;

	private Dictionary<string, PlayerAnimationClipInfo> _animClipsDict;

	private Dictionary<PlayerAnimationClipTag, int> _tagLevel;

	private List<PlayerAnimationStateInfo> States
	{
		get
		{
			if (_animStates == null)
			{
				Reload();
			}
			return _animStates;
		}
	}

	private List<PlayerAnimationClipInfo> Clips
	{
		get
		{
			if (_animClips == null)
			{
				Reload();
			}
			return _animClips;
		}
	}

	private Dictionary<string, PlayerAnimationClipInfo> ClipsDict
	{
		get
		{
			if (_animClipsDict == null)
			{
				Reload();
			}
			return _animClipsDict;
		}
	}

	private Dictionary<PlayerAnimationClipTag, int> TagLevel
	{
		get
		{
			if (_tagLevel == null)
			{
				Reload();
			}
			return _tagLevel;
		}
	}

	public List<PlayerAnimationStateInfo> ReadStateJson()
	{
		List<PlayerAnimationStateInfo> list = Json.ReadFromFile<List<PlayerAnimationStateInfo>>("player_animation_state");
		if (list == null)
		{
			list = new List<PlayerAnimationStateInfo>();
		}
		RemoveNullorEmpty(list);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[i].Init();
		}
		return list;
	}

	public List<PlayerAnimationClipInfo> ReadClipJson()
	{
		List<PlayerAnimationClipInfo> list = Json.ReadFromFile<List<PlayerAnimationClipInfo>>("player_animation_clips");
		if (list == null)
		{
			list = new List<PlayerAnimationClipInfo>();
		}
		RemoveNullorEmpty(list);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[i].Init();
		}
		return list;
	}

	public Dictionary<PlayerAnimationClipTag, int> ReadTagLevelJson()
	{
		Dictionary<string, int> dictionary = Json.ReadFromFile<Dictionary<string, int>>("player_animation_tag_level");
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, int>();
		}
		Dictionary<PlayerAnimationClipTag, int> dictionary2 = new Dictionary<PlayerAnimationClipTag, int>(default(PlayerAnimationClipTagComparer));
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			try
			{
				dictionary2.Add((PlayerAnimationClipTag)Enum.Parse(typeof(PlayerAnimationClipTag), item.Key), item.Value);
			}
			catch (ArgumentException)
			{
			}
		}
		return dictionary2;
	}

	public static bool IsValid(PlayerAnimationClipInfoBase obj)
	{
		return obj != null && !string.IsNullOrEmpty(obj.Clip);
	}

	public static void RemoveNullorEmpty(List<PlayerAnimationClipInfo> list)
	{
		int size = KUtility.GetSize(list);
		for (int num = size - 1; num >= 0; num--)
		{
			if (!IsValid(list[num]))
			{
				list.RemoveAt(num);
			}
		}
	}

	public static void RemoveNullorEmpty(List<PlayerAnimationStateClip> list)
	{
		int size = KUtility.GetSize(list);
		for (int num = size - 1; num >= 0; num--)
		{
			RemoveNullorEmpty(list[num].Transitions);
			RemoveNullorEmpty(list[num].Conditions);
			if (!IsValid(list[num]))
			{
				list.RemoveAt(num);
			}
		}
	}

	public static bool IsValid(PlayerAnimationStateInfo obj)
	{
		if (obj == null || string.IsNullOrEmpty(obj.State))
		{
			return false;
		}
		return KUtility.GetSize(obj.Clips) > 0;
	}

	public static void RemoveNullorEmpty(List<PlayerAnimationStateInfo> list)
	{
		int size = KUtility.GetSize(list);
		for (int num = size - 1; num >= 0; num--)
		{
			RemoveNullorEmpty(list[num].Clips);
			if (!IsValid(list[num]))
			{
				list.RemoveAt(num);
			}
		}
	}

	public static bool IsValid(PlayerAnimationCondition obj)
	{
		return obj != null;
	}

	public static void RemoveNullorEmpty(List<PlayerAnimationCondition> list)
	{
		int size = KUtility.GetSize(list);
		for (int num = size - 1; num >= 0; num--)
		{
			if (!IsValid(list[num]))
			{
				list.RemoveAt(num);
			}
		}
	}

	public static bool IsValid(PlayerAnimationClipTrasitionInfo obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(obj.Clip) && string.IsNullOrEmpty(obj.State))
		{
			return false;
		}
		return KUtility.GetSize(obj.Conditions) > 0;
	}

	public static void RemoveNullorEmpty(List<PlayerAnimationClipTrasitionInfo> list)
	{
		int size = KUtility.GetSize(list);
		for (int num = size - 1; num >= 0; num--)
		{
			RemoveNullorEmpty(list[num].Conditions);
			if (!IsValid(list[num]))
			{
				list.RemoveAt(num);
			}
		}
	}

	public void Reload()
	{
		_animStates = ReadStateJson();
		_animClips = ReadClipJson();
		_tagLevel = ReadTagLevelJson();
		_animClipsDict = new Dictionary<string, PlayerAnimationClipInfo>();
		for (int i = 0; i < _animClips.Count; i++)
		{
			string clip = _animClips[i].Clip;
			if (!_animClipsDict.ContainsKey(clip))
			{
				_animClipsDict.Add(clip, _animClips[i]);
			}
		}
	}

	public PlayerAnimationStateInfo GetPlayerAnimationStateInfo(string state)
	{
		if (string.IsNullOrEmpty(state))
		{
			return null;
		}
		int count = States.Count;
		for (int i = 0; i < count; i++)
		{
			if (States[i].State == state)
			{
				return States[i];
			}
		}
		return null;
	}

	[CanBeNull]
	public PlayerAnimationStateClip GetPlayerAnimationStateClipInfo(string key, string state)
	{
		PlayerAnimationStateInfo playerAnimationStateInfo = GetPlayerAnimationStateInfo(state);
		if (playerAnimationStateInfo == null)
		{
			return null;
		}
		if (playerAnimationStateInfo.Clips == null)
		{
			return null;
		}
		return playerAnimationStateInfo.Clips.FirstOrDefault((PlayerAnimationStateClip c) => c.Clip == key);
	}

	[CanBeNull]
	public PlayerAnimationClipInfo GetPlayerAnimationClipInfo(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		if (ClipsDict.ContainsKey(key))
		{
			return ClipsDict[key];
		}
		return Clips.FirstOrDefault((PlayerAnimationClipInfo t) => t.Clip == key);
	}

	[CanBeNull]
	public string GetPlayerAnimationClip(string stateName, int framework)
	{
		return (GetPlayerAnimationStateInfo(stateName)?.Get(new PlayerAnimationConditionArguments
		{
			Framework = framework
		}))?.Clip;
	}

	public static PlayerAnimationClipTrasitionInfo GetTransitionCondition(List<PlayerAnimationClipTrasitionInfo> transitions, TransitionCondition type)
	{
		if (transitions == null)
		{
			return null;
		}
		for (int i = 0; i < transitions.Count; i++)
		{
			if (transitions[i].Conditions != null && transitions[i].Conditions.Count > 0)
			{
				PlayerAnimationCondition playerAnimationCondition = transitions[i].Conditions[0];
				TransitionCondition conditionType = (TransitionCondition)playerAnimationCondition.GetConditionType();
				if (conditionType == type)
				{
					return transitions[i];
				}
			}
		}
		return null;
	}

	public int GetTagLevel(PlayerAnimationClipInfo clip)
	{
		if (clip == null)
		{
			return 0;
		}
		int num = 0;
		PlayerAnimationClipTag[] array = Enums<PlayerAnimationClipTag>.All();
		for (int i = 0; i < array.Length; i++)
		{
			if ((clip.Tag & array[i]) != 0)
			{
				num = Mathf.Max(num, TagLevel.Get(array[i], 0));
			}
		}
		return num;
	}
}
