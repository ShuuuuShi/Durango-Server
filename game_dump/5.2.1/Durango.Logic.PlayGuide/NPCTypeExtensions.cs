namespace Durango.Logic.PlayGuide;

public static class NPCTypeExtensions
{
	public static string ToDoIcon(this NPCType type)
	{
		return "todo_icon_npc_" + type;
	}
}
