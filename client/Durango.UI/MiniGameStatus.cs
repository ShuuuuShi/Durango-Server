using System.Collections.Generic;
using Durango.Utils;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class MiniGameStatus
{
	public enum AccuracyType
	{
		None,
		Normal,
		Great,
		Perfect
	}

	public enum Mode
	{
		None,
		End,
		Game
	}

	public string PlayingMusicName;

	public Mode CurrentMode;

	public bool IsPlaying;

	public float StartTime;

	public bool IsPressed;

	public bool IsSwipeTried;

	public bool IsSuccessfullySwiped;

	public readonly List<Pair<MiniGameDanceAsset.DanceNoteData, AccuracyType>> PressibleNotes = new List<Pair<MiniGameDanceAsset.DanceNoteData, AccuracyType>>();

	public uint MusicInstanceId;

	public int AccuracyAccumulatedCount;

	private readonly HashSet<float> _pressedNote = new HashSet<float>();

	public float TotalScore { get; private set; }

	public void Init()
	{
		IsSwipeTried = (IsPlaying = (IsPressed = (IsSuccessfullySwiped = false)));
		PressibleNotes.Clear();
		_pressedNote.Clear();
		TotalScore = 0f;
		AccuracyAccumulatedCount = 0;
		PlayingMusicName = ((Application.isEditor && !string.IsNullOrEmpty(ResourceSingleton<MiniGameDanceAsset>.Instance().FixedMusicName)) ? ResourceSingleton<MiniGameDanceAsset>.Instance().FixedMusicName : ResourceSingleton<MiniGameDanceAsset>.Instance().MusicNames.Random());
	}

	public float AddToScore(Pair<float, AccuracyType> curAccuracy)
	{
		TotalScore += MiniGameDanceHelper.AccuracyToScore(curAccuracy.Item2);
		if (_pressedNote.Contains(curAccuracy.Item1))
		{
			return TotalScore;
		}
		_pressedNote.Add(curAccuracy.Item1);
		AccuracyAccumulatedCount++;
		return TotalScore;
	}

	public float GetDuration()
	{
		SoundEventInstance soundInstance = Singleton<SoundManager>.Instance().GetSoundInstance(MusicInstanceId);
		if (soundInstance == null)
		{
			return 30f;
		}
		return Mathf.Max(30f, soundInstance.Duration * 0.001f);
	}

	public void UpdatePressibleNotes(MiniGameStatus status)
	{
		for (int num = PressibleNotes.Count - 1; num >= 0; num--)
		{
			for (AccuracyType accuracyType = AccuracyType.Perfect; accuracyType >= AccuracyType.None; accuracyType--)
			{
				if (accuracyType == AccuracyType.None)
				{
					PressibleNotes.RemoveAt(num);
				}
				else if (MiniGameDanceHelper.IsInTimeRange(status, PressibleNotes[num].Item1, MiniGameDanceHelper.AccuracyToTimeRange(accuracyType)))
				{
					PressibleNotes[num] = new Pair<MiniGameDanceAsset.DanceNoteData, AccuracyType>(PressibleNotes[num].Item1, accuracyType);
					break;
				}
			}
		}
	}

	public Pair<MiniGameDanceAsset.DanceNoteData, AccuracyType> GetPressbieNote()
	{
		Pair<MiniGameDanceAsset.DanceNoteData, AccuracyType> result = default(Pair<MiniGameDanceAsset.DanceNoteData, AccuracyType>);
		float num = float.MaxValue;
		foreach (Pair<MiniGameDanceAsset.DanceNoteData, AccuracyType> pressibleNote in PressibleNotes)
		{
			if (!_pressedNote.Contains(pressibleNote.Item1.TimeKey))
			{
				float num2 = Mathf.Abs(StartTime + pressibleNote.Item1.TimeKey - MiniGameDanceHelper.ElapsedTime);
				if (!(num2 >= num))
				{
					num = num2;
					result = pressibleNote;
				}
			}
		}
		return result;
	}
}
