using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Building;
using Crafting;
using Durango.Logic.Clusters;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

/// <summary>
/// [แก้เอง] เหตุผลที่สูตรยังทำไม่ได้ — เรียงจาก "ใกล้ทำได้" ไป "ไกล"
/// ใช้ทั้งตอนกรองรายการคราฟและตอนขึ้นข้อความบอกผู้เล่น
/// </summary>
public enum CraftBlocker
{
	/// <summary>ทำได้เลย</summary>
	None,
	/// <summary>ของครบ แต่ยังไม่มีเครื่องมือที่ต้องใช้</summary>
	Tool,
	/// <summary>ของครบ แต่ไม่ได้ยืนอยู่ที่โต๊ะ/เตาที่ต้องใช้</summary>
	Workbench,
	/// <summary>วัตถุดิบไม่พอ</summary>
	Materials
}

public class RecipeSystem : GameSystem<RecipeSystem>
{
	public enum RecipeType
	{
		None,
		Crafting,
		Building
	}

	private const string TrackingKey = "TrackingRecipe";

	private static readonly BipartiteMatching BipartiteMatching;

	private readonly RecipeContainer _recipeContainer = new RecipeContainer();

	private readonly RemodelingBlueprints _remodelingBlueprints = new RemodelingBlueprints();

	private Crafting.Recipe[] _dyeingRecipes;

	private Crafting.Recipe[] _bleachingRecipes;

	private Artifact[] _nearWorkbenches;

	private ItemSlotsTodoCollection _trackingTodo;

	[CompilerGenerated]
	private static Func<GameObject, GameObject> cache0;

	public string TrackingTarget { get; private set; }

	[NotNull]
	public RecipeContainer RecipeContainer => _recipeContainer;

	[NotNull]
	public RemodelingBlueprints RemodelingBlueprints => _remodelingBlueprints;

	public event Action<RecipeType> RecipeItemsUpdated;

	public CategoryItem GetCategoryItem(RecipeType type, string id)
	{
		CategoryItem result = null;
		switch (type)
		{
		case RecipeType.Crafting:
			result = _recipeContainer.GetRecipe(id);
			break;
		case RecipeType.Building:
			result = _recipeContainer.GetBlueprint(id);
			break;
		}
		return result;
	}

	public bool GetCategoryItem(RecipeType type, string id, out Category category, out CategoryItem item)
	{
		switch (type)
		{
		case RecipeType.Crafting:
		{
			if (_recipeContainer.GetRecipe(id, out var recipe, out var category3))
			{
				category = category3;
				item = recipe;
				return true;
			}
			break;
		}
		case RecipeType.Building:
		{
			if (_recipeContainer.GetBlueprint(id, out var blueprint, out var category2))
			{
				category = category2;
				item = blueprint;
				return true;
			}
			break;
		}
		}
		category = null;
		item = null;
		return false;
	}

	private void Awake()
	{
		Connections.Frontend.On<Recipes>(OnRecipeListMsg);
		Connections.Frontend.On<ArtifactBlueprints>(OnBlueprintListMsg);
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(OnReady);
		Durango.Utils.Singleton<GameManager>.Instance().YamlLoaded += delegate
		{
			_recipeContainer.InitRecipes(SingletonDict<string, Yaml.Recipe>.Instance);
			_recipeContainer.InitBlueprints(SingletonDict<string, Yaml.Blueprint>.Instance, SingletonDict<int, ArtifactPrototype>.Instance);
			if (GameManager.ClusterMode != Mode.Offline)
			{
				_recipeContainer.InitAbstractNaturalBlueprint(DataHelper.GetBiomeSpriteInfos());
			}
			_recipeContainer.InitNotifications();
			RemodelingBlueprints.Initialize(SingletonDict<string, Dictionary<string, RemodelingBlueprint>>.Instance, SingletonDict<int, ArtifactPrototype>.Instance);
		};
	}

