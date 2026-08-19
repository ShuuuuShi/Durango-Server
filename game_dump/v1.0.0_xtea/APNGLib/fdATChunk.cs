using System;

namespace APNGLib;

public class fdATChunk : PNGChunk
{
	public const string NAME = "fdAT";

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = PNGUtils.GetBytes(SequenceNumber);
			return PNGUtils.Combine(bytes, FrameData);
		}
		set
		{
			int offset = 0;
			SequenceNumber = PNGUtils.ParseUint(value, ref offset);
			FrameData = new byte[value.Length - offset];
			Array.Copy(value, offset, FrameData, 0, value.Length - offset);
		}
	}

	public uint SequenceNumber { get; set; }

	public byte[] FrameData { get; set; }

	public fdATChunk()
		: base("fdAT")
	{
	}
}
