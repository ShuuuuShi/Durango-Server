namespace APNGLib;

public class cHRMChunk : PNGChunk
{
	public const string NAME = "cHRM";

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = PNGUtils.GetBytes(WhitePointX);
			byte[] bytes2 = PNGUtils.GetBytes(WhitePointY);
			byte[] bytes3 = PNGUtils.GetBytes(RedX);
			byte[] bytes4 = PNGUtils.GetBytes(RedY);
			byte[] bytes5 = PNGUtils.GetBytes(GreenX);
			byte[] bytes6 = PNGUtils.GetBytes(GreenY);
			byte[] bytes7 = PNGUtils.GetBytes(BlueX);
			byte[] bytes8 = PNGUtils.GetBytes(BlueY);
			return PNGUtils.Combine(bytes, bytes2, bytes3, bytes4, bytes5, bytes6, bytes7, bytes8);
		}
		set
		{
			int offset = 0;
			WhitePointX = PNGUtils.ParseUint(value, ref offset);
			WhitePointY = PNGUtils.ParseUint(value, ref offset);
			RedX = PNGUtils.ParseUint(value, ref offset);
			RedY = PNGUtils.ParseUint(value, ref offset);
			GreenX = PNGUtils.ParseUint(value, ref offset);
			GreenY = PNGUtils.ParseUint(value, ref offset);
			BlueX = PNGUtils.ParseUint(value, ref offset);
			BlueY = PNGUtils.ParseUint(value, ref offset);
		}
	}

	public uint WhitePointX { get; set; }

	public uint WhitePointY { get; set; }

	public uint RedX { get; set; }

	public uint RedY { get; set; }

	public uint GreenX { get; set; }

	public uint GreenY { get; set; }

	public uint BlueX { get; set; }

	public uint BlueY { get; set; }

	public cHRMChunk()
		: base("cHRM")
	{
	}
}
