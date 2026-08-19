using UnityEngine;

namespace Durango.UI.Control;

public class Emphasis : EffectWidget
{
	[SerializeField]
	private SpriteData _glitter;

	[SerializeField]
	private float _glitterDurationRatio;

	[SerializeField]
	private SpriteData _border;

	[SerializeField]
	private float _borderDurationRatio;

	[SerializeField]
	private AnimationCurve _borderAlpha;

	[SerializeField]
	private float _borderLeft;

	[SerializeField]
	private float _borderRight;

	[SerializeField]
	private float _borderBottom;

	[SerializeField]
	private float _borderTop;

	[SerializeField]
	private SpriteData _background;

	[SerializeField]
	private float _backgroundDurationRatio;

	[SerializeField]
	private AnimationCurve _backgroundAlpha;

	[SerializeField]
	private float _bgLeft;

	[SerializeField]
	private float _bgRight;

	[SerializeField]
	private float _bgBottom;

	[SerializeField]
	private float _bgTop;

	private static Vector2[] _tempPos = new Vector2[4];

	private static Vector2[] _tempUVs = new Vector2[4];

	private bool _isInitialize;

	private Material _material;

	private UISpriteData _glitterSprite;

	private UISpriteData _borderSprite;

	private UISpriteData _backgroundSprite;

	public override Material material
	{
		get
		{
			Initialize();
			return _material;
		}
	}

	private void Initialize()
	{
		if (!_isInitialize)
		{
			_isInitialize = true;
			_material = null;
			_glitterSprite = null;
			if (ResourceSingleton<UISpriteManager>.Instance().TryGet(_glitter.sprite, out var atlas, out var spriteData))
			{
				_material = atlas.spriteMaterial;
				_glitterSprite = spriteData;
			}
			if (ResourceSingleton<UISpriteManager>.Instance().TryGet(_border.sprite, out atlas, out spriteData))
			{
				_borderSprite = spriteData;
			}
			if (ResourceSingleton<UISpriteManager>.Instance().TryGet(_background.sprite, out atlas, out spriteData))
			{
				_backgroundSprite = spriteData;
			}
		}
	}

	protected override void Sample(float ratio, UIGeometry.Arguments arguments)
	{
		Initialize();
		DrawBackground((!(_backgroundDurationRatio > 0f)) ? ratio : Mathf.Clamp01(ratio / _backgroundDurationRatio), arguments);
		DrawGlitter((!(_glitterDurationRatio > 0f)) ? ratio : Mathf.Clamp01(ratio / _glitterDurationRatio), arguments);
		DrawBorder((!(_borderDurationRatio > 0f)) ? ratio : Mathf.Clamp01(ratio / _borderDurationRatio), arguments);
	}

	private void DrawGlitter(float ratio, UIGeometry.Arguments arguments)
	{
		UISpriteData glitterSprite = _glitterSprite;
		if (glitterSprite == null)
		{
			return;
		}
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		Rect rect = new Rect(localCorners[0], localSize);
		Rect rect2 = new Rect(glitterSprite.x, glitterSprite.y, glitterSprite.width, glitterSprite.height);
		Texture texture = _material.mainTexture;
		rect2 = NGUIMath.ConvertToTexCoords(rect2, texture.width, texture.height);
		Rect rect3 = new Rect(new Vector2(rect.xMin - (float)glitterSprite.width + (rect.width + (float)glitterSprite.width) * ratio, rect.yMin), new Vector2(glitterSprite.width, rect.height));
		if (rect3.xMax > rect.xMin && rect3.xMin < rect.xMax)
		{
			if (rect3.xMin < rect.xMin)
			{
				rect2.xMin += rect2.width * ((rect.xMin - rect3.xMin) / rect3.width);
				rect3.xMin = rect.xMin;
			}
			if (rect3.xMax > rect.xMax)
			{
				rect2.xMax -= rect2.width * ((rect3.xMax - rect.xMax) / rect3.width);
				rect3.xMax = rect.xMax;
			}
			Color item = color * _glitter.color;
			verts.Add(new Vector3(rect3.xMin, rect3.yMin));
			uvs.Add(new Vector2(rect2.xMin, rect2.yMin));
			cols.Add(item);
			verts.Add(new Vector3(rect3.xMin, rect3.yMax));
			uvs.Add(new Vector2(rect2.xMin, rect2.yMax));
			cols.Add(item);
			verts.Add(new Vector3(rect3.xMax, rect3.yMax));
			uvs.Add(new Vector2(rect2.xMax, rect2.yMax));
			cols.Add(item);
			verts.Add(new Vector3(rect3.xMax, rect3.yMin));
			uvs.Add(new Vector2(rect2.xMax, rect2.yMin));
			cols.Add(item);
		}
	}

