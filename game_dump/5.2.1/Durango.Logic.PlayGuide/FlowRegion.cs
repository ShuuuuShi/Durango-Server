using Durango.Logic.Explore;
using Durango.Utils.Extensions;
using Newtonsoft.Json;
using Shared.Region;

namespace Durango.Logic.PlayGuide;

public class FlowRegion
{
	[JsonProperty(PropertyName = "role")]
	private string _role;

	[JsonProperty(PropertyName = "max_level")]
	private int _maxLevel;

	[JsonProperty(PropertyName = "min_level")]
	private int _minLevel;

	public bool IsAllowed(Region region)
	{
		Role role = _role.ToEnum(Role.Invalid);
		if (role != Role.Invalid && region.Role() != role)
		{
			return false;
		}
		if (_minLevel == 0 || _minLevel <= region.Level)
		{
			if (_maxLevel != 0)
			{
				return _maxLevel >= region.Level;
			}
			return true;
		}
		return false;
	}
}
