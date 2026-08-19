using Durango.Logic.Clusters;
using Newtonsoft.Json;

namespace Yaml;

public class ArtifactFloor
{
	[JsonProperty(PropertyName = "floorable_types")]
	public int[] FloorableTypes;

	private int _maxStories;

	[JsonProperty(PropertyName = "max_stories")]
	public int MaxStories
	{
		get
		{
			if (GameManager.ClusterMode == Mode.Online)
			{
				return _maxStories;
			}
			return 10;
		}
		set
		{
			_maxStories = value;
		}
	}
}
