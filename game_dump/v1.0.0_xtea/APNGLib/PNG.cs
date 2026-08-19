using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace APNGLib;

public class PNG
{
	protected ICollection<PNGChunk> chunks;

	public IHDRChunk IHDR { get; set; }

	public IList<IDATChunk> IDATList { get; private set; }

	public IENDChunk IEND { get; set; }

	public PLTEChunk PLTE { get; set; }

	public tRNSChunk tRNS { get; set; }

	public cHRMChunk cHRM { get; set; }

	public gAMAChunk gAMA { get; set; }

	public iCCPChunk iCCP { get; set; }

	public sBITChunk sBIT { get; set; }

	public sRGBChunk sRGB { get; set; }

	public ICollection<tEXtChunk> tEXtList { get; private set; }

	public ICollection<zTXtChunk> zTXtList { get; private set; }

	public ICollection<iTXtChunk> iTXtList { get; private set; }

	public bKGDChunk bKGD { get; set; }

	public hISTChunk hIST { get; set; }

	public pHYsChunk pHYs { get; set; }

	public sPLTChunk sPLT { get; set; }

	public tIMEChunk tIME { get; set; }

	public uint Width => IHDR.Width;

	public uint Height => IHDR.Height;

	public PNG()
	{
		IDATList = new List<IDATChunk>();
		tEXtList = new HashSet<tEXtChunk>();
		zTXtList = new HashSet<zTXtChunk>();
		iTXtList = new HashSet<iTXtChunk>();
		chunks = new List<PNGChunk>();
	}

	public virtual Stream ToStream()
	{
		Validate();
		IList<byte> list = new List<byte>();
		foreach (IDATChunk iDAT in IDATList)
		{
			byte[] imageData = iDAT.ImageData;
			foreach (byte item in imageData)
			{
				list.Add(item);
			}
		}
		Stream stream = new MemoryStream();
		WriteImageData(stream, list);
		return stream;
	}

	protected void WriteImageData(Stream s, IList<byte> imageData, uint width, uint height)
	{
		WriteSignature(s);
		IHDRChunk iHDRChunk = new IHDRChunk();
		iHDRChunk.ChunkData = IHDR.ChunkData;
		iHDRChunk.Width = width;
		iHDRChunk.Height = height;
		WriteChunk(s, iHDRChunk);
		WriteAncillaryChunks(s);
		IDATChunk iDATChunk = new IDATChunk();
		iDATChunk.ChunkData = imageData.ToArray();
		WriteChunk(s, iDATChunk);
		WriteChunk(s, IEND);
	}

	protected void WriteImageData(Stream s, IList<byte> imageData)
	{
		WriteImageData(s, imageData, IHDR.Width, IHDR.Height);
	}

	protected void WriteSignature(Stream s)
	{
		s.Write(PNGSignature.Signature, 0, PNGSignature.Signature.Length);
	}

	protected void WriteAncillaryChunks(Stream s)
	{
		WriteChunk(s, PLTE);
		WriteChunk(s, tRNS);
		WriteChunk(s, cHRM);
		WriteChunk(s, gAMA);
		WriteChunk(s, iCCP);
		WriteChunk(s, sBIT);
		WriteChunk(s, sRGB);
		WriteChunk(s, bKGD);
		WriteChunk(s, hIST);
		WriteChunk(s, pHYs);
		WriteChunk(s, sPLT);
		WriteChunk(s, tIME);
		foreach (tEXtChunk tEXt in tEXtList)
		{
			WriteChunk(s, tEXt);
		}
		foreach (zTXtChunk zTXt in zTXtList)
		{
			WriteChunk(s, zTXt);
		}
		foreach (iTXtChunk iTXt in iTXtList)
		{
			WriteChunk(s, iTXt);
		}
		foreach (PNGChunk chunk in chunks)
		{
			WriteChunk(s, chunk);
		}
	}

	protected static void WriteChunk(Stream s, PNGChunk chunk)
	{
		if (chunk != null)
		{
			byte[] chunk2 = chunk.Chunk;
			s.Write(chunk2, 0, chunk2.Length);
		}
	}

	protected PNGChunk GetNextChunk(Stream stream)
	{
		PNGChunk pNGChunk = new PNGChunk();
		byte[] buffer = new byte[4];
		stream.Read(buffer, 0, 4);
		uint num = PNGUtils.ParseUint(buffer);
		byte[] buffer2 = new byte[4];
		stream.Read(buffer2, 0, 4);
		pNGChunk.ChunkType = PNGUtils.ParseString(buffer2, 4);
		byte[] array = new byte[num];
		stream.Read(array, 0, (int)num);
		pNGChunk.ChunkData = array;
		byte[] buffer3 = new byte[4];
		stream.Read(buffer3, 0, 4);
		uint num2 = PNGUtils.ParseUint(buffer3);
		uint num3 = pNGChunk.CalculateCRC();
		if (num2 != num3)
		{
			throw new ApplicationException($"APNG Chunk CRC Mismatch.  Chunk CRC = {num2}, Calculated CRC = {num3}.");
		}
		return pNGChunk;
	}

