namespace APNGLib;

public class sBITChunkType2 : sBITChunk
{
	public override byte[] ChunkData
	{
		get
		{
			return new byte[3] { SignificantRedBits, SignificantGreenBits, SignificantBlueBits };
		}
		set
		{
			int offset = 0;
			SignificantRedBits = PNGUtils.ParseByte(value, ref offset);
			SignificantGreenBits = PNGUtils.ParseByte(value, ref offset);
			SignificantBlueBits = PNGUtils.ParseByte(value, ref offset);
		}
	}

	public byte SignificantRedBits { get; set; }

	public byte SignificantGreenBits { get; set; }

	public byte SignificantBlueBits { get; set; }
}
