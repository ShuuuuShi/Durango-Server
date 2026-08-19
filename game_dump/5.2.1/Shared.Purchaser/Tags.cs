using System;

namespace Shared.Purchaser;

[Flags]
public enum Tags
{
	None = 0,
	Recommended = 1,
	Discounted = 2,
	Free = 4,
	Event = 8,
	AcceptAutomatically = 0x10,
	Representative = 0x20
}
