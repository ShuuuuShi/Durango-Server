using System;

namespace Durango.Logic.InputSystem;

[Flags]
public enum Trigger
{
	None = 0,
	Down = 1,
	Up = 2,
	Press = 4,
	Stream = 5,
	UpStream = 6,
	DownUp = 3,
	All = -1
}
