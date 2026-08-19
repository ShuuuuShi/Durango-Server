using System;
using System.Collections.Generic;
using UnityEngine;

public class UICircleFill : MonoBehaviour
{
	public struct Rect
	{
		public float left;

		public float right;

		public float bottom;

		public float top;

		public Vector2 Center => new Vector2((left + right) / 2f, (bottom + top) / 2f);

		public float Width => Mathf.Abs(right - left);

		public float Height => Mathf.Abs(top - bottom);

		public Rect(float left, float right, float bottom, float top)
		{
			this.left = left;
			this.right = right;
			this.bottom = bottom;
			this.top = top;
		}

		public Vector2 DegreeToPos(float degree)
		{
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			float num = degree * ((float)Math.PI / 180f);
			Vector2 result = default(Vector2);
			result.x = Mathf.Sin(num);
			result.y = Mathf.Cos(num);
			float num2 = Mathf.Abs(result.x) / (Width / 2f);
			float num3 = Mathf.Abs(result.y) / (Height / 2f);
			if (num2 > num3)
			{
				result.x /= num2;
				result.y /= num2;
			}
			else
			{
				result.x /= num3;
				result.y /= num3;
			}
			return result;
		}
	}

	[SerializeField]
	private UISprite _target;

	[SerializeField]
	private List<KeyValuePair<int, bool>> _hideDegreeList = new List<KeyValuePair<int, bool>>();

