using System.Collections.Generic;
using ItemSystem;

namespace PlayGuide;

public class GetItemToDo : ToDoBase
{
	public readonly TagFilter[] RequiredTags;

	private readonly TagEvaluator _tag;

	private readonly bool _ignoreAlreadyOwned;

	private int _alreadyOwnedItemCount;

	public GetItemToDo(string tag, Dictionary<string, Dictionary<string, string>> requiredTags, bool ignoreAlreadyOwned)
	{
		if (!string.IsNullOrEmpty(tag))
		{
			_tag = new TagEvaluator(tag);
		}
		else if (requiredTags != null && requiredTags.Count > 0)
		{
			RequiredTags = new TagFilter[requiredTags.Count];
			int num = 0;
			foreach (KeyValuePair<string, Dictionary<string, string>> requiredTag in requiredTags)
			{
				RequiredTags[num] = new TagFilter(requiredTag.Key, 0);
				num++;
			}
		}
		_ignoreAlreadyOwned = ignoreAlreadyOwned;
	}

	public GetItemToDo(TagFilter[] filters)
	{
		RequiredTags = filters;
	}

	private int CalcItemCount()
	{
		int result = 0;
		if (_tag != null)
		{
			result = GameSystem<InventorySystem>.Instance().GetTaggedItemCount(_tag);
		}
		else if (RequiredTags != null)
		{
			result = GameSystem<InventorySystem>.Instance().GetFilteredItemCount(RequiredTags);
		}
		return result;
	}

	protected void OnUpdateInventory()
	{
		int num = CalcItemCount() - _alreadyOwnedItemCount;
		if (num < 0)
		{
			_alreadyOwnedItemCount += num;
			num = 0;
		}
		CallProgressChange(num);
	}

	private void SetAlreadyOwnedItemCount()
	{
		_alreadyOwnedItemCount = CalcItemCount();
	}

	public override void OnAddItem()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
		GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded();
		_alreadyOwnedItemCount = CalcItemCount();
		OnUpdateInventory();
	}

	public override void OnRemoveItem()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
	}
}
