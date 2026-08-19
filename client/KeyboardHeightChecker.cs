using System;
using UnityEngine;

public class KeyboardHeightChecker
{
	private bool _isVisible;

	public static int Height { get; private set; }

	public static event Action<int> KeyboardHeightUpdated;

	public KeyboardHeightChecker()
	{
		KeyboardHeightChecker.KeyboardHeightUpdated = null;
	}

	public void Check()
	{
	}

	private void CheckHeight()
	{
		int height = 0;
		SetHeight(height);
	}

	private void SetHeight(int height)
	{
		if (Height != height)
		{
			Height = height;
			if (KeyboardHeightChecker.KeyboardHeightUpdated != null)
			{
				KeyboardHeightChecker.KeyboardHeightUpdated(height);
			}
		}
	}

	private int GetDeviceHeight()
	{
		return (Screen.height <= Screen.width) ? DeviceInfo.FullScreenSize.y : DeviceInfo.FullScreenSize.x;
	}
}
