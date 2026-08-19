using Durango.Logic.Item;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class RewardIconWidget : UIWidget
{
	[SerializeField]
	protected UISprite _iconSprite;

	[SerializeField]
	private ItemIconTex _rgbIconTex;

	[SerializeField]
	private TweenerPlayer _tweener;

	public void Set(ItemIcon itemIcon, float iconScale)
	{
		SetItemIcon(_iconSprite, _rgbIconTex, itemIcon, iconScale);
	}

	public void PlayTweener()
	{
		if (_tweener != null)
		{
			_tweener.Play();
		}
	}

	public static void SetItemIcon(UISprite iconSprite, ItemIconTex rgbIconTex, ItemIcon itemIcon, float iconScale)
	{
		bool flag = iconSprite != null;
		bool flag2 = rgbIconTex != null;
		if (flag2 && itemIcon.Colors.HasValue)
		{
			rgbIconTex.SetIcon(itemIcon);
			rgbIconTex.gameObject.SetActive(value: true);
			if (flag)
			{
				iconSprite.gameObject.SetActive(value: false);
			}
		}
		else if (flag)
		{
			iconSprite.spriteName = itemIcon.Main;
			iconSprite.color = ((!itemIcon.Colors.HasValue) ? Color.white : itemIcon.Colors[0]);
			UIUtility.ResizeToSquare(iconSprite);
			iconSprite.gameObject.SetActive(value: true);
			if (flag2)
			{
				rgbIconTex.gameObject.SetActive(value: false);
			}
		}
		else if (flag2)
		{
			rgbIconTex.SetIcon(itemIcon);
		}
		if (flag)
		{
			iconSprite.transform.localScale = GetIconScale(iconScale);
		}
		if (flag2)
		{
			rgbIconTex.transform.localScale = GetIconScale(iconScale);
		}
	}

	private static Vector3 GetIconScale(float scale)
	{
		return (scale == 0f) ? Vector3.one : new Vector3(scale, scale, 1f);
	}
}
