using System.Collections.Generic;

namespace Durango.Logic.Clan;

public struct ClanJson
{
	public string id;

	public string name;

	public long fund;

	public int level;

	public long exp;

	public string intro;

	public string notice;

	public int member_count;

	public int capacity;

	public Gettext mainland;

	public Pair<string, int>[] members;

	public double renameable_until;

	public double emblem_changeable_until;

	public string[] appliers;

	public Dictionary<int, RoleInfo> role_infos;
}
