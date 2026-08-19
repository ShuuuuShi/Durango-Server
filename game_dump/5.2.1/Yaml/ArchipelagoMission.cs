using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public class ArchipelagoMission
{
	[JsonProperty(PropertyName = "clear_point")]
	public int ClearPoint;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "intro")]
	public Dialogue Intro;

	[JsonProperty(PropertyName = "outro")]
	public Dialogue Outro;

	[JsonProperty(PropertyName = "title")]
	public Gettext Title;

	[JsonProperty(PropertyName = "todo_list")]
	public Dictionary<string, ToDoContents> ToDos;
}
