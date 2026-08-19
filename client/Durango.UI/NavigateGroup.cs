using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class NavigateGroup : UIBase
{
	[SerializeField]
	private PointTargetController _pointTargetController;

	[SerializeField]
	private TargetFloatingController _targetFloatingController;

	public PointTargetController Point => _pointTargetController;

	private void Start()
	{
		Connections.Frontend.On(delegate(EntityRescueRequested msg, PacketHeader header)
		{
			PlayerBehavior player2 = Singleton<PlayerManager>.Instance().GetPlayer(msg.EntityId);
			SetCharacterPoint(player2, rescue: true);
		});
		GameSystem<GatheringSystem>.Instance().CollectiblePermissionChanged += delegate(string entityId, bool hasPermission)
		{
			bool flag = hasPermission;
			GameObject gameObject = null;
			if (flag)
			{
				gameObject = Singleton<ObjectManager>.Instance().FindObject(entityId);
				if (gameObject == null)
				{
					flag = false;
				}
			}
			if (flag)
			{
				_targetFloatingController.MakeOrAdd(entityId).SetIcon("icon_map_poi_animal_dead").SetIconColor(Color.white)
					.SetBorderColor(PresetColor.UIYellow)
					.Target.Set(gameObject);
			}
			else
			{
				_targetFloatingController.Release(entityId);
			}
		};
		Singleton<PlayerManager>.Instance().PlayerAppeared += delegate(PlayerBehavior player)
		{
			SetCharacterPoint(player, player.RescueRequested);
		};
		Singleton<PlayerManager>.Instance().PlayerDisappeared += RemoveCharacterPoint;
		Singleton<PetManager>.Instance().PetAppeared += delegate(AnimalBehavior behavior)
		{
			if (!(behavior == null))
			{
				PetAI component2 = behavior.GetComponent<PetAI>();
				if (!(component2 == null) && component2.IsLocalPlayersPet())
				{
					SetCharacterPoint(behavior, !behavior.IsAlive);
					behavior.Died += AnimalBehavior_Died;
				}
			}
		};
		Singleton<PetManager>.Instance().PetDisappeared += delegate(AnimalBehavior behavior)
		{
			if (!(behavior == null))
			{
				PetAI component = behavior.GetComponent<PetAI>();
				if (!(component == null) && component.IsLocalPlayersPet())
				{
					SetCharacterPoint(behavior, rescue: false);
					behavior.Died -= AnimalBehavior_Died;
				}
			}
		};
		SetChildrenActive(activated: true);
	}

	private void RemoveCharacterPoint([CanBeNull] CharacterBehavior characterBehavior)
	{
		if (!(characterBehavior == null))
		{
			SetCharacterPoint(characterBehavior, rescue: false);
		}
	}

	private void AnimalBehavior_Died(CharacterBehavior characterBehavior, bool _)
	{
		SetCharacterPoint(characterBehavior, rescue: true);
	}

	private void SetCharacterPoint([CanBeNull] CharacterBehavior character, bool rescue)
	{
		if (!(character == null))
		{
			string key = GetKey(character);
			character.Revived -= RemoveCharacterPoint;
			if (rescue)
			{
				character.Revived += RemoveCharacterPoint;
				bool flag = character is AnimalBehavior;
				string icon = ((!flag) ? "todo_icon_friend_dead" : "todo_icon_animal_dead");
				Color value = ((!flag) ? PresetColor.UIDarkRed : PresetColor.UIWhite);
				Point.SetTarget(key, new PointTargetController.Arguments
				{
					Position = character.CurrentPosition,
					Icon = icon,
					BorderColor = value
				});
			}
			else
			{
				Point.ClearTarget(key);
			}
		}
	}

	private static string GetKey([NotNull] CharacterBehavior characterBehavior)
	{
		return "rescue_reqeusted_" + characterBehavior.EntityId;
	}
}
