using System;

namespace Sanford.Multimedia.Midi;

public class SysExMessageEventArgs : EventArgs
{
	private SysExMessage message;

	public SysExMessage Message => message;

	public SysExMessageEventArgs(SysExMessage message)
	{
		this.message = message;
	}
}
