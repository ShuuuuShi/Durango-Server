using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public class PetMilestoneRollTermTexture : UITexture
{
	private readonly List<Vector3> _vectors = new List<Vector3>();

	private readonly float[] _values = new float[4];

	private readonly Color[] _colors = new Color[2];

	private readonly Vector3[] _spriteVerts = new Vector3[4];

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		int count = _vectors.Count;
		if (count == 0)
		{
			return;
		}
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		int size = verts.size;
		float size2 = (float)Mathf.Min(base.width, base.height) * 0.5f;
		for (int i = 0; i < count - 1; i++)
		{
			int index = i;
			int index2 = i + 1;
			float num = Mathf.Clamp01((float)i / (float)count);
			float num2 = Mathf.Clamp01((float)(i + 1) / (float)count);
			if (_values[0] < _values[1])
			{
				ref Vector3 reference = ref _spriteVerts[0];
				reference = _vectors[index] * GetSizeByArgument(size2, _values[0]);
				ref Vector3 reference2 = ref _spriteVerts[1];
				reference2 = _vectors[index] * GetSizeByArgument(size2, _values[1]);
				ref Vector3 reference3 = ref _spriteVerts[2];
				reference3 = _vectors[index2] * GetSizeByArgument(size2, _values[1]);
				ref Vector3 reference4 = ref _spriteVerts[3];
				reference4 = _vectors[index2] * GetSizeByArgument(size2, _values[0]);
				FillTexture(verts, uvs, cols, _spriteVerts, _colors[0], new Rect(num, 0f, num2 - num, 1f));
			}
			if (_values[2] < _values[3])
			{
				ref Vector3 reference5 = ref _spriteVerts[0];
				reference5 = _vectors[index] * GetSizeByArgument(size2, _values[2]);
				ref Vector3 reference6 = ref _spriteVerts[1];
				reference6 = _vectors[index] * GetSizeByArgument(size2, _values[3]);
				ref Vector3 reference7 = ref _spriteVerts[2];
				reference7 = _vectors[index2] * GetSizeByArgument(size2, _values[3]);
				ref Vector3 reference8 = ref _spriteVerts[3];
				reference8 = _vectors[index2] * GetSizeByArgument(size2, _values[2]);
				FillTexture(verts, uvs, cols, _spriteVerts, _colors[1], new Rect(num, 0f, num2 - num, 1f));
			}
		}
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}

	private void FillTexture(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Vector3[] corners, Color col, Rect rect)
	{
		for (int i = 0; i < 4; i++)
		{
			verts.Add(corners[i]);
		}
		uvs.Add(new Vector2(rect.xMin, rect.yMin));
		uvs.Add(new Vector2(rect.xMin, rect.yMax));
		uvs.Add(new Vector2(rect.xMax, rect.yMax));
		uvs.Add(new Vector2(rect.xMax, rect.yMin));
		Color item = col * color;
		item.a = col.a * finalAlpha;
		for (int j = 0; j < 4; j++)
		{
			cols.Add(item);
		}
	}

	public void DrawArc(float angleStart, float angleEnd, float v1, float v2, Color c1, float v3, float v4, Color c2)
	{
		CalcArcVector(angleStart, angleEnd, v1, v2, c1, v3, v4, c2);
		MarkAsChanged();
	}

	private void CalcArcVector(float angleStart, float angleEnd, float v1, float v2, Color c1, float v3, float v4, Color c2)
	{
		_vectors.Clear();
		_values[0] = v1;
		_values[1] = v2;
		_values[2] = v3;
		_values[3] = v4;
		_colors[0] = c1;
		_colors[1] = c2;
		while (angleStart > angleEnd)
		{
			angleEnd += 360f;
		}
		int num = Mathf.Max(1, (int)((angleEnd - angleStart) / 4f));
		for (int i = 0; i <= num; i++)
		{
			float t = (float)i / (float)num;
			float f = Mathf.Lerp(angleStart, angleEnd, t) * ((float)Math.PI / 180f);
			Vector3 item = new Vector3(Mathf.Cos(f), Mathf.Sin(f));
			_vectors.Add(item);
		}
	}

	private float GetSizeByArgument(float size, float argument)
	{
		if (argument > 0f)
		{
			if (argument > 1f)
			{
				return argument;
			}
			return size * argument;
		}
		return size + argument;
	}
}
