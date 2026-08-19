using System;
using Building;
using Durango.UI;
using Durango.Utils;
using L10N;
using Shared.Building;

namespace Durango.Logic.PlayGuide;

public class BuildToDo : ToDoBase
{
	private readonly string _id;

	public BuildToDo(string id)
	{
		_id = id;
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(_id);
		base.LocalText = T._("<link>{0}</link> 건설", (blueprint == null) ? _id : blueprint.Name);
	}

	public static bool CanComplete(Artifact artifact, string id)
	{
		return string.IsNullOrEmpty(id) || string.Compare(id, artifact.BlueprintId, StringComparison.OrdinalIgnoreCase) == 0;
	}

	public override bool OnClicked()
	{
		RecipeSelectorGroup.OpenRecipeOrLearnableUI(RecipeSystem.RecipeType.Building, _id);
		return true;
	}

	public override void OnAddItem()
	{
		GameSystem<BuildSystem>.Instance().BuildFinished += BuildSystem_BuildFinished;
		Singleton<ArtifactManager>.Instance().Added += ArtifactManager_Added;
		foreach (Artifact artifact in Singleton<ArtifactManager>.Instance().GetArtifacts())
		{
			ArtifactManager_Added(artifact);
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<BuildSystem>.Instance().BuildFinished -= BuildSystem_BuildFinished;
		Singleton<ArtifactManager>.Instance().Added -= ArtifactManager_Added;
	}

	private void BuildSystem_BuildFinished(Artifact artifact)
	{
		if (CanComplete(artifact, _id))
		{
			CallComplete();
		}
	}

	private void ArtifactManager_Added(Artifact artifact)
	{
		if (CanComplete(artifact, _id) && artifact.FounderId == PlayerBehavior.LocalPlayer.EntityId && artifact.BuildState == BuildingState.Completed)
		{
			CallComplete();
		}
	}
}
