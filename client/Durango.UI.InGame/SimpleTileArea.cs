using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.InGame;

public class SimpleTileArea : GridAreaBase
{
	[CanBeNull]
	public IEnumerable<Point2> Offsets;

	public override Vector2 CenterTile => (Vector2)Tile;

	protected override void DoDraw(UIGeometry geometry)
	{
		DrawTileQuads(geometry);
	}

	private void DrawTileQuads(UIGeometry geometry)
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
				Vector3 pos = (point - TileOffset).ToVector2() * 200f;
				DrawQuad(geometry, pos, Vector2.one * 200f, color);
			}
		}
	}
}
