using Building;
using Durango.UI;
using Durango.Utils;
using L10N;
using Shared.Building;

namespace Durango.Logic.PlayGuide;

public class CompletedArtifactToDo : ToDoBase
{
	private readonly string _id;

	public CompletedArtifactToDo(string id)
	{
		_id = id;
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(_id);
		base.LocalText = T._("<link>{0}</link> 건설", (blueprint == null) ? _id : blueprint.Name);
	}

	public override bool OnClicked()
	{
		RecipeSelectorGroup.OpenRecipeOrLearnableUI(RecipeSystem.RecipeType.Building, _id);
		return true;
	}

	public override void OnAddItem()
	{
		Singleton<ArtifactManager>.Instance().Added += CheckArtifact;
		Singleton<ArtifactManager>.Instance().StateChanged += CheckArtifact;
		foreach (Artifact artifact in Singleton<ArtifactManager>.Instance().GetArtifacts())
		{
			CheckArtifact(artifact);
		}
	}

	public override void OnRemoveItem()
	{
		Singleton<ArtifactManager>.Instance().Added -= CheckArtifact;
		Singleton<ArtifactManager>.Instance().StateChanged -= CheckArtifact;
	}

	private void CheckArtifact(Artifact artifact)
	{
		if (BuildToDo.CanComplete(artifact, _id) && artifact.FounderId == PlayerBehavior.LocalPlayer.EntityId && artifact.BuildState == BuildingState.Completed)
		{
			CallComplete();
		}
	}
}
