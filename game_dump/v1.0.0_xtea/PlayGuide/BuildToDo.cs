using System;
using Building_;
using L10N;

namespace PlayGuide;

public class BuildToDo : ToDoBase
{
	private readonly string _id;

	public BuildToDo(string id)
	{
		_id = id;
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(_id);
		base.LocalText = T._("<link>{0}</link> 건설", (blueprint == null) ? _id : blueprint.LocalizedName);
	}

	private void OnFinishBuilding(Artifact structure)
	{
		if (string.IsNullOrEmpty(_id) || string.Compare(_id, structure.ArtifactId, StringComparison.OrdinalIgnoreCase) == 0)
		{
			CallComplete();
		}
	}

	public override bool OnClicked()
	{
		UIManager.FindScript<RecipeSelectorGroup>().Open(RecipeSystem.RecipeType.Building, _id);
		return true;
	}

	public override void OnAddItem()
	{
		GameSystem<BuildSystem>.Instance().BuildFinished += OnFinishBuilding;
	}

	public override void OnRemoveItem()
	{
		GameSystem<BuildSystem>.Instance().BuildFinished -= OnFinishBuilding;
	}
}
