using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Control;

public class UISliceSprite : MonoBehaviour
{
	private struct Rect
	{
		public float Left;

		public float Right;

		public float Bottom;

		public float Top;

		public Vector2 Center => new Vector2((Left + Right) / 2f, (Bottom + Top) / 2f);

		public float Width => Mathf.Abs(Right - Left);

		public float Height => Mathf.Abs(Top - Bottom);
	}

	public struct SliceInfo
	{
		public Vector2 Dot;

		public float A;

		public float B;

		public float C;

		public float Angle;

		public Vector2 CrossDot1;

		public Vector2 CrossDot2;

		public float DotLen1;

		public float DotLen2;

		public SliceInfo(Vector2 v)
		{
			Vector2 normalized = v.normalized;
			float y = normalized.y;
			float num = 0f - normalized.x;
			A = 0f - num;
			B = y;
			if (y == 0f)
			{
				C = (0f - v.x) * num;
			}
			else
			{
				C = (v.y - num * v.x / y) * y;
			}
			Dot = v;
			Angle = Mathf.Atan2(v.y, v.x) * 57.29578f;
			CrossDot1.x = float.NaN;
			CrossDot1.y = float.NaN;
			CrossDot2.x = float.NaN;
			CrossDot2.y = float.NaN;
			DotLen1 = float.MinValue;
			DotLen2 = float.MaxValue;
		}

		public static bool CrossPoint(SliceInfo s1, SliceInfo s2, out Vector2 point)
		{
			float num = s1.A * s2.B - s1.B * s2.A;
			if (num == 0f)
			{
				point = default(Vector2);
				return false;
			}
			point.x = (s1.C * s2.B - s1.B * s2.C) / num;
			point.y = (s1.C * s2.A - s1.A * s2.C) / (0f - num);
			return true;
		}

		public static void Calc(ref SliceInfo line1, ref SliceInfo line2)
		{
			if (CrossPoint(line1, line2, out var point))
			{
				float f = line2.Angle - line1.Angle;
				float num = Mathf.Sign(f);
				if (Mathf.Abs(f) > 180f)
				{
					num = 0f - num;
				}
				line1.AddDot(point, num);
				line2.AddDot(point, 0f - num);
			}
		}

		private void AddDot(Vector2 p, float sign)
		{
			Vector2 vector = p - Dot;
			float num = ((B != 0f) ? (vector.x / B) : (vector.y / (0f - A)));
			if (sign > 0f)
			{
				if (DotLen1 < num)
				{
					DotLen1 = num;
					CrossDot1 = p;
				}
			}
			else if (DotLen2 > num)
			{
				DotLen2 = num;
				CrossDot2 = p;
			}
		}
	}

	public class SliceInfoComparer : IComparer<SliceInfo>
	{
		public int Compare(SliceInfo x, SliceInfo y)
		{
			int num = (int)(x.Angle - y.Angle);
			if (num == 0)
			{
				return (int)(x.Dot.sqrMagnitude - y.Dot.sqrMagnitude);
			}
			return num;
		}
	}

	public class Vector2Comparer : IComparer<Vector2>
	{
		public int Compare(Vector2 v1, Vector2 v2)
		{
			if (float.IsNaN(v1.x) || float.IsNaN(v1.y) || float.IsNaN(v2.x) || float.IsNaN(v2.y))
			{
				return 0;
			}
			int quadrant = GetQuadrant(v1);
			int quadrant2 = GetQuadrant(v2);
			if (quadrant == quadrant2)
			{
				float num = Mathf.Abs(v1.x / v1.y);
				float num2 = Mathf.Abs(v2.x / v2.y);
				if (num == num2)
				{
					return 0;
				}
				int num3 = ((!(num > num2)) ? 1 : (-1));
				return quadrant switch
				{
					0 => num3, 
					1 => -num3, 
					2 => num3, 
					3 => -num3, 
					_ => num3, 
				};
			}
			return quadrant - quadrant2;
		}

		public static int GetQuadrant(Vector2 vec)
		{
			if (vec.y >= 0f)
			{
				if (vec.x >= 0f)
				{
					return 0;
				}
				return 1;
			}
			if (vec.x >= 0f)
			{
				return 3;
			}
			return 2;
		}
	}

	public UIWidget.OnPostFillCallback OnPostFill;

	private UISprite _target;

	private bool _isDirty;

	private readonly List<SliceInfo> _sliceInfos = new List<SliceInfo>();

	private readonly List<Vector2> _dots = new List<Vector2>();

	private readonly List<SliceInfo> _slices = new List<SliceInfo>();

	private readonly List<SliceInfo> _tmpList = new List<SliceInfo>();

	private Rect _vert;

	private Rect _uvs;

	private readonly SliceInfoComparer _sliceInfoComparer = new SliceInfoComparer();

	private readonly Vector2Comparer _vector2Comparer = new Vector2Comparer();

	public UISprite Target
	{
		get
		{
			return _target;
		}
		set
		{
			if (!(_target == value))
			{
				_target = value;
				if (_target != null)
				{
					_target.type = UIBasicSprite.Type.Simple;
					_target.pivot = UIWidget.Pivot.Center;
					_target.onPostFill = OnFillSprite;
				}
			}
		}
	}

	public List<SliceInfo> SliceInfos => _sliceInfos;

	public void Refresh(bool forceRefresh = true)
	{
		if (_target != null && (_isDirty || forceRefresh))
		{
			_isDirty = false;
			if (UpdateDots())
			{
				_target.MarkAsChanged();
			}
		}
	}

