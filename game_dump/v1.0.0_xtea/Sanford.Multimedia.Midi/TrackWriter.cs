using System;
using System.Collections.Generic;
using System.IO;

namespace Sanford.Multimedia.Midi;

internal class TrackWriter
{
	private static readonly byte[] TrackHeader = new byte[4] { 77, 84, 114, 107 };

	private Track track = new Track();

	private Stream stream;

	private int runningStatus;

	private List<byte> trackData = new List<byte>();

	public Track Track
	{
		get
		{
			return track;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Track");
			}
			runningStatus = 0;
			trackData.Clear();
			track = value;
		}
	}

	public void Write(Stream strm)
	{
		stream = strm;
		trackData.Clear();
		stream.Write(TrackHeader, 0, TrackHeader.Length);
		foreach (MidiEvent item in track.Iterator())
		{
			WriteVariableLengthValue(item.DeltaTicks);
			switch (item.MidiMessage.MessageType)
			{
			case MessageType.Channel:
				Write((ChannelMessage)item.MidiMessage);
				break;
			case MessageType.SystemExclusive:
				Write((SysExMessage)item.MidiMessage);
				break;
			case MessageType.Meta:
				Write((MetaMessage)item.MidiMessage);
				break;
			case MessageType.SystemCommon:
				Write((SysCommonMessage)item.MidiMessage);
				break;
			case MessageType.SystemRealtime:
				Write((SysRealtimeMessage)item.MidiMessage);
				break;
			}
		}
		byte[] bytes = BitConverter.GetBytes(trackData.Count);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes);
		}
		stream.Write(bytes, 0, bytes.Length);
		foreach (byte trackDatum in trackData)
		{
			stream.WriteByte(trackDatum);
		}
	}

	private void WriteVariableLengthValue(int value)
	{
		int num = value;
		byte[] array = new byte[4];
		int num2 = 0;
		array[0] = (byte)((uint)num & 0x7Fu);
		for (num >>= 7; num > 0; num >>= 7)
		{
			num2++;
			array[num2] = (byte)(((uint)num & 0x7Fu) | 0x80u);
		}
		while (num2 >= 0)
		{
			trackData.Add(array[num2]);
			num2--;
		}
	}

	private void Write(ChannelMessage message)
	{
		if (runningStatus != message.Status)
		{
			trackData.Add((byte)message.Status);
			runningStatus = message.Status;
		}
		trackData.Add((byte)message.Data1);
		if (ChannelMessage.DataBytesPerType(message.Command) == 2)
		{
			trackData.Add((byte)message.Data2);
		}
	}

	private void Write(SysExMessage message)
	{
		runningStatus = 0;
		trackData.Add((byte)message.Status);
		WriteVariableLengthValue(message.Length - 1);
		for (int i = 1; i < message.Length; i++)
		{
			trackData.Add(message[i]);
		}
	}

	private void Write(MetaMessage message)
	{
		trackData.Add((byte)message.Status);
		trackData.Add((byte)message.MetaType);
		WriteVariableLengthValue(message.Length);
		trackData.AddRange(message.GetBytes());
	}

	private void Write(SysCommonMessage message)
	{
		runningStatus = 0;
		trackData.Add(247);
		trackData.Add((byte)message.Status);
		switch (message.SysCommonType)
		{
		case SysCommonType.MidiTimeCode:
			trackData.Add((byte)message.Data1);
			break;
		case SysCommonType.SongPositionPointer:
			trackData.Add((byte)message.Data1);
			trackData.Add((byte)message.Data2);
			break;
		case SysCommonType.SongSelect:
			trackData.Add((byte)message.Data1);
			break;
		}
	}

	private void Write(SysRealtimeMessage message)
	{
		runningStatus = 0;
		trackData.Add(247);
		trackData.Add((byte)message.Status);
	}
}
