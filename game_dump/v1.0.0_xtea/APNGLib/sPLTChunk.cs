using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace APNGLib;

public class sPLTChunk : PNGChunk
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SuggestedPalette
	{
		public ushort Red { get; set; }

		public ushort Green { get; set; }

		public ushort Blue { get; set; }

		public ushort Alpha { get; set; }

		public ushort Frequency { get; set; }
	}

	public const string NAME = "sPLT";

	public ICollection<SuggestedPalette> palettes;

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = Encoding.UTF8.GetBytes(Name);
			byte[] array = new byte[1] { SampleDepth };
			byte[] array2 = PNGUtils.Combine(bytes, array);
			foreach (SuggestedPalette palette in palettes)
			{
				if (SampleDepth == 8)
				{
					array2 = PNGUtils.Combine(array2, new byte[4]
					{
						(byte)palette.Red,
						(byte)palette.Green,
						(byte)palette.Blue,
						(byte)palette.Alpha
					});
				}
				else if (SampleDepth == 16)
				{
					byte[] bytes2 = PNGUtils.GetBytes(palette.Red);
					byte[] bytes3 = PNGUtils.GetBytes(palette.Green);
					byte[] bytes4 = PNGUtils.GetBytes(palette.Blue);
					byte[] bytes5 = PNGUtils.GetBytes(palette.Alpha);
					array2 = PNGUtils.Combine(array2, bytes2, bytes3, bytes4, bytes5);
				}
				byte[] bytes6 = PNGUtils.GetBytes(palette.Frequency);
				array2 = PNGUtils.Combine(array2, bytes6);
			}
			return array2;
		}
		set
		{
			int offset = 0;
			Name = PNGUtils.ParseString(value, ref offset);
			string name = Name;
			foreach (char c in name)
			{
				if (!PNGChunk.IsPrintable(c))
				{
					throw new ApplicationException("Non-printable characters in sPLT chunk name");
				}
			}
			SampleDepth = PNGUtils.ParseByte(value, ref offset);
			while (offset < value.Length)
			{
				SuggestedPalette item = default(SuggestedPalette);
				if (SampleDepth == 8)
				{
					item.Red = PNGUtils.ParseByte(value, ref offset);
					item.Green = PNGUtils.ParseByte(value, ref offset);
					item.Blue = PNGUtils.ParseByte(value, ref offset);
					item.Alpha = PNGUtils.ParseByte(value, ref offset);
				}
				else
				{
					if (SampleDepth != 16)
					{
						throw new ApplicationException("Suggest Palette Sample Depth not 8 or 16");
					}
					item.Red = PNGUtils.ParseUshort(value, ref offset);
					item.Green = PNGUtils.ParseUshort(value, ref offset);
					item.Blue = PNGUtils.ParseUshort(value, ref offset);
					item.Alpha = PNGUtils.ParseUshort(value, ref offset);
				}
				item.Frequency = PNGUtils.ParseUshort(value, ref offset);
				palettes.Add(item);
			}
		}
	}

	public string Name { get; set; }

	public byte SampleDepth { get; set; }

	public sPLTChunk()
		: base("sPLT")
	{
		palettes = new List<SuggestedPalette>();
	}
}
