using System;
using System.Text;

namespace APNGLib;

public class iCCPChunk : PNGChunk
{
	public const string NAME = "iCCP";

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = Encoding.UTF8.GetBytes(Name);
			return PNGUtils.Combine(bytes, PNGChunk.NullSeparator, new byte[1] { CompressionMethod }, CompressionProfile);
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
					throw new ApplicationException("Non-printable character in iCCP chunk name");
				}
			}
			CompressionMethod = PNGUtils.ParseByte(value, ref offset);
			CompressionProfile = new byte[value.Length - offset];
			Array.Copy(value, offset, CompressionProfile, 0, CompressionProfile.Length - offset);
		}
	}

	public string Name { get; set; }

	public byte CompressionMethod { get; set; }

	public byte[] CompressionProfile { get; set; }

	public iCCPChunk()
		: base("iCCP")
	{
	}
}
