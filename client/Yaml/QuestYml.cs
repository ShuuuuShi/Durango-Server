using Newtonsoft.Json;
using Shared.Quest;

namespace Yaml;

public class QuestYml
{
	[JsonProperty(PropertyName = "quest_type")]
	public QuestType QuestType;

	[JsonProperty(PropertyName = "category")]
	public string Category;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "chapter_subject")]
	public Gettext ChapterSubject;

	[JsonProperty(PropertyName = "subject")]
	public Gettext Subject;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "display_on_hud")]
	public bool DisplayOnHud;

	[JsonProperty(PropertyName = "auto_finish")]
	public bool AutoFinish;

	[JsonProperty(PropertyName = "order")]
	public int Order;

	[JsonProperty(PropertyName = "last_quest")]
	public bool LastQuest;

	[JsonProperty(PropertyName = "quest_start_messages")]
	public QuestMessages[] QuestStartMessages;

	[JsonProperty(PropertyName = "quest_end_messages")]
	public QuestMessages[] QuestEndMessages;
}
