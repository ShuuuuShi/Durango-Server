using System.Collections.Generic;
using System.Linq;
using Building;
using Crafting;
using Durango.Logic;
using Durango.Logic.Clusters;
using Durango.Logic.Item;
using Durango.Logic.Notification;
using Durango.Logic.Skill;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Shared.Ability;
using Shared.Skill;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Recipe")]
public class RecipeSelectorGroup : UIBase, INotificationable
{
	private class LastSelectedInfo
	{
		private readonly Dictionary<string, RecipeListWidget.SelectInfo?> _dictionary = new Dictionary<string, RecipeListWidget.SelectInfo?>();

		public RecipeListWidget.SelectInfo? Get([CanBeNull] Crafting.Category category)
		{
			if (_dictionary.TryGetValue(GetKey(category), out var value))
			{
				return value;
			}
			return null;
		}

		public void Set([CanBeNull] Crafting.Category category, RecipeListWidget.SelectInfo? lastSelected)
		{
			string key = GetKey(category);
			if (lastSelected.HasValue)
			{
				_dictionary[key] = lastSelected;
			}
			else
			{
				_dictionary.Remove(key);
			}
		}

		[NotNull]
		private static string GetKey([CanBeNull] Crafting.Category category)
		{
			if (category != null)
			{
				return category.Id;
			}
			return "entire_category";
		}
	}

	[SerializeField]
	private CategoryListWidget _categoryListWidget;

	[SerializeField]
	private RecipeListWidget _recipeListWidget;

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private RecipeInfoWidget _recipeInfo;

	[SerializeField]
	private RecipeBuildCheatWidget _recipeBuildCheatWidget;

	[SerializeField]
	private GameObject _noSelect;

	private readonly Container _notification = new Container();

	private readonly List<Crafting.Category> _validCategories = new List<Crafting.Category>();

	private readonly Dictionary<string, RecipeListWidget.SubList> _currentRecipes = new Dictionary<string, RecipeListWidget.SubList>();

	private readonly LastSelectedInfo _lastSelectedInfo = new LastSelectedInfo();

	private Artifact _selectedWorkbench;

	private bool _scrollToLastSelectedRecipe;

	private bool _isBuildCheat;

	[CanBeNull]
	public string SelectedCategoryId { get; private set; }

	[CanBeNull]
	public string SelectedRecipeId { get; private set; }

	public Notification Notification => _notification;

