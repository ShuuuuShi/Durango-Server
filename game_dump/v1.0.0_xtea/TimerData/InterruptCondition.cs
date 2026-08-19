using System;

namespace TimerData;

[Flags]
public enum InterruptCondition
{
	None = 0,
	MoveStart = 1,
	TakeDamage = 2,
	Dead = 4,
	All = -1
}
