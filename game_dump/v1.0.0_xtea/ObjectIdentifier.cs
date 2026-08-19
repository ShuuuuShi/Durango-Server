using ClanData;
using JetBrains.Annotations;
using UnityEngine;

internal static class ObjectIdentifier
{
	public static bool IsTargetableEnemy([CanBeNull] GameObject o, bool includePets)
	{
		if ((Object)(object)o == (Object)null)
		{
			return false;
		}
		if (!o.CompareTag("Enemy"))
		{
			return false;
		}
		if (!includePets && Object.op_Implicit((Object)(object)o.GetComponent<PetAI>()))
		{
			return false;
		}
		CharacterBehavior component = o.GetComponent<CharacterBehavior>();
		return (Object)(object)component != (Object)null && component.IsAlive;
	}

	public static bool IsTargetablePlayer([CanBeNull] GameObject o, bool filterClan)
	{
		if ((Object)(object)o == (Object)null)
		{
			return false;
		}
		if (!o.CompareTag("Player"))
		{
			return false;
		}
		PlayerBehavior component = o.GetComponent<PlayerBehavior>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		if (component.EntityId == PlayerBehavior.LocalPlayer.EntityId)
		{
			return false;
		}
		if (filterClan && GameSystem<ClanSystem>.Instance().GetClanWarState(component.ClanId) != ClanWarState.Match)
		{
			return false;
		}
		return component.GetRenderEnabled();
	}

	public static bool IsDeadBody([CanBeNull] GameObject o)
	{
		if ((Object)(object)o == (Object)null)
		{
			return false;
		}
		CharacterBehavior component = o.GetComponent<CharacterBehavior>();
		return (Object)(object)component != (Object)null && !component.IsAlive;
	}

	public static ulong GetEntityId([CanBeNull] GameObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return 0uL;
		}
		ImmovableBase component = obj.GetComponent<ImmovableBase>();
		if ((Object)(object)component != (Object)null)
		{
			return component.EntityId;
		}
		SelectableObject component2 = obj.GetComponent<SelectableObject>();
		if ((Object)(object)component2 != (Object)null)
		{
			return component2.EntityId;
		}
		CharacterBehavior component3 = obj.GetComponent<CharacterBehavior>();
		return (!((Object)(object)component3 != (Object)null)) ? 0 : component3.EntityId;
	}

	public static int GetEntityType([CanBeNull] GameObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return 0;
		}
		ImmovableBase component = obj.GetComponent<ImmovableBase>();
		if ((Object)(object)component != (Object)null)
		{
			return component.EntityType;
		}
		CharacterBehavior component2 = obj.GetComponent<CharacterBehavior>();
		return ((Object)(object)component2 != (Object)null) ? component2.EntityTypeId : 0;
	}

	public static bool IsWarphole(int type)
	{
		return type == 15001;
	}

	public static bool IsCrater(int type)
	{
		return type == 15002;
	}

	public static bool IsPort(int type)
	{
		return type == 7001;
	}
}
