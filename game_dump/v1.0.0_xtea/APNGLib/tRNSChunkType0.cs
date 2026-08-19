namespace APNGLib;

public class tRNSChunkType0 : tRNSChunk
{
	public override byte[] ChunkData
	{
		get
		{
			return PNGUtils.GetBytes(GreySample);
		}
		set
		{
			GreySample = PNGUtils.ParseUshort(value);
		}
	}

	public ushort GreySample { get; set; }
}
