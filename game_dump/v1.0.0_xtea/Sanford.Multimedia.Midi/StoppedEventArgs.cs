using System;
using System.Collections;

namespace Sanford.Multimedia.Midi;

public class StoppedEventArgs : EventArgs
{
	private ICollection messages;

	public ICollection Messages => messages;

	public StoppedEventArgs(ICollection messages)
	{
		this.messages = messages;
	}
}
