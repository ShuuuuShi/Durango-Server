using System;
using System.Linq;

namespace APNGLib;

public class fcTLChunk : PNGChunk
{
	public const string NAME = "fcTL";

	private ushort[] AcceptedDisposeOps = new ushort[3] { 0, 1, 2 };

	private ushort[] AcceptedBlendOps = new ushort[2] { 0, 1 };

	public override byte[] ChunkData
	{
		get
		{
			byte[] bytes = PNGUtils.GetBytes(SequenceNumber);
			byte[] bytes2 = PNGUtils.GetBytes(Width);
			byte[] bytes3 = PNGUtils.GetBytes(Height);
			byte[] bytes4 = PNGUtils.GetBytes(XOffset);
			byte[] bytes5 = PNGUtils.GetBytes(YOffset);
			byte[] bytes6 = PNGUtils.GetBytes(DelayNumerator);
			byte[] bytes7 = PNGUtils.GetBytes(DelayDenominator);
			byte[] bytes8 = PNGUtils.GetBytes(DisposeOperation);
			byte[] bytes9 = PNGUtils.GetBytes(BlendOperation);
			return PNGUtils.Combine(bytes, bytes2, bytes3, bytes4, bytes5, bytes6, bytes7, bytes8, bytes9);
		}
		set
		{
			int offset = 0;
			SequenceNumber = PNGUtils.ParseUint(value, ref offset);
			Width = PNGUtils.ParseUint(value, ref offset);
			Height = PNGUtils.ParseUint(value, ref offset);
			XOffset = PNGUtils.ParseUint(value, ref offset);
			YOffset = PNGUtils.ParseUint(value, ref offset);
			DelayNumerator = PNGUtils.ParseUshort(value, ref offset);
			DelayDenominator = PNGUtils.ParseUshort(value, ref offset);
			DisposeOperation = PNGUtils.ParseByte(value, ref offset);
			BlendOperation = PNGUtils.ParseByte(value, ref offset);
			if (XOffset < 0 || YOffset < 0 || Width == 0 || Height == 0)
			{
				throw new ApplicationException("Frame size cannot be understood");
			}
			if (!AcceptedDisposeOps.Contains(DisposeOperation))
			{
				throw new ApplicationException("Dispose Operation not supported");
			}
			if (!AcceptedBlendOps.Contains(BlendOperation))
			{
				throw new ApplicationException("Blend Operation not supported");
			}
		}
	}

	public uint SequenceNumber { get; set; }

	public uint Width { get; set; }

	public uint Height { get; set; }

	public uint XOffset { get; set; }

	public uint YOffset { get; set; }

	public ushort DelayNumerator { get; set; }

	public ushort DelayDenominator { get; set; }

	public byte DisposeOperation { get; set; }

	public byte BlendOperation { get; set; }

	public fcTLChunk()
		: base("fcTL")
	{
	}
}
