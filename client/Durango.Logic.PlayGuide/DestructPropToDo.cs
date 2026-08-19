using UnityEngine;

namespace Durango.Logic.PlayGuide;

public class DestructPropToDo : ToDoBase
{
	private readonly string _targetId;

	public DestructPropToDo(string id)
	{
		_targetId = id;
	}

	private ClientRemovableProp FindProp()
	{
		ClientRemovableProp result = null;
		ClientRemovableProp[] array = Object.FindObjectsOfType<ClientRemovableProp>();
		int num = 0;
		int num2 = array.Length;
		for (int i = 0; i < num2; i++)
		{
			if (array[i].EntityId == _targetId)
			{
				result = array[i];
				num++;
			}
		}
		return result;
	}

	public override void OnAddItem()
	{
		ClientRemovableProp clientRemovableProp = FindProp();
		if (!(clientRemovableProp == null))
		{
			clientRemovableProp.ClientPropDestructed += ClientPropDestructed;
		}
	}

	public override void OnRemoveItem()
	{
		ClientRemovableProp clientRemovableProp = FindProp();
		if (!(clientRemovableProp == null))
		{
			clientRemovableProp.ClientPropDestructed -= ClientPropDestructed;
		}
	}

	private void ClientPropDestructed(string entityId)
	{
		if (entityId == _targetId)
		{
			CallComplete();
		}
	}
}
