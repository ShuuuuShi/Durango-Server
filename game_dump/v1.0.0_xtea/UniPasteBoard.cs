using System;
using UnityEngine;

public class UniPasteBoard
{
	private static AndroidJavaClass _javaClass;

	private static AndroidJavaClass JavaClass
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			if (_javaClass == null)
			{
				try
				{
					_javaClass = new AndroidJavaClass("com.onevcat.UniPasteBoard.PasteBoard");
				}
				catch (Exception)
				{
				}
			}
			return _javaClass;
		}
	}

	public static string GetClipBoardString()
	{
		return androidGetClipBoardString();
	}

	public static void SetClipBoardString(string text)
	{
		androidSetClipBoardString(text);
	}

	private static string androidGetClipBoardString()
	{
		string result = null;
		if (JavaClass != null)
		{
			result = ((AndroidJavaObject)JavaClass).CallStatic<string>("getClipBoardString", new object[0]);
		}
		return result;
	}

	private static void androidSetClipBoardString(string text)
	{
		if (JavaClass != null)
		{
			((AndroidJavaObject)JavaClass).CallStatic("setClipBoardString", new object[1] { text });
		}
	}
}
