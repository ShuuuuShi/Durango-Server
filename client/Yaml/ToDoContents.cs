using Newtonsoft.Json;

namespace Yaml;

public class ToDoContents
{
	[JsonProperty(PropertyName = "point")]
	public int Point;

	[JsonProperty(PropertyName = "subject")]
	public Gettext Subject;
}
