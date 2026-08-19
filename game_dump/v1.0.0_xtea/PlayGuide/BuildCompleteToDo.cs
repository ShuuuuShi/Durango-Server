using System;
using Building_;
using L10N;

namespace PlayGuide;

public class BuildCompleteToDo : ToDoBase
{
	private readonly string _id;

	public BuildCompleteToDo(string id)
	{
		_id = id;
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(_id);
		base.LocalText = T._("<em>{0}</em> 건설 완료", (blueprint == null) ? _id : blueprint.LocalizedName);
	}

	public override void OnAddItem()
	{
		GameSystem<BuildSystem>.Instance().BuildCompleted += BuildSystem_BuildCompleted;
	}

	public override void OnRemoveItem()
	{
		GameSystem<BuildSystem>.Instance().BuildCompleted -= BuildSystem_BuildCompleted;
	}

	private void BuildSystem_BuildCompleted(Artifact artifact)
	{
		if (string.IsNullOrEmpty(_id) || string.Compare(_id, artifact.ArtifactId, StringComparison.OrdinalIgnoreCase) == 0)
		{
			CallComplete();
		}
	}
}
