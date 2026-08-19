using Shared.Region;

namespace Durango.Logic.Explore;

public class RegionJson
{
	public string id;

	public Gettext name;

	public string template_id;

	public string terrain_id;

	public double created_at;

	public Role role = Role.Invalid;
}
