namespace APNGLib;

public class pHYsChunk : PNGChunk
{
	public const string NAME = "pHYs";

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = PNGUtils.GetBytes(PixelsPerUnitXAxis);
			byte[] bytes2 = PNGUtils.GetBytes(PixelsPerUnitYAxis);
			byte[] array = new byte[1] { Unit };
			return PNGUtils.Combine(bytes, bytes2, array);
		}
		set
		{
			int offset = 0;
			PixelsPerUnitXAxis = PNGUtils.ParseUint(value, ref offset);
			PixelsPerUnitYAxis = PNGUtils.ParseUint(value, ref offset);
			Unit = PNGUtils.ParseByte(value, ref offset);
		}
	}

	public uint PixelsPerUnitXAxis { get; set; }

	public uint PixelsPerUnitYAxis { get; set; }

	public byte Unit { get; set; }

	public pHYsChunk()
		: base("pHYs")
	{
	}
}
