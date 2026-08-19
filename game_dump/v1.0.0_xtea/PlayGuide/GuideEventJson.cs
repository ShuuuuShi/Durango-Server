namespace PlayGuide;

public class GuideEventJson
{
	public string[] messages;

	public string npc_type;

	public string todo_title;

	public ToDoBase.ToDoJson[] todos;

	public string spawn_flow;

	public bool is_system;

	public bool autorun;

	public float msg_duration;

	public bool is_blur;

	public bool play_audio;

	public string portrait;

	public string faction;

	public string name_tag;

	public string[] lock_menus;

	public string[] unlock_menus;

	public string[] highlight;

	public string[] marking_new;

	public HelperTarget[] helper;

	public SpotlightTarget spotlight;

	public int survival_memo;

	public string custom_cmd;
}