	public void Load(Stream stream)
	{
		byte[] array = new byte[PNGSignature.Signature.Length];
		stream.Read(array, 0, PNGSignature.Signature.Length);
		PNGSignature.Compare(array);
		PNGChunk nextChunk = GetNextChunk(stream);
		if (nextChunk.ChunkType != "IHDR")
		{
			throw new ApplicationException("First chunk is not IHDR chunk");
		}
		Handle_IHDR(nextChunk);
		do
		{
			nextChunk = GetNextChunk(stream);
			if (!HandleChunk(nextChunk))
			{
				HandleDefaultChunk(nextChunk);
			}
		}
		while (nextChunk.ChunkType != "IEND");
		Validate();
	}

	protected virtual bool HandleChunk(PNGChunk chunk)
	{
		switch (chunk.ChunkType)
		{
		case "IHDR":
			Handle_IHDR(chunk);
			break;
		case "PLTE":
			Handle_PLTE(chunk);
			break;
		case "IDAT":
			Handle_IDAT(chunk);
			break;
		case "IEND":
			Handle_IEND(chunk);
			break;
		case "tRNS":
			Handle_tRNS(chunk);
			break;
		case "cHRM":
			Handle_cHRM(chunk);
			break;
		case "gAMA":
			Handle_gAMA(chunk);
			break;
		case "iCCP":
			Handle_iCCP(chunk);
			break;
		case "sBIT":
			Handle_sBIT(chunk);
			break;
		case "sRGB":
			Handle_sRGB(chunk);
			break;
		case "tEXt":
			Handle_tEXt(chunk);
			break;
		case "zTXt":
			Handle_zTXt(chunk);
			break;
		case "iTXt":
			Handle_iTXt(chunk);
			break;
		case "bKGD":
			Handle_bKGD(chunk);
			break;
		case "hIST":
			Handle_hIST(chunk);
			break;
		case "pHYs":
			Handle_pHYs(chunk);
			break;
		case "sPLT":
			Handle_sPLT(chunk);
			break;
		case "tIME":
			Handle_tIME(chunk);
			break;
		default:
			return false;
		}
		return true;
	}

	private void Handle_tIME(PNGChunk chunk)
	{
		if (tIME != null)
		{
			throw new ApplicationException("tIME chunk encountered more than once");
		}
		tIME = new tIMEChunk();
		tIME.ChunkData = chunk.ChunkData;
	}

	private void Handle_sPLT(PNGChunk chunk)
	{
		if (sPLT != null)
		{
			throw new ApplicationException("sPLT chunk encountered more than once");
		}
		sPLT = new sPLTChunk();
		sPLT.ChunkData = chunk.ChunkData;
	}

	private void Handle_pHYs(PNGChunk chunk)
	{
		if (pHYs != null)
		{
			throw new ApplicationException("pHYs chunk encountered more than once");
		}
		pHYs = new pHYsChunk();
		pHYs.ChunkData = chunk.ChunkData;
	}

	private void Handle_hIST(PNGChunk chunk)
	{
		if (hIST != null)
		{
			throw new ApplicationException("hIST chunk encountered more than once");
		}
		hIST = new hISTChunk();
		hIST.ChunkData = chunk.ChunkData;
	}

	private void Handle_bKGD(PNGChunk chunk)
	{
		if (bKGD != null)
		{
			throw new ApplicationException("bKGD chunk encountered more than once");
		}
		switch (IHDR.ColorType)
		{
		case 0:
			bKGD = new bKGDChunkType0();
			break;
		case 2:
			bKGD = new bKGDChunkType2();
			break;
		case 3:
			bKGD = new bKGDChunkType3();
			break;
		case 4:
			bKGD = new bKGDChunkType4();
			break;
		case 6:
			bKGD = new bKGDChunkType6();
			break;
		default:
			throw new ApplicationException("Colour type is not supported");
		}
		bKGD.ChunkData = chunk.ChunkData;
	}

	private void Handle_iTXt(PNGChunk chunk)
	{
		iTXtChunk iTXtChunk2 = new iTXtChunk();
		iTXtChunk2.ChunkData = chunk.ChunkData;
		iTXtList.Add(iTXtChunk2);
	}

