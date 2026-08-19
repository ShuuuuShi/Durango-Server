using System;
using System.Collections.Generic;
using Crafting;
using Durango.Logic.Item;
using Messages;
using UnityEngine;

public class CraftSlotContainer : SlotContainer<CraftSlotInfo>
{
	public enum CraftState
	{
		CanQuickFill,
		ToolNotReady,
		MaterialsNotReady,
		ReadyToCraft
	}

	private Durango.Logic.Item.Inventory _inventory;

	private int _quantity;

	public Recipe Recipe { get; private set; }

	public Artifact Workbench { get; private set; }

	public CraftState State { get; private set; }

	public override int Quantity => _quantity;

	public override IList<ItemData> Items
	{
		get
		{
			if (_inventory == null)
			{
				return null;
			}
			return _inventory.Items;
		}
	}

	public TechSupportBaseSlotInfo TechSupportBaseSlotInfo { get; private set; }

	public void Set(Recipe recipe, Artifact workbench, Durango.Logic.Item.Inventory inventory, TechSupportTarget? techSupportTarget)
	{
		Recipe = recipe;
		Workbench = workbench;
		_inventory = inventory;
		_quantity = 1;
		ClearSlots();
		TechSupportBaseSlotInfo = null;
		int i = 0;
		for (int num = recipe.Slots.Length; i < num; i++)
		{
			RecipeSlot recipeSlot = recipe.Slots[i];
			SlotInfo slot;
			if (recipeSlot.IsBase && techSupportTarget.HasValue)
			{
				TechSupportBaseSlotInfo = new TechSupportBaseSlotInfo(this, recipeSlot, i, techSupportTarget.Value);
				slot = TechSupportBaseSlotInfo;
			}
			else
			{
				slot = new CraftSlotInfo(this, recipeSlot, i);
			}
			AddSlot(slot);
		}
		base.Tool.Refresh(recipe.Slots.Length, (!techSupportTarget.HasValue) ? recipe.AllowedTool : null);
		base.CurrentSlot = GetSlotInfo(0);
		SlotItemSelectionUpdated();
		OnInit();
	}

	public bool ReadyToRequestEstimation()
	{
		if (Recipe == null)
		{
			return false;
		}
		if (State == CraftState.ReadyToCraft)
		{
			return true;
		}
		return false;
	}

	public void GetEstimation(Action<CraftEstimationInfo?> onResult)
	{
		if (onResult != null)
		{
			EstimateCraft info = default(EstimateCraft);
			info.RecipeId = Recipe.Id;
			info.Materials = CreateFirstMaterialsDictionary(canFinished: false);
			info.ToolItemId = GetToolItemId();
			info.Workbench = GetWorkbench();
			CraftSystem.CraftEstimation(info, onResult);
		}
	}

	public PropKey? GetWorkbench()
	{
		if (Workbench != null)
		{
			PropKey value = default(PropKey);
			value.EntityId = Workbench.EntityId;
			value.Tile = Workbench.WorldTile;
			return value;
		}
		return null;
	}

	private CraftState GetReadyState()
	{
		int num = 0;
		bool canQuickFill = false;
		GatherOtherSlotsSelectedItemIds();
		for (int i = 0; i < SlotCount; i++)
		{
			SlotInfo slotInfo = GetSlotInfo(i);
			if (slotInfo != null)
			{
				if (slotInfo.State == SlotInfo.SlotState.FullSelected)
				{
					num++;
				}
				GetSlotCanQuickFillFlag(slotInfo, ref canQuickFill);
			}
		}
		if (num < SlotCount)
		{
			if (canQuickFill)
			{
				return CraftState.CanQuickFill;
			}
			if (base.Tool.ToolRequired && base.Tool.State != SlotInfo.SlotState.FullSelected)
			{
				return CraftState.ToolNotReady;
			}
			return CraftState.MaterialsNotReady;
		}
		return CraftState.ReadyToCraft;
	}

	protected override void SlotItemSelectionUpdated()
	{
		State = GetReadyState();
	}

	public override int ItemPriorityComparison(ItemData i1, ItemData i2)
	{
		if (i1.SafeLevel != i2.SafeLevel)
		{
			if (i1.SafeLevel <= i2.SafeLevel)
			{
				return -1;
			}
			return 1;
		}
		if (Recipe is RecipeCraft recipeCraft)
		{
			int num = ((i1.PrototypeId == recipeCraft.PrototypeId) ? 1 : 0);
			int num2 = ((i2.PrototypeId == recipeCraft.PrototypeId) ? 1 : 0);
			if (num != num2)
			{
				return num - num2;
			}
		}
		int num3 = i2.ModifiableCount - i1.ModifiableCount;
		if (num3 == 0)
		{
			num3 = string.CompareOrdinal(i1.Id, i2.Id);
		}
		return num3;
	}

	public override void SetQuantity(int value)
	{
		int quantity = _quantity;
		_quantity = Mathf.Max(1, value);
		if (quantity != _quantity)
		{
			OnQuantityChanged();
		}
	}

	public override int CalcMaxQuantity()
	{
		int num = 1;
		int num2 = -1;
		while (num2 - num != 1)
		{
			int num3 = ((num2 != -1) ? ((num + num2) / 2) : num);
			if (RecipeSystem.HasMaterials(Recipe, num3))
			{
				if (num3 > 15)
				{
					return 15;
				}
				num = ((num2 != -1) ? num3 : (num3 * 2));
			}
			else if (num2 == -1)
			{
				num2 = num3;
				num = num3 / 2;
			}
			else
			{
				num2 = num3;
			}
		}
		return num;
	}
}
