using System.Collections.Generic;
using UnityEngine;

namespace PlayGuide;

public static class Util
{
	private static readonly List<GameObject> SearchList = new List<GameObject>();

	public static bool CheckNearAnimal(int typeId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		InteractionSystem.GetNearObjectsInternal(SearchList, LayerMask.op_Implicit(LayerHelper.DefaultMask), 800f, (GameObject o) => (!ObjectIdentifier.IsTargetableEnemy(o, includePets: false)) ? null : o);
		if (SearchList.Count > 0 && typeId == 0)
		{
			return true;
		}
		for (int i = 0; i < SearchList.Count; i++)
		{
			GameObject obj = SearchList[i];
			if (ObjectIdentifier.GetEntityType(obj) == typeId)
			{
				return true;
			}
		}
		return false;
	}

	public static GameObject GetNearImmovable(int[] types, float radius)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		InteractionSystem.GetNearObjectsInternal(SearchList, LayerMask.op_Implicit(LayerHelper.PropMask), radius, InteractionSystem.ImmovableObjectFilter);
		for (int i = 0; i < SearchList.Count; i++)
		{
			GameObject val = SearchList[i];
			if (types.Contains(ObjectIdentifier.GetEntityType(val)))
			{
				return val;
			}
		}
		return null;
	}
}
