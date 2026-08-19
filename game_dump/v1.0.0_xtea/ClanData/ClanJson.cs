using System.Collections.Generic;

namespace ClanData;

public struct ClanJson
{
	public ulong id;

	public string name;

	public long fund;

	public int level;

	public long exp;

	public string intro;

	public string notice;

	public int capacity;

	public Gettext mainland;

	public double shield_starts_at;

	public ulong[][] members;

	public ulong[] appliers;

	public Dictionary<int, RoleInfo> role_infos;
}
