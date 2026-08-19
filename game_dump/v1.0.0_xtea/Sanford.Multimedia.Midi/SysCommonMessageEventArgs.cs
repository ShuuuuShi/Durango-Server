using System;

namespace Sanford.Multimedia.Midi;

public class SysCommonMessageEventArgs : EventArgs
{
	private SysCommonMessage message;

	public SysCommonMessage Message => message;

	public SysCommonMessageEventArgs(SysCommonMessage message)
	{
		this.message = message;
	}
}
