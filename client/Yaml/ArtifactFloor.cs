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
			return (GameManager.ClusterMode != 0) ? 10 : _maxStories;
		}
		set
		{
			_maxStories = value;
		}
	}
}
