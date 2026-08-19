namespace APNGLib;

public class acTLChunk : PNGChunk
{
	public const string NAME = "acTL";

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = PNGUtils.GetBytes(NumFrames);
			byte[] bytes2 = PNGUtils.GetBytes(NumPlays);
			return PNGUtils.Combine(bytes, bytes2);
		}
		set
		{
			int offset = 0;
			NumFrames = PNGUtils.ParseUint(value, ref offset);
			NumPlays = PNGUtils.ParseUint(value, ref offset);
		}
	}

	public uint NumFrames { get; set; }

	public uint NumPlays { get; set; }

	public acTLChunk()
		: base("acTL")
	{
	}
}
