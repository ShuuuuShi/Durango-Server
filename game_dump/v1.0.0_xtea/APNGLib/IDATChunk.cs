namespace APNGLib;

public class IDATChunk : PNGChunk
{
	public const string NAME = "IDAT";

	public override byte[] ChunkData
	{
		get
		{
			return ImageData;
		}
		set
		{
			ImageData = value;
		}
	}

	public byte[] ImageData { get; set; }

	public IDATChunk()
		: base("IDAT")
	{
	}
}
