using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class RewardItemWidget : UIWidget
{
	[SerializeField]
	private UILabel _singularTitle;

	[SerializeField]
	private UILabel _smallTitle;

	[SerializeField]
	private UILabel _description;

	[SerializeField]
	private UILabel _supText;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private GameObject _goodEffect;

	[SerializeField]
	private ItemIconTex _itemIcon;

	[SerializeField]
	private GameObject _bonusObject;

	public RewardItemWidget SetTitle(string title, string subTitle)
	{
		bool flag = string.IsNullOrEmpty(subTitle);
		_singularTitle.gameObject.SetActive(flag);
		_smallTitle.gameObject.SetActive(!flag);
		_description.gameObject.SetActive(!flag);
		if (flag)
		{
			_singularTitle.text = title;
		}
		else
		{
			_smallTitle.text = title;
			_description.text = subTitle;
		}
		return this;
	}

	public RewardItemWidget SetIcon(string icon, ItemColor iconColor = default(ItemColor), string rTable = null, string gTable = null, string bTable = null)
	{
		if (iconColor.HasValue)
		{
			_itemIcon.SetIcon(icon, iconColor);
			_icon.gameObject.SetActive(value: false);
			_itemIcon.gameObject.SetActive(value: true);
		}
		else if (!string.IsNullOrEmpty(rTable) || !string.IsNullOrEmpty(gTable) || !string.IsNullOrEmpty(bTable))
		{
			_itemIcon.SetIcon(icon, rTable, gTable, bTable);
			_icon.gameObject.SetActive(value: false);
			_itemIcon.gameObject.SetActive(value: true);
		}
		else
		{
			_icon.spriteName = icon;
			_icon.gameObject.SetActive(value: true);
			_itemIcon.gameObject.SetActive(value: false);
		}
		UIUtility.ResizeToSquare(_icon, 48);
		return this;
	}

	public RewardItemWidget SetSupText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			_supText.gameObject.SetActive(value: false);
		}
		else
		{
			_supText.gameObject.SetActive(value: true);
			_supText.text = text;
		}
		return this;
	}

	public RewardItemWidget SetBonus(bool isBonus)
	{
		if (isBonus)
		{
			_bonusObject.gameObject.SetActive(value: true);
			Vector3[] array = localCorners;
			_bonusObject.transform.localPosition = Vector3.Lerp(array[2], array[3], 0.5f);
		}
		else
		{
			_bonusObject.gameObject.SetActive(value: false);
		}
		return this;
	}

	public RewardItemWidget SetGoodEffect(bool on)
	{
		_goodEffect.gameObject.SetActive(on);
		return this;
	}
}
