using Newtonsoft.Json;
using Shared.Faction;

namespace Yaml;

public struct Talk
{
	[JsonProperty(PropertyName = "messenger")]
	public Shared.Faction.Messenger Messenger;

	[JsonProperty(PropertyName = "target")]
	public Shared.Faction.Messenger? Target;

	[JsonProperty(PropertyName = "message")]
	public Gettext Message;
}
