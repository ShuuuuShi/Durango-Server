using System;
using UnityEngine;

namespace MMT;

public class AssetStream
{
	private static string lastZipFilePath;

	private static AndroidJavaObject cachedZipFile;

	public static bool GetZipFileOffsetLength(string zipFilePath, string fileName, out long offset, out long length)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		offset = 0L;
		length = 0L;
		AndroidJavaObject val3;
		if (zipFilePath.EndsWith("apk"))
		{
			AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			try
			{
				AndroidJavaObject @static = ((AndroidJavaObject)val).GetStatic<AndroidJavaObject>("currentActivity");
				try
				{
					AndroidJavaObject val2 = @static.Call<AndroidJavaObject>("getAssets", new object[0]);
					try
					{
						val3 = val2.Call<AndroidJavaObject>("openFd", new object[1] { fileName });
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)@static)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else
		{
			if (lastZipFilePath != zipFilePath)
			{
				lastZipFilePath = zipFilePath;
				if (cachedZipFile != null)
				{
					cachedZipFile.Dispose();
					cachedZipFile = null;
				}
				cachedZipFile = new AndroidJavaObject("com.android.vending.expansion.zipfile.ZipResourceFile", new object[1] { zipFilePath });
			}
			val3 = cachedZipFile.Call<AndroidJavaObject>("getAssetFileDescriptor", new object[1] { "assets/" + fileName });
		}
		if (val3 != null && val3.GetRawObject() != IntPtr.Zero)
		{
			offset = val3.Call<long>("getStartOffset", new object[0]);
			length = val3.Call<long>("getLength", new object[0]);
			val3.Dispose();
			val3 = null;
			return true;
		}
		Debug.LogError((object)("Couldn't find file: " + fileName + " in: " + zipFilePath));
		return false;
	}
}
