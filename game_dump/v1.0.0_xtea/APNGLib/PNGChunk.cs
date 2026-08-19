using System;
using System.Text;

namespace APNGLib;

public class PNGChunk
{
	private byte[] data;

	protected static readonly byte[] NullSeparator = new byte[1];

	public uint ChunkLength => (uint)ChunkData.Length;

	public string ChunkType { get; set; }

	public virtual byte[] ChunkData
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public uint ChunkCRC => CalculateCRC();

	public byte[] Chunk
	{
		get
		{
			int num = 0;
			byte[] chunkData = ChunkData;
			byte[] bytes = PNGUtils.GetBytes((uint)chunkData.Length);
			byte[] bytes2 = Encoding.UTF8.GetBytes(ChunkType);
			byte[] bytes3 = PNGUtils.GetBytes(CalculateCRC());
			int num2 = bytes.Length + bytes2.Length + chunkData.Length + bytes3.Length;
			byte[] array = new byte[num2];
			Array.Copy(bytes, 0, array, num, bytes.Length);
			num += bytes.Length;
			Array.Copy(bytes2, 0, array, num, bytes2.Length);
			num += bytes2.Length;
			Array.Copy(chunkData, 0, array, num, chunkData.Length);
			num += chunkData.Length;
			Array.Copy(bytes3, 0, array, num, bytes3.Length);
			return array;
		}
	}

	public PNGChunk()
	{
	}

	public PNGChunk(string Type)
	{
		ChunkType = Type;
	}

	public uint CalculateCRC()
	{
		uint crc = uint.MaxValue;
		crc = CRC.UpdateCRC(crc, Encoding.UTF8.GetBytes(ChunkType));
		crc = CRC.UpdateCRC(crc, ChunkData);
		return ~crc;
	}

	protected static bool IsPrintable(char c)
	{
		return (c >= ' ' && c <= '~') || (c >= '¡' && c <= 'ÿ');
	}
}
