using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public static class RectLayoutCalculator
{
	public static void CalcLayout(RectLayout.LayoutArgument layout, IList<RectLayout.RectArgument> list, ref List<Rect> results, out Vector2 contentsSize, out Vector2 parentSize)
	{
		if (KUtility.GetSize(list) == 0)
		{
			contentsSize = Vector2.zero;
			parentSize = Vector2.zero;
			return;
		}
		if (results == null)
		{
			results = new List<Rect>();
		}
		results.Clear();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			results.Add(default(Rect));
		}
		parentSize = new Vector2(layout.Width, layout.Height);
		CalcRectsSize(layout, list, ref parentSize, out contentsSize, results);
		CalcRectsLayout(layout, list, parentSize, results);
		for (int j = 0; j < count; j++)
		{
			if (list[j].Compatible != null)
			{
				Vector2 size = results[j].size;
				list[j].Compatible(size.x, size.y);
			}
		}
	}

	private static void CalcRectsLayout(RectLayout.LayoutArgument layout, IList<RectLayout.RectArgument> list, Vector2 contentsSize, List<Rect> results)
	{
		Rect parentRect = new Rect(new Vector2(0f, 0f - contentsSize.y), contentsSize);
		CalcRectLayout(layout, list, parentRect, 0, results);
	}

	private static void CalcRectLayout(RectLayout.LayoutArgument layout, IList<RectLayout.RectArgument> list, Rect parentRect, int index, List<Rect> results)
	{
		int depth = list[index].Depth;
		RectLayout.Direction direction = layout.Direction;
		if (depth % 2 == 1)
		{
			direction = (RectLayout.Direction)((int)(direction + 1) % 2);
		}
		float num = 0f;
		for (int i = index; i < list.Count; i++)
		{
			RectLayout.RectArgument rectArgument = list[i];
			int depth2 = rectArgument.Depth;
			if (depth2 < depth)
			{
				break;
			}
			if (depth2 == depth)
			{
				Vector2 size = results[i].size;
				float num2 = 0f;
				switch (rectArgument.PositionType)
				{
				case 0:
					num2 = 0f;
					break;
				case 1:
					num2 = 0.5f;
					break;
				case 2:
					num2 = 1f;
					break;
				}
				Vector2 position = new Vector2(parentRect.xMin, parentRect.yMax);
				if (direction == RectLayout.Direction.Horizontal)
				{
					position += new Vector2(num, (0f - (parentRect.height - size.y)) * num2);
					position.x += (float)rectArgument.Padding.Left + rectArgument.Spacing.Begin;
					position.y -= (float)rectArgument.Padding.Top + rectArgument.Spacing.Side1;
					num += size.x + (float)rectArgument.Padding.GetSize(RectLayout.Direction.Horizontal) + rectArgument.Spacing.Sum();
				}
				else
				{
					position += new Vector2((parentRect.width - size.x) * num2, 0f - num);
					position.y -= (float)rectArgument.Padding.Top + rectArgument.Spacing.Begin;
					position.x += (float)rectArgument.Padding.Left + rectArgument.Spacing.Side1;
					num += size.y + (float)rectArgument.Padding.GetSize(RectLayout.Direction.Vertical) + rectArgument.Spacing.Sum();
				}
				position.y -= size.y;
				Rect value = new Rect(position, size);
				results[i] = value;
			}
		}
		for (int j = index; j < list.Count; j++)
		{
			int depth3 = list[j].Depth;
			if (depth3 >= depth)
			{
				if (depth3 == depth && j + 1 < list.Count && list[j + 1].Depth > depth)
				{
					CalcRectLayout(layout, list, results[j], j + 1, results);
				}
				continue;
			}
			break;
		}
	}

	private static void CalcRectsSize(RectLayout.LayoutArgument layout, IList<RectLayout.RectArgument> list, ref Vector2 parentSize, out Vector2 contentsSize, List<Rect> result)
	{
		if (parentSize.x == 0f || parentSize.y == 0f)
		{
			CalcRectSize(layout, list, parentSize, 0, out var size, result);
			if (parentSize.x == 0f)
			{
				parentSize.x = size.x;
			}
			if (parentSize.y == 0f)
			{
				parentSize.y = size.y;
			}
		}
		CalcRectSize(layout, list, parentSize, 0, out contentsSize, result);
	}

	private static float? GetDirectionalLength(RectLayout.RectArgument r, Vector2 parentSize)
	{
		float? result = null;
		switch (r.Breadth.Type)
		{
		case RectLayout.ItemType.Pixel:
			if (r.Breadth.Value > 0f)
			{
				result = r.Breadth.Value;
			}
			else if (r.Breadth.Value < 0f && parentSize.y > 0f)
			{
				result = parentSize.y + r.Breadth.Value;
			}
			break;
		case RectLayout.ItemType.Ratio:
			if (parentSize.y > 0f)
			{
				result = parentSize.y * r.Breadth.Value;
			}
			break;
		case RectLayout.ItemType.Weight:
			if (parentSize.y > 0f)
			{
				result = parentSize.y * r.Breadth.Value;
			}
			break;
		}
		if (result.HasValue && r.Breadth.Min > result.GetValueOrDefault())
		{
			result = r.Breadth.Min;
		}
		if (r.Breadth.Max > 0f && result.HasValue && r.Breadth.Max < result.GetValueOrDefault())
		{
			result = r.Breadth.Max;
		}
		return result;
	}

	private static void CalcRectSize(RectLayout.LayoutArgument layout, IList<RectLayout.RectArgument> list, Vector2 parentSize, int index, out Vector2 size, List<Rect> result)
	{
		int depth = list[index].Depth;
		int num = 0;
		for (int i = index; i < list.Count; i++)
		{
			int depth2 = list[i].Depth;
			if (depth2 < depth)
			{
				break;
			}
			if (depth2 == depth)
			{
				num++;
			}
		}
		RectLayout.Direction direction = layout.Direction;
		if (depth % 2 == 1)
		{
			direction = (RectLayout.Direction)((int)(direction + 1) % 2);
		}
		if (direction == RectLayout.Direction.Vertical)
		{
			parentSize = new Vector2(parentSize.y, parentSize.x);
		}
		using (Reusable<List<RectLayout.RectArgument>> reusable = ReusableList<RectLayout.RectArgument>.Pop())
		{
			SetCollectionSize(reusable.Value, num);
			for (int j = index; j < list.Count; j++)
			{
				int depth3 = list[j].Depth;
				if (depth3 < depth)
				{
					break;
				}
				if (depth3 != depth)
				{
					continue;
				}
				RectLayout.RectArgument rectArgument = list[j];
				if (rectArgument.Compatible == null || rectArgument.Size.Type != 0 || rectArgument.Size.Value != 0f)
				{
					continue;
				}
				float? num2 = GetDirectionalLength(rectArgument, parentSize);
				Vector2 vector;
				if (direction == RectLayout.Direction.Horizontal)
				{
					if (num2.HasValue)
					{
						num2 = Mathf.Max(0f, num2.Value - (float)rectArgument.Padding.GetSize(RectLayout.Direction.Vertical));
					}
					vector = rectArgument.Compatible(null, num2);
					rectArgument.Size.Value = vector.x;
				}
				else
				{
					if (num2.HasValue)
					{
						num2 = Mathf.Max(0f, num2.Value - (float)rectArgument.Padding.GetSize(RectLayout.Direction.Horizontal));
					}
					vector = rectArgument.Compatible(num2, null);
					rectArgument.Size.Value = vector.y;
				}
				if (rectArgument.Breadth.Type == RectLayout.ItemType.Pixel && rectArgument.Breadth.Value == 0f)
				{
					rectArgument.Breadth.Value = ((direction != 0) ? vector.x : vector.y);
				}
				rectArgument.Compatible = null;
				list[j] = rectArgument;
			}
			int num3 = 0;
			for (int k = index; k < list.Count; k++)
			{
				int depth4 = list[k].Depth;
				if (depth4 < depth)
				{
					break;
				}
				if (depth4 == depth)
				{
					reusable.Value[num3] = list[k];
					num3++;
				}
			}
			using Reusable<List<float>> reusable2 = CalcItemsLength(parentSize.x, reusable.Value, num);
			num3 = 0;
			for (int l = index; l < list.Count; l++)
			{
				int depth5 = list[l].Depth;
				if (depth5 < depth)
				{
					break;
				}
				if (depth5 != depth)
				{
					continue;
				}
				RectLayout.RectArgument value = list[l];
				if (value.Compatible != null && value.Breadth.Type == RectLayout.ItemType.Pixel && value.Breadth.Value == 0f)
				{
					float num4 = reusable2.Value[num3];
					if (direction == RectLayout.Direction.Horizontal)
					{
						num4 = Mathf.Max(0f, num4 - (float)value.Padding.GetSize(RectLayout.Direction.Horizontal));
						Vector2 vector2 = value.Compatible(num4, null);
						value.Breadth.Value = vector2.y;
					}
					else
					{
						num4 = Mathf.Max(0f, num4 - (float)value.Padding.GetSize(RectLayout.Direction.Vertical));
						Vector2 vector3 = value.Compatible(null, num4);
						value.Breadth.Value = vector3.x;
					}
					value.Compatible = null;
					list[l] = value;
				}
				num3++;
			}
			int num5 = 0;
			for (int m = index; m < list.Count; m++)
			{
				int depth6 = list[m].Depth;
				if (depth6 < depth)
				{
					break;
				}
				if (depth6 != depth)
				{
					continue;
				}
				RectLayout.RectArgument rectArgument2 = list[m];
				float num6 = 0f;
				switch (rectArgument2.Breadth.Type)
				{
				case RectLayout.ItemType.Pixel:
					if (rectArgument2.Breadth.Value >= 0f)
					{
						num6 = rectArgument2.Breadth.Value;
					}
					else if (parentSize.y > 0f)
					{
						num6 = parentSize.y + rectArgument2.Breadth.Value;
					}
					break;
				case RectLayout.ItemType.Ratio:
					if (parentSize.y > 0f)
					{
						num6 = parentSize.y * rectArgument2.Breadth.Value;
					}
					break;
				case RectLayout.ItemType.Weight:
					if (parentSize.y > 0f)
					{
						num6 = parentSize.y * rectArgument2.Breadth.Value;
					}
					break;
				}
				if (rectArgument2.Breadth.Min > num6)
				{
					num6 = rectArgument2.Breadth.Min;
				}
				if (rectArgument2.Breadth.Max > 0f && rectArgument2.Breadth.Max < num6)
				{
					num6 = rectArgument2.Breadth.Max;
				}
				Vector2 size2 = new Vector2(reusable2.Value[num5], num6);
				if (direction == RectLayout.Direction.Vertical)
				{
					size2 = new Vector2(size2.y, size2.x);
				}
				size2 -= new Vector2(rectArgument2.Padding.GetSize(RectLayout.Direction.Horizontal), rectArgument2.Padding.GetSize(RectLayout.Direction.Vertical));
				result[m] = new Rect(Vector2.zero, size2);
				num5++;
			}
		}
		for (int n = index; n < list.Count; n++)
		{
			int depth7 = list[n].Depth;
			if (depth7 < depth)
			{
				break;
			}
			if (depth7 != depth || n + 1 >= list.Count || list[n + 1].Depth <= depth)
			{
				continue;
			}
			Vector2 size3 = result[n].size;
			Vector2 size4;
			if (size3.x == 0f || size3.y == 0f)
			{
				CalcRectSize(layout, list, size3, n + 1, out size4, result);
				if (size3.x == 0f)
				{
					size3.x = size4.x;
				}
				if (size3.y == 0f)
				{
					size3.y = size4.y;
				}
			}
			CalcRectSize(layout, list, size3, n + 1, out size4, result);
			result[n] = new Rect(Vector2.zero, size3);
		}
		size = Vector2.zero;
		for (int num7 = index; num7 < list.Count; num7++)
		{
			RectLayout.RectArgument rectArgument3 = list[num7];
			int depth8 = list[num7].Depth;
			if (depth8 < depth)
			{
				break;
			}
			if (depth8 == depth)
			{
				Vector2 vector4 = result[num7].size + new Vector2(rectArgument3.Padding.GetSize(RectLayout.Direction.Horizontal), rectArgument3.Padding.GetSize(RectLayout.Direction.Vertical));
				if (direction == RectLayout.Direction.Horizontal)
				{
					vector4 += new Vector2(rectArgument3.Spacing.Sum(), rectArgument3.Spacing.Breadth());
					size.x += vector4.x;
					size.y = Mathf.Max(size.y, vector4.y);
				}
				else
				{
					vector4 += new Vector2(rectArgument3.Spacing.Breadth(), rectArgument3.Spacing.Sum());
					size.x = Mathf.Max(size.x, vector4.x);
					size.y += vector4.y;
				}
			}
		}
	}

	private static Reusable<List<float>> CalcItemsLength(float size, IList<RectLayout.RectArgument> list, int count)
	{
		Reusable<List<float>> reusable = ReusableList<float>.Pop();
		using Reusable<List<bool>> reusable2 = ReusableList<bool>.Pop();
		SetCollectionSize(reusable.Value, count, 0f);
		SetCollectionSize(reusable2.Value, count, value: false);
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			RectLayout.ItemArgument size2 = list[i].Size;
			float? num2 = null;
			switch (size2.Type)
			{
			case RectLayout.ItemType.Pixel:
				num2 = ((!(size2.Value < 0f)) ? size2.Value : (size + size2.Value));
				break;
			case RectLayout.ItemType.Ratio:
				num2 = size * size2.Value;
				break;
			}
			if (num2.HasValue && size2.Min > num2.GetValueOrDefault())
			{
				num2 = size2.Min;
			}
			if (size2.Max > 0f && num2.HasValue && size2.Max < num2.GetValueOrDefault())
			{
				num2 = size2.Max;
			}
			if (num2.HasValue)
			{
				float value = num2.Value;
				value = Mathf.Max(value, 0f);
				num += value;
				reusable.Value[i] = value;
				reusable2.Value[i] = true;
			}
		}
		float num3 = 0f;
		for (int j = 0; j < count; j++)
		{
			if (!reusable2.Value[j])
			{
				RectLayout.ItemArgument size3 = list[j].Size;
				if (size3.Type == RectLayout.ItemType.Weight)
				{
					num3 += size3.Value;
				}
			}
		}
		float num4 = size - num;
		for (int k = 0; k < count; k++)
		{
			num4 -= list[k].Spacing.Sum();
		}
		num4 = Mathf.Max(num4, 0f);
		float num5 = num4;
		float num6 = num3;
		for (int l = 0; l < count; l++)
		{
			if (reusable2.Value[l])
			{
				continue;
			}
			RectLayout.ItemArgument size4 = list[l].Size;
			float? num7 = null;
			if (size4.Type == RectLayout.ItemType.Weight)
			{
				num7 = ((!(num6 > 0f)) ? num5 : (num5 * size4.Value / num6));
			}
			if (num7.HasValue)
			{
				float? num8 = null;
				if (size4.Min > num7.Value)
				{
					num8 = size4.Min;
				}
				if (size4.Max > 0f && size4.Max < num7.Value)
				{
					num8 = size4.Max;
				}
				if (num8.HasValue)
				{
					float value2 = num8.Value;
					value2 = Mathf.Max(value2, 0f);
					num += value2;
					reusable.Value[l] = value2;
					reusable2.Value[l] = true;
					num4 -= value2;
					num3 -= size4.Value;
				}
			}
		}
		for (int m = 0; m < count; m++)
		{
			if (!reusable2.Value[m])
			{
				RectLayout.ItemArgument size5 = list[m].Size;
				float? num9 = null;
				if (size5.Type == RectLayout.ItemType.Weight)
				{
					num9 = ((!(num3 > 0f)) ? num4 : (num4 * size5.Value / num3));
				}
				if (num9.HasValue)
				{
					float value3 = num9.Value;
					value3 = Mathf.Max(value3, 0f);
					num += value3;
					reusable.Value[m] = value3;
					reusable2.Value[m] = true;
				}
			}
		}
		return reusable;
	}

	private static void SetCollectionSize<T>([NotNull] ICollection<T> collection, int count, T value = default(T))
	{
		collection.Clear();
		for (int i = 0; i < count; i++)
		{
			collection.Add(value);
		}
	}
}
