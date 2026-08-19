using UnityEngine;
using UnityEngine.UI;

public class RunJavaScriptInWebSceneManager : MonoBehaviour
{
	public Text result;

	private UniWebView _webView;

	private string _fileName = "UniWebViewDemo/demo.html";

	public void LoadFromFile()
	{
		if (!((Object)(object)_webView != (Object)null))
		{
			_webView = CreateWebView();
			_webView.url = UniWebViewHelper.streamingAssetURLForPath(_fileName);
			int screenHeight = UniWebViewHelper.screenHeight;
			_webView.insets = new UniWebViewEdgeInsets(0, 0, screenHeight / 2, 0);
			_webView.OnEvalJavaScriptFinished += OnEvalJavaScriptFinished;
			_webView.OnWebViewShouldClose += delegate
			{
				_webView = null;
				return true;
			};
			_webView.Load();
			_webView.Show();
		}
	}

	public void AddScript(InputField input)
	{
		if ((Object)(object)_webView == (Object)null)
		{
			result.text = "Please open the web view first.";
		}
		else
		{
			_webView.AddJavaScript(input.text);
		}
	}

	public void RunScript(InputField input)
	{
		if ((Object)(object)_webView == (Object)null)
		{
			result.text = "Please open the web view first.";
		}
		else
		{
			_webView.EvaluatingJavaScript(input.text);
		}
	}

	private void OnEvalJavaScriptFinished(UniWebView webView, string r)
	{
		result.text = r;
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
