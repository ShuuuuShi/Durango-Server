using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class QuestBannerWidget : UIWidget
{
	[SerializeField]
	private UIWidget _imageContainer;

	[SerializeField]
	private UISprite _imageSprite;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _descriptionContainer;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UIWidget _durationContainer;

	[SerializeField]
	private UILabel _durationLabel;

	public void Set(Season? season)
	{
		if (!season.HasValue)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		Season value = season.Value;
		_titleLabel.text = value.Name;
		if (string.IsNullOrEmpty(value.BannerImg))
		{
			_imageContainer.gameObject.SetActive(value: false);
		}
		else
		{
			_imageContainer.gameObject.SetActive(value: true);
			_imageSprite.spriteName = value.BannerImg;
			UIUtility.ResizeToSquare(_imageSprite);
		}
		if (string.IsNullOrEmpty(value.BannerText))
		{
			_descriptionContainer.gameObject.SetActive(value: false);
		}
		else
		{
			_descriptionContainer.gameObject.SetActive(value: true);
			_descriptionLabel.text = value.BannerText;
		}
		string dateString = Times.GetDateString(value.Since, value.Until, "{0:m} {0:HH:mm}", useClientTime: true);
		if (string.IsNullOrEmpty(dateString))
		{
			_durationContainer.gameObject.SetActive(value: false);
			return;
		}
		_durationContainer.gameObject.SetActive(value: true);
		_durationLabel.text = dateString;
		Vector2 printedSize = _durationLabel.printedSize;
		_durationContainer.leftAnchor.absolute = _durationContainer.rightAnchor.absolute - ((int)printedSize.x + 20);
		UIUtility.UpdateAnchors(_durationContainer.transform);
	}
}
