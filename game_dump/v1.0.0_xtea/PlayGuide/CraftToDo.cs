using System.Collections.Generic;
using ItemSystem;
using L10N;

namespace PlayGuide;

public class CraftToDo : ToDoBase
{
	private readonly TagEvaluator _tag;

	private readonly string _id;

	public CraftToDo(string tag, string id)
	{
		_tag = new TagEvaluator(tag);
		_id = id;
		base.LocalText = T._("<link>{0}</link> 제작", TagData.GetTagName(tag));
	}

	private void OnFinishCrafting(IList<ItemData> items, string recipe)
	{
		if (!string.IsNullOrEmpty(_id) && recipe != _id)
		{
			return;
		}
		int i = 0;
		for (int num = items?.Count ?? 0; i < num; i++)
		{
			if (items != null && _tag.Evaluate(items[i]))
			{
				CallComplete();
				break;
			}
		}
	}

	public override bool OnClicked()
	{
		UIManager.FindScript<RecipeSelectorGroup>().Open(RecipeSystem.RecipeType.Crafting, _id);
		return true;
	}

	public override void OnAddItem()
	{
		GameSystem<ItemCraftingSystem>.Instance().CraftingFinished += OnFinishCrafting;
	}

	public override void OnRemoveItem()
	{
		GameSystem<ItemCraftingSystem>.Instance().CraftingFinished -= OnFinishCrafting;
	}
}
