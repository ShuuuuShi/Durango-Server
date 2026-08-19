using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.Network;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ShopCommodityWidget : SelectableWidget, RectLayout.ICompatible
{
	[CompilerGenerated]
	private sealed class _003CContentsIconEnumerable_003Ed__25 : IEnumerable<UIWidget>, IEnumerable, IEnumerator<UIWidget>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private UIWidget _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public ShopCommodityWidget _003C_003E4__this;

		private float margin;

		public float _003C_003E3__margin;

		private float length;

		public float _003C_003E3__length;

		private UIWidget ellipsis;

		public UIWidget _003C_003E3__ellipsis;

		private float _003Csum_003E5__2;

		private int _003Ccount_003E5__3;

		private int _003Ci_003E5__4;

		UIWidget IEnumerator<UIWidget>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CContentsIconEnumerable_003Ed__25(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			ShopCommodityWidget shopCommodityWidget = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Csum_003E5__2 = 0f;
				_003Ccount_003E5__3 = shopCommodityWidget._contentsItemList.Count;
				_003Ci_003E5__4 = 0;
				break;
			case 1:
			{
				_003C_003E1__state = -1;
				for (int i = _003Ci_003E5__4; i < _003Ccount_003E5__3; i++)
				{
					shopCommodityWidget._contentsItemList[i].gameObject.SetActive(value: false);
				}
				return false;
			}
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__4++;
				break;
			}
			if (_003Ci_003E5__4 < _003Ccount_003E5__3)
			{
				float num2 = shopCommodityWidget._contentsItemList[_003Ci_003E5__4].width;
				if (_003Ci_003E5__4 + 1 < _003Ccount_003E5__3)
				{
					float num3 = shopCommodityWidget._contentsItemList[_003Ci_003E5__4 + 1].width;
					if (_003Csum_003E5__2 + num2 + margin + num3 > length)
					{
						ellipsis.gameObject.SetActive(value: true);
						UILabel uILabel = ellipsis.gameObject.FindComponent<UILabel>("Text");
						if (uILabel != null)
						{
							uILabel.text = $"<weak>+</weak><em>{_003Ccount_003E5__3 - _003Ci_003E5__4}</em>";
						}
						_003C_003E2__current = ellipsis;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003Csum_003E5__2 += num2 + margin;
				_003C_003E2__current = shopCommodityWidget._contentsItemList[_003Ci_003E5__4];
				_003C_003E1__state = 2;
				return true;
			}
			ellipsis.gameObject.SetActive(value: false);
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<UIWidget> IEnumerable<UIWidget>.GetEnumerator()
		{
			_003CContentsIconEnumerable_003Ed__25 _003CContentsIconEnumerable_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CContentsIconEnumerable_003Ed__ = this;
			}
			else
			{
				_003CContentsIconEnumerable_003Ed__ = new _003CContentsIconEnumerable_003Ed__25(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CContentsIconEnumerable_003Ed__.ellipsis = _003C_003E3__ellipsis;
			_003CContentsIconEnumerable_003Ed__.length = _003C_003E3__length;
			_003CContentsIconEnumerable_003Ed__.margin = _003C_003E3__margin;
			return _003CContentsIconEnumerable_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<UIWidget>)this).GetEnumerator();
		}
	}

	[SerializeField]
	private bool _isLargeIcon;

	[CanBeNull]
	[SerializeField]
	private UILabel _titleLabel;

	[CanBeNull]
	[SerializeField]
	private UILabel _remainingTime;

	[CanBeNull]
	[SerializeField]
	private UILabel _priceLabel;

	[CanBeNull]
	[SerializeField]
	private UIWidget _contentsWidget;

	[Range(0f, 1f)]
	[SerializeField]
	private float _contentsAlign;

	[CanBeNull]
	[SerializeField]
	private ShopCommodityContentItemWidget _contentIconBase;

	[CanBeNull]
	[SerializeField]
	private UIWidget _contentEllipsis;

	[CanBeNull]
	[SerializeField]
	private UILabel _descriptionLabel;

	[CanBeNull]
	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private bool _iconSizeLimit;

	[CanBeNull]
	[SerializeField]
	private UILabel _purchaseLimitLabel;

	[CanBeNull]
	[SerializeField]
	private RecommendMarker _recommendObject;

	[CanBeNull]
	[SerializeField]
	private UILabel _sealedLabel;

	[CanBeNull]
	[SerializeField]
	private GameObject _sealedObject;

	[CanBeNull]
	[SerializeField]
	private GameObject _soldoutObject;

	[SerializeField]
	private RectLayout _layout;

	private ListObjectPool<ShopCommodityContentItemWidget> _contentsItemList;

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ShopCommodity;
		if (_contentsWidget != null)
		{
			_layout.AddCompatible(_contentsWidget, UpdateContentsItemLayout);
		}
	}

	public void Set(Durango.Logic.Shop.Commodity commodity)
	{
		Init();
		if (_titleLabel != null)
		{
			_titleLabel.text = commodity.Title;
		}
		SetPriceLabel(commodity);
		if (_remainingTime != null)
		{
			string remainingTime = commodity.GetRemainingTime();
			_remainingTime.text = T._("{0} 남음", remainingTime);
			_remainingTime.gameObject.SetActive(!string.IsNullOrEmpty(remainingTime));
		}
		if (_descriptionLabel != null)
		{
			if (string.IsNullOrEmpty(commodity.Description))
			{
				_descriptionLabel.gameObject.SetActive(value: false);
			}
			else
			{
				_descriptionLabel.gameObject.SetActive(value: true);
				_descriptionLabel.text = commodity.Description;
			}
		}
		if (_recommendObject != null)
		{
			_recommendObject.Set(commodity);
		}
		SetIcon(commodity);
		SetContents(commodity);
		SetPurchaseLimitLabel(commodity);
		SetSealed(commodity);
		UpdateLayout();
	}

	private void SetSealed(Durango.Logic.Shop.Commodity commodity)
	{
		string text = null;
		bool active = false;
		PurchaseLimit purchaseLimit = commodity.Data.PurchaseLimit;
		if (purchaseLimit.MaxCount > 0 && (!commodity.CommodityInfo.MaxPurchasableCount.HasValue || commodity.CommodityInfo.MaxPurchasableCount.Value == 0))
		{
			active = true;
			text = T._("구매 완료!");
		}
		if ((purchaseLimit.PeriodicCountsLimit.Counts > 0 || purchaseLimit.PeriodicLimit.Days > 0) && commodity.CommodityInfo.PeriodicPurchasableAt.HasValue)
		{
			string text2 = TimedeltaFormatter.Format(commodity.CommodityInfo.PeriodicPurchasableAt.Value - Connections.Frontend.GetPredictedServerTime(), 2, "hour");
			text = string.Format("[icon=icon_mainmenu_lock] {0}", T._("{0} 후 구매가능", text2));
		}
		if (string.IsNullOrEmpty(text) && !commodity.IsPurchasable())
		{
			text = ((commodity.Data.PurchaseCondition.MinLevel.HasValue && commodity.Data.PurchaseCondition.MaxLevel.HasValue) ? ("[icon=icon_mainmenu_lock] " + LocalizeUtil.FormatLevel(commodity.Data.PurchaseCondition.MinLevel.Value) + " - " + LocalizeUtil.FormatLevel(commodity.Data.PurchaseCondition.MaxLevel.Value)) : ((!commodity.Data.PurchaseCondition.MinLevel.HasValue) ? "[icon=icon_mainmenu_lock]" : ("[icon=icon_mainmenu_lock] " + LocalizeUtil.FormatLevel(commodity.Data.PurchaseCondition.MinLevel.Value))));
		}
		if (_soldoutObject != null)
		{
			_soldoutObject.gameObject.SetActive(active);
		}
		if (string.IsNullOrEmpty(text))
		{
			if (_sealedObject != null)
			{
				_sealedObject.gameObject.SetActive(value: false);
			}
			return;
		}
		if (_sealedObject != null)
		{
			_sealedObject.gameObject.SetActive(value: true);
		}
		if (_sealedLabel != null)
		{
			_sealedLabel.text = text;
		}
	}

	private void SetPriceLabel(Durango.Logic.Shop.Commodity commodity)
	{
		if (!(_priceLabel == null))
		{
			_priceLabel.text = GetPriceText(commodity);
		}
	}

	private void SetIcon(Durango.Logic.Shop.Commodity commodity)
	{
		if (_iconTexture == null)
		{
			return;
		}
		ItemIcon icon = commodity.GetIcon(_isLargeIcon);
		Vector4 drawRegion = new Vector4(0f, 0f, 1f, 1f);
		if (_iconSizeLimit)
		{
			UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(icon.Main);
			if (sprite != null)
			{
				Vector2 size = _iconTexture.GetSize();
				int num = sprite.paddingLeft + sprite.paddingRight + sprite.width;
				int num2 = sprite.paddingBottom + sprite.paddingTop + sprite.height;
				Vector2 vector = size - new Vector2(num, num2);
				Vector2 pivotOffset = _iconTexture.pivotOffset;
				if (vector.x > 0f)
				{
					vector.x /= size.x;
				}
				else
				{
					vector.x = 0f;
				}
				if (vector.y > 0f)
				{
					vector.y /= size.y;
				}
				else
				{
					vector.y = 0f;
				}
				drawRegion.x = vector.x * pivotOffset.x;
				drawRegion.y = vector.y * pivotOffset.y;
				drawRegion.z = 1f - vector.x * (1f - pivotOffset.x);
				drawRegion.w = 1f - vector.y * (1f - pivotOffset.y);
			}
		}
		_iconTexture.drawRegion = drawRegion;
		_iconTexture.SetIcon(icon);
	}

	private void SetContents(Durango.Logic.Shop.Commodity commodity)
	{
		if (_contentIconBase == null || _contentsWidget == null)
		{
			return;
		}
		List<ContentDescription> contentDescriptions = commodity.ContentDescriptions;
		if (_contentsItemList == null)
		{
			_contentsItemList = new ListObjectPool<ShopCommodityContentItemWidget>();
			_contentsItemList.BaseObject = _contentIconBase;
		}
		_contentsItemList.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(contentDescriptions); i < size; i++)
		{
			ContentDescription contentDescription = contentDescriptions[i];
			if (!contentDescription.OnlyPopup)
			{
				_contentsItemList.GetNext().Set(contentDescription);
			}
		}
		_contentsItemList.EndLoad();
		if (_contentsItemList.Count == 0)
		{
			_contentsWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_contentsWidget.gameObject.SetActive(value: true);
		}
	}

	private Vector2 UpdateContentsItemLayout(float? width, float? height)
	{
		if (_contentsWidget == null)
		{
			return Vector2.zero;
		}
		if (width.HasValue)
		{
			_contentsWidget.width = (int)width.Value;
		}
		float num = _contentsWidget.width;
		Vector2 localSize = _contentsItemList.BaseObject.localSize;
		Vector3[] localCorners = _contentsWidget.localCorners;
		Vector2 vector = new Vector2(Mathf.Lerp(localCorners[0].x, localCorners[2].x, _contentsAlign), Mathf.Lerp(localCorners[0].y, localCorners[2].y, 0.5f));
		float y;
		if (_contentEllipsis == null)
		{
			int num2 = Mathf.FloorToInt(num / localSize.x);
			float breadth = (float)num2 * localSize.x + (float)(num2 - 1) * 5f;
			y = UIUtility.WidgetsGridReposition(_contentsItemList, null, Vector2.down, vector, breadth, localSize, 5f, 5f, 0f, new Vector2(_contentsAlign, 0.5f)).y;
		}
		else
		{
			y = localSize.y;
			UIUtility.WidgetsReposition(ContentsIconEnumerable(_contentEllipsis, num, 5f), Vector3.right, vector, 5f, _contentsAlign);
		}
		float y2 = (height.HasValue ? height.Value : (y + 20f));
		return new Vector2(_contentsWidget.width, y2);
	}

	private IEnumerable<UIWidget> ContentsIconEnumerable([NotNull] UIWidget ellipsis, float length, float margin)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CContentsIconEnumerable_003Ed__25(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__ellipsis = ellipsis,
			_003C_003E3__length = length,
			_003C_003E3__margin = margin
		};
	}

	private void SetPurchaseLimitLabel(Durango.Logic.Shop.Commodity commodity)
	{
		if (_purchaseLimitLabel == null)
		{
			return;
		}
		string text = null;
		PurchaseLimit purchaseLimit = commodity.Data.PurchaseLimit;
		if (purchaseLimit.MaxCount > 0)
		{
			int num = (commodity.CommodityInfo.MaxPurchasableCount.HasValue ? commodity.CommodityInfo.MaxPurchasableCount.Value : 0);
			if (num != 0)
			{
				text = ((purchaseLimit.MaxCount <= num) ? T._("캐릭터당 {0}회", purchaseLimit.MaxCount) : T._("남은 구매 횟수 {0}회", num));
			}
		}
		else if (purchaseLimit.PeriodicCountsLimit.Counts > 0)
		{
			if (!commodity.CommodityInfo.PeriodicPurchasableAt.HasValue)
			{
				int num2 = (commodity.CommodityInfo.PeriodicPurchasableCount.HasValue ? commodity.CommodityInfo.PeriodicPurchasableCount.Value : 0);
				string text2 = ((purchaseLimit.PeriodicCountsLimit.Counts <= 1) ? string.Empty : T._(" ({0}남음)", num2));
				text = T._("{0}일 {1}회", purchaseLimit.PeriodicCountsLimit.Days, purchaseLimit.PeriodicCountsLimit.Counts) + text2;
			}
		}
		else if (purchaseLimit.PeriodicLimit.Days > 0 && !commodity.CommodityInfo.PeriodicPurchasableAt.HasValue)
		{
			text = T._("{0}일 {1}회", purchaseLimit.PeriodicLimit.Days, 1);
		}
		if (string.IsNullOrEmpty(text))
		{
			_purchaseLimitLabel.gameObject.SetActive(value: false);
			return;
		}
		_purchaseLimitLabel.gameObject.SetActive(value: true);
		_purchaseLimitLabel.text = text;
	}

	public static string GetPriceText(Durango.Logic.Shop.Commodity commodity)
	{
		if (commodity.GetQuestPurchase(CommodityCondition.Type.Level) != null)
		{
			return T._("진행중");
		}
		if (InventorySystem.Wallet.PurchasableVoucherCount(commodity) <= 0)
		{
			return commodity.GetCurrencyText(hasDiscountRatio: true);
		}
		Voucher voucher = SingletonDict<string, Voucher>.Instance.Get(commodity.Data.VoucherId);
		if (string.IsNullOrEmpty(voucher.Icon))
		{
			return null;
		}
		return $"{voucher.GetIconText()} {commodity.Data.VoucherAmount}";
	}

	public void UpdateLayout()
	{
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	Vector2 RectLayout.ICompatible.UpdateLayout(float? x, float? y)
	{
		return _layout.UpdateLayout(x, y);
	}
}
