using Durango.Logic.Item;
using Durango.UI.Control;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class ShopCommodityContentItem : UIWidget
{
	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private GameObject _hasDetailSprite;

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying)
		{
			GetComponent<RectLayoutComponent>().UpdateOnSizeChange();
		}
	}

	public void Set(ContentDescription item)
	{
		if (item.IconColor.HasValue)
		{
			_iconTexture.SetIcon(item.Icon, item.IconColor);
		}
		else
		{
			_iconTexture.SetIcon(item.Icon);
		}
		_nameLabel.text = item.Name;
		if (string.IsNullOrEmpty(item.Text))
		{
			_textLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_textLabel.gameObject.SetActive(value: true);
			_textLabel.text = item.Text;
		}
		if (item.Item != null && item.Item.HasPreview())
		{
			_hasDetailSprite.gameObject.SetActive(value: true);
		}
		else if (!string.IsNullOrEmpty(item.Motion) && GameSystem<SocialSystem>.Instance().Emotional.GetMotion(item.Motion) != null)
		{
			_hasDetailSprite.gameObject.SetActive(value: true);
		}
		else
		{
			_hasDetailSprite.gameObject.SetActive(value: false);
		}
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}
}
