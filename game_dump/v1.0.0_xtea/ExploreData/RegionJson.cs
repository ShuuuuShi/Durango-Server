using Shared.Region;

namespace ExploreData;

public class RegionJson
{
	public ulong id;

	public Gettext name;

	public string template_id;

	public ulong terrain_id;

	public double created_at;

	public Role role = Role.Invalid;
}
