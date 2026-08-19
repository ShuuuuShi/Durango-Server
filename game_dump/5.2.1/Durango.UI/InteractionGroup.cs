using System;
using System.Runtime.CompilerServices;
using Durango.Logic;
using Durango.Logic.Interactions;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Render.Camera;
using Durango.System;
using Durango.Terrain;
using Durango.UI.Control;
using Durango.UI.InGame;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using InteractionData;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class InteractionGroup : UIBase
{
	public Action InteractionMenuShowed;

	[SerializeField]
	private InteractionMenuListWidgetBase _interactionMenu;

	[SerializeField]
	private bool _ignoreInteractionOnMove;

	private GameObject _hoveredPickingObject;

	private InteractionObject _selectedPickingObject;

	[CompilerGenerated]
	private static InteractionSystem.InteractionHandler cache0;

	[CompilerGenerated]
	private static InteractionSystem.InteractionHandler cache1;

	[CompilerGenerated]
	private static InteractionSystem.InteractionHandler cache2;

	[CompilerGenerated]
	private static InteractionSystem.InteractionHandler cache3;

	[CompilerGenerated]
	private static InteractionSystem.InteractionHandler cache4;

	[CompilerGenerated]
	private static InteractionSystem.InteractionHandler cache5;

	[CompilerGenerated]
	private static InteractionSystem.InteractionHandler cache6;

	[CompilerGenerated]
	private static InteractionSystem.InteractionHandler cache7;

	public InteractionMenuListWidgetBase InteractionMenu => _interactionMenu;

	private void Start()
	{
		AddInteractionHandler();
		InteractionMenuListWidgetBase interactionMenu = _interactionMenu;
		interactionMenu.OnClickInteractionMenu = (Action<InteractionMenuData>)Delegate.Combine(interactionMenu.OnClickInteractionMenu, (Action<InteractionMenuData>)delegate(InteractionMenuData data)
		{
			OnClickInteractionMenu(data, selectAll: false);
		});
		InteractionMenuListWidgetBase interactionMenu2 = _interactionMenu;
		interactionMenu2.OnLongPressInteractionMenu = (Action<InteractionMenuData>)Delegate.Combine(interactionMenu2.OnLongPressInteractionMenu, (Action<InteractionMenuData>)delegate(InteractionMenuData data)
		{
			OnClickInteractionMenu(data, selectAll: true);
		});
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			if (!PlayerBehavior.LocalPlayer.IsAlive)
			{
				ShowPlayerDeadInteractionMenu();
			}
		});
		GameSystem<InteractionSystem>.Instance().InteractionTargetSelected += OnSelectInteractionTarget;
		Durango.Utils.Singleton<PlayerController>.Instance().MoveStarted += OnStartMove;
		GameSystem<InputSystem>.Instance().On(InputCommand.HoverPicking, OnHoverPickingObject);
		GameSystem<InputSystem>.Instance().On(InputCommand.TouchPicking, OnTouchPickingObject);
		GameSystem<TimerSystem>.Instance().StartSubjectProgress += OnStartSubjectProgress;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += delegate
		{
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		};
	}

	private void OnStartSubjectProgress(string subject)
	{
		if (subject.TryEnum<Interaction>(out var value) && !InteractionMenuData.IsKeepInteractionMenuAction(value))
		{
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		}
	}

	private void OnStartMove()
	{
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
	}

	private void OnSelectInteractionTarget(InteractionObject obj)
	{
		InteractionObject selectedPickingObject = _selectedPickingObject;
		MarkingTarget(obj);
		if (obj == null)
		{
			_interactionMenu.Hide();
			if (selectedPickingObject != null && PlayerBehavior.LocalPlayer != null && Durango.Utils.Singleton<CameraController>.HasInstance())
			{
				Durango.Utils.Singleton<CameraController>.Instance().Target(null, 0.3f);
			}
		}
		else if (!GameSystem<CombatSystem>.Instance().CombatMode)
		{
			Durango.Utils.Singleton<CameraController>.Instance().Target(obj.Target, 0.3f);
			_interactionMenu.Show();
			if (InteractionMenuShowed != null)
			{
				InteractionMenuShowed();
			}
		}
	}

	private void MarkingCancel()
	{
		if (_selectedPickingObject != null && _selectedPickingObject.Target != null && _selectedPickingObject.Target.activeSelf)
		{
			ImmovableBase targetComponent = _selectedPickingObject.GetTargetComponent<ImmovableBase>();
			if (targetComponent != null)
			{
				targetComponent.Select(selected: false);
				targetComponent.Hover(hovered: false);
			}
			else
			{
				CharacterBehavior targetComponent2 = _selectedPickingObject.GetTargetComponent<CharacterBehavior>();
				if (targetComponent2 != null)
				{
					targetComponent2.Select(selected: false);
				}
			}
		}
		_selectedPickingObject = null;
	}

	private void MarkingTarget(InteractionObject obj)
	{
		MarkingCancel();
		_selectedPickingObject = obj;
		if (_selectedPickingObject == null || !(_selectedPickingObject.Target != null) || !_selectedPickingObject.Target.activeSelf)
		{
			return;
		}
		bool flag = ObjectIdentifier.IsAlly(obj.Target);
		ImmovableBase targetComponent = _selectedPickingObject.GetTargetComponent<ImmovableBase>();
		if (targetComponent != null)
		{
			targetComponent.Select(selected: true);
			return;
		}
		CharacterBehavior targetComponent2 = _selectedPickingObject.GetTargetComponent<CharacterBehavior>();
		if (targetComponent2 != null)
		{
			targetComponent2.Select(selected: true, (!flag) ? Color.red : Color.white, 1f);
		}
	}

	public void ShowPlayerDeadInteractionMenu()
	{
		if (!GameManager.Region.CanRevive())
		{
			return;
		}
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		if (!GameSystem<StatisticsSystem>.Instance().IsNewbie)
		{
			menuList.Add(new InteractionMenuData(Interaction.SetReviveReward));
		}
		if (Durango.Utils.Singleton<MapIndicators>.Instance().HasOneOrMoreWarpHoles())
		{
			menuList.Add(new InteractionMenuData(Interaction.ReviveAtWarphole));
		}
		ReviveImmediatelyCost reviveImmediately = Yaml.Util.Singleton<CostsYaml>.Instance.ReviveImmediately;
		string arg = (reviveImmediately.HasVoucherFromCommodity() ? T._("무료") : ((!reviveImmediately.HasVoucher()) ? Durango.Logic.Item.Inventory.CurrencyFormat(reviveImmediately.Amount, reviveImmediately.Currency) : T._("[icon={0}] {1}", reviveImmediately.GetVoucher().VoucherId, 1)));
		InteractionMenuData data = new InteractionMenuData(Interaction.ReviveImmediately);
		data.Name = string.Format("{0} {1}", T._("즉시 부활"), arg);
		menuList.Add(data);
		InteractionMenuData data2 = new InteractionMenuData(Interaction.Revive);
		if (GameManager.Region.Role() == Role.Risky)
		{
			if (GameSystem<MapSystem>.Instance().Points.CampPoint.HasValue)
			{
				data2.Name = T._("캠프에서 부활");
			}
			else
			{
				data2.Name = T._("부활");
			}
		}
		else
		{
			data2.Name = T._("귀환 후 부활");
		}
		menuList.Add(data2);
		menuList.Name = string.Empty;
		GameSystem<InteractionSystem>.Instance().ShowClientMenuList();
	}

	private static void OnClickInteractionMenu(InteractionMenuData menu, bool selectAll)
	{
		GameSystem<InteractionSystem>.Instance().SelectTargetInteractionMenu(menu, selectAll);
	}

	private void OnHoverPickingObject(InputCommandMessage message)
	{
		GameObject gameObject = null;
		gameObject = InteractionUtil.PickingObject(null, message.PickingRay, message.PickingTouchEvent.CurrentPos, out var _, null);
		if (message.PickingTouchEvent.IsNguiTouched)
		{
			gameObject = null;
		}
		if (_selectedPickingObject != null && !(_selectedPickingObject.Target != _hoveredPickingObject))
		{
			return;
		}
		if (_hoveredPickingObject != null && _hoveredPickingObject != gameObject)
		{
			ImmovableBase component = _hoveredPickingObject.GetComponent<ImmovableBase>();
			if (component != null)
			{
				component.Hover(hovered: false);
			}
		}
		if (gameObject != null)
		{
			ImmovableBase component2 = gameObject.GetComponent<ImmovableBase>();
			if (component2 != null)
			{
				component2.Hover(hovered: true);
			}
			_hoveredPickingObject = gameObject;
		}
	}

	private void OnTouchPickingObject(InputCommandMessage message)
	{
		if (!base.Visible || ((bool)PlayerBehavior.LocalPlayer.IsMoving && _ignoreInteractionOnMove && !PlayerController.MotionUpdater.IsWaterCarried) || GameSystem<CombatSystem>.Instance().CombatMode)
		{
			return;
		}
		GameSystem<InputSystem>.Instance().Touch.NotifyObjectPicked();
		if (!PlayerBehavior.LocalPlayer.IsAlive)
		{
			if (GameSystem<InteractionSystem>.Instance().Target == null)
			{
				ShowPlayerDeadInteractionMenu();
			}
			else
			{
				GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
			}
			return;
		}
		Ray pickingRay = message.PickingRay;
		InputTouch.TouchEvent pickingTouchEvent = message.PickingTouchEvent;
		float sqrMagnitude = (pickingTouchEvent.CurrentPos - pickingTouchEvent.BeginPos).sqrMagnitude;
		float dragThreshold = Durango.Utils.Singleton<PlayerController>.Instance().DragThreshold;
		if (sqrMagnitude > dragThreshold * dragThreshold)
		{
			return;
		}
		bool isPrev;
		GameObject gameObject = InteractionUtil.PickingObject((_selectedPickingObject != null) ? _selectedPickingObject.Target : null, pickingRay, pickingTouchEvent.CurrentPos, out isPrev, null);
		if (gameObject != null)
		{
			if (Platform.Instance.UsePCUI && (bool)PlayerBehavior.LocalPlayer.IsMoving && !GameSystem<InteractionSystem>.Instance().IsIgnoreInteraction())
			{
				Durango.Utils.Singleton<PlayerController>.Instance().StopMove();
			}
			InteractionObject interactionTarget = new InteractionObject(gameObject);
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(interactionTarget);
		}
		else if (isPrev || _selectedPickingObject != null)
		{
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		}
	}

	private static void AddInteractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Revive, delegate
		{
			string mainText2 = ((GameManager.Region.Role() != Role.Risky) ? ((!GameSystem<StatisticsSystem>.Instance().IsNewbie) ? T._("<em>귀환 지점</em>에서 부활하시겠습니까?\n죽은 곳에 아이템 일부가 떨어집니다.\n(상점에서 구매한 아이템 포함)") : T._("<em>귀환</em> 하시겠습니까?")) : ((!GameSystem<MapSystem>.Instance().Points.CampPoint.HasValue) ? T._("부활하시겠습니까?\n죽은 곳에 아이템 일부가 떨어집니다.\n(상점에서 구매한 아이템 포함)") : T._("캠프로 <em>귀환</em>해 부활하시겠습니까?\n죽은 곳에 아이템 일부가 떨어집니다.\n(상점에서 구매한 아이템 포함)")));
			UIManager.MessageBox.Show(mainText2, delegate(bool ok)
			{
				if (ok)
				{
					Durango.Utils.Singleton<PlayerController>.Instance().ResurrectionRequest();
				}
			});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ReviveAtWarphole, delegate
		{
			Connections.Frontend.Send(default(GetWarpCosts)).On(delegate(WarpCosts msg, PacketHeader _)
			{
				if (msg.Costs.Length != 0)
				{
					UIManager.FindScript<WorldMapGroup>().OpenForRevive();
				}
				else
				{
					UIManager.SystemMsg(T._("이 섬에서 워프홀을 발견한 적이 없어서, 워프홀에서 부활할 수 없습니다."));
				}
			});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ReviveImmediately, delegate
		{
			MessageBox messageBox = UIManager.MessageBox;
			PresetButton.Effect effect = PresetButton.Effect.None;
			ReviveImmediatelyCost reviveImmediately = Yaml.Util.Singleton<CostsYaml>.Instance.ReviveImmediately;
			string voucherId;
			string text;
			if (reviveImmediately.HasVoucherFromCommodity())
			{
				VoucherWithCommodity voucherFromCommodity = reviveImmediately.GetVoucherFromCommodity();
				voucherId = voucherFromCommodity.VoucherId;
				int countMax = SingletonDict<string, Voucher>.Instance.Get(voucherId).CountMax;
				int voucherCount = InventorySystem.Wallet.GetVoucherCount(voucherId);
				text = string.Format("{0}  [preset=round_box?{1} ]", T._("확인"), T._("무료 ({0}/{1})", voucherCount, countMax));
				string textFormat = T._("<em>{0}</em> 의 효과로 하루에 {1}번 무료로 부활할 수 있습니다.  <bar/>  <em>{2} 남음</em>");
				string packageEffectText = Vouchers.GetPackageEffectText(voucherId, voucherFromCommodity.IncludingCommodityId, textFormat);
				messageBox.SetLowerText(packageEffectText);
				effect = PresetButton.Effect.Emphasis;
			}
			else if (reviveImmediately.HasVoucher())
			{
				voucherId = reviveImmediately.GetVoucher().VoucherId;
				text = string.Format("{0}  [preset=round_box?[icon={1}]  1 ]", T._("확인"), voucherId);
			}
			else
			{
				voucherId = null;
				text = Durango.Logic.Item.Inventory.ToCurrencyButtonText(T._("확인"), reviveImmediately.Amount, reviveImmediately.Currency);
			}
			messageBox.SetVoucherInfo(reviveImmediately.GetVoucher().VoucherId);
			string mainText = T._("즉시 부활하시겠습니까?");
			Action<int> onSelect = delegate(int index)
			{
				if (index == 0)
				{
					Durango.Utils.Singleton<PlayerController>.Instance().RequestReviveImmediately(voucherId);
				}
			};
			MessageBox.Button[] array = new MessageBox.Button[2];
			ref MessageBox.Button reference = ref array[0];
			string text2 = text;
			PresetButton.Effect effect2 = effect;
			reference = new MessageBox.Button(text2, PresetButton.Style.Solid, null, disabled: false, effect2);
			ref MessageBox.Button reference2 = ref array[1];
			reference2 = T._("취소");
			messageBox.Show(mainText, onSelect, array);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GetProfile, delegate(InteractionObject target)
		{
			PlayerBehavior targetComponent5 = target.GetTargetComponent<PlayerBehavior>();
			if (!(targetComponent5 == null) && !string.IsNullOrEmpty(targetComponent5.EntityId))
			{
				PlayerInfoPopup.RequestShow(targetComponent5.EntityId);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Whisper, delegate(InteractionObject target)
		{
			PlayerBehavior targetComponent4 = target.GetTargetComponent<PlayerBehavior>();
			if (!(targetComponent4 == null) && !string.IsNullOrEmpty(targetComponent4.EntityId))
			{
				UIManager.FindScript<ChattingGroupBase>().Open(targetComponent4.EntityId);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.InviteIntoParty, delegate(InteractionObject target)
		{
			PlayerBehavior targetComponent3 = target.GetTargetComponent<PlayerBehavior>();
			if (!(targetComponent3 == null) && !string.IsNullOrEmpty(targetComponent3.EntityId))
			{
				PartySystem partySystem = GameSystem<PartySystem>.Instance();
				if (partySystem.CanInvite(targetComponent3.EntityId))
				{
					partySystem.InviteIntoParty(targetComponent3.EntityId);
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.HelpPostprocess, delegate(InteractionObject target)
		{
			Artifact targetComponent2 = target.GetTargetComponent<Artifact>();
			if (targetComponent2 != null && targetComponent2.ArtifactState.Postprocess.HasValue)
			{
				BuildPostprocessHelpTooltip buildPostprocessHelpTooltip = UIManager.Popup.Tooltip<BuildPostprocessHelpTooltip>();
				buildPostprocessHelpTooltip.Set(targetComponent2);
				buildPostprocessHelpTooltip.Show();
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GiveUpDistribution, delegate(InteractionObject target)
		{
			string id = target.EntityId;
			Point2 tile = new Point2(target.Tile);
			UIManager.MessageBox.Show(T._("전리품의 권한을 포기하시겠습니까?"), T._("[icon=icon_make_alert] 모두에게 전리품 획득 권한이 공유됩니다."), delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<GatheringSystem>.Instance().GiveUpDistribution(new PropKey
					{
						EntityId = id,
						Tile = tile
					});
					GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
				}
			});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SearchWarphole, SearchWarpholes);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.WashBody, WashBody);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SelectDrawContainer, SelectDrawContainer);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.DrawWater, DrawWater);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.DrawLava, DrawLava);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.DrinkWater, DrinkWater);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.InteractionArtifact, InteractArtifact);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.LookAroundArtifact, LookAroundArtifact);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.CaptureScreenShot, delegate
		{
			UIManager.FindScript<ScreenCaptureGroup>().Open();
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Dash, delegate
		{
			Durango.Utils.Singleton<PlayerController>.Instance().TryJump();
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RemoveNatural, delegate(InteractionObject obj)
		{
			NaturalObject targetComponent = obj.GetTargetComponent<NaturalObject>();
			if ((bool)targetComponent)
			{
				Connections.Frontend.Send(new DisappearEntityOnTile
				{
					EntityId = targetComponent.EntityId,
					Tile = targetComponent.WorldTile
				});
			}
		});
		ReactingPropInteractions.AddInteractionHandler();
	}

	private static void SearchWarpholes(InteractionObject target)
	{
		GameSystem<InteractionSystem>.Instance().SearchWarpholes(delegate(SearchedPOIs msg)
		{
			Durango.Utils.Singleton<PlayerController>.Instance().StopMove();
			if (!PlayerBehavior.LocalPlayer.IsRiding)
			{
				PlayerController.MotionUpdater.Motion("Warp_Find", 5f, 1f, forceTransition: true);
			}
			Durango.Utils.Singleton<DetectWarpHoleUI>.Instance().ShowScanner(msg.Results);
		});
	}

	private static void WashBody(InteractionObject target)
	{
		GameSystem<InteractionSystem>.Instance().WashBody();
	}

	private static void SelectDrawContainer(InteractionObject target)
	{
		string id = InteractionSystem.CurrentMenu.Id;
		PutInContainerInfo putInContainerInfo = Yaml.Util.Singleton<Constants>.Instance.PutInContainerInfos.Get(id);
		if (putInContainerInfo == null)
		{
			return;
		}
		Interaction interaction = Interaction.Invalid;
		if (!(id == "water"))
		{
			if (id == "lava")
			{
				interaction = Interaction.DrawLava;
			}
		}
		else
		{
			interaction = Interaction.DrawWater;
		}
		Durango.Logic.Item.Inventory playerInventory = GameSystem<InventorySystem>.Instance().PlayerInventory;
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		string[] tags = putInContainerInfo.Tags;
		foreach (string text in tags)
		{
			int count = playerInventory.Items.Count;
			for (int j = 0; j < count; j++)
			{
				ItemData itemData = playerInventory.Items[j];
				if (!itemData.IsDestroyed() && itemData.HasTag(text))
				{
					InteractionMenuData data = new InteractionMenuData(interaction);
					data.Id = itemData.Id;
					data.Name = T._("{0} {1:lv:}", itemData.Name, itemData.Level);
					data.Icon = itemData.Icon;
					menuList.Add(data);
				}
			}
		}
		if (menuList.Count > 0)
		{
			GameSystem<InteractionSystem>.Instance().ShowClientMenuList();
			return;
		}
		switch (interaction)
		{
		case Interaction.DrawWater:
			GameSystem<InteractionSystem>.Instance().Draw(default(DrawWater));
			break;
		case Interaction.DrawLava:
			GameSystem<InteractionSystem>.Instance().Draw(default(DrawLava));
			break;
		}
	}

	private static void DrawWater(InteractionObject target)
	{
		ItemData item = GameSystem<InventorySystem>.Instance().FindItem(InteractionSystem.CurrentMenu.Id);
		if (item != null)
		{
			UIManager.MessageBox.ShowLockConfirm(item, delegate
			{
				GameSystem<InteractionSystem>.Instance().Draw(new DrawWater
				{
					ToolItemId = item.Id
				});
			});
		}
	}

	private static void DrawLava(InteractionObject target)
	{
		ItemData item = GameSystem<InventorySystem>.Instance().FindItem(InteractionSystem.CurrentMenu.Id);
		if (item != null)
		{
			UIManager.MessageBox.ShowLockConfirm(item, delegate
			{
				GameSystem<InteractionSystem>.Instance().Draw(new DrawLava
				{
					ToolItemId = item.Id
				});
			});
		}
	}

	private static void DrinkWater(InteractionObject target)
	{
		GameSystem<InteractionSystem>.Instance().DrinkWater();
	}

	private static void InteractArtifact(InteractionObject target)
	{
		TileObject tileObject = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileObject(new Point2(Durango.Terrain.Util.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition)), warning: false);
		if (tileObject != null && !(tileObject.Artifact == null))
		{
			InteractionObject interactionTarget = new InteractionObject(tileObject.Artifact.gameObject);
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(interactionTarget);
		}
	}

	private static void LookAroundArtifact(InteractionObject target)
	{
		Point2 point = new Point2(Durango.Terrain.Util.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition));
		TileObject tileObject = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileObject(point, warning: false);
		if (tileObject != null && !(tileObject.Artifact == null))
		{
			GameSystem<InteractionSystem>.Instance().ArtifactLookAround(point, tileObject.Artifact);
		}
	}
}
