using System;
using System.Collections.Generic;
using Building_;
using ItemSystem;
using Messages;
using UnityEngine;

public class BuildSlotContainer : SlotContainer<BuildSlotInfo>
{
	public enum BuildState
	{
		CanQuickFill,
		MaterialsNotReady,
		ReadyToPutMaterials,
		ReadyToPutMaterialsAndBuild,
		ReadyToBuild
	}

	private ItemSystem.Inventory _inventory;

	public BuildState State { get; private set; }

	public Artifact Artifact { get; set; }

	public Blueprint Blueprint => (!((Object)(object)Artifact == (Object)null)) ? Artifact.Blueprint : null;

	public override IList<ItemData> Items => (_inventory != null) ? _inventory.Items : null;

	public event Action SlotMaterialUpdated;

	public void Set(Artifact artifact, ItemSystem.Inventory inventory)
	{
		Artifact = artifact;
		_inventory = inventory;
		Blueprint blueprint = Blueprint;
		ClearSlots();
		int i = 0;
		for (int num = blueprint.Slots.Length; i < num; i++)
		{
			int maxCountModifier = 1;
			if (blueprint.IsSizeVariable)
			{
				maxCountModifier = BuildManager.GetBlueprintSlotCountModifier(blueprint.Slots[i], artifact.Size);
			}
			BuildSlotInfo slot = new BuildSlotInfo(blueprint.Slots[i], i, maxCountModifier);
			AddSlot(slot);
		}
		_tool.Refresh(blueprint.Slots.Length, blueprint.ToolTags);
		_expectedResultInfo.Clear();
		SelectFirstIncompletedSlot();
		SlotItemSelectionUpdated();
		OnInit();
	}

	public void SetPrevMaterial(Dictionary<string, Item[]> prevMaterials)
	{
		int i = 0;
		for (int count = _slots.Count; i < count; i++)
		{
			BuildSlotInfo buildSlotInfo = _slots[i];
			Item[] prevMaterials2 = prevMaterials.Get(buildSlotInfo.Id);
			buildSlotInfo.SetPrevMaterials(prevMaterials2);
		}
	}

	public void SetPrevAssignedItemsDummyCount(Dictionary<string, int> prevMaterialsDummyCounts)
	{
		int i = 0;
		for (int count = _slots.Count; i < count; i++)
		{
			BuildSlotInfo buildSlotInfo = _slots[i];
			int prevAssignedItemsDummyCount = prevMaterialsDummyCounts.Get(buildSlotInfo.Id, 0);
			buildSlotInfo.SetPrevAssignedItemsDummyCount(prevAssignedItemsDummyCount);
		}
	}

	protected override void OnDispose()
	{
		Artifact = null;
		_inventory = null;
		base.OnDispose();
	}

	public void UpdateEstimateResult(BuildEstimation? estimation)
	{
		if (!((Object)(object)Artifact == (Object)null))
		{
			if (estimation.HasValue)
			{
				_expectedResultInfo.Refresh(Artifact.ArtifactId, estimation.Value);
			}
			else
			{
				_expectedResultInfo.Clear();
			}
			OnUpdateExpectedResult();
		}
	}

	public void OnSlotMaterialUpdate()
	{
		if (this.SlotMaterialUpdated != null)
		{
			this.SlotMaterialUpdated();
		}
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
		for (int i = 0; i < _slots.Count; i++)
		{
			BuildSlotInfo buildSlotInfo = _slots[i];
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
		if (_tool.ToolRequired)
		{
			flag = _tool.State == SlotInfo.SlotState.FullSelected;
			GetSlotCanQuickFillFlag(_tool, ref canQuickFill);
		}
		if (num < _slots.Count || !flag)
		{
			if (canQuickFill && !flag2)
			{
				return BuildState.CanQuickFill;
			}
			if (flag2)
			{
				return BuildState.ReadyToPutMaterials;
			}
			return BuildState.MaterialsNotReady;
		}
		if (flag2)
		{
			return (!Blueprint.Available) ? BuildState.ReadyToPutMaterials : BuildState.ReadyToPutMaterialsAndBuild;
		}
		return BuildState.ReadyToBuild;
	}

	private SlotInfo GetFirstIncompletedSlot()
	{
		for (int i = 0; i < SlotCount; i++)
		{
			SlotInfo slotInfo = GetSlotInfo(i);
			if (slotInfo != null && slotInfo.CurrentCount < slotInfo.MaxCount)
			{
				return slotInfo;
			}
		}
		return GetSlotInfo(0);
	}
}
