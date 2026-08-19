using System;

namespace Durango.Logic.InputSystem;

[Flags]
public enum Modifier
{
	None = 0,
	LeftAlt = 1,
	LeftCommand = 2,
	LeftControl = 4,
	LeftShift = 8,
	RightAlt = 0x10,
	RightCommand = 0x20,
	RightControl = 0x40,
	RightShift = 0x80
}
