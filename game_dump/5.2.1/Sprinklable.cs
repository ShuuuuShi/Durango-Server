using Durango.UI.InGame;
using Durango.Utils;
using InteractionData;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class Sprinklable : ArtifactComponent
{
	public static Color WateringTileColor => Durango.Utils.Singleton<BuildLocator>.Instance().WateringTileColor;

	public static Color FertilizingTileColor => Durango.Utils.Singleton<BuildLocator>.Instance().FertilizingTileColor;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void AddStaticEvents()
	{
		GameManager.Started += delegate
		{
			GameSystem<InteractionSystem>.Instance().PostTouched += SetSprinklerMenu;
		};
	}

	public override bool OnUpdateState(double eventTime)
	{
		SprinklerState? sprinkler = base.Artifact.ArtifactState.Sprinkler;
		if (!sprinkler.HasValue)
		{
			return false;
		}
		InteractionSystem interactionSystem = GameSystem<InteractionSystem>.Instance();
		InteractionObject target = interactionSystem.Target;
		if (target == null || target.EntityId != base.EntityId)
		{
			return false;
		}
		InteractionMenuList menuList = interactionSystem.MenuList;
		int num = menuList.IndexOf(Interaction.Sprinkle);
		if (num == -1)
		{
			return false;
		}
		menuList[num] = SetSprinklerMenu(menuList[num], sprinkler.Value);
		menuList.Apply();
		return false;
	}

	private static void SetSprinklerMenu(InteractionMenuList menuList, InteractionObject target)
	{
		int num = menuList.IndexOf(Interaction.Sprinkle);
		if (num != -1)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			SprinklerState? sprinklerState = ((!(targetComponent == null)) ? targetComponent.ArtifactState.Sprinkler : null);
			if (sprinklerState.HasValue)
			{
				menuList[num] = SetSprinklerMenu(menuList[num], sprinklerState.Value);
			}
		}
	}

	private static InteractionMenuData SetSprinklerMenu(InteractionMenuData menu, SprinklerState sprinkler)
	{
		menu.Count = sprinkler.ChargeCount;
		menu.Duration = Yaml.Util.Singleton<Constants>.Instance.Sprinkler.SprinkleWater.Duration;
		return menu;
	}
}
