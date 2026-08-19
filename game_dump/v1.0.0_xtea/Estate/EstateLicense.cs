using System.Collections.Generic;
using Shared.Estate;

namespace Estate;

public struct EstateLicense
{
	public AccessRights Others;

	public AccessRights? Friends;

	public Dictionary<int, AccessRights> ClanMembers;
}
