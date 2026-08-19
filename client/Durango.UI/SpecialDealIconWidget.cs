using System;
using Durango.Logic;
using Durango.Network;
using UnityEngine;

namespace Durango.UI;

public class SpecialDealIconWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _textRemainTime;

	public event Action IconClicked;

	private void Start()
	{
		UIEventListener.Get(_icon.gameObject).onClick = OnClick_Icon;
		base.gameObject.SetActive(value: false);
	}

	public bool Refresh()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double expiresAt = GameSystem<ShopSystem>.Instance().SpecialDealsMinExpiresAt;
		bool flag = GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Shop) && GameSystem<ShopSystem>.Instance().HasSpecialDeals && expiresAt > predictedServerTime;
		if (base.gameObject.activeSelf != flag)
		{
			base.gameObject.SetActive(flag);
			if (flag)
			{
				_textRemainTime.SetText(new SyncString(delegate(out string text, out float period)
				{
					SyncString.UpdateRemainTimeMsg(expiresAt, "{0}", out text, out period, string.Empty, 1);
				}));
			}
			else
			{
				_textRemainTime.text = string.Empty;
			}
			return true;
		}
		return false;
	}

	private void OnClick_Icon(GameObject go)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		if (this.IconClicked != null)
		{
			this.IconClicked();
		}
	}
}
