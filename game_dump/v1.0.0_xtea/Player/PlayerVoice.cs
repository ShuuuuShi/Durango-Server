using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player;

public class PlayerVoice
{
	public enum Type
	{
		Attack,
		Attack_L,
		Die,
		Die_S,
		Hurt,
		Moan
	}

	private const int ManVoiceTypeCount = 4;

	private const int WomanVoiceTypeCount = 4;

	public const string VoicePath = "Sound/Effect/Voice/Human";

	private static readonly int[] TypeCount = new int[6] { 3, 2, 2, 2, 2, 2 };

	private readonly Dictionary<Type, List<string>> _voices = new Dictionary<Type, List<string>>();

	private readonly PlayerBehavior _player;

	public PlayerVoice(PlayerBehavior player)
	{
		_player = player;
	}

	public void Set(bool isMale, int voiceType)
	{
		string text = ((!isMale) ? "Woman" : "Man");
		string text2 = string.Format("{0}/{1}/{1}{2:00}", "Sound/Effect/Voice/Human", text, voiceType);
		_voices.Clear();
		Array values = Enum.GetValues(typeof(Type));
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			Type type = (Type)(int)values.GetValue(i);
			int typeCount = GetTypeCount(type);
			List<string> list = new List<string>();
			for (int j = 0; j < typeCount; j++)
			{
				string text3 = $"{text2}/VO_{text}{voiceType:00}_{type}_{j + 1:00}.wav";
				list.Add(text3);
				SoundManager.Cache(text3);
			}
			_voices.Add(type, list);
		}
	}

	public static int GetTypeCount(Type type)
	{
		return TypeCount[(int)type];
	}

	public void Play(Type type)
	{
		int typeCount = GetTypeCount(type);
		int index = Random.Range(0, typeCount);
		Play(type, index);
	}

	public void Play(Type type, int index)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = _voices.Get(type);
		if (list != null && list.Count > 0)
		{
			index = Mathf.Clamp(index, 0, list.Count - 1);
			if (_player.IsLocalPlayer)
			{
				SoundManager.Play(list[index]);
			}
			else
			{
				SoundManager.Play(list[index], _player.CurrentPosition);
			}
		}
	}

	public static IList<string> GetSampleVoices(bool isMale, Type type = Type.Attack, int index = 0)
	{
		string text = ((!isMale) ? "Woman" : "Man");
		string text2 = string.Format("{0}/{1}/{1}", "Sound/Effect/Voice/Human", text);
		List<string> list = new List<string>();
		int num = ((!isMale) ? 4 : 4);
		for (int i = 0; i < num; i++)
		{
			int num2 = i + 1;
			string text3 = string.Format("{0}{1:00}/VO_{2}{1:00}_{3}_{4:00}.wav", text2, num2, text, type, index + 1);
			SoundManager.Cache(text3);
			list.Add(text3);
		}
		return list;
	}
}
