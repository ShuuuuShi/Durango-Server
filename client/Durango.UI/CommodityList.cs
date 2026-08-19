using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Clusters;
using Durango.Logic.Market;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Market;
using UnityEngine;

namespace Durango.UI;

public class CommodityList : MonoBehaviour, IScreenResizeReceiver
{
	private const SortableColumnWidget<ProductSortField>.State DefaultSortState = SortableColumnWidget<ProductSortField>.State.Ascending;

	private const ProductSortField DefaultSortField = ProductSortField.Price;

	private static readonly ProductSortField[] SortFieldsByTime;

	[SerializeField]
	private RectLayoutComponent _headerLayout;

	[SerializeField]
	private GameObject[] _onlyLandscape;

	[SerializeField]
	private GameObject[] _onlyOnline;

	[SerializeField]
	private KInfiniteScrollView _commodityList;

	[SerializeField]
	private AnimationWidget _noData;

	[SerializeField]
	private UILabel _resultGuide;

	[SerializeField]
	private AnimationWidget _loadingIcon;

	[CanBeNull]
	[SerializeField]
	private CommoditySelectableColumn _historyColumn;

	[CanBeNull]
	[SerializeField]
	private CommoditySelectableColumn _receiveColumn;

	[SerializeField]
	private string _prefKey;

	private Func<Commodity, bool> _filterFunc;

	[CanBeNull]
	private Durango.Logic.Market.Commodities _commodities;

	private KInfiniteScrollView.View<Commodity, CommodityNode> _commodityView;

	private readonly List<Commodity> _filteredList = new List<Commodity>();

	private ProductType _productType;

	private CommoditySelectableColumn[] _selectableColumns;

	private SortableColumnWidget<ProductSortField>.State _savedSortState;

	private ProductSortField _savedSortField;

	private string _saveSortStateKey;

	private string _saveSortFieldKey;

	private UIWidget _widget;

	private bool _isInit;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	[CanBeNull]
	public Commodity Selected { get; private set; }

	public event Action<Commodity> CommoditySelected;

	private void OnDisable()
	{
		OnSelectCommodity(null);
		_commodityList.ResetPosition();
	}

	public void ResetPosition()
	{
		_commodityList.ResetPosition();
	}

