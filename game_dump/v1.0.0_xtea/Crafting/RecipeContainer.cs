using System;
using System.Collections.Generic;
using ItemSystem;
using Shared.Item;
using Yaml;

namespace Crafting;

public class RecipeContainer : GenericRecipeContainer<Recipe, RecipeCategory>
{
	public void SetRecipes(Dictionary<string, Yaml.Recipe> recipesJson)
	{
		Clear();
		Dictionary<string, List<Recipe>> dictionary = new Dictionary<string, List<Recipe>>();
		foreach (KeyValuePair<string, Yaml.Recipe> item in recipesJson)
		{
			Yaml.Recipe value = item.Value;
			Recipe recipe;
			if (value.type == CraftType.Modify)
			{
				RecipeModify recipeModify = new RecipeModify();
				recipeModify.AddColorRate = value.add_color_rate;
				recipe = recipeModify;
			}
			else
			{
				RecipeCraft recipeCraft = new RecipeCraft();
				recipeCraft.PrototypeId = value.prototype_id;
				recipe = recipeCraft;
			}
			recipe.Id = item.Key;
			recipe.Name = value.name;
			recipe.Description = value.description;
			recipe.Icon = value.icon;
			recipe.MinLevel = value.min_level;
			recipe.MaxLevel = value.max_level;
			recipe.Type = value.type;
			recipe.Entrusts = value.entrusts;
			recipe.DurationWait = value.duration_wait;
			recipe.ToolTags = TagFilter.CreateTagFilters(value.tool_tags);
			recipe.Category = value.category;
			recipe.Subcategory = value.subcategory;
			recipe.RequiredWorkbenches = TagFilter.CreateTagFilters(value.workbench_tags);
			if (value.slots != null)
			{
				recipe.Slots = new RecipeSlot[value.slots.Count];
				int num = 0;
				foreach (KeyValuePair<string, Yaml.RecipeSlot> slot in value.slots)
				{
					RecipeSlot recipeSlot = CreateRecipeSlot(recipe.Id, slot.Key, slot.Value);
					recipe.Slots[num++] = recipeSlot;
				}
				if (recipe is RecipeModify)
				{
					Array.Sort(recipe.Slots, (RecipeSlot x, RecipeSlot y) => ((!x.IsModifyBase) ? 1 : 0) - ((!y.IsModifyBase) ? 1 : 0));
				}
			}
			else
			{
				recipe.Slots = new RecipeSlot[0];
			}
			if (!dictionary.ContainsKey(recipe.Category))
			{
				dictionary.Add(recipe.Category, new List<Recipe>());
			}
			dictionary[recipe.Category].Add(recipe);
		}
		foreach (KeyValuePair<string, List<Recipe>> item2 in dictionary)
		{
			RecipeCategory recipeCategory = new RecipeCategory();
			recipeCategory.Id = item2.Key;
			recipeCategory.Name = LocalizeSystem.Get("#recipe_category_" + item2.Key);
			recipeCategory.Recipes = item2.Value;
			_categoryList.Add(recipeCategory);
		}
		OnInit();
	}

	public void Enumerate(Action<Recipe> delegator)
	{
		if (_categoryList == null)
		{
			return;
		}
		for (int i = 0; i < _categoryList.Count; i++)
		{
			RecipeCategory recipeCategory = _categoryList[i];
			for (int j = 0; j < recipeCategory.Recipes.Count; j++)
			{
				delegator(recipeCategory.Recipes[j]);
			}
		}
	}

	private static RecipeSlot CreateRecipeSlot(string recipeId, string slotId, Yaml.RecipeSlot slotYml)
	{
		RecipeSlot recipeSlot = new RecipeSlot();
		recipeSlot.Name = slotYml.slot_name;
		recipeSlot.Id = slotId;
		recipeSlot.CountMin = slotYml.count_min;
		recipeSlot.CountMax = slotYml.count_max;
		recipeSlot.RequiredTags = TagFilter.CreateTagFilters(slotYml.required_tags);
		recipeSlot.RequiredMaterials = TagFilter.CreateTagFilters(slotYml.required_materials);
		return recipeSlot;
	}
}
