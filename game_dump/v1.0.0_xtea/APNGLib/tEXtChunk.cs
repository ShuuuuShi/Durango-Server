using System;
using System.Text;

namespace APNGLib;

public class tEXtChunk : PNGChunk
{
	public const string NAME = "tEXt";

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = Encoding.UTF8.GetBytes(Keyword);
			byte[] bytes2 = Encoding.UTF8.GetBytes(Text);
			return PNGUtils.Combine(bytes, PNGChunk.NullSeparator, bytes2);
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
					throw new ApplicationException("Non-printable character in tEXT chunk keyword");
				}
			}
			Text = PNGUtils.ParseString(value, ref offset);
		}
	}

	public string Keyword { get; set; }

	public string Text { get; set; }

	public tEXtChunk()
		: base("tEXt")
	{
	}
}
