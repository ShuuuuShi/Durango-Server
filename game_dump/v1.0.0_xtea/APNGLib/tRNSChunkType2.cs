namespace APNGLib;

public class tRNSChunkType2 : tRNSChunk
{
	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = PNGUtils.GetBytes(RedSample);
			byte[] bytes2 = PNGUtils.GetBytes(BlueSample);
			byte[] bytes3 = PNGUtils.GetBytes(GreenSample);
			return PNGUtils.Combine(bytes, bytes2, bytes3);
		}
		set
		{
			int offset = 0;
			RedSample = PNGUtils.ParseUshort(value, ref offset);
			BlueSample = PNGUtils.ParseUshort(value, ref offset);
			GreenSample = PNGUtils.ParseUshort(value, ref offset);
		}
	}

	public ushort RedSample { get; set; }

	public ushort BlueSample { get; set; }

	public ushort GreenSample { get; set; }
}
