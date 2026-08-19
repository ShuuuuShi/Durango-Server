using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using JetBrains.Annotations;
using Messages;

namespace InteractionData;

public class GatheringData
{
	public const string BarehandKey = "bare_hands";

	public Action<float> DurationChanged;

	public string GeneratorId;

	public string Name;

	public string Icon;

	public int Level;

	public int Amount;

	public float Effort;

	public float Duration;

	public Dictionary<string, int> RequiredTools;

	public bool Enabled;

	public int BestPerformance;

	[CanBeNull]
	public ItemData BestTool;

	public bool IsCritical;

	public bool IsValid;

	public GatheringData(Generator gen, bool isCritical)
	{
		Set(gen, isCritical);
	}

	public bool IsAvailableForGathering()
	{
		return Enabled && BestPerformance > 0;
	}

	public void Set(Generator gen, bool isCritical)
	{
		GeneratorId = gen.Id;
		Name = gen.Name;
		Icon = gen.Icon;
		Level = gen.Level;
		Amount = gen.Amount;
		Effort = gen.Effort;
		Duration = gen.Duration;
		RequiredTools = gen.ToolRequirements;
		Enabled = gen.Enabled;
		BestPerformance = 0;
		BestTool = null;
		IsCritical = isCritical;
		IsValid = true;
		DurationChanged = null;
	}

	public void FindBestTool(IList<ItemData> tools)
	{
		BestTool = null;
		BestPerformance = 0;
		int count = tools.Count;
		ItemData itemData = null;
		int bestPerformance = 0;
		ItemData itemData2 = null;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			ItemData itemData3 = tools[i];
			if (itemData3 == null || itemData3.IsDestroyed())
			{
				continue;
			}
			foreach (KeyValuePair<string, int> requiredTool in RequiredTools)
			{
				TagData tagData = itemData3.GetTagData(requiredTool.Key);
				if (tagData == null)
				{
					continue;
				}
				int level = tagData.Level;
				if (level < requiredTool.Value)
				{
					if (itemData2 == null || num < level)
					{
						itemData2 = itemData3;
						num = level;
					}
					continue;
				}
				if (itemData3.IsEquipments)
				{
					itemData = itemData3;
					bestPerformance = level;
				}
				if (BestTool != null)
				{
					if (BestTool.Locked)
					{
						if (itemData3.Locked && level < BestPerformance)
						{
							continue;
						}
					}
					else if (itemData3.Locked || level < BestPerformance)
					{
						continue;
					}
				}
				BestPerformance = level;
				BestTool = itemData3;
			}
		}
		if (itemData != null && (!itemData.Locked || BestTool == null || BestTool.Locked))
		{
			BestTool = itemData;
			BestPerformance = bestPerformance;
		}
		else if (BestTool == null)
		{
			BestTool = itemData2;
		}
		if (BestPerformance == 0 && RequiredTools.TryGetValue("bare_hands", out var value))
		{
			BestPerformance = value;
			BestTool = null;
		}
	}

	public string CanGateringWithThisTool(ItemData item)
	{
		if (item == null)
		{
			return null;
		}
		foreach (KeyValuePair<string, int> requiredTool in RequiredTools)
		{
			TagData tagData = item.GetTagData(requiredTool.Key);
			if (tagData != null && tagData.Level >= requiredTool.Value)
			{
				return tagData.Id;
			}
		}
		return null;
	}
}
