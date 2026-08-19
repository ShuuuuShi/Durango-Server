using Newtonsoft.Json;
using Shared.Faction;

namespace Yaml;

public class MissionTalk
{
	[JsonProperty(PropertyName = "messenger")]
	public Shared.Faction.Messenger Messenger;

	[JsonProperty(PropertyName = "message")]
	public Gettext Message;

	[JsonProperty(PropertyName = "image")]
	public string Image;

	[JsonProperty(PropertyName = "hide_portrait")]
	public bool HidePortrait;
}
