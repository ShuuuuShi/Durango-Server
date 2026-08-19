using UnityEngine;
using UnityEngine.UI;

public class SizeAndTransitionSceneManager : MonoBehaviour
{
	private UniWebView _webView;

	private bool _webViewReady;

	public InputField top;

	public InputField left;

	public InputField bottom;

	public InputField right;

	public InputField x;

	public InputField y;

	public InputField width;

	public InputField height;

	public bool fade { get; set; }

	public int transitionEdge { get; set; }

	private void Start()
	{
		_webView = CreateWebView();
		_webView.Load("https://example.com");
	}

	public void ShowClicked()
	{
		if (_webViewReady)
		{
			_webView.Show(fade, (UniWebViewTransitionEdge)transitionEdge, 0.4f, delegate
			{
			});
			_webView.ShowToolBar(animate: true);
		}
		else
		{
			_webView.Load("https://example.com");
		}
	}

	public void SetInsetsClicked()
	{
		int aTop = ((!string.IsNullOrEmpty(top.text)) ? int.Parse(top.text) : 0);
		int aLeft = ((!string.IsNullOrEmpty(left.text)) ? int.Parse(left.text) : 0);
		int aBottom = ((!string.IsNullOrEmpty(bottom.text)) ? int.Parse(bottom.text) : 0);
		int aRight = ((!string.IsNullOrEmpty(right.text)) ? int.Parse(right.text) : 0);
		_webView.insets = new UniWebViewEdgeInsets(aTop, aLeft, aBottom, aRight);
	}

	private UniWebView CreateWebView()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		GameObject val = GameObject.Find("WebView");
		if ((Object)(object)val == (Object)null)
		{
			val = new GameObject("WebView");
		}
		UniWebView webView = val.AddComponent<UniWebView>();
		webView.OnLoadComplete += delegate(UniWebView view, bool success, string errorMessage)
		{
			if (success)
			{
				_webViewReady = true;
			}
			else
			{
				Debug.LogError((object)("Loading failed: " + errorMessage));
			}
		};
		webView.OnWebViewShouldClose += delegate
		{
			webView.Hide(fade, (UniWebViewTransitionEdge)transitionEdge, 0.4f, delegate
			{
			});
			webView.HideToolBar(animate: true);
			return false;
		};
		return webView;
	}
}
