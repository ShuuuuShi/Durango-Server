using Shared.Building;

public class Bridge : ArtifactComponent
{
	protected override Artifact.Interaction InteractionType => Artifact.Interaction.Context;

	public override string ContextIcon => "act_remove_road";

	public override bool IsIgnoreWaterDepth()
	{
		if (base.Artifact.BuildState != BuildingState.Completed)
		{
			return base.IsIgnoreWaterDepth();
		}
		return true;
	}
}
