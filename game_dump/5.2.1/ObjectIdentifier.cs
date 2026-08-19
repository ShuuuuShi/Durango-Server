using Durango.Logic;
using JetBrains.Annotations;
using UnityEngine;

public static class ObjectIdentifier
{
	public static bool IsTargetableEnemy([CanBeNull] GameObject o, bool includePets)
	{
		if (o == null)
		{
			return false;
		}
		if (!o.CompareTag("Enemy"))
		{
			return false;
		}
		if (!includePets && (bool)o.GetComponent<PetAI>())
		{
			return false;
		}
		CharacterBehavior component = o.GetComponent<CharacterBehavior>();
		if (component != null)
		{
			return component.IsAlive;
		}
		return false;
	}

	public static bool IsTargetablePlayer([CanBeNull] GameObject o)
	{
		if (o == null)
		{
			return false;
		}
		if (!o.CompareTag("Player"))
		{
			return false;
		}
		PlayerBehavior component = o.GetComponent<PlayerBehavior>();
		if (component == null)
		{
			return false;
		}
		if (component.EntityId == PlayerBehavior.LocalPlayer.EntityId)
		{
			return false;
		}
		return component.GetVisible();
	}

	public static bool IsLocalPlayersPet([CanBeNull] GameObject o)
	{
		if (o == null)
		{
			return false;
		}
		PetAI component = o.GetComponent<PetAI>();
		if (component == null)
		{
			return false;
		}
		return component.IsLocalPlayersPet();
	}

	public static bool IsDeadBody([CanBeNull] GameObject o)
	{
		if (o == null)
		{
			return false;
		}
		CharacterBehavior component = o.GetComponent<CharacterBehavior>();
		if (component != null)
		{
			return !component.IsAlive;
		}
		return false;
	}

	public static bool IsAlly(GameObject obj)
	{
		if (GameManager.IsPrologueMode)
		{
			return false;
		}
		if (CombatSystem.IsPvPEnabled())
		{
			if (IsAlly(obj.GetComponent<PlayerBehavior>()))
			{
				return true;
			}
			PetAI component = obj.GetComponent<PetAI>();
			if (component != null && component.Master != null && IsAlly(component.Master.GetComponent<PlayerBehavior>()))
			{
				return true;
			}
		}
		else
		{
			if (obj.GetComponent<PlayerBehavior>() != null)
			{
				return true;
			}
			if (obj.GetComponent<PetAI>() != null)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsAlly([CanBeNull] PlayerBehavior player)
	{
		if (player == null)
		{
			return false;
		}
		if (player.EntityId == PlayerBehavior.LocalPlayer.EntityId)
		{
			return true;
		}
		if (!GameSystem<PartySystem>.Instance().IsInParty(player.EntityId))
		{
			return ClanSystem.IsMyClanOrAlliance(player);
		}
		return true;
	}

	public static string GetEntityId([CanBeNull] GameObject obj)
	{
		if (obj == null)
		{
			return string.Empty;
		}
		ImmovableBase component = obj.GetComponent<ImmovableBase>();
		if (component != null)
		{
			return component.EntityId;
		}
		SelectableObject component2 = obj.GetComponent<SelectableObject>();
		if (component2 != null)
		{
			return component2.EntityId;
		}
		CharacterBehavior component3 = obj.GetComponent<CharacterBehavior>();
		if (component3 != null)
		{
			return component3.EntityId;
		}
		return string.Empty;
	}

	public static int GetEntityType([CanBeNull] GameObject obj)
	{
		if (obj == null)
		{
			return 0;
		}
		ImmovableBase component = obj.GetComponent<ImmovableBase>();
		if (component != null)
		{
			return component.EntityType;
		}
		CharacterBehavior component2 = obj.GetComponent<CharacterBehavior>();
		if (!(component2 != null))
		{
			return 0;
		}
		return component2.EntityTypeId;
	}
}
