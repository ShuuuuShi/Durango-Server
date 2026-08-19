using System;
using System.Collections;

namespace Sanford.Multimedia.Midi;

public class ChasedEventArgs : EventArgs
{
	private ICollection messages;

	public ICollection Messages => messages;

	public ChasedEventArgs(ICollection messages)
	{
		this.messages = messages;
	}
}
