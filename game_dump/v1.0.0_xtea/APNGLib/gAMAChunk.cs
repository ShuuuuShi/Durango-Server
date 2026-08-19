namespace APNGLib;

public class gAMAChunk : PNGChunk
{
	public const string NAME = "gAMA";

	public override byte[] ChunkData
	{
		get
		{
			return PNGUtils.GetBytes(Gamma);
		}
		set
		{
			Gamma = PNGUtils.ParseUint(value);
		}
	}

	public uint Gamma { get; set; }

	public gAMAChunk()
		: base("gAMA")
	{
	}
}
