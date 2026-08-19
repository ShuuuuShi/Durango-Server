using System;

namespace Sanford.Multimedia.Midi;

public class MetaMessageEventArgs : EventArgs
{
	private MetaMessage message;

	public MetaMessage Message => message;

	public MetaMessageEventArgs(MetaMessage message)
	{
		this.message = message;
	}
}
