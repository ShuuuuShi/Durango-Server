using System;
using AndroidKeyboard;
using UnityEngine;

public static class AndroidKeyboardManager
{
	private static string NAME_CLASS = "com.OhYeahDev.softInput.KeyboardActivity";

	private static AndroidJavaObject _AndroidPluginObj;

	private static AndroidJavaClass deployGate;

	private static bool installed;

	public static void Install()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		if (!installed)
		{
			AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject @static = ((AndroidJavaObject)val).GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject app = @static.Call<AndroidJavaObject>("getApplicationContext", new object[0]);
			deployGate = new AndroidJavaClass(NAME_CLASS);
			@static.Call("runOnUiThread", new object[1] { (object)(AndroidJavaRunnable)delegate
			{
				int num = Mathf.Min(Screen.width, Screen.height);
				((AndroidJavaObject)deployGate).CallStatic("Init", new object[3] { app, num, 5 });
			} });
			installed = true;
		}
	}

	public static void Open(string text, int maskOptions, bool alert, string textPlaceholder)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("Open", new object[5]
			{
				text,
				maskOptions,
				alert,
				textPlaceholder,
				AdditionalOptions.selectAllTextOnFocus
			});
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void Close()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("Close", new object[0]);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void CloseInCode()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("CloseInCode", new object[0]);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static bool IsOpen()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			return ((AndroidJavaObject)val).CallStatic<bool>("IsOpen", new object[0]);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void ClearText()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("ClearText", new object[0]);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void SetCursorPosition(int start, int end)
	{
		((AndroidJavaObject)deployGate).CallStatic("SetCursorPosition", new object[2] { start, end });
		TouchScreenKeyboard.CursorPositionStart = start;
		TouchScreenKeyboard.CursorPositionEnd = end;
	}

	public static void SetHideInput(bool value)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("SetHideInput", new object[1] { value });
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void SetFullScreen(bool value)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("SetFullScreen", new object[1] { value });
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void SetSoftInputMode(InputAdjustType value)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("SetSoftInputMode", new object[1] { value == InputAdjustType.SOFT_INPUT_ADJUST_RESIZE });
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void SetNeverEditTextOnTop(bool value)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("SetNeverEditTextOnTop", new object[1] { value });
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void KeepKeyboardOn(bool value)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("KeepKeyboardOn", new object[1] { value });
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void EnableLogging(bool value)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("EnableLogging", new object[1] { value });
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void SetCharacterLimit(int num)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(NAME_CLASS);
		try
		{
			((AndroidJavaObject)val).CallStatic("SetCharacterLimit", new object[1] { num });
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void SetText(string text)
	{
		((AndroidJavaObject)deployGate).CallStatic("SetText", new object[1] { text });
	}

	public static void ClearComposition()
	{
		((AndroidJavaObject)deployGate).CallStatic("ClearComposition", new object[0]);
	}
}