	private void Start()
	{
		_openCloseSound = UISound.GroupType.RecipeSelect;
		HideDetailPanel();
		_titleWidget.Object.SetTitle(T._("제작/건설"));
		_categoryListWidget.CategorySelected += CategoryListWidget_CategorySelected;
		_recipeListWidget.RecipeSelected += RecipeListWidget_RecipeSelected;
		_recipeInfo.BuildSizeChanged += RecipeInfo_BuildSizeChanged;
		_recipeInfo.Confirmed += RecipeInfo_Confirmed;
		_recipeInfo.LikeButtonClicked += RecipeInfo_LikeButtonClicked;
		_notification.AddChild(GameSystem<RecipeSystem>.Instance().RecipeContainer);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Craft, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if (targetComponent != null)
			{
				Open(targetComponent);
			}
		});
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += InventorySystem_PlayerInventoryUpdated;
		GameSystem<RecipeSystem>.Instance().RecipeItemsUpdated += RecipeSystem_RecipeItemsUpdated;
		base.OnOpenSucceed += delegate
		{
			_isBuildCheat = GameManager.ClusterMode == Mode.Editable;
			_recipeListWidget.ClearSearchText();
			ResetCategories(scrollToSelected: true);
			GameSystem<RecipeSystem>.Instance().RefreshNearWorkbenches(_selectedWorkbench);
		};
		base.OnCloseSucceed += delegate
		{
			HideDetailPanel();
			_selectedWorkbench = null;
		};
		SetChildrenActive(activated: false);
	}

	public void Open(RecipeSystem.RecipeType type, string id)
	{
		if (!GameSystem<RecipeSystem>.Instance().GetCategoryItem(type, id, out var category, out var item))
		{
			if (base.IsOpened)
			{
				ForceClose();
			}
			return;
		}
		SelectedCategoryId = category.Id;
		_lastSelectedInfo.Set(category, new RecipeListWidget.SelectInfo(item));
		if (base.IsOpened)
		{
			Open();
			Refresh();
		}
		else
		{
			Open();
		}
	}

	public void Open(Artifact workbench)
	{
		_selectedWorkbench = workbench;
		Open();
	}

	public void QuickOpenCraftingUI(RecipeSystem.RecipeType type, string recipeId)
	{
		GameSystem<RecipeSystem>.Instance().RefreshNearWorkbenches();
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

	public void ScrollToRecipe(RecipeSystem.RecipeType type, string id, bool instant = false)
	{
		if (GameSystem<RecipeSystem>.Instance().GetCategoryItem(type, id, out var category, out var item) && SelectedCategoryId == category.Id)
		{
			_recipeListWidget.ScrollToRecipe(new RecipeListWidget.SelectInfo(item), instant);
		}
	}

	public Transform FindCategoryTransform(string categoryId)
	{
		CategoryWidget categoryWidget = _categoryListWidget.FindCategory(categoryId);
		if (categoryWidget != null)
		{
			return categoryWidget.transform;
		}
		return null;
	}

	public Transform FindRecipeTransform(string recipe)
	{
		RecipeItemWidget recipeItemWidget = _recipeListWidget.FindRecipe(recipe);
		if (recipeItemWidget != null)
		{
			return recipeItemWidget.transform;
		}
		return null;
	}

	public Transform GetCraftButtonTransform()
	{
		return _recipeInfo.GetButtonTransform();
	}

	public static void OpenRecipeOrLearnableUI(RecipeSystem.RecipeType type, string id)
	{
		Crafting.Recipe recipe = null;
		Building.Blueprint blueprint = null;
		switch (type)
		{
		case RecipeSystem.RecipeType.None:
			recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(id);
			if (recipe == null)
			{
				blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(id);
				if (blueprint != null)
				{
					type = RecipeSystem.RecipeType.Building;
				}
			}
			else
			{
				type = RecipeSystem.RecipeType.Crafting;
			}
			break;
		case RecipeSystem.RecipeType.Crafting:
			recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(id);
			break;
		case RecipeSystem.RecipeType.Building:
			blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(id);
			break;
		}
		List<Bundle> skills = GameSystem<SkillSystem>.Instance().Skills;
		switch (type)
		{
		case RecipeSystem.RecipeType.Crafting:
			if (recipe != null && recipe.Available)
			{
				RecipeSelectorGroup recipeSelectorGroup2 = UIManager.FindScript<RecipeSelectorGroup>();
				if (recipeSelectorGroup2 != null)
				{
					recipeSelectorGroup2.Open(RecipeSystem.RecipeType.Crafting, id);
				}
				return;
			}
			break;
		case RecipeSystem.RecipeType.Building:
			if (blueprint != null && blueprint.Available)
			{
				RecipeSelectorGroup recipeSelectorGroup = UIManager.FindScript<RecipeSelectorGroup>();
				if (recipeSelectorGroup != null)
				{
					recipeSelectorGroup.Open(RecipeSystem.RecipeType.Building, id);
				}
				return;
			}
			break;
		}
		for (int i = 0; i < skills.Count; i++)
		{
			int j = 0;
			for (int num = KUtility.GetSize(skills[i].Sub) + 1; j < num; j++)
			{
				Durango.Logic.Skill.Skill skill = ((j != 0) ? skills[i].Sub[j - 1] : skills[i].Base);
				if (skill == null)
				{
					continue;
				}
				for (int k = 1; k <= skill.MaxLevel; k++)
				{
					Node node = skill.Get(k);
					int l = 0;
					for (int size = KUtility.GetSize(node.Rewards); l < size; l++)
					{
						Durango.Logic.Skill.Reward reward = node.Rewards[l];
						if (type == RecipeSystem.RecipeType.Crafting && reward.Type == RewardType.Recipe && reward.RecipeIds != null)
						{
							if (reward.RecipeIds.Contains(id))
							{
								SkillGroup skillGroup = UIManager.FindScript<SkillGroup>();
								if (skillGroup != null)
								{
									skillGroup.Open(node);
								}
								return;
							}
						}
						else if (type == RecipeSystem.RecipeType.Building && reward.Type == RewardType.Blueprint && reward.BlueprintIds != null && reward.BlueprintIds.Contains(id))
						{
							SkillGroup skillGroup2 = UIManager.FindScript<SkillGroup>();
							if (skillGroup2 != null)
							{
								skillGroup2.Open(node);
							}
							return;
						}
					}
				}
			}
		}
		foreach (KeyValuePair<Derived, DerivedRewardData[]> item in SingletonDict<Derived, DerivedRewardData[]>.Instance)
		{
			if (item.Value == null)
			{
				continue;
			}
			DerivedRewardData[] value = item.Value;
			foreach (DerivedRewardData derivedRewardData in value)
			{
				DerivedReward derivedReward = SingletonDict<string, DerivedReward>.Get(derivedRewardData.RewardId);
				if (derivedReward != null && ((type == RecipeSystem.RecipeType.Crafting && derivedReward.RecipeId == id) || (type == RecipeSystem.RecipeType.Building && derivedReward.BlueprintId == id)))
				{
					RepresentTypePopup representTypePopup = UIManager.Popup.Tooltip<RepresentTypePopup>();
					if (representTypePopup.FocusReward(derivedRewardData.RewardId))
					{
						UIManager.Open<CharacterInfoGroup>();
						representTypePopup.Show();
					}
					return;
				}
			}
		}
	}

	[Uri("Crafting")]
	private void RecipeUri(string recipe)
	{
		OpenRecipeOrLearnableUI(RecipeSystem.RecipeType.Crafting, recipe);
	}

	[Uri("Building")]
	private void BlueprintUri(string blueprint)
	{
		OpenRecipeOrLearnableUI(RecipeSystem.RecipeType.Building, blueprint);
	}

	[Uri]
	private void RecipeOrBlueprintUri(string id)
	{
		OpenRecipeOrLearnableUI(RecipeSystem.RecipeType.None, id);
	}

	[Uri("Bonus")]
	private void RecipeBonusInfoPopup(string id)
	{
		int num = UriParser.GetArgument("lv").ToInt();
		int? level = null;
		if (num > 0)
		{
			level = num;
		}
		UIManager.Popup.Tooltip<ReceiveRewardsPopup>().ShowRecipeBonusInfo(id, level);
	}

	[Uri("LastBuildCheat")]
	private void BuildCheatUri()
	{
		_recipeBuildCheatWidget.SelectLastCheatBuildGrid();
	}

	private void ResetCategories(bool scrollToSelected)
	{
		_validCategories.Clear();
		foreach (Crafting.Category category in GameSystem<RecipeSystem>.Instance().RecipeContainer.Categories)
		{
			if (HasValidCategoryItems(category))
			{
				_validCategories.Add(category);
			}
		}
		if (!_validCategories.Any((Crafting.Category category) => category.Id == SelectedCategoryId))
		{
			SelectedCategoryId = null;
		}
		_categoryListWidget.ResetCategories(_validCategories);
		Refresh(scrollToSelected);
	}

	private void Refresh(bool scrollToSelected = false)
	{
		_scrollToLastSelectedRecipe = scrollToSelected;
		_categoryListWidget.SelectCategory(SelectedCategoryId);
	}

	private bool RestoreRecipeSelection(bool resetPosition)
	{
		RecipeListWidget.SelectInfo? info = _lastSelectedInfo.Get(_categoryListWidget.SelectedCategory);
		if (info.HasValue && resetPosition && _categoryListWidget.SelectedCategory == null && !info.Value.InFavorites)
		{
			info = null;
		}
		return _recipeListWidget.SelectRecipe(info);
	}

	private bool IsValidCategoryItem(CategoryItem item)
	{
		if (!item.Available)
		{
			return false;
		}
		if (_selectedWorkbench == null)
		{
			return true;
		}
		if (item is Crafting.Recipe recipe)
		{
			return recipe.IsValidWorkbench(_selectedWorkbench);
		}
		return false;
	}

	private bool HasValidCategoryItems(Crafting.Category category)
	{
		for (int i = 0; i < category.ItemCount; i++)
		{
			if (IsValidCategoryItem(category.GetItem(i)))
			{
				return true;
			}
		}
		return false;
	}

	private void AddRecipeItems([NotNull] Crafting.Category category, Dictionary<string, RecipeListWidget.SubList> result, bool inEntireCategory, RecipeListWidget.SubList favorites = null)
	{
		for (int i = 0; i < category.ItemCount; i++)
		{
			CategoryItem item = category.GetItem(i);
			if (IsValidCategoryItem(item))
			{
				GetSubListTitle(item, out var key, out var text);
				GetOrCreateRecipeSubList(key, text, inEntireCategory, result).AddItem(item);
				if (item.Like)
				{
					favorites?.AddItem(item);
				}
			}
		}
	}

	[NotNull]
	private static RecipeListWidget.SubList GetOrCreateRecipeSubList([NotNull] string key, [NotNull] string title, bool inEntireCategory, Dictionary<string, RecipeListWidget.SubList> result)
	{
		if (!result.TryGetValue(key, out var value))
		{
			value = new RecipeListWidget.SubList(inEntireCategory);
			value.Text = title;
			result.Add(key, value);
		}
		return value;
	}

	private static void GetSubListTitle(CategoryItem item, out string key, out string text)
	{
		key = null;
		if (item is Crafting.Recipe)
		{
			key = ((Crafting.Recipe)item).Subcategory;
		}
		else if (item is Building.Blueprint)
		{
			key = ((Building.Blueprint)item).SubCategory;
			if (int.TryParse(key, out var _))
			{
				text = string.Empty;
				return;
			}
		}
		if (string.IsNullOrEmpty(key))
		{
			key = item.Category;
		}
		if (key == null)
		{
			text = T._("알수없음");
		}
		else
		{
			text = LocalizeSystem.Get("#recipe_category_" + key);
		}
	}

	private void RefreshDetailPanel(bool reset)
	{
		RecipeListWidget.SelectInfo? selectedRecipe = _recipeListWidget.SelectedRecipe;
		if (selectedRecipe.HasValue)
		{
			if (_isBuildCheat && selectedRecipe.Value.RecipeType == RecipeSystem.RecipeType.Building)
			{
				Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(selectedRecipe.Value.Id);
				if (blueprint != null)
				{
					_recipeBuildCheatWidget.Show(blueprint);
					_recipeInfo.Hide();
					_noSelect.SetActive(value: false);
					return;
				}
			}
			if (_recipeInfo.Show(selectedRecipe.Value.RecipeType, selectedRecipe.Value.Id, reset))
			{
				_noSelect.SetActive(value: false);
				_recipeBuildCheatWidget.Hide();
				return;
			}
		}
		HideDetailPanel();
	}

	private void HideDetailPanel()
	{
		_recipeInfo.Hide();
		_recipeBuildCheatWidget.Hide();
		_noSelect.SetActive(value: true);
	}

	private Crafting.Recipe OpenItemCraftingUI(string recipeId, bool quickFill = false)
	{
		Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(recipeId);
		if (recipe == null)
		{
			return null;
		}
		if (!recipe.HasRequiredRecipe())
		{
			Crafting.Recipe recipe2 = GameSystem<RecipeSystem>.Instance().GetRecipe(recipe.RequiredRecipe);
			UIManager.SystemMsg(T._("<em>{0}</em> 제작법이 필요합니다.", (recipe2 != null) ? recipe2.Name : recipe.RequiredRecipe));
			return null;
		}
		CraftGroupBase craftingGroup = UIManager.FindScript<CraftGroupBase>();
		if (recipe.HasRequiredWorkbench)
		{
			Artifact workbench = GetValidWorkbench(recipe);
			if (workbench == null)
			{
				UIManager.SystemMsg(T._("주변에 제작대가 필요합니다.\n<alert>{0}</alert>", Durango.Logic.Item.Util.LocalizedTagRequiredMsg(recipe.AllowedWorkbench)));
				return null;
			}
			UIBase.CloseAllUI();
			Durango.Utils.Singleton<PlayerController>.Instance().MoveToPosition(workbench.InteractionPosition, delegate
			{
				craftingGroup.Open(recipe, workbench, quickFill);
			}, 100f * ((float)Mathf.Max(workbench.Size.x, workbench.Size.y) + 0.5f));
		}
		else
		{
			craftingGroup.Open(recipe, null, quickFill);
		}
		return recipe;
	}

	private Building.Blueprint OpenBuildingUI(string recipeId)
	{
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(recipeId);
		if (blueprint == null)
		{
			return null;
		}
		if (!blueprint.HasRequiredBlueprint())
		{
			Building.Blueprint blueprint2 = GameSystem<RecipeSystem>.Instance().GetBlueprint(blueprint.RequiredBlueprint);
			UIManager.SystemMsg(T._("<em>{0}</em> 건설법이 필요합니다.", (blueprint2 != null) ? blueprint2.Name : blueprint.RequiredBlueprint));
			return null;
		}
		BuildGridGroupBase buildGridGroupBase = UIManager.FindScript<BuildGridGroupBase>();
		Point2 size = _recipeInfo.Size;
		if (blueprint.IsSizeVariable)
		{
			buildGridGroupBase.Open(blueprint, size, 1, hasRoof: true, null, GameSystem<BuildSystem>.Instance().OccupyArtifactSite);
		}
		else
		{
			buildGridGroupBase.Open(blueprint, GameSystem<BuildSystem>.Instance().OccupyArtifactSite);
		}
		return blueprint;
	}

	private Artifact GetValidWorkbench(Crafting.Recipe recipe)
	{
		if (_selectedWorkbench != null)
		{
			if (recipe.IsValidWorkbench(_selectedWorkbench))
			{
				return _selectedWorkbench;
			}
			return null;
		}
		return GameSystem<RecipeSystem>.Instance().FindNearestAvailableWorkbench(recipe);
	}

	private void CategoryListWidget_CategorySelected()
	{
		string text = ((_categoryListWidget.SelectedCategory == null) ? null : _categoryListWidget.SelectedCategory.Id);
		bool flag = text == null;
		bool flag2 = _scrollToLastSelectedRecipe;
		_scrollToLastSelectedRecipe = false;
		if (SelectedCategoryId != text)
		{
			SelectedCategoryId = text;
			flag2 = true;
		}
		_currentRecipes.Clear();
		if (flag)
		{
			RecipeListWidget.SubList subList = new RecipeListWidget.SubList(flag, isFavorites: true);
			foreach (Crafting.Category validCategory in _validCategories)
			{
				AddRecipeItems(validCategory, _currentRecipes, flag, subList);
			}
			if (subList.Count > 0)
			{
				subList.Text = T._("즐겨찾기");
				_currentRecipes.Add("_favorite", subList);
			}
		}
		else
		{
			AddRecipeItems(_categoryListWidget.SelectedCategory, _currentRecipes, flag);
		}
		_recipeListWidget.SetRecipes(_currentRecipes, flag2);
		if (RestoreRecipeSelection(flag2) && flag2)
		{
			RecipeListWidget.SelectInfo? selectedRecipe = _recipeListWidget.SelectedRecipe;
			if (selectedRecipe.HasValue)
			{
				_recipeListWidget.ScrollToRecipe(selectedRecipe.Value, instant: true);
			}
		}
	}

	private void RecipeListWidget_RecipeSelected()
	{
		RecipeListWidget.SelectInfo? selectedRecipe = _recipeListWidget.SelectedRecipe;
		if (selectedRecipe.HasValue)
		{
			RefreshDetailPanel(SelectedRecipeId != selectedRecipe.Value.Id);
			SelectedRecipeId = selectedRecipe.Value.Id;
		}
		else
		{
			HideDetailPanel();
			SelectedRecipeId = null;
		}
		_lastSelectedInfo.Set(_categoryListWidget.SelectedCategory, selectedRecipe);
	}

	private void RecipeInfo_BuildSizeChanged(int x, int y)
	{
		RefreshDetailPanel(reset: false);
	}

	private void RecipeInfo_Confirmed()
	{
		RecipeListWidget.SelectInfo? selectedRecipe = _recipeListWidget.SelectedRecipe;
		if (!selectedRecipe.HasValue)
		{
			return;
		}
		RecipeListWidget.SelectInfo value = selectedRecipe.Value;
		MenuListGroupBase menuListGroupBase = UIManager.FindScript<MenuListGroupBase>();
		switch (value.RecipeType)
		{
		case RecipeSystem.RecipeType.Crafting:
		{
			Crafting.Recipe recipe = OpenItemCraftingUI(value.Id);
			if (recipe != null)
			{
				menuListGroupBase.SetLastOpenCraft(recipe.Icon, value.RecipeType, value.Id);
			}
			break;
		}
		case RecipeSystem.RecipeType.Building:
		{
			Building.Blueprint blueprint = OpenBuildingUI(value.Id);
			if (blueprint != null)
			{
				menuListGroupBase.SetLastOpenCraft(blueprint.Icon, value.RecipeType, value.Id);
			}
			break;
		}
		}
	}

	private void RecipeInfo_LikeButtonClicked()
	{
		RecipeListWidget.SelectInfo? selectedRecipe = _recipeListWidget.SelectedRecipe;
		if (!selectedRecipe.HasValue)
		{
			return;
		}
		CategoryItem item = GameSystem<RecipeSystem>.Instance().GetCategoryItem(selectedRecipe.Value.RecipeType, selectedRecipe.Value.Id);
		if (item == null)
		{
			return;
		}
		bool like = !item.Like;
		switch (selectedRecipe.Value.RecipeType)
		{
		case RecipeSystem.RecipeType.Crafting:
			GameSystem<RecipeSystem>.Instance().LikeRecipe(item.Id, like, delegate
			{
				if (like)
				{
					UIManager.Alarm.ShowNotify(T._("<em>{0}</em>{0:-이} 즐겨찾기에 추가되었습니다.", item.Name), "act_Craft", major: false);
				}
			});
			break;
		case RecipeSystem.RecipeType.Building:
			GameSystem<RecipeSystem>.Instance().LikeBlueprint(item.Id, like, delegate
			{
				if (like)
				{
					UIManager.Alarm.ShowNotify(T._("<em>{0}</em>{0:-이} 즐겨찾기에 추가되었습니다.", item.Name), "act_Craft", major: false);
				}
			});
			break;
		}
	}

	private void InventorySystem_PlayerInventoryUpdated()
	{
		if (base.IsOpened)
		{
			Refresh();
		}
	}

	private void RecipeSystem_RecipeItemsUpdated(RecipeSystem.RecipeType type)
	{
		if (base.IsOpened)
		{
			ResetCategories(scrollToSelected: false);
		}
	}
}
