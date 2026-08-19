namespace APNGLib;

public class tRNSChunkType3 : tRNSChunk
{
	public override byte[] ChunkData
	{
		get
		{
			return AlphaSettings;
		}
		set
		{
			AlphaSettings = value;
		}
	}

	public byte[] AlphaSettings { get; set; }
}
