using System;
using System.Collections.Generic;

namespace APNGLib;

public class PLTEChunk : PNGChunk
{
	public class Entry
	{
		public byte Red;

		public byte Green;

		public byte Blue;
	}

	public const string NAME = "PLTE";

	public override byte[] ChunkData
	{
		get
		{
			byte[] array = new byte[0];
			foreach (Entry paletteEntry in PaletteEntries)
			{
				byte[] array2 = new byte[3] { paletteEntry.Red, paletteEntry.Green, paletteEntry.Blue };
				array = PNGUtils.Combine(array, array2);
			}
			return array;
		}
		set
		{
			if (value.Length % 3 != 0)
			{
				throw new ApplicationException("PLTE chunk length not divisible by 3");
			}
			int offset = 0;
			while (offset < value.Length)
			{
				Entry entry = new Entry();
				entry.Red = PNGUtils.ParseByte(value, ref offset);
				entry.Green = PNGUtils.ParseByte(value, ref offset);
				entry.Blue = PNGUtils.ParseByte(value, ref offset);
				PaletteEntries.Add(entry);
			}
			if (PaletteEntries.Count > 256)
			{
				throw new ApplicationException("Too many entries on PLTE chunk");
			}
		}
	}

	public IList<Entry> PaletteEntries { get; set; }

	public PLTEChunk()
		: base("PLTE")
	{
		PaletteEntries = new List<Entry>();
	}
}
