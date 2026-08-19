using Durango.Logic.PlayGuide;
using L10N;

namespace Durango.Logic;

public class CustomerServiceToDoCollection : ToDoCollection
{
	public const string ToDoKey = "read_cs";

	public CustomerServiceToDoCollection()
	{
		base.Key = "read_cs";
		Title = T._("답변이 도착했어요.");
		Icon = NPCType.Compi.ToDoIcon();
		Season = null;
		SetHelpClicked(GameSystem<CustomerServiceSystem>.Instance().ShowCustomerServiece);
		ReadCustomerServiceToDo item = new ReadCustomerServiceToDo();
		ToDoList.Add(item);
	}
}
