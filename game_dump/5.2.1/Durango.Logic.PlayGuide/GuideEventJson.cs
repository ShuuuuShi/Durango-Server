using Newtonsoft.Json.Linq;

namespace Durango.Logic.PlayGuide;

public class GuideEventJson
{
	public string[] messages;

	public string[] chapter;

	public string npc_type;

	public string image;

	public bool hide_portrait;

	public string todo_title;

	public ToDoBase.ToDoJson[] todos;

	public string spawn_flow;

	public bool is_system;

	public float duration;

	public bool is_blur;

	public bool remote;

	public string portrait;

	public string faction;

	public string name_tag;

	public HelperTarget[] helper;

	public SpotlightTarget spotlight;

	public int survival_memo;

	public string touch_todo;

	public string activate_faction;

	public string custom_cmd;

	public string card_news;

	public string nx_ads;

	public JArray quiz;

	public GuideEventJson override_pc;
}
