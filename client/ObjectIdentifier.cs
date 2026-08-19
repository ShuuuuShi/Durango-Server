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
		return component != null && component.IsAlive;
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
		return component != null && !component.IsAlive;
	}

	public static bool IsAlly(GameObject obj)
	{
		if (GameManager.IsPrologueMode)
		{
			return false;
		}
		if (CombatSystem.IsPvPEnabled())
		{
			PlayerBehavior component = obj.GetComponent<PlayerBehavior>();
			if (IsAlly(component))
			{
				return true;
			}
			PetAI component2 = obj.GetComponent<PetAI>();
			if (component2 != null && component2.Master != null)
			{
				PlayerBehavior component3 = component2.Master.GetComponent<PlayerBehavior>();
				if (IsAlly(component3))
				{
					return true;
				}
			}
		}
		else
		{
			PlayerBehavior component4 = obj.GetComponent<PlayerBehavior>();
			if (component4 != null)
			{
				return true;
			}
			PetAI component5 = obj.GetComponent<PetAI>();
			if (component5 != null)
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
		return GameSystem<PartySystem>.Instance().IsInParty(player.EntityId) || ClanSystem.IsMyClanOrAlliance(player);
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
		return (!(component3 != null)) ? string.Empty : component3.EntityId;
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
		return (component2 != null) ? component2.EntityTypeId : 0;
	}
}
