using System;
using System.Collections.Generic;
using Crafting;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class RecipeSubListWidget : MonoBehaviour
{
	public struct Data
	{
		[NotNull]
		public CategoryItem Item;

		public bool? CanCraft;
	}

	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _recipesContainer;

	[SerializeField]
	private RecipeItemWidget _recipeItemBase;

	[SerializeField]
	private UISprite _recipeSeparatorBase;

	private bool _isInit;

	private readonly List<Data> _recipeDataList = new List<Data>();

	private bool _isFavorites;

	private ListObjectPool<RecipeItemWidget> _recipes;

	private ListObjectPool<UISprite> _recipeSeparators;

	[SerializeField]
	private int _widgetCountPerLine;

	public int WidgetCountPerLine
	{
		get
		{
			if (_widgetCountPerLine == 0)
			{
				_widgetCountPerLine = (UIManager.IsPortraitWidget(base.gameObject) ? 1 : 2);
			}
			return _widgetCountPerLine;
		}
	}

	public int TitleHeight => _titleWidget.height;

	public int RecipeHeight => _recipeItemBase.Widget.height;

	public event Action<RecipeItemWidget, bool> RecipeClicked;

	public void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_recipes = new ListObjectPool<RecipeItemWidget>();
			_recipes.BaseObject = _recipeItemBase;
			_recipeSeparators = new ListObjectPool<UISprite>();
			_recipeSeparators.BaseObject = _recipeSeparatorBase;
			_recipes.Init(delegate(RecipeItemWidget node)
			{
				node.Clicked = (Action)Delegate.Combine(node.Clicked, new Action(RecipeItemWidget_Clicked));
			});
		}
	}

	public void SetRecipes(int width, RecipeListWidget.SubList subList, string searchText, RecipeListWidget.SelectInfo? selectedRecipeItem)
	{
		_isFavorites = subList.IsFavorites;
		_recipeDataList.Clear();
		_recipeDataList.AddRange(subList.EnumerateItems(searchText));
		SetWidgetWidth(_titleWidget, width);
		SetWidgetWidth(_recipesContainer, width, updateAnchors: false);
		_titleLabel.text = subList.Text;
		int num = width / WidgetCountPerLine;
		_recipes.Set(_recipeDataList.Count);
		for (int i = 0; i < _recipes.Count; i++)
		{
			Data data = _recipeDataList[i];
			RecipeItemWidget recipeItemWidget = _recipes[i];
			recipeItemWidget.Set(data);
			recipeItemWidget.Selected = GetSelectedState(recipeItemWidget, selectedRecipeItem);
			recipeItemWidget.Widget.width = num;
		}
		Vector2 vector = new Vector2(num, RecipeHeight);
		Vector2 vector2 = UIUtility.WidgetsGridReposition(_recipes, null, Vector2.down, _recipesContainer.localCorners[1], width, vector, 0f, 0f);
		_recipesContainer.height = (int)vector2.y;
		UIUtility.MakeGridBackground(Vector3.zero, _recipesContainer.pivotOffset, _recipesContainer.width, _recipesContainer.height, vector, new UIUtility.Separators
		{
			List = _recipeSeparators,
			Bottom = true
		});
		UIUtility.UpdateAnchors(_recipesContainer.transform);
		_widget.height = _titleWidget.height + _recipesContainer.height;
	}

	public void RefreshSelectState(RecipeListWidget.SelectInfo? selectedRecipeItem)
	{
		for (int i = 0; i < _recipes.Count; i++)
		{
			RecipeItemWidget recipeItemWidget = _recipes[i];
			recipeItemWidget.Selected = GetSelectedState(recipeItemWidget, selectedRecipeItem);
		}
	}

	public RecipeItemWidget FindRecipeComponent(string id)
	{
		int num = -1;
		int i = 0;
		for (int count = _recipeDataList.Count; i < count; i++)
		{
			if (_recipeDataList[i].Item.Id == id)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return null;
		}
		return _recipes[num];
	}

	private bool GetSelectedState(RecipeItemWidget node, RecipeListWidget.SelectInfo? selectedRecipeItem)
	{
		if (selectedRecipeItem.HasValue)
		{
			if (node.Item.Id == selectedRecipeItem.Value.Id)
			{
				return _isFavorites == selectedRecipeItem.Value.InFavorites;
			}
			return false;
		}
		return false;
	}

	private static void SetWidgetWidth(UIWidget widget, int width, bool updateAnchors = true)
	{
		if (widget.width != width)
		{
			widget.width = width;
			if (updateAnchors)
			{
				UIUtility.UpdateAnchors(widget.transform);
			}
		}
	}

	private void RecipeItemWidget_Clicked()
	{
		RecipeItemWidget recipeItemWidget = Selectable.Current as RecipeItemWidget;
		if (!(recipeItemWidget == null) && this.RecipeClicked != null)
		{
			this.RecipeClicked(recipeItemWidget, _isFavorites);
		}
	}
}