	private void Start()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += delegate
		{
			if (_trackingTodo != null && _trackingTodo.Refresh())
			{
				GameSystem<ToDoListSystem>.Instance().SetUpdated(_trackingTodo);
			}
		};
	}

	private void OnReady()
	{
		Connections.Frontend.Send(default(GetRecipes));
		Connections.Frontend.Send(default(GetArtifactBlueprints));
		string trackingToDo = (TrackingTarget = Preferences.GetString("TrackingRecipe", null, Preferences.Level.Player));
		SetTrackingToDo(trackingToDo);
	}

	private void OnRecipeListMsg(Recipes m, PacketHeader header)
	{
		_recipeContainer.SetAvailableList(m.Ids, m.NewRecipeIds, RecipeType.Crafting);
		_recipeContainer.SetLikeList(m.LikedRecipeIds, RecipeType.Crafting);
		if (this.RecipeItemsUpdated != null)
		{
			this.RecipeItemsUpdated(RecipeType.Crafting);
		}
	}

	private void OnBlueprintListMsg(ArtifactBlueprints m, PacketHeader header)
	{
		_recipeContainer.SetAvailableList(m.Ids, m.NewBlueprintIds, RecipeType.Building);
		_recipeContainer.SetLikeList(m.LikedBlueprintIds, RecipeType.Building);
		if (this.RecipeItemsUpdated != null)
		{
			this.RecipeItemsUpdated(RecipeType.Building);
		}
	}

	public void SetTrackingTarget(string id)
	{
		if (!(TrackingTarget == id))
		{
			TrackingTarget = id;
			Preferences.SetString("TrackingRecipe", id, Preferences.Level.Player);
			SetTrackingToDo(id);
		}
	}

	private void SetTrackingToDo(string id)
	{
		ItemSlotsTodoCollection trackingTodo = _trackingTodo;
		_trackingTodo = null;
		if (!string.IsNullOrEmpty(id))
		{
			Crafting.Recipe recipe = GetRecipe(id);
			if (recipe != null)
			{
				_trackingTodo = new RecipeTodoCollection(recipe);
			}
			else
			{
				Building.Blueprint blueprint = GetBlueprint(id);
				if (blueprint != null)
				{
					_trackingTodo = new BlueprintTodoCollection(blueprint);
				}
			}
		}
		if (_trackingTodo != null)
		{
			GameSystem<ToDoListSystem>.Instance().Add(_trackingTodo, immediately: true);
		}
		if (trackingTodo != null)
		{
			GameSystem<ToDoListSystem>.Instance().Remove(trackingTodo, immediately: true);
		}
	}

	public void LikeRecipe(string id, bool like, Action onSuccess = null)
	{
		Connections.Frontend.Send(new SetRecipeLike
		{
			RecipeId = id,
			Like = like
		}).On(delegate(Recipes msg, PacketHeader header)
		{
			OnRecipeListMsg(msg, header);
			if (onSuccess != null)
			{
				onSuccess();
			}
		});
	}

	public void LikeBlueprint(string id, bool like, Action onSuccess = null)
	{
		Connections.Frontend.Send(new SetBlueprintLike
		{
			BlueprintId = id,
			Like = like
		}).On(delegate(ArtifactBlueprints msg, PacketHeader header)
		{
			OnBlueprintListMsg(msg, header);
			if (onSuccess != null)
			{
				onSuccess();
			}
		});
	}

	public Crafting.Recipe GetRecipe(string id)
	{
		return _recipeContainer.GetRecipe(id);
	}

	public Building.Blueprint GetBlueprint(string id)
	{
		return _recipeContainer.GetBlueprint(id);
	}

	public Building.Blueprint GetBlueprint(int entityType)
	{
		return _recipeContainer.GetBlueprint(entityType);
	}

	public static bool HasMaterials([NotNull] Crafting.Recipe recipe, int quantity = 1)
	{
		bool hasTool;
		return HasMaterials(recipe, out hasTool, null, quantity);
	}

	public static bool HasMaterials([NotNull] Crafting.Recipe recipe, out bool hasTool, List<int> slotCount, int quantity = 1)
	{
		return HasMaterials(recipe.Slots, (!recipe.HasRequiredTool) ? null : recipe.AllowedTool, Point2.zero, out hasTool, slotCount, quantity);
	}

	public static bool HasMaterials([NotNull] Building.Blueprint blueprint, int quantity = 1)
	{
		bool hasTool;
		return HasMaterials(blueprint, Point2.zero, out hasTool, null, quantity);
	}

	public static bool HasMaterials([NotNull] Building.Blueprint blueprint, Point2 size, out bool hasTool, List<int> slotCount, int quantity = 1)
	{
		return HasMaterials(blueprint.Slots, (blueprint.AllowedToolTag.Count <= 0) ? null : blueprint.AllowedToolTag, (!blueprint.IsSizeVariable) ? Point2.zero : size, out hasTool, slotCount, quantity);
	}

	private static bool HasMaterials(IEnumerable<ItemSlot> slots, OrTagFilter toolFilter, Point2 size, out bool hasTool, List<int> slotCount, int quantity)
	{
		List<ItemData> playerItemList = GameSystem<InventorySystem>.Instance().PlayerItemList;
		int num = 0;
		BipartiteMatching.Reset();
		bool flag = toolFilter != null;
		foreach (ItemSlot slot in slots)
		{
			int num2 = 1;
			int num3 = 0;
			if (!(slot is Crafting.RecipeSlot recipeSlot))
			{
				if (slot is Building.BlueprintSlot blueprintSlot)
				{
					if (size.x > 0 && size.y > 0)
					{
						num2 = blueprintSlot.GetSlotCountModifier(size);
					}
					num3 = blueprintSlot.Count * num2;
				}
			}
			else
			{
				num3 = recipeSlot.Count;
			}
			num3 *= quantity;
			if (num3 <= 0)
			{
				continue;
			}
			for (int i = 0; i < playerItemList.Count; i++)
			{
				if (slot.IsSuitableItem(playerItemList[i]))
				{
					for (int j = 0; j < num3; j++)
					{
						BipartiteMatching.SetLink(num + j, i);
					}
				}
			}
			num += num3;
		}
		if (flag)
		{
			for (int k = 0; k < playerItemList.Count; k++)
			{
				if (!playerItemList[k].IsDestroyed() && playerItemList[k].HasTag(toolFilter))
				{
					BipartiteMatching.SetLink(num, k);
				}
			}
			num++;
		}
		int num4 = BipartiteMatching.Match();
		if (slotCount != null)
		{
			slotCount.Clear();
			int num5 = 0;
			foreach (ItemSlot slot2 in slots)
			{
				int num6 = 1;
				int num7 = 0;
				if (!(slot2 is Crafting.RecipeSlot recipeSlot2))
				{
					if (slot2 is Building.BlueprintSlot blueprintSlot2)
					{
						if (size.x > 0 && size.y > 0)
						{
							num6 = blueprintSlot2.GetSlotCountModifier(size);
						}
						num7 = blueprintSlot2.Count * num6;
					}
				}
				else
				{
					num7 = recipeSlot2.Count;
				}
				int num8 = 0;
				for (int l = 0; l < num7; l++)
				{
					int index = num5 + l;
					if (l == 0)
					{
						num8 = BipartiteMatching.GetRemainCount(index);
					}
					if (BipartiteMatching.GetLink(index) != -1)
					{
						num8++;
					}
				}
				slotCount.Add(num8);
				num5 += num7;
			}
		}
		hasTool = !flag || BipartiteMatching.GetLink(num - 1) != -1;
		return num4 >= num;
	}

	public void RefreshNearWorkbenches(Artifact workbench = null)
	{
		if (workbench != null)
		{
			_nearWorkbenches = new Artifact[1] { workbench };
		}
		else
		{
			_nearWorkbenches = FindNearArtifacts();
		}
	}

	private static Artifact[] FindNearArtifacts()
	{
		List<GameObject> list = new List<GameObject>();
		int mask = LayerHelper.PropMask;
		InteractionSystem.GetNearObjectsInternal(list, mask, 1200f, InteractionSystem.ArtifactObjectFilter);
		Artifact[] array = new Artifact[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = list[i].GetComponent<Artifact>();
		}
		return array;
	}

	public Artifact FindNearestAvailableWorkbench(Crafting.Recipe recipe)
	{
		if (_nearWorkbenches == null)
		{
			return null;
		}
		float num = float.MaxValue;
		Artifact result = null;
		int num2 = _nearWorkbenches.Length;
		for (int i = 0; i < num2; i++)
		{
			Artifact artifact = _nearWorkbenches[i];
			if (recipe.IsValidWorkbench(artifact))
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
		_recipeContainer.EnumerateRecipes(delegate(Crafting.Recipe recipe)
		{
			if (recipe.Available)
			{
				for (int i = 0; i < recipe.Slots.Length; i++)
				{
					Crafting.RecipeSlot recipeSlot = recipe.Slots[i];
					if (recipeSlot.Count > 0 && (recipeSlot.AllowedTags.Count > 0 || recipeSlot.AllowedMaterials.Count > 0) && recipeSlot.IsSuitableItem(itemData))
					{
						hashSet.Add(recipe);
					}
				}
			}
		});
	}

	public void FillAvailableBlueprintsByItemData(HashSet<Building.Blueprint> hashSet, ItemData itemData)
	{
		hashSet.Clear();
		_recipeContainer.EnumerateBlueprints(delegate(Building.Blueprint blueprint)
		{
			if (blueprint.Available)
			{
				for (int i = 0; i < blueprint.Slots.Length; i++)
				{
					Building.BlueprintSlot blueprintSlot = blueprint.Slots[i];
					if (blueprintSlot.Count > 0 && (blueprintSlot.AllowedTags.Count > 0 || blueprintSlot.AllowedMaterials.Count > 0) && blueprintSlot.IsSuitableItem(itemData))
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
			Dictionary<ColorChannel, string> dyeRecipe = Yaml.Util.Singleton<Constants>.Instance.Item.DyeRecipe;
			for (int i = 0; i < _dyeingRecipes.Length; i++)
			{
				_dyeingRecipes[i] = GetRecipe(dyeRecipe.Get((ColorChannel)i));
			}
		}
		return (channel >= ColorChannel.ColorR && (int)channel < _dyeingRecipes.Length) ? _dyeingRecipes[(int)channel] : null;
	}

	public Crafting.Recipe GetBleachingRecipe(ColorChannel channel)
	{
		if (_bleachingRecipes == null)
		{
			_bleachingRecipes = new Crafting.Recipe[3];
			Dictionary<ColorChannel, string> bleachRecipe = Yaml.Util.Singleton<Constants>.Instance.Item.BleachRecipe;
			for (int i = 0; i < _bleachingRecipes.Length; i++)
			{
				_bleachingRecipes[i] = GetRecipe(bleachRecipe.Get((ColorChannel)i));
			}
		}
		return (channel >= ColorChannel.ColorR && (int)channel < _bleachingRecipes.Length) ? _bleachingRecipes[(int)channel] : null;
	}

	/// <summary>
	/// คราฟตอนนี้ได้จริงไหม (มีวัตถุดิบครบ + มีโต๊ะทำงานที่ต้องใช้)
	///
	/// 🐛 [แก้เอง] เดิมบรรทัดแรกคือ `if (ClusterMode != Editable) return true;`
	/// ⇒ **ในโหมดต่อเซิร์ฟ ไม่เคยเช็ควัตถุดิบเลย** ทุกสูตรจึงดูคราฟได้หมดทั้งที่ของไม่มี
	/// (ตัวเช็คจริง HasMaterials อ่านจาก InventorySystem.PlayerItemList ซึ่งมาจากเซิร์ฟอยู่แล้ว
	///  จึงใช้ได้ทั้งสองโหมด ไม่มีเหตุผลต้องลัดข้าม)
	/// </summary>
	public bool CanCraftNow(CategoryItem categoryItem)
	{
		return WhyCannotCraft(categoryItem) == CraftBlocker.None;
	}

	/// <summary>
	/// [แก้เอง] เหตุผลที่สูตรนี้ยังทำไม่ได้ตอนนี้
	///
	/// เดิมมีแค่ <see cref="CanCraftNow" /> ที่ตอบ ได้/ไม่ได้ เฉย ๆ แล้วรายการคราฟก็ซ่อนอันที่ทำไม่ได้ทิ้ง
	/// พอ server บังคับ "ต้องยืนที่โต๊ะ + ต้องถือเครื่องมือ" จริง สูตรที่ต้องใช้โต๊ะ (587 จาก 720)
	/// ก็หายจากรายการทั้งหมดเวลาไม่ได้ยืนอยู่ที่โต๊ะ ⇒ **ผู้เล่นไม่มีทางรู้เลยว่าต้องไปสร้างอะไรก่อน**
	/// จึงต้องแยกให้ออกว่า "ขาดของ" (ซ่อนได้) กับ "ขาดโต๊ะ/เครื่องมือ" (ต้องโชว์พร้อมบอกเหตุผล)
	/// </summary>
	public CraftBlocker WhyCannotCraft(CategoryItem categoryItem)
	{
		if (categoryItem is Crafting.Recipe recipe)
		{
			bool hasTool;
			bool hasMaterials = HasMaterials(recipe, out hasTool, null);
			if (recipe.HasRequiredWorkbench && FindNearestAvailableWorkbench(recipe) == null)
			{
				return CraftBlocker.Workbench;
			}
			if (recipe.HasRequiredTool && !hasTool)
			{
				return CraftBlocker.Tool;
			}
			return hasMaterials ? CraftBlocker.None : CraftBlocker.Materials;
		}
		if (categoryItem is Building.Blueprint blueprint)
		{
			bool hasBuildTool;
			bool hasBuildMaterials = HasMaterials(blueprint, Point2.zero, out hasBuildTool, null);
			if (blueprint.AllowedToolTag.Count > 0 && !hasBuildTool)
			{
				return CraftBlocker.Tool;
			}
			return hasBuildMaterials ? CraftBlocker.None : CraftBlocker.Materials;
		}
		return CraftBlocker.Materials;
	}

	/// <summary>
	/// [แก้เอง] ข้อความสั้น ๆ ว่าขาดอะไร — เอาไปต่อท้ายชื่อสูตรในรายการ
	/// คืนค่าว่างถ้าทำได้อยู่แล้ว หรือขาดแค่วัตถุดิบ (อันนั้นดูจากช่องวัตถุดิบเอาเองได้)
	/// </summary>
	public string MissingRequirementText(CategoryItem categoryItem)
	{
		CraftBlocker blocker = WhyCannotCraft(categoryItem);
		if (!(categoryItem is Crafting.Recipe recipe))
		{
			return string.Empty;
		}
		switch (blocker)
		{
		case CraftBlocker.Workbench:
			return Durango.Logic.Item.Util.LocalizedTagRequiredMsg(recipe.AllowedWorkbench);
		case CraftBlocker.Tool:
			return Durango.Logic.Item.Util.LocalizedTagRequiredMsg(recipe.AllowedTool);
		default:
			return string.Empty;
		}
	}

	static RecipeSystem()
	{
		BipartiteMatching = new BipartiteMatching();
	}
}
