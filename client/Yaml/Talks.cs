using Newtonsoft.Json;
using Shared.Faction;

namespace Yaml;

public class Talks
{
	[JsonProperty(PropertyName = "friendship_point")]
	public int FriendshipPoint;

	[JsonProperty(PropertyName = "notice_type")]
	public TalkType NoticeType;

	[JsonProperty(PropertyName = "talks")]
	public Talk[] List;

	[JsonProperty(PropertyName = "title")]
	public Gettext Title;

	public bool IsRead;
}
