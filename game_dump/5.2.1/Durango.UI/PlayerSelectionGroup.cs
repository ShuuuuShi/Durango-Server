using System.Collections.Generic;
using Durango.Logic.Clusters;
using Durango.Logic.Shop;
using Durango.Network;
using Durango.Offline;
using L10N;
using Shared.Purchaser;
using UnityEngine;

namespace Durango.UI;

public class PlayerSelectionGroup : UIBase
{
	[SerializeField]
	private PlayerSlotList _playerList;

	[SerializeField]
	private PlayerPreviewPage _playerPreviewPage;

	private readonly PlayerContext _playerContext;

	private void Start()
	{
		_playerList.ButtonClicked += OnPlayerSlotActionButtonClicked;
		_playerList.SlotSelected += OnPlayerSlotSelected;
		base.VisibleController.Changed += PlayerSelectionGroup_OnVisible;
		GameSystem<PlayerSelectionSystem>.Instance().AccountsUpdated += OnAccountsUpdated;
		SetChildrenActive(activated: false);
	}

	public override bool Open()
	{
		UIManager.Popup.LoadingRing.AttachToWidget(base.gameObject);
		GameSystem<PlayerSelectionSystem>.Instance().UpdateAccounts(delegate
		{
			_playerList.Select(_playerContext.EntityId);
		});
		UIManager.SystemMsg("Test");
		return base.Open();
	}

	private void OnAccountsUpdated(List<PlayerInfo> players)
	{
		PlayerSelectionSystem playerSelectionSystem = GameSystem<PlayerSelectionSystem>.Instance();
		_playerList.Set(players, playerSelectionSystem.EmptySlotCount, playerSelectionSystem.PlayerSlotCount, playerSelectionSystem.LockedSlotCount, playerSelectionSystem.PlayerSlotExceeded);
		PlayerSlotNode selectedNode = _playerList.GetSelectedNode();
		if (selectedNode != null)
		{
			_playerPreviewPage.Set(selectedNode.Type, selectedNode.PlayerInfo);
		}
		UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
	}

	private void OnPlayerSlotSelected(PlayerSlotNode.SlotType slotType, PlayerInfo playerInfo)
	{
		_playerPreviewPage.Set(slotType, playerInfo);
	}

	private void OnPlayerSlotActionButtonClicked(PlayerSlotNode.SlotType slotType, string playerEntityId)
	{
		PlayerSelectionSystem system = GameSystem<PlayerSelectionSystem>.Instance();
		switch (slotType)
		{
		case PlayerSlotNode.SlotType.Empty:
			UIManager.MessageBox.Show(T._("<em>프롤로그</em>를 건너뛰시겠습니까?"), delegate(int index)
			{
				if (index != 2)
				{
					system.CreateNewPlayer(index == 0);
				}
			}, new MessageBox.Button(T._("건너뛰기")), T._("진행"), T._("생성 취소"));
			break;
		case PlayerSlotNode.SlotType.Locked:
			GameSystem<ShopSystem>.Instance().GetPurchasableCommodities(delegate(List<Commodity> list)
			{
				Commodity commodity = list.Find((Commodity x) => x.Data.Type == CommodityType.PlayerSlot);
				if (commodity != null)
				{
					UIManager.FindScript<ShopGroup>().Open(commodity.Id, select: true);
				}
			});
			break;
		case PlayerSlotNode.SlotType.HasPlayer:
		{
			PlayerInfo playerInfo = system.FindPlayerInfo(playerEntityId);
			if (playerInfo.IsSoftDeleted)
			{
				MessageBox messageBox = UIManager.MessageBox;
				double seconds = ((!playerInfo.DeletesAt.HasValue) ? 0.0 : (playerInfo.DeletesAt.Value - Connections.Frontend.GetPredictedServerTime()));
				messageBox.Show(T._("캐릭터 삭제를 취소하시겠습니까?"), T._("<alert>삭제까지 {0} 남음<alert>", TimedeltaFormatter.Format(seconds, 2, "min")), delegate(bool ok)
				{
					if (ok)
					{
						system.ChangePlayer(playerEntityId);
					}
				});
			}
			else
			{
				system.ChangePlayer(playerEntityId);
			}
			break;
		}
		}
	}

	private void PlayerSelectionGroup_OnVisible(bool visible)
	{
		if (visible && base.IsOpened)
		{
			GameSystem<PlayerSelectionSystem>.Instance().UpdateAccounts();
		}
	}
}
