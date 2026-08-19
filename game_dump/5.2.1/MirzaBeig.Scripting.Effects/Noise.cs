using System;

namespace MirzaBeig.Scripting.Effects;

[Serializable]
public static class Noise
{
	private static float F3 = 1f / 3f;

	private static float G3 = 1f / 6f;

	private static byte[] perm = new byte[512]
	{
		151, 160, 137, 91, 90, 15, 131, 13, 201, 95,
		96, 53, 194, 233, 7, 225, 140, 36, 103, 30,
		69, 142, 8, 99, 37, 240, 21, 10, 23, 190,
		6, 148, 247, 120, 234, 75, 0, 26, 197, 62,
		94, 252, 219, 203, 117, 35, 11, 32, 57, 177,
		33, 88, 237, 149, 56, 87, 174, 20, 125, 136,
		171, 168, 68, 175, 74, 165, 71, 134, 139, 48,
		27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
		60, 211, 133, 230, 220, 105, 92, 41, 55, 46,
		245, 40, 244, 102, 143, 54, 65, 25, 63, 161,
		1, 216, 80, 73, 209, 76, 132, 187, 208, 89,
		18, 169, 200, 196, 135, 130, 116, 188, 159, 86,
		164, 100, 109, 198, 173, 186, 3, 64, 52, 217,
		226, 250, 124, 123, 5, 202, 38, 147, 118, 126,
		255, 82, 85, 212, 207, 206, 59, 227, 47, 16,
		58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
		119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
		101, 155, 167, 43, 172, 9, 129, 22, 39, 253,
		19, 98, 108, 110, 79, 113, 224, 232, 178, 185,
		112, 104, 218, 246, 97, 228, 251, 34, 242, 193,
		238, 210, 144, 12, 191, 179, 162, 241, 81, 51,
		145, 235, 249, 14, 239, 107, 49, 192, 214, 31,
		181, 199, 106, 157, 184, 84, 204, 176, 115, 121,
		50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
		222, 114, 67, 29, 24, 72, 243, 141, 128, 195,
		78, 66, 215, 61, 156, 180, 151, 160, 137, 91,
		90, 15, 131, 13, 201, 95, 96, 53, 194, 233,
		7, 225, 140, 36, 103, 30, 69, 142, 8, 99,
		37, 240, 21, 10, 23, 190, 6, 148, 247, 120,
		234, 75, 0, 26, 197, 62, 94, 252, 219, 203,
		117, 35, 11, 32, 57, 177, 33, 88, 237, 149,
		56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
		74, 165, 71, 134, 139, 48, 27, 166, 77, 146,
		158, 231, 83, 111, 229, 122, 60, 211, 133, 230,
		220, 105, 92, 41, 55, 46, 245, 40, 244, 102,
		143, 54, 65, 25, 63, 161, 1, 216, 80, 73,
		209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
		135, 130, 116, 188, 159, 86, 164, 100, 109, 198,
		173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
		5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
		207, 206, 59, 227, 47, 16, 58, 17, 182, 189,
		28, 42, 223, 183, 170, 213, 119, 248, 152, 2,
		44, 154, 163, 70, 221, 153, 101, 155, 167, 43,
		172, 9, 129, 22, 39, 253, 19, 98, 108, 110,
		79, 113, 224, 232, 178, 185, 112, 104, 218, 246,
		97, 228, 251, 34, 242, 193, 238, 210, 144, 12,
		191, 179, 162, 241, 81, 51, 145, 235, 249, 14,
		239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
		184, 84, 204, 176, 115, 121, 50, 45, 127, 4,
		150, 254, 138, 236, 205, 93, 222, 114, 67, 29,
		24, 72, 243, 141, 128, 195, 78, 66, 215, 61,
		156, 180
	};

	private static float smooth(float t)
	{
		return t * t * (3f - 2f * t);
	}

	private static float fade(float t)
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	private static int floor(float x)
	{
		if (x > 0f)
		{
			return (int)x;
		}
		return (int)x - 1;
	}

	private static float lerp(float from, float to, float t)
	{
		return from + t * (to - from);
	}

