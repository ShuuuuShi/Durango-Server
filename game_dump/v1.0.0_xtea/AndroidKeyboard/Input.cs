using UnityEngine;

namespace AndroidKeyboard;

public static class Input
{
	public static string inputString;

	private static string m_compositionString = string.Empty;

	public static string compositionString
	{
		get
		{
			return m_compositionString;
		}
		set
		{
			if (m_compositionString != value)
			{
				m_compositionString = value;
			}
		}
	}

	public static IMECompositionMode imeCompositionMode { get; set; }

	public static Vector2 compositionCursorPos { get; set; }

	public static int touchCount => AndroidTouch.instance.touchCount;

	public static AndroidTouch.Touch[] touches
	{
		get
		{
			int num = touchCount;
			AndroidTouch.Touch[] array = new AndroidTouch.Touch[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = AndroidTouch.instance.GetTouch(i);
			}
			return array;
		}
	}

	public static void LateUpdate()
	{
	}

	public static bool GetKey(KeyCode key)
	{
		return false;
	}

	public static bool GetKey(string name)
	{
		return false;
	}

	public static bool GetKeyDown(KeyCode key)
	{
		return false;
	}

	public static bool GetKeyDown(string name)
	{
		return false;
	}
}
