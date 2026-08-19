using System;

public class XXHash : HashFunction
{
	private const uint PRIME32_1 = 2654435761u;

	private const uint PRIME32_2 = 2246822519u;

	private const uint PRIME32_3 = 3266489917u;

	private const uint PRIME32_4 = 668265263u;

	private const uint PRIME32_5 = 374761393u;

	private uint seed;

	public XXHash(int seed)
	{
		this.seed = (uint)seed;
	}

	public uint GetHash(byte[] buf)
	{
		int i = 0;
		int num = buf.Length;
		uint num3;
		if (num >= 16)
		{
			int num2 = num - 16;
			uint value = (uint)((int)seed + -1640531535 + -2048144777);
			uint value2 = seed + 2246822519u;
			uint value3 = seed;
			uint value4 = seed - 2654435761u;
			do
			{
				value = CalcSubHash(value, buf, i);
				i += 4;
				value2 = CalcSubHash(value2, buf, i);
				i += 4;
				value3 = CalcSubHash(value3, buf, i);
				i += 4;
				value4 = CalcSubHash(value4, buf, i);
				i += 4;
			}
			while (i <= num2);
			num3 = RotateLeft(value, 1) + RotateLeft(value2, 7) + RotateLeft(value3, 12) + RotateLeft(value4, 18);
		}
		else
		{
			num3 = seed + 374761393;
		}
		num3 += (uint)num;
		for (; i <= num - 4; i += 4)
		{
			num3 += (uint)((int)BitConverter.ToUInt32(buf, i) * -1028477379);
			num3 = RotateLeft(num3, 17) * 668265263;
		}
		for (; i < num; i++)
		{
			num3 += (uint)(buf[i] * 374761393);
			num3 = RotateLeft(num3, 11) * 2654435761u;
		}
		num3 ^= num3 >> 15;
		num3 *= 2246822519u;
		num3 ^= num3 >> 13;
		num3 *= 3266489917u;
		return num3 ^ (num3 >> 16);
	}

	public uint GetHash(params uint[] buf)
	{
		int i = 0;
		int num = buf.Length;
		uint num3;
		if (num >= 4)
		{
			int num2 = num - 4;
			uint value = (uint)((int)seed + -1640531535 + -2048144777);
			uint value2 = seed + 2246822519u;
			uint value3 = seed;
			uint value4 = seed - 2654435761u;
			do
			{
				value = CalcSubHash(value, buf[i]);
				i++;
				value2 = CalcSubHash(value2, buf[i]);
				i++;
				value3 = CalcSubHash(value3, buf[i]);
				i++;
				value4 = CalcSubHash(value4, buf[i]);
				i++;
			}
			while (i <= num2);
			num3 = RotateLeft(value, 1) + RotateLeft(value2, 7) + RotateLeft(value3, 12) + RotateLeft(value4, 18);
		}
		else
		{
			num3 = seed + 374761393;
		}
		num3 += (uint)(num * 4);
		for (; i < num; i++)
		{
			num3 += (uint)((int)buf[i] * -1028477379);
			num3 = RotateLeft(num3, 17) * 668265263;
		}
		num3 ^= num3 >> 15;
		num3 *= 2246822519u;
		num3 ^= num3 >> 13;
		num3 *= 3266489917u;
		return num3 ^ (num3 >> 16);
	}

	public override uint GetHash(params int[] buf)
	{
		int i = 0;
		int num = buf.Length;
		uint num3;
		if (num >= 4)
		{
			int num2 = num - 4;
			uint value = (uint)((int)seed + -1640531535 + -2048144777);
			uint value2 = seed + 2246822519u;
			uint value3 = seed;
			uint value4 = seed - 2654435761u;
			do
			{
				value = CalcSubHash(value, (uint)buf[i]);
				i++;
				value2 = CalcSubHash(value2, (uint)buf[i]);
				i++;
				value3 = CalcSubHash(value3, (uint)buf[i]);
				i++;
				value4 = CalcSubHash(value4, (uint)buf[i]);
				i++;
			}
			while (i <= num2);
			num3 = RotateLeft(value, 1) + RotateLeft(value2, 7) + RotateLeft(value3, 12) + RotateLeft(value4, 18);
		}
		else
		{
			num3 = seed + 374761393;
		}
		num3 += (uint)(num * 4);
		for (; i < num; i++)
		{
			num3 += (uint)(buf[i] * -1028477379);
			num3 = RotateLeft(num3, 17) * 668265263;
		}
		num3 ^= num3 >> 15;
		num3 *= 2246822519u;
		num3 ^= num3 >> 13;
		num3 *= 3266489917u;
		return num3 ^ (num3 >> 16);
	}

	public override uint GetHash(int buf)
	{
		uint num = seed + 374761393;
		num += 4;
		num += (uint)(buf * -1028477379);
		num = RotateLeft(num, 17) * 668265263;
		num ^= num >> 15;
		num *= 2246822519u;
		num ^= num >> 13;
		num *= 3266489917u;
		return num ^ (num >> 16);
	}

	private static uint CalcSubHash(uint value, byte[] buf, int index)
	{
		uint num = BitConverter.ToUInt32(buf, index);
		value += (uint)((int)num * -2048144777);
		value = RotateLeft(value, 13);
		value *= 2654435761u;
		return value;
	}

	private static uint CalcSubHash(uint value, uint read_value)
	{
		value += (uint)((int)read_value * -2048144777);
		value = RotateLeft(value, 13);
		value *= 2654435761u;
		return value;
	}

	private static uint RotateLeft(uint value, int count)
	{
		return (value << count) | (value >> 32 - count);
	}
}
