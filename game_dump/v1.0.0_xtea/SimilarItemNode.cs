using ItemSystem;
using L10N;
using MarketData;
using UnityEngine;

public class SimilarItemNode : MonoBehaviour
{
	[SerializeField]
	private ItemIconTex _itemIcon;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISpriteLabel _priceLabel;

	private string _itemName;

	public void Set(Commodity commodity)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		ItemData item = commodity.GetItem();
		_itemIcon.SetIcon(item);
		_levelLabel.text = T._("{0:lv:}", item.Level);
		_priceLabel.text = Inventory.CurrencyFormat(commodity.Price, commodity.CurrencyType);
		_itemName = item.Name;
		_nameLabel.UpdateNGUIText();
		NGUIText.regionWidth = 100000;
		NGUIText.rectWidth = 100000;
		Vector2 val = NGUIText.CalculatePrintedSize(_itemName);
		if (val.x > (float)_nameLabel.width)
		{
			Vector2 val2 = NGUIText.CalculatePrintedSize("...");
			float num = ((float)_nameLabel.width - val2.x) / val.x;
			_nameLabel.text = $"{_itemName.Substring(0, (int)((float)_itemName.Length * num)).Trim()}...";
		}
		else
		{
			_nameLabel.text = _itemName;
		}
	}

	private void OnClick()
	{
		LineTooltipControl lineTooltipControl = UIManager.Popup.Tooltip<LineTooltipControl>();
		lineTooltipControl.Set(_itemName, null);
		lineTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		lineTooltipControl.Show(10f);
	}
}
