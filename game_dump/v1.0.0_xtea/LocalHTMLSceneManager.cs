using UnityEngine;

public class LocalHTMLSceneManager : MonoBehaviour
{
	public string fileName;

	public string htmlText;

	public void LoadFromFile()
	{
		UniWebView uniWebView = CreateWebView();
		uniWebView.url = UniWebViewHelper.streamingAssetURLForPath(fileName);
		uniWebView.Load();
		uniWebView.Show();
	}

	public void LoadFromText()
	{
		UniWebView uniWebView = CreateWebView();
		uniWebView.LoadHTMLString(htmlText, null);
		uniWebView.Show();
	}

	private UniWebView CreateWebView()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		GameObject val = GameObject.Find("WebView");
		if ((Object)(object)val == (Object)null)
		{
			val = new GameObject("WebView");
		}
		UniWebView uniWebView = val.AddComponent<UniWebView>();
		uniWebView.toolBarShow = true;
		return uniWebView;
	}
}
