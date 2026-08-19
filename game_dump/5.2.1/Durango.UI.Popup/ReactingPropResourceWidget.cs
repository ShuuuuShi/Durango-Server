using Durango.Logic.Item;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class ReactingPropResourceWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _iconCheck;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private UISpriteLabel _textCount;

	[SerializeField]
	private SpriteData _checkIcon;

	[SerializeField]
	private SpriteData _normalIcon;

	private UIWidget _widget;

	public bool Set(Cost cost)
	{
		_textName.text = cost.Currency.GetName();
		_textCount.text = Durango.Logic.Item.Inventory.CurrencyFormat(cost.Amount, cost.Currency);
		if (InventorySystem.Wallet.GetBalance(cost.Currency) < cost.Amount)
		{
			_normalIcon.Set(_iconCheck);
			return false;
		}
		_checkIcon.Set(_iconCheck);
		return true;
	}

	public bool Set(ReactingPropPopup.RequiredItemTags requiredItemTags)
	{
		int num = Util.Counting(GameSystem<InventorySystem>.Instance().PlayerItemList, ((ReactingPropPopup.RequiredItemTags)requiredItemTags).Filter);
		int count = requiredItemTags.Count;
		string format = ((num >= count) ? "[FFD85B]{0}[-] [71716B]/[-] [E8E5DF]{1}[-]" : "[DD5C56]{0}[-] [71716B]/[-] [E8E5DF]{1}[-]");
		_textName.text = requiredItemTags.LocalizedTagRequiredMsg;
		_textCount.text = string.Format(format, num, count);
		if (num < count)
		{
			_normalIcon.Set(_iconCheck);
			return false;
		}
		_checkIcon.Set(_iconCheck);
		return true;
	}
}
