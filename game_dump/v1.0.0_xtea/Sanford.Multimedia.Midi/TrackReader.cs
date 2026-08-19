using System;
using System.IO;

namespace Sanford.Multimedia.Midi;

internal class TrackReader
{
	private Track track = new Track();

	private Track newTrack = new Track();

	private ChannelMessageBuilder cmBuilder = new ChannelMessageBuilder();

	private SysCommonMessageBuilder scBuilder = new SysCommonMessageBuilder();

	private Stream stream;

	private byte[] trackData;

	private int trackIndex;

	private int previousTicks;

	private int ticks;

	private int status;

	private int runningStatus;

	public Track Track => track;

	public void Read(Stream strm)
	{
		stream = strm;
		FindTrack();
		int trackLength = GetTrackLength();
		trackData = new byte[trackLength];
		int num = strm.Read(trackData, 0, trackLength);
		if (num < 0)
		{
			throw new MidiFileException("End of MIDI file unexpectedly reached.");
		}
		newTrack = new Track();
		ParseTrackData();
		track = newTrack;
	}

	private void FindTrack()
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
					if (num == 114)
					{
						num = stream.ReadByte();
						if (num == 107)
						{
							flag = true;
						}
					}
				}
			}
			if (num < 0)
			{
				throw new MidiFileException("Unable to find track in MIDI file.");
			}
		}
	}

	private int GetTrackLength()
	{
		byte[] array = new byte[4];
		int num = stream.Read(array, 0, array.Length);
		if (num < array.Length)
		{
			throw new MidiFileException("End of MIDI file unexpectedly reached.");
		}
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(array);
		}
		return BitConverter.ToInt32(array, 0);
	}

	private void ParseTrackData()
	{
		trackIndex = (ticks = (runningStatus = 0));
		while (trackIndex < trackData.Length)
		{
			previousTicks = ticks;
			ticks += ReadVariableLengthValue();
			if ((trackData[trackIndex] & 0x80) == 128)
			{
				status = trackData[trackIndex];
				trackIndex++;
			}
			else
			{
				status = runningStatus;
			}
			ParseMessage();
		}
	}

	private void ParseMessage()
	{
		if (status >= 128 && status <= 239)
		{
			ParseChannelMessage();
		}
		else if (status == 255)
		{
			ParseMetaMessage();
		}
		else if (status == 240)
		{
			ParseSysExMessageStart();
		}
		else if (status == 247)
		{
			ParseSysExMessageContinue();
		}
		else if (status >= 241 && status <= 246)
		{
			ParseSysCommonMessage();
		}
		else if (status >= 248 && status <= 255)
		{
			ParseSysRealtimeMessage();
		}
	}

	private void ParseChannelMessage()
	{
		if (trackIndex >= trackData.Length)
		{
			throw new MidiFileException("End of track unexpectedly reached.");
		}
		cmBuilder.Command = ChannelMessage.UnpackCommand(status);
		cmBuilder.MidiChannel = ChannelMessage.UnpackMidiChannel(status);
		cmBuilder.Data1 = trackData[trackIndex];
		trackIndex++;
		if (ChannelMessage.DataBytesPerType(cmBuilder.Command) == 2)
		{
			if (trackIndex >= trackData.Length)
			{
				throw new MidiFileException("End of track unexpectedly reached.");
			}
			cmBuilder.Data2 = trackData[trackIndex];
			trackIndex++;
		}
		cmBuilder.Build();
		newTrack.Insert(ticks, cmBuilder.Result);
		runningStatus = status;
	}

	private void ParseMetaMessage()
	{
		if (trackIndex >= trackData.Length)
		{
			throw new MidiFileException("End of track unexpectedly reached.");
		}
		MetaType metaType = (MetaType)trackData[trackIndex];
		trackIndex++;
		if (trackIndex >= trackData.Length)
		{
			throw new MidiFileException("End of track unexpectedly reached.");
		}
		if (metaType == MetaType.EndOfTrack)
		{
			newTrack.EndOfTrackOffset = ticks - previousTicks;
			trackIndex++;
			return;
		}
		byte[] array = new byte[ReadVariableLengthValue()];
		Array.Copy(trackData, trackIndex, array, 0, array.Length);
		newTrack.Insert(ticks, new MetaMessage(metaType, array));
		trackIndex += array.Length;
	}

	private void ParseSysExMessageStart()
	{
		runningStatus = 0;
		byte[] array = new byte[ReadVariableLengthValue() + 1];
		array[0] = 240;
		Array.Copy(trackData, trackIndex, array, 1, array.Length - 1);
		newTrack.Insert(ticks, new SysExMessage(array));
		trackIndex += array.Length - 1;
	}

	private void ParseSysExMessageContinue()
	{
		trackIndex++;
		if (trackIndex >= trackData.Length)
		{
			throw new MidiFileException("End of track unexpectedly reached.");
		}
		runningStatus = 0;
		if ((trackData[trackIndex] & 0x80) == 128)
		{
			status = trackData[trackIndex];
			trackIndex++;
			ParseMessage();
		}
		else
		{
			byte[] array = new byte[ReadVariableLengthValue() + 1];
			array[0] = 247;
			Array.Copy(trackData, trackIndex, array, 1, array.Length - 1);
			newTrack.Insert(ticks, new SysExMessage(array));
			trackIndex += array.Length - 1;
		}
	}

	private void ParseSysCommonMessage()
	{
		if (trackIndex >= trackData.Length)
		{
			throw new MidiFileException("End of track unexpectedly reached.");
		}
		runningStatus = 0;
		scBuilder.Type = (SysCommonType)status;
		switch ((SysCommonType)status)
		{
		case SysCommonType.MidiTimeCode:
			scBuilder.Data1 = trackData[trackIndex];
			trackIndex++;
			break;
		case SysCommonType.SongPositionPointer:
			scBuilder.Data1 = trackData[trackIndex];
			trackIndex++;
			if (trackIndex >= trackData.Length)
			{
				throw new MidiFileException("End of track unexpectedly reached.");
			}
			scBuilder.Data2 = trackData[trackIndex];
			trackIndex++;
			break;
		case SysCommonType.SongSelect:
			scBuilder.Data1 = trackData[trackIndex];
			trackIndex++;
			break;
		}
		scBuilder.Build();
		newTrack.Insert(ticks, scBuilder.Result);
	}

	private void ParseSysRealtimeMessage()
	{
		SysRealtimeMessage message = null;
		switch ((SysRealtimeType)status)
		{
		case SysRealtimeType.ActiveSense:
			message = SysRealtimeMessage.ActiveSenseMessage;
			break;
		case SysRealtimeType.Clock:
			message = SysRealtimeMessage.ClockMessage;
			break;
		case SysRealtimeType.Continue:
			message = SysRealtimeMessage.ContinueMessage;
			break;
		case SysRealtimeType.Reset:
			message = SysRealtimeMessage.ResetMessage;
			break;
		case SysRealtimeType.Start:
			message = SysRealtimeMessage.StartMessage;
			break;
		case SysRealtimeType.Stop:
			message = SysRealtimeMessage.StopMessage;
			break;
		case SysRealtimeType.Tick:
			message = SysRealtimeMessage.TickMessage;
			break;
		}
		newTrack.Insert(ticks, message);
	}

	private int ReadVariableLengthValue()
	{
		if (trackIndex >= trackData.Length)
		{
			throw new MidiFileException("End of track unexpectedly reached.");
		}
		int num = 0;
		num = trackData[trackIndex];
		trackIndex++;
		if ((num & 0x80) == 128)
		{
			num &= 0x7F;
			int num2;
			do
			{
				if (trackIndex >= trackData.Length)
				{
					throw new MidiFileException("End of track unexpectedly reached.");
				}
				num2 = trackData[trackIndex];
				trackIndex++;
				num <<= 7;
				num |= num2 & 0x7F;
			}
			while ((num2 & 0x80) == 128);
		}
		return num;
	}
}
