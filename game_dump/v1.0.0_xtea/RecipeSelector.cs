using System;
using System.Collections;
using System.Collections.Generic;
using Building_;
using Crafting;
using UnityEngine;

public class RecipeSelector : MonoBehaviour
{
	public struct RecipeItem
	{
		public CategoryItem Item;

		public bool CanCraft;

		public bool IsGuided;
	}

	public Action<RecipeSystem.RecipeType, string> OnSelectRecipeCategory;

	public Action<RecipeSystem.RecipeType, string> OnSelectRecipeItem;

	[SerializeField]
	private ListObjectPool _categoryPool;

	[SerializeField]
	private UIScrollView _categoryScrollView;

	[SerializeField]
	private ScrollViewGridBackground _categoryBg;

	[SerializeField]
	private ListObjectPool _recipePool;

	[SerializeField]
	private ListObjectPool _recipeSubCategoryPool;

	[SerializeField]
	private UIScrollView _recipeScrollView;

	[SerializeField]
	private GameObject _recipeNoSelect;

	[SerializeField]
	private int _recipeMargin;

	private CategoryControl _selectedCategory;

	private CategoryNodeControl _selectedRecipe;

	private CategoryControl SelectedCategory
	{
		get
		{
			return _selectedCategory;
		}
		set
		{
			if ((Object)(object)_selectedCategory != (Object)null)
			{
				_selectedCategory.Select = false;
			}
			_selectedCategory = value;
			if ((Object)(object)_selectedCategory != (Object)null)
			{
				_selectedCategory.Select = true;
			}
		}
	}

	private CategoryNodeControl SelectedRecipe
	{
		get
		{
			return _selectedRecipe;
		}
		set
		{
			if ((Object)(object)_selectedRecipe != (Object)null)
			{
				_selectedRecipe.Select = false;
			}
			_selectedRecipe = value;
			if ((Object)(object)_selectedRecipe != (Object)null)
			{
				_selectedRecipe.Select = true;
			}
		}
	}

	private void OnEnable()
	{
		_categoryScrollView.ResetPosition();
		_recipeScrollView.ResetPosition();
		_categoryBg.Reset();
	}

	public void Init()
	{
		((Component)_recipeScrollView).gameObject.SetActive(false);
		_recipeNoSelect.SetActive(true);
	}

	public void GetSelectedData(out RecipeSystem.RecipeType type, out string category, out string recipe)
	{
		if ((Object)(object)SelectedCategory == (Object)null)
		{
			type = RecipeSystem.RecipeType.None;
			category = null;
			recipe = null;
		}
		else
		{
			type = SelectedCategory.Type;
			category = SelectedCategory.Id;
			recipe = ((!((Object)(object)SelectedRecipe == (Object)null)) ? SelectedRecipe.Id : null);
		}
	}

	public void InitRecipeSelector(RecipeSystem.RecipeType type, string category, string recipe)
	{
		CategoryControl categoryControl = null;
		if (type != 0 && !string.IsNullOrEmpty(category))
		{
			int i = 0;
			for (int count = _categoryPool.Count; i < count; i++)
			{
				CategoryControl component = _categoryPool[i].GetComponent<CategoryControl>();
				if (component.Type == type && component.Id == category)
				{
					categoryControl = component;
					break;
				}
			}
		}
		if ((Object)(object)categoryControl == (Object)null)
		{
			ClearRecipeItems();
			return;
		}
		Category_OnSelectItem(categoryControl);
		SelectRecipe(type, recipe);
	}

