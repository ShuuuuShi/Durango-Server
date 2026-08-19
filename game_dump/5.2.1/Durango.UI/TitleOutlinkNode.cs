using Durango.Logic.Clusters;
using Durango.System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class TitleOutlinkNode : UIWidget
{
	[SerializeField]
	private UISpriteLabel _content;

	[SerializeField]
	private UISprite _verticalBorder;

	[SerializeField]
	private UISprite _horizontalBorder;

	private Urls _data;

	private string _title;

	public void Set(string title, Urls data)
	{
		_data = data;
		_title = title;
		_content.text = "[icon=" + data.IconKey + "] " + title;
	}

	public void SetBorder(bool isPortrait, bool isLast)
	{
		_horizontalBorder.gameObject.SetActive(isPortrait && !isLast);
		_verticalBorder.gameObject.SetActive(!isPortrait && !isLast);
	}

	private void OnClick()
	{
		if (_data != null && !string.IsNullOrEmpty(_data.UrlLink))
		{
			Platform.Instance.ShowWeb(_title, _data.UrlLink);
		}
	}
}
