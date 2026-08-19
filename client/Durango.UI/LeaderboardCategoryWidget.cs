using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class LeaderboardCategoryWidget : SelectableWidget
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private GameObject _imageHorizontalDotLine;

	[SerializeField]
	private GameObject _imageVerticalDotLine;

	public PunchingLeaderboardSystem.Category Category { get; private set; }

	public void Refresh(PunchingLeaderboardSystem.Category category, SpriteData iconSprite)
	{
		Category = category;
		iconSprite.Set(_icon);
		_text.text = category.GetName();
	}

	public void SetPortraitMode(bool portraitMode)
	{
		_imageHorizontalDotLine.SetActive(!portraitMode);
		_imageVerticalDotLine.SetActive(portraitMode);
	}
}
