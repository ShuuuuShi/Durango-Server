using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace APNGLib;

public class APNG : PNG
{
	protected IList<Frame> frames;

	public acTLChunk acTL { get; set; }

	public int FrameCount => frames.Count;

	public bool IsAnimated => frames.Count > 0;

	public uint MaxPlays => acTL.NumPlays;

	public APNG()
	{
		frames = new List<Frame>();
		chunks = new HashSet<PNGChunk>();
	}

	public void AddFrame(Frame f)
	{
		frames.Add(f);
	}

	public void RemoveFrame(Frame f)
	{
		frames.Remove(f);
	}

	public override Stream ToStream()
	{
		Validate();
		Stream stream = new MemoryStream();
		WriteSignature(stream);
		PNG.WriteChunk(stream, base.IHDR);
		PNG.WriteChunk(stream, acTL);
		WriteAncillaryChunks(stream);
		Frame frame = frames.First();
		if (frame.IFrame)
		{
			PNG.WriteChunk(stream, frame.fctl);
			foreach (IDATChunk iDAT in frame.IDATs)
			{
				PNG.WriteChunk(stream, iDAT);
			}
		}
		else
		{
			foreach (IDATChunk iDAT2 in base.IDATList)
			{
				PNG.WriteChunk(stream, iDAT2);
			}
			PNG.WriteChunk(stream, frame.fctl);
			foreach (fdATChunk fdAT in frame.fdATs)
			{
				PNG.WriteChunk(stream, fdAT);
			}
		}
		foreach (Frame item in frames.Skip(1))
		{
			PNG.WriteChunk(stream, item.fctl);
			foreach (fdATChunk fdAT2 in item.fdATs)
			{
				PNG.WriteChunk(stream, fdAT2);
			}
		}
		PNG.WriteChunk(stream, base.IEND);
		stream.Seek(0L, SeekOrigin.Begin);
		return stream;
	}

	public Stream ToStream(int index)
	{
		Validate();
		Stream stream = new MemoryStream();
		Frame frame = GetFrame(index);
		WriteImageData(stream, frame.ImageData, frame.Width, frame.Height);
		stream.Seek(0L, SeekOrigin.Begin);
		return stream;
	}

	public Stream DefaultImageToStream()
	{
		Validate();
		IList<byte> list = new List<byte>();
		foreach (IDATChunk iDAT in base.IDATList)
		{
			byte[] imageData = iDAT.ImageData;
			foreach (byte item in imageData)
			{
				list.Add(item);
			}
		}
		Stream stream = new MemoryStream();
		WriteImageData(stream, list);
		stream.Seek(0L, SeekOrigin.Begin);
		return stream;
	}

	public Frame GetFrame(int index)
	{
		return frames[index];
	}

	public override void Validate()
	{
		base.Validate();
		if (acTL != null && acTL.NumFrames != frames.Count)
		{
			throw new ApplicationException("Number of frames not specified correctly in acTL chunk");
		}
	}

	protected override bool HandleChunk(PNGChunk chunk)
	{
		switch (chunk.ChunkType)
		{
		case "IDAT":
			Handle_IDAT(chunk);
			break;
		case "fcTL":
			Handle_fcTL(chunk);
			break;
		case "fdAT":
			Handle_fdAT(chunk);
			break;
		case "acTL":
			Handle_acTL(chunk);
			break;
		default:
			return base.HandleChunk(chunk);
		}
		return true;
	}

	private void Handle_acTL(PNGChunk chunk)
	{
		if (acTL != null)
		{
			throw new ApplicationException("acTL defined more than once");
		}
		acTL = new acTLChunk();
		acTL.ChunkData = chunk.ChunkData;
	}

	private void Handle_fcTL(PNGChunk chunk)
	{
		bool first = base.IDATList.Count < 1;
		fcTLChunk fcTLChunk2 = new fcTLChunk();
		fcTLChunk2.ChunkData = chunk.ChunkData;
		if (fcTLChunk2.XOffset + fcTLChunk2.Width > base.IHDR.Width || fcTLChunk2.YOffset + fcTLChunk2.Height > base.IHDR.Height)
		{
			throw new ApplicationException("Frame is outside of image space");
		}
		Frame item = new Frame(first, fcTLChunk2);
		frames.Add(item);
	}

	private void Handle_fdAT(PNGChunk chunk)
	{
		fdATChunk fdATChunk2 = new fdATChunk();
		fdATChunk2.ChunkData = chunk.ChunkData;
		Frame frame = frames.LastOrDefault();
		if (frame == null)
		{
			throw new ApplicationException("No fctl chunk defined, fdat chunk received out of order");
		}
		frame.AddChunk(fdATChunk2);
	}

	private void Handle_IDAT(PNGChunk chunk)
	{
		IDATChunk iDATChunk = new IDATChunk();
		iDATChunk.ChunkData = chunk.ChunkData;
		base.IDATList.Add(iDATChunk);
		if (frames.Count > 1)
		{
			throw new ApplicationException("IDAT chunk encountered out of order");
		}
		if (frames.Count == 1)
		{
			Frame frame = frames.First();
			frame.AddChunk(iDATChunk);
		}
	}
}
