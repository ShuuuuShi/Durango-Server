using System.Collections.Generic;
using Durango.Logic.Item;

namespace Durango.Logic.PlayGuide;

public class GetItemToDo : ToDoBase
{
	public readonly SingularTagFilter[] RequiredTags;

	private readonly TagEvaluator _tag;

	public GetItemToDo(string tag, Dictionary<string, Dictionary<string, string>> requiredTags)
	{
		if (!string.IsNullOrEmpty(tag))
		{
			_tag = new TagEvaluator(tag);
		}
		else
		{
			if (requiredTags == null || requiredTags.Count <= 0)
			{
				return;
			}
			RequiredTags = new SingularTagFilter[requiredTags.Count];
			int num = 0;
			foreach (KeyValuePair<string, Dictionary<string, string>> requiredTag in requiredTags)
			{
				RequiredTags[num] = new SingularTagFilter(requiredTag.Key, 0);
				num++;
			}
		}
	}

	public GetItemToDo(SingularTagFilter[] filters)
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
		int progress = CalcItemCount();
		CallProgressChange(progress);
	}

	public override void OnAddItem()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
		OnUpdateInventory();
	}

	public override void OnRemoveItem()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
	}
}
