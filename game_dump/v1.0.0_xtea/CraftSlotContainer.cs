using System.Collections.Generic;
using Crafting;
using ItemSystem;
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

	private ItemSystem.Inventory _inventory;

	public Recipe Recipe { get; private set; }

	public Artifact Workbench { get; private set; }

	public CraftState State { get; private set; }

	public override IList<ItemData> Items => (_inventory != null) ? _inventory.Items : null;

	public void Set(Recipe recipe, Artifact workbench, ItemSystem.Inventory inventory)
	{
		Recipe = recipe;
		Workbench = workbench;
		_inventory = inventory;
		ClearSlots();
		int i = 0;
		for (int num = recipe.Slots.Length; i < num; i++)
		{
			CraftSlotInfo slot = new CraftSlotInfo(recipe.Slots[i], i);
			AddSlot(slot);
		}
		_tool.Refresh(recipe.Slots.Length, recipe.ToolTags);
		_expectedResultInfo.Clear();
		base.CurrentSlot = GetSlotInfo(0);
		SlotItemSelectionUpdated();
		OnInit();
	}

	protected override void OnDispose()
	{
		Recipe = null;
		Workbench = null;
		_inventory = null;
		base.OnDispose();
	}

	public void UpdateEstimateResult(CraftEstimation? estimation)
	{
		if (estimation.HasValue)
		{
			_expectedResultInfo.Refresh(estimation.Value);
		}
		else
		{
			_expectedResultInfo.Clear();
		}
		OnUpdateExpectedResult();
	}

	public ulong? GetWorkbenchEntityId()
	{
		ulong? result = null;
		if ((Object)(object)Workbench != (Object)null)
		{
			result = Workbench.EntityId;
		}
		return result;
	}

	public Point2? GetWorkbenchTile()
	{
		return (!((Object)(object)Workbench != (Object)null)) ? ((Point2)null) : Workbench.WorldTile;
	}

	private CraftState GetReadyState()
	{
		int num = 0;
		bool canQuickFill = false;
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
			if (_tool.ToolRequired && _tool.State != SlotInfo.SlotState.FullSelected)
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
}
