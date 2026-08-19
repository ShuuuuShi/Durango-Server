using System;
using System.Collections.Generic;
using Crafting;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class CategoryListWidget : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private KGridScrollView _categoryScrollView;

	private CategoryWidget _selectedWidget;

	private CategoryWidget SelectedWidget
	{
		get
		{
			return _selectedWidget;
		}
		set
		{
			if (_selectedWidget == value)
			{
				return;
			}
			if (_selectedWidget != null)
			{
				_selectedWidget.Selected = false;
				if (_selectedWidget.Category != null)
				{
					_selectedWidget.Category.ClearNotification();
				}
			}
			_selectedWidget = value;
			if (_selectedWidget != null)
			{
				_selectedWidget.Selected = true;
			}
		}
	}

	[CanBeNull]
	public Category SelectedCategory
	{
		get
		{
			if (SelectedWidget != null)
			{
				return SelectedWidget.Category;
			}
			return null;
		}
	}

	public event Action CategorySelected;

	private void OnDisable()
	{
		if (SelectedWidget != null && SelectedWidget.Category != null)
		{
			SelectedWidget.Category.ClearNotification();
		}
	}

	public void ResetCategories(List<Category> categories)
	{
		_categoryScrollView.Nodes.BeginLoad();
		CategoryWidget categoryWidget = AddCategoryItem();
		categoryWidget.SetEntireCategory();
		categoryWidget.SetNotification(on: false);
		foreach (Category category in categories)
		{
			AddCategoryItem().SetCategory(category);
		}
		_categoryScrollView.Nodes.EndLoad();
		_categoryScrollView.Reposition();
	}

	public void SelectCategory([CanBeNull] string id)
	{
		ListObjectPool nodes = _categoryScrollView.Nodes;
		CategoryWidget categoryWidget = null;
		for (int i = 0; i < nodes.Count; i++)
		{
			CategoryWidget component = nodes[i].GetComponent<CategoryWidget>();
			if (component.Id == id)
			{
				categoryWidget = component;
				break;
			}
		}
		if (categoryWidget != null)
		{
			SelectCategoryItem(categoryWidget);
		}
	}

	public CategoryWidget FindCategory([CanBeNull] string id)
	{
		ListObjectPool nodes = _categoryScrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			CategoryWidget component = nodes[i].GetComponent<CategoryWidget>();
			if (component.Id == id)
			{
				return component;
			}
		}
		return null;
	}

	void IUIInitializable.Init()
	{
		_categoryScrollView.Nodes.Init(delegate(GameObject go)
		{
			CategoryWidget component = go.GetComponent<CategoryWidget>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(CategoryControl_Clicked));
		});
	}

	private CategoryWidget AddCategoryItem()
	{
		return _categoryScrollView.Nodes.GetNext().GetComponent<CategoryWidget>();
	}

	private void SelectCategoryItem(CategoryWidget widget)
	{
		SelectedWidget = widget;
		if (widget != null && this.CategorySelected != null)
		{
			this.CategorySelected();
		}
	}

	private void CategoryControl_Clicked()
	{
		CategoryWidget categoryWidget = Selectable.Current as CategoryWidget;
		if (categoryWidget != null)
		{
			SelectCategoryItem(categoryWidget);
		}
	}
}
