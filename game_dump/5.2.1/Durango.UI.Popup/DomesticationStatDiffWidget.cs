using UnityEngine;

namespace Durango.UI.Popup;

public class DomesticationStatDiffWidget : UIWidget
{
	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private UILabel _content;

	[SerializeField]
	private UISprite _seperator;

	public void Set(string title, string content, bool showSeperator)
	{
		_title.text = title;
		_content.text = content;
		if (_seperator != null)
		{
			_seperator.gameObject.SetActive(showSeperator);
		}
	}
}
