using ExploreData;
using Messages;

namespace Player;

public class PlayerInfoJson
{
	public ulong entity_id;

	public int freq;

	public string name;

	public int level;

	public PlayerClanInfoJson clan;

	public double disconnected_at;

	public bool online;

	public RegionJson region;

	public RegionJson returning_region;

	public PlayerDisplay display;
}
