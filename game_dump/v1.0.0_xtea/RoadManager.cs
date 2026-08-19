using UnityEngine;

public class RoadManager : KSingleton<RoadManager>
{
	[SerializeField]
	private int _curveLineCount = 1;

	[SerializeField]
	private float _roadWidth = 180f;

	[SerializeField]
	private float _pivotRatio = 0.3f;

	[SerializeField]
	private float _randomOffset = 0.1f;

	[SerializeField]
	private bool _isTileRoad;

	[SerializeField]
	private Material _debugLineMaterial;

	private bool _showDebugLine;

	public static int CurveLineCount => KSingleton<RoadManager>.Instance()._curveLineCount;

	public static float RoadWidth => KSingleton<RoadManager>.Instance()._roadWidth;

	public static float PivotRatio => KSingleton<RoadManager>.Instance()._pivotRatio;

	public static float RandomOffset => KSingleton<RoadManager>.Instance()._randomOffset;

	public static bool IsTileRoad => KSingleton<RoadManager>.Instance()._isTileRoad;

	public static Material DebugLineMaterial => KSingleton<RoadManager>.Instance()._debugLineMaterial;

	public static bool ShowDebugLine => KSingleton<RoadManager>.Instance()._showDebugLine;

	public static RoadGrid.RoadTile GetRoad(Point2 tile)
	{
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(tile);
		if ((Object)(object)chunkFromTile == (Object)null || (Object)(object)chunkFromTile.RoadGrid == (Object)null)
		{
			return null;
		}
		Point2 localTile = chunkFromTile.FromWorldTile(tile);
		return chunkFromTile.RoadGrid.GetRoad(localTile);
	}

	public static bool HasRoad(Point2 tile)
	{
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(tile);
		if ((Object)(object)chunkFromTile == (Object)null || (Object)(object)chunkFromTile.RoadGrid == (Object)null)
		{
			return false;
		}
		Point2 localTile = chunkFromTile.FromWorldTile(tile);
		return chunkFromTile.RoadGrid.HasRoad(localTile);
	}

	public static void AddRoad(Point2 tile, string sprite)
	{
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(tile);
		if (!((Object)(object)chunkFromTile == (Object)null) && !((Object)(object)chunkFromTile.RoadGrid == (Object)null))
		{
			Point2 localTile = chunkFromTile.FromWorldTile(tile);
			chunkFromTile.RoadGrid.AddRoad(localTile, sprite);
		}
	}

	public static void RemoveRoad(Point2 tile)
	{
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(tile);
		if (!((Object)(object)chunkFromTile == (Object)null) && !((Object)(object)chunkFromTile.RoadGrid == (Object)null))
		{
			Point2 localTile = chunkFromTile.FromWorldTile(tile);
			chunkFromTile.RoadGrid.RemoveRoad(localTile);
		}
	}

	public static Rect GetMaskingRect(int linkCount)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		float num = ((IsTileRoad || linkCount == 0) ? 0f : ((linkCount != 1) ? 2f : 1f));
		return new Rect(num / 3f, 0f, 1f / 3f, 1f);
	}

	public static Rect GetRoadRect(string sprite)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		float num = ((!sprite.Contains("01")) ? 1 : 0);
		return new Rect(num / 2f, 0f, 0.5f, 1f);
	}

	[ExposedInEditor(null)]
	private void ForceUpdateRoads()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = TerrainA6.ClientPositionToChunkCoords(PlayerBehavior.LocalPlayer.CurrentPosition);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				TerrainChunkA6 terrainChunk = KSingleton<TerrainA6>.Instance().GetTerrainChunk(val + new Vector2((float)i, (float)j));
				if (!((Object)(object)terrainChunk == (Object)null) && !((Object)(object)terrainChunk.RoadGrid == (Object)null))
				{
					terrainChunk.RoadGrid.ForceUpdateRoads();
				}
			}
		}
	}

	[ExposedInEditor(null)]
	private void ToggleDebugLine()
	{
		if (Application.isPlaying)
		{
			_showDebugLine = !_showDebugLine;
			ForceUpdateRoads();
		}
	}
}
