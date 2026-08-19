using System;
using System.Linq;
using System.Text;

namespace APNGLib;

public static class PNGUtils
{
	public static byte ParseByte(byte[] buffer)
	{
		int offset = 0;
		return ParseByte(buffer, ref offset);
	}

	public static byte ParseByte(byte[] buffer, ref int offset)
	{
		byte result = buffer[offset];
		offset++;
		return result;
	}

	public static ushort ParseUshort(byte[] buffer)
	{
		int offset = 0;
		return ParseUshort(buffer, ref offset);
	}

	public static ushort ParseUshort(byte[] buffer, ref int offset)
	{
		ushort num = 0;
		if (buffer.Length - offset < 2)
		{
			throw new ArgumentException($"buffer is not long enough to extract {2} bytes at offset {offset}");
		}
		int num2 = offset + 2 - 1;
		int num3 = 0;
		while (num2 >= offset)
		{
			num |= (ushort)(buffer[num2] << 8 * num3);
			num2--;
			num3++;
		}
		offset += 2;
		return num;
	}

	public static uint ParseUint(byte[] buffer)
	{
		int offset = 0;
		return ParseUint(buffer, ref offset);
	}

	public static uint ParseUint(byte[] buffer, ref int offset)
	{
		uint num = 0u;
		if (buffer.Length - offset < 4)
		{
			throw new ArgumentException($"buffer is not long enough to extract {4} bytes at offset {offset}");
		}
		int num2 = offset + 4 - 1;
		int num3 = 0;
		while (num2 >= offset)
		{
			num |= (uint)(buffer[num2] << 8 * num3);
			num2--;
			num3++;
		}
		offset += 4;
		return num;
	}

	public static string ParseString(byte[] buffer, int length)
	{
		int offset = 0;
		return ParseString(buffer, ref offset, length);
	}

	public static string ParseString(byte[] buffer, ref int offset, int length)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (buffer.Length - offset < length)
		{
			throw new ArgumentException($"buffer is not long enough to extract {length} bytes at offset {offset}");
		}
		for (int i = offset; i < offset + length; i++)
		{
			stringBuilder.Append((char)buffer[i]);
		}
		offset += length;
		return stringBuilder.ToString();
	}

	public static string ParseString(byte[] buffer)
	{
		int offset = 0;
		return ParseString(buffer, ref offset);
	}

	public static string ParseString(byte[] buffer, ref int offset)
	{
		if (buffer.Length <= offset)
		{
			throw new ArgumentException($"buffer is not long enough to extract string at offset {offset}");
		}
		StringBuilder stringBuilder = new StringBuilder();
		char c = (char)buffer[offset];
		do
		{
			stringBuilder.Append(c);
			c = (char)buffer[++offset];
		}
		while (c != 0 && offset < buffer.Length - 1);
		return stringBuilder.ToString();
	}

	public static byte[] ParseByteArray(byte[] buffer, int length)
	{
		int offset = 0;
		return ParseByteArray(buffer, ref offset, length);
	}

	public static byte[] ParseByteArray(byte[] buffer, ref int offset, int length)
	{
		byte[] array = new byte[length];
		if (buffer.Length - offset < length)
		{
			throw new ArgumentException($"buffer is not long enough to extract {length} bytes at offset {offset}");
		}
		Array.Copy(buffer, offset, array, 0, length);
		return array;
	}

	public static byte[] Combine(params byte[][] arrays)
	{
		byte[] array = new byte[arrays.Sum((byte[] a) => a.Length)];
		int num = 0;
		foreach (byte[] array2 in arrays)
		{
			Buffer.BlockCopy(array2, 0, array, num, array2.Length);
			num += array2.Length;
		}
		return array;
	}

	public static byte[] GetBytes(byte b)
	{
		return new byte[1] { b };
	}

	public static byte[] GetBytes(ushort s)
	{
		byte[] bytes = BitConverter.GetBytes(s);
		Array.Reverse(bytes);
		return bytes;
	}

	public static byte[] GetBytes(uint i)
	{
		byte[] bytes = BitConverter.GetBytes(i);
		Array.Reverse(bytes);
		return bytes;
	}
}
