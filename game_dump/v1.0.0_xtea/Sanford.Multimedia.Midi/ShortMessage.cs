using System;

namespace Sanford.Multimedia.Midi;

public abstract class ShortMessage : IMidiMessage
{
	public const int DataMaxValue = 127;

	public const int StatusMaxValue = 255;

	private const int StatusMask = -256;

	protected const int DataMask = 255;

	private const int Data1Mask = -65281;

	private const int Data2Mask = 65535;

	private const int Shift = 8;

	protected int msg;

	public int Message => msg;

	public int Status => UnpackStatus(msg);

	public abstract MessageType MessageType { get; }

	public byte[] GetBytes()
	{
		return BitConverter.GetBytes(msg);
	}

	internal static int PackStatus(int message, int status)
	{
		if (status < 0 || status > 255)
		{
			throw new ArgumentOutOfRangeException("status", status, "Status value out of range.");
		}
		return (message & -256) | status;
	}

	internal static int PackData1(int message, int data1)
	{
		if (data1 < 0 || data1 > 127)
		{
			throw new ArgumentOutOfRangeException("data1", data1, "Data 1 value out of range.");
		}
		return (message & -65281) | (data1 << 8);
	}

	internal static int PackData2(int message, int data2)
	{
		if (data2 < 0 || data2 > 127)
		{
			throw new ArgumentOutOfRangeException("data2", data2, "Data 2 value out of range.");
		}
		return (message & 0xFFFF) | (data2 << 16);
	}

	internal static int UnpackStatus(int message)
	{
		return message & 0xFF;
	}

	internal static int UnpackData1(int message)
	{
		return (message & 0xFF00) >> 8;
	}

	internal static int UnpackData2(int message)
	{
		return (message & -65536) >> 16;
	}
}
