namespace APNGLib;

public class sBITChunkType6 : sBITChunk
{
	public override byte[] ChunkData
	{
		get
		{
			return new byte[4] { SignificantRedBits, SignificantGreenBits, SignificantBlueBits, SignificantAlphaBits };
		}
		set
		{
			int offset = 0;
			SignificantRedBits = PNGUtils.ParseByte(value, ref offset);
			SignificantGreenBits = PNGUtils.ParseByte(value, ref offset);
			SignificantBlueBits = PNGUtils.ParseByte(value, ref offset);
			SignificantAlphaBits = PNGUtils.ParseByte(value, ref offset);
		}
	}

	public byte SignificantRedBits { get; set; }

	public byte SignificantGreenBits { get; set; }

	public byte SignificantBlueBits { get; set; }

	public byte SignificantAlphaBits { get; set; }
}
