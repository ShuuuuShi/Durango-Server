using Building;
using L10N;

namespace Durango.Logic.PlayGuide;

public class BuildCompleteToDo : ToDoBase
{
	private readonly string _id;

	public BuildCompleteToDo(string id)
	{
		_id = id;
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(_id);
		base.LocalText = T._("<em>{0}</em> 건설 완료", (blueprint == null) ? _id : blueprint.Name);
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
		if (BuildToDo.CanComplete(artifact, _id))
		{
			CallComplete();
		}
	}
}
