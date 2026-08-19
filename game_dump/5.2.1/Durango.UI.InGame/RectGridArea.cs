using UnityEngine;

namespace Durango.UI.InGame;

public class RectGridArea : GridAreaBase
{
	public Point2 Size;

	public Color BgColor;

	private const int GridThickness = 4;

	private const int BorderThickness = 8;

	private readonly Color _gridColor = new Color(1f, 1f, 1f, 0.4f);

	private readonly Color _borderColor = Color.black;

	public override Vector2 CenterTile => Tile.ToVector2() + Size.ToVector2() * 0.5f;

	protected override void DoDraw(UIGeometry geometry)
	{
		DrawBgQuads(geometry);
		DrawTileQuads(geometry);
		DrawGridQuads(geometry, 4, _gridColor);
		DrawBorderQuads(geometry, 8, _borderColor);
	}

	private void DrawBgQuads(UIGeometry geometry)
	{
		Vector3 pos = (Tile - TileOffset).ToVector2() * 200f;
		Point2 point = Size * 200;
		Color bgColor = BgColor;
		bgColor.a *= 0.25f;
		DrawQuad(geometry, pos, point.ToVector2(), bgColor);
	}

	private void DrawTileQuads(UIGeometry geometry)
	{
		if (TileColorFunc == null)
		{
			return;
		}
		for (int i = 0; i < Size.x; i++)
		{
			for (int j = 0; j < Size.y; j++)
			{
				Point2 point = Tile + new Point2(i, j);
				if (TileColorFunc(point, out var color))
				{
					color.a *= 0.25f;
					Vector3 pos = (point - TileOffset).ToVector2() * 200f;
					DrawQuad(geometry, pos, Vector2.one * 200f, color);
				}
			}
		}
	}

	private void DrawGridQuads(UIGeometry geometry, int thickness, Color color)
	{
		Vector3 vector = (Tile - TileOffset).ToVector2() * 200f;
		Point2 point = Size * 200;
		for (int i = 1; i < Size.x; i++)
		{
			Vector3 pos = vector + Vector3.right * ((float)(i * 200) - (float)thickness * 0.5f);
			DrawQuad(geometry, pos, new Vector2(thickness, point.y), color);
		}
		for (int j = 1; j < Size.y; j++)
		{
			Vector3 pos2 = vector + Vector3.up * ((float)(j * 200) - (float)thickness * 0.5f);
			DrawQuad(geometry, pos2, new Vector2(point.x, thickness), color);
		}
	}

	private void DrawBorderQuads(UIGeometry geometry, int thickness, Color color)
	{
		Vector3 vector = (Tile - TileOffset).ToVector2() * 200f;
		Point2 point = Size * 200;
		DrawQuad(geometry, vector - new Vector3(1f, 1f) * thickness * 0.5f, new Vector2(thickness, point.y + thickness), color);
		DrawQuad(geometry, vector - new Vector3(1f, 1f) * thickness * 0.5f + Vector3.right * point.x, new Vector2(thickness, point.y + thickness), color);
		DrawQuad(geometry, vector - new Vector3(1f, 1f) * thickness * 0.5f, new Vector2(point.x + thickness, thickness), color);
		DrawQuad(geometry, vector - new Vector3(1f, 1f) * thickness * 0.5f + Vector3.up * point.y, new Vector2(point.x + thickness, thickness), color);
	}
}