	private void DrawBorder(float ratio, UIGeometry.Arguments arguments)
	{
		UISpriteData borderSprite = _borderSprite;
		if (borderSprite == null)
		{
			return;
		}
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		Rect rect = new Rect(localCorners[0] - new Vector3(_borderLeft, _borderBottom), localSize + new Vector2(_borderLeft + _borderRight, _borderBottom + _borderTop));
		Color item = color * _border.color;
		item.a *= ((_borderAlpha != null) ? _borderAlpha.Evaluate(ratio) : 0f);
		Rect rect2 = new Rect(borderSprite.x, borderSprite.y, borderSprite.width, borderSprite.height);
		Vector4 vector = new Vector4(borderSprite.borderLeft, borderSprite.borderBottom, borderSprite.borderRight, borderSprite.borderTop);
		vector.x = Mathf.Max(0, borderSprite.borderLeft);
		vector.y = Mathf.Max(0, borderSprite.borderBottom);
		vector.z = Mathf.Max(0, borderSprite.borderRight);
		vector.w = Mathf.Max(0, borderSprite.borderTop);
		Rect rect3 = new Rect(rect2.x + vector.x, rect2.y + vector.w, rect2.width - vector.x - vector.z, rect2.height - vector.y - vector.w);
		Texture texture = _material.mainTexture;
		rect2 = NGUIMath.ConvertToTexCoords(rect2, texture.width, texture.height);
		rect3 = NGUIMath.ConvertToTexCoords(rect3, texture.width, texture.height);
		_tempPos[0].x = rect.xMin;
		_tempPos[0].y = rect.yMin;
		_tempPos[3].x = rect.xMax;
		_tempPos[3].y = rect.yMax;
		_tempPos[1].x = _tempPos[0].x + (float)borderSprite.borderLeft;
		_tempPos[2].x = _tempPos[3].x - (float)borderSprite.borderRight;
		_tempUVs[0].x = rect2.xMin;
		_tempUVs[1].x = rect3.xMin;
		_tempUVs[2].x = rect3.xMax;
		_tempUVs[3].x = rect2.xMax;
		_tempPos[1].y = _tempPos[0].y + (float)borderSprite.borderBottom;
		_tempPos[2].y = _tempPos[3].y - (float)borderSprite.borderTop;
		_tempUVs[0].y = rect2.yMin;
		_tempUVs[1].y = rect3.yMin;
		_tempUVs[2].y = rect3.yMax;
		_tempUVs[3].y = rect2.yMax;
		for (int i = 0; i < 3; i++)
		{
			int num = i + 1;
			for (int j = 0; j < 3; j++)
			{
				if (i != 1 || j != 1)
				{
					int num2 = j + 1;
					verts.Add(new Vector3(_tempPos[i].x, _tempPos[j].y));
					verts.Add(new Vector3(_tempPos[i].x, _tempPos[num2].y));
					verts.Add(new Vector3(_tempPos[num].x, _tempPos[num2].y));
					verts.Add(new Vector3(_tempPos[num].x, _tempPos[j].y));
					uvs.Add(new Vector2(_tempUVs[i].x, _tempUVs[j].y));
					uvs.Add(new Vector2(_tempUVs[i].x, _tempUVs[num2].y));
					uvs.Add(new Vector2(_tempUVs[num].x, _tempUVs[num2].y));
					uvs.Add(new Vector2(_tempUVs[num].x, _tempUVs[j].y));
					cols.Add(item);
					cols.Add(item);
					cols.Add(item);
					cols.Add(item);
				}
			}
		}
	}

	private void DrawBackground(float ratio, UIGeometry.Arguments arguments)
	{
		UISpriteData backgroundSprite = _backgroundSprite;
		if (backgroundSprite != null)
		{
			BetterList<Vector3> verts = arguments.verts;
			BetterList<Vector2> uvs = arguments.uvs;
			BetterList<Color> cols = arguments.cols;
			Rect rect = new Rect(localCorners[0] - new Vector3(_bgLeft, _bgBottom), localSize + new Vector2(_bgLeft + _bgRight, _bgBottom + _bgTop));
			Color item = color * _background.color;
			item.a *= ((_backgroundAlpha != null) ? _backgroundAlpha.Evaluate(ratio) : 0f);
			Rect rect2 = new Rect(backgroundSprite.x, backgroundSprite.y, backgroundSprite.width, backgroundSprite.height);
			Texture texture = _material.mainTexture;
			rect2 = NGUIMath.ConvertToTexCoords(rect2, texture.width, texture.height);
			verts.Add(new Vector3(rect.xMin, rect.yMin));
			uvs.Add(new Vector2(rect2.xMin, rect2.yMin));
			cols.Add(item);
			verts.Add(new Vector3(rect.xMin, rect.yMax));
			uvs.Add(new Vector2(rect2.xMin, rect2.yMax));
			cols.Add(item);
			verts.Add(new Vector3(rect.xMax, rect.yMax));
			uvs.Add(new Vector2(rect2.xMax, rect2.yMax));
			cols.Add(item);
			verts.Add(new Vector3(rect.xMax, rect.yMin));
			uvs.Add(new Vector2(rect2.xMax, rect2.yMin));
			cols.Add(item);
		}
	}
}
