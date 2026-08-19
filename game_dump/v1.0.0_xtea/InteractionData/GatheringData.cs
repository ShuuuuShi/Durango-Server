using System.Collections.Generic;
using ItemSystem;
using Messages;

namespace InteractionData;

public class GatheringData
{
	public string Id;

	public string Name;

	public string Icon;

	public int Level;

	public int Amount;

	public float Effort;

	public float Duration;

	public Dictionary<string, int> RequiredTools;

	public bool Enabled;

	public int BestPerformance;

	public ItemData BestTool;

	public bool IsValid { get; set; }

	public GatheringData(Generator gen)
	{
		Set(gen);
	}

	public bool IsAvailableForGathering()
	{
		return Enabled && BestPerformance > 0;
	}

	public void Set(Generator gen)
	{
		Id = gen.Id;
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
		IsValid = true;
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
		for (int i = 0; i < count + 1; i++)
		{
			ItemData itemData3 = ((i >= count) ? GameSystem<EquipSystem>.Instance().Barehands : tools[i]);
			if (itemData3 == null)
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
					if (BestTool.Like)
					{
						if (itemData3.Like && level < BestPerformance)
						{
							continue;
						}
					}
					else if (itemData3.Like || level < BestPerformance)
					{
						continue;
					}
				}
				BestPerformance = level;
				BestTool = itemData3;
			}
		}
		if (itemData != null && (!itemData.Like || BestTool == null || BestTool.Like))
		{
			BestTool = itemData;
			BestPerformance = bestPerformance;
		}
		else if (BestTool == null)
		{
			BestTool = itemData2;
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
