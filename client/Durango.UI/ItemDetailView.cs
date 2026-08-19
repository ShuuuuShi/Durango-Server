using System.Collections.Generic;
using Building;
using Crafting;
using Durango.Logic.Item;
using Durango.UI.Control;
using JetBrains.Annotations;
using Messages;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class ItemDetailView : KWidgetScrollView
{
	private static bool _reformInfoOpend = true;

	private static bool _performanceInfoOpend = true;

	private static bool _repairInfoOpend = true;

	private static bool _recipeInfoOpend = true;

	private static bool _blueprintInfoOpend = true;

	private static readonly Dictionary<string, int> _reformSlotTagLevels = new Dictionary<string, int>();

	[SerializeField]
	private UIWidget _itemHelpWidget;

	[SerializeField]
	private UILabel _itemHelpLabel;

	[SerializeField]
	private NestedPrefabLinker _tagsViewerLinker;

	[SerializeField]
	private ItemContextReform _reformInfo;

	[SerializeField]
	private ItemContextPerformance _performanceInfo;

	[SerializeField]
	private ItemContextRepair _repairInfo;

	[SerializeField]
	private ItemContextCraftInfo _recipeInfo;

	private ItemContextCraftInfo _blueprintInfo;

	private readonly HashSet<Recipe> _availableRecipes = new HashSet<Recipe>();

	private readonly HashSet<Blueprint> _availableBlueprints = new HashSet<Blueprint>();

	private TagsViewerWidget _tagsViewer;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_blueprintInfo = _recipeInfo.transform.parent.gameObject.AddChild(_recipeInfo.gameObject).GetComponent<ItemContextCraftInfo>();
			ItemContextControlInitializer(_reformInfo);
			ItemContextControlInitializer(_performanceInfo);
			ItemContextControlInitializer(_repairInfo);
			ItemContextControlInitializer(_recipeInfo);
			ItemContextControlInitializer(_blueprintInfo);
			_tagsViewer = _tagsViewerLinker.Object.GetComponent<TagsViewerWidget>();
			List<UIWidget> widgets = base.Widgets;
			widgets.Add(_itemHelpWidget);
			widgets.Add(_tagsViewerLinker.GetComponent<UIWidget>());
			widgets.Add(_reformInfo);
			widgets.Add(_performanceInfo);
			widgets.Add(_repairInfo);
			widgets.Add(_recipeInfo);
			widgets.Add(_blueprintInfo);
		}
	}

	private void ItemContextControlInitializer(ItemContextBase control)
	{
		control.Init();
		control.OnExpandChanged += OnControlExpandChanged;
	}

	public void Set([NotNull] ItemData itemData, bool enableRecipeLink)
	{
		Init();
		bool active = _tagsViewer.Set((itemData.ReformSlots.Count <= 0) ? itemData.Tags : EnumerateAdjustedItemTags(itemData.Tags, itemData.ReformSlots));
		_tagsViewerLinker.gameObject.SetActive(active);
		if (itemData.IsEquipments)
		{
			_availableRecipes.Clear();
			_availableBlueprints.Clear();
		}
		else
		{
			GameSystem<RecipeSystem>.Instance().FillAvailableRecipesByItemData(_availableRecipes, itemData);
			GameSystem<RecipeSystem>.Instance().FillAvailableBlueprintsByItemData(_availableBlueprints, itemData);
		}
		if (itemData.Prototype == null || string.IsNullOrEmpty(itemData.Prototype.Help))
		{
			_itemHelpWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_itemHelpWidget.gameObject.SetActive(value: true);
			_itemHelpLabel.text = itemData.Prototype.Help;
			_itemHelpWidget.height = _itemHelpLabel.height + 22;
		}
		_reformInfo.Set(itemData);
		_performanceInfo.Set(itemData);
		_repairInfo.Set(itemData);
		_recipeInfo.Set(_availableRecipes, enableRecipeLink);
		_blueprintInfo.Set(_availableBlueprints, enableRecipeLink);
		OnDataFillFinished();
	}

	public void Set(Pet pet)
	{
		Init();
		PetStats stat = pet.Stat;
		_tagsViewer.SettingBegin();
		if (stat.Tags != null)
		{
			foreach (KeyValuePair<string, int> tag in stat.Tags)
			{
				_tagsViewer.AddTagData(tag.Key, tag.Value);
			}
		}
		_tagsViewerLinker.gameObject.SetActive(_tagsViewer.SettingEnd());
		_itemHelpWidget.gameObject.SetActive(value: false);
		_reformInfo.Clear();
		_performanceInfo.Set(pet);
		_repairInfo.Set(null);
		_recipeInfo.Clear();
		_blueprintInfo.Clear();
		OnDataFillFinished();
	}

	private static IEnumerable<TagData> EnumerateAdjustedItemTags(IEnumerable<TagData> itemTags, IEnumerable<ReformSlot> reformSlots)
	{
		_reformSlotTagLevels.Clear();
		foreach (ReformSlot reformSlot in reformSlots)
		{
			Tag[] tags = reformSlot.Tags;
			for (int i = 0; i < tags.Length; i++)
			{
				Tag tag2 = tags[i];
				int num = _reformSlotTagLevels.Get(tag2.Id, 0);
				_reformSlotTagLevels[tag2.Id] = num + tag2.Level;
			}
		}
		foreach (TagData tag in itemTags)
		{
			if (_reformSlotTagLevels.TryGetValue(tag.Id, out var reformTagLevel))
			{
				if (tag.Level - reformTagLevel > 0)
				{
					yield return TagData.Create(tag.Id, tag.Level - reformTagLevel);
				}
			}
			else
			{
				yield return tag;
			}
		}
	}

	private void OnDataFillFinished()
	{
		if (_reformInfo.gameObject.activeSelf)
		{
			_reformInfo.SetExpand(_reformInfoOpend, instant: true);
		}
		if (_performanceInfo.gameObject.activeSelf)
		{
			_performanceInfo.SetExpand(_performanceInfoOpend, instant: true);
		}
		if (_repairInfo.gameObject.activeSelf)
		{
			_repairInfo.SetExpand(_repairInfoOpend, instant: true);
		}
		if (_recipeInfo.gameObject.activeSelf)
		{
			_recipeInfo.SetExpand(_recipeInfoOpend, instant: true);
		}
		if (_blueprintInfo.gameObject.activeSelf)
		{
			_blueprintInfo.SetExpand(_blueprintInfoOpend, instant: true);
		}
		ResetPosition();
	}

	private void OnControlExpandChanged(ItemContextBase comp)
	{
		bool flag = !comp.IsExpanded;
		comp.SetExpand(flag, instant: false);
		if (comp == _reformInfo)
		{
			_reformInfoOpend = flag;
		}
		else if (comp == _performanceInfo)
		{
			_performanceInfoOpend = flag;
		}
		else if (comp == _repairInfo)
		{
			_repairInfoOpend = flag;
		}
		else if (comp == _recipeInfo)
		{
			_recipeInfoOpend = flag;
		}
		else if (comp == _blueprintInfo)
		{
			_blueprintInfoOpend = flag;
		}
		UpdateLayout(instant: false);
	}
}
