using System;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Animal;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class ShopBuyConfirmPopup : TooltipBase
{
	[SerializeField]
	private UILabel _mainTitleLabel;

	[SerializeField]
	private ShopCommodityInfoView _infoWidget;

	[SerializeField]
	private ShopCommodityItemPreview _previewWidget;

	[SerializeField]
	private ShopCommodityContentsList _itemListWidget;

	[SerializeField]
	private SelectableButton _cancelButton;

	[SerializeField]
	private SelectableButton _confirmButton;

	private Durango.Logic.Shop.Commodity _commodity;

	private Action<Durango.Logic.Shop.Commodity> _confirmed;

	private ContentDescription _previewData;

	private RectLayoutComponent _layout;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		_layout = GetComponent<RectLayoutComponent>();
	}

	protected override void Start()
	{
		base.Start();
		_cancelButton.Text = T._("취소");
		SelectableButton cancelButton = _cancelButton;
		cancelButton.Clicked = (Action)Delegate.Combine(cancelButton.Clicked, (Action)delegate
		{
			if (base.State == VisibleState.Show)
			{
				Hide();
			}
		});
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, (Action)delegate
		{
			if (base.State == VisibleState.Show)
			{
				if (_confirmButton.Disabled)
				{
					UIManager.SystemMsg(T._("구매할 수 없습니다."));
				}
				else
				{
					Hide();
					if (_confirmed != null)
					{
						_confirmed(_commodity);
					}
				}
			}
		});
		_confirmButton.CanClickWhenDisabled = true;
		_previewWidget.Closed += delegate
		{
			ShowPreview(null);
		};
		ShopCommodityContentsList itemListWidget = _itemListWidget;
		itemListWidget.Clicked = (Action<ContentDescription>)Delegate.Combine(itemListWidget.Clicked, new Action<ContentDescription>(OnClickItem));
	}

	protected override void OnTryConfirmOnModal()
	{
		if (_confirmButton != null && _confirmButton.Clicked != null)
		{
			_confirmButton.Clicked();
		}
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}

	protected override SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = true;
		return _cancelButton;
	}

	private void OnClickItem(ContentDescription data)
	{
		if (data == _previewData)
		{
			data = null;
		}
		ShowPreview(data);
		if (data == null)
		{
			return;
		}
		ItemData item = data.Item;
		if (item == null)
		{
			return;
		}
		int petEntityType = item.GetPetEntityType();
		if (petEntityType != 0)
		{
			PetManager.GetPreviewPet(petEntityType, PetRank.A, item.Level, delegate(Messages.Pet? pet)
			{
				if (data == _previewData && pet.HasValue)
				{
					Messages.Pet value = pet.Value;
					value.Rank = PetRank.Invalid;
					ItemInfoTooltip itemInfoTooltip2 = UIManager.Popup.Tooltip<ItemInfoTooltip>();
					itemInfoTooltip2.Sign = 1;
					itemInfoTooltip2.Set(value);
					itemInfoTooltip2.Show();
				}
			});
		}
		else
		{
			ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
			itemInfoTooltip.Sign = 1;
			itemInfoTooltip.Set(item);
			itemInfoTooltip.Show();
		}
	}

	public void Set(Durango.Logic.Shop.Commodity commodity, Action<Durango.Logic.Shop.Commodity> confirmed)
	{
		_commodity = commodity;
		_previewData = null;
		_confirmed = confirmed;
		bool flag = commodity.IsPurchasable();
		string text = ((!flag) ? T._("구매") : string.Format("{0}  [preset=round_box?{1}]", T._("구매"), commodity.GetCurrencyText(hasDiscountRatio: false)));
		_confirmButton.Text = text;
		_confirmButton.Disabled = !flag;
	}

	protected override void FillData()
	{
		_mainTitleLabel.text = _commodity.Title;
		_infoWidget.Set(_commodity);
		if (KUtility.GetSize(_commodity.ContentDescriptions) > 0)
		{
			_itemListWidget.gameObject.SetActive(value: true);
			_itemListWidget.Set(_commodity.ContentDescriptions);
		}
		else
		{
			_itemListWidget.gameObject.SetActive(value: false);
		}
		ShowPreview(_previewData);
	}

	protected override void UpdateLayout()
	{
		if (UIManager.IsPortraitScreen)
		{
			_layout.UpdateLayout(700f, null);
		}
		else if (_itemListWidget.gameObject.activeSelf)
		{
			_layout.UpdateLayout(1000f, null);
		}
		else
		{
			_layout.UpdateLayout(700f, null);
		}
		_previewWidget.SetDimensions(_infoWidget.width, _itemListWidget.height);
		_previewWidget.GetComponent<RectLayoutComponent>().UpdateLayout();
		_previewWidget.SetPosition(_infoWidget.GetPosition(1f, 1f), 1f, 1f);
		UIUtility.UpdateAnchors(base.transform);
		base.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
	}

	private void ShowPreview(ContentDescription data)
	{
		_previewData = data;
		if (_previewWidget.SetPreview(data))
		{
			_infoWidget.visible = false;
			_previewWidget.visible = true;
			_itemListWidget.SelectItem(_commodity.ContentDescriptions.IndexOf(data));
		}
		else
		{
			_infoWidget.visible = true;
			_previewWidget.visible = false;
			_itemListWidget.SelectItem(-1);
		}
	}
}
