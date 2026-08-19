namespace APNGLib;

public class hISTChunk : PNGChunk
{
	public const string NAME = "hIST";

	public override byte[] ChunkData
	{
		get
		{
			byte[] array = new byte[0];
			ushort[] frequency = Frequency;
			foreach (ushort s in frequency)
			{
				byte[] bytes = PNGUtils.GetBytes(s);
				array = PNGUtils.Combine(array, bytes);
			}
			return array;
		}
		set
		{
			int num = value.Length / 2;
			Frequency = new ushort[num];
			int offset = 0;
			for (int i = 0; i < num; i++)
			{
				Frequency[i] = PNGUtils.ParseUshort(value, ref offset);
			}
		}
	}

	public ushort[] Frequency { get; set; }

	public hISTChunk()
		: base("hIST")
	{
	}
}
