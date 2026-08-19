using System;
using System.Collections.Generic;
using Crafting;
using Durango.Logic.Item;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using NestedPrefab;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class RepairKitsWidget : MonoBehaviour
{
	public const string TagIdForArtifactRepairKit = "artifact_repair_kit";

	public const string TagIdForToolRepairKit = "tool_repair_kit";

	public const string TagIdForClothesRepairKit = "clothes_repair_kit";

	[SerializeField]
	private UILabel _textRepairValues;

	[SerializeField]
	private UISprite _repairGaugeBar;

	[SerializeField]
	private NestedPrefabLinker _repairKitItemListLinker;

	[SerializeField]
	private GameObject _kitsNotFound;

	[SerializeField]
	private UILabel _labelKitsNotFoundInfo;

	[SerializeField]
	private SelectableButton _buttonJumpToRecipeUI;

	[SerializeField]
	private SelectableButton _buttonJumpToMarketUI;

	private bool _initialized;

	private string _formatRepairValues;

	private string _tagIdForRepairKit;

	private int _currentRepairValue;

	private int _requiredRepairValue;

	private ItemList _repairKitItemList;

	private string _recipeId;

	public bool IsInsufficient => _currentRepairValue < _requiredRepairValue;

	public IList<ItemData> SelectedItems
	{
		get
		{
			Init();
			return _repairKitItemList.SelectedList;
		}
	}

	public event Action RepairValueChanged;

	public event Action<string> JumpToRecipeUIButtonClicked;

	public event Action<string> JumpToMarketUIButtonClicked;

	public void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			_formatRepairValues = _textRepairValues.text;
			_repairKitItemList = _repairKitItemListLinker.Object.GetComponent<ItemList>();
			ItemList repairKitItemList = _repairKitItemList;
			repairKitItemList.OnUpdateSelectItem = (Action)Delegate.Combine(repairKitItemList.OnUpdateSelectItem, new Action(repairKitItemList_OnUpdateSelectItem));
			_repairKitItemList.SelectableCount = -1;
			SelectableButton buttonJumpToRecipeUI = _buttonJumpToRecipeUI;
			buttonJumpToRecipeUI.Clicked = (Action)Delegate.Combine(buttonJumpToRecipeUI.Clicked, new Action(ButtonJumpToRecipeUI_Clicked));
			SelectableButton buttonJumpToMarketUI = _buttonJumpToMarketUI;
			buttonJumpToMarketUI.Clicked = (Action)Delegate.Combine(buttonJumpToMarketUI.Clicked, new Action(ButtonJumpToMarketUI_Clicked));
		}
	}

	public void Refresh(RepairRequirement repairRequirement)
	{
		_tagIdForRepairKit = repairRequirement.TagId;
		_currentRepairValue = 0;
		_requiredRepairValue = repairRequirement.RepairPerformance;
		RefreshRepairValueText();
		_repairKitItemList.DeselectAllItems(sendEvent: false);
	}

	public void Refresh([NotNull] Artifact artifact)
	{
		_tagIdForRepairKit = "artifact_repair_kit";
		_currentRepairValue = 0;
		_requiredRepairValue = Singleton<Constants>.Instance.Repair.GetRepairRequirementPerformance(artifact.Blueprint.RepairRequirement, artifact.ArtifactState.Level);
		RefreshRepairValueText();
		_repairKitItemList.DeselectAllItems(sendEvent: false);
	}

	public void RefreshRepairKitItemList()
	{
		_repairKitItemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, (ItemData data) => data.HasTag(_tagIdForRepairKit));
		RefreshRepairVaule();
		if (_repairKitItemList.Count > 0)
		{
			_repairKitItemListLinker.gameObject.SetActive(value: true);
			_kitsNotFound.SetActive(value: false);
			_recipeId = null;
			return;
		}
		_repairKitItemListLinker.gameObject.SetActive(value: false);
		_kitsNotFound.SetActive(value: true);
		if (___TempHardCoded___TryGetAvailableRepairKitRecipeId(out var id))
		{
			_recipeId = id;
			_labelKitsNotFoundInfo.text = T._("소지한 수리키트가 없습니다.\n제작하거나 장터에서 구입할 수 있습니다.");
			_buttonJumpToRecipeUI.Text = T._("수리키트 제작");
		}
		else
		{
			_recipeId = ___TempHardCoded___GetBasicRepairKitRecipeId();
			_labelKitsNotFoundInfo.text = T._("소지한 수리키트가 없습니다.\n제작하거나 장터에서 구입할 수 있습니다.");
			_buttonJumpToRecipeUI.Text = T._("스킬로 이동");
		}
	}

	public void ClearSelectedItems()
	{
		_repairKitItemList.DeselectAllItems(sendEvent: false);
		RefreshRepairVaule();
	}

	public static int GetRepairPerformance(ItemData item)
	{
		int num = 0;
		foreach (Performance performance in item.Performances)
		{
			if (performance.Nums != null && performance.Nums.TryGetValue("ability", out var value))
			{
				num += (int)value;
			}
		}
		return num;
	}

	private void RefreshRepairValueText()
	{
		_textRepairValues.text = T._(_formatRepairValues, _currentRepairValue, _requiredRepairValue);
	}

	private void RefreshRepairVaule()
	{
		_currentRepairValue = 0;
		foreach (ItemData selected in _repairKitItemList.SelectedList)
		{
			_currentRepairValue += GetRepairPerformance(selected);
		}
		if (_repairKitItemList.SelectedList.Count > 1 && _currentRepairValue > _requiredRepairValue)
		{
			AdjustSelectedItems();
		}
		RefreshRepairValueText();
		_repairGaugeBar.fillAmount = Mathf.Clamp01((float)_currentRepairValue / (float)_requiredRepairValue);
		if (this.RepairValueChanged != null)
		{
			this.RepairValueChanged();
		}
	}

	private void AdjustSelectedItems()
	{
		List<ItemData> list = new List<ItemData>(_repairKitItemList.SelectedList);
		list.Sort((ItemData x, ItemData y) => GetRepairPerformance(x) - GetRepairPerformance(y));
		foreach (ItemData item in list)
		{
			int repairPerformance = GetRepairPerformance(item);
			if (_currentRepairValue - repairPerformance < _requiredRepairValue)
			{
				break;
			}
			_repairKitItemList.SelectItem(item, sendEvent: false, scrollTo: false);
			_currentRepairValue -= repairPerformance;
		}
	}

	private bool ___TempHardCoded___TryGetAvailableRepairKitRecipeId(out string id)
	{
		for (int num = 3; num >= 1; num--)
		{
			id = $"{_tagIdForRepairKit}_{num:00}";
			Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(id);
			if (recipe != null && recipe.Available)
			{
				return true;
			}
		}
		id = string.Empty;
		return false;
	}

	private string ___TempHardCoded___GetBasicRepairKitRecipeId()
	{
		return _tagIdForRepairKit + "_01";
	}

	private void repairKitItemList_OnUpdateSelectItem()
	{
		RefreshRepairVaule();
	}

	private void ButtonJumpToRecipeUI_Clicked()
	{
		if (_recipeId != null && this.JumpToRecipeUIButtonClicked != null)
		{
			this.JumpToRecipeUIButtonClicked(_recipeId);
		}
	}

	private void ButtonJumpToMarketUI_Clicked()
	{
		if (this.JumpToMarketUIButtonClicked != null)
		{
			this.JumpToMarketUIButtonClicked(_tagIdForRepairKit);
		}
	}
}
