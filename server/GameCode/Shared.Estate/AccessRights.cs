using System;

namespace Shared.Estate;

[Flags]
public enum AccessRights
{
	None = 0,
	Enter = 1,
	UseFacility = 2,
	Give = 4,
	Take = 8,
	Occupy = 0x10,
	Destruct = 0x20
}
