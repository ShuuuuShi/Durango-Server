using System;
using System.Collections.Generic;
using Building;
using Durango.Logic.Notification;

namespace Crafting;

public class Category : INotificationable
{
	public string Id;

	public string Name;

	public readonly List<Recipe> Recipes = new List<Recipe>();

	public readonly List<Blueprint> Blueprints = new List<Blueprint>();

	private readonly Container _notification = new Container();

	public int ItemCount => Recipes.Count + Blueprints.Count;

	public Notification Notification => _notification;

	public void UpdateNotification()
	{
		_notification.ClearChild();
		for (int i = 0; i < ItemCount; i++)
		{
			CategoryItem item = GetItem(i);
			if (item.Available)
			{
				_notification.AddChild(item);
			}
		}
		_notification.Refresh();
	}

	public void ClearNotification()
	{
		Notification notification = Notification;
		notification.BeginSetting();
		for (int i = 0; i < ItemCount; i++)
		{
			CategoryItem item = GetItem(i);
			item.Notification.On = false;
		}
		notification.EndSetting();
	}

	public CategoryItem GetItem(int index)
	{
		if (index < Recipes.Count)
		{
			return Recipes[index];
		}
		return Blueprints[index - Recipes.Count];
	}

	private IEnumerable<CategoryItem> GetItems(RecipeSystem.RecipeType type)
	{
		if (type == RecipeSystem.RecipeType.Crafting)
		{
			for (int j = 0; j < Recipes.Count; j++)
			{
				yield return Recipes[j];
			}
		}
		if (type == RecipeSystem.RecipeType.Building)
		{
			for (int i = 0; i < Blueprints.Count; i++)
			{
				yield return Blueprints[i];
			}
		}
	}

	public void SetAvailableList(string[] updatedRecipes, RecipeSystem.RecipeType type)
	{
		bool flag = false;
		foreach (CategoryItem item in GetItems(type))
		{
			bool available = item.Available;
			item.Available = updatedRecipes != null && Array.IndexOf(updatedRecipes, item.Id) != -1;
			flag = item.Available != available;
		}
		if (flag)
		{
			UpdateNotification();
		}
	}

	public void SetNewList(string[] newList, RecipeSystem.RecipeType type)
	{
		if (newList == null)
		{
			return;
		}
		foreach (CategoryItem item in GetItems(type))
		{
			if (item != null && item.Available && Array.IndexOf(newList, item.Id) != -1)
			{
				item.Notification.On = true;
			}
		}
	}

	public void SetLikeList(string[] likeList, RecipeSystem.RecipeType type)
	{
		foreach (CategoryItem item in GetItems(type))
		{
			item.Like = likeList != null && Array.IndexOf(likeList, item.Id) != -1;
		}
	}
}
