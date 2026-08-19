using System;

namespace Sanford.Multimedia.Midi;

public class ChannelMessageEventArgs : EventArgs
{
	private ChannelMessage message;

	public ChannelMessage Message => message;

	public ChannelMessageEventArgs(ChannelMessage message)
	{
		this.message = message;
	}
}