	private static float grad(int hash, float x, float y, float z)
	{
		return (hash & 0xF) switch
		{
			0 => x + y, 
			1 => 0f - x + y, 
			2 => x - y, 
			3 => 0f - x - y, 
			4 => x + x, 
			5 => 0f - x + x, 
			6 => x - x, 
			7 => 0f - x - x, 
			8 => y + x, 
			9 => 0f - y + x, 
			10 => y - x, 
			11 => 0f - y - x, 
			12 => y + z, 
			13 => 0f - y + x, 
			14 => y - x, 
			15 => 0f - y - z, 
			_ => 0f, 
		};
	}

	public static float perlin(float x, float y, float z)
	{
		int num = ((!(x > 0f)) ? ((int)x - 1) : ((int)x));
		int num2 = ((!(y > 0f)) ? ((int)y - 1) : ((int)y));
		int num3 = ((!(z > 0f)) ? ((int)z - 1) : ((int)z));
		float num4 = x - (float)num;
		float num5 = y - (float)num2;
		float num6 = z - (float)num3;
		float num7 = num4 - 1f;
		float num8 = num5 - 1f;
		float num9 = num6 - 1f;
		int num10 = (num + 1) & 0xFF;
		int num11 = (num2 + 1) & 0xFF;
		int num12 = (num3 + 1) & 0xFF;
		num &= 0xFF;
		num2 &= 0xFF;
		num3 &= 0xFF;
		float num13 = num6 * num6 * num6 * (num6 * (num6 * 6f - 15f) + 10f);
		float num14 = num5 * num5 * num5 * (num5 * (num5 * 6f - 15f) + 10f);
		float num15 = num4 * num4 * num4 * (num4 * (num4 * 6f - 15f) + 10f);
		float num16 = (perm[num + perm[num2 + perm[num3]]] & 0xF) switch
		{
			0 => num4 + num5, 
			1 => 0f - num4 + num5, 
			2 => num4 - num5, 
			3 => 0f - num4 - num5, 
			4 => num4 + num4, 
			5 => 0f - num4 + num4, 
			6 => num4 - num4, 
			7 => 0f - num4 - num4, 
			8 => num5 + num4, 
			9 => 0f - num5 + num4, 
			10 => num5 - num4, 
			11 => 0f - num5 - num4, 
			12 => num5 + num6, 
			13 => 0f - num5 + num4, 
			14 => num5 - num4, 
			15 => 0f - num5 - num6, 
			_ => 0f, 
		};
		byte num17 = perm[num + perm[num2 + perm[num12]]];
		float num18 = num16;
		float num19 = num13;
		float num20 = num18 + num19 * ((num17 & 0xF) switch
		{
			0 => num4 + num5, 
			1 => 0f - num4 + num5, 
			2 => num4 - num5, 
			3 => 0f - num4 - num5, 
			4 => num4 + num4, 
			5 => 0f - num4 + num4, 
			6 => num4 - num4, 
			7 => 0f - num4 - num4, 
			8 => num5 + num4, 
			9 => 0f - num5 + num4, 
			10 => num5 - num4, 
			11 => 0f - num5 - num4, 
			12 => num5 + num9, 
			13 => 0f - num5 + num4, 
			14 => num5 - num4, 
			15 => 0f - num5 - num9, 
			_ => 0f, 
		} - num16);
		num16 = (perm[num + perm[num11 + perm[num3]]] & 0xF) switch
		{
			0 => num4 + num8, 
			1 => 0f - num4 + num8, 
			2 => num4 - num8, 
			3 => 0f - num4 - num8, 
			4 => num4 + num4, 
			5 => 0f - num4 + num4, 
			6 => num4 - num4, 
			7 => 0f - num4 - num4, 
			8 => num8 + num4, 
			9 => 0f - num8 + num4, 
			10 => num8 - num4, 
			11 => 0f - num8 - num4, 
			12 => num8 + num6, 
			13 => 0f - num8 + num4, 
			14 => num8 - num4, 
			15 => 0f - num8 - num6, 
			_ => 0f, 
		};
		byte num21 = perm[num + perm[num11 + perm[num12]]];
		float num22 = num16;
		num19 = num13;
		float num23 = num22 + num19 * ((num21 & 0xF) switch
		{
			0 => num4 + num8, 
			1 => 0f - num4 + num8, 
			2 => num4 - num8, 
			3 => 0f - num4 - num8, 
			4 => num4 + num4, 
			5 => 0f - num4 + num4, 
			6 => num4 - num4, 
			7 => 0f - num4 - num4, 
			8 => num8 + num4, 
			9 => 0f - num8 + num4, 
			10 => num8 - num4, 
			11 => 0f - num8 - num4, 
			12 => num8 + num9, 
			13 => 0f - num8 + num4, 
			14 => num8 - num4, 
			15 => 0f - num8 - num9, 
			_ => 0f, 
		} - num16);
		float num24 = num20 + num14 * (num23 - num20);
		num16 = (perm[num10 + perm[num2 + perm[num3]]] & 0xF) switch
		{
			0 => num7 + num5, 
			1 => 0f - num7 + num5, 
			2 => num7 - num5, 
			3 => 0f - num7 - num5, 
			4 => num7 + num7, 
			5 => 0f - num7 + num7, 
			6 => num7 - num7, 
			7 => 0f - num7 - num7, 
			8 => num5 + num7, 
			9 => 0f - num5 + num7, 
			10 => num5 - num7, 
			11 => 0f - num5 - num7, 
			12 => num5 + num6, 
			13 => 0f - num5 + num7, 
			14 => num5 - num7, 
			15 => 0f - num5 - num6, 
			_ => 0f, 
		};
		byte num25 = perm[num10 + perm[num2 + perm[num12]]];
		num18 = num16;
		num19 = num13;
		num20 = num18 + num19 * ((num25 & 0xF) switch
		{
			0 => num7 + num5, 
			1 => 0f - num7 + num5, 
			2 => num7 - num5, 
			3 => 0f - num7 - num5, 
			4 => num7 + num7, 
			5 => 0f - num7 + num7, 
			6 => num7 - num7, 
			7 => 0f - num7 - num7, 
			8 => num5 + num7, 
			9 => 0f - num5 + num7, 
			10 => num5 - num7, 
			11 => 0f - num5 - num7, 
			12 => num5 + num9, 
			13 => 0f - num5 + num7, 
			14 => num5 - num7, 
			15 => 0f - num5 - num9, 
			_ => 0f, 
		} - num16);
		num16 = (perm[num10 + perm[num11 + perm[num3]]] & 0xF) switch
		{
			0 => num7 + num8, 
			1 => 0f - num7 + num8, 
			2 => num7 - num8, 
			3 => 0f - num7 - num8, 
			4 => num7 + num7, 
			5 => 0f - num7 + num7, 
			6 => num7 - num7, 
			7 => 0f - num7 - num7, 
			8 => num8 + num7, 
			9 => 0f - num8 + num7, 
			10 => num8 - num7, 
			11 => 0f - num8 - num7, 
			12 => num8 + num6, 
			13 => 0f - num8 + num7, 
			14 => num8 - num7, 
			15 => 0f - num8 - num6, 
			_ => 0f, 
		};
		byte num26 = perm[num10 + perm[num11 + perm[num12]]];
		num22 = num16;
		num19 = num13;
		num23 = num22 + num19 * ((num26 & 0xF) switch
		{
			0 => num7 + num8, 
			1 => 0f - num7 + num8, 
			2 => num7 - num8, 
			3 => 0f - num7 - num8, 
			4 => num7 + num7, 
			5 => 0f - num7 + num7, 
			6 => num7 - num7, 
			7 => 0f - num7 - num7, 
			8 => num8 + num7, 
			9 => 0f - num8 + num7, 
			10 => num8 - num7, 
			11 => 0f - num8 - num7, 
			12 => num8 + num9, 
			13 => 0f - num8 + num7, 
			14 => num8 - num7, 
			15 => 0f - num8 - num9, 
			_ => 0f, 
		} - num16);
		float num27 = num20 + num14 * (num23 - num20);
		return 0.936f * (num24 + num15 * (num27 - num24));
	}