	private bool _hideDegreeListneedRefresh;

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
				}
			}
		}
	}

	public int HideDegreeRangeCount
	{
		get
		{
			if (_hideDegreeListneedRefresh)
			{
				_hideDegreeListneedRefresh = false;
				CalcHideDegreeList();
			}
			return _hideDegreeList.Count / 2;
		}
	}

	private void OnEnable()
	{
		if ((Object)(object)_target != (Object)null)
		{
			((Behaviour)_target).enabled = true;
			RefreshSprite();
		}
	}

	public void RefreshSprite()
	{
		if ((Object)(object)_target != (Object)null)
		{
			_target.onPostFill = OnFillSprite;
			_target.MarkAsChanged();
		}
	}

	public void AddHideRange(int start, int end)
	{
		if (AddHideRange(start, end, _hideDegreeList))
		{
			_hideDegreeListneedRefresh = true;
			Target.MarkAsChanged();
		}
	}

	public static bool AddHideRange(int start, int end, List<KeyValuePair<int, bool>> hideList)
	{
		while (start > 360)
		{
			start -= 360;
		}
		while (start < 0)
		{
			start += 360;
		}
		while (end > 360)
		{
			end -= 360;
		}
		while (end < 0)
		{
			end += 360;
		}
		if (start == end)
		{
			return false;
		}
		bool flag = true;
		bool flag2 = true;
		int i = 0;
		for (int count = hideList.Count; i < count; i++)
		{
			if (hideList[i].Key == start && hideList[i].Value)
			{
				flag = false;
			}
			if (hideList[i].Key == end && !hideList[i].Value)
			{
				flag2 = false;
			}
		}
		if (flag || flag2)
		{
			if (start < end)
			{
				hideList.Add(new KeyValuePair<int, bool>(start, value: true));
				hideList.Add(new KeyValuePair<int, bool>(end, value: false));
			}
			else
			{
				hideList.Add(new KeyValuePair<int, bool>(start, value: true));
				hideList.Add(new KeyValuePair<int, bool>(360, value: false));
				hideList.Add(new KeyValuePair<int, bool>(0, value: true));
				hideList.Add(new KeyValuePair<int, bool>(end, value: false));
			}
			return true;
		}
		return false;
	}

	public void RemoveHideRange(int start, int end)
	{
		if (RemoveHideRange(start, end, _hideDegreeList))
		{
			Target.MarkAsChanged();
		}
	}

	public static bool RemoveHideRange(int start, int end, List<KeyValuePair<int, bool>> hideList)
	{
		int count = hideList.Count;
		for (int num = hideList.Count - 1; num >= 0; num--)
		{
			if (hideList[num].Key == start && hideList[num].Value)
			{
				hideList.RemoveAt(num);
			}
			else if (hideList[num].Key == end && !hideList[num].Value)
			{
				hideList.RemoveAt(num);
			}
		}
		return count != hideList.Count;
	}

	public void ClearHideRanges()
	{
		_hideDegreeList.Clear();
		Target.MarkAsChanged();
	}

	private static int DegreeListCompare(KeyValuePair<int, bool> v1, KeyValuePair<int, bool> v2)
	{
		int num = v1.Key - v2.Key;
		if (num == 0)
		{
			return (v1.Value != v2.Value) ? ((!v1.Value) ? 1 : (-1)) : 0;
		}
		return num;
	}

	private void CalcHideDegreeList()
	{
		CalcHideDegreeList(_hideDegreeList);
	}

	public static void CalcHideDegreeList(List<KeyValuePair<int, bool>> hideList)
	{
		hideList.Sort(DegreeListCompare);
		int num = 0;
		for (int i = 0; i < hideList.Count; i++)
		{
			if (hideList[i].Value)
			{
				if (num != 0)
				{
					hideList.RemoveAt(i);
					i--;
				}
				num++;
			}
			else if (num != 0)
			{
				num--;
				if (num != 0)
				{
					hideList.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public static List<KeyValuePair<int, bool>> GetHideList(params int[] angles)
	{
		if (angles == null || angles.Length % 2 != 0)
		{
			return null;
		}
		List<KeyValuePair<int, bool>> list = new List<KeyValuePair<int, bool>>();
		for (int i = 0; i < angles.Length; i += 2)
		{
			AddHideRange(angles[i], angles[i + 1], list);
		}
		CalcHideDegreeList(list);
		return list;
	}

	public void GetHideDegreeRange(int index, out int min, out int max)
	{
		GetHideDegreeRange(index, _hideDegreeList, out min, out max);
	}

	private static void GetHideDegreeRange(int index, List<KeyValuePair<int, bool>> hideList, out int min, out int max)
	{
		min = 0;
		max = 0;
		if (index * 2 + 1 < hideList.Count)
		{
			min = hideList[index * 2].Key;
			max = hideList[index * 2 + 1].Key;
		}
	}

	private void OnFillSprite(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		CalcHideDegreeList();
		Convert(_hideDegreeList, Target.color, verts, uvs, cols);
	}

	private static bool IsSameDegreePart(int v1, int v2)
	{
		int num = DegreePart(v1);
		int num2 = DegreePart(v2);
		return num == num2;
	}

	private static int DegreePart(int v)
	{
		return v / 90;
	}

	private static void FillPart(Rect v, Rect u, Color[] c, int start, int end, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		if (start < end)
		{
			int num = DegreePart(start);
			Vector2[] array = (Vector2[])(object)new Vector2[4];
			Vector2[] array2 = (Vector2[])(object)new Vector2[4];
			float num2 = v.Height / 2f;
			float num3 = v.Width / 2f;
			ref Vector2 reference = ref array[0];
			reference = Vector2.zero;
			Vector2 val = v.DegreeToPos(start);
			Vector2 val2 = v.DegreeToPos(end);
			array[1] = val;
			int num4 = start % 90;
			int num5 = end % 90;
			if (num4 == 0 && start / 90 != num)
			{
				num4 = 90;
			}
			if (num5 == 0 && end / 90 != num)
			{
				num5 = 90;
			}
			if (num4 < 45 && num5 < 45)
			{
				array[2] = val2;
				ref Vector2 reference2 = ref array[3];
				reference2 = Vector2.zero;
			}
			else if (num4 >= 45 && num5 >= 45)
			{
				array[2] = val;
				array[3] = val2;
			}
			else
			{
				array[2].x = num3 * (float)((num == 0 || num == 1) ? 1 : (-1));
				array[2].y = num2 * (float)((num == 0 || num == 3) ? 1 : (-1));
				array[3] = val2;
			}
			float num6 = u.Width / 2f;
			float num7 = u.Height / 2f;
			for (int i = 0; i < 4; i++)
			{
				array2[i].x = array[i].x / num3 * num6;
				array2[i].y = array[i].y / num2 * num7;
			}
			for (int j = 0; j < 4; j++)
			{
				ref Vector2 reference3 = ref array[j];
				reference3 += v.Center;
				ref Vector2 reference4 = ref array2[j];
				reference4 += u.Center;
				verts.Add(Vector2.op_Implicit(array[j]));
				uvs.Add(array2[j]);
				cols.Add(c[j]);
			}
		}
	}

	public static void Convert(Rect v, Rect u, Color[] c, int hideCount, List<KeyValuePair<int, bool>> hideList, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		int num = 0;
		for (int i = 0; i < hideCount; i++)
		{
			GetHideDegreeRange(i, hideList, out var min, out var max);
			while (num < min && !IsSameDegreePart(num, min))
			{
				int num2 = (DegreePart(num) + 1) * 90;
				FillPart(v, u, c, num, num2, verts, uvs, cols);
				num = num2;
			}
			FillPart(v, u, c, num, min, verts, uvs, cols);
			num = max;
		}
		while (num < 360 && !IsSameDegreePart(num, 360))
		{
			int num3 = (DegreePart(num) + 1) * 90;
			FillPart(v, u, c, num, num3, verts, uvs, cols);
			num = num3;
		}
	}

	public static void Convert(List<KeyValuePair<int, bool>> hideList, Color color, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		if (hideList != null && hideList.Count != 0)
		{
			Color[] array = (Color[])(object)new Color[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = color;
			}
			Rect v = default(Rect);
			v.left = KMathUtil.Min(verts[0].x, verts[1].x, verts[2].x, verts[3].x);
			v.right = KMathUtil.Max(verts[0].x, verts[1].x, verts[2].x, verts[3].x);
			v.bottom = KMathUtil.Min(verts[0].y, verts[1].y, verts[2].y, verts[3].y);
			v.top = KMathUtil.Max(verts[0].y, verts[1].y, verts[2].y, verts[3].y);
			Rect u = default(Rect);
			u.left = KMathUtil.Min(uvs[0].x, uvs[1].x, uvs[2].x, uvs[3].x);
			u.right = KMathUtil.Max(uvs[0].x, uvs[1].x, uvs[2].x, uvs[3].x);
			u.bottom = KMathUtil.Min(uvs[0].y, uvs[1].y, uvs[2].y, uvs[3].y);
			u.top = KMathUtil.Max(uvs[0].y, uvs[1].y, uvs[2].y, uvs[3].y);
			verts.Clear();
			uvs.Clear();
			cols.Clear();
			Convert(v, u, array, hideList.Count / 2, hideList, verts, uvs, cols);
		}
	}
}
