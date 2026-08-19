using System;

namespace APNGLib;

public class IHDRChunk : PNGChunk
{
	public const string NAME = "IHDR";

	private static readonly byte[] AllowedColorTypes = new byte[5] { 0, 2, 3, 4, 6 };

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = PNGUtils.GetBytes(Width);
			byte[] bytes2 = PNGUtils.GetBytes(Height);
			byte[] array = new byte[5] { BitDepth, ColorType, CompressionMethod, FilterMethod, InterlaceMethod };
			return PNGUtils.Combine(bytes, bytes2, array);
		}
		set
		{
			int offset = 0;
			Width = PNGUtils.ParseUint(value, ref offset);
			Height = PNGUtils.ParseUint(value, ref offset);
			BitDepth = PNGUtils.ParseByte(value, ref offset);
			ColorType = PNGUtils.ParseByte(value, ref offset);
			if (!AllowedColorTypes.Contains(ColorType))
			{
				throw new ApplicationException("Colour type is not supported");
			}
			CompressionMethod = PNGUtils.ParseByte(value, ref offset);
			FilterMethod = PNGUtils.ParseByte(value, ref offset);
			InterlaceMethod = PNGUtils.ParseByte(value, ref offset);
		}
	}

	public uint Width { get; set; }

	public uint Height { get; set; }

	public byte BitDepth { get; set; }

	public byte ColorType { get; set; }

	public byte CompressionMethod { get; set; }

	public byte FilterMethod { get; set; }

	public byte InterlaceMethod { get; set; }

	public IHDRChunk()
		: base("IHDR")
	{
	}
}
