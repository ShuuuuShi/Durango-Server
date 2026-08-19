using Durango.Utils.Extensions;
using Shared.Region;

namespace Durango.Logic.PlayGuide;

public class MoveToRegionToDo : ToDoBase
{
	private readonly Role _role;

	public MoveToRegionToDo(string type)
	{
		_role = type.ToEnum(Role.Sandbox);
	}

	public override void OnAddItem()
	{
		if (GameManager.Region.Role() == _role)
		{
			CallComplete();
		}
	}
}
