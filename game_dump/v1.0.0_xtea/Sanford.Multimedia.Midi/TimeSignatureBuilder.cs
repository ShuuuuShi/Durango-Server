using System;

namespace Sanford.Multimedia.Midi;

public class TimeSignatureBuilder : IMessageBuilder
{
	private const byte DefaultNumerator = 4;

	private const byte DefaultDenominator = 2;

	private const byte DefaultClocksPerMetronomeClick = 24;

	private const byte DefaultThirtySecondNotesPerQuarterNote = 8;

	private byte[] data = new byte[4];

	private MetaMessage result;

	private bool changed = true;

	public byte Numerator
	{
		get
		{
			return data[0];
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentOutOfRangeException("Numerator", value, "Numerator out of range.");
			}
			data[0] = value;
			changed = true;
		}
	}

	public byte Denominator
	{
		get
		{
			return Convert.ToByte(Math.Pow(2.0, (int)data[1]));
		}
		set
		{
			if (value < 2 || value > 32)
			{
				throw new ArgumentOutOfRangeException("Denominator must be between 2 and 32.");
			}
			if ((value & (value - 1)) != 0)
			{
				throw new ArgumentException("Denominator must be a power of 2.");
			}
			data[1] = Convert.ToByte(Math.Log((int)value, 2.0));
			changed = true;
		}
	}

	public byte ClocksPerMetronomeClick
	{
		get
		{
			return data[2];
		}
		set
		{
			data[2] = value;
			changed = true;
		}
	}

	public byte ThirtySecondNotesPerQuarterNote
	{
		get
		{
			return data[3];
		}
		set
		{
			data[3] = value;
			changed = true;
		}
	}

	public MetaMessage Result => result;

	public TimeSignatureBuilder()
	{
		Numerator = 4;
		Denominator = 2;
		ClocksPerMetronomeClick = 24;
		ThirtySecondNotesPerQuarterNote = 8;
	}

	public TimeSignatureBuilder(MetaMessage message)
	{
		Initialize(message);
	}

	public void Initialize(MetaMessage message)
	{
		if (message.MetaType != MetaType.TimeSignature)
		{
			throw new ArgumentException("Wrong meta event type.", "message");
		}
		data = message.GetBytes();
	}

	public void Build()
	{
		if (changed)
		{
			result = new MetaMessage(MetaType.TimeSignature, data);
			changed = false;
		}
	}
}
