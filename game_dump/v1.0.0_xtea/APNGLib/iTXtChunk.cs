using System;
using System.Text;

namespace APNGLib;

public class iTXtChunk : PNGChunk
{
	public const string NAME = "iTXt";

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = Encoding.UTF8.GetBytes(Keyword);
			byte[] bytes2 = Encoding.UTF8.GetBytes(LanguageTag);
			byte[] bytes3 = Encoding.UTF8.GetBytes(TranslatedKeyword);
			byte[] bytes4 = Encoding.UTF8.GetBytes(Text);
			return PNGUtils.Combine(bytes, PNGChunk.NullSeparator, new byte[2] { CompressionFlag, CompressionMethod }, bytes2, PNGChunk.NullSeparator, bytes3, PNGChunk.NullSeparator, bytes4);
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
					throw new ApplicationException("Non-printable character in iTXt chunk keyword");
				}
			}
			CompressionFlag = PNGUtils.ParseByte(value, ref offset);
			CompressionMethod = PNGUtils.ParseByte(value, ref offset);
			LanguageTag = PNGUtils.ParseString(value, ref offset);
			TranslatedKeyword = PNGUtils.ParseString(value, ref offset);
			Text = PNGUtils.ParseString(value, ref offset);
		}
	}

	public string Keyword { get; set; }

	public byte CompressionFlag { get; set; }

	public byte CompressionMethod { get; set; }

	public string LanguageTag { get; set; }

	public string TranslatedKeyword { get; set; }

	public string Text { get; set; }

	public iTXtChunk()
		: base("iTXt")
	{
	}
}
