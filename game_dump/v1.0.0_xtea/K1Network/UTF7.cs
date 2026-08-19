namespace K1Network;

public static class UTF7
{
	public static int Encode(byte[] buffer, ulong num)
	{
		int result = 0;
		byte b;
		while (true)
		{
			b = (byte)(num & 0x7F);
			num >>= 7;
			if (num == 0L)
			{
				break;
			}
			buffer[result++] = (byte)(b | 0x80u);
		}
		buffer[result++] = b;
		return result;
	}

	public static int Decode(byte[] buffer, out ulong num)
	{
		ulong num2 = 0uL;
		int num3 = 0;
		byte b = 128;
		int num4 = 0;
		while ((b & 0x80u) != 0)
		{
			b = buffer[num4++];
			num2 |= (ulong)((long)(b & 0x7F) << num3);
			num3 += 7;
		}
		num = num2;
		return (num4 <= 8) ? num4 : 8;
	}
}
