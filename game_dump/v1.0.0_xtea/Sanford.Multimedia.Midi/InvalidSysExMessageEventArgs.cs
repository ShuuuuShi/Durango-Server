using System;
using System.Collections;

namespace Sanford.Multimedia.Midi;

public class InvalidSysExMessageEventArgs : EventArgs
{
	private byte[] messageData;

	public ICollection MessageData => messageData;

	public InvalidSysExMessageEventArgs(byte[] messageData)
	{
		this.messageData = messageData;
	}
}
