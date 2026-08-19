using System;
using System.Collections.Generic;
using Building;
using Durango.Logic.Item;
using Durango.Logic.Notification;
using Durango.Terrain;
using JetBrains.Annotations;
using Shared.Item;
using Yaml;

namespace Crafting;

public class RecipeContainer : INotificationable
{
	public List<Category> _categoryList = new List<Category>();

	private readonly Container _notification = new Container();

	public List<Category> Categories => _categoryList;

	public Notification Notification => _notification;

	private void FillArtifactPrototypes(Dictionary<int, ArtifactPrototype> data)
	{
		foreach (KeyValuePair<int, ArtifactPrototype> datum in data)
		{
			string _name__ = datum.Value.__name__;
			GetBlueprint(_name__)?.SetPrototypeInfo(datum.Key, datum.Value);
		}
	}

	public void InitBlueprints(Dictionary<string, Yaml.Blueprint> blueprints, Dictionary<int, ArtifactPrototype> prototypes)
	{
		foreach (KeyValuePair<string, Yaml.Blueprint> blueprint in blueprints)
		{
			if (blueprint.Value != null)
			{
				Building.Blueprint item = new Building.Blueprint(blueprint.Key, blueprint.Value);
				Category orAddCategory = GetOrAddCategory(blueprint.Value.category);
				orAddCategory.Blueprints.Add(item);
			}
		}
		FillArtifactPrototypes(prototypes);
	}

	public void InitAbstractNaturalBlueprint(IEnumerable<BiomeSpriteInfo> naturals)
	{
		if (naturals == null)
		{
			return;
		}
		foreach (BiomeSpriteInfo natural in naturals)
		{
			if (natural.IsCraft)
			{
				Building.Blueprint blueprint = new Building.Blueprint(natural);
				Category orAddCategory = GetOrAddCategory(blueprint.Category);
				orAddCategory.Blueprints.Add(blueprint);
			}
		}
	}

	public void InitRecipes(Dictionary<string, Yaml.Recipe> recipesJson)
	{
		foreach (KeyValuePair<string, Yaml.Recipe> item in recipesJson)
		{
			Yaml.Recipe value = item.Value;
			Recipe recipe;
			switch (value.type)
			{
			case CraftType.Craft:
			{
				RecipeCraft recipeCraft = new RecipeCraft();
				recipeCraft.PrototypeId = value.prototype_id;
				recipe = recipeCraft;
				break;
			}
			case CraftType.Modify:
			{
				RecipeModify recipeModify = new RecipeModify();
				recipeModify.AddColorRate = value.add_color_rate;
				recipe = recipeModify;
				break;
			}
			case CraftType.Reform:
			{
				RecipeReform recipeReform = new RecipeReform();
				recipeReform.RecipeNameForSlot = value.recipe_name_for_slot;
				recipe = recipeReform;
				break;
			}
			default:
				continue;
			}
			recipe.Id = item.Key;
			recipe.Name = value.name;
			recipe.Description = value.description;
			recipe.Count = value.count;
			recipe.DeductModifiableCount = value.deduct_modifiable_count;
			recipe.Icon = value.icon;
			recipe.MinLevel = value.min_level;
			recipe.MaxLevel = value.max_level;
			recipe.Type = value.type;
			recipe.Entrusts = value.entrusts;
			recipe.DurationWait = value.duration_wait;
			recipe.AllowedTool = new OrTagFilter(value.tool_tags);
			recipe.Season = value.season;
			recipe.IsSeasonalRegionBounded = value.IsSeasonalRegionBounded;
			recipe.RequiredAbility = value.required_ability;
			recipe.RequiredRecipe = value.required_recipe;
			recipe.Category = value.category;
			recipe.Subcategory = value.subcategory;
			recipe.AllowedWorkbench = new OrTagFilter(value.workbench_tags);
			if (value.slots != null)
			{
				recipe.Slots = new RecipeSlot[value.slots.Length];
				for (int i = 0; i < value.slots.Length; i++)
				{
					RecipeSlot recipeSlot = CreateRecipeSlot(value.slots[i]);
					recipeSlot.SlotType = RecipeSlot.Type.General;
					if (recipeSlot.IsBase)
					{
						if (value.type == CraftType.Reform)
						{
							recipeSlot.SlotType = RecipeSlot.Type.ReformBase;
						}
						else if (recipe.DeductModifiableCount)
						{
							recipeSlot.SlotType = RecipeSlot.Type.ModifyBase;
						}
					}
					recipe.Slots[i] = recipeSlot;
				}
			}
			else
			{
				recipe.Slots = new RecipeSlot[0];
			}
			Category orAddCategory = GetOrAddCategory(recipe.Category);
			orAddCategory.Recipes.Add(recipe);
		}
	}

