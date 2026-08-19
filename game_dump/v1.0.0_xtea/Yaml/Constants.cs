using System.Collections.Generic;
using Yaml.Util;

namespace Yaml;

public class Constants : Singleton<Constants>
{
	public int newbie_level;

	public PointOfInterestDistance point_of_interest;

	public Exploring exploring;

	public Dictionary<int, int> required_risky_regions;

	public ConstantsItem item;

	public WarInfo war;

	public Dictionary<string, Dictionary<int, TimelineCategory>> timeline;
}
