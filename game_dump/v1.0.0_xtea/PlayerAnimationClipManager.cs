using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PlayerAnimationClipManager : KSingleton<PlayerAnimationClipManager>
{
	public const float DefaultTransitionTime = 0.1f;

	public TextAsset stateJson;

	public TextAsset clipJson;

	public TextAsset tagLevelJson;

	public TextAsset blendTreeJson;

	private List<PlayerAnimationStateInfo> _animStates;

	private List<PlayerAnimationClipInfo> _animClips;

	private Dictionary<PlayerAnimationClipTag, int> _tagLevel;

	private List<PlayerAnimationBlendTree> _blendTrees;

	public List<PlayerAnimationStateInfo> States
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

	public List<PlayerAnimationClipInfo> Clips
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

	public Dictionary<PlayerAnimationClipTag, int> TagLevel
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

	public List<PlayerAnimationBlendTree> BlendTrees
	{
		get
		{
			if (_blendTrees == null)
			{
				Reload();
			}
			return _blendTrees;
		}
	}

	public List<PlayerAnimationStateInfo> ReadStateJson()
	{
		List<PlayerAnimationStateInfo> list = KUtility.ParseJson<List<PlayerAnimationStateInfo>>(stateJson.text);
		if (list == null)
		{
			list = new List<PlayerAnimationStateInfo>();
		}
		RemoveNullofEmpty(list);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[i].Init();
		}
		return list;
	}

	public List<PlayerAnimationClipInfo> ReadClipJson()
	{
		List<PlayerAnimationClipInfo> list = KUtility.ParseJson<List<PlayerAnimationClipInfo>>(clipJson.text);
		if (list == null)
		{
			list = new List<PlayerAnimationClipInfo>();
		}
		RemoveNullofEmpty(list);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[i].Init();
		}
		return list;
	}

	public List<PlayerAnimationBlendTree> ReadBlendTreeJson()
	{
		List<PlayerAnimationBlendTree> list = KUtility.ParseJson<List<PlayerAnimationBlendTree>>(blendTreeJson.text);
		if (list == null)
		{
			list = new List<PlayerAnimationBlendTree>();
		}
		RemoveNullofEmpty(list);
		return list;
	}

	public Dictionary<PlayerAnimationClipTag, int> ReadTagLevelJson()
	{
		Dictionary<string, int> dictionary = KUtility.ParseJson<Dictionary<string, int>>(tagLevelJson.text);
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, int>();
		}
		Dictionary<PlayerAnimationClipTag, int> dictionary2 = new Dictionary<PlayerAnimationClipTag, int>();
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			try
			{
				dictionary2.Add((PlayerAnimationClipTag)(int)Enum.Parse(typeof(PlayerAnimationClipTag), item.Key), item.Value);
			}
			catch (ArgumentException)
			{
			}
		}
		return dictionary2;
	}

	public static bool IsValid(PlayerAnimationClipInfo obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(obj.Clip))
		{
			return false;
		}
		return true;
	}

	public static void RemoveNullofEmpty(List<PlayerAnimationClipInfo> list)
	{
		int num = list?.Count ?? 0;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			RemoveNullofEmpty(list[num2].Transitions);
			if (!IsValid(list[num2]))
			{
				list.RemoveAt(num2);
			}
		}
	}

	public static void RemoveNullofEmpty(List<PlayerAnimationStateClip> list)
	{
		int num = list?.Count ?? 0;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			RemoveNullofEmpty(list[num2].Conditions);
			if (!IsValid(list[num2]))
			{
				list.RemoveAt(num2);
			}
		}
	}

	public static bool IsValid(PlayerAnimationStateInfo obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(obj.State))
		{
			return false;
		}
		if (obj.Clips == null || obj.Clips.Count == 0)
		{
			return false;
		}
		return true;
	}

	public static void RemoveNullofEmpty(List<PlayerAnimationStateInfo> list)
	{
		int num = list?.Count ?? 0;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			RemoveNullofEmpty(list[num2].Clips);
			RemoveNullofEmpty(list[num2].StateTransitions);
			if (!IsValid(list[num2]))
			{
				list.RemoveAt(num2);
			}
		}
	}

	public static bool IsValid(PlayerAnimationBlendTree obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(obj.Name))
		{
			return false;
		}
		if (obj.Clips == null || obj.Clips.Count == 0)
		{
			return false;
		}
		if (string.IsNullOrEmpty(obj.Parameter))
		{
			return false;
		}
		return true;
	}

	public static void RemoveNullofEmpty(List<PlayerAnimationBlendTree> list)
	{
		int num = list?.Count ?? 0;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			RemoveNullofEmpty(list[num2].Clips);
			if (!IsValid(list[num2]))
			{
				list.RemoveAt(num2);
			}
		}
	}

	public static bool IsValid(PlayerAnimationBlendTreeNode obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(obj.Clip))
		{
			return false;
		}
		return true;
	}

	public static void RemoveNullofEmpty(List<PlayerAnimationBlendTreeNode> list)
	{
		int num = list?.Count ?? 0;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			if (!IsValid(list[num2]))
			{
				list.RemoveAt(num2);
			}
		}
	}

	public static bool IsValid(PlayerAnimationCondition obj)
	{
		if (obj == null)
		{
			return false;
		}
		return true;
	}

	public static void RemoveNullofEmpty(List<PlayerAnimationCondition> list)
	{
		int num = list?.Count ?? 0;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			if (!IsValid(list[num2]))
			{
				list.RemoveAt(num2);
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
		if (obj.Conditions == null || obj.Conditions.Count == 0)
		{
			return false;
		}
		return true;
	}

	public static void RemoveNullofEmpty(List<PlayerAnimationClipTrasitionInfo> list)
	{
		int num = list?.Count ?? 0;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			RemoveNullofEmpty(list[num2].Conditions);
			if (!IsValid(list[num2]))
			{
				list.RemoveAt(num2);
			}
		}
	}

	public void Reload()
	{
		_animStates = ReadStateJson();
		_animClips = ReadClipJson();
		_tagLevel = ReadTagLevelJson();
		_blendTrees = ReadBlendTreeJson();
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

	public PlayerAnimationClipInfo GetPlayerAnimationClipInfo(string key, string state)
	{
		PlayerAnimationStateInfo playerAnimationStateInfo = GetPlayerAnimationStateInfo(state);
		if (playerAnimationStateInfo == null)
		{
			int count = Clips.Count;
			for (int i = 0; i < count; i++)
			{
				if (Clips[i].Clip == key)
				{
					return Clips[i];
				}
			}
		}
		else
		{
			int num = ((playerAnimationStateInfo.Clips != null) ? playerAnimationStateInfo.Clips.Count : 0);
			for (int j = 0; j < num; j++)
			{
				if (playerAnimationStateInfo.Clips[j].Clip == key)
				{
					return playerAnimationStateInfo.Clips[j];
				}
			}
		}
		return null;
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

	public static PlayerAnimationBlendTree GetBlendTree(string key)
	{
		List<PlayerAnimationBlendTree> blendTrees = KSingleton<PlayerAnimationClipManager>.Instance().BlendTrees;
		for (int i = 0; i < blendTrees.Count; i++)
		{
			if (blendTrees[i].Name == key)
			{
				return blendTrees[i];
			}
		}
		return null;
	}

	public bool CheckClip(PlayerAnimationClipInfo clip, string value)
	{
		if (string.IsNullOrEmpty(value) || clip == null)
		{
			return false;
		}
		PlayerAnimationStateClip playerAnimationStateClip = clip as PlayerAnimationStateClip;
		string[] array = value.Split(' ');
		if (array.Length == 0)
		{
			return false;
		}
		if (array.Length >= 2)
		{
			if (playerAnimationStateClip == null)
			{
				return false;
			}
			return playerAnimationStateClip.GetParent().State == array[0] && clip.Clip == array[1];
		}
		PlayerAnimationStateInfo playerAnimationStateInfo = GetPlayerAnimationStateInfo(value);
		if (playerAnimationStateInfo != null && playerAnimationStateClip != null)
		{
			return playerAnimationStateClip.GetParent() == playerAnimationStateInfo;
		}
		return clip.Clip == value;
	}

	public static bool IsClipInState(PlayerAnimationClipInfo clip, string state)
	{
		if (clip == null)
		{
			return false;
		}
		PlayerAnimationStateInfo playerAnimationStateInfo = KSingleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationStateInfo(state);
		if (playerAnimationStateInfo == null || playerAnimationStateInfo.Clips == null)
		{
			return false;
		}
		for (int i = 0; i < playerAnimationStateInfo.Clips.Count; i++)
		{
			if (playerAnimationStateInfo.Clips[i].Clip == clip.Clip)
			{
				return true;
			}
		}
		return false;
	}

	public static PlayerAnimationStateInfo GetClipState(PlayerAnimationClipInfo clip)
	{
		return (clip is PlayerAnimationStateClip playerAnimationStateClip) ? playerAnimationStateClip.GetParent() : null;
	}

	private static bool GetParameterValue(object obj, string parameter, out float value)
	{
		Type type = obj.GetType();
		MemberInfo[] member = type.GetMember(parameter, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (member.Length == 0)
		{
			value = 0f;
			return false;
		}
		object obj2 = null;
		for (int i = 0; i < member.Length; i++)
		{
			if (member[0] is FieldInfo fieldInfo)
			{
				obj2 = fieldInfo.GetValue(obj);
			}
			else if (member[0] is PropertyInfo propertyInfo)
			{
				obj2 = propertyInfo.GetValue(obj, null);
			}
		}
		if (obj2 == null || !(obj2 is float))
		{
			value = 0f;
			return false;
		}
		value = (float)obj2;
		return true;
	}

	public static void CalcBlendTreeClipWeight(PlayerAnimationBlendTree tree, object obj)
	{
		if (obj == null || tree == null || tree.Clips == null || !GetParameterValue(obj, tree.Parameter, out var value))
		{
			return;
		}
		float paramMin = tree.ParamMin;
		float paramMax = tree.ParamMax;
		for (int i = 0; i < tree.Clips.Count; i++)
		{
			PlayerAnimationBlendTreeNode playerAnimationBlendTreeNode = tree.Clips[i];
			float min = playerAnimationBlendTreeNode.Min;
			float max = playerAnimationBlendTreeNode.Max;
			float num = 0f;
			float num2 = value;
			if (max > paramMax && num2 < playerAnimationBlendTreeNode.Param)
			{
				num2 += paramMax - paramMin;
			}
			num = Mathf.Max(num, NormalizeRatio(num2, max, playerAnimationBlendTreeNode.Param));
			num2 = value;
			if (min < paramMin && num2 > playerAnimationBlendTreeNode.Param)
			{
				num2 -= paramMax - paramMin;
			}
			num = Mathf.Max(num, NormalizeRatio(num2, min, playerAnimationBlendTreeNode.Param));
			playerAnimationBlendTreeNode.Weight = num;
		}
	}

	public static float NormalizeRatio(float value, float min, float max)
	{
		if (min > max)
		{
			max = min + (min - max);
			value = min + (min - value);
		}
		if (value < min || value > max)
		{
			return 0f;
		}
		return (value - min) * (1f / (max - min));
	}

	public int GetTagLevel(PlayerAnimationClipInfo clip)
	{
		if (clip == null)
		{
			return 0;
		}
		int num = 0;
		PlayerAnimationClipTag[] array = (PlayerAnimationClipTag[])Enum.GetValues(typeof(PlayerAnimationClipTag));
		for (int i = 0; i < array.Length; i++)
		{
			if ((clip.Tag & array[i]) != 0)
			{
				num = Mathf.Max(num, GetTagLevel(array[i]));
			}
		}
		return num;
	}

	private int GetTagLevel(PlayerAnimationClipTag clipTag)
	{
		TagLevel.TryGetValue(clipTag, out var value);
		return value;
	}
}