	private void Handle_zTXt(PNGChunk chunk)
	{
		zTXtChunk zTXtChunk2 = new zTXtChunk();
		zTXtChunk2.ChunkData = chunk.ChunkData;
		zTXtList.Add(zTXtChunk2);
	}

	private void Handle_tEXt(PNGChunk chunk)
	{
		tEXtChunk tEXtChunk2 = new tEXtChunk();
		tEXtChunk2.ChunkData = chunk.ChunkData;
		tEXtList.Add(tEXtChunk2);
	}

	private void Handle_sRGB(PNGChunk chunk)
	{
		if (sRGB != null)
		{
			throw new ApplicationException("sRGB chunk encountered more than once");
		}
		sRGB = new sRGBChunk();
		sRGB.ChunkData = chunk.ChunkData;
	}

	private void Handle_sBIT(PNGChunk chunk)
	{
		if (sBIT != null)
		{
			throw new ApplicationException("sBIT chunk encountered more than once");
		}
		switch (IHDR.ColorType)
		{
		case 0:
			sBIT = new sBITChunkType0();
			break;
		case 2:
			sBIT = new sBITChunkType2();
			break;
		case 3:
			sBIT = new sBITChunkType3();
			break;
		case 4:
			sBIT = new sBITChunkType4();
			break;
		case 6:
			sBIT = new sBITChunkType6();
			break;
		default:
			throw new ApplicationException("Colour type is not supported");
		}
		sBIT.ChunkData = chunk.ChunkData;
	}

	private void Handle_iCCP(PNGChunk chunk)
	{
		if (iCCP != null)
		{
			throw new ApplicationException("iCCP chunk encountered more than once");
		}
		iCCP = new iCCPChunk();
		iCCP.ChunkData = chunk.ChunkData;
	}

	private void Handle_gAMA(PNGChunk chunk)
	{
		if (gAMA != null)
		{
			throw new ApplicationException("gAMA chunk encountered more than once");
		}
		gAMA = new gAMAChunk();
		gAMA.ChunkData = chunk.ChunkData;
	}

	private void Handle_cHRM(PNGChunk chunk)
	{
		if (cHRM != null)
		{
			throw new ApplicationException("cHRM chunk encountered more than once");
		}
		cHRM = new cHRMChunk();
		cHRM.ChunkData = chunk.ChunkData;
	}

	private void Handle_tRNS(PNGChunk chunk)
	{
		if (tRNS != null)
		{
			throw new ApplicationException("tRNS chunk encountered more than once");
		}
		switch (IHDR.ColorType)
		{
		case 0:
			tRNS = new tRNSChunkType0();
			break;
		case 2:
			tRNS = new tRNSChunkType2();
			break;
		case 3:
			tRNS = new tRNSChunkType3();
			break;
		case 4:
		case 6:
			throw new ApplicationException("tRNS chunk encountered, Colour type does not support");
		default:
			throw new ApplicationException("Colour type is not supported");
		}
		tRNS.ChunkData = chunk.ChunkData;
	}

	private void Handle_PLTE(PNGChunk chunk)
	{
		if (PLTE != null)
		{
			throw new ApplicationException("PLTE chunk encountered more than once");
		}
		PLTE = new PLTEChunk();
		PLTE.ChunkData = chunk.ChunkData;
	}

	private void Handle_IHDR(PNGChunk chunk)
	{
		if (IHDR != null)
		{
			throw new ApplicationException("IHDR defined more than once");
		}
		IHDR = new IHDRChunk();
		IHDR.ChunkData = chunk.ChunkData;
	}

	private void Handle_IDAT(PNGChunk chunk)
	{
		IDATChunk iDATChunk = new IDATChunk();
		iDATChunk.ChunkData = chunk.ChunkData;
		IDATList.Add(iDATChunk);
	}

	private void Handle_IEND(PNGChunk chunk)
	{
		if (IEND != null)
		{
			throw new ApplicationException("IEND defined more than once");
		}
		IEND = new IENDChunk();
		IEND.ChunkData = chunk.ChunkData;
	}

	private void HandleDefaultChunk(PNGChunk chunk)
	{
		chunks.Add(chunk);
	}

	public virtual void Validate()
	{
		if (IHDR == null || IDATList.Count < 1 || IEND == null)
		{
			throw new ApplicationException("Required chunk(s) missing");
		}
		if (hIST != null && PLTE == null)
		{
			throw new ApplicationException("Cannot have a hIST chunk without a PLTE chunk");
		}
		if (hIST != null && hIST.Frequency.Length != PLTE.PaletteEntries.Count)
		{
			throw new ApplicationException("Number of hIST chunk entries different from number of PLTE chunk entries");
		}
	}
}
