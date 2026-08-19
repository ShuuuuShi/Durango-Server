using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class StableRoutesWaveSprite : CustomFillSprite
{
	private static readonly string[] WaveSpriteSet = new string[2] { "stable_area_wave_00", "stable_area_wave_02" };

	private const string WhiteSprite = "map_square";

	private UISpriteData _white;

	private UISpriteData _wave0;

	private UISpriteData _wave1;

	private Vector3[] _waveCorners = new Vector3[4];

	protected override void OnStart()
	{
		base.OnStart();
		_white = ResourceSingleton<UISpriteManager>.Instance().GetSprite("map_square");
		_wave0 = ResourceSingleton<UISpriteManager>.Instance().GetSprite(WaveSpriteSet[0]);
		_wave1 = ResourceSingleton<UISpriteManager>.Instance().GetSprite(WaveSpriteSet[1]);
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		int size = verts.size;
		Vector3[] array = localCorners;
		Rect widgetRect = new Rect(array[0], array[2] - array[0]);
		Color white = Color.white;
		DrawWave(verts, uvs, cols, widgetRect, _wave1, new Vector2(0.35f, 0.73f), new Vector2(0.35f, 0.93f), new Vector2(0.2f, 0.93f), new Vector2(0.2f, 0.73f), new Rect(0f, 0f, 1f, 0.28f), white);
		DrawWave(verts, uvs, cols, widgetRect, _wave0, new Vector2(0.33f, 0.93f), new Vector2(0.62f, 0.93f), new Vector2(0.62f, 1f), new Vector2(0.33f, 1f), new Rect(0f, 0f, 1f, 1f), white);
		DrawWave(verts, uvs, cols, widgetRect, _wave1, new Vector2(0.6f, 0.73f), new Vector2(0.6f, 0.93f), new Vector2(0.785f, 0.93f), new Vector2(0.785f, 0.73f), new Rect(0f, 0f, 1f, 0.28f), white);
		DrawWave(verts, uvs, cols, widgetRect, _wave1, new Vector2(0.75f, 0.73f), new Vector2(0.75f, 0.1f), new Vector2(1f, 0.1f), new Vector2(1f, 0.73f), new Rect(0f, 0.3f, 1f, 0.7f), white);
		DrawWave(verts, uvs, cols, widgetRect, _wave0, new Vector2(0.4f, 0.1f), new Vector2(0.76f, 0.1f), new Vector2(0.76f, 0f), new Vector2(0.4f, 0f), new Rect(0f, 0f, 1f, 1f), white);
		DrawWave(verts, uvs, cols, widgetRect, _wave1, new Vector2(0.4f, 0.1f), new Vector2(0.3f, 0.73f), new Vector2(0f, 0.73f), new Vector2(0.1f, 0.1f), new Rect(0f, 0.3f, 1f, 0.7f), white);
		DrawWave(verts, uvs, cols, widgetRect, _white, new Vector2(0.35f, 0.73f), new Vector2(0.35f, 0.93f), new Vector2(0.6f, 0.93f), new Vector2(0.6f, 0.73f), new Rect(0f, 0f, 1f, 1f), white);
		DrawWave(verts, uvs, cols, widgetRect, _white, new Vector2(0.4f, 0.1f), new Vector2(0.3f, 0.73f), new Vector2(0.75f, 0.73f), new Vector2(0.75f, 0.1f), new Rect(0f, 0f, 1f, 1f), white);
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}

	private static Vector3 GetPosition(Rect rect, Vector2 pivot)
	{
		return rect.position + Vector2.Scale(rect.size, pivot);
	}

	private void DrawWave(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Rect widgetRect, UISpriteData sprite, Vector2 r1, Vector2 r2, Vector2 r3, Vector2 r4, Rect uv, Color col)
	{
		ref Vector3 reference = ref _waveCorners[0];
		reference = GetPosition(widgetRect, r1);
		ref Vector3 reference2 = ref _waveCorners[1];
		reference2 = GetPosition(widgetRect, r2);
		ref Vector3 reference3 = ref _waveCorners[2];
		reference3 = GetPosition(widgetRect, r3);
		ref Vector3 reference4 = ref _waveCorners[3];
		reference4 = GetPosition(widgetRect, r4);
		DrawSprite(verts, uvs, cols, sprite, _waveCorners, col, uv);
	}
}
