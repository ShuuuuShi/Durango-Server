using UnityEngine;
using UnityEngine.UI;

public class CallbackFromWebSceneManager : MonoBehaviour
{
	public Text result;

	private UniWebView _webView;

	private string _fileName = "UniWebViewDemo/callback.html";

	public void LoadFromFile()
	{
		if (!((Object)(object)_webView != (Object)null))
		{
			_webView = CreateWebView();
			_webView.url = UniWebViewHelper.streamingAssetURLForPath(_fileName);
			int screenHeight = UniWebViewHelper.screenHeight;
			_webView.insets = new UniWebViewEdgeInsets(0, 0, screenHeight / 2, 0);
			_webView.OnReceivedMessage += OnReceivedMessage;
			_webView.Load();
			_webView.Show();
		}
	}

	private void OnReceivedMessage(UniWebView webView, UniWebViewMessage message)
	{
		if (message.path == "close")
		{
			result.text = string.Empty;
			Object.Destroy((Object)(object)webView);
			_webView = null;
		}
		if (message.path == "add")
		{
			int num = 0;
			int num2 = 0;
			if (int.TryParse(message.args["num1"], out num) && int.TryParse(message.args["num2"], out num2))
			{
				int num3 = num + num2;
				result.text = num + " + " + num2 + " = " + num3;
			}
			else
			{
				result.text = "Invalid Input";
			}
		}
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
