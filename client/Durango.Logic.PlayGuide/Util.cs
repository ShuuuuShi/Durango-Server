using System.Collections.Generic;
using Durango.Utils;
using Durango.Utils.Extensions;
using Shared.Faction;
using UnityEngine;

namespace Durango.Logic.PlayGuide;

public static class Util
{
	private static readonly List<GameObject> SearchList = new List<GameObject>();

	public static bool CheckNearAnimal(int typeId)
	{
		SearchList.Clear();
		InteractionSystem.GetNearObjectsInternal(SearchList, LayerHelper.DefaultMask, 800f, (GameObject o) => (!ObjectIdentifier.IsTargetableEnemy(o, includePets: false)) ? null : o);
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

	public static ImmovableBase GetNearestImmovable(int[] types, float radius)
	{
		SearchList.Clear();
		InteractionSystem.GetNearObjectsInternal(SearchList, LayerHelper.PropMask, radius, InteractionSystem.ImmovableObjectFilter);
		ImmovableBase result = null;
		float num = float.MaxValue;
		for (int i = 0; i < SearchList.Count; i++)
		{
			GameObject gameObject = SearchList[i];
			if (!types.Contains(ObjectIdentifier.GetEntityType(gameObject)))
			{
				continue;
			}
			ImmovableBase component = gameObject.GetComponent<ImmovableBase>();
			if (!(component == null))
			{
				float distance = InteractionObject.GetDistance(gameObject);
				if (distance < num)
				{
					result = component;
					num = distance;
				}
			}
		}
		return result;
	}

	public static NPCType FactionTypeToNPCType(FactionType type)
	{
		return type switch
		{
			FactionType.ChlorophylForum => NPCType.ChlorophylForum, 
			FactionType.ChamberOfPioneer => NPCType.ChamberOfPioneer, 
			FactionType.TheFirm => NPCType.TheFirm, 
			FactionType.TheCommittee => NPCType.TheCommittee, 
			FactionType.Lama => NPCType.Lama, 
			FactionType.RescueTf => NPCType.RescueTf, 
			FactionType.SubStory => NPCType.SubStory, 
			_ => NPCType.TheFirm, 
		};
	}
}