	[NotNull]
	private Category GetOrAddCategory(string id)
	{
		foreach (Category category2 in _categoryList)
		{
			if (category2.Id == id)
			{
				return category2;
			}
		}
		Category category = new Category();
		category.Id = id;
		category.Name = LocalizeSystem.Get("#recipe_category_" + id);
		_categoryList.Add(category);
		return category;
	}

	public void InitNotifications()
	{
		Container notification = _notification;
		notification.BeginSetting();
		notification.ClearChild();
		foreach (Category category in _categoryList)
		{
			category.UpdateNotification();
			notification.AddChild(category);
		}
		notification.EndSetting();
	}

	public List<Building.Blueprint> GetAllBlueprints()
	{
		List<Building.Blueprint> list = new List<Building.Blueprint>();
		foreach (Category category in _categoryList)
		{
			list.AddRange(category.Blueprints);
		}
		return list;
	}

	public Recipe GetRecipe(string id)
	{
		GetRecipe(id, out var recipe, out var _);
		return recipe;
	}

	public bool GetRecipe(string id, out Recipe recipe, out Category category)
	{
		if (!string.IsNullOrEmpty(id))
		{
			foreach (Category category2 in _categoryList)
			{
				foreach (Recipe recipe2 in category2.Recipes)
				{
					if (recipe2.Id == id)
					{
						recipe = recipe2;
						category = category2;
						return true;
					}
				}
			}
		}
		recipe = null;
		category = null;
		return false;
	}

	public Building.Blueprint GetBlueprint(string id)
	{
		GetBlueprint(id, out var blueprint, out var _);
		return blueprint;
	}

	public bool GetBlueprint(string id, out Building.Blueprint blueprint, out Category category)
	{
		foreach (Category category2 in _categoryList)
		{
			foreach (Building.Blueprint blueprint2 in category2.Blueprints)
			{
				if (blueprint2.Id == id)
				{
					blueprint = blueprint2;
					category = category2;
					return true;
				}
			}
		}
		blueprint = null;
		category = null;
		return false;
	}

	public Building.Blueprint GetBlueprint(int entityType)
	{
		foreach (Category category in _categoryList)
		{
			foreach (Building.Blueprint blueprint in category.Blueprints)
			{
				if (blueprint.EntityType == entityType)
				{
					return blueprint;
				}
			}
		}
		return null;
	}

	public void SetAvailableList(string[] availableList, string[] newList, RecipeSystem.RecipeType type)
	{
		_notification.BeginSetting();
		for (int i = 0; i < _notification.Children.Count; i++)
		{
			_notification.Children[i].BeginSetting();
		}
		for (int j = 0; j < _categoryList.Count; j++)
		{
			Category category = _categoryList[j];
			category.SetAvailableList(availableList, type);
		}
		for (int k = 0; k < _categoryList.Count; k++)
		{
			Category category2 = _categoryList[k];
			category2.SetNewList(newList, type);
		}
		for (int l = 0; l < _notification.Children.Count; l++)
		{
			_notification.Children[l].EndSetting();
		}
		_notification.EndSetting();
	}

	public void SetLikeList(string[] likeList, RecipeSystem.RecipeType type)
	{
		for (int i = 0; i < _categoryList.Count; i++)
		{
			Category category = _categoryList[i];
			category.SetLikeList(likeList, type);
		}
	}

	public void EnumerateBlueprints(Action<Building.Blueprint> delegator)
	{
		foreach (Category category in _categoryList)
		{
			foreach (Building.Blueprint blueprint in category.Blueprints)
			{
				delegator(blueprint);
			}
		}
	}

	public void EnumerateRecipes(Action<Recipe> delegator)
	{
		foreach (Category category in _categoryList)
		{
			foreach (Recipe recipe in category.Recipes)
			{
				delegator(recipe);
			}
		}
	}

	private static RecipeSlot CreateRecipeSlot(Yaml.RecipeSlot slotYml)
	{
		RecipeSlot recipeSlot = new RecipeSlot();
		recipeSlot.Name = slotYml.slot_name;
		recipeSlot.Id = slotYml.slot_id;
		recipeSlot.Count = slotYml.count_min;
		recipeSlot.CountMax = slotYml.count_max;
		recipeSlot.AllowedTags = new OrTagFilter(slotYml.required_tags);
		recipeSlot.AllowedMaterials = new OrTagFilter(slotYml.required_materials);
		recipeSlot.RequiredLevel = OrTagFilter.GetLevelFromFirstTag(recipeSlot.AllowedTags, recipeSlot.AllowedMaterials);
		recipeSlot.SlotSourceInfos = slotYml.source_info;
		return recipeSlot;
	}
}
