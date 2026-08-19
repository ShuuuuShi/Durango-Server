using System;
using System.Collections.Generic;

namespace Crafting;

public abstract class GenericRecipeContainer<T, T_Category> : INewCheckerable where T : CategoryItem where T_Category : CategoryGeneric<T>
{
	protected List<T_Category> _categoryList = new List<T_Category>();

	protected string[] _availableRecipeList;

	private NewCheckerContainer _newChecker;

	public IList<T_Category> Categories => _categoryList;

	public NewChecker NewChecker
	{
		get
		{
			if (_newChecker == null)
			{
				_newChecker = new NewCheckerContainer();
			}
			return _newChecker;
		}
	}

	public void Clear()
	{
		_categoryList.Clear();
	}

	public List<T> GetRecipes()
	{
		List<T> list = new List<T>();
		int i = 0;
		for (int num = ((_categoryList != null) ? _categoryList.Count : 0); i < num; i++)
		{
			list.AddRange(_categoryList[i].Recipes);
		}
		return list;
	}

	public T GetRecipe(string id)
	{
		int i = 0;
		for (int num = ((_categoryList != null) ? _categoryList.Count : 0); i < num; i++)
		{
			int j = 0;
			for (int count = _categoryList[i].Recipes.Count; j < count; j++)
			{
				T val = _categoryList[i].Recipes[j];
				if (val.Id == id)
				{
					return val;
				}
			}
		}
		return (T)null;
	}

	public T_Category GetCategory(string id)
	{
		int i = 0;
		for (int num = ((_categoryList != null) ? _categoryList.Count : 0); i < num; i++)
		{
			if (_categoryList[i].Id == id)
			{
				return _categoryList[i];
			}
		}
		return (T_Category)null;
	}

	public void SetAvailableList(string[] availableRecipeList)
	{
		if (_availableRecipeList != null)
		{
			int num = ((_availableRecipeList != null) ? _availableRecipeList.Length : 0);
			int num2 = ((availableRecipeList != null) ? availableRecipeList.Length : 0);
			for (int i = 0; i < num2; i++)
			{
				if (num == 0 || Array.IndexOf(_availableRecipeList, availableRecipeList[i]) == -1)
				{
					T recipe = GetRecipe(availableRecipeList[i]);
					if (recipe != null)
					{
						recipe.NewChecker.IsNew = true;
					}
				}
			}
		}
		_availableRecipeList = availableRecipeList;
		if (_availableRecipeList != null)
		{
			int j = 0;
			for (int count = _categoryList.Count; j < count; j++)
			{
				int k = 0;
				for (int count2 = _categoryList[j].Recipes.Count; k < count2; k++)
				{
					T val = _categoryList[j].Recipes[k];
					if (Array.IndexOf(_availableRecipeList, val.Id) == -1)
					{
						val.NewChecker.IsNew = false;
					}
				}
			}
		}
		int l = 0;
		for (int num3 = ((_categoryList != null) ? _categoryList.Count : 0); l < num3; l++)
		{
			T_Category val2 = _categoryList[l];
			int m = 0;
			for (int num4 = ((val2.Items != null) ? val2.Items.Length : 0); m < num4; m++)
			{
				CategoryItem categoryItem = val2.Items[m];
				categoryItem.Available = _availableRecipeList != null && Array.IndexOf(_availableRecipeList, categoryItem.Id) != -1;
			}
		}
	}

	public void ClearNewCheckerCallback()
	{
		int count = _categoryList.Count;
		for (int i = 0; i < count; i++)
		{
			int count2 = _categoryList[i].Recipes.Count;
			for (int j = 0; j < count2; j++)
			{
				T val = _categoryList[i].Recipes[j];
				val.NewChecker.ClearCallback();
			}
		}
	}

	protected void OnInit()
	{
		NewChecker.ClearChild();
		int count = _categoryList.Count;
		for (int i = 0; i < count; i++)
		{
			T_Category val = _categoryList[i];
			val.OnInit();
			NewChecker.AddChild(_categoryList[i]);
		}
		NewCheckUtil.Refresh(_categoryList);
	}
}
