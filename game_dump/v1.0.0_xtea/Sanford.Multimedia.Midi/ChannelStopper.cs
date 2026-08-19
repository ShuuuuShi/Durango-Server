using System;
using System.Collections;

namespace Sanford.Multimedia.Midi;

public class ChannelStopper
{
	private ChannelMessage[,] noteOnMessage;

	private bool[] holdPedal1Message;

	private bool[] holdPedal2Message;

	private bool[] sustenutoMessage;

	private ChannelMessageBuilder builder = new ChannelMessageBuilder();

	public event EventHandler<StoppedEventArgs> Stopped;

	public ChannelStopper()
	{
		int num = 16;
		int num2 = 128;
		noteOnMessage = new ChannelMessage[num, num2];
		holdPedal1Message = new bool[num];
		holdPedal2Message = new bool[num];
		sustenutoMessage = new bool[num];
	}

	public void Process(ChannelMessage message)
	{
		switch (message.Command)
		{
		case ChannelCommand.NoteOn:
			if (message.Data2 > 0)
			{
				noteOnMessage[message.MidiChannel, message.Data1] = message;
			}
			else
			{
				noteOnMessage[message.MidiChannel, message.Data1] = null;
			}
			break;
		case ChannelCommand.NoteOff:
			noteOnMessage[message.MidiChannel, message.Data1] = null;
			break;
		case ChannelCommand.Controller:
			switch (message.Data1)
			{
			case 64:
				if (message.Data2 > 63)
				{
					holdPedal1Message[message.MidiChannel] = true;
				}
				else
				{
					holdPedal1Message[message.MidiChannel] = false;
				}
				break;
			case 69:
				if (message.Data2 > 63)
				{
					holdPedal2Message[message.MidiChannel] = true;
				}
				else
				{
					holdPedal2Message[message.MidiChannel] = false;
				}
				break;
			case 66:
				if (message.Data2 > 63)
				{
					sustenutoMessage[message.MidiChannel] = true;
				}
				else
				{
					sustenutoMessage[message.MidiChannel] = false;
				}
				break;
			case 65:
			case 67:
			case 68:
				break;
			}
			break;
		}
	}

	public void AllSoundOff()
	{
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i <= 15; i++)
		{
			for (int j = 0; j <= 127; j++)
			{
				if (noteOnMessage[i, j] != null)
				{
					builder.MidiChannel = i;
					builder.Command = ChannelCommand.NoteOff;
					builder.Data1 = noteOnMessage[i, j].Data1;
					builder.Build();
					arrayList.Add(builder.Result);
					noteOnMessage[i, j] = null;
				}
			}
			if (holdPedal1Message[i])
			{
				builder.MidiChannel = i;
				builder.Command = ChannelCommand.Controller;
				builder.Data1 = 64;
				builder.Build();
				arrayList.Add(builder.Result);
				holdPedal1Message[i] = false;
			}
			if (holdPedal2Message[i])
			{
				builder.MidiChannel = i;
				builder.Command = ChannelCommand.Controller;
				builder.Data1 = 69;
				builder.Build();
				arrayList.Add(builder.Result);
				holdPedal2Message[i] = false;
			}
			if (sustenutoMessage[i])
			{
				builder.MidiChannel = i;
				builder.Command = ChannelCommand.Controller;
				builder.Data1 = 66;
				builder.Build();
				arrayList.Add(builder.Result);
				sustenutoMessage[i] = false;
			}
		}
		OnStopped(new StoppedEventArgs(arrayList));
	}

	public void Reset()
	{
		for (int i = 0; i <= 15; i++)
		{
			for (int j = 0; j <= 127; j++)
			{
				noteOnMessage[i, j] = null;
			}
			holdPedal1Message[i] = false;
			holdPedal2Message[i] = false;
			sustenutoMessage[i] = false;
		}
	}

	protected virtual void OnStopped(StoppedEventArgs e)
	{
		this.Stopped?.Invoke(this, e);
	}
}
