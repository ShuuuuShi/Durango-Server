using System;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.InGame;

public abstract class GridAreaBase
{
	public Point2 Tile;

	public string ButtonText;

	public PresetButton.Style ButtonStyle;

	public Color ButtonColor;

	public Action<Point2> OnSelect;

	public TileColorFunc TileColorFunc;

	protected const float TileAlpha = 0.25f;

	private float _finalAlpha = 1f;

	protected Point2 TileOffset;

	public abstract Vector2 CenterTile { get; }

	protected GridAreaBase()
	{
		TileOffset = Singleton<GridAreaViewer>.Instance().GetTileOffset();
	}

	public bool HasButton()
	{
		return ButtonText != null;
	}

	public void Draw(UIGeometry geometry, float alpha)
	{
		_finalAlpha = alpha;
		DoDraw(geometry);
	}

	protected abstract void DoDraw(UIGeometry geometry);

	protected void DrawQuad(UIGeometry geometry, Vector3 pos, Vector2 size, Color color)
	{
		if (!(color.a <= 0f))
		{
			geometry.verts.Add(pos);
			geometry.verts.Add(pos + Vector3.right * size.x);
			geometry.verts.Add(pos + Vector3.right * size.x + Vector3.up * size.y);
			geometry.verts.Add(pos + Vector3.up * size.y);
			geometry.uvs.Add(new Vector2(0f, 0f));
			geometry.uvs.Add(new Vector2(0f, 1f));
			geometry.uvs.Add(new Vector2(1f, 1f));
			geometry.uvs.Add(new Vector2(1f, 0f));
			color.a *= _finalAlpha;
			geometry.cols.Add(color);
			geometry.cols.Add(color);
			geometry.cols.Add(color);
			geometry.cols.Add(color);
		}
	}
}
