namespace APNGLib;

public class IENDChunk : PNGChunk
{
	public const string NAME = "IEND";

	public override byte[] ChunkData
	{
		get
		{
			return new byte[0];
		}
		set
		{
		}
	}

	public IENDChunk()
		: base("IEND")
	{
	}
}
