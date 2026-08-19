using Durango.Logic.Shop;
using Durango.UI.Control;
using Durango.Utils;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PurchasedListWidget : UIWidget
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private GameObject _voucherWidget;

	[SerializeField]
	private UISpriteLabel _voucherNameLabel;

	[SerializeField]
	private UILabel _voucherCountLabel;

	[SerializeField]
	private Transform _resultsContainer;

	[SerializeField]
	private PurchasedWidget _resultBase;

	private ListObjectPool<PurchasedWidget> _results;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_results = new ListObjectPool<PurchasedWidget>();
			_results.BaseObject = _resultBase;
			_results.UseBase = true;
			_results.Clear();
		}
	}

	public void Set(Durango.Logic.Shop.Commodity commodity, Purchased purchased)
	{
		Init();
		_titleLabel.text = commodity.Title;
		if (commodity.VoucherPurchasable() && InventorySystem.Wallet.PurchasableVoucherCount(commodity) > 0)
		{
			_voucherWidget.gameObject.SetActive(value: true);
			Voucher voucher = SingletonDict<string, Voucher>.Get(commodity.Data.VoucherId);
			int voucherCount = InventorySystem.Wallet.GetVoucherCount(commodity.Data.VoucherId);
			_voucherCountLabel.text = $"[ffd85b]{voucherCount}[-]/{voucher.CountMax}";
			_voucherNameLabel.text = $"{voucher.GetIconText()} {voucher.Name}";
		}
		else
		{
			_voucherWidget.gameObject.SetActive(value: false);
		}
		SoundManager.PlayEvent("ui_random_box_item_open");
		_results.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(purchased.Purchases); i < size; i++)
		{
			PurchasedWidget next = _results.GetNext();
			next.Set(purchased.Purchases[i]);
			next.PlayAnimation((float)i * 0.25f);
			next.PlayPaybackAnimation((float)(size + 2) * 0.25f);
			KUtility.DelayedCall(Durango.Utils.Singleton<SoundManager>.Instance(), delegate
			{
				SoundManager.PlayEvent("ui_random_box_item");
			}, (float)(i + 1) * 0.25f + 0.05f);
		}
		_results.EndLoad();
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		Vector2 vector = new Vector2((float)base.width * 0.8f, (float)base.height * 0.7f);
		Vector2 baseNodeSize = _results.BaseObject.localSize;
		int num = Mathf.Min(5, (int)((vector.x + 20f) / (baseNodeSize.x + 20f)));
		int num2 = Mathf.CeilToInt((float)_results.Count / (float)num);
		float rowMargin = ((num <= 1) ? 0f : ((vector.x - (float)num * baseNodeSize.x) / (float)(num - 1)));
		float colMargin = Mathf.Max(20f, (num2 <= 1) ? 0f : ((vector.y - (float)num2 * baseNodeSize.y) / (float)(num2 - 1)));
		int rowItemCount;
		float rowSize;
		float colSize;
		Vector2 vector2 = UIUtility.WidgetsGridReposition(_results, null, Vector2.down, Vector3.zero, vector.x, baseNodeSize, rowMargin, colMargin, out rowItemCount, out rowSize, out colSize);
		_resultsContainer.transform.localPosition = new Vector3(0f - vector2.x, vector2.y) * 0.5f + new Vector3(0f, -15f);
	}
}
