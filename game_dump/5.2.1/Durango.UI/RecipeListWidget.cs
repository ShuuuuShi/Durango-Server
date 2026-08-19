using System;
using System.Collections.Generic;
using System.Linq;
using Building;
using Crafting;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class RecipeListWidget : MonoBehaviour, IUIInitializable
{
	public class SubList
	{
		private readonly List<RecipeSubListWidget.Data> _list = new List<RecipeSubListWidget.Data>();

		private readonly bool _inEntireCategory;

		public int Count => _list.Count;

		public bool IsFavorites { get; private set; }

		public string Text { get; set; }

		public SubList(bool inEntireCategory, bool isFavorites = false)
		{
			_inEntireCategory = inEntireCategory;
			IsFavorites = isFavorites;
		}

		public void AddItem(CategoryItem item)
		{
			RecipeSubListWidget.Data item2 = default(RecipeSubListWidget.Data);
			item2.Item = item;
			_list.Add(item2);
		}

		public CategoryItem GetItem(string id, string searchText)
		{
			foreach (RecipeSubListWidget.Data item in EnumerateItems(searchText))
			{
				if (item.Item.Id == id)
				{
					return item.Item;
				}
			}
			return null;
		}

		public int IndexOf(string id, string searchText)
		{
			int num = 0;
			foreach (RecipeSubListWidget.Data item in EnumerateItems(searchText))
			{
				if (item.Item.Id == id)
				{
					return num;
				}
				num++;
			}
			return -1;
		}

		public bool ContainsFilteredItem(string searchText)
		{
			if (!string.IsNullOrEmpty(searchText))
			{
				return _list.Any((RecipeSubListWidget.Data data) => PredicateSearchFilter(data, searchText));
			}
			return true;
		}

		public IEnumerable<RecipeSubListWidget.Data> EnumerateItems(string searchText)
		{
			if (string.IsNullOrEmpty(searchText))
			{
				return _list;
			}
			return _list.Where((RecipeSubListWidget.Data data) => PredicateSearchFilter(data, searchText));
		}

		public static int SortComparison(KeyValuePair<string, SubList> x, KeyValuePair<string, SubList> y)
		{
			if (y.Value.IsFavorites != x.Value.IsFavorites)
			{
				return (y.Value.IsFavorites ? 1 : 0) - (x.Value.IsFavorites ? 1 : 0);
			}
			return string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase);
		}

		private bool PredicateSearchFilter(RecipeSubListWidget.Data data, string searchText)
		{
			if (_inEntireCategory && !IsFavorites)
			{
				if (!data.Item.Like)
				{
					return SearchFilter(data.Item.Name, searchText);
				}
				return false;
			}
			return SearchFilter(data.Item.Name, searchText);
		}

		private static bool SearchFilter(string name, string searchText)
		{
			return name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1;
		}
	}

	public struct SelectInfo
	{
		public RecipeSystem.RecipeType RecipeType;

		public string Id;

		public bool InFavorites;

		public SelectInfo([NotNull] CategoryItem item, bool inFavorites = false)
		{
			if (item is Recipe)
			{
				RecipeType = RecipeSystem.RecipeType.Crafting;
			}
			else if (item is Blueprint)
			{
				RecipeType = RecipeSystem.RecipeType.Building;
			}
			else
			{
				RecipeType = RecipeSystem.RecipeType.None;
			}
			Id = item.Id;
			InFavorites = inFavorites;
		}
	}

	[SerializeField]
	private RecipeFilterWidget _recipeFilterWidget;

	[SerializeField]
	private KInfiniteScrollView _recipeScrollView;

	[SerializeField]
	private RecipeSubListWidget _recipeSubListWidgetBase;

	[SerializeField]
	private UILabel _recipeNoData;

	[SerializeField]
	private bool _toggleRecipeSelection;

	private int _widthRecipeScrollView;

	private readonly List<KeyValuePair<string, SubList>> _listForRecipeView = new List<KeyValuePair<string, SubList>>();

	private KInfiniteScrollView.View<KeyValuePair<string, SubList>, RecipeSubListWidget> _recipeView;

	public SelectInfo? SelectedRecipe { get; private set; }

	public event Action RecipeSelected;

	public void ClearSearchText()
	{
		_recipeFilterWidget.SearchText = string.Empty;
	}

	public void SetRecipes([NotNull] IEnumerable<KeyValuePair<string, SubList>> subLists, bool resetPosition)
	{
		_widthRecipeScrollView = (int)_recipeScrollView.ViewSize.x;
		_recipeFilterWidget.SetRecipeItems(subLists);
		SelectedRecipe = null;
		RefreshRecipeItems(resetPosition);
	}

	public RecipeItemWidget FindRecipe(string id)
	{
		foreach (RecipeSubListWidget item in _recipeView.List)
		{
			RecipeItemWidget recipeItemWidget = item.FindRecipeComponent(id);
			if (recipeItemWidget != null)
			{
				return recipeItemWidget;
			}
		}
		return null;
	}

	public bool SelectRecipe(SelectInfo? info)
	{
		if (GetCategoryItem(info) != null)
		{
			RefreshSelection(info);
			return true;
		}
		RefreshSelection();
		return false;
	}

	public void ScrollToRecipe(SelectInfo info, bool instant)
	{
		int num = 0;
		foreach (KeyValuePair<string, SubList> item in _listForRecipeView)
		{
			if (item.Value.IsFavorites == info.InFavorites)
			{
				int num2 = item.Value.IndexOf(info.Id, _recipeFilterWidget.SearchText);
				if (num2 != -1)
				{
					float recipeItemOffset = GetRecipeItemOffset(num, num2);
					recipeItemOffset -= (float)_recipeNoData.height / 3f;
					_recipeScrollView.MoveTo(recipeItemOffset, instant);
					break;
				}
			}
			num++;
		}
	}

	void IUIInitializable.Init()
	{
		_recipeView = _recipeScrollView.Initialize(delegate(RecipeSubListWidget widget, KeyValuePair<string, SubList> subList)
		{
			widget.SetRecipes(_widthRecipeScrollView, subList.Value, _recipeFilterWidget.SearchText, SelectedRecipe);
		}, delegate(RecipeSubListWidget widget)
		{
			widget.Init();
			widget.RecipeClicked += RecipeSubListWidget_RecipeClicked;
		});
		_recipeNoData.gameObject.SetActive(value: false);
		_recipeFilterWidget.Init();
		_recipeFilterWidget.SearchTextSubmitted += RecipeFilterWidget_SearchTextSubmitted;
	}

	private void RefreshRecipeItems(bool resetPosition)
	{
		_listForRecipeView.Clear();
		_listForRecipeView.AddRange(_recipeFilterWidget.EnumerateFilteredLists());
		_recipeView.SetList(_listForRecipeView);
		_recipeScrollView.UpdateLayout();
		if (resetPosition)
		{
			_recipeScrollView.MoveTo(0f, instant: true);
		}
		RefreshRecipeNoData();
	}

	private void RefreshRecipeNoData()
	{
		if (_listForRecipeView.Count == 0)
		{
			_recipeNoData.text = ((!string.IsNullOrEmpty(_recipeFilterWidget.SearchText)) ? T._("검색 결과가 없습니다") : T._("도안이 없습니다"));
			_recipeNoData.gameObject.SetActive(value: true);
		}
		else
		{
			_recipeNoData.gameObject.SetActive(value: false);
		}
	}

	private void RefreshSelection(SelectInfo? info = null)
	{
		SelectedRecipe = info;
		foreach (RecipeSubListWidget item in _recipeView.List)
		{
			item.RefreshSelectState(SelectedRecipe);
		}
		if (this.RecipeSelected != null)
		{
			this.RecipeSelected();
		}
	}

	private CategoryItem GetCategoryItem(SelectInfo? info)
	{
		if (info.HasValue)
		{
			foreach (KeyValuePair<string, SubList> item2 in _listForRecipeView)
			{
				if (item2.Value.IsFavorites == info.Value.InFavorites)
				{
					CategoryItem item = item2.Value.GetItem(info.Value.Id, _recipeFilterWidget.SearchText);
					if (item != null)
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	private float GetRecipeItemOffset(int indexNode, int indexRecipe)
	{
		return _recipeScrollView.GetNodeOffset(indexNode) + (float)_recipeSubListWidgetBase.TitleHeight + (float)(_recipeSubListWidgetBase.RecipeHeight * (indexRecipe / _recipeSubListWidgetBase.WidgetCountPerLine));
	}

	private void RecipeFilterWidget_SearchTextSubmitted()
	{
		RefreshRecipeItems(resetPosition: true);
		if (GetCategoryItem(SelectedRecipe) == null)
		{
			RefreshSelection();
		}
	}

	private void RecipeSubListWidget_RecipeClicked(RecipeItemWidget widget, bool inFavorites)
	{
		if (widget != null)
		{
			RefreshSelection((!_toggleRecipeSelection || !SelectedRecipe.HasValue || !(SelectedRecipe.Value.Id == widget.Item.Id)) ? new SelectInfo?(new SelectInfo(widget.Item, inFavorites)) : null);
		}
	}
}
