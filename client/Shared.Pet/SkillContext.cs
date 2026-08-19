using System;

namespace Shared.Pet;

[Flags]
public enum SkillContext
{
	None = 0,
	Idle = 1,
	Battle = 2,
	Riding = 4,
	Swimming = 8,
	Resting = 0x10,
	Dead = 0x20,
	InsideBuilding = 0x40
}
