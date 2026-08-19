using System.Collections;
using System.Collections.Generic;
using Building_;
using Crafting;
using ItemSystem;
using JetBrains.Annotations;
using L10N;
using Shared.Ability;
using Shared.Item;
using Shared.System;
using UnityEngine;

public class RecipeSelectorGroup : UIBase, INewCheckerable
{
	[SerializeField]
	private KWidgetScrollView _mainScrollView;

	[SerializeField]
	private UIWidget _mainContainer;

	[SerializeField]
	private UIWidget _scaleContainer;

	[SerializeField]
	private RecipeSelector _recipeSelector;

	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private RecipeInfoWidget _recipeInfo;

	[SerializeField]
	private GameObject _noSelect;

	private RecipeSystem.RecipeType _selectedType;

	private IList<Artifact> _nearWorkbenchs;

	private Artifact _selectedWorkbench;

	private NewChecker _newChecker;

	public string SelectedCategory { get; private set; }

	public string SelectedRecipe { get; private set; }

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

	private void Start()
	{
		SetOpenCloseSound("Sound/Effect/UI/UI_Menu_Craft_Open_01.wav", "Sound/Effect/UI/UI_Menu_Craft_Close_01.wav");
		_recipeInfo.Init();
		HideDetailPanel();
		InitRecipeSelector();
		_titleWidget.OnClose += base.ForceClose;
		_recipeInfo.BuildSizeChanged += OnBuildSizeChange;
		_recipeInfo.Confirmed += OnConfirmRecipe;
		NewChecker.AddChild(GameSystem<RecipeSystem>.Instance().RecipeContainer);
		NewChecker.AddChild(GameSystem<RecipeSystem>.Instance().BlueprintContainer);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Craft, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if ((Object)(object)targetComponent != (Object)null)
			{
				Open(targetComponent);
			}
		});
		base.OnOpenSucceed += delegate
		{
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
			GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded();
			GameSystem<RecipeSystem>.Instance().RecipeItemsUpdated += OnUpdateRecipeItems;
			UpdateLayout();
			RefreshRecipeList();
			_nearWorkbenchs = (((Object)(object)_selectedWorkbench == (Object)null) ? RecipeSystem.FindNearWorkbenches() : new Artifact[1] { _selectedWorkbench });
		};
		base.OnCloseSucceed += delegate
		{
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
			GameSystem<RecipeSystem>.Instance().RecipeItemsUpdated -= OnUpdateRecipeItems;
			HideDetailPanel();
			_selectedWorkbench = null;
		};
		OnClose();
	}

	[UsedImplicitly]
	private void OnPortraitMode(bool isPortrait)
	{
		((Behaviour)_mainScrollView.ScrollView).enabled = isPortrait;
	}

	private void UpdateLayout()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		float num = ((!UIManager.IsPortraitMode) ? 1f : ((float)_mainContainer.width / (float)(_mainScrollView.GetNode(1).width + _mainScrollView.GetNode(2).width + _mainScrollView.Margin)));
		int width = (int)((float)_mainContainer.width / num);
		int height = (int)((float)_mainContainer.height / num);
		((Component)_scaleContainer).transform.localScale = Vector3.one * num;
		_scaleContainer.width = width;
		_scaleContainer.height = height;
		for (int i = 0; i < _mainScrollView.GetNodeCount(); i++)
		{
			_mainScrollView.GetNode(i).height = height;
		}
		UIUtility.UpdateAnchors(((Component)this).transform);
		_mainScrollView.UpdateLayout();
	}

	public void Open(RecipeSystem.RecipeType type, string id)
	{
		CategoryItem categoryItem = null;
		switch (type)
		{
		case RecipeSystem.RecipeType.Building:
			categoryItem = GameSystem<RecipeSystem>.Instance().GetBlueprint(id);
			break;
		case RecipeSystem.RecipeType.Crafting:
			categoryItem = GameSystem<RecipeSystem>.Instance().GetRecipe(id);
			break;
		}
		if (categoryItem == null)
		{
			if (base.IsOpen)
			{
				ForceClose();
			}
			return;
		}
		_selectedType = type;
		SelectedRecipe = categoryItem.Id;
		SelectedCategory = categoryItem.Category;
		if (base.IsOpen)
		{
			RefreshCurrentRecipe();
		}
		else
		{
			Open();
		}
		ScrollToRecipe(type, id);
	}

	public void Open(Artifact workbench)
	{
		_selectedWorkbench = workbench;
		Open();
	}

	public void QuickOpenCraftingUI(RecipeSystem.RecipeType type, string recipeId)
	{
		switch (type)
		{
		case RecipeSystem.RecipeType.Crafting:
			OpenItemCraftingUI(recipeId, quickFill: true);
			break;
		case RecipeSystem.RecipeType.Building:
			OpenBuildingUI(recipeId);
			break;
		}
	}

	public Transform FindCategoryTransform(string category)
	{
		CategoryControl categoryControl = _recipeSelector.FindCategory(category);
		return (!((Object)(object)categoryControl != (Object)null)) ? null : ((Component)categoryControl).transform;
	}

	public Transform FindRecipeTransform(string recipe)
	{
		CategoryNodeControl categoryNodeControl = _recipeSelector.FindRecipe(recipe);
		return (!((Object)(object)categoryNodeControl != (Object)null)) ? null : ((Component)categoryNodeControl).transform;
	}

	public void ScrollToRecipe(RecipeSystem.RecipeType type, string id)
	{
		_recipeSelector.ScrollToRecipe(type, id);
	}

	public Transform GetCraftButtonTransform()
	{
		return _recipeInfo.GetButtonTransform();
	}

	private void RefreshRecipeList()
	{
		List<RecipeCategory> list = new List<RecipeCategory>(GameSystem<RecipeSystem>.Instance().RecipeContainer.Categories);
		List<BlueprintCategory> list2 = new List<BlueprintCategory>(GameSystem<RecipeSystem>.Instance().BlueprintContainer.Categories);
		IList[] array = new IList[2] { list, list2 };
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			IList list3 = array[i];
			for (int num2 = list3.Count - 1; num2 >= 0; num2--)
			{
				if (!(list3[num2] is Category category))
				{
					list3.RemoveAt(num2);
				}
				else
				{
					bool flag = false;
					CategoryItem[] items = category.Items;
					int j = 0;
					for (int num3 = items.Length; j < num3; j++)
					{
						if (IsValidCategoryItem(items[j]))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						list3.RemoveAt(num2);
					}
				}
			}
		}
		_recipeSelector.SetCategories(list, list2);
		RefreshCurrentRecipe();
	}

	private void RefreshCurrentRecipe()
	{
		CategoryItem categoryItem = GameSystem<RecipeSystem>.Instance().GetCategoryItem(_selectedType, SelectedRecipe);
		if (categoryItem == null || !categoryItem.Available)
		{
			SelectedRecipe = null;
		}
		_recipeSelector.InitRecipeSelector(_selectedType, SelectedCategory, SelectedRecipe);
	}

	private void OnUpdateInventory()
	{
		if (base.IsOpen)
		{
			RefreshCurrentRecipe();
		}
	}

	private void OnUpdateRecipeItems(RecipeSystem.RecipeType type)
	{
		if (base.IsOpen)
		{
			RefreshRecipeList();
		}
	}

	private void InitRecipeSelector()
	{
		_recipeSelector.Init();
		_recipeSelector.OnSelectRecipeCategory = OnSelectCategory;
		_recipeSelector.OnSelectRecipeItem = OnSelectItem;
	}

	private bool IsValidCategoryItem(CategoryItem item)
	{
		bool result = false;
		if (item.Available)
		{
			if ((Object)(object)_selectedWorkbench == (Object)null)
			{
				result = true;
			}
			else if (item is Recipe { WorkbenchRequired: not false } recipe)
			{
				result = recipe.IsvalidWorkbench(_selectedWorkbench);
			}
		}
		return result;
	}

	private void OnSelectCategory(RecipeSystem.RecipeType type, string id)
	{
		RecipeSelector_UpdateSelectItem();
		if (SelectedCategory != id || _selectedType != type)
		{
			SelectedRecipe = null;
		}
		Category category = GameSystem<RecipeSystem>.Instance().GetCategory(type, id);
		_selectedType = type;
		SelectedCategory = id;
		CategoryItem[] items = category.Items;
		int num = ((items != null) ? items.Length : 0);
		Dictionary<string, List<RecipeSelector.RecipeItem>> dictionary = new Dictionary<string, List<RecipeSelector.RecipeItem>>();
		for (int i = 0; i < num; i++)
		{
			CategoryItem categoryItem = category.Items[i];
			if (!IsValidCategoryItem(categoryItem))
			{
				continue;
			}
			bool canCraft = false;
			string text = null;
			switch (type)
			{
			case RecipeSystem.RecipeType.Crafting:
			{
				Recipe recipe = (Recipe)categoryItem;
				canCraft = RecipeSystem.RecipeCanCrafting(recipe, _nearWorkbenchs);
				text = recipe.Subcategory;
				if (string.IsNullOrEmpty(text))
				{
					text = recipe.Category;
				}
				break;
			}
			case RecipeSystem.RecipeType.Building:
			{
				Blueprint blueprint = (Blueprint)categoryItem;
				canCraft = RecipeSystem.BlueprintCanBuild(blueprint);
				text = blueprint.SubCategory;
				if (string.IsNullOrEmpty(text))
				{
					text = blueprint.Category;
				}
				break;
			}
			}
			text = ((text != null) ? LocalizeSystem.Get("#recipe_category_" + text) : T._("알수없음"));
			RecipeSelector.RecipeItem item = default(RecipeSelector.RecipeItem);
			item.Item = categoryItem;
			item.CanCraft = canCraft;
			item.IsGuided = GameSystem<AutoGuideSystem>.Instance().IsGuided(categoryItem.Id);
			if (dictionary.TryGetValue(text, out var value))
			{
				value.Add(item);
				continue;
			}
			value = new List<RecipeSelector.RecipeItem>();
			value.Add(item);
			dictionary.Add(text, value);
		}
		_recipeSelector.SetRecipeItems(_selectedType, dictionary);
	}

	private void OnSelectItem(RecipeSystem.RecipeType type, string id)
	{
		bool reset = SelectedRecipe != id;
		SelectedRecipe = id;
		switch (type)
		{
		case RecipeSystem.RecipeType.Crafting:
		{
			Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(id);
			recipe.NewChecker.IsNew = false;
			ShowRecipeInfo(recipe);
			break;
		}
		case RecipeSystem.RecipeType.Building:
		{
			Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(id);
			blueprint.NewChecker.IsNew = false;
			ShowBlueprintInfo(blueprint);
			break;
		}
		}
		_recipeInfo.UpdateLayout(reset);
		if (UIManager.IsPortraitMode)
		{
			_mainScrollView.MoveToNode(1, instant: false);
		}
	}

	private void RecipeSelector_UpdateSelectItem()
	{
		_recipeSelector.GetSelectedData(out var _, out var _, out var recipe);
		if (recipe == null)
		{
			HideDetailPanel();
		}
	}

	private void ShowRecipeInfo(Recipe recipe)
	{
		if (recipe == null)
		{
			HideDetailPanel();
			return;
		}
		_noSelect.SetActive(false);
		_recipeInfo.Show();
		_recipeInfo.SetTitle(recipe.LocalizedName);
		_recipeInfo.SetDescription(recipe.Description);
		float time = ((!recipe.Entrusts) ? 0f : recipe.DurationWait);
		_recipeInfo.SetRemainTime(time, T._("[icon_clock_small:1.5] [D0CBC0]제작 시간[-]"));
		RecipeSystem.RecipeCheckCraftCondition(recipe, _nearWorkbenchs, out var slotCounts, out var hasTool, out var foundWorkbench);
		List<Tuple<string, string, string>> list = new List<Tuple<string, string, string>>();
		if (recipe.Type == CraftType.Modify)
		{
			list.Add(new Tuple<string, string, string>(T._("가공횟수 소모"), "1", "bg_itemview_work_white"));
		}
		if (recipe.WorkbenchRequired)
		{
			list.Add(new Tuple<string, string, string>(T._("제작대"), Util.LocalizedTagRequiredMsg(recipe.RequiredWorkbenches), (!foundWorkbench) ? "button_checkbox_normal" : "button_checkbox_selected"));
		}
		_recipeInfo.SetMaxLevel(recipe.MaxLevel);
		_recipeInfo.SetConditions(list);
		List<Tuple<string, int, int>> list2 = new List<Tuple<string, int, int>>();
		Tuple<string, int, int> toolInfo = null;
		int i = 0;
		for (int size = KUtility.GetSize(recipe.Slots); i < size; i++)
		{
			RecipeSlot recipeSlot = recipe.Slots[i];
			list2.Add(new Tuple<string, int, int>(recipeSlot.LocalizedName, slotCounts[i], recipeSlot.CountMin));
		}
		if (recipe.ToolRequired)
		{
			toolInfo = new Tuple<string, int, int>(Util.LocalizedTagRequiredMsg(recipe.ToolTags), hasTool ? 1 : 0, 1);
		}
		_recipeInfo.SetNonResiable();
		_recipeInfo.SetMaterials(list2, toolInfo);
		_recipeInfo.SetNextButton(T._("제작"));
	}

	private void ShowBlueprintInfo(Blueprint blueprint)
	{
		if (blueprint == null)
		{
			HideDetailPanel();
			return;
		}
		_noSelect.SetActive(false);
		_recipeInfo.Show();
		_recipeInfo.SetTitle(blueprint.LocalizedName);
		_recipeInfo.SetDescription(blueprint.Description);
		_recipeInfo.SetRemainTime(0f, LocalizeSystem.Get("#build_duration_wait_label"));
		_recipeInfo.SetMaxLevel(0);
		_recipeInfo.SetConditions(null);
		if (blueprint.IsSizeVariable)
		{
			int num = blueprint.Size.x;
			int num2 = blueprint.Size.y;
			if (blueprint.IsModular)
			{
				int num3 = GameSystem<StatisticsSystem>.Instance().DerivedAbilities.Get(Derived.MaxModularSize, 0);
				num = ((num > num3) ? num3 : num);
				num2 = ((num2 > num3) ? num3 : num2);
			}
			_recipeInfo.SetResizable(num, num2);
		}
		else
		{
			_recipeInfo.SetNonResiable();
		}
		RecipeSystem.BlueprintCheckBuildCondition(blueprint, _recipeInfo.Size, out var slotCounts, out var hasTool);
		List<Tuple<string, int, int>> list = new List<Tuple<string, int, int>>();
		Tuple<string, int, int> toolInfo = null;
		int i = 0;
		for (int size = KUtility.GetSize(blueprint.Slots); i < size; i++)
		{
			BlueprintSlot blueprintSlot = blueprint.Slots[i];
			int num4 = 1;
			if (blueprint.IsSizeVariable)
			{
				num4 = BuildManager.GetBlueprintSlotCountModifier(blueprintSlot, _recipeInfo.Size);
			}
			list.Add(new Tuple<string, int, int>(blueprintSlot.LocalizedName, slotCounts[i], blueprintSlot.RequiredCount * num4));
		}
		if (blueprint.ToolRequired)
		{
			toolInfo = new Tuple<string, int, int>(Util.LocalizedTagRequiredMsg(blueprint.ToolTags), hasTool ? 1 : 0, 1);
		}
		_recipeInfo.SetMaterials(list, toolInfo);
		_recipeInfo.SetNextButton(T.GetParticularString("동사", "건설"));
	}

	private void HideDetailPanel()
	{
		_recipeInfo.Hide();
		_noSelect.SetActive(true);
		if (UIManager.IsPortraitMode)
		{
			_mainScrollView.MoveToNode(0, instant: false);
		}
	}

	private Recipe OpenItemCraftingUI(string recipeId, bool quickFill = false)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		ItemCraftingGroup craftingGroup = UIManager.FindScript<ItemCraftingGroup>();
		Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(recipeId);
		if (recipe != null)
		{
			if (recipe.WorkbenchRequired)
			{
				Artifact workbench = GetValidWorkbench(recipe);
				if ((Object)(object)workbench == (Object)null)
				{
					UIManager.SystemMsg(T._("주변에 제작대가 필요합니다.\n<alert>{0}</alert>", Util.LocalizedTagRequiredMsg(recipe.RequiredWorkbenches)));
				}
				else
				{
					ForceClose();
					KSingleton<PlayerController>.Instance().MoveToTarget(workbench.Center, delegate
					{
						craftingGroup.Open(recipe, workbench, quickFill);
					}, 100f * ((float)Mathf.Max(workbench.Size.x, workbench.Size.y) + 0.5f));
				}
			}
			else
			{
				craftingGroup.Open(recipe, null, quickFill);
			}
		}
		return recipe;
	}

	private Blueprint OpenBuildingUI(string recipeId)
	{
		BuildGridGroup buildGridGroup = UIManager.FindScript<BuildGridGroup>();
		Point2 size = _recipeInfo.Size;
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(recipeId);
		if (blueprint != null)
		{
			if (blueprint.IsSizeVariable)
			{
				buildGridGroup.Open(blueprint, size);
			}
			else
			{
				buildGridGroup.Open(blueprint);
			}
		}
		return blueprint;
	}

	private void OnBuildSizeChange(int x, int y)
	{
		OnSelectItem(_selectedType, SelectedRecipe);
	}

	private void OnConfirmRecipe()
	{
		LeftMenuListGroup leftMenuListGroup = UIManager.FindScript<LeftMenuListGroup>();
		switch (_selectedType)
		{
		case RecipeSystem.RecipeType.Crafting:
		{
			Recipe recipe = OpenItemCraftingUI(SelectedRecipe);
			if (recipe != null)
			{
				leftMenuListGroup.SetLastOpenCraft(recipe.Icon, _selectedType, SelectedRecipe);
			}
			break;
		}
		case RecipeSystem.RecipeType.Building:
		{
			Blueprint blueprint = OpenBuildingUI(SelectedRecipe);
			if (blueprint != null)
			{
				leftMenuListGroup.SetLastOpenCraft(blueprint.Icon, _selectedType, SelectedRecipe);
			}
			break;
		}
		}
	}

	private Artifact GetValidWorkbench(Recipe recipe)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (recipe.WorkbenchRequired)
		{
			if ((Object)(object)_selectedWorkbench == (Object)null)
			{
				Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
				Artifact result = null;
				float num = float.MaxValue;
				int i = 0;
				for (int size = KUtility.GetSize(_nearWorkbenchs); i < size; i++)
				{
					Artifact artifact = _nearWorkbenchs[i];
					if (recipe.IsvalidWorkbench(artifact))
					{
						Vector3 val = artifact.Center - currentPosition;
						float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							result = artifact;
						}
					}
				}
				return result;
			}
			if (recipe.IsvalidWorkbench(_selectedWorkbench))
			{
				return _selectedWorkbench;
			}
		}
		return null;
	}
}
