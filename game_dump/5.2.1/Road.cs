using Durango.Terrain;
using Durango.Utils;
using Messages;
using Yaml;
using Yaml.Util;

public class Road : ArtifactComponent
{
	private string _targetSprite;

	protected override Artifact.Interaction InteractionType => Artifact.Interaction.Context;

	public override string ContextIcon => "act_remove_road";

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		string text = msg.Parts.Get("common");
		string targetSprite = string.Empty;
		if (!string.IsNullOrEmpty(text))
		{
			ArtifactModel artifactModel = SingletonDict<string, ArtifactModel>.Get(text);
			if (artifactModel != null)
			{
				targetSprite = artifactModel.file_names[0];
			}
		}
		SetTargetSprite(targetSprite);
		return true;
	}

	public override void ArtifactPlaced()
	{
		Point2 worldTile = base.Artifact.WorldTile;
		TerrainChunkBase chunkFromTile = Durango.Utils.Singleton<TerrainBase>.Instance().GetChunkFromTile(worldTile);
		if (chunkFromTile != null)
		{
			chunkFromTile.FillRoadCollisionToGrid(chunkFromTile.FromWorldTile(worldTile), this);
		}
		OnUpdateTargetSprite();
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		TerrainChunkBase chunkFromTile = Durango.Utils.Singleton<TerrainBase>.Instance().GetChunkFromTile(base.Artifact.WorldTile);
		if (chunkFromTile != null)
		{
			chunkFromTile.RemoveFromCollisionGrid(base.Artifact);
		}
		SetTargetSprite(string.Empty);
	}

	private void SetTargetSprite(string sprite)
	{
		_targetSprite = sprite;
		OnUpdateTargetSprite();
	}

	private void OnUpdateTargetSprite()
	{
		Point2 worldTile = base.Artifact.WorldTile;
		TerrainChunkBase chunkFromTile = Durango.Utils.Singleton<TerrainBase>.Instance().GetChunkFromTile(worldTile);
		if (!(chunkFromTile == null) && !(chunkFromTile.RoadGrid == null))
		{
			Point2 localTile = chunkFromTile.FromWorldTile(worldTile);
			if (string.IsNullOrEmpty(_targetSprite))
			{
				chunkFromTile.RoadGrid.RemoveRoad(localTile);
			}
			else
			{
				chunkFromTile.RoadGrid.AddRoad(localTile, _targetSprite);
			}
		}
	}
}
