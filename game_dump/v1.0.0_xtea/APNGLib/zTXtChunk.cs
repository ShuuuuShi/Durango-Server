using System;
using System.Text;

namespace APNGLib;

public class zTXtChunk : PNGChunk
{
	public const string NAME = "zTXt";

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = Encoding.UTF8.GetBytes(Keyword);
			return PNGUtils.Combine(bytes, PNGChunk.NullSeparator, new byte[1] { CompressionMethod }, TextDatastream);
		}
		set
		{
			int offset = 0;
			Keyword = PNGUtils.ParseString(value, ref offset);
			string keyword = Keyword;
			foreach (char c in keyword)
			{
				if (!PNGChunk.IsPrintable(c))
				{
					throw new ApplicationException("Non-printable character in zTXt chunk keyword");
				}
			}
			CompressionMethod = PNGUtils.ParseByte(value, ref offset);
			TextDatastream = new byte[value.Length - offset];
			Array.Copy(value, offset, TextDatastream, 0, value.Length - offset);
		}
	}

	public string Keyword { get; set; }

	public byte CompressionMethod { get; set; }

	public byte[] TextDatastream { get; set; }

	public zTXtChunk()
		: base("zTXt")
	{
	}
}
