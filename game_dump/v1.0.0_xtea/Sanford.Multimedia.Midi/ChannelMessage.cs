using System;
using System.ComponentModel;

namespace Sanford.Multimedia.Midi;

[ImmutableObject(true)]
public sealed class ChannelMessage : ShortMessage
{
	private const int MidiChannelMask = -16;

	private const int CommandMask = -241;

	public const int MidiChannelMaxValue = 15;

	public ChannelCommand Command => UnpackCommand(msg);

	public int MidiChannel => UnpackMidiChannel(msg);

	public int Data1 => ShortMessage.UnpackData1(msg);

	public int Data2 => ShortMessage.UnpackData2(msg);

	public override MessageType MessageType => MessageType.Channel;

	public ChannelMessage(ChannelCommand command, int midiChannel, int data1)
	{
		msg = 0;
		msg = PackCommand(msg, command);
		msg = PackMidiChannel(msg, midiChannel);
		msg = ShortMessage.PackData1(msg, data1);
	}

	public ChannelMessage(ChannelCommand command, int midiChannel, int data1, int data2)
	{
		msg = 0;
		msg = PackCommand(msg, command);
		msg = PackMidiChannel(msg, midiChannel);
		msg = ShortMessage.PackData1(msg, data1);
		msg = ShortMessage.PackData2(msg, data2);
	}

	internal ChannelMessage(int message)
	{
		msg = message;
	}

	public override int GetHashCode()
	{
		return msg;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ChannelMessage))
		{
			return false;
		}
		ChannelMessage channelMessage = (ChannelMessage)obj;
		return msg == channelMessage.msg;
	}

	internal static int DataBytesPerType(ChannelCommand command)
	{
		if (command == ChannelCommand.ChannelPressure || command == ChannelCommand.ProgramChange)
		{
			return 1;
		}
		return 2;
	}

	internal static ChannelCommand UnpackCommand(int message)
	{
		return (ChannelCommand)(message & 0xFF & -16);
	}

	internal static int UnpackMidiChannel(int message)
	{
		return message & 0xFF & -241;
	}

	internal static int PackMidiChannel(int message, int midiChannel)
	{
		if (midiChannel < 0 || midiChannel > 15)
		{
			throw new ArgumentOutOfRangeException("midiChannel", midiChannel, "MIDI channel out of range.");
		}
		return (message & -16) | midiChannel;
	}

	internal static int PackCommand(int message, ChannelCommand command)
	{
		return (message & -241) | (int)command;
	}
}
