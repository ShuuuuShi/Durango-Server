using System;
using System.Collections.Generic;
using Building_;
using Crafting;
using ItemSystem;
using K1Network;
using Messages;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class RecipeSystem : GameSystem<RecipeSystem>
{
	public enum RecipeType
	{
		None,
		Crafting,
		Building
	}

	private static readonly HashSet<ulong> ItemRepetitionCheckSet = new HashSet<ulong>();

	private readonly RecipeContainer _recipeContainer = new RecipeContainer();

	private readonly BlueprintContainer _blueprintContainer = new BlueprintContainer();

	private Crafting.Recipe[] _dyeingRecipes;

	private Crafting.Recipe[] _bleachingRecipes;

	public RecipeContainer RecipeContainer => _recipeContainer;

	public BlueprintContainer BlueprintContainer => _blueprintContainer;

	public event Action<RecipeType> RecipeItemsUpdated;

	public Category GetCategory(RecipeType type, string id)
	{
		Category result = null;
		switch (type)
		{
		case RecipeType.Crafting:
			result = _recipeContainer.GetCategory(id);
			break;
		case RecipeType.Building:
			result = _blueprintContainer.GetCategory(id);
			break;
		}
		return result;
	}

	public CategoryItem GetCategoryItem(RecipeType type, string id)
	{
		CategoryItem result = null;
		switch (type)
		{
		case RecipeType.Crafting:
			result = _recipeContainer.GetRecipe(id);
			break;
		case RecipeType.Building:
			result = _blueprintContainer.GetRecipe(id);
			break;
		}
		return result;
	}

	private void Awake()
	{
		Connections.Frontend.On<Recipes>(OnRecipeListMsg);
		Connections.Frontend.On<ArtifactBlueprints>(OnBlueprintListMsg);
		KSingleton<GameManager>.Instance().Ready += delegate
		{
			RequestRecipeList();
			RequestBlueprintList();
		};
	}

	private void OnRecipeListMsg(Recipes m, PacketHeader header)
	{
		_recipeContainer.SetAvailableList(m.Ids);
		if (this.RecipeItemsUpdated != null)
		{
			this.RecipeItemsUpdated(RecipeType.Crafting);
		}
	}

	private void OnBlueprintListMsg(ArtifactBlueprints m, PacketHeader header)
	{
		_blueprintContainer.SetAvailableList(m.Ids);
		if (this.RecipeItemsUpdated != null)
		{
			this.RecipeItemsUpdated(RecipeType.Building);
		}
	}

	public void SetRecipes(Dictionary<string, Yaml.Recipe> recipesData)
	{
		_recipeContainer.SetRecipes(recipesData);
	}

	public void SetBlueprints(Dictionary<string, Yaml.Blueprint> dict)
	{
		_blueprintContainer.SetBlueprints(dict);
	}

	public void SetArtifactPrototypes(Dictionary<int, ArtifactPrototype> dict)
	{
		_blueprintContainer.SetArtifactPrototypes(dict);
	}

	public Crafting.Recipe GetRecipe(string id)
	{
		return _recipeContainer.GetRecipe(id);
	}

	public Building_.Blueprint GetBlueprint(string id)
	{
		return _blueprintContainer.GetBlueprint(id);
	}

	public Building_.Blueprint GetBlueprint(int entityType)
	{
		return _blueprintContainer.GetBlueprint(entityType);
	}

	private void RequestRecipeList()
	{
		Connections.Frontend.Send(default(GetRecipes));
	}

	private void RequestBlueprintList()
	{
		Connections.Frontend.Send(default(GetArtifactBlueprints));
	}

	public static bool RecipeCanCrafting(Crafting.Recipe recipe, IList<Artifact> workbenchs)
	{
		RecipeCheckCraftCondition(recipe, workbenchs, out var slotCounts, out var hasTool, out var foundWorkbench);
		bool flag = hasTool && foundWorkbench;
		if (flag)
		{
			int i = 0;
			for (int size = KUtility.GetSize(slotCounts); i < size; i++)
			{
				flag = slotCounts[i] >= recipe.Slots[i].CountMin;
				if (!flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	public static void RecipeCheckCraftCondition(Crafting.Recipe recipe, IList<Artifact> workbenchs, out int[] slotCounts, out bool hasTool, out bool foundWorkbench)
	{
		List<ItemData> playerItemList = GameSystem<InventorySystem>.Instance().PlayerItemList;
		hasTool = !recipe.ToolRequired || Util.Exist(playerItemList, (ItemData item) => item.HasTag(recipe.ToolTags));
		foundWorkbench = !recipe.WorkbenchRequired || (Object)(object)FindNearestAvailableWorkbench(recipe, workbenchs) != (Object)null;
		List<ItemData> list = new List<ItemData>(playerItemList);
		list.Sort((ItemData x, ItemData y) => Util.GetSlotCountBySuitableItem(x, recipe.Slots) - Util.GetSlotCountBySuitableItem(y, recipe.Slots));
		ItemRepetitionCheckSet.Clear();
		slotCounts = new int[recipe.Slots.Length];
		int i = 0;
		for (int num = recipe.Slots.Length; i < num; i++)
		{
			Crafting.RecipeSlot slot = recipe.Slots[i];
			int remainCount = slot.CountMin;
			slotCounts[i] = Util.Counting(list, delegate(ItemData item)
			{
				if (!ItemRepetitionCheckSet.Contains(item.Id) && slot.IsSuitableItem(item))
				{
					if (remainCount > 0)
					{
						ItemRepetitionCheckSet.Add(item.Id);
						remainCount--;
					}
					return true;
				}
				return false;
			});
		}
	}

	public static bool BlueprintCanBuild(Building_.Blueprint blueprint)
	{
		BlueprintCheckBuildCondition(blueprint, out var slotCounts, out var hasTool);
		bool flag = hasTool;
		if (hasTool)
		{
			int i = 0;
			for (int size = KUtility.GetSize(slotCounts); i < size; i++)
			{
				flag = slotCounts[i] >= blueprint.Slots[i].RequiredCount;
				if (!flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	public static void BlueprintCheckBuildCondition(Building_.Blueprint blueprint, out int[] slotCounts, out bool hasTool)
	{
		BlueprintCheckBuildCondition(blueprint, blueprint.Size, out slotCounts, out hasTool);
	}

	public static void BlueprintCheckBuildCondition(Building_.Blueprint blueprint, Point2 size, out int[] slotCounts, out bool hasTool)
	{
		List<ItemData> playerItemList = GameSystem<InventorySystem>.Instance().PlayerItemList;
		hasTool = !blueprint.ToolRequired || Util.Exist(playerItemList, (ItemData item) => item.HasTag(blueprint.ToolTags));
		List<ItemData> list = new List<ItemData>(playerItemList);
		list.Sort((ItemData x, ItemData y) => Util.GetSlotCountBySuitableItem(x, blueprint.Slots) - Util.GetSlotCountBySuitableItem(y, blueprint.Slots));
		ItemRepetitionCheckSet.Clear();
		slotCounts = new int[blueprint.Slots.Length];
		int i = 0;
		for (int num = slotCounts.Length; i < num; i++)
		{
			Building_.BlueprintSlot slot = blueprint.Slots[i];
			int num2 = 1;
			if (blueprint.IsSizeVariable)
			{
				num2 = BuildManager.GetBlueprintSlotCountModifier(slot, size);
			}
			int remainCount = slot.RequiredCount * num2;
			slotCounts[i] = Util.Counting(list, delegate(ItemData item)
			{
				if (!ItemRepetitionCheckSet.Contains(item.Id) && slot.IsSuitableItem(item))
				{
					if (remainCount > 0)
					{
						ItemRepetitionCheckSet.Add(item.Id);
						remainCount--;
					}
					return true;
				}
				return false;
			});
		}
	}

	public static Artifact[] FindNearWorkbenches()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<GameObject> list = new List<GameObject>();
		int mask = LayerMask.op_Implicit(LayerHelper.PropMask);
		InteractionSystem.GetNearObjectsInternal(list, mask, 800f, InteractionSystem.ArtifactObjectFilter);
		Artifact[] array = new Artifact[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = list[i].GetComponent<Artifact>();
		}
		return array;
	}

	public static Artifact FindNearestAvailableWorkbench(Crafting.Recipe recipe, IList<Artifact> artifacts)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		if (artifacts == null)
		{
			return null;
		}
		float num = float.MaxValue;
		Artifact result = null;
		int count = artifacts.Count;
		for (int i = 0; i < count; i++)
		{
			Artifact artifact = artifacts[i];
			if ((Object)(object)artifact == (Object)null)
			{
				continue;
			}
			bool flag = true;
			int num2 = recipe.RequiredWorkbenches.Length;
			int count2 = artifact.Tags.Count;
			for (int j = 0; j < num2; j++)
			{
				for (int k = 0; k < count2; k++)
				{
					if (artifact.Tags[k].Id == recipe.RequiredWorkbenches[j].TagId && artifact.Tags[k].Level >= recipe.RequiredWorkbenches[j].RequiredLevel)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			if (!flag)
			{
				float num3 = Vector3.Distance(artifact.InteractionPosition, PlayerBehavior.LocalPlayer.CurrentPosition);
				if (num3 < num)
				{
					num = num3;
					result = artifact;
				}
			}
		}
		return result;
	}

	public void FillAvailableRecipesByItemData(HashSet<Crafting.Recipe> hashSet, ItemData itemData)
	{
		hashSet.Clear();
		_recipeContainer.Enumerate(delegate(Crafting.Recipe recipe)
		{
			if (recipe.Available)
			{
				for (int i = 0; i < recipe.Slots.Length; i++)
				{
					Crafting.RecipeSlot recipeSlot = recipe.Slots[i];
					if (itemData.HasTagsAndMaterials(recipeSlot.RequiredTags, recipeSlot.RequiredMaterials))
					{
						hashSet.Add(recipe);
					}
				}
			}
		});
	}

	public void FillAvailableBlueprintsByItemData(HashSet<Building_.Blueprint> hashSet, ItemData itemData)
	{
		hashSet.Clear();
		_blueprintContainer.Enumerate(delegate(Building_.Blueprint blueprint)
		{
			if (blueprint.Available)
			{
				for (int i = 0; i < blueprint.Slots.Length; i++)
				{
					Building_.BlueprintSlot blueprintSlot = blueprint.Slots[i];
					if (itemData.HasTagsAndMaterials(blueprintSlot.RequiredTags, blueprintSlot.RequiredMaterials))
					{
						hashSet.Add(blueprint);
					}
				}
			}
		});
	}

	public Crafting.Recipe GetDyeingRecipe(ColorChannel channel)
	{
		if (_dyeingRecipes == null)
		{
			_dyeingRecipes = new Crafting.Recipe[3];
			Dictionary<ColorChannel, string> dye_recipe = Singleton<Constants>.Instance.item.dye_recipe;
			for (int i = 0; i < _dyeingRecipes.Length; i++)
			{
				_dyeingRecipes[i] = GetRecipe(dye_recipe.Get((ColorChannel)i));
			}
		}
		return (channel >= ColorChannel.ColorR && (int)channel < _dyeingRecipes.Length) ? _dyeingRecipes[(int)channel] : null;
	}

	public Crafting.Recipe GetBleachingRecipe(ColorChannel channel)
	{
		if (_bleachingRecipes == null)
		{
			_bleachingRecipes = new Crafting.Recipe[3];
			Dictionary<ColorChannel, string> bleach_recipe = Singleton<Constants>.Instance.item.bleach_recipe;
			for (int i = 0; i < _bleachingRecipes.Length; i++)
			{
				_bleachingRecipes[i] = GetRecipe(bleach_recipe.Get((ColorChannel)i));
			}
		}
		return (channel >= ColorChannel.ColorR && (int)channel < _bleachingRecipes.Length) ? _bleachingRecipes[(int)channel] : null;
	}
}
