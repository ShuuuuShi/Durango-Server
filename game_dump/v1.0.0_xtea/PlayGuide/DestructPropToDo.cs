using System;
using UnityEngine;

namespace PlayGuide;

public class DestructPropToDo : ToDoBase
{
	private readonly ulong _targetId;

	public DestructPropToDo(string id)
	{
		_targetId = Convert.ToUInt64(id);
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
		if (!((Object)(object)clientRemovableProp == (Object)null))
		{
			clientRemovableProp.ClientPropDestructed += ClientPropDestructed;
		}
	}

	public override void OnRemoveItem()
	{
		ClientRemovableProp clientRemovableProp = FindProp();
		if (!((Object)(object)clientRemovableProp == (Object)null))
		{
			clientRemovableProp.ClientPropDestructed -= ClientPropDestructed;
		}
	}

	private void ClientPropDestructed(ulong entityId)
	{
		if (entityId == _targetId)
		{
			CallComplete();
		}
	}
}
