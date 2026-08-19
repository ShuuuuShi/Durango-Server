using UnityEngine;

namespace Durango.UI;

public class TagButtonWidget : UIWidget
{
	[SerializeField]
	private UILabel _buttonText;

	public void Set(string text)
	{
		_buttonText.text = text;
	}
}
