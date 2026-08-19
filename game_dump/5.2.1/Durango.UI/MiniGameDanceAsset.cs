using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Durango.UI;

[ResourcePath("mini_game_dance_note")]
public class MiniGameDanceAsset : ResourceSingleton<MiniGameDanceAsset>
{
	[Serializable]
	public class DanceNoteData
	{
		public enum Type
		{
			None = 0,
			Left = 1,
			Up = 2,
			Right = 4,
			Down = 8,
			Dot = 0x10
		}

		public static Type ArrowPattern = (Type)15;

		public float TimeKey;

		public float TransitionTime;

		public Type Pattern;
	}

	[HideInInspector]
	public float DefaultTransitionTime = 3f;

	[SerializeField]
	public float NormalTime = 0.22f;

	[SerializeField]
	public float GreatTime = 0.15f;

	[SerializeField]
	public float PerfectTime = 0.09f;

	[HideInInspector]
	public string FixedMusicName = string.Empty;

	[HideInInspector]
	[SerializeField]
	public List<float> Bpm = new List<float> { 126f, 185f };

	[FormerlySerializedAs("Notes")]
	[SerializeField]
	[HideInInspector]
	public List<DanceNoteData> MiniGame01Notes = new List<DanceNoteData>();

	[SerializeField]
	[HideInInspector]
	public List<DanceNoteData> MiniGame02Notes = new List<DanceNoteData>();

	public const string FirstMusicName = "minigame_01";

	public const string SecondMusicName = "minigame_02";

	public readonly string[] MusicNames = new string[2] { "minigame_01", "minigame_02" };

	public void ModifyTime(float timeOffset, string musicName)
	{
		if (string.IsNullOrEmpty(musicName))
		{
			return;
		}
		foreach (DanceNoteData item in MusicNameToList(musicName))
		{
			item.TimeKey += timeOffset;
		}
	}

	public int MusicNameToIndex(string musicName)
	{
		return Array.IndexOf(MusicNames, musicName);
	}

	public List<DanceNoteData> MusicNameToList(string musicName)
	{
		if (!(musicName == "minigame_01"))
		{
			if (musicName == "minigame_02")
			{
				return MiniGame02Notes;
			}
			throw new NotImplementedException("invalid music selected");
		}
		return MiniGame01Notes;
	}

	public void FillData(string playingMusicName, Stack<DanceNoteData> target)
	{
		target.Clear();
		foreach (DanceNoteData item in from elem in MusicNameToList(playingMusicName)
			orderby elem.TimeKey descending
			select elem)
		{
			target.Push(item);
		}
	}

	public void AddNote(string name, float timeKey)
	{
		if (!string.IsNullOrEmpty(name))
		{
			MusicNameToList(name).Add(new DanceNoteData
			{
				Pattern = DanceNoteData.Type.Dot,
				TimeKey = timeKey,
				TransitionTime = DefaultTransitionTime
			});
		}
	}

	public void Sort()
	{
		MiniGame01Notes = MiniGame01Notes.OrderBy((DanceNoteData elem) => elem.TimeKey).ToList();
		MiniGame02Notes = MiniGame02Notes.OrderBy((DanceNoteData elem) => elem.TimeKey).ToList();
	}
}
