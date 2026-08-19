using System;
using Durango.System;
using Durango.Utils.Extensions;
using UnityEngine;

public static class Preferences
{
	public enum Level
	{
		Device,
		Player,
		User
	}

	public static string GetString(string key, string defaultValue = "", Level level = Level.Device)
	{
		key = ToLevelKey(key, level);
		return PlayerPrefs.GetString(key, defaultValue);
	}

	public static int GetInt(string key, int defaultValue = 0, Level level = Level.Device)
	{
		key = ToLevelKey(key, level);
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	public static float GetFloat(string key, float defaultValue = 0f, Level level = Level.Device)
	{
		key = ToLevelKey(key, level);
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	public static bool GetBool(string key, bool defaultValue = false, Level level = Level.Device)
	{
		return GetInt(key, defaultValue ? 1 : 0, level) != 0;
	}

	public static void SetString(string key, string value, Level level = Level.Device)
	{
		key = ToLevelKey(key, level);
		PlayerPrefs.SetString(key, value);
		PlayerPrefs.Save();
	}

	public static void SetInt(string key, int value, Level level = Level.Device)
	{
		key = ToLevelKey(key, level);
		PlayerPrefs.SetInt(key, value);
		PlayerPrefs.Save();
	}

	public static void SetFloat(string key, float value, Level level = Level.Device)
	{
		key = ToLevelKey(key, level);
		PlayerPrefs.SetFloat(key, value);
		PlayerPrefs.Save();
	}

	public static void SetBool(string key, bool value, Level level = Level.Device)
	{
		SetInt(key, value ? 1 : 0, level);
	}

	public static bool CheckTimePassed(string key, int timesInSec, Level level = Level.Device)
	{
		DateTime dateTime = DateTime.FromFileTimeUtc(GetString(key, string.Empty, level).ToInt64());
		DateTime utcNow = DateTime.UtcNow;
		if ((utcNow - dateTime).TotalSeconds >= (double)timesInSec)
		{
			SetString(key, utcNow.ToFileTimeUtc().ToString(), level);
			return true;
		}
		return false;
	}

	public static void ResetTimePassed(string key, Level level = Level.Device)
	{
		SetString(key, DateTime.UtcNow.ToFileTimeUtc().ToString(), level);
	}

	private static string ToLevelKey(string key, Level level)
	{
		switch (level)
		{
		case Level.Player:
			key = key + "_" + GameManager.PlayerId;
			break;
		case Level.User:
			key = key + "_" + Platform.Instance.NPA;
			break;
		}
		return key;
	}
}