	public static float simplex(float x, float y, float z)
	{
		float num = (x + y + z) * F3;
		float num2 = x + num;
		float num3 = y + num;
		float num4 = z + num;
		int num5 = ((!(num2 > 0f)) ? ((int)num2 - 1) : ((int)num2));
		int num6 = ((!(num3 > 0f)) ? ((int)num3 - 1) : ((int)num3));
		int num7 = ((!(num4 > 0f)) ? ((int)num4 - 1) : ((int)num4));
		float num8 = (float)(num5 + num6 + num7) * G3;
		float num9 = (float)num5 - num8;
		float num10 = (float)num6 - num8;
		float num11 = (float)num7 - num8;
		float num12 = x - num9;
		float num13 = y - num10;
		float num14 = z - num11;
		int num15;
		int num16;
		int num17;
		int num18;
		int num19;
		int num20;
		if (num12 >= num13)
		{
			if (num13 >= num14)
			{
				num15 = 1;
				num16 = 0;
				num17 = 0;
				num18 = 1;
				num19 = 1;
				num20 = 0;
			}
			else if (num12 >= num14)
			{
				num15 = 1;
				num16 = 0;
				num17 = 0;
				num18 = 1;
				num19 = 0;
				num20 = 1;
			}
			else
			{
				num15 = 0;
				num16 = 0;
				num17 = 1;
				num18 = 1;
				num19 = 0;
				num20 = 1;
			}
		}
		else if (num13 < num14)
		{
			num15 = 0;
			num16 = 0;
			num17 = 1;
			num18 = 0;
			num19 = 1;
			num20 = 1;
		}
		else if (num12 < num14)
		{
			num15 = 0;
			num16 = 1;
			num17 = 0;
			num18 = 0;
			num19 = 1;
			num20 = 1;
		}
		else
		{
			num15 = 0;
			num16 = 1;
			num17 = 0;
			num18 = 1;
			num19 = 1;
			num20 = 0;
		}
		float num21 = num12 - (float)num15 + G3;
		float num22 = num13 - (float)num16 + G3;
		float num23 = num14 - (float)num17 + G3;
		float num24 = num12 - (float)num18 + 2f * G3;
		float num25 = num13 - (float)num19 + 2f * G3;
		float num26 = num14 - (float)num20 + 2f * G3;
		float num27 = num12 - 1f + 3f * G3;
		float num28 = num13 - 1f + 3f * G3;
		float num29 = num14 - 1f + 3f * G3;
		int num30 = num5 & 0xFF;
		int num31 = num6 & 0xFF;
		int num32 = num7 & 0xFF;
		float num33 = 0.6f - num12 * num12 - num13 * num13 - num14 * num14;
		float num34;
		if (num33 < 0f)
		{
			num34 = 0f;
		}
		else
		{
			num33 *= num33;
			byte num35 = perm[num30 + perm[num31 + perm[num32]]];
			float num36 = num33 * num33;
			num34 = num36 * (num35 & 0xF) switch
			{
				0 => num12 + num13, 
				1 => 0f - num12 + num13, 
				2 => num12 - num13, 
				3 => 0f - num12 - num13, 
				4 => num12 + num12, 
				5 => 0f - num12 + num12, 
				6 => num12 - num12, 
				7 => 0f - num12 - num12, 
				8 => num13 + num12, 
				9 => 0f - num13 + num12, 
				10 => num13 - num12, 
				11 => 0f - num13 - num12, 
				12 => num13 + num14, 
				13 => 0f - num13 + num12, 
				14 => num13 - num12, 
				15 => 0f - num13 - num14, 
				_ => 0f, 
			};
		}
		float num37 = 0.6f - num21 * num21 - num22 * num22 - num23 * num23;
		float num38;
		if (num37 < 0f)
		{
			num38 = 0f;
		}
		else
		{
			num37 *= num37;
			byte num39 = perm[num30 + num15 + perm[num31 + num16 + perm[num32 + num17]]];
			float num40 = num37 * num37;
			num38 = num40 * (num39 & 0xF) switch
			{
				0 => num21 + num22, 
				1 => 0f - num21 + num22, 
				2 => num21 - num22, 
				3 => 0f - num21 - num22, 
				4 => num21 + num21, 
				5 => 0f - num21 + num21, 
				6 => num21 - num21, 
				7 => 0f - num21 - num21, 
				8 => num22 + num21, 
				9 => 0f - num22 + num21, 
				10 => num22 - num21, 
				11 => 0f - num22 - num21, 
				12 => num22 + num23, 
				13 => 0f - num22 + num21, 
				14 => num22 - num21, 
				15 => 0f - num22 - num23, 
				_ => 0f, 
			};
		}
		float num41 = 0.6f - num24 * num24 - num25 * num25 - num26 * num26;
		float num42;
		if (num41 < 0f)
		{
			num42 = 0f;
		}
		else
		{
			num41 *= num41;
			byte num43 = perm[num30 + num18 + perm[num31 + num19 + perm[num32 + num20]]];
			float num36 = num41 * num41;
			num42 = num36 * (num43 & 0xF) switch
			{
				0 => num24 + num25, 
				1 => 0f - num24 + num25, 
				2 => num24 - num25, 
				3 => 0f - num24 - num25, 
				4 => num24 + num24, 
				5 => 0f - num24 + num24, 
				6 => num24 - num24, 
				7 => 0f - num24 - num24, 
				8 => num25 + num24, 
				9 => 0f - num25 + num24, 
				10 => num25 - num24, 
				11 => 0f - num25 - num24, 
				12 => num25 + num26, 
				13 => 0f - num25 + num24, 
				14 => num25 - num24, 
				15 => 0f - num25 - num26, 
				_ => 0f, 
			};
		}
		float num44 = 0.6f - num27 * num27 - num28 * num28 - num29 * num29;
		float num45;
		if (num44 < 0f)
		{
			num45 = 0f;
		}
		else
		{
			num44 *= num44;
			byte num46 = perm[num30 + 1 + perm[num31 + 1 + perm[num32 + 1]]];
			float num40 = num44 * num44;
			num45 = num40 * (num46 & 0xF) switch
			{
				0 => num27 + num28, 
				1 => 0f - num27 + num28, 
				2 => num27 - num28, 
				3 => 0f - num27 - num28, 
				4 => num27 + num27, 
				5 => 0f - num27 + num27, 
				6 => num27 - num27, 
				7 => 0f - num27 - num27, 
				8 => num28 + num27, 
				9 => 0f - num28 + num27, 
				10 => num28 - num27, 
				11 => 0f - num28 - num27, 
				12 => num28 + num29, 
				13 => 0f - num28 + num27, 
				14 => num28 - num27, 
				15 => 0f - num28 - num29, 
				_ => 0f, 
			};
		}
		return 32f * (num34 + num38 + num42 + num45);
	}

