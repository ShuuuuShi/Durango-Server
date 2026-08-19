using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class TestWebBrowserGroup : UIBase
{
	[SerializeField]
	private UIInput _inputUrl;

	[SerializeField]
	private WebBrowserControl _webBrowserControl;

	private void Start()
	{
		SetChildrenActive(activated: false);
		WebBrowserControl webBrowserControl = _webBrowserControl;
		webBrowserControl.UrlChanged = (Action<string>)Delegate.Combine(webBrowserControl.UrlChanged, (Action<string>)delegate(string url)
		{
			_inputUrl.value = url;
		});
	}

	protected override void OnScreenResized()
	{
		_webBrowserControl.OnScreenResized();
	}

	public void OpenUrl()
	{
		_webBrowserControl.OpenUrl(_inputUrl.value);
	}

	public override bool Close()
	{
		if (!base.Close())
		{
			return false;
		}
		_webBrowserControl.StopBrowsing();
		return true;
	}
}
