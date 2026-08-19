using System;

namespace Sanford.Multimedia.Midi;

public class InvalidShortMessageEventArgs : EventArgs
{
	private int message;

	public int Message => message;

	public InvalidShortMessageEventArgs(int message)
	{
		this.message = message;
	}
}
