using UnityEngine;

namespace Durango.Utils.Extensions;

public static class ColorExtensions
{
	public static Color WithR(this Color c, float r)
	{
		return new Color(r, c.g, c.b, c.a);
	}

	public static Color WithG(this Color c, float g)
	{
		return new Color(c.r, g, c.b, c.a);
	}

	public static Color WithB(this Color c, float b)
	{
		return new Color(c.r, c.g, b, c.a);
	}

	public static Color WithA(this Color c, float a)
	{
		return new Color(c.r, c.g, c.b, a);
	}

	public static string ToHex(this Color c)
	{
		return NGUIText.EncodeColor(c);
	}

	public static float SumOfColorElement(this Color c)
	{
		return c.r + c.g + c.b;
	}

	public static float SqrMagnitude(this Color c)
	{
		return c.r * c.r + c.g * c.g + c.b * c.b;
	}
}
