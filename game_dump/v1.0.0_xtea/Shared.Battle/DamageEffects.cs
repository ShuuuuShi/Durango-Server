using System;

namespace Shared.Battle;

[Flags]
public enum DamageEffects
{
	None = 0,
	Critical = 1,
	KnockBack = 2,
	Blow = 4,
	Tamed = 8,
	CrossCounter = 0x10,
	Conquered = 0x20,
	Incapacitate = 0x40
}
