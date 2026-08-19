using System;
using System.IO;
using UnityEngine;

public static class XigncodeIntegration
{
	public static void SetUserInfo(string userUid)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass("com.wellbia.xigncode.XigncodeClient");
		try
		{
			AndroidJavaObject val2 = ((AndroidJavaObject)val).CallStatic<AndroidJavaObject>("getInstance", new object[0]);
			try
			{
				val2.Call("setUserInfo", new object[1] { userUid });
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static string GetCookie(string seed)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass("com.wellbia.xigncode.XigncodeClient");
		try
		{
			AndroidJavaObject val2 = ((AndroidJavaObject)val).CallStatic<AndroidJavaObject>("getInstance", new object[0]);
			try
			{
				return val2.Call<string>("getCookie2", new object[1] { seed });
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void CopyXigncodeLibs(bool debug)
	{
		string text = Application.dataPath + ((!debug) ? "/Editor/XigncodeLibs/live/" : "/Editor/XigncodeLibs/debug/");
		string text2 = Application.dataPath + "/Plugins/Android/libs/armeabi-v7a/";
		File.Copy(text + "libgabriel.so", text2 + "libgabriel.so", overwrite: true);
		File.Copy(text + "libxigncode.so", text2 + "libxigncode.so", overwrite: true);
	}
}
