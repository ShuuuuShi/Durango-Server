using Newtonsoft.Json;

namespace Yaml;

public class RegionCoOp
{
	[JsonProperty(PropertyName = "co_op_todo_id")]
	public string CoOpTodoId;

	[JsonProperty(PropertyName = "subject")]
	public Gettext Subject;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "notice_icon")]
	public string NoticeIcon;
}
