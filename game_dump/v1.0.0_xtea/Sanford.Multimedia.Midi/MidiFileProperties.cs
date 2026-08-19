using System;
using System.Diagnostics;
using System.IO;

namespace Sanford.Multimedia.Midi;

internal class MidiFileProperties
{
	private const int PropertyLength = 2;

	private static readonly byte[] MidiFileHeader = new byte[8] { 77, 84, 104, 100, 0, 0, 0, 6 };

	private int format = 1;

	private int trackCount;

	private int division = 24;

	private SequenceType sequenceType;

	public int Format
	{
		get
		{
			return format;
		}
		set
		{
			switch (value)
			{
			default:
				throw new ArgumentOutOfRangeException("Format", value, "MIDI file format out of range.");
			case 0:
				if (trackCount > 1)
				{
					throw new ArgumentException("MIDI file format invalid for this track count.");
				}
				break;
			case 1:
			case 2:
			case 3:
				break;
			}
			format = value;
		}
	}

	public int TrackCount
	{
		get
		{
			return trackCount;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("TrackCount", value, "Track count out of range.");
			}
			if (value > 1 && Format == 0)
			{
				throw new ArgumentException("Track count invalid for this format.");
			}
			trackCount = value;
		}
	}

	public int Division
	{
		get
		{
			return division;
		}
		set
		{
			if (IsSmpte(value))
			{
				byte[] bytes = BitConverter.GetBytes((short)value);
				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(bytes);
				}
				if ((sbyte)bytes[0] != -24 && (sbyte)bytes[0] != -25 && (sbyte)bytes[0] != -30 && (sbyte)bytes[0] != -29)
				{
					throw new ArgumentException("Invalid SMPTE frame rate.");
				}
				sequenceType = SequenceType.Smpte;
			}
			else
			{
				if (value % 24 != 0)
				{
					throw new ArgumentException("Invalid pulses per quarter note value.");
				}
				sequenceType = SequenceType.Ppqn;
			}
			division = value;
		}
	}

	public SequenceType SequenceType => sequenceType;

	public void Read(Stream strm)
	{
		if (strm == null)
		{
			throw new ArgumentNullException("strm");
		}
		format = (trackCount = (division = 0));
		FindHeader(strm);
		Format = ReadProperty(strm);
		TrackCount = ReadProperty(strm);
		Division = ReadProperty(strm);
	}

	private void FindHeader(Stream stream)
	{
		bool flag = false;
		while (!flag)
		{
			int num = stream.ReadByte();
			if (num == 77)
			{
				num = stream.ReadByte();
				if (num == 84)
				{
					num = stream.ReadByte();
					if (num == 104)
					{
						num = stream.ReadByte();
						if (num == 100)
						{
							flag = true;
						}
					}
				}
			}
			if (num < 0)
			{
				throw new MidiFileException("Unable to find MIDI file header.");
			}
		}
		for (int i = 0; i < 4; i++)
		{
			if (stream.ReadByte() < 0)
			{
				throw new MidiFileException("Unable to find MIDI file header.");
			}
		}
	}

	private ushort ReadProperty(Stream strm)
	{
		byte[] array = new byte[2];
		int num = strm.Read(array, 0, array.Length);
		if (num != array.Length)
		{
			throw new MidiFileException("End of MIDI file unexpectedly reached.");
		}
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(array);
		}
		return BitConverter.ToUInt16(array, 0);
	}

	public void Write(Stream strm)
	{
		if (strm == null)
		{
			throw new ArgumentNullException("strm");
		}
		strm.Write(MidiFileHeader, 0, MidiFileHeader.Length);
		WriteProperty(strm, (ushort)Format);
		WriteProperty(strm, (ushort)TrackCount);
		WriteProperty(strm, (ushort)Division);
	}

	private void WriteProperty(Stream strm, ushort property)
	{
		byte[] bytes = BitConverter.GetBytes(property);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes);
		}
		strm.Write(bytes, 0, 2);
	}

	private static bool IsSmpte(int division)
	{
		byte[] bytes = BitConverter.GetBytes((short)division);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes);
		}
		if ((sbyte)bytes[0] < 0)
		{
			return true;
		}
		return false;
	}

	[Conditional("DEBUG")]
	private void AssertValid()
	{
	}
}
