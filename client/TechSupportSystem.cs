using System;
using System.Collections.Generic;
using System.Linq;
using Crafting;
using Durango.Logic.Item;
using Durango.Network;
using Messages;

public class TechSupportSystem : GameSystem<TechSupportSystem>
{
	private readonly List<string> _itemIds = new List<string>();

	private Dictionary<string, Dictionary<int, TechSupportEstimateInfo>> _estimatesDict;

	public bool EstimatesLoaded { get; private set; }

	public event Action EstimatesLoadCompleted;

	public event Action<string, TechSupportEstimateResult?> EstimateUpdated;

	public event Action DecorationRemoved;

	private void Awake()
	{
		Connections.Frontend.On<TechSupportEstimates>(OnTechSupportEstimates);
	}

	public void ClearEstimates()
	{
		_estimatesDict = null;
		EstimatesLoaded = false;
	}

	public void RequestAllEstimates(PropKey propKey)
	{
		ClearEstimates();
		IEnumerable<ItemData> enumerable = GameSystem<InventorySystem>.Instance().PlayerItemList.Where(CanTechSupport);
		_itemIds.Clear();
		foreach (ItemData item in enumerable)
		{
			_itemIds.Add(item.Id);
		}
		if (_itemIds.Count > 0)
		{
			Connections.Frontend.Send(new GetTechSupportEstimates
			{
				EntityId = propKey.EntityId,
				Tile = propKey.Tile,
				ItemIds = _itemIds.ToArray()
			});
		}
		else
		{
			EstimatesLoaded = true;
		}
	}

	public TechSupportEstimateInfo? GetEstimateInfo(TechSupportTarget target)
	{
		if (target.Item == null)
		{
			return null;
		}
		if (_estimatesDict != null && _estimatesDict.TryGetValue(target.Item.Id, out var value) && value.TryGetValue(target.ReformSlotIndex, out var value2))
		{
			return value2;
		}
		return null;
	}

	public TechSupportEstimate? GetEstimate(TechSupportTarget target)
	{
		TechSupportEstimateInfo? estimateInfo = GetEstimateInfo(target);
		return (!estimateInfo.HasValue) ? null : estimateInfo.Value.Estimate;
	}

	public void RequestNewEstimate(PropKey propKey, TechSupportTarget target, string[] lockedTags)
	{
		if (target.Item == null)
		{
			return;
		}
		Connections.Frontend.Send(new RequestTechSupportEstimate
		{
			EntityId = propKey.EntityId,
			Tile = propKey.Tile,
			ItemId = target.Item.Id,
			ReformSlotIndex = target.ReformSlotIndex,
			TagsToLock = lockedTags
		}).On(delegate(TechSupportEstimateResult msg, PacketHeader header)
		{
			SetEstimate(target.Item.Id, msg);
			if (this.EstimateUpdated != null)
			{
				this.EstimateUpdated(target.Item.Id, msg);
			}
		}).Rest(delegate
		{
			if (this.EstimateUpdated != null)
			{
				this.EstimateUpdated(target.Item.Id, null);
			}
		});
	}

	public void RemoveDecoration(PropKey propKey, TechSupportTarget target)
	{
		if (target.Item == null)
		{
			return;
		}
		Connections.Frontend.Send(new RequestResetReformSlot
		{
			EntityId = propKey.EntityId,
			Tile = propKey.Tile,
			ItemId = target.Item.Id,
			ReformSlotIndex = target.ReformSlotIndex
		}).On<OK>(delegate
		{
			if (this.DecorationRemoved != null)
			{
				this.DecorationRemoved();
			}
		});
	}

	public static RecipeReform GetReformRecipe(ReformSlot? reformSlot)
	{
		if (reformSlot.HasValue)
		{
			return GameSystem<RecipeSystem>.Instance().GetRecipe(reformSlot.Value.RecipeId) as RecipeReform;
		}
		return null;
	}

	public static bool CanTechSupport(ItemData itemData)
	{
		if (itemData != null)
		{
			foreach (ReformSlot reformSlot in itemData.ReformSlots)
			{
				if (!string.IsNullOrEmpty(reformSlot.RecipeId))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SetEstimate(string itemId, TechSupportEstimateResult result)
	{
		if (_estimatesDict == null)
		{
			_estimatesDict = new Dictionary<string, Dictionary<int, TechSupportEstimateInfo>>();
		}
		if (!_estimatesDict.TryGetValue(itemId, out var value))
		{
			value = new Dictionary<int, TechSupportEstimateInfo>();
			_estimatesDict[itemId] = value;
		}
		value[result.Estimate.Index] = new TechSupportEstimateInfo
		{
			Estimate = result.Estimate,
			RequestCount = result.RequestCount
		};
	}

	private void OnTechSupportEstimates(TechSupportEstimates msg, PacketHeader header)
	{
		_estimatesDict = msg.Estimates;
		EstimatesLoaded = true;
		if (this.EstimatesLoadCompleted != null)
		{
			this.EstimatesLoadCompleted();
		}
	}
}
