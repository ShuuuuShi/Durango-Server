using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Logic.Market;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Market;
using UnityEngine;

namespace Durango.UI;

public class CommodityListWidget : AnimationWidget, IUIInitializable, IScreenResizeReceiver
{
	[SerializeField]
	private UIPanel _parent;

	[SerializeField]
	private MarketCategoriesWidget _categoriesWidget;

	[SerializeField]
	private MarketSubCatecoriesWidget _subCategoriesWidget;

	[SerializeField]
	private CommodityList _commodityList;

	[SerializeField]
	private CommodityListBottomBar _bottomBar;

	[SerializeField]
	private MarketSearchWidget _searchWidget;

	[SerializeField]
	private KWidgetScrollView _mainScrollView;

	[SerializeField]
	private SearchInfoWidget _searchInfo;

	[SerializeField]
	private SelectableWidget _reSearchButton;

	[SerializeField]
	private RectLayout _commodityListLayout;

	private bool _isOpened;

	private int _currentPageIndex;

	private readonly Durango.Logic.Market.Commodities _commodities = new Durango.Logic.Market.Commodities();

	private readonly SearchOption _searchOption = new SearchOption();

	void IUIInitializable.Init()
	{
		foreach (UIWidget widget in _mainScrollView.Widgets)
		{
			widget.gameObject.SetActive(value: true);
		}
		_commodities.OnRequestGoodsList += OnRequestGoodsList;
		_commodities.GoodsListUpdated += OnUpdatedGoodsList;
		_categoriesWidget.MainCategorySelected += OnSelectMainCategory;
		_categoriesWidget.SearchSelected += OnResetAndSearch;
		SelectableWidget reSearchButton = _reSearchButton;
		reSearchButton.Clicked = (Action)Delegate.Combine(reSearchButton.Clicked, new Action(OnReSearch));
		_subCategoriesWidget.SubCategorySelected += OnSelectSubCategory;
		_commodityList.Init();
		_commodityList.CommoditySelected += OnSelectCommodity;
		_bottomBar.BuyButtonClicked += OnBuyCommodity;
		_bottomBar.PrototypeSearchClicked += OnPrototypeSearch;
		_searchWidget.Enabled += delegate(bool enable)
		{
			_parent.alpha = ((!enable) ? 1f : 0f);
		};
		_searchWidget.SearchClicked += delegate
		{
			SearchCommodities(instant: false);
		};
		_commodityList.SetFilter(delegate(Commodity commodity)
		{
			if (commodity.State != ProductState.Registered)
			{
				return false;
			}
			ItemData item = commodity.GetItem();
			return item == null || _searchOption.Filter(item.Name);
		});
		_searchInfo.SearchClicked += delegate
		{
			SearchCommodities(instant: false);
		};
		_parent.gameObject.SetActive(value: true);
		_commodities.Request.Condition = _commodityList.GetSortCondition();
	}

	public void Open(bool instant)
	{
		bool isOpened = _isOpened;
		_searchOption.Clear();
		_Open(instant);
		ShowCategoryPage(!isOpened);
	}

	public void Open(string prototype, bool instant)
	{
		_searchOption.Clear();
		_searchOption.Prototype = prototype;
		_Open(instant);
		SearchCommodities(instant: true);
	}

	public void Open(string prototype, int prototypeLevel, string itemTag, bool instant)
	{
		_searchOption.Clear();
		if (!string.IsNullOrEmpty(prototype))
		{
			_searchOption.Prototype = prototype;
			_searchOption.Level = new RangePredicate
			{
				Min = prototypeLevel,
				Max = null
			};
		}
		if (!string.IsNullOrEmpty(itemTag))
		{
			HashSet<TagFilterBase> hashSet = new HashSet<TagFilterBase>();
			hashSet.Add(new SingularTagFilter(itemTag, 1));
			_searchOption.Tags = hashSet;
		}
		_Open(instant);
		SearchCommodities(instant: true);
	}

	public void Open(OrTagFilter tagFilter, OrTagFilter material, int level, bool instant)
	{
		_searchOption.Tags.Clear();
		if (tagFilter != null)
		{
			_searchOption.Tags.Add(tagFilter);
		}
		_searchOption.Materials.Clear();
		if (material != null)
		{
			_searchOption.Materials.Add(material);
		}
		if (level > 1)
		{
			_searchOption.Level.Min = level;
		}
		_Open(instant);
		SearchCommodities(instant: true);
	}

	private void _Open(bool instant)
	{
		_isOpened = true;
		base.gameObject.SetActive(value: true);
		if (instant)
		{
			SetAlpha(1f, useTween: false);
			return;
		}
		base.Delay = 0.2f;
		base.Alpha = 1f;
	}

	public bool Back()
	{
		if (!_isOpened)
		{
			return true;
		}
		if (_searchWidget.IsOpen)
		{
			_searchWidget.Close();
			return false;
		}
		if (_currentPageIndex > 0)
		{
			ShowCategoryPage(instant: false);
			return false;
		}
		Close();
		return true;
	}

