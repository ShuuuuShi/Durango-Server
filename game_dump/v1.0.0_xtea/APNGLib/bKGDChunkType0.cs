namespace APNGLib;

public class bKGDChunkType0 : bKGDChunk
{
	public override byte[] ChunkData
	{
		get
		{
			return PNGUtils.GetBytes(Greyscale);
		}
		set
		{
			Greyscale = PNGUtils.ParseUshort(value);
		}
	}

	public ushort Greyscale { get; set; }
}
