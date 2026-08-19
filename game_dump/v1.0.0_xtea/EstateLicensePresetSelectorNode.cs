using UnityEngine;

public class EstateLicensePresetSelectorNode : SelectableWidget
{
	[SerializeField]
	private UISprite _border;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UIWidget _lineSprite;

	public void SetText(string text)
	{
		_textLabel.text = text;
	}

	public void EnableLine(int width)
	{
		((Component)_lineSprite).gameObject.SetActive(true);
		_lineSprite.width = width + base.Widget.width - _border.width;
	}

	public void DisableLine()
	{
		((Component)_lineSprite).gameObject.SetActive(false);
	}
}
