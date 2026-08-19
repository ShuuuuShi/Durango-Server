using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public static class MiniGameDanceHelper
{
	public static float ElapsedTime => Time.realtimeSinceStartup;

	public static float DeltaTime => Time.deltaTime;

	public static bool IsOverTimeRange(MiniGameStatus status, MiniGameDanceAsset.DanceNoteData data, float range)
	{
		if (data == null)
		{
			return false;
		}
		return ElapsedTime > data.TimeKey + status.StartTime + range;
	}

	public static bool IsInTimeRange(MiniGameStatus status, MiniGameDanceAsset.DanceNoteData data, float range)
	{
		if (data == null)
		{
			return false;
		}
		return ElapsedTime >= data.TimeKey + status.StartTime - range && ElapsedTime < data.TimeKey + status.StartTime + range;
	}

	public static float AccuracyToTimeRange(MiniGameStatus.AccuracyType accuracy)
	{
		return accuracy switch
		{
			MiniGameStatus.AccuracyType.Normal => ResourceSingleton<MiniGameDanceAsset>.Instance().NormalTime, 
			MiniGameStatus.AccuracyType.Great => ResourceSingleton<MiniGameDanceAsset>.Instance().GreatTime, 
			MiniGameStatus.AccuracyType.Perfect => ResourceSingleton<MiniGameDanceAsset>.Instance().PerfectTime, 
			_ => 0f, 
		};
	}

	public static float AccuracyToScore(MiniGameStatus.AccuracyType accuracy)
	{
		return accuracy switch
		{
			MiniGameStatus.AccuracyType.Normal => Singleton<Constants>.Instance.MiniGameDance.Get("good_score", 100f), 
			MiniGameStatus.AccuracyType.Great => Singleton<Constants>.Instance.MiniGameDance.Get("great_score", 200f), 
			MiniGameStatus.AccuracyType.Perfect => Singleton<Constants>.Instance.MiniGameDance.Get("perfect_score", 400f), 
			_ => 0f, 
		};
	}

	public static Color AccuracyToColor(MiniGameStatus.AccuracyType accuracy)
	{
		return accuracy switch
		{
			MiniGameStatus.AccuracyType.Normal => PresetColor.Starship, 
			MiniGameStatus.AccuracyType.Great => PresetColor.Shakespeare, 
			MiniGameStatus.AccuracyType.Perfect => PresetColor.Cerise, 
			_ => Color.white, 
		};
	}

	public static string AccuracyToText(MiniGameStatus.AccuracyType accuracy)
	{
		return accuracy switch
		{
			MiniGameStatus.AccuracyType.Normal => T._("GOOD"), 
			MiniGameStatus.AccuracyType.Great => T._("GREAT!"), 
			MiniGameStatus.AccuracyType.Perfect => T._("PERFECT!!"), 
			_ => string.Empty, 
		};
	}

	public static float GetRotation(MiniGameDanceAsset.DanceNoteData.Type direction)
	{
		return direction switch
		{
			MiniGameDanceAsset.DanceNoteData.Type.Left => 180f, 
			MiniGameDanceAsset.DanceNoteData.Type.Up => 90f, 
			MiniGameDanceAsset.DanceNoteData.Type.Down => 270f, 
			_ => 0f, 
		};
	}

	public static MiniGameDanceAsset.DanceNoteData.Type AnalyzeSwipeDirection(Vector2 dir)
	{
		float num = 0f;
		MiniGameDanceAsset.DanceNoteData.Type result = MiniGameDanceAsset.DanceNoteData.Type.None;
		float num2 = Vector2.Dot(dir, Vector2.left);
		if (num2 > num)
		{
			num = num2;
			result = MiniGameDanceAsset.DanceNoteData.Type.Left;
		}
		num2 = Vector2.Dot(dir, Vector2.up);
		if (num2 > num)
		{
			num = num2;
			result = MiniGameDanceAsset.DanceNoteData.Type.Up;
		}
		num2 = Vector2.Dot(dir, Vector2.right);
		if (num2 > num)
		{
			num = num2;
			result = MiniGameDanceAsset.DanceNoteData.Type.Right;
		}
		num2 = Vector2.Dot(dir, Vector2.down);
		if (num2 > num)
		{
			num = num2;
			result = MiniGameDanceAsset.DanceNoteData.Type.Down;
		}
		return result;
	}

	public static WaitForSeconds WaitForNode(this MiniGameDanceGroup mono, MiniGameStatus status, float targetTime)
	{
		return new WaitForSeconds(status.StartTime + targetTime - ElapsedTime);
	}
}
