using UnityEngine;

public static class WebViewManager
{
	public static void Load(string url)
	{
		Application.OpenURL(url);
	}
}
