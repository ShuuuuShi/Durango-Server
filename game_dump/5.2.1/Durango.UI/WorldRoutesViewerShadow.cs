using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class WorldRoutesViewerShadow : CustomFillSprite
{
	private struct CloudStruct
	{
		public UISpriteData Sprite;

		public Vector2 Size;

		public Vector2 Position;

		public Vector2 Vector;

		public float Alpha;

		public float CreatedAt;

		public float Duration;

		public float FadeIn;

		public float FadeOut;
	}

	public readonly string[] CloudSprites = new string[2] { "explore_map_cloud_00", "explore_map_cloud_01" };

	private UISpriteData[] _cloudSprites;

	private int _borderSize;

	private int _randomSeed;

	private global::System.Random _random;

	private Rect _widgetRect;

	private float _leftShadow;

	private float _rightShadow;

	private float _lastCloudCreatedAt;

	private bool _isInitClouds;

	private List<CloudStruct> _clouds = new List<CloudStruct>();

	private Point2 _widgetSize;

	private bool _isDirtyBackground;

	private readonly BetterList<Vector3> _bgVerts = new BetterList<Vector3>();

	private readonly BetterList<Vector2> _bgUvs = new BetterList<Vector2>();

	private readonly BetterList<Color> _bgCols = new BetterList<Color>();

	[SerializeField]
	private Color _shadowColor;

	[SerializeField]
	private Color _cloudColor;

	[SerializeField]
	private float _cloudDensity;

	[SerializeField]
	private float _cloudMakeDelay;

	[SerializeField]
	private float _cloudMinXSpeed;

	[SerializeField]
	private float _cloudMaxXSpeed;

	[SerializeField]
	private float _cloudMinYSpeed;

	[SerializeField]
	private float _cloudMaxYSpeed;

	[SerializeField]
	private float _cloudMinScale;

	[SerializeField]
	private float _cloudMaxScale;

	[SerializeField]
	private float _cloudMinAlpha;

	[SerializeField]
	private float _cloudMaxAlpha;

	[SerializeField]
	private float _cloudMinFadeIn;

	[SerializeField]
	private float _cloudMaxFadeIn;

	[SerializeField]
	private float _cloudMinFadeOut;

	[SerializeField]
	private float _cloudMaxFadeOut;

	[SerializeField]
	private float _cloudMinDuration;

	[SerializeField]
	private float _cloudMaxDuration;

	public void Initialize(int randomSeed)
	{
		_randomSeed = randomSeed;
	}

	public void Set(float leftShadow, float rightShadow)
	{
		_leftShadow = leftShadow;
		_rightShadow = rightShadow;
		_isDirtyBackground = true;
		mChanged = true;
	}

	protected override void OnStart()
	{
		base.OnStart();
		_cloudSprites = new UISpriteData[KUtility.GetSize(CloudSprites)];
		for (int i = 0; i < _cloudSprites.Length; i++)
		{
			_cloudSprites[i] = ResourceSingleton<UISpriteManager>.Instance().GetSprite(CloudSprites[i]);
		}
		UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(RoutesViewerBackground.SideSpriteSet[0]);
		_borderSize = sprite.height + sprite.paddingBottom + sprite.paddingTop;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_isInitClouds = false;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (_rightShadow > 0f)
		{
			ProcessClouds();
		}
		else if (_clouds.Count > 0)
		{
			_clouds.Clear();
			_isInitClouds = false;
			mChanged = true;
		}
	}

	private void ProcessClouds()
	{
		if (!Application.isPlaying || _cloudSprites == null)
		{
			return;
		}
		float time = Time.time;
		Vector2 vector = localSize;
		int num = Mathf.CeilToInt(vector.x * vector.y * _cloudDensity);
		if (_isInitClouds)
		{
			for (int num2 = _clouds.Count - 1; num2 >= 0; num2--)
			{
				if (_clouds[num2].CreatedAt + _clouds[num2].Duration < time)
				{
					_clouds.RemoveAt(num2);
				}
			}
			if (_clouds.Count < num && time - _lastCloudCreatedAt > _cloudMakeDelay)
			{
				MakeCloud();
			}
		}
		else
		{
			_isInitClouds = true;
			_clouds.Clear();
			for (int i = 0; i < num; i++)
			{
				MakeCloud();
			}
			for (int j = 0; j < _clouds.Count; j++)
			{
				float num3 = UnityEngine.Random.value * 0.9f;
				CloudStruct value = _clouds[j];
				value.CreatedAt -= value.Duration * num3;
				_clouds[j] = value;
			}
		}
		mChanged = true;
	}

	private void MakeCloud()
	{
		Vector3[] array = localCorners;
		CloudStruct item = default(CloudStruct);
		item.Sprite = _cloudSprites[UnityEngine.Random.Range(0, _cloudSprites.Length)];
		Vector2 vector = GetSize(item.Sprite).ToVector2();
		item.Position = new Vector2(UnityEngine.Random.Range(array[2].x - _rightShadow + vector.x, array[2].x), UnityEngine.Random.Range(array[0].y, array[2].y));
		item.Vector = new Vector2(UnityEngine.Random.Range(_cloudMinXSpeed, _cloudMaxXSpeed), UnityEngine.Random.Range(_cloudMinYSpeed, _cloudMaxYSpeed));
		item.Size = vector * UnityEngine.Random.Range(_cloudMinScale, _cloudMaxScale);
		item.Alpha = UnityEngine.Random.Range(_cloudMinAlpha, _cloudMaxAlpha);
		item.FadeIn = UnityEngine.Random.Range(_cloudMinFadeIn, _cloudMaxFadeIn);
		item.FadeOut = UnityEngine.Random.Range(_cloudMinFadeOut, _cloudMaxFadeOut);
		item.Duration = UnityEngine.Random.Range(_cloudMinDuration, _cloudMaxDuration);
		item.CreatedAt = Time.time;
		_lastCloudCreatedAt = item.CreatedAt;
		_clouds.Add(item);
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		int size = verts.size;
		if (!Application.isPlaying)
		{
			_leftShadow = (float)base.width * 0.2f;
			_rightShadow = (float)base.width * 0.3f;
		}
		Point2 point = new Point2(base.width, base.height);
		if (point != _widgetSize)
		{
			_isDirtyBackground = true;
			_widgetSize = point;
		}
		Vector3[] array = localCorners;
		_widgetRect = new Rect(array[0], array[2] - array[0]);
		DrawBackground(verts, uvs, cols);
		DrawClouds(verts, uvs, cols);
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}

	private void DrawBackground(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		if (_isDirtyBackground)
		{
			_isDirtyBackground = false;
			_random = new global::System.Random(_randomSeed);
			_bgVerts.Clear();
			_bgUvs.Clear();
			_bgCols.Clear();
			DrawCorner(_bgVerts, _bgUvs, _bgCols);
			DrawSide(_bgVerts, _bgUvs, _bgCols);
			DrawCenter(_bgVerts, _bgUvs, _bgCols);
		}
		for (int i = 0; i < _bgVerts.size; i++)
		{
			verts.Add(_bgVerts[i]);
		}
		for (int j = 0; j < _bgUvs.size; j++)
		{
			uvs.Add(_bgUvs[j]);
		}
		for (int k = 0; k < _bgCols.size; k++)
		{
			cols.Add(_bgCols[k]);
		}
	}

	private void DrawClouds(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		float time = Time.time;
		for (int i = 0; i < _clouds.Count; i++)
		{
			CloudStruct cloudStruct = _clouds[i];
			float num = time - cloudStruct.CreatedAt;
			Vector2 position = cloudStruct.Position;
			position += cloudStruct.Vector * num;
			float num2 = 1f;
			if (num < cloudStruct.FadeIn)
			{
				num2 = num / cloudStruct.FadeIn;
			}
			if (num > cloudStruct.Duration - cloudStruct.FadeOut)
			{
				num2 = Mathf.Min(num2, (cloudStruct.Duration - num) / cloudStruct.FadeOut);
			}
			num2 *= cloudStruct.Alpha;
			Color cloudColor = _cloudColor;
			cloudColor.a *= num2;
			if (num2 > 0f)
			{
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = cloudStruct.Sprite,
					Position = position,
					Size = cloudStruct.Size,
					Pivot = new Vector2(0.5f, 0.5f),
					Rect = new Rect(0f, 0f, 1f, 1f),
					Color = cloudColor
				});
			}
		}
	}

	protected void DrawCenter(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Vector3[] array = localCorners;
		Vector3 vector = array[0] + new Vector3(_borderSize, _borderSize);
		Vector2 vector2 = array[2] - array[0] - new Vector3(_borderSize, _borderSize) * 2f;
		UISpriteData sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(RoutesViewerBackground.CenterSprite);
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
				DrawBackgroundSprite(size: new Vector2(Mathf.Min(point.x, vector2.x - vector3.x), Mathf.Min(point.y, vector2.y - vector3.y)), verts: verts, uvs: uvs, cols: cols, sprite: RoutesViewerBackground.CenterSprite, pos: vector4, r: Rotate.Nothing);
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
			DrawBackgroundSprite(verts, uvs, cols, RoutesViewerBackground.CornerSprite, array[i], new Vector2(_borderSize, _borderSize), r);
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
		UISpriteData uISpriteData = ((KUtility.GetSize(RoutesViewerBackground.SideSpriteSet) != 0) ? ResourceSingleton<UISpriteManager>.Instance().GetSprite(RoutesViewerBackground.SideSpriteSet[0]) : null);
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
				int num3 = _random.Next(0, RoutesViewerBackground.SideSpriteSet.Length);
				DrawBackgroundSprite(verts, uvs, cols, RoutesViewerBackground.SideSpriteSet[num3], vector4, size, r);
				num += (float)point.x;
			}
		}
	}

	protected void DrawBackgroundSprite(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, string sprite, Vector2 pos, Vector2 size, Rotate r)
	{
		float num = _widgetRect.xMin + _leftShadow;
		float num2 = _widgetRect.xMax - _rightShadow;
		Vector2 vector = pos;
		Vector2 vector2 = pos + size;
		Color value;
		Color value2;
		if (vector.x < num)
		{
			vector2.x = Mathf.Min(vector2.x, num);
			value = Color.Lerp(_shadowColor, color.WithA(0f), (vector.x - _widgetRect.xMin) / _leftShadow);
			value2 = Color.Lerp(_shadowColor, color.WithA(0f), (vector2.x - _widgetRect.xMin) / _leftShadow);
		}
		else
		{
			if (!(vector2.x > num2))
			{
				return;
			}
			vector.x = Mathf.Max(vector.x, num2);
			value = Color.Lerp(_shadowColor, color.WithA(0f), (_widgetRect.xMax - vector.x) / _rightShadow);
			value2 = Color.Lerp(_shadowColor, color.WithA(0f), (_widgetRect.xMax - vector2.x) / _rightShadow);
		}
		Vector2 vector3 = vector2 - vector;
		if (vector3.x <= 0f || vector3.y <= 0f)
		{
			return;
		}
		float num3 = (vector.x - pos.x) / size.x;
		float num4 = vector3.x / size.x;
		float num5 = (vector.y - pos.y) / size.y;
		float num6 = vector3.y / size.y;
		float angle = 0f;
		Rect rect;
		Vector2 vector4;
		Vector2 size2;
		switch (r)
		{
		case Rotate.Radial90:
			rect = new Rect(num5, 1f - num3 - num4, num6, num4);
			angle = 90f;
			vector4 = new Vector2(0f, 1f);
			size2 = new Vector2(vector3.y, vector3.x);
			break;
		case Rotate.Radial180:
			rect = new Rect(1f - num3 - num4, 1f - num5 - num6, num4, num6);
			angle = 180f;
			vector4 = new Vector2(1f, 1f);
			size2 = vector3;
			break;
		case Rotate.Radial270:
			rect = new Rect(1f - num5 - num6, num3, num6, num4);
			angle = 270f;
			vector4 = new Vector2(1f, 0f);
			size2 = new Vector2(vector3.y, vector3.x);
			break;
		default:
			rect = new Rect(num3, num5, num4, num6);
			size2 = vector3;
			vector4 = new Vector2(0f, 0f);
			break;
		}
		int size3 = cols.size;
		DrawSprite(verts, uvs, cols, new DrawParam
		{
			Sprite = ResourceSingleton<UISpriteManager>.Instance().GetSprite(sprite),
			Position = vector,
			Size = size2,
			Rect = rect,
			Angle = angle,
			Pivot = vector4
		});
		for (int i = size3; i < cols.size; i++)
		{
			int num7 = i - size3;
			switch (r)
			{
			case Rotate.Radial90:
				if (num7 == 1 || num7 == 2)
				{
					cols[i] = value;
				}
				else
				{
					cols[i] = value2;
				}
				break;
			case Rotate.Radial180:
				if (num7 == 2 || num7 == 3)
				{
					cols[i] = value;
				}
				else
				{
					cols[i] = value2;
				}
				break;
			case Rotate.Radial270:
				if (num7 == 0 || num7 == 3)
				{
					cols[i] = value;
				}
				else
				{
					cols[i] = value2;
				}
				break;
			default:
				if (num7 == 0 || num7 == 1)
				{
					cols[i] = value;
				}
				else
				{
					cols[i] = value2;
				}
				break;
			}
		}
	}
}