	public static float octavePerlin(float x, float y, float z, float frequency, int octaves, float lacunarity, float persistence)
	{
		if (octaves < 2)
		{
			return perlin(x * frequency, y * frequency, z * frequency);
		}
		float num = 0f;
		float num2 = 1f;
		float num3 = 0f;
		for (int i = 0; i < octaves; i++)
		{
			num += perlin(x * frequency, y * frequency, z * frequency) * num2;
			num3 += num2;
			frequency *= lacunarity;
			num2 *= persistence;
		}
		return num / num3;
	}

	public static float octaveSimplex(float x, float y, float z, float frequency, int octaves, float lacunarity, float persistence)
	{
		if (octaves < 2)
		{
			return simplex(x * frequency, y * frequency, z * frequency);
		}
		float num = 0f;
		float num2 = 1f;
		float num3 = 0f;
		for (int i = 0; i < octaves; i++)
		{
			num += simplex(x * frequency, y * frequency, z * frequency) * num2;
			num3 += num2;
			frequency *= lacunarity;
			num2 *= persistence;
		}
		return num / num3;
	}

	public static float perlinUnoptimized(float x, float y, float z)
	{
		int num = floor(x);
		int num2 = floor(y);
		int num3 = floor(z);
		float num4 = x - (float)num;
		float num5 = y - (float)num2;
		float num6 = z - (float)num3;
		float x2 = num4 - 1f;
		float y2 = num5 - 1f;
		float z2 = num6 - 1f;
		int num7 = (num + 1) & 0xFF;
		int num8 = (num2 + 1) & 0xFF;
		int num9 = (num3 + 1) & 0xFF;
		num &= 0xFF;
		num2 &= 0xFF;
		num3 &= 0xFF;
		float t = fade(num6);
		float t2 = fade(num5);
		float t3 = fade(num4);
		float from = grad(perm[num + perm[num2 + perm[num3]]], num4, num5, num6);
		float to = grad(perm[num + perm[num2 + perm[num9]]], num4, num5, z2);
		float from2 = lerp(from, to, t);
		float from3 = grad(perm[num + perm[num8 + perm[num3]]], num4, y2, num6);
		to = grad(perm[num + perm[num8 + perm[num9]]], num4, y2, z2);
		float to2 = lerp(from3, to, t);
		float from4 = lerp(from2, to2, t2);
		float from5 = grad(perm[num7 + perm[num2 + perm[num3]]], x2, num5, num6);
		to = grad(perm[num7 + perm[num2 + perm[num9]]], x2, num5, z2);
		float from6 = lerp(from5, to, t);
		float from7 = grad(perm[num7 + perm[num8 + perm[num3]]], x2, y2, num6);
		to = grad(perm[num7 + perm[num8 + perm[num9]]], x2, y2, z2);
		to2 = lerp(from7, to, t);
		float to3 = lerp(from6, to2, t2);
		return 0.936f * lerp(from4, to3, t3);
	}