	public void SetCategories(List<RecipeCategory> recipeCategories, List<BlueprintCategory> blueprintCategories)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		UIWidget component = _categoryPool.BaseObject.GetComponent<UIWidget>();
		Vector3 localPosition = ((Component)component).transform.localPosition;
		KeyValuePair<IList, RecipeSystem.RecipeType>[] array = new KeyValuePair<IList, RecipeSystem.RecipeType>[2]
		{
			new KeyValuePair<IList, RecipeSystem.RecipeType>(recipeCategories, RecipeSystem.RecipeType.Crafting),
			new KeyValuePair<IList, RecipeSystem.RecipeType>(blueprintCategories, RecipeSystem.RecipeType.Building)
		};
		_categoryPool.Clear();
		int i = 0;
		for (int num2 = array.Length; i < num2; i++)
		{
			if (array[i].Key != null)
			{
				int j = 0;
				for (int count = array[i].Key.Count; j < count; j++)
				{
					Category category = array[i].Key[j] as Category;
					CategoryControl categoryControl = ((ListObjectPoolBase<GameObject>)_categoryPool).Add<CategoryControl>();
					int num3 = num / 2;
					int num4 = num % 2;
					Vector3 position = localPosition + new Vector3((float)(num4 * component.width), (float)(-num3 * component.height));
					categoryControl.Position = position;
					categoryControl.SetCategory(category, array[i].Value);
					categoryControl.Clicked = Category_OnSelectItem;
					categoryControl.Select = (Object)(object)categoryControl == (Object)(object)SelectedCategory;
					num++;
				}
			}
		}
		_categoryScrollView.UpdateScrollbars();
	}

	public void ClearRecipeItems()
	{
		((Component)_recipeScrollView).gameObject.SetActive(false);
		_recipeNoSelect.SetActive(true);
		SelectedCategory = null;
		SelectedRecipe = null;
	}

	public void SetRecipeItems(RecipeSystem.RecipeType type, Dictionary<string, List<RecipeItem>> dict)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		_recipeNoSelect.SetActive(false);
		((Component)_recipeScrollView).gameObject.SetActive(true);
		UIWidget component = _recipePool.BaseObject.GetComponent<UIWidget>();
		int num = component.width + _recipeMargin;
		int num2 = component.height + _recipeMargin;
		UIWidget component2 = _recipeSubCategoryPool.BaseObject.GetComponent<UIWidget>();
		int height = component2.height;
		int num3 = 0;
		_recipePool.Clear();
		_recipeSubCategoryPool.Clear();
		foreach (KeyValuePair<string, List<RecipeItem>> item in dict)
		{
			Transform val = ((ListObjectPoolBase<GameObject>)_recipeSubCategoryPool).Add<Transform>();
			((Component)val).gameObject.SetActive(true);
			((Component)val.Find("Name")).GetComponent<UILabel>().text = item.Key;
			val.localPosition = new Vector3(0f, (float)(-num3), 0f);
			num3 += height;
			List<RecipeItem> value = item.Value;
			if (value == null)
			{
				continue;
			}
			int i = 0;
			for (int count = value.Count; i < count; i++)
			{
				RecipeItem recipeItem = value[i];
				CategoryNodeControl categoryNodeControl = ((ListObjectPoolBase<GameObject>)_recipePool).Add<CategoryNodeControl>();
				categoryNodeControl.Position = Vector3.right * (float)((i % 3 - 1) * num) + Vector3.down * (float)num3;
				categoryNodeControl.Clicked = CategoryNodeControl_Clicked;
				categoryNodeControl.Disable = !recipeItem.CanCraft;
				categoryNodeControl.Select = (Object)(object)categoryNodeControl == (Object)(object)SelectedRecipe;
				categoryNodeControl.IsGuided = recipeItem.IsGuided;
				categoryNodeControl.Set(recipeItem.Item, type);
				if (i % 3 == 2 || i == count - 1)
				{
					num3 += num2;
				}
			}
		}
		if ((Object)(object)SelectedRecipe == (Object)null)
		{
			_recipeScrollView.ResetPosition();
		}
	}

	public CategoryControl FindCategory(string id)
	{
		int i = 0;
		for (int count = _categoryPool.Count; i < count; i++)
		{
			CategoryControl component = _categoryPool[i].GetComponent<CategoryControl>();
			if (component.Id == id)
			{
				return component;
			}
		}
		return null;
	}

	public CategoryNodeControl FindRecipe(string id)
	{
		for (int i = 0; i < _recipePool.Count; i++)
		{
			CategoryNodeControl component = _recipePool[i].GetComponent<CategoryNodeControl>();
			if (component.Id == id)
			{
				return component;
			}
		}
		return null;
	}

	public void SelectRecipe(RecipeSystem.RecipeType type, string recipe)
	{
		if (string.IsNullOrEmpty(recipe))
		{
			return;
		}
		for (int i = 0; i < _recipePool.Count; i++)
		{
			CategoryNodeControl component = _recipePool[i].GetComponent<CategoryNodeControl>();
			if (component.Type == type && component.Id == recipe)
			{
				Recipe_OnSelectItem(component);
				break;
			}
		}
	}

	public void ScrollToRecipe(RecipeSystem.RecipeType type, string recipe)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(recipe))
		{
			return;
		}
		for (int i = 0; i < _recipePool.Count; i++)
		{
			CategoryNodeControl component = _recipePool[i].GetComponent<CategoryNodeControl>();
			if (component.Type == type && component.Id == recipe)
			{
				Vector4 finalClipRegion = _recipeScrollView.panel.finalClipRegion;
				float num = finalClipRegion.w * 0.5f;
				Bounds bounds = _recipeScrollView.bounds;
				Vector3 localPosition = ((Component)_recipeScrollView).transform.localPosition;
				localPosition.y = Mathf.Min(((Bounds)(ref bounds)).size.y - finalClipRegion.w, 0f - ((Component)component).transform.localPosition.y - num);
				localPosition.y = Mathf.Max(0f, localPosition.y);
				SpringPanel.Begin(((Component)_recipeScrollView).gameObject, localPosition, 5f);
				break;
			}
		}
	}

	private void Category_OnSelectItem()
	{
		CategoryControl item = Selectable.Current as CategoryControl;
		Category_OnSelectItem(item);
	}

	private void Category_OnSelectItem(CategoryControl item)
	{
		if ((Object)(object)SelectedCategory != (Object)(object)item)
		{
			SelectedRecipe = null;
		}
		SelectedCategory = item;
		if (OnSelectRecipeCategory != null && (Object)(object)item != (Object)null)
		{
			OnSelectRecipeCategory(item.Type, item.Id);
		}
	}

	private void CategoryNodeControl_Clicked()
	{
		CategoryNodeControl node = Selectable.Current as CategoryNodeControl;
		Recipe_OnSelectItem(node);
	}

	private void Recipe_OnSelectItem(CategoryNodeControl node)
	{
		SelectedRecipe = node;
		if (OnSelectRecipeItem != null && (Object)(object)node != (Object)null)
		{
			OnSelectRecipeItem(node.Type, node.Id);
		}
	}
}
