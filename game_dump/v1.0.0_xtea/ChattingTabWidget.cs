using UnityEngine;

public class ChattingTabWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISpriteLabel _subLabel;

	[SerializeField]
	private PressColorChange _pressColorChange;

	public void Set(string tabName, string subText)
	{
		_nameLabel.text = tabName;
		_subLabel.text = subText;
	}

	public void Select(bool select)
	{
		_pressColorChange.Select(select);
	}

	private void OnPress(bool press)
	{
		_pressColorChange.Press(press);
	}
}
