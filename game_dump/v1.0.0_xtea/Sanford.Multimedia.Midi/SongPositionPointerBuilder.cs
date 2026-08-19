using System;

namespace Sanford.Multimedia.Midi;

public class SongPositionPointerBuilder : IMessageBuilder
{
	private const int TicksPer16thNote = 6;

	private const int Shift = 7;

	private const int Mask = 127;

	private int tickScale;

	private int ppqn;

	private SysCommonMessageBuilder builder;

	public int PositionInTicks
	{
		get
		{
			return SongPosition * tickScale * 6;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("PositionInTicks", value, "Position in ticks out of range.");
			}
			SongPosition = value / (tickScale * 6);
		}
	}

	public int Ppqn
	{
		get
		{
			return ppqn;
		}
		set
		{
			if (value % 24 != 0)
			{
				throw new ArgumentException("Invalid pulses per quarter note value.");
			}
			ppqn = value;
			tickScale = ppqn / 24;
		}
	}

	public int SongPosition
	{
		get
		{
			return (builder.Data2 << 7) | builder.Data1;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("SongPosition", value, "Song position pointer out of range.");
			}
			builder.Data1 = value & 0x7F;
			builder.Data2 = value >> 7;
		}
	}

	public SysCommonMessage Result => builder.Result;

	public SongPositionPointerBuilder()
	{
		builder = new SysCommonMessageBuilder();
		builder.Type = SysCommonType.SongPositionPointer;
		Ppqn = 24;
	}

	public SongPositionPointerBuilder(SysCommonMessage message)
	{
		builder = new SysCommonMessageBuilder();
		builder.Type = SysCommonType.SongPositionPointer;
		Initialize(message);
		Ppqn = 24;
	}

	public void Initialize(SysCommonMessage message)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		if (message.SysCommonType != SysCommonType.SongPositionPointer)
		{
			throw new ArgumentException("Message is not a song position pointer message.");
		}
		builder.Initialize(message);
	}

	public void Build()
	{
		builder.Build();
	}
}
