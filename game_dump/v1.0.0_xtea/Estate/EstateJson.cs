using Shared.Estate;

namespace Estate;

public class EstateJson
{
	public ulong owner_id;

	public OwnerType owner_type;

	public double valid_since;

	public double valid_until;

	public EstateLicense license;

	public int extend_cost;

	public EstateOnWarWith on_war_with;
}
