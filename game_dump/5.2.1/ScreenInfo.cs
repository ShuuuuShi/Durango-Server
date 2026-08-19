using System;
using System.Collections.Generic;
using System.Linq;
using Durango.System.Config;
using UnityEngine;

public static class ScreenInfo
{
	private const string WindowSaveKey = "LastWindowScreenSize";

	private const string FullSaveKey = "LastFullScreenSize";

	private static ScreenSize _lastWindowScreenSize;

	private static ScreenSize _lastFullScreenSize;

	private static ScreenSize _currentScreenSize;

	public static void Init()
	{
		LoadFromPlayerPrefs("LastWindowScreenSize", out _lastWindowScreenSize);
		LoadFromPlayerPrefs("LastFullScreenSize", out _lastFullScreenSize);
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, (UICamera.OnScreenResize)delegate
		{
			if (!Screen.fullScreen)
			{
				UpdateCurrentScreenSize(new ScreenSize(Screen.width, Screen.height));
			}
		});
		_currentScreenSize = ((!Screen.fullScreen) ? _lastWindowScreenSize : _lastFullScreenSize);
	}

	private static void LoadFromPlayerPrefs(string saveKey, out ScreenSize target)
	{
		target = default(ScreenSize);
		string @string = Preferences.GetString(saveKey, null);
		if (string.IsNullOrEmpty(@string))
		{
			target = new ScreenSize(Screen.currentResolution);
		}
		else if (!ScreenSize.FromString(@string, out target))
		{
			target = new ScreenSize(Screen.currentResolution);
		}
	}

	public static void SetScreenMode(bool fullScreen)
	{
		if (Screen.fullScreen != fullScreen)
		{
			UpdateCurrentScreenSize((!fullScreen) ? _lastWindowScreenSize : _lastFullScreenSize);
			Screen.SetResolution(_currentScreenSize.Width, _currentScreenSize.Height, fullScreen);
		}
	}

	public static void ToggleScreenMode()
	{
		bool flag = !Screen.fullScreen;
		ConfigInstance.ChangeValue("screen_mode", (!flag) ? "window" : "fullscreen");
	}

	public static ScreenSize GetCurrentScreenSize()
	{
		return _currentScreenSize;
	}

	public static bool SetScreenSize(string screenSizeString)
	{
		if (!ScreenSize.FromString(screenSizeString, out var screenSize))
		{
			return false;
		}
		SetScreenSize(screenSize);
		return true;
	}

	private static void SetScreenSize(ScreenSize screenSize)
	{
		UpdateCurrentScreenSize(screenSize);
		Screen.SetResolution(screenSize.Width, screenSize.Height, Screen.fullScreen);
	}

	private static void UpdateCurrentScreenSize(ScreenSize screenSize)
	{
		if (!(_currentScreenSize == screenSize))
		{
			_currentScreenSize = screenSize;
			if (Screen.fullScreen)
			{
				_lastFullScreenSize = _currentScreenSize;
				Preferences.SetString("LastFullScreenSize", _lastFullScreenSize.ToString());
			}
			else
			{
				_lastWindowScreenSize = _currentScreenSize;
				Preferences.SetString("LastWindowScreenSize", _lastWindowScreenSize.ToString());
			}
			ConfigInstance.UpdateValue("resolution_pc", _currentScreenSize.ToString());
		}
	}

	public static IEnumerable<ScreenSize> GetAvailableScreenSizes()
	{
		return Screen.resolutions.Select((Resolution resolution) => new ScreenSize(resolution)).Where(ScreenSize.IsAvailable).Distinct();
	}
}
