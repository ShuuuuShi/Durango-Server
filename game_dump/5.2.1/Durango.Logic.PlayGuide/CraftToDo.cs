using Durango.Logic.Item;
using Durango.UI;
using L10N;
using Messages;

namespace Durango.Logic.PlayGuide;

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

	private void OnSuccessCraft(string recipeId, Crafted crafted)
	{
		if (!string.IsNullOrEmpty(_id) && recipeId != _id)
		{
			return;
		}
		int i = 0;
		for (int size = KUtility.GetSize(crafted.Items); i < size; i++)
		{
			ItemData orMakeItemData = InventorySystem.GetOrMakeItemData(crafted.Items[i]);
			if (_tag.Evaluate(orMakeItemData))
			{
				CallComplete();
				break;
			}
		}
	}

	public override bool OnClicked()
	{
		RecipeSelectorGroup.OpenRecipeOrLearnableUI(RecipeSystem.RecipeType.Crafting, _id);
		return true;
	}

	public override void OnAddItem()
	{
		GameSystem<CraftSystem>.Instance().CraftSucceed += OnSuccessCraft;
	}

	public override void OnRemoveItem()
	{
		GameSystem<CraftSystem>.Instance().CraftSucceed -= OnSuccessCraft;
	}
}
