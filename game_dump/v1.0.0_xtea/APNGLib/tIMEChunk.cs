namespace APNGLib;

public class tIMEChunk : PNGChunk
{
	public const string NAME = "tIME";

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = PNGUtils.GetBytes(Year);
			byte[] array = new byte[5] { Month, Day, Hour, Minute, Second };
			return PNGUtils.Combine(bytes, array);
		}
		set
		{
			int offset = 0;
			Year = PNGUtils.ParseUshort(value, ref offset);
			Month = PNGUtils.ParseByte(value, ref offset);
			Day = PNGUtils.ParseByte(value, ref offset);
			Hour = PNGUtils.ParseByte(value, ref offset);
			Minute = PNGUtils.ParseByte(value, ref offset);
			Second = PNGUtils.ParseByte(value, ref offset);
		}
	}

	public ushort Year { get; set; }

	public byte Month { get; set; }

	public byte Day { get; set; }

	public byte Hour { get; set; }

	public byte Minute { get; set; }

	public byte Second { get; set; }

	public tIMEChunk()
		: base("tIME")
	{
	}
}
