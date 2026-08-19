using System;
using System.ComponentModel;

namespace Sanford.Multimedia.Midi;

[ImmutableObject(true)]
public sealed class MetaMessage : IMidiMessage
{
	private const int Shift = 7;

	public const int TempoLength = 3;

	public const int SmpteOffsetLength = 5;

	public const int TimeSigLength = 4;

	public const int KeySigLength = 2;

	public static readonly MetaMessage EndOfTrackMessage = new MetaMessage(MetaType.EndOfTrack, new byte[0]);

	private MetaType type;

	private byte[] data;

	private int hashCode;

	public byte this[int index]
	{
		get
		{
			if (index < 0 || index >= Length)
			{
				throw new ArgumentOutOfRangeException("index", index, "Index into MetaMessage out of range.");
			}
			return data[index];
		}
	}

	public int Length => data.Length;

	public MetaType MetaType => type;

	public int Status => 255;

	public MessageType MessageType => MessageType.Meta;

	public MetaMessage(MetaType type, byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (!ValidateDataLength(type, data.Length))
		{
			throw new ArgumentException("Length of data not valid for meta message type.");
		}
		this.type = type;
		this.data = new byte[data.Length];
		data.CopyTo(this.data, 0);
		CalculateHashCode();
	}

	public byte[] GetBytes()
	{
		return (byte[])data.Clone();
	}

	public override int GetHashCode()
	{
		return hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MetaMessage))
		{
			return false;
		}
		bool flag = true;
		MetaMessage metaMessage = (MetaMessage)obj;
		if (MetaType != metaMessage.MetaType)
		{
			flag = false;
		}
		if (flag && Length != metaMessage.Length)
		{
			flag = false;
		}
		for (int i = 0; i < Length; i++)
		{
			if (!flag)
			{
				break;
			}
			if (this[i] != metaMessage[i])
			{
				flag = false;
			}
		}
		return flag;
	}

	private void CalculateHashCode()
	{
		hashCode = (int)MetaType;
		for (int i = 0; i < data.Length; i += 3)
		{
			hashCode ^= data[i];
		}
		for (int j = 1; j < data.Length; j += 3)
		{
			hashCode ^= data[j] << 7;
		}
		for (int k = 2; k < data.Length; k += 3)
		{
			hashCode ^= data[k] << 14;
		}
	}

	private bool ValidateDataLength(MetaType type, int length)
	{
		bool result = true;
		switch (type)
		{
		case MetaType.SequenceNumber:
			if (length != 0 || length != 2)
			{
				result = false;
			}
			break;
		case MetaType.EndOfTrack:
			if (length != 0)
			{
				result = false;
			}
			break;
		case MetaType.Tempo:
			if (length != 3)
			{
				result = false;
			}
			break;
		case MetaType.SmpteOffset:
			if (length != 5)
			{
				result = false;
			}
			break;
		case MetaType.TimeSignature:
			if (length != 4)
			{
				result = false;
			}
			break;
		case MetaType.KeySignature:
			if (length != 2)
			{
				result = false;
			}
			break;
		default:
			result = true;
			break;
		}
		return result;
	}
}
