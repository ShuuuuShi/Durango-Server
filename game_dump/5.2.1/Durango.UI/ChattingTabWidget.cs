using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ChattingTabWidget : SelectableWidget
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _subLabel;

	[SerializeField]
	private UISprite _pushStateSprite;

	[SerializeField]
	private UISprite _hideStateSprite;

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
	}

	public void Set(string tabName, string subText, bool pushOff, bool hided)
	{
		_nameLabel.text = tabName;
		_subLabel.text = subText;
		_pushStateSprite.gameObject.SetActive(pushOff);
		_hideStateSprite.gameObject.SetActive(hided);
		Vector3 localPosition = _pushStateSprite.transform.localPosition;
		if (pushOff)
		{
			localPosition -= new Vector3(_hideStateSprite.width + 2, 0f);
		}
		_hideStateSprite.transform.localPosition = localPosition;
	}
}
