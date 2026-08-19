using UnityEngine;

namespace Durango.UI;

public class UnstableRoutesBackground : RoutesViewerBackground
{
	private Vector2 _gridOffset;

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
	private float _gridCount;

	protected override Vector2 GridOffset => _gridOffset;

	public void SetCompass(bool value)
	{
		_onCompass = value;
	}

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
		float num = InnerRect.height / Mathf.Max(1f, _gridCount);
		_gridOffset.x = InnerRect.width * 0.5f % num;
		_gridOffset.y = InnerRect.height * 0.5f % num;
		return num;
	}

	private void DrawHighlight(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Vector2 size = new Vector2(OutterRect.width * 0.5f, OutterRect.height * 0.5f);
		Vector2 center = OutterRect.center;
		DrawHighlight(verts, uvs, cols, center, size);
	}
}
