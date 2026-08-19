using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.InGame;

public class SimpleTileEdgeArea : GridAreaBase
{
	[CanBeNull]
	public IEnumerable<Point2> Offsets;

	public override Vector2 CenterTile => (Vector2)Tile;

	protected override void DoDraw(UIGeometry geometry)
	{
		if (TileColorFunc == null || Offsets == null)
		{
			return;
		}
		foreach (Point2 offset in Offsets)
		{
			Point2 point = Tile + offset;
			if (TileColorFunc(point, out var color))
			{
				color.a *= 0.25f;
				Vector3 vector = (point - TileOffset).ToVector2() * 200f + Vector2.one * 15f * 0.5f;
				DrawQuad(geometry, vector - new Vector3(1f, 1f) * 15f * 0.5f, new Vector2(15f, 185f), color);
				DrawQuad(geometry, vector - new Vector3(1f, -1f) * 15f * 0.5f + Vector3.right * 185f, new Vector2(15f, 185f), color);
				DrawQuad(geometry, vector - new Vector3(-1f, 1f) * 15f * 0.5f, new Vector2(185f, 15f), color);
				DrawQuad(geometry, vector - new Vector3(1f, 1f) * 15f * 0.5f + Vector3.up * 185f, new Vector2(185f, 15f), color);
			}
		}
	}
}
