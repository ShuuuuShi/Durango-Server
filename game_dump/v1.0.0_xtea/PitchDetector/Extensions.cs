using System;

namespace PitchDetector;

internal static class Extensions
{
	public static void Clear(this float[] buffer)
	{
		Array.Clear(buffer, 0, buffer.Length);
	}

	public static void Clear(this double[] buffer)
	{
		Array.Clear(buffer, 0, buffer.Length);
	}

	public static void Copy(this float[] fromBuffer, float[] toBuffer, int fromStart, int toStart, int length)
	{
		if (toBuffer == null || fromBuffer.Length == 0 || toBuffer.Length == 0)
		{
			return;
		}
		int num = fromStart;
		int num2 = fromStart + length;
		int num3 = toStart;
		int num4 = toStart + length;
		if (num < 0)
		{
			num3 -= num;
			num = 0;
		}
		if (num3 < 0)
		{
			num -= num3;
			num3 = 0;
		}
		if (num2 >= fromBuffer.Length)
		{
			num4 -= num2 - fromBuffer.Length + 1;
			num2 = fromBuffer.Length - 1;
		}
		if (num4 >= toBuffer.Length)
		{
			num2 -= num4 - toBuffer.Length + 1;
			num4 = fromBuffer.Length - 1;
		}
		if (num < num3)
		{
			int num5 = num2;
			int num6 = num4;
			while (num5 >= num)
			{
				toBuffer[num6] = fromBuffer[num5];
				num5--;
				num6--;
			}
		}
		else
		{
			int num7 = num;
			int num8 = num3;
			while (num7 <= num2)
			{
				toBuffer[num8] = fromBuffer[num7];
				num7++;
				num8++;
			}
		}
	}

	public static void Clear(this float[] buffer, int startIdx, int endIdx)
	{
		Array.Clear(buffer, startIdx, endIdx - startIdx + 1);
	}

	public static void Fill(this double[] buffer, double value)
	{
		for (int i = 0; i < buffer.Length; i++)
		{
			buffer[i] = value;
		}
	}
}
