using System;
using System.Collections;

namespace Sanford.Multimedia.Midi;

public class ChannelChaser
{
	private ChannelMessage[,] controllerMessages;

	private ChannelMessage[] programChangeMessages;

	private ChannelMessage[] pitchBendMessages;

	private ChannelMessage[] channelPressureMessages;

	private ChannelMessage[] polyPressureMessages;

	public event EventHandler<ChasedEventArgs> Chased;

	public ChannelChaser()
	{
		int num = 16;
		int num2 = 128;
		controllerMessages = new ChannelMessage[num, num2];
		programChangeMessages = new ChannelMessage[num];
		pitchBendMessages = new ChannelMessage[num];
		channelPressureMessages = new ChannelMessage[num];
		polyPressureMessages = new ChannelMessage[num];
	}

	public void Process(ChannelMessage message)
	{
		switch (message.Command)
		{
		case ChannelCommand.Controller:
			controllerMessages[message.MidiChannel, message.Data1] = message;
			break;
		case ChannelCommand.ChannelPressure:
			channelPressureMessages[message.MidiChannel] = message;
			break;
		case ChannelCommand.PitchWheel:
			pitchBendMessages[message.MidiChannel] = message;
			break;
		case ChannelCommand.PolyPressure:
			polyPressureMessages[message.MidiChannel] = message;
			break;
		case ChannelCommand.ProgramChange:
			programChangeMessages[message.MidiChannel] = message;
			break;
		}
	}

	public void Chase()
	{
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i <= 15; i++)
		{
			for (int j = 0; j <= 127; j++)
			{
				if (controllerMessages[i, j] != null)
				{
					arrayList.Add(controllerMessages[i, j]);
					controllerMessages[i, j] = null;
				}
			}
			if (programChangeMessages[i] != null)
			{
				arrayList.Add(programChangeMessages[i]);
				programChangeMessages[i] = null;
			}
			if (pitchBendMessages[i] != null)
			{
				arrayList.Add(pitchBendMessages[i]);
				pitchBendMessages[i] = null;
			}
			if (channelPressureMessages[i] != null)
			{
				arrayList.Add(channelPressureMessages[i]);
				channelPressureMessages[i] = null;
			}
			if (polyPressureMessages[i] != null)
			{
				arrayList.Add(polyPressureMessages[i]);
				polyPressureMessages[i] = null;
			}
		}
		OnChased(new ChasedEventArgs(arrayList));
	}

	public void Reset()
	{
		for (int i = 0; i <= 15; i++)
		{
			for (int j = 0; j <= 127; j++)
			{
				controllerMessages[i, j] = null;
			}
			programChangeMessages[i] = null;
			pitchBendMessages[i] = null;
			channelPressureMessages[i] = null;
			polyPressureMessages[i] = null;
		}
	}

	protected virtual void OnChased(ChasedEventArgs e)
	{
		this.Chased?.Invoke(this, e);
	}
}
