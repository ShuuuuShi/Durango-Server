namespace APNGLib;

public class sBITChunkType4 : sBITChunk
{
	public override byte[] ChunkData
	{
		get
		{
			return new byte[2] { SignificantGreyscaleBits, SignificantAlphaBits };
		}
		set
		{
			int offset = 0;
			SignificantGreyscaleBits = PNGUtils.ParseByte(value, ref offset);
			SignificantAlphaBits = PNGUtils.ParseByte(value, ref offset);
		}
	}

	public byte SignificantGreyscaleBits { get; set; }

	public byte SignificantAlphaBits { get; set; }
}