	public static float simplexUnoptimized(float x, float y, float z)
	{
		float num = (x + y + z) * F3;
		float x2 = x + num;
		float x3 = y + num;
		float x4 = z + num;
		int num2 = floor(x2);
		int num3 = floor(x3);
		int num4 = floor(x4);
		float num5 = (float)(num2 + num3 + num4) * G3;
		float num6 = (float)num2 - num5;
		float num7 = (float)num3 - num5;
		float num8 = (float)num4 - num5;
		float num9 = x - num6;
		float num10 = y - num7;
		float num11 = z - num8;
		int num12;
		int num13;
		int num14;
		int num15;
		int num16;
		int num17;
		if (num9 >= num10)
		{
			if (num10 >= num11)
			{
				num12 = 1;
				num13 = 0;
				num14 = 0;
				num15 = 1;
				num16 = 1;
				num17 = 0;
			}
			else if (num9 >= num11)
			{
				num12 = 1;
				num13 = 0;
				num14 = 0;
				num15 = 1;
				num16 = 0;
				num17 = 1;
			}
			else
			{
				num12 = 0;
				num13 = 0;
				num14 = 1;
				num15 = 1;
				num16 = 0;
				num17 = 1;
			}
		}
		else if (num10 < num11)
		{
			num12 = 0;
			num13 = 0;
			num14 = 1;
			num15 = 0;
			num16 = 1;
			num17 = 1;
		}
		else if (num9 < num11)
		{
			num12 = 0;
			num13 = 1;
			num14 = 0;
			num15 = 0;
			num16 = 1;
			num17 = 1;
		}
		else
		{
			num12 = 0;
			num13 = 1;
			num14 = 0;
			num15 = 1;
			num16 = 1;
			num17 = 0;
		}
		float num18 = num9 - (float)num12 + G3;
		float num19 = num10 - (float)num13 + G3;
		float num20 = num11 - (float)num14 + G3;
		float num21 = num9 - (float)num15 + 2f * G3;
		float num22 = num10 - (float)num16 + 2f * G3;
		float num23 = num11 - (float)num17 + 2f * G3;
		float num24 = num9 - 1f + 3f * G3;
		float num25 = num10 - 1f + 3f * G3;
		float num26 = num11 - 1f + 3f * G3;
		int num27 = num2 & 0xFF;
		int num28 = num3 & 0xFF;
		int num29 = num4 & 0xFF;
		float num30 = 0.6f - num9 * num9 - num10 * num10 - num11 * num11;
		float num31;
		if (num30 < 0f)
		{
			num31 = 0f;
		}
		else
		{
			num30 *= num30;
			num31 = num30 * num30 * grad(perm[num27 + perm[num28 + perm[num29]]], num9, num10, num11);
		}
		float num32 = 0.6f - num18 * num18 - num19 * num19 - num20 * num20;
		float num33;
		if (num32 < 0f)
		{
			num33 = 0f;
		}
		else
		{
			num32 *= num32;
			num33 = num32 * num32 * grad(perm[num27 + num12 + perm[num28 + num13 + perm[num29 + num14]]], num18, num19, num20);
		}
		float num34 = 0.6f - num21 * num21 - num22 * num22 - num23 * num23;
		float num35;
		if (num34 < 0f)
		{
			num35 = 0f;
		}
		else
		{
			num34 *= num34;
			num35 = num34 * num34 * grad(perm[num27 + num15 + perm[num28 + num16 + perm[num29 + num17]]], num21, num22, num23);
		}
		float num36 = 0.6f - num24 * num24 - num25 * num25 - num26 * num26;
		float num37;
		if (num36 < 0f)
		{
			num37 = 0f;
		}
		else
		{
			num36 *= num36;
			num37 = num36 * num36 * grad(perm[num27 + 1 + perm[num28 + 1 + perm[num29 + 1]]], num24, num25, num26);
		}
		return 32f * (num31 + num33 + num35 + num37);
	}
}
