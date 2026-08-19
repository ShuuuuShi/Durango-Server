using Building;
using Durango.Utils;
using JetBrains.Annotations;

namespace Durango.UI;

public class BlueprintTodoCollection : ItemSlotsTodoCollection
{
	private readonly Blueprint _target;

	public BlueprintTodoCollection([NotNull] Blueprint target)
	{
		_target = target;
		base.Key = target.Id;
		Title = target.Name;
		Icon = target.Icon;
		Begin();
		if (target.Slots != null)
		{
			BlueprintSlot[] slots = target.Slots;
			foreach (BlueprintSlot slot in slots)
			{
				Add(slot);
			}
		}
		End();
	}

	protected override void FillSlotCount()
	{
		RecipeSystem.HasMaterials(_target, Point2.zero, out HasTool, SlotCounts);
	}

	protected override void OpenUI()
	{
		if (Singleton<UIManager>.HasInstance())
		{
			UIManager.FindScript<RecipeSelectorGroup>().Open(RecipeSystem.RecipeType.Building, _target.Id);
		}
	}
}
