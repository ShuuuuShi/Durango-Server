using System;
using System.Collections.Generic;

namespace APNGLib;

public class Frame
{
	public enum DisposeOperation
	{
		NONE,
		BACKGROUND,
		PREVIOUS
	}

	public enum BlendOperation
	{
		SOURCE,
		OVER
	}

	private IList<IDATChunk> idats;

	private IList<fdATChunk> fdats;

	private bool milliFlag;

	private int milli;

	private bool secFlag;

	private float sec;

	public fcTLChunk fctl { get; private set; }

	public bool IFrame { get; set; }

	public IEnumerable<IDATChunk> IDATs => idats;

	public IEnumerable<fdATChunk> fdATs => fdats;

	public uint Width
	{
		get
		{
			return fctl.Width;
		}
		set
		{
			fctl.Width = value;
		}
	}

	public uint Height
	{
		get
		{
			return fctl.Height;
		}
		set
		{
			fctl.Height = value;
		}
	}

	public uint XOffset
	{
		get
		{
			return fctl.XOffset;
		}
		set
		{
			fctl.XOffset = value;
		}
	}

	public uint YOffset
	{
		get
		{
			return fctl.YOffset;
		}
		set
		{
			fctl.YOffset = value;
		}
	}

	public ushort DelayNumerator
	{
		get
		{
			return fctl.DelayNumerator;
		}
		set
		{
			fctl.DelayNumerator = value;
			milliFlag = false;
			secFlag = false;
		}
	}

	public ushort DelayDenominator
	{
		get
		{
			return fctl.DelayDenominator;
		}
		set
		{
			fctl.DelayDenominator = value;
			milliFlag = false;
			secFlag = false;
		}
	}

	public int Milliseconds
	{
		get
		{
			if (!milliFlag)
			{
				milli = (int)(Seconds * 1000f);
				milliFlag = true;
			}
			return milli;
		}
	}

	public float Seconds
	{
		get
		{
			if (!secFlag)
			{
				sec = (float)(int)DelayNumerator / (float)(int)DelayDenominator;
				secFlag = true;
			}
			return sec;
		}
	}

	public DisposeOperation DisposeOp
	{
		get
		{
			return fctl.DisposeOperation switch
			{
				0 => DisposeOperation.NONE, 
				1 => DisposeOperation.BACKGROUND, 
				2 => DisposeOperation.PREVIOUS, 
				_ => throw new ApplicationException("Invalid Dispose Op"), 
			};
		}
		set
		{
			fctl.DisposeOperation = (byte)value;
		}
	}

	public BlendOperation BlendOp
	{
		get
		{
			return fctl.BlendOperation switch
			{
				0 => BlendOperation.SOURCE, 
				1 => BlendOperation.OVER, 
				_ => throw new ApplicationException("Invalid Blend Op"), 
			};
		}
		set
		{
			fctl.BlendOperation = (byte)value;
		}
	}

	public IList<byte> ImageData
	{
		get
		{
			IList<byte> list = new List<byte>();
			if (IFrame)
			{
				foreach (IDATChunk idat in idats)
				{
					byte[] imageData = idat.ImageData;
					foreach (byte item in imageData)
					{
						list.Add(item);
					}
				}
			}
			else
			{
				foreach (fdATChunk fdat in fdats)
				{
					byte[] frameData = fdat.FrameData;
					foreach (byte item2 in frameData)
					{
						list.Add(item2);
					}
				}
			}
			return list;
		}
	}

	public Frame(bool first, fcTLChunk fChunk)
	{
		IFrame = first;
		if (IFrame)
		{
			idats = new List<IDATChunk>();
		}
		else
		{
			fdats = new List<fdATChunk>();
		}
		fctl = fChunk;
	}

	public void AddChunk(IDATChunk i)
	{
		if (IFrame)
		{
			idats.Add(i);
			return;
		}
		throw new ApplicationException("Cannot add IDAT chunk to fdAT frame");
	}

	public void AddChunk(fdATChunk f)
	{
		if (IFrame)
		{
			throw new ApplicationException("Cannot add fdAT chunk to IDAT frame");
		}
		fdats.Add(f);
	}
}
