using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public static class DrawExtension
{
	public static List<Point2> GetNode(ToolDatum tool)
	{
		if (tool is PenToolDatum penToolDatum)
		{
			return GetPenPoint2(penToolDatum.PenType);
		}
		if (tool is BrushToolDatum brushToolDatum)
		{
			return GetBrushPoint2(brushToolDatum.BrushType);
		}
		return null;
	}

	private static List<Point2> GetPenPoint2(PenType size)
	{
		switch (size)
		{
		case PenType.Size1:
		{
			List<Point2> list = new List<Point2>();
			list.Add(new Point2(0, 0));
			return list;
		}
		case PenType.Size3:
		{
			List<Point2> penPoint2 = GetPenPoint2(PenType.Size1);
			penPoint2.Add(new Point2(-1, 0));
			penPoint2.Add(new Point2(1, 0));
			penPoint2.Add(new Point2(0, -1));
			penPoint2.Add(new Point2(0, 1));
			return penPoint2;
		}
		case PenType.Size5:
		{
			List<Point2> penPoint = GetPenPoint2(PenType.Size3);
			penPoint.Add(new Point2(-2, 0));
			penPoint.Add(new Point2(2, 0));
			penPoint.Add(new Point2(0, 2));
			penPoint.Add(new Point2(0, -2));
			penPoint.Add(new Point2(-1, -1));
			penPoint.Add(new Point2(1, -1));
			penPoint.Add(new Point2(-1, 1));
			penPoint.Add(new Point2(1, 1));
			return penPoint;
		}
		default:
			throw new ArgumentOutOfRangeException("size", size, null);
		}
	}

	private static List<Point2> AddTo(this List<Point2> list, Point2 Point2)
	{
		list.Add(Point2);
		return list;
	}

	public static List<Point2> GetBrushPoint2(BrushType type)
	{
		switch (type)
		{
		case BrushType.Bayer1:
		{
			List<Point2> list2 = new List<Point2>();
			list2.Add(new Point2(-1, -1));
			return list2;
		}
		case BrushType.Bayer2:
			return GetBrushPoint2(BrushType.Bayer1).AddTo(new Point2(0, 0));
		case BrushType.Bayer2Inv:
		{
			List<Point2> list = new List<Point2>();
			list.Add(new Point2(-1, 0));
			list.Add(new Point2(0, -1));
			return list;
		}
		case BrushType.Bayer3:
			return GetBrushPoint2(BrushType.Bayer2).AddTo(new Point2(1, 1)).AddTo(new Point2(-1, 1)).AddTo(new Point2(1, -1));
		case BrushType.Bayer3Inv:
			return GetBrushPoint2(BrushType.Bayer2Inv).AddTo(new Point2(1, 0)).AddTo(new Point2(0, 1));
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	public static void FloodFill(Texture2D texture, int tX, int tY, Color targetColor, DrawHistory history)
	{
		int width = texture.width;
		int height = texture.height;
		Color[] pixels = texture.GetPixels();
		Color color = pixels[tX + tY * width];
		Queue<Point2> queue = new Queue<Point2>();
		queue.Enqueue(new Point2(tX, tY));
		while (queue.Count > 0)
		{
			Point2 point = queue.Dequeue();
			for (int i = point.x; i < width; i++)
			{
				Color color2 = pixels[i + point.y * width];
				if (color2 != color || color2 == targetColor)
				{
					break;
				}
				history.Add(i, point.y, pixels[i + point.y * width], targetColor);
				pixels[i + point.y * width] = targetColor;
				if (point.y + 1 < height)
				{
					color2 = pixels[i + point.y * width + width];
					if (color2 == color && color2 != targetColor)
					{
						queue.Enqueue(new Point2(i, point.y + 1));
					}
				}
				if (point.y - 1 >= 0)
				{
					color2 = pixels[i + point.y * width - width];
					if (color2 == color && color2 != targetColor)
					{
						queue.Enqueue(new Point2(i, point.y - 1));
					}
				}
			}
			for (int num = point.x - 1; num >= 0; num--)
			{
				Color color3 = pixels[num + point.y * width];
				if (color3 != color || color3 == targetColor)
				{
					break;
				}
				history.Add(num, point.y, pixels[num + point.y * width], targetColor);
				pixels[num + point.y * width] = targetColor;
				if (point.y + 1 < height)
				{
					color3 = pixels[num + point.y * width + width];
					if (color3 == color && color3 != targetColor)
					{
						queue.Enqueue(new Point2(num, point.y + 1));
					}
				}
				if (point.y - 1 >= 0)
				{
					color3 = pixels[num + point.y * width - width];
					if (color3 == color && color3 != targetColor)
					{
						queue.Enqueue(new Point2(num, point.y - 1));
					}
				}
			}
		}
		texture.SetPixels(pixels);
	}

	public static Texture2D MakeTexture(int width = 0, int height = 0)
	{
		Texture2D texture2D = new Texture2D(width, height);
		texture2D.filterMode = FilterMode.Point;
		texture2D.wrapMode = TextureWrapMode.Clamp;
		return texture2D;
	}

	public static Texture2D MakeEmptyTexture(int width, int height)
	{
		Texture2D texture2D = MakeTexture(width, height);
		Color32[] array = new Color32[width * height];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			ref Color32 reference = ref array[i];
			reference = new Color32(0, 0, 0, 0);
		}
		texture2D.SetPixels32(array);
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D Clear(this Texture2D tex)
	{
		Color32[] array = new Color32[tex.width * tex.height];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			ref Color32 reference = ref array[i];
			reference = new Color32(0, 0, 0, 0);
		}
		tex.SetPixels32(array);
		tex.Apply();
		return tex;
	}

	public static Point2 GetContourSquareSize(ICollection<Point2> nodes)
	{
		if (nodes.Count == 0)
		{
			return new Point2(0, 0);
		}
		int num = int.MinValue;
		int num2 = int.MaxValue;
		int num3 = int.MinValue;
		int num4 = int.MaxValue;
		foreach (Point2 node in nodes)
		{
			if (num < node.x)
			{
				num = node.x;
			}
			if (num2 > node.x)
			{
				num2 = node.x;
			}
			if (num3 < node.y)
			{
				num3 = node.y;
			}
			if (num4 > node.y)
			{
				num4 = node.y;
			}
		}
		num = Math.Max(0, num);
		num2 = Math.Min(0, num2);
		num3 = Math.Max(0, num3);
		num4 = Math.Min(0, num4);
		return new Point2(num - num2 + 1, num3 - num4 + 1);
	}
}
