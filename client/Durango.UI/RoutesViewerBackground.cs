using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public abstract class RoutesViewerBackground : CustomFillSprite
{
	public const int Padding = 8;

	public const int InnerPadding = 30;

	protected Rect WidgetRect;

	protected Rect OutterRect;

	protected Rect InnerRect;

	public static readonly string[] SideSpriteSet = new string[3] { "bg_map_outline_01", "bg_map_outline_02", "bg_map_outline_03" };

	public static readonly string CornerSprite = "bg_map_outline_00";

	public static readonly string CenterSprite = "bg_map";

	public static readonly string WhiteSprite = "map_square";

	public static readonly string[] GrungeBigSet = new string[2] { "bg_map_grunge_01", "bg_map_grunge_02" };

	public static readonly string[] GrungeSmallSet = new string[2] { "bg_map_grunge_03", "bg_map_grunge_04" };

	public const string CenterGrunge = "bg_map_grunge_05";

	public static readonly string MapHighlight = "explore_map_highlight";

	public static readonly string[] MapCrossBlotSet = new string[1] { "explore_map_cross_blot" };

	public static readonly string MapCompass = "explore_map_compass";

	public static readonly string[] WaveSpriteSet = new string[2] { "img_sailing_wave1", "img_sailing_wave2" };

	public static readonly string[] ScatterSpriteSet = new string[5] { "img_scatter_01", "img_scatter_02", "img_scatter_03", "img_scatter_04", "img_scatter_05" };

	private int _borderSize;

	private global::System.Random _random;

	[SerializeField]
	protected Color[] _rowColors;

	[SerializeField]
	private Vector2 _compasOffset;

	[SerializeField]
	private float _waveDensity;

	[Range(0f, 2f)]
	[SerializeField]
	private float _waveMinScale;

	[Range(0f, 2f)]
	[SerializeField]
	private float _waveMaxScale;

	[SerializeField]
	private Color _waveColor;

	[SerializeField]
	private float _scatterDensity;

	[Range(0f, 2f)]
	[SerializeField]
	private float _scatterMinScale;

	[Range(0f, 2f)]
	[SerializeField]
	private float _scatterMaxScale;

	protected float GridSize { get; private set; }

	protected virtual Vector2 GridOffset => Vector2.zero;

	protected override void OnStart()
	{
		base.OnStart();
		UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(SideSpriteSet[0]);
		_borderSize = sprite.height + sprite.paddingBottom + sprite.paddingTop;
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		_random = new global::System.Random(GetHashCode());
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		int size = verts.size;
		CalcRect();
		OnFillBackground(verts, uvs, cols);
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}

	protected abstract void OnFillBackground(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols);

	protected abstract float GetGridSize();

	private void CalcRect()
	{
		Vector3[] array = localCorners;
		WidgetRect = new Rect(array[0], array[2] - array[0]);
		OutterRect = new Rect(WidgetRect.position + Vector2.one * 8f, WidgetRect.size - Vector2.one * 8f * 2f);
		InnerRect = new Rect(WidgetRect.position + Vector2.one * 30f, WidgetRect.size - Vector2.one * 30f * 2f);
		GridSize = GetGridSize();
	}

	protected void DrawCenter(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Vector3[] array = localCorners;
		Vector3 vector = array[0] + new Vector3(_borderSize, _borderSize);
		Vector2 vector2 = array[2] - array[0] - new Vector3(_borderSize, _borderSize) * 2f;
		UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(CenterSprite);
		if (sprite == null)
		{
			return;
		}
		Point2 point = new Point2(sprite.width + sprite.paddingLeft + sprite.paddingRight, sprite.height + sprite.paddingBottom + sprite.paddingTop);
		float num = 0f;
		float num2 = 0f;
		while (!(num2 > vector2.y))
		{
			if (num > vector2.x)
			{
				num = 0f;
				num2 += (float)point.y;
			}
			else
			{
				Vector3 vector3 = new Vector3(num, num2);
				Vector3 vector4 = vector + vector3;
				DrawBackgroundSprite(size: new Vector2(Mathf.Min(point.x, vector2.x - vector3.x), Mathf.Min(point.y, vector2.y - vector3.y)), verts: verts, uvs: uvs, cols: cols, sprite: CenterSprite, pos: vector4, r: Rotate.Nothing);
				num += (float)point.x;
			}
		}
	}

	protected void DrawCorner(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Vector3[] array = localCorners;
		array[1] += new Vector3(0f, -_borderSize);
		array[2] += new Vector3(-_borderSize, -_borderSize);
		array[3] += new Vector3(-_borderSize, 0f);
		for (int i = 0; i < array.Length; i++)
		{
			Rotate r = Rotate.Nothing;
			switch (i)
			{
			case 0:
				r = Rotate.Radial90;
				break;
			case 1:
				r = Rotate.Nothing;
				break;
			case 2:
				r = Rotate.Radial270;
				break;
			case 3:
				r = Rotate.Radial180;
				break;
			}
			DrawBackgroundSprite(verts, uvs, cols, CornerSprite, array[i], new Vector2(_borderSize, _borderSize), r);
		}
	}

	protected void DrawSide(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Vector3[] array = localCorners;
		Vector3[] array2 = new Vector3[4]
		{
			array[1] + new Vector3(_borderSize, -_borderSize),
			array[3] + new Vector3(-_borderSize, _borderSize),
			array[0] + new Vector3(_borderSize, 0f),
			array[0] + new Vector3(0f, _borderSize)
		};
		Vector2 vector = array[2] - array[0];
		UISpriteData uISpriteData = ((KUtility.GetSize(SideSpriteSet) != 0) ? ResourceSingleton<UISpriteManager>.Instance().GetSprite(SideSpriteSet[0]) : null);
		if (uISpriteData == null)
		{
			return;
		}
		Point2 point = new Point2(uISpriteData.width + uISpriteData.paddingLeft + uISpriteData.paddingRight, uISpriteData.height + uISpriteData.paddingBottom + uISpriteData.paddingTop);
		for (int i = 0; i < array2.Length; i++)
		{
			Vector3 vector2 = array2[i];
			Vector2 zero = Vector2.zero;
			switch (i)
			{
			case 0:
			case 2:
				zero.x = vector.x - (float)_borderSize * 2f;
				zero.y = _borderSize;
				break;
			case 1:
			case 3:
			{
				zero.x = _borderSize;
				zero.y = vector.y - (float)_borderSize * 2f;
				int x = point.x;
				point.x = point.y;
				point.y = x;
				break;
			}
			}
			Rotate r = Rotate.Nothing;
			switch (i)
			{
			case 0:
				r = Rotate.Nothing;
				break;
			case 1:
				r = Rotate.Radial270;
				break;
			case 2:
				r = Rotate.Radial180;
				break;
			case 3:
				r = Rotate.Radial90;
				break;
			}
			float num = 0f;
			float num2 = 0f;
			while (!(num2 > zero.y))
			{
				if (num > zero.x)
				{
					num = 0f;
					num2 += (float)point.y;
					continue;
				}
				Vector3 vector3 = new Vector3(num, num2);
				Vector3 vector4 = vector2 + vector3;
				Vector2 size = new Vector2(Mathf.Min(point.x, zero.x - vector3.x), Mathf.Min(point.y, zero.y - vector3.y));
				int num3 = _random.Next(0, SideSpriteSet.Length);
				DrawBackgroundSprite(verts, uvs, cols, SideSpriteSet[num3], vector4, size, r);
				num += (float)point.x;
			}
		}
	}

	protected void DrawGrids(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Color color = new Color32(0, 0, 0, 30);
		float num = InnerRect.height;
		float num2 = InnerRect.width;
		for (float num3 = GridOffset.y; num3 < num - 1f; num3 += GridSize)
		{
			if (num3 > 0f)
			{
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(WhiteSprite),
					Position = InnerRect.position + new Vector2(-22f, num3),
					Size = new Vector2(num2 + 44f, 1f),
					Color = color,
					Rect = new Rect(0f, 0f, 1f, 1f)
				});
			}
		}
		for (float num3 = GridOffset.x; num3 < num2 - 1f; num3 += GridSize)
		{
			if (num3 > 0f)
			{
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(WhiteSprite),
					Position = InnerRect.position + new Vector2(num3, -22f),
					Size = new Vector2(1f, num + 44f),
					Color = color,
					Rect = new Rect(0f, 0f, 1f, 1f)
				});
			}
		}
	}

	protected void DrawCompass(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(MapCompass);
		if (sprite != null)
		{
			Vector2 size = new Vector2(sprite.width + sprite.paddingLeft + sprite.paddingRight, sprite.height + sprite.paddingBottom + sprite.paddingTop);
			Vector2 position = InnerRect.position + new Vector2(0f, InnerRect.height);
			position += _compasOffset;
			DrawSprite(verts, uvs, cols, new DrawParam
			{
				Sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(MapCompass),
				Position = position,
				Size = size,
				Color = Color.white,
				Pivot = new Vector2(0f, 1f),
				Rect = new Rect(0f, 0f, 1f, 1f)
			});
		}
	}

	protected void DrawGrunge(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Rect outterRect = OutterRect;
		Rect rect = new Rect(outterRect);
		for (int i = 0; i < 4; i++)
		{
			int num = _random.Next(0, GrungeBigSet.Length);
			UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(GrungeBigSet[num]);
			if (sprite != null)
			{
				Vector2 position = Vector2.zero;
				Vector2 vector = new Vector2(sprite.width + sprite.paddingLeft + sprite.paddingRight, outterRect.height * 0.5f);
				switch (i)
				{
				case 0:
					rect.xMin = Mathf.Max(rect.xMin, outterRect.xMin + vector.x);
					position = new Vector2(outterRect.xMin, outterRect.yMin);
					break;
				case 1:
					rect.xMin = Mathf.Max(rect.xMin, outterRect.xMin + vector.x);
					position = new Vector2(outterRect.xMin, outterRect.yMax);
					vector.y = 0f - vector.y;
					break;
				case 2:
					rect.xMax = Mathf.Min(rect.xMax, outterRect.xMax - vector.x);
					position = new Vector2(outterRect.xMax, outterRect.yMax);
					vector = -vector;
					break;
				case 3:
					rect.xMax = Mathf.Min(rect.xMax, outterRect.xMax - vector.x);
					position = new Vector2(outterRect.xMax, outterRect.yMin);
					vector.x = 0f - vector.x;
					break;
				}
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = sprite,
					Position = position,
					Size = vector,
					Color = Color.white,
					Rect = new Rect(0f, 0f, 1f, 1f)
				});
			}
		}
		if (rect.width <= 0f)
		{
			return;
		}
		for (int j = 0; j < 2; j++)
		{
			float num2 = 100f * (float)_random.NextDouble();
			while (true)
			{
				int num3 = _random.Next(0, GrungeSmallSet.Length);
				UISpriteData sprite2 = ResourceSingleton<UISpriteManager>.Instance().GetSprite(GrungeSmallSet[num3]);
				if (sprite2 == null)
				{
					break;
				}
				Vector2 size = new Vector2(sprite2.width + sprite2.paddingLeft + sprite2.paddingRight, sprite2.height + sprite2.paddingBottom + sprite2.paddingTop);
				if (num2 + size.x > rect.width)
				{
					break;
				}
				int num4 = _random.Next(0, 2);
				Vector2 position2 = new Vector2(rect.xMin + num2 + ((num4 != 0) ? size.x : 0f), (j != 0) ? rect.yMax : rect.yMin);
				if (j == 1)
				{
					size.y = 0f - size.y;
				}
				if (num4 == 1)
				{
					size.x = 0f - size.x;
				}
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = sprite2,
					Position = position2,
					Size = size,
					Color = Color.white,
					Rect = new Rect(0f, 0f, 1f, 1f)
				});
				num2 = num2 + Mathf.Abs(size.x) + 100f * (float)_random.NextDouble();
			}
		}
		UISpriteData sprite3 = ResourceSingleton<UISpriteManager>.Instance().GetSprite("bg_map_grunge_05");
		if (sprite3 != null)
		{
			Vector2 vector2 = new Vector2(sprite3.width + sprite3.paddingLeft + sprite3.paddingRight, sprite3.height + sprite3.paddingBottom + sprite3.paddingTop);
			int num5 = Mathf.CeilToInt(rect.width * rect.height / (vector2.x * vector2.y));
			for (int k = 0; k < num5; k++)
			{
				Vector2 position3 = rect.position + new Vector2(rect.width * (float)_random.NextDouble(), rect.height * (float)_random.NextDouble());
				float num6 = Mathf.Lerp(0.7f, 1.1f, (float)_random.NextDouble());
				float angle = Mathf.Lerp(0f, 360f, (float)_random.NextDouble());
				Vector2 size2 = vector2 * num6;
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = sprite3,
					Position = position3,
					Size = size2,
					Color = Color.white,
					Rect = new Rect(0f, 0f, 1f, 1f),
					Pivot = new Vector2(0.5f, 0.5f),
					Angle = angle
				});
			}
		}
	}

	protected void DrawHighlight(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Vector2 pos, Vector2 size)
	{
		Vector2 position = OutterRect.position;
		Vector2 vector = position + OutterRect.size;
		for (int i = 0; i < 4; i++)
		{
			Vector2 position2 = pos;
			Vector2 vector2 = size;
			Color color = Color.white;
			switch (i)
			{
			case 1:
				vector2.y = 0f - vector2.y;
				color = Color.black;
				break;
			case 2:
				vector2 = -vector2;
				break;
			case 3:
				vector2.x = 0f - vector2.x;
				color = Color.black;
				break;
			}
			color.a = 0.4f;
			vector2.x = Mathf.Clamp(vector2.x, position.x - position2.x, vector.x - position2.x);
			vector2.y = Mathf.Clamp(vector2.y, position.y - position2.y, vector.y - position2.y);
			DrawSprite(verts, uvs, cols, new DrawParam
			{
				Sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(MapHighlight),
				Position = position2,
				Size = vector2,
				Color = color,
				Rect = new Rect(0f, 0f, 1f, 1f)
			});
		}
		int num = _random.Next(0, MapCrossBlotSet.Length);
		UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(MapCrossBlotSet[num]);
		if (sprite != null)
		{
			Vector2 size2 = new Vector2(sprite.width + sprite.paddingLeft + sprite.paddingRight, sprite.height + sprite.paddingBottom + sprite.paddingTop);
			DrawSprite(verts, uvs, cols, new DrawParam
			{
				Sprite = sprite,
				Position = pos,
				Size = size2,
				Color = Color.white,
				Rect = new Rect(0f, 0f, 1f, 1f),
				Pivot = new Vector2(0.5f, 0.5f)
			});
		}
	}

	protected void DrawWave(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		int num = Mathf.CeilToInt(OutterRect.width * OutterRect.height * _waveDensity);
		int num2 = num / WaveSpriteSet.Length;
		for (int i = 0; i < WaveSpriteSet.Length; i++)
		{
			UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(WaveSpriteSet[i]);
			Point2 size = GetSize(sprite);
			for (int j = 0; j < num2; j++)
			{
				Vector3 vector = new Vector3(InnerRect.xMin + InnerRect.width * (float)_random.NextDouble(), InnerRect.yMin + InnerRect.height * (float)_random.NextDouble());
				float num3 = _waveMinScale + (float)_random.NextDouble() * (_waveMaxScale - _waveMinScale);
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = sprite,
					Position = vector,
					Pivot = new Vector2(0.5f, 0.5f),
					Size = new Vector2((float)size.x * num3, (float)size.y * num3),
					Rect = new Rect(0f, 0f, 1f, 1f),
					Color = _waveColor
				});
			}
		}
	}

	protected void DrawScatter(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		int num = Mathf.CeilToInt(OutterRect.width * OutterRect.height * _scatterDensity);
		int num2 = num / ScatterSpriteSet.Length;
		for (int i = 0; i < ScatterSpriteSet.Length; i++)
		{
			UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(ScatterSpriteSet[i]);
			Point2 size = GetSize(sprite);
			for (int j = 0; j < num2; j++)
			{
				Vector3 vector = new Vector3(InnerRect.xMin + InnerRect.width * (float)_random.NextDouble(), InnerRect.yMin + InnerRect.height * (float)_random.NextDouble());
				float num3 = _scatterMinScale + (float)_random.NextDouble() * (_scatterMaxScale - _scatterMinScale);
				float angle = _random.Next(360);
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = sprite,
					Position = vector,
					Pivot = new Vector2(0.5f, 0.5f),
					Size = new Vector2((float)size.x * num3, (float)size.y * num3),
					Rect = new Rect(0f, 0f, 1f, 1f),
					Angle = angle,
					Color = Color.white
				});
			}
		}
	}

	protected void DrawBackgroundSprite(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, string sprite, Vector2 pos, Vector2 size, Rotate r)
	{
		int size2 = KUtility.GetSize(_rowColors);
		int num = Mathf.Clamp(Mathf.FloorToInt((pos.y - InnerRect.y) / GridSize), 0, size2 - 1);
		int num2 = Mathf.Clamp(Mathf.FloorToInt((pos.y + size.y - InnerRect.y) / GridSize), 0, size2 - 1);
		if (size.x == 0f || size.y == 0f)
		{
			return;
		}
		for (int i = num; i <= num2; i++)
		{
			float a = ((i <= 0) ? float.MinValue : (InnerRect.y + (float)i * GridSize));
			float a2 = ((i >= size2 - 1) ? float.MaxValue : (InnerRect.y + (float)(i + 1) * GridSize));
			Vector2 vector = pos;
			Vector2 vector2 = pos + size;
			vector.y = Mathf.Max(a, vector.y);
			vector2.y = Mathf.Min(a2, vector2.y);
			Vector2 vector3 = vector2 - vector;
			Color color = _rowColors[i];
			float num3 = (vector.y - pos.y) / size.y;
			float num4 = vector3.y / size.y;
			float angle = 0f;
			Rect rect;
			Vector2 vector4;
			Vector2 size3;
			switch (r)
			{
			case Rotate.Radial90:
				rect = new Rect(num3, 0f, num4, 1f);
				angle = 90f;
				vector4 = new Vector2(0f, 1f);
				size3 = new Vector2(vector3.y, vector3.x);
				break;
			case Rotate.Radial180:
				rect = new Rect(0f, 1f - num3 - num4, 1f, num4);
				angle = 180f;
				vector4 = new Vector2(1f, 1f);
				size3 = vector3;
				break;
			case Rotate.Radial270:
				rect = new Rect(1f - num3 - num4, 0f, num4, 1f);
				angle = 270f;
				vector4 = new Vector2(1f, 0f);
				size3 = new Vector2(vector3.y, vector3.x);
				break;
			default:
				rect = new Rect(0f, num3, 1f, num4);
				size3 = vector3;
				vector4 = new Vector2(0f, 0f);
				break;
			}
			DrawSprite(verts, uvs, cols, new DrawParam
			{
				Sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(sprite),
				Position = vector,
				Size = size3,
				Color = color,
				Rect = rect,
				Angle = angle,
				Pivot = vector4
			});
		}
	}
}
