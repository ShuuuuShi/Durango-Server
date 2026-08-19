using Newtonsoft.Json;

namespace Yaml;

public class Chapters
{
	[JsonProperty(PropertyName = "chapters")]
	public Chapter[] ChapterList;
}
