using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Logic.Social;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.UI;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using L10N;
using Messages;
using Shared.Chat;
using UnityEngine;

namespace Durango.Logic.Interactions;

public static class ReactingPropInteractions
{
	private static readonly string[] MourningWords = new string[4]
	{
		T._("좋은 곳으로 가시길."),
		T._("안타까운 죽음이군."),
		T._("이 사람도 가족이 있겠지."),
		T._("슬퍼하는 사람이 있기를.")
	};

	public static void AddInteractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RescueWithFood, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RescueWithMedicine, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RescueWithWater, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RescueWithCpr, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ConfirmIdentity, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.BreakDownRadio, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveRequestedPapers, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveRequestedWallet, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveRequestedNote, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveWater, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveFood, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveArmor, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveClothesRepairKit50, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveJuiceFruit50, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveAxeTool50, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveHammerTool50, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveRope50, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RebootSystem, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GetCoordinates, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.PowerDown, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GetSignal, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GetNoise, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GetStatusEffect, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ArchipelagoGivePoisonsac, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ArchipelagoDiscoverSite, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ArchipelagoGiveLava, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ArchipelagoWeaponStone, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ArchipelagoWeaponBone, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ArchipelagoWeaponMetal, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ArchipelagoGivePaper, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ArchipelagoGiveKeepsake, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ArchipelagoRemoveBug, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicFindSecretDocument, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicContactPioneercouncil, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicContactChlorophylfourm, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicWarehouseFix, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicWarehouseSecretDocument, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicLighthouse, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicSilo, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicTrap, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicLighthouseLightUp, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicSiloAttachBomb, OnInteractionReactingProp);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EpicLastStaion, OnInteractionReactingProp);
	}

	private static void OnInteractionReactingProp(InteractionObject target)
	{
		Touched lastTouched = GameSystem<InteractionSystem>.Instance().LastTouched;
		ReactingProp? reactingProp = lastTouched.ReactingProp;
		if (!reactingProp.HasValue)
		{
			return;
		}
		ReactingProp value = lastTouched.ReactingProp.Value;
		Interaction interaction = (Interaction)value.Interaction;
		if (interaction == Interaction.RescueWithCpr)
		{
			RescueWithCPR(target);
			return;
		}
		string motionName = ((value.Motions.Length <= 0) ? null : value.Motions[UnityEngine.Random.Range(0, value.Motions.Length)]);
		bool flag = value.RequiredItems.HasValue || value.RequiredMoney.HasValue;
		ReactingPropPopup.RequiredItemTags? requiredItemTags = ReactingPropPopup.GetRequiredItemTags(value.RequiredItems);
		if (interaction == Interaction.GetStatusEffect && flag)
		{
			UIManager.Popup.Tooltip<ReactingPropPopup>().Show(requiredItemTags, value.RequiredMoney, value.GivingItems, value.RewardStatusEffect, value.Cooltime, delegate
			{
				ContactReactingPropWithItemUI(requiredItemTags, target, interaction, motionName);
			});
		}
		else
		{
			ContactReactingPropWithItemUI(requiredItemTags, target, interaction, motionName);
		}
	}

	private static void RescueWithCPR(InteractionObject target)
	{
		Singleton<PlayerController>.Instance().PrepareCPR(target.CharacterTarget);
		UIManager.FindScript<CPRGroup>().Open();
		Connections.Frontend.Send(new ContactReactingProp
		{
			EntityId = target.EntityId,
			Tile = new Point2((int)target.Tile.x, (int)target.Tile.y),
			ItemIds = new string[0]
		}).On(delegate(Messages.Timer msg, PacketHeader header)
		{
			Durango.Logic.Timer.Timer timer = new Durango.Logic.Timer.Timer("reacting_prop", msg.Duration);
			timer.Finished += delegate
			{
				CPRGroup cPRGroup = UIManager.FindScript<CPRGroup>();
				cPRGroup.FinishCPR(interrupted: true);
			};
			IconProgressGauge iconProgressGauge = Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer);
			iconProgressGauge.AddIcon(GetGaugeIcon(Interaction.RescueWithCpr));
		});
	}

	private static void ContactReactingPropWithItemUI(ReactingPropPopup.RequiredItemTags? requiredItemTags, InteractionObject target, Interaction interaction, string motionName)
	{
		if (!requiredItemTags.HasValue)
		{
			ContactReactingProp(target, interaction, new ItemIcon(GetGaugeIcon(interaction)), motionName);
			return;
		}
		string interactionName = interaction.GetName();
		PopupItemSelector selector = UIManager.Popup.Tooltip<PopupItemSelector>();
		selector.Filter(requiredItemTags.Value.Filter).SelectableCount(requiredItemTags.Value.Count).OnConfirmed(delegate(IList<ItemData> items)
		{
			if (items != null)
			{
				UIManager.MessageBox.ShowLockConfirm(items, delegate(string[] itemIds)
				{
					ContactReactingProp(target, interaction, (items.Count != 0) ? items[0].Icon : new ItemIcon(GetGaugeIcon(interaction)), motionName, itemIds);
				});
			}
		})
			.OnChanged(delegate(IList<ItemData> items)
			{
				selector.Title($"{interactionName}  {items.Count}/{requiredItemTags.Value.Count}");
			})
			.Title($"{interactionName}  0/{requiredItemTags.Value.Count}")
			.HelpText(requiredItemTags.Value.LocalizedTagRequiredMsg);
		selector.Show(3600f);
	}

	private static void ContactReactingProp(InteractionObject target, Interaction interaction, ItemIcon guageIcon, string motionName, string[] itemIds = null)
	{
		Action onCancelAction = null;
		Artifact targetComponent = target.GetTargetComponent<Artifact>();
		if (string.IsNullOrEmpty(motionName) && targetComponent != null)
		{
			onCancelAction = ArtifactInteractions.SetInteractionMotion(targetComponent, interaction);
		}
		ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new ContactReactingProp
		{
			EntityId = target.EntityId,
			Tile = new Point2((int)target.Tile.x, (int)target.Tile.y),
			ItemIds = ((itemIds != null) ? itemIds : new string[0])
		}).On(delegate(Messages.Timer msg, PacketHeader header)
		{
			IconProgressGauge iconProgressGauge = Durango.Logic.Timer.Timer.Play<IconProgressGauge>(new Durango.Logic.Timer.Timer("reacting_prop", msg.Duration));
			iconProgressGauge.AddIcon(guageIcon);
			if (onCancelAction == null)
			{
				PlayerController.MotionUpdater.Motion(motionName ?? "Barehand_Gather_Low");
			}
		});
		Action onSuccess = GetOnSuccessAction(interaction);
		replyMessageHandlerRegistrar.On<OK>(delegate
		{
			if (onSuccess != null)
			{
				onSuccess();
			}
		}).On(delegate(ReactingPropRewarded msg, PacketHeader header)
		{
			if (KUtility.GetSize(msg.Items) > 0)
			{
				ReceiveRewardsPopup receiveRewardsPopup = UIManager.Popup.Tooltip<ReceiveRewardsPopup>();
				receiveRewardsPopup.ShowReactingPropRewardItems(msg.Items);
			}
		}).Rest(delegate(Packet packet)
		{
			if (!Packet.IsSuccess(packet) && onCancelAction != null)
			{
				onCancelAction();
			}
		});
	}

	private static Action GetOnSuccessAction(Interaction interaction)
	{
		return (interaction != Interaction.ConfirmIdentity) ? null : new Action(OnSuccessConfirmIdentity);
	}

	private static string GetGaugeIcon(Interaction interaction)
	{
		return interaction switch
		{
			Interaction.ConfirmIdentity => "act_GetProfile", 
			Interaction.RescueWithCpr => "act_Resurrect", 
			Interaction.RescueWithFood => "warp_plasticbag_energybar", 
			Interaction.RescueWithWater => "act_drinkwater", 
			Interaction.RescueWithMedicine => "act_heal", 
			Interaction.GiveRequestedPapers => "icon_papers", 
			Interaction.GiveRequestedWallet => "icon_wallet", 
			Interaction.GiveRequestedNote => "warp_book", 
			Interaction.GiveWater => "act_drinkwater", 
			Interaction.GiveFood => "cook_skewer", 
			Interaction.GiveArmor => "clothes_beginner_leaf", 
			Interaction.GiveClothesRepairKit50 => "repair_clothes_03", 
			Interaction.GiveJuiceFruit50 => "act_drinkwater", 
			Interaction.GiveAxeTool50 => "skill_meleecrafting_axe_1", 
			Interaction.GiveHammerTool50 => "icon_tool_onehanded_hammer_assembled", 
			Interaction.GiveRope50 => "material_rope", 
			Interaction.RebootSystem => "guide_hand_touched", 
			Interaction.GetCoordinates => "explore_compass", 
			Interaction.PowerDown => "guide_hand_touched", 
			Interaction.GetSignal => "faction_icon_walkie", 
			Interaction.GetNoise => "faction_icon_walkie", 
			Interaction.GetStatusEffect => "act_buff_artifact", 
			Interaction.ArchipelagoGivePoisonsac => "event_unstable_material", 
			Interaction.ArchipelagoDiscoverSite => "skill_discover_1", 
			Interaction.ArchipelagoGiveLava => "icon_nat_lava", 
			Interaction.ArchipelagoWeaponStone => "repair_weapon_01", 
			Interaction.ArchipelagoWeaponBone => "repair_weapon_01", 
			Interaction.ArchipelagoWeaponMetal => "repair_weapon_01", 
			Interaction.ArchipelagoRemoveBug => "act_Repair", 
			Interaction.ArchipelagoGivePaper => "icon_papers", 
			Interaction.ArchipelagoGiveKeepsake => "box_01_army", 
			Interaction.EpicFindSecretDocument => "repair_weapon_01", 
			Interaction.EpicContactChlorophylfourm => "act_talkie", 
			Interaction.EpicContactPioneercouncil => "act_talkie", 
			Interaction.EpicWarehouseFix => "act_Repair", 
			Interaction.EpicWarehouseSecretDocument => "act_take", 
			Interaction.EpicLighthouse => "act_inspection", 
			Interaction.EpicSilo => "act_inspection", 
			Interaction.EpicTrap => "act_inspection", 
			Interaction.EpicLighthouseLightUp => "act_Fire", 
			Interaction.EpicSiloAttachBomb => "act_attach_bomb", 
			Interaction.EpicLastStaion => "act_inspection", 
			_ => "act_heal", 
		};
	}

	[ExposedInEditor(null)]
	private static void OnSuccessConfirmIdentity()
	{
		ChatStruct chatStruct = new ChatStruct();
		chatStruct.EntityId = PlayerBehavior.LocalPlayer.EntityId;
		chatStruct.Chatter = PlayerBehavior.LocalPlayer.ChatableBase;
		chatStruct.Body = new RadioNotice
		{
			Text = MourningWords[UnityEngine.Random.Range(0, MourningWords.Length)]
		};
		chatStruct.Name = PlayerBehavior.LocalPlayer.GetName();
		chatStruct.Emotion = PortraitEmotion.Normal;
		chatStruct.Type = ChannelType.System;
		chatStruct.Duration = 3f;
		chatStruct.IsVolatile = true;
		ChatStruct chat = chatStruct;
		GameSystem<SocialSystem>.Instance().AddChat(chat);
	}
}