	public bool AddSlice(Vector2 dot)
	{
		int num = Target.width / 2;
		int num2 = Target.height / 2;
		if (Mathf.Abs(dot.x) > (float)num || Mathf.Abs(dot.y) > (float)num2)
		{
			return false;
		}
		if (dot == Vector2.zero)
		{
			return false;
		}
		if (HasSlice(dot))
		{
			return false;
		}
		_sliceInfos.Add(new SliceInfo(dot));
		_isDirty = true;
		return true;
	}

	public bool RemoveSlice(Vector2 dot)
	{
		for (int i = 0; i < _sliceInfos.Count; i++)
		{
			if (_sliceInfos[i].Dot == dot)
			{
				_sliceInfos.RemoveAt(i);
				_isDirty = true;
				return true;
			}
		}
		return false;
	}

	public void ClearSlices()
	{
		if (_sliceInfos.Count > 0)
		{
			_sliceInfos.Clear();
			Refresh();
		}
	}

	public bool HasSlice(Vector2 dot)
	{
		for (int i = 0; i < _sliceInfos.Count; i++)
		{
			if (_sliceInfos[i].Dot == dot)
			{
				return true;
			}
		}
		return false;
	}

	private bool UpdateDots()
	{
		_tmpList.Clear();
		for (int i = 0; i < _sliceInfos.Count; i++)
		{
			_tmpList.Add(_sliceInfos[i]);
		}
		UISpriteData atlasSprite = Target.GetAtlasSprite();
		float num = (float)atlasSprite.width * 0.5f * (float)Target.width / (float)(atlasSprite.width + atlasSprite.paddingLeft + atlasSprite.paddingRight);
		float num2 = (float)atlasSprite.height * 0.5f * (float)Target.height / (float)(atlasSprite.height + atlasSprite.paddingTop + atlasSprite.paddingBottom);
		_tmpList.Add(new SliceInfo(new Vector2(num, 0f)));
		_tmpList.Add(new SliceInfo(new Vector2(0f, num2)));
		_tmpList.Add(new SliceInfo(new Vector2(0f - num, 0f)));
		_tmpList.Add(new SliceInfo(new Vector2(0f, 0f - num2)));
		_tmpList.Sort(_sliceInfoComparer);
		for (int num3 = _tmpList.Count - 1; num3 > 0; num3--)
		{
			if (_tmpList[num3].Angle == _tmpList[num3 - 1].Angle)
			{
				_tmpList.RemoveAt(num3);
			}
		}
		bool flag = false;
		if (_tmpList.Count == _slices.Count)
		{
			int j = 0;
			for (int count = _tmpList.Count; j < count; j++)
			{
				if (_tmpList[j].Dot != _slices[j].Dot)
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			return false;
		}
		_slices.Clear();
		for (int k = 0; k < _tmpList.Count; k++)
		{
			_slices.Add(_tmpList[k]);
		}
		_dots.Clear();
		int count2 = _slices.Count;
		for (int l = 0; l < count2; l++)
		{
			for (int m = l + 1; m < count2; m++)
			{
				SliceInfo line = _slices[l];
				SliceInfo line2 = _slices[m];
				SliceInfo.Calc(ref line, ref line2);
				_slices[l] = line;
				_slices[m] = line2;
			}
		}
		for (int n = 0; n < count2; n++)
		{
			if (_slices[n].DotLen1 < _slices[n].DotLen2)
			{
				_dots.Add(_slices[n].CrossDot1);
				_dots.Add(_slices[n].CrossDot2);
			}
		}
		_dots.Sort(_vector2Comparer);
		for (int num4 = _dots.Count - 1; num4 > 0; num4--)
		{
			if (_dots[num4] == _dots[num4 - 1])
			{
				_dots.RemoveAt(num4);
			}
		}
		return true;
	}

	private void OnFillSprite(UIWidget widget, int bufferOffset, UIGeometry.Arguments arguments)
	{
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		Color color = widget.color;
		if (_dots.Count > 0)
		{
			_vert.Left = Maths.Min(verts[0].x, verts[1].x, verts[2].x, verts[3].x);
			_vert.Right = Maths.Max(verts[0].x, verts[1].x, verts[2].x, verts[3].x);
			_vert.Bottom = Maths.Min(verts[0].y, verts[1].y, verts[2].y, verts[3].y);
			_vert.Top = Maths.Max(verts[0].y, verts[1].y, verts[2].y, verts[3].y);
			_uvs.Left = Maths.Min(uvs[0].x, uvs[1].x, uvs[2].x, uvs[3].x);
			_uvs.Right = Maths.Max(uvs[0].x, uvs[1].x, uvs[2].x, uvs[3].x);
			_uvs.Bottom = Maths.Min(uvs[0].y, uvs[1].y, uvs[2].y, uvs[3].y);
			_uvs.Top = Maths.Max(uvs[0].y, uvs[1].y, uvs[2].y, uvs[3].y);
			verts.Clear();
			uvs.Clear();
			cols.Clear();
			float num = _vert.Height / 2f;
			float num2 = _vert.Width / 2f;
			float num3 = _uvs.Width / 2f;
			float num4 = _uvs.Height / 2f;
			int i = 0;
			for (int count = _dots.Count; i < count; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					Vector2 vector = Vector2.zero;
					switch (j)
					{
					case 1:
						vector = _dots[(i + 1) % count];
						break;
					case 2:
						vector = _dots[i];
						break;
					}
					verts.Add(vector + _vert.Center);
					uvs.Add(new Vector2(vector.x / num2 * num3, vector.y / num * num4) + _uvs.Center);
					cols.Add(color);
				}
			}
		}
		if (OnPostFill != null)
		{
			OnPostFill(widget, bufferOffset, arguments);
		}
	}
}