	public void Close(bool instant = false)
	{
		_isOpened = false;
		if (instant)
		{
			base.gameObject.SetActive(value: false);
			SetAlpha(0f, useTween: false);
		}
		else
		{
			base.Delay = 0f;
			base.Alpha = 0f;
		}
	}

	private void ShowCategoryPage(bool instant)
	{
		ShowPage(0, instant);
	}

	private void ShowCommoditiesPage(bool instant)
	{
		ShowPage(1, instant);
	}

	private void ShowPage(int index, bool instant)
	{
		_currentPageIndex = index;
		if (index == 1)
		{
			int num = ((!UIManager.IsPortraitWidget(base.gameObject) && _commodities.SearchOption != null && _commodities.SearchOption.MainCategory != null) ? (base.Widget.width - _subCategoriesWidget.Widget.width - _mainScrollView.Margin) : base.Widget.width);
			if (_commodityList.Widget.width != num)
			{
				_commodityList.Widget.width = num;
				_mainScrollView.UpdateLayout();
			}
			_commodityListLayout.GetParentWidget().UpdateAnchors();
			_commodityListLayout.UpdateLayout();
			UIUtility.UpdateAnchors(_commodityList.transform);
		}
		_mainScrollView.MoveToNode(index, instant);
		Refersh();
	}

	private void OnRequestGoodsList(bool isReset)
	{
		if (_isOpened && isReset)
		{
			_commodityList.SetLoading();
		}
	}

	private void OnUpdatedGoodsList()
	{
		if (_isOpened)
		{
			_commodityList.Set(_commodities);
			if (_commodityList.Selected == null && _bottomBar.enabled)
			{
				_bottomBar.Hide();
			}
		}
	}

	private void OnBuyCommodity()
	{
		Commodity selected = _commodityList.Selected;
		if (selected != null)
		{
			ItemData item = selected.GetItem();
			if (item != null)
			{
				UIManager.MessageBox.Show(T._("<t_stone> {0:으로} <em>{1}</em>{1:-을} 구매합니다", selected.Price.ToString("N0", T.Culture), item.Name), CommodityBought);
			}
		}
	}

	private void CommodityBought(bool ok)
	{
		if (ok)
		{
			_commodities.Buy(_commodityList.Selected);
		}
	}

	private void OnPrototypeSearch()
	{
		Commodity selected = _commodityList.Selected;
		if (selected != null)
		{
			ItemData item = selected.GetItem();
			if (item != null)
			{
				Open(item.PrototypeId, instant: false);
			}
		}
	}

	private void OnSelectMainCategory([CanBeNull] Category.Main main)
	{
		_searchOption.Clear();
		_searchOption.MainCategory = main;
		if (main == null)
		{
			SearchCommodities(instant: false);
		}
		else
		{
			Refersh();
		}
	}

	private void OnSelectSubCategory([CanBeNull] Category.Sub sub)
	{
		if (_searchOption.MainCategory != null)
		{
			_searchOption.SubCategory = sub;
		}
		SearchCommodities(instant: false);
	}

	private void Refersh()
	{
		if (_currentPageIndex == 0)
		{
			_searchOption.ClearExceptCategory();
			_categoriesWidget.SelectCategory(_searchOption.MainCategory);
			_subCategoriesWidget.SetCategory(_searchOption.MainCategory);
		}
		else
		{
			_categoriesWidget.SelectCategory(_searchOption.MainCategory);
			_subCategoriesWidget.SetCategory(_searchOption.MainCategory, _searchOption.SubCategory);
		}
	}

	private void OnResetAndSearch()
	{
		_searchOption.Clear();
		_searchWidget.Open(_searchOption);
		Refersh();
	}

	private void OnReSearch()
	{
		_searchWidget.Open(_searchOption);
	}

	private void OnSelectCommodity(Commodity commodity)
	{
		_bottomBar.Show(commodity, _commodityList.UpdateCommodity);
	}

	private void SearchCommodities(bool instant)
	{
		_searchInfo.Set(_searchOption);
		_commodityList.ResetPosition();
		_commodities.SearchOption = _searchOption;
		_commodities.Request.Type = ProductType.Searched;
		_commodities.Request.Condition = _commodityList.GetSortCondition();
		_commodities.Get(reset: true);
		ShowCommoditiesPage(instant);
		Refersh();
	}

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		Point2 point = new Point2(base.Widget.width, base.Widget.height);
		foreach (UIWidget widget in _mainScrollView.Widgets)
		{
			widget.SetDimensions(point.x, point.y);
		}
		_mainScrollView.UpdateViewSize();
		_mainScrollView.ResetPosition();
		_commodityListLayout.GetParentWidget().UpdateAnchors();
		_commodityListLayout.UpdateLayout();
	}

	static CommodityListWidget()
	{
	}
}
