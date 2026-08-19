using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class AccessRightsTab : SelectableWidget
{
	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UILabel _subTextLabel;

	public void Set(string text, string subText)
	{
		_textLabel.text = text;
		_subTextLabel.text = subText;
	}
}
