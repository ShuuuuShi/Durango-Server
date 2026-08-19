namespace APNGLib;

public class sBITChunkType0 : sBITChunk
{
	public override byte[] ChunkData
	{
		get
		{
			return new byte[1] { SignificantGreyscaleBits };
		}
		set
		{
			SignificantGreyscaleBits = PNGUtils.ParseByte(value);
		}
	}

	public byte SignificantGreyscaleBits { get; set; }
}
