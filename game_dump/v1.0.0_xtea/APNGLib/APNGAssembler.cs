using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace APNGLib;

public static class APNGAssembler
{
	public static APNG ToPNG(IList<Texture2D> textures, float second)
	{
		PNG[] array = new PNG[textures.Count];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = new PNG();
			array[i].Load(new MemoryStream(textures[i].EncodeToPNG()));
		}
		return ToPNG(array, second);
	}

	public static APNG ToPNG(IList<PNG> images, float second)
	{
		if (images.Count < 1)
		{
			return null;
		}
		ushort delayNumerator = 1;
		ushort delayDenominator = 1;
		int[] array = new int[8] { 100, 50, 25, 10, 5, 4, 2, 1 };
		int num = Mathf.RoundToInt(second * 100f);
		int i = 0;
		for (int num2 = array.Length; i < num2; i++)
		{
			if (num % array[i] == 0)
			{
				delayNumerator = (ushort)(num / array[i]);
				delayDenominator = (ushort)(100 / array[i]);
			}
		}
		uint seq = 0u;
		APNG aPNG = new APNG();
		PNG pNG = images[0];
		SetupAPNGChunks(aPNG, pNG);
		Frame f = CreateFrame(pNG.Height, pNG.Width, 0u, 0u, ref seq, first: true, pNG.IDATList, delayNumerator, delayDenominator);
		aPNG.AddFrame(f);
		int j = 1;
		Vector2 val = default(Vector2);
		for (int count = images.Count; j < count; j++)
		{
			((Vector2)(ref val))._002Ector(0f, 0f);
			PNG pNG2 = images[j];
			Frame f2 = CreateFrame(pNG2.Height, pNG2.Width, (uint)val.x, (uint)val.y, ref seq, first: false, pNG2.IDATList, delayNumerator, delayDenominator);
			aPNG.AddFrame(f2);
		}
		aPNG.acTL.NumFrames = (uint)aPNG.FrameCount;
		aPNG.Validate();
		return aPNG;
	}

	private static Frame CreateFrame(uint h, uint w, uint xoff, uint yoff, ref uint seq, bool first, IList<IDATChunk> idats, ushort delayNumerator, ushort delayDenominator)
	{
		fcTLChunk fcTLChunk2 = new fcTLChunk();
		fcTLChunk2.DelayNumerator = delayNumerator;
		fcTLChunk2.DelayDenominator = delayDenominator;
		fcTLChunk2.Height = h;
		fcTLChunk2.Width = w;
		fcTLChunk2.DisposeOperation = 1;
		fcTLChunk2.BlendOperation = 0;
		fcTLChunk2.XOffset = xoff;
		fcTLChunk2.YOffset = yoff;
		fcTLChunk2.SequenceNumber = seq++;
		fcTLChunk fChunk = fcTLChunk2;
		Frame frame = new Frame(first, fChunk);
		foreach (IDATChunk idat in idats)
		{
			if (first)
			{
				frame.AddChunk(idat);
				continue;
			}
			fdATChunk fdATChunk2 = new fdATChunk();
			fdATChunk2.FrameData = idat.ImageData;
			fdATChunk2.SequenceNumber = seq++;
			fdATChunk f = fdATChunk2;
			frame.AddChunk(f);
		}
		return frame;
	}

	private static void SetupAPNGChunks(APNG apng, PNG png)
	{
		apng.IHDR = png.IHDR;
		apng.acTL = new acTLChunk
		{
			NumPlays = 0u
		};
		foreach (IDATChunk iDAT in png.IDATList)
		{
			apng.IDATList.Add(iDAT);
		}
		apng.IEND = png.IEND;
		apng.PLTE = png.PLTE;
		apng.tRNS = png.tRNS;
		apng.cHRM = png.cHRM;
		apng.gAMA = png.gAMA;
		apng.iCCP = png.iCCP;
		apng.sBIT = png.sBIT;
		apng.sRGB = png.sRGB;
		foreach (tEXtChunk tEXt in png.tEXtList)
		{
			apng.tEXtList.Add(tEXt);
		}
		foreach (zTXtChunk zTXt in png.zTXtList)
		{
			apng.zTXtList.Add(zTXt);
		}
		foreach (iTXtChunk iTXt in png.iTXtList)
		{
			apng.iTXtList.Add(iTXt);
		}
		apng.bKGD = png.bKGD;
		apng.hIST = png.hIST;
		apng.pHYs = png.pHYs;
		apng.sPLT = png.sPLT;
		DateTime now = DateTime.Now;
		apng.tIME = new tIMEChunk
		{
			Day = (byte)now.Day,
			Month = (byte)now.Month,
			Year = (ushort)now.Year,
			Hour = (byte)now.Hour,
			Minute = (byte)now.Minute,
			Second = (byte)now.Second
		};
	}
}
