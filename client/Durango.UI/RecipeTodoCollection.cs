using Crafting;
using Durango.Utils;
using JetBrains.Annotations;

namespace Durango.UI;

public class RecipeTodoCollection : ItemSlotsTodoCollection
{
	private readonly Recipe _target;

	public RecipeTodoCollection([NotNull] Recipe target)
	{
		_target = target;
		base.Key = target.Id;
		Title = target.Name;
		Icon = target.Icon;
		Begin();
		if (target.Slots != null)
		{
			RecipeSlot[] slots = target.Slots;
			foreach (RecipeSlot slot in slots)
			{
				Add(slot);
			}
		}
		if (target.HasRequiredTool)
		{
		}
		End();
	}

	protected override void FillSlotCount()
	{
		RecipeSystem.HasMaterials(_target, out HasTool, SlotCounts);
	}

	protected override void OpenUI()
	{
		if (Singleton<UIManager>.HasInstance())
		{
			UIManager.FindScript<RecipeSelectorGroup>().Open(RecipeSystem.RecipeType.Crafting, _target.Id);
		}
	}
}
