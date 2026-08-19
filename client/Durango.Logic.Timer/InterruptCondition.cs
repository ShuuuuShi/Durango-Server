using System;

namespace Durango.Logic.Timer;

[Flags]
public enum InterruptCondition
{
	None = 0,
	MoveStart = 1,
	TakeDamage = 2,
	Dead = 4,
	Blow = 8,
	All = -1
}
