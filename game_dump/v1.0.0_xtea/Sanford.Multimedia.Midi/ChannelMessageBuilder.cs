using System.Collections;

namespace Sanford.Multimedia.Midi;

public class ChannelMessageBuilder : IMessageBuilder
{
	private static Hashtable messageCache = Hashtable.Synchronized(new Hashtable());

	private int message;

	private ChannelMessage result;

	public static int Count => messageCache.Count;

	public ChannelMessage Result => result;

	internal int Message
	{
		get
		{
			return message;
		}
		set
		{
			message = value;
		}
	}

	public ChannelCommand Command
	{
		get
		{
			return ChannelMessage.UnpackCommand(message);
		}
		set
		{
			message = ChannelMessage.PackCommand(message, value);
		}
	}

	public int MidiChannel
	{
		get
		{
			return ChannelMessage.UnpackMidiChannel(message);
		}
		set
		{
			message = ChannelMessage.PackMidiChannel(message, value);
		}
	}

	public int Data1
	{
		get
		{
			return ShortMessage.UnpackData1(message);
		}
		set
		{
			message = ShortMessage.PackData1(message, value);
		}
	}

	public int Data2
	{
		get
		{
			return ShortMessage.UnpackData2(message);
		}
		set
		{
			message = ShortMessage.PackData2(message, value);
		}
	}

	public ChannelMessageBuilder()
	{
		Command = ChannelCommand.Controller;
		MidiChannel = 0;
		Data1 = 120;
		Data2 = 0;
	}

	public ChannelMessageBuilder(ChannelMessage message)
	{
		Initialize(message);
	}

	public void Initialize(ChannelMessage message)
	{
		this.message = message.Message;
	}

	public static void Clear()
	{
		messageCache.Clear();
	}

	public void Build()
	{
		result = (ChannelMessage)messageCache[message];
		if (result == null)
		{
			result = new ChannelMessage(message);
			messageCache.Add(message, result);
		}
	}
}
