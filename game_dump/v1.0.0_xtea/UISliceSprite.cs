using System.Collections.Generic;
using UnityEngine;

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
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			Vector2 normalized = ((Vector2)(ref v)).normalized;
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
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
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
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if (CrossPoint(line1, line2, out var point))
			{
				float num = line2.Angle - line1.Angle;
				float num2 = Mathf.Sign(num);
				if (Mathf.Abs(num) > 180f)
				{
					num2 = 0f - num2;
				}
				line1.AddDot(point, num2);
				line2.AddDot(point, 0f - num2);
			}
		}

		private void AddDot(Vector2 p, float sign)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = p - Dot;
			float num = ((B != 0f) ? (val.x / B) : (val.y / (0f - A)));
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
				return (int)(((Vector2)(ref x.Dot)).sqrMagnitude - ((Vector2)(ref y.Dot)).sqrMagnitude);
			}
			return num;
		}
	}

	public class Vector2Comparer : IComparer<Vector2>
	{
		public int Compare(Vector2 v1, Vector2 v2)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
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
			if (!((Object)(object)_target == (Object)(object)value))
			{
				_target = value;
				if ((Object)(object)_target != (Object)null)
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
		if ((Object)(object)_target != (Object)null && (_isDirty || forceRefresh))
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
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
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

	private void OnFillSprite(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		Color color = widget.color;
		if (_dots.Count > 0)
		{
			_vert.Left = KMathUtil.Min(verts[0].x, verts[1].x, verts[2].x, verts[3].x);
			_vert.Right = KMathUtil.Max(verts[0].x, verts[1].x, verts[2].x, verts[3].x);
			_vert.Bottom = KMathUtil.Min(verts[0].y, verts[1].y, verts[2].y, verts[3].y);
			_vert.Top = KMathUtil.Max(verts[0].y, verts[1].y, verts[2].y, verts[3].y);
			_uvs.Left = KMathUtil.Min(uvs[0].x, uvs[1].x, uvs[2].x, uvs[3].x);
			_uvs.Right = KMathUtil.Max(uvs[0].x, uvs[1].x, uvs[2].x, uvs[3].x);
			_uvs.Bottom = KMathUtil.Min(uvs[0].y, uvs[1].y, uvs[2].y, uvs[3].y);
			_uvs.Top = KMathUtil.Max(uvs[0].y, uvs[1].y, uvs[2].y, uvs[3].y);
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
					Vector2 val = Vector2.zero;
					switch (j)
					{
					case 1:
						val = _dots[(i + 1) % count];
						break;
					case 2:
						val = _dots[i];
						break;
					}
					verts.Add(Vector2.op_Implicit(val + _vert.Center));
					uvs.Add(new Vector2(val.x / num2 * num3, val.y / num * num4) + _uvs.Center);
					cols.Add(color);
				}
			}
		}
		if (OnPostFill != null)
		{
			OnPostFill(widget, bufferOffset, verts, uvs, cols);
		}
	}
}
