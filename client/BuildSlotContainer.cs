using System;
using System.Collections.Generic;
using Building;
using Durango.Logic.Item;
using JetBrains.Annotations;
using Messages;

public class BuildSlotContainer : SlotContainer<BuildSlotInfo>
{
	public enum BuildState
	{
		CanQuickFill,
		ToolNotReady,
		MaterialsNotReady,
		ReadyToPutMaterials,
		ReadyToPutMaterialsAndBuild,
		ReadyToBuild
	}

	private Durango.Logic.Item.Inventory _inventory;

	public BuildState State { get; private set; }

	public Artifact Artifact { get; private set; }

	public Blueprint Blueprint { get; private set; }

	public override IList<ItemData> Items => (_inventory != null) ? _inventory.Items : null;

	public void Set(Artifact artifact, Blueprint blueprint, Durango.Logic.Item.Inventory inventory)
	{
		Artifact = artifact;
		Blueprint = blueprint;
		_inventory = inventory;
		ClearSlots();
		int i = 0;
		for (int num = Blueprint.Slots.Length; i < num; i++)
		{
			int maxCountModifier = 1;
			if (Blueprint.IsSizeVariable)
			{
				maxCountModifier = Blueprint.Slots[i].GetSlotCountModifier(artifact.Size);
			}
			BuildSlotInfo slot = new BuildSlotInfo(this, Blueprint.Slots[i], i, maxCountModifier);
			AddSlot(slot);
		}
		base.Tool.Refresh(Blueprint.Slots.Length, Blueprint.AllowedToolTag);
		SelectFirstIncompletedSlot();
		SlotItemSelectionUpdated();
		OnInit();
	}

	public void SetPrevMaterial(Dictionary<string, Item[]> prevMaterials)
	{
		int i = 0;
		for (int count = Slots.Count; i < count; i++)
		{
			BuildSlotInfo buildSlotInfo = Slots[i];
			Item[] prevMaterials2 = prevMaterials.Get(buildSlotInfo.Id);
			buildSlotInfo.SetPrevMaterials(prevMaterials2);
		}
	}

	public void SetPrevAssignedItemsDummyCount(Dictionary<string, int> prevMaterialsDummyCounts)
	{
		int i = 0;
		for (int count = Slots.Count; i < count; i++)
		{
			BuildSlotInfo buildSlotInfo = Slots[i];
			int prevAssignedItemsDummyCount = prevMaterialsDummyCounts.Get(buildSlotInfo.Id, 0);
			buildSlotInfo.SetPrevAssignedItemsDummyCount(prevAssignedItemsDummyCount);
		}
	}

	public bool ReadyToRequestEstimation()
	{
		if (Artifact == null)
		{
			return false;
		}
		BuildState state = State;
		if (state == BuildState.ReadyToPutMaterialsAndBuild || state == BuildState.ReadyToBuild)
		{
			return true;
		}
		return false;
	}

	public void GetEstimation(Action<BuildEstimation?> onResult)
	{
		if (onResult != null)
		{
			EstimateBuild estimateBuild = default(EstimateBuild);
			estimateBuild.EntityId = Artifact.EntityId;
			estimateBuild.Tile = Artifact.WorldTile;
			estimateBuild.ToolId = GetToolItemId();
			estimateBuild.Materials = ((!Blueprint.IsRemodeling) ? CreateFirstMaterialsDictionary(canFinished: false) : null);
			EstimateBuild info = estimateBuild;
			BuildSystem.EstimateBuild(info, onResult);
		}
	}

	public void GetRemodelingEstimation([NotNull] Action<BuildEstimation?> onResult)
	{
		EstimateRemodeling estimateRemodeling = default(EstimateRemodeling);
		estimateRemodeling.EntityId = Artifact.EntityId;
		estimateRemodeling.Tile = Artifact.WorldTile;
		estimateRemodeling.ToolId = GetToolItemId();
		estimateRemodeling.Materials = CreateFirstMaterialsDictionary(canFinished: false);
		EstimateRemodeling info = estimateRemodeling;
		BuildSystem.EstimateRemodeling(info, onResult);
	}

	public void SelectFirstIncompletedSlot()
	{
		base.CurrentSlot = GetFirstIncompletedSlot();
	}

	protected override void SlotItemSelectionUpdated()
	{
		State = GetReadyState();
	}

	private BuildState GetReadyState()
	{
		int num = 0;
		bool canQuickFill = false;
		bool flag = true;
		bool flag2 = false;
		GatherOtherSlotsSelectedItemIds();
		for (int i = 0; i < Slots.Count; i++)
		{
			BuildSlotInfo buildSlotInfo = Slots[i];
			if (buildSlotInfo == null)
			{
				continue;
			}
			switch (buildSlotInfo.State)
			{
			case SlotInfo.SlotState.SomeSelected:
				flag2 = true;
				break;
			case SlotInfo.SlotState.FullSelected:
				if (buildSlotInfo.SelectedItems.Count > 0)
				{
					flag2 = true;
				}
				num++;
				break;
			}
			GetSlotCanQuickFillFlag(buildSlotInfo, ref canQuickFill);
		}
		if (base.Tool.ToolRequired)
		{
			flag = base.Tool.State == SlotInfo.SlotState.FullSelected;
			GetSlotCanQuickFillFlag(base.Tool, ref canQuickFill);
		}
		bool flag3 = num < Slots.Count;
		if (flag3 || !flag)
		{
			if (canQuickFill && !flag2)
			{
				return BuildState.CanQuickFill;
			}
			if (flag2)
			{
				return BuildState.ReadyToPutMaterials;
			}
			if (!flag3)
			{
				return BuildState.ToolNotReady;
			}
			return BuildState.MaterialsNotReady;
		}
		if (flag2)
		{
			return (!CanBuild()) ? BuildState.ReadyToPutMaterials : BuildState.ReadyToPutMaterialsAndBuild;
		}
		return BuildState.ReadyToBuild;
	}

	public bool CanBuild()
	{
		return (Blueprint != null && Blueprint.Available) || (Artifact != null && Artifact.FounderId == PlayerBehavior.LocalPlayer.EntityId);
	}

	private SlotInfo GetFirstIncompletedSlot()
	{
		for (int i = 0; i < SlotCount; i++)
		{
			SlotInfo slotInfo = GetSlotInfo(i);
			if (slotInfo != null && slotInfo.CurrentCount < slotInfo.TotalCount)
			{
				return slotInfo;
			}
		}
		return GetSlotInfo(0);
	}

	public override int ItemPriorityComparison(ItemData i1, ItemData i2)
	{
		if (i1.SafeLevel != i2.SafeLevel)
		{
			return (i1.SafeLevel > i2.SafeLevel) ? 1 : (-1);
		}
		int num = i2.ModifiableCount - i1.ModifiableCount;
		if (num == 0)
		{
			num = string.CompareOrdinal(i1.Id, i2.Id);
		}
		return num;
	}

	public override int CalcMaxQuantity()
	{
		return 0;
	}
}
