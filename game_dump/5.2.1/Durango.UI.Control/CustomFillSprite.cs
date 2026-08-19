using System;
using UnityEngine;

namespace Durango.UI.Control;

public abstract class CustomFillSprite : UISprite
{
	protected struct DrawParam
	{
		public UISpriteData Sprite;

		public Vector2 Position;

		public Vector2 Size;

		public Color Color;

		public Rect Rect;

		public Vector2 Pivot;

		public float Angle;
	}

	private Vector2[] _spriteCorners = new Vector2[4];

	protected Point2 GetSize(UISpriteData spriteData)
	{
		return new Point2(spriteData.width + spriteData.paddingLeft + spriteData.paddingRight, spriteData.height + spriteData.paddingBottom + spriteData.paddingTop);
	}

	protected void DrawSprite(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, DrawParam param)
	{
		if (param.Rect.width <= 0f || param.Rect.height <= 0f)
		{
			return;
		}
		UISpriteData sprite = param.Sprite;
		if (sprite == null)
		{
			return;
		}
		Vector2 position = param.Position;
		Vector2 size = param.Size;
		float f = param.Angle * ((float)Math.PI / 180f);
		Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
		Vector2 vector2 = new Vector2(0f - vector.y, vector.x);
		Rect rect = new Rect(sprite.x, sprite.y, sprite.width, sprite.height);
		Texture texture = mainTexture;
		rect = NGUIMath.ConvertToTexCoords(rect, texture.width, texture.height);
		Point2 size2 = GetSize(sprite);
		Rect source = new Rect(sprite.paddingLeft, sprite.paddingBottom, sprite.width, sprite.height);
		source.xMin /= size2.x;
		source.xMax /= size2.x;
		source.yMin /= size2.y;
		source.yMax /= size2.y;
		Rect rect2 = new Rect(source);
		if (param.Rect.xMax < source.xMin || param.Rect.xMin > source.xMax || param.Rect.yMax < source.yMin || param.Rect.yMin > source.yMax)
		{
			return;
		}
		Vector4 vector3 = new Vector4(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
		if (source.xMin < param.Rect.xMin)
		{
			vector3.x = rect.xMin + rect.width * (param.Rect.xMin - source.xMin) / source.width;
			rect2.xMin = 0f;
			rect2.xMax = Mathf.Lerp(param.Rect.xMin, 1f, rect2.xMax);
		}
		if (source.yMin < param.Rect.yMin)
		{
			vector3.y = rect.yMin + rect.height * (param.Rect.yMin - source.yMin) / source.height;
			rect2.yMin = 0f;
			rect2.yMax = Mathf.Lerp(param.Rect.yMin, 1f, rect2.yMax);
		}
		if (source.xMax > param.Rect.xMax)
		{
			vector3.z = rect.xMax - rect.width * (source.xMax - param.Rect.xMax) / source.width;
			rect2.xMax = 1f;
			rect2.xMin = Mathf.Lerp(0f, param.Rect.xMax, rect2.xMin);
		}
		if (source.yMax > param.Rect.yMax)
		{
			vector3.w = rect.yMax - rect.height * (source.yMax - param.Rect.yMax) / source.height;
			rect2.yMax = 1f;
			rect2.yMin = Mathf.Lerp(0f, param.Rect.yMax, rect2.yMin);
		}
		ref Vector2 reference = ref _spriteCorners[0];
		reference = position + vector * size.x * (rect2.xMin - param.Pivot.x) + vector2 * size.y * (rect2.yMin - param.Pivot.y);
		ref Vector2 reference2 = ref _spriteCorners[1];
		reference2 = position + vector * size.x * (rect2.xMin - param.Pivot.x) + vector2 * size.y * (rect2.yMax - param.Pivot.y);
		ref Vector2 reference3 = ref _spriteCorners[2];
		reference3 = position + vector * size.x * (rect2.xMax - param.Pivot.x) + vector2 * size.y * (rect2.yMax - param.Pivot.y);
		ref Vector2 reference4 = ref _spriteCorners[3];
		reference4 = position + vector * size.x * (rect2.xMax - param.Pivot.x) + vector2 * size.y * (rect2.yMin - param.Pivot.y);
		for (int i = 0; i < 4; i++)
		{
			verts.Add(_spriteCorners[i]);
		}
		uvs.Add(new Vector2(vector3.x, vector3.y));
		uvs.Add(new Vector2(vector3.x, vector3.w));
		uvs.Add(new Vector2(vector3.z, vector3.w));
		uvs.Add(new Vector2(vector3.z, vector3.y));
		Color color = param.Color * this.color;
		color.a = param.Color.a * finalAlpha;
		if (base.applyGradient)
		{
			cols.Add(base.gradientTop);
			cols.Add(base.gradientBottom);
			cols.Add(base.gradientBottom);
			cols.Add(base.gradientTop);
		}
		else
		{
			for (int j = 0; j < 4; j++)
			{
				cols.Add(param.Color);
			}
		}
	}

	protected void DrawSprite(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, UISpriteData sprite, Vector3[] corners, Color col)
	{
		DrawSprite(verts, uvs, cols, sprite, corners, col, new Rect(0f, 0f, 1f, 1f));
	}

	protected void DrawSprite(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, UISpriteData sprite, Vector3[] corners, Color col, Rect r)
	{
		if (sprite == null)
		{
			return;
		}
		Rect rect = new Rect(sprite.x, sprite.y, sprite.width, sprite.height);
		Texture texture = mainTexture;
		rect = NGUIMath.ConvertToTexCoords(rect, texture.width, texture.height);
		Point2 size = GetSize(sprite);
		Rect source = new Rect(sprite.paddingLeft, sprite.paddingBottom, sprite.width, sprite.height);
		source.xMin /= size.x;
		source.xMax /= size.x;
		source.yMin /= size.y;
		source.yMax /= size.y;
		if (!(r.xMax < source.xMin) && !(r.xMin > source.xMax) && !(r.yMax < source.yMin) && !(r.yMin > source.yMax))
		{
			Rect rect2 = new Rect(source);
			Vector4 vector = new Vector4(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
			if (source.xMin < r.xMin)
			{
				vector.x = rect.xMin + rect.width * (r.xMin - source.xMin) / source.width;
				rect2.xMin = 0f;
				rect2.xMax = Mathf.Lerp(r.xMin, 1f, rect2.xMax);
			}
			if (source.yMin < r.yMin)
			{
				vector.y = rect.yMin + rect.height * (r.yMin - source.yMin) / source.height;
				rect2.yMin = 0f;
				rect2.yMax = Mathf.Lerp(r.yMin, 1f, rect2.yMax);
			}
			if (source.xMax > r.xMax)
			{
				vector.z = rect.xMax - rect.width * (source.xMax - r.xMax) / source.width;
				rect2.xMax = 1f;
				rect2.xMin = Mathf.Lerp(0f, r.xMax, rect2.xMin);
			}
			if (source.yMax > r.yMax)
			{
				vector.w = rect.yMax - rect.height * (source.yMax - r.yMax) / source.height;
				rect2.yMax = 1f;
				rect2.yMin = Mathf.Lerp(0f, r.yMax, rect2.yMin);
			}
			ref Vector2 reference = ref _spriteCorners[0];
			reference = Vector2.Lerp(Vector2.Lerp(corners[0], corners[3], rect2.xMin), Vector2.Lerp(corners[1], corners[2], rect2.xMin), rect2.yMin);
			ref Vector2 reference2 = ref _spriteCorners[1];
			reference2 = Vector2.Lerp(Vector2.Lerp(corners[0], corners[3], rect2.xMin), Vector2.Lerp(corners[1], corners[2], rect2.xMin), rect2.yMax);
			ref Vector2 reference3 = ref _spriteCorners[2];
			reference3 = Vector2.Lerp(Vector2.Lerp(corners[0], corners[3], rect2.xMax), Vector2.Lerp(corners[1], corners[2], rect2.xMax), rect2.yMax);
			ref Vector2 reference4 = ref _spriteCorners[3];
			reference4 = Vector2.Lerp(Vector2.Lerp(corners[0], corners[3], rect2.xMax), Vector2.Lerp(corners[1], corners[2], rect2.xMax), rect2.yMin);
			for (int i = 0; i < 4; i++)
			{
				verts.Add(_spriteCorners[i]);
			}
			uvs.Add(new Vector2(vector.x, vector.y));
			uvs.Add(new Vector2(vector.x, vector.w));
			uvs.Add(new Vector2(vector.z, vector.w));
			uvs.Add(new Vector2(vector.z, vector.y));
			Color item = col * color;
			item.a = col.a * finalAlpha;
			for (int j = 0; j < 4; j++)
			{
				cols.Add(item);
			}
		}
	}
}
