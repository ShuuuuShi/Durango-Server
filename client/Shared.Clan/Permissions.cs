using System;

namespace Shared.Clan;

[Flags]
public enum Permissions
{
	None = 0,
	ApproveMember = 1,
	PromoteMember = 2,
	EditClanInfo = 4,
	OccupyWarphole = 8,
	Research = 0x10
}
