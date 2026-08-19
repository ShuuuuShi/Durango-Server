using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public abstract class DrawerBase
{
	protected virtual Point2 ChangePos(int x, int y, int kernel)
	{
		return new Point2(x, y);
	}

	public void Draw(Texture2D canvas, int x, int y, int kernel, Color targetColor, List<Point2> points, DrawHistory history)
	{
		Point2 point = ChangePos(x, y, kernel);
		int i = 0;
		for (int size = KUtility.GetSize(points); i < size; i++)
		{
			int num = points[i].x + point.x;
			if (num < 0 || num > canvas.width - 1)
			{
				continue;
			}
			int num2 = points[i].y + point.y;
			if (num2 >= 0 && num2 <= canvas.height - 1)
			{
				Color pixel = canvas.GetPixel(num, num2);
				if (!(pixel == targetColor))
				{
					history.Add(num, num2, pixel, targetColor);
					canvas.SetPixel(num, num2, targetColor);
				}
			}
		}
	}
}
