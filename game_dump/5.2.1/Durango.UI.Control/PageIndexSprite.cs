using System;
using UnityEngine;

namespace Durango.UI.Control;

public class PageIndexSprite : CustomFillSprite
{
	[Serializable]
	private struct ViewPram
	{
		public Color Color;

		public float Scale;
	}

	[SerializeField]
	private float _pivot = 0.5f;

	[SerializeField]
	private Vector2 _direction = new Vector2(1f, 0f);

	[SerializeField]
	private float _margin = 20f;

	[SerializeField]
	private ViewPram _selected = new ViewPram
	{
		Color = Color.white,
		Scale = 1f
	};

	[SerializeField]
	private ViewPram _normal = new ViewPram
	{
		Color = new Color(1f, 1f, 1f, 0.5f),
		Scale = 1f
	};

	private int _pageCount;

	private float _index;

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		UISpriteData atlasSprite = GetAtlasSprite();
		if (atlasSprite == null)
		{
			return;
		}
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		int size = verts.size;
		int num = _pageCount;
		float num2 = _index;
		if (!Application.isPlaying)
		{
			num = 3;
			num2 = 0.9f;
		}
		Vector3 vector = base.localCenter;
		Vector3 vector2 = _direction;
		vector -= vector2 * _margin * (num - 1) * _pivot;
		Vector2 vector3 = localSize;
		int num3 = Mathf.FloorToInt(num2);
		for (int i = 0; i < num; i++)
		{
			float t = 0f;
			if (i == num3)
			{
				t = 1f - (num2 - (float)i);
			}
			else if (i - 1 == num3)
			{
				t = 1f - ((float)i - num2);
			}
			Color color = Color.Lerp(_normal.Color, _selected.Color, t);
			float num4 = Mathf.Lerp(_normal.Scale, _selected.Scale, t);
			DrawSprite(verts, uvs, cols, new DrawParam
			{
				Sprite = atlasSprite,
				Pivot = new Vector2(0.5f, 0.5f),
				Position = vector + i * vector2 * _margin,
				Rect = new Rect(0f, 0f, 1f, 1f),
				Size = vector3 * num4,
				Color = color
			});
		}
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}

	public void Set(float index)
	{
		if (_index != index)
		{
			_index = index;
			mChanged = true;
		}
	}

	public void Make(int count)
	{
		if (_pageCount != count)
		{
			_pageCount = count;
			mChanged = true;
		}
	}
}