	public void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		InitSelectableColumns();
		_commodityView = _commodityList.Initialize(delegate(CommodityNode node, Commodity data)
		{
			node.Set(data, _productType);
			node.Selected = Selected != null && data != null && Selected.Id == data.Id;
		}, delegate(CommodityNode node)
		{
			node.Clicked = OnClickCommodityNode;
		});
		_commodityList.DragFinishedOnLast += delegate
		{
			if (_commodities != null)
			{
				_commodities.Get(reset: false);
			}
		};
		UpdateItemsOnOnline();
	}

	public void SetFilter(Func<Commodity, bool> filter)
	{
		_filterFunc = filter;
		UpdateList();
	}

	private void OnClickCommodityNode()
	{
		CommodityNode commodityNode = Selectable.Current as CommodityNode;
		if (!(commodityNode == null))
		{
			Commodity commodity = commodityNode.Data;
			if (commodity == Selected)
			{
				commodity = null;
			}
			OnSelectCommodity(commodity);
			int index = _commodityView.IndexOf(commodityNode);
			_commodityList.MoveToVisibleArea(index, instant: false);
		}
	}

	private void OnSelectCommodity(Commodity commodity)
	{
		string text = ((Selected != null) ? Selected.Id : string.Empty);
		Selected = null;
		int num = -1;
		if (commodity != null && _commodities != null)
		{
			for (int i = 0; i < _commodities.Goods.Count; i++)
			{
				if (_commodities.Goods[i].Id == commodity.Id)
				{
					num = i;
					Selected = _commodities.Goods[i];
				}
			}
		}
		LinkedList<CommodityNode> linkedList = ((_commodityView != null) ? _commodityView.List : null);
		string text2 = ((Selected != null) ? Selected.Id : string.Empty);
		if (linkedList != null)
		{
			foreach (CommodityNode item in linkedList)
			{
				item.Selected = item.Data != null && item.Data.Id == text2;
			}
		}
		if (text != text2)
		{
			if (this.CommoditySelected != null)
			{
				this.CommoditySelected(commodity);
			}
			if (num != -1)
			{
				_commodityList.MoveToVisibleArea(num, instant: false);
			}
		}
	}

	public void SetLoading()
	{
		_commodityList.Panel.alpha = 0f;
		if (_noData != null)
		{
			_noData.SetAlpha(0f, useTween: false);
		}
		if (_loadingIcon != null)
		{
			_loadingIcon.SetAlpha(0f, useTween: false);
			_loadingIcon.gameObject.SetActive(value: true);
			_loadingIcon.Alpha = 1f;
		}
		OnSelectCommodity(null);
	}

	public void Set(Durango.Logic.Market.Commodities commodities)
	{
		_commodities = commodities;
		GameSystem<MarketSystem>.Instance().GetFavoriteProduct(delegate
		{
			UpdateList();
			UpdateSortCondition();
		});
	}

	public void SetProductType(ProductType type)
	{
		_productType = type;
		SetHistoryColumn(type);
		SetReceiveColumn(type);
		bool clickEnabled = _productType != ProductType.Favorites;
		int i = 0;
		for (int size = KUtility.GetSize(_selectableColumns); i < size; i++)
		{
			_selectableColumns[i].ClickEnabled = clickEnabled;
		}
		RefreshSelectableColumns();
		if ((bool)_headerLayout)
		{
			_headerLayout.UpdateLayout();
			UIUtility.UpdateAnchors(_headerLayout.transform);
		}
	}

	public void PaymentReceived(string productId = null)
	{
		LinkedList<CommodityNode> linkedList = ((_commodityView != null) ? _commodityView.List : null);
		if (linkedList == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(productId))
		{
			if (_commodities != null)
			{
				for (int i = 0; i < _commodities.Goods.Count; i++)
				{
					Commodity commodity = _commodities.Goods[i];
					if (commodity.State == ProductState.PaymentPending)
					{
						commodity.State = ProductState.PaymentReceived;
					}
				}
			}
			{
				foreach (CommodityNode item in linkedList)
				{
					item.RefreshReceiveButton();
				}
				return;
			}
		}
		foreach (CommodityNode item2 in linkedList)
		{
			if (item2.Data.Id == productId)
			{
				if (item2.Data.State == ProductState.PaymentPending)
				{
					item2.Data.State = ProductState.PaymentReceived;
				}
				item2.RefreshReceiveButton();
				break;
			}
		}
	}

	private void SetHistoryColumn(ProductType type)
	{
		if (!(_historyColumn == null))
		{
			switch (type)
			{
			case ProductType.Purchased:
				_historyColumn.SetText(T._("구매시간"));
				_historyColumn.Value = ProductSortField.PurchasedAt;
				break;
			case ProductType.Sold:
				_historyColumn.SetText(T._("판매시간"));
				_historyColumn.Value = ProductSortField.PurchasedAt;
				break;
			case ProductType.Expired:
				_historyColumn.SetText(T._("수령기한"));
				_historyColumn.Value = ProductSortField.ExpiresAt;
				break;
			default:
				_historyColumn.SetText(T._("남은시간"));
				_historyColumn.Value = ProductSortField.ExpiresAt;
				break;
			}
		}
	}

	private void SetReceiveColumn(ProductType type)
	{
		if (!(_receiveColumn == null))
		{
			_receiveColumn.gameObject.SetActive(type == ProductType.Sold);
		}
	}

	public SortCondition GetSortCondition()
	{
		SortCondition result = default(SortCondition);
		result.Field = _savedSortField;
		result.Ascending = _savedSortState == SortableColumnWidget<ProductSortField>.State.Ascending;
		return result;
	}

	public void SetScrollEndPadding(int padding)
	{
		_commodityList.EndPadding = padding;
	}

	private void UpdateList()
	{
		List<Commodity> list;
		if (_commodities == null)
		{
			list = null;
		}
		else
		{
			_filteredList.Clear();
			for (int i = 0; i < _commodities.Goods.Count; i++)
			{
				Commodity commodity = _commodities.Goods[i];
				if (commodity != null && commodity.GetItem() != null && (_filterFunc == null || _filterFunc(commodity)))
				{
					_filteredList.Add(commodity);
				}
			}
			list = _filteredList;
		}
		_commodityView.SetList(list);
		OnSelectCommodity(Selected);
		TweenAlpha.Begin(_commodityList.ScrollView.gameObject, 0.2f, 1f);
		if (_noData != null)
		{
			_noData.gameObject.SetActive(_commodityView.Count == 0);
			_noData.Alpha = 1f;
		}
		UpdateResultGuide();
		if (_loadingIcon != null)
		{
			_loadingIcon.Alpha = 0f;
		}
		_commodityList.Reposition();
	}

	private void UpdateResultGuide()
	{
		if (!(_resultGuide == null))
		{
			bool flag = _commodityView.Count > 0;
			if (_commodities != null && _commodities.Request.NoMore)
			{
				_resultGuide.text = ((!flag) ? T._("등록된 상품이 없습니다.") : T._("더 이상 등록된 상품이 없습니다."));
			}
			else
			{
				_resultGuide.text = T._("위로 밀어서 상품 더 찾기");
			}
			float y = (0f - _commodityList.Panel.GetViewSize().y) / 2f;
			if (flag)
			{
				int index = _commodityView.Count - 1;
				y = 0f - _commodityView.GetNodeOffset(index) - 150f;
			}
			_resultGuide.transform.localPosition = new Vector3(0f, y, 0f);
			_resultGuide.gameObject.SetActive(value: true);
		}
	}

	private void InitSelectableColumns()
	{
		_saveSortStateKey = "MarketSort_State_" + _prefKey;
		_saveSortFieldKey = "MarketSort_Field_" + _prefKey;
		LoadSortPref();
		_selectableColumns = base.gameObject.GetComponentsInChildren<CommoditySelectableColumn>();
		int i = 0;
		for (int size = KUtility.GetSize(_selectableColumns); i < size; i++)
		{
			CommoditySelectableColumn obj = _selectableColumns[i];
			obj.Clicked = (Action<ProductSortField>)Delegate.Combine(obj.Clicked, new Action<ProductSortField>(SelectableColumns_Clicked));
		}
		RefreshSelectableColumns();
	}

	private bool ContainsColumn(ProductSortField sortType)
	{
		int i = 0;
		for (int size = KUtility.GetSize(_selectableColumns); i < size; i++)
		{
			CommoditySelectableColumn commoditySelectableColumn = _selectableColumns[i];
			if (commoditySelectableColumn.gameObject.activeSelf && commoditySelectableColumn.Value == sortType)
			{
				return true;
			}
		}
		return false;
	}

	private ProductSortField GetAcceptableSortField(ProductSortField previous)
	{
		if (ContainsColumn(previous))
		{
			return previous;
		}
		if (SortFieldsByTime.Contains(previous))
		{
			foreach (ProductSortField item in SortFieldsByTime.Where((ProductSortField f) => f != previous))
			{
				if (ContainsColumn(item))
				{
					return item;
				}
			}
			return ProductSortField.Price;
		}
		return ProductSortField.Price;
	}

	private void RefreshSelectableColumns()
	{
		ProductSortField acceptableSortField = GetAcceptableSortField(_savedSortField);
		if (_savedSortField != acceptableSortField)
		{
			SetSortValues(_savedSortState, acceptableSortField);
			SaveSortPref();
			UpdateSortCondition();
		}
		int i = 0;
		for (int size = KUtility.GetSize(_selectableColumns); i < size; i++)
		{
			CommoditySelectableColumn obj = _selectableColumns[i];
			obj.SetState((obj.Value == _savedSortField) ? _savedSortState : SortableColumnWidget<ProductSortField>.State.None);
		}
	}

	private void SelectableColumns_Clicked(ProductSortField sortType)
	{
		if (_commodities == null || _commodities.Goods == null || _commodities.Goods.Count == 0 || _commodities.Request.IsLoading)
		{
			return;
		}
		int i = 0;
		for (int size = KUtility.GetSize(_selectableColumns); i < size; i++)
		{
			CommoditySelectableColumn commoditySelectableColumn = _selectableColumns[i];
			if (commoditySelectableColumn.Value == sortType)
			{
				SortCommodity(commoditySelectableColumn);
			}
			else
			{
				((SortableColumnWidget<ProductSortField>)commoditySelectableColumn).SetState(SortableColumnWidget<ProductSortField>.State.None);
			}
		}
	}

	private void SortCommodity(CommoditySelectableColumn columnObj)
	{
		SetSortValues(columnObj.NextState(), columnObj.Value);
		SaveSortPref();
		UpdateSortCondition();
		_commodities.Get(reset: true);
	}

	private void UpdateSortCondition()
	{
		if (_commodities != null)
		{
			_commodities.Request.Condition = GetSortCondition();
		}
	}

	private void LoadSortPref()
	{
		SetSortValues((SortableColumnWidget<ProductSortField>.State)Preferences.GetInt(_saveSortStateKey), (ProductSortField)Preferences.GetInt(_saveSortFieldKey, -1));
	}

	private void SaveSortPref()
	{
		Preferences.SetInt(_saveSortStateKey, (int)_savedSortState);
		Preferences.SetInt(_saveSortFieldKey, (int)_savedSortField);
	}

	private void SetSortValues(SortableColumnWidget<ProductSortField>.State state, ProductSortField field)
	{
		_savedSortState = state;
		_savedSortField = field;
		if (_savedSortState == SortableColumnWidget<ProductSortField>.State.None)
		{
			_savedSortState = SortableColumnWidget<ProductSortField>.State.Ascending;
		}
		if (_savedSortField == ProductSortField.Invalid)
		{
			_savedSortField = ProductSortField.Price;
		}
	}

	public void UpdateCommodity(Commodity commodity)
	{
		LinkedList<CommodityNode> linkedList = ((_commodityView != null) ? _commodityView.List : null);
		if (linkedList == null)
		{
			return;
		}
		foreach (CommodityNode item in linkedList)
		{
			if (item.Data.Id == commodity.Id)
			{
				item.Set(commodity, _productType);
			}
		}
	}

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		UpdateItemsOnScreenChanged();
	}

	private void UpdateItemsOnScreenChanged()
	{
		bool flag = UIManager.IsPortraitWidget(base.gameObject);
		int i = 0;
		for (int size = KUtility.GetSize(_onlyLandscape); i < size; i++)
		{
			_onlyLandscape[i].SetActive(!flag);
		}
		if ((bool)_headerLayout)
		{
			_headerLayout.UpdateLayout();
			UIUtility.UpdateAnchors(_headerLayout.transform);
		}
	}

	private void UpdateItemsOnOnline()
	{
		bool active = GameManager.ClusterMode == Mode.Editable || GameManager.ClusterMode == Mode.SingleMode;
		int i = 0;
		for (int size = KUtility.GetSize(_onlyOnline); i < size; i++)
		{
			_onlyOnline[i].SetActive(active);
		}
		if ((bool)_headerLayout)
		{
			_headerLayout.UpdateLayout();
			UIUtility.UpdateAnchors(_headerLayout.transform);
		}
	}

	static CommodityList()
	{
		SortFieldsByTime = new ProductSortField[3]
		{
			ProductSortField.RegisteredAt,
			ProductSortField.ExpiresAt,
			ProductSortField.PurchasedAt
		};
	}
}
