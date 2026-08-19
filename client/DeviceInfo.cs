using System;
using UnityEngine;

public static class DeviceInfo
{
	public enum Resolution
	{
		High,
		Medium,
		Low
	}

	private const int MinScreeinHeight = 720;

	public static Resolution DefaultResolution { get; private set; }

	public static Point2 FullScreenSize { get; private set; }

	public static Rect SafeRect { get; private set; }

	public static Point2 CurrentScreenSize { get; private set; }

	private static float Dpi { get; set; }

	public static float AspectRatio { get; private set; }

	public static float DeviceInch => Mathf.Sqrt(Mathf.Pow(FullScreenSize.x, 2f) + Mathf.Pow(FullScreenSize.y, 2f)) / Dpi;

	public static void Init()
	{
		if (FullScreenSize.x == 0 || FullScreenSize.y == 0)
		{
			FullScreenSize = new Point2(Screen.width, Screen.height);
			if (FullScreenSize.x < FullScreenSize.y)
			{
				FullScreenSize = new Point2(FullScreenSize.y, FullScreenSize.x);
			}
			SafeRect = new Rect(0f, 0f, 1f, 1f);
		}
		Dpi = Mathf.Clamp(Screen.dpi, 96f, Screen.dpi);
		AspectRatio = (float)FullScreenSize.x / (float)FullScreenSize.y;
		CalcDefaultResolution();
	}

	public static bool IsLowResolutionAllowed()
	{
		return FullScreenSize.x > 1280 && FullScreenSize.y > 720;
	}

	public static void ChangeResolution(Resolution resolution)
	{
		float screenSizeRatio = GetScreenSizeRatio(resolution);
		CurrentScreenSize = TransferScreenSize(FullScreenSize, screenSizeRatio);
		if (Application.isMobilePlatform)
		{
			SetPortraitOrientation();
		}
	}

	private static void SetPortraitOrientation()
	{
		if (Screen.width < Screen.height)
		{
			Screen.SetResolution(CurrentScreenSize.y, CurrentScreenSize.x, fullscreen: true);
		}
		else
		{
			Screen.SetResolution(CurrentScreenSize.x, CurrentScreenSize.y, fullscreen: true);
		}
	}

	private static void LogDeviceInfo()
	{
	}

	private static void CalcDefaultResolution()
	{
		LogDeviceInfo();
		DefaultResolution = Resolution.High;
		if (!(Dpi < 324f) && !((float)FullScreenSize.y / GetScreenSizeRatio(Resolution.Medium) < 720f))
		{
			if (Dpi <= 480f || (float)FullScreenSize.y / GetScreenSizeRatio(Resolution.Low) < 720f)
			{
				DefaultResolution = Resolution.Medium;
			}
			else
			{
				DefaultResolution = ((!IsLowResolutionAllowed()) ? Resolution.Medium : Resolution.Low);
			}
		}
	}

	private static float GetScreenSizeRatio(Resolution resolution)
	{
		return resolution switch
		{
			Resolution.High => 1f, 
			Resolution.Medium => 1.5f, 
			Resolution.Low => 2f, 
			_ => throw new ArgumentException("Resolution must be one of high/medium/low: " + resolution), 
		};
	}

	private static Point2 TransferScreenSize(Point2 vec, float val)
	{
		vec.x = (int)((float)vec.x / val);
		vec.y = (int)((float)vec.y / val);
		if (vec.x % 2 == 1)
		{
			vec.x++;
		}
		if (vec.y % 2 == 1)
		{
			vec.y++;
		}
		return vec;
	}
}
