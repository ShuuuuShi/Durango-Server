using Messages;
using Shared.Etc;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class Road : ArtifactComponent
{
	private string _roadSprite;

	protected override bool HasShadow => false;

	protected override bool InteractionDisabled => true;

	public override void PostInit(string artifactId, int worldTileX, int worldTileY, Rotation rotation, Point2 size)
	{
		AddToRoadCollisionGrid(worldTileX, worldTileY);
	}

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		string text = msg.Parts.Get("common");
		bool flag = false;
		string roadSprite = _roadSprite;
		if (!string.IsNullOrEmpty(text))
		{
			ArtifactModel artifactModel = SingletonDict<string, ArtifactModel>.Get(text);
			if (artifactModel != null)
			{
				_roadSprite = artifactModel.file_names[0];
				flag = true;
			}
		}
		if (flag)
		{
			if (roadSprite != _roadSprite)
			{
				RoadManager.AddRoad(base.Artifact.WorldTile, _roadSprite);
			}
		}
		else
		{
			RoadManager.RemoveRoad(base.Artifact.WorldTile);
		}
		return true;
	}

	public void AddToRoadCollisionGrid(int worldTileX, int worldTileY)
	{
		Point2 point = new Point2(worldTileX, worldTileY);
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(point);
		if ((Object)(object)chunkFromTile != (Object)null)
		{
			chunkFromTile.FillRoadCollisionToGrid(chunkFromTile.FromWorldTile(point), this);
		}
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(base.Artifact.WorldTile);
		if ((Object)(object)chunkFromTile != (Object)null)
		{
			chunkFromTile.RemoveFromCollisionGrid(base.Artifact);
		}
		RoadManager.RemoveRoad(base.Artifact.WorldTile);
	}
}
