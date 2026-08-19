using System;
using System.Collections;

namespace Sanford.Multimedia.Midi;

public sealed class SysExMessage : IEnumerable, IMidiMessage
{
	public const int SysExChannelMaxValue = 127;

	private byte[] data;

	public byte this[int index]
	{
		get
		{
			if (index < 0 || index >= Length)
			{
				throw new ArgumentOutOfRangeException("index", index, "Index into system exclusive message out of range.");
			}
			return data[index];
		}
	}

	public int Length => data.Length;

	public SysExType SysExType => (SysExType)data[0];

	public int Status => data[0];

	public MessageType MessageType => MessageType.SystemExclusive;

	public SysExMessage(byte[] data)
	{
		if (data.Length < 1)
		{
			throw new ArgumentException("System exclusive data is too short.", "data");
		}
		if (data[0] != 240 && data[0] != 247)
		{
			throw new ArgumentException("Unknown status value.", "data");
		}
		this.data = new byte[data.Length];
		data.CopyTo(this.data, 0);
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[data.Length];
		data.CopyTo(array, 0);
		return array;
	}

	public void CopyTo(byte[] buffer, int index)
	{
		data.CopyTo(buffer, index);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SysExMessage))
		{
			return false;
		}
		SysExMessage sysExMessage = (SysExMessage)obj;
		bool flag = true;
		if (Length != sysExMessage.Length)
		{
			flag = false;
		}
		for (int i = 0; i < Length; i++)
		{
			if (!flag)
			{
				break;
			}
			if (this[i] != sysExMessage[i])
			{
				flag = false;
			}
		}
		return flag;
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public IEnumerator GetEnumerator()
	{
		return data.GetEnumerator();
	}
}
