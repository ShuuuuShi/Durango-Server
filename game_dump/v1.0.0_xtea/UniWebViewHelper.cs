using UnityEngine;

public class UniWebViewHelper
{
	public static int screenHeight => Screen.height;

	public static int screenWidth => Screen.width;

	public static int screenScale => 1;

	public static string streamingAssetURLForPath(string path)
	{
		return "file:///android_asset/" + path;
	}
}
