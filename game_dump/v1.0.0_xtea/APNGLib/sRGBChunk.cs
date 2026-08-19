namespace APNGLib;

public class sRGBChunk : PNGChunk
{
	public const string NAME = "sRGB";

	public override byte[] ChunkData
	{
		get
		{
			return new byte[1] { RenderingIntent };
		}
		set
		{
			RenderingIntent = PNGUtils.ParseByte(value);
		}
	}

	public byte RenderingIntent { get; set; }

	public sRGBChunk()
		: base("sRGB")
	{
	}
}
