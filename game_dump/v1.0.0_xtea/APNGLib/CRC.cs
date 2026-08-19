namespace APNGLib;

internal class CRC
{
	public const uint INITIAL_CRC = uint.MaxValue;

	private static uint[] crcTable;

	private static void MakeCRCTable()
	{
		crcTable = new uint[256];
		for (uint num = 0u; num < crcTable.Length; num++)
		{
			uint num2 = num;
			for (uint num3 = 0u; num3 < 8; num3++)
			{
				num2 = (((num2 & 1) == 0) ? (num2 >> 1) : (0xEDB88320u ^ (num2 >> 1)));
			}
			crcTable[num] = num2;
		}
	}

	public static uint UpdateCRC(uint crc, byte[] bytes)
	{
		uint num = crc;
		if (crcTable == null)
		{
			MakeCRCTable();
		}
		for (uint num2 = 0u; num2 < bytes.Length; num2++)
		{
			num = crcTable[(num ^ bytes[num2]) & 0xFF] ^ (num >> 8);
		}
		return num;
	}

	public static uint Calculate(byte[] bytes)
	{
		return UpdateCRC(uint.MaxValue, bytes);
	}
}
