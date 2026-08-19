namespace APNGLib;

public class bKGDChunkType6 : bKGDChunk
{
	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = PNGUtils.GetBytes(Red);
			byte[] bytes2 = PNGUtils.GetBytes(Green);
			byte[] bytes3 = PNGUtils.GetBytes(Blue);
			return PNGUtils.Combine(bytes, bytes2, bytes3);
		}
		set
		{
			int offset = 0;
			Red = PNGUtils.ParseUshort(value, ref offset);
			Green = PNGUtils.ParseUshort(value, ref offset);
			Blue = PNGUtils.ParseUshort(value, ref offset);
		}
	}

	public ushort Red { get; set; }

	public ushort Green { get; set; }

	public ushort Blue { get; set; }
}
