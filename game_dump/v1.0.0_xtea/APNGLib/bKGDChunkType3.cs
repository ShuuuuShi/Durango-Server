namespace APNGLib;

public class bKGDChunkType3 : bKGDChunk
{
	public override byte[] ChunkData
	{
		get
		{
			return PNGUtils.GetBytes(PaletteIndex);
		}
		set
		{
			PaletteIndex = PNGUtils.ParseByte(value);
		}
	}

	public byte PaletteIndex { get; set; }
}
