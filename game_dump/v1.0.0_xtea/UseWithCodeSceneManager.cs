using UnityEngine;
using UnityEngine.UI;

public class UseWithCodeSceneManager : MonoBehaviour
{
	public InputField urlInput;

	private void Start()
	{
		urlInput.text = "https://google.com";
	}

	public void OpenButtonClicked()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		GameObject val = GameObject.Find("WebView");
		if ((Object)(object)val == (Object)null)
		{
			val = new GameObject("WebView");
		}
		UniWebView uniWebView = val.AddComponent<UniWebView>();
		uniWebView.OnLoadComplete += OnLoadComplete;
		uniWebView.InsetsForScreenOreitation += InsetsForScreenOreitation;
		uniWebView.toolBarShow = true;
		uniWebView.url = urlInput.text;
		uniWebView.Load();
	}

	private void OnLoadComplete(UniWebView webView, bool success, string errorMessage)
	{
		if (success)
		{
			webView.Show();
		}
	}

	private UniWebViewEdgeInsets InsetsForScreenOreitation(UniWebView webView, UniWebViewOrientation orientation)
	{
		if (orientation == UniWebViewOrientation.Portrait)
		{
			return new UniWebViewEdgeInsets(5, 5, 5, 5);
		}
		return new UniWebViewEdgeInsets(5, 5, 5, 5);
	}
}
