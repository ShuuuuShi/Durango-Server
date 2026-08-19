using UnityEngine;

namespace Durango.UI;

public class WorldRoutesBackground : RoutesViewerBackground
{
	[SerializeField]
	private bool _onCorner;

	[SerializeField]
	private bool _onSide;

	[SerializeField]
	private bool _onCenter;

	[SerializeField]
	private bool _onWave;

	[SerializeField]
	private bool _onScatter;

	[SerializeField]
	private bool _onCompass;

	[SerializeField]
	private bool _onGrids;

	[SerializeField]
	private bool _onHighlight;

	[SerializeField]
	private bool _onGrunge;

	[SerializeField]
	private float _hilightSize;

	[Range(0f, 1f)]
	[SerializeField]
	private float _hilightRatio;

	protected override void OnFillBackground(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		if (_onCorner)
		{
			DrawCorner(verts, uvs, cols);
		}
		if (_onSide)
		{
			DrawSide(verts, uvs, cols);
		}
		if (_onCenter)
		{
			DrawCenter(verts, uvs, cols);
		}
		if (_onWave)
		{
			DrawWave(verts, uvs, cols);
		}
		if (_onScatter)
		{
			DrawScatter(verts, uvs, cols);
		}
		if (_onCompass)
		{
			DrawCompass(verts, uvs, cols);
		}
		if (_onGrids)
		{
			DrawGrids(verts, uvs, cols);
		}
		if (_onHighlight)
		{
			DrawHighlight(verts, uvs, cols);
		}
		if (_onGrunge)
		{
			DrawGrunge(verts, uvs, cols);
		}
	}

	protected override float GetGridSize()
	{
		return InnerRect.height / (float)KUtility.GetSize(_rowColors);
	}

	private void DrawHighlight(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		float num = base.GridSize * Mathf.Max(1f, _hilightSize);
		Vector2 size = new Vector2(num * Mathf.Max(1f, _hilightRatio), OutterRect.height * 0.5f);
		float num2 = num;
		float y = OutterRect.yMin + OutterRect.height * 0.5f;
		for (; !(num2 > InnerRect.width); num2 += num)
		{
			Vector2 pos = new Vector2(InnerRect.x + num2, y);
			DrawHighlight(verts, uvs, cols, pos, size);
		}
	}
}
