using System;
using Durango.Logic.Clusters;
using Durango.Network;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PlayerPreviewPage : UIWidget
{
	[SerializeField]
	private UILabel _playerNameLabel;

	[SerializeField]
	private UITexture _playerPreview;

	[SerializeField]
	private UITexture _playerShadow;

	[SerializeField]
	private GameObject _emptyPlayerLabel;

	[SerializeField]
	private UILabel _deleteHelperLabel;

	[SerializeField]
	private GameObject _grassObject;

	[SerializeField]
	private GameObject _needMoreSlotLabel;

	[SerializeField]
	private UILabel _islandLabel;

	[SerializeField]
	private SelectableWidget _button;

	[SerializeField]
	private UILabel _buttonLabel;

	private UIModelRender _uiModelRender;

	private PlayerBehavior _previewModel;

	private Durango.Logic.Clusters.PlayerInfo _selectedPlayerInfo;

	private bool _isWaitingRequests;

	protected override void OnDisable()
	{
		base.OnDisable();
		ReleasePreviewRenderers();
	}

	private void ReleasePreviewRenderers()
	{
		UIModelRenderBuilder.Release(_uiModelRender);
		_uiModelRender = null;
		_previewModel = null;
		_playerPreview.mainTexture = null;
		_playerShadow.mainTexture = null;
	}

	protected override void OnStart()
	{
		base.OnStart();
		SelectableWidget button = _button;
		button.Clicked = (Action)Delegate.Combine(button.Clicked, new Action(OnButtonClicked));
		UIEventListener uIEventListener = UIEventListener.Get(_playerPreview.gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(Preview_Drag));
	}

	public void Set(PlayerSlotNode.SlotType slotType, Durango.Logic.Clusters.PlayerInfo info)
	{
		_selectedPlayerInfo = info;
		_emptyPlayerLabel.SetActive(slotType == PlayerSlotNode.SlotType.Empty);
		_needMoreSlotLabel.SetActive(slotType == PlayerSlotNode.SlotType.Locked);
		_grassObject.SetActive(slotType != PlayerSlotNode.SlotType.Locked);
		bool flag = slotType == PlayerSlotNode.SlotType.HasPlayer;
		_playerNameLabel.gameObject.SetActive(flag);
		_playerPreview.gameObject.SetActive(flag);
		_button.gameObject.SetActive(flag);
		_islandLabel.gameObject.SetActive(value: false);
		if (flag)
		{
			_playerNameLabel.text = info.PlayerName;
			_deleteHelperLabel.gameObject.SetActive(info.IsSoftDeleted);
			_deleteHelperLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				double num = ((!info.DeletesAt.HasValue) ? 0.0 : (info.DeletesAt.Value - Connections.Frontend.GetPredictedServerTime()));
				if (num > 0.0)
				{
					text = T._("삭제까지 {0} 남음", TimedeltaFormatter.Format(num, 2, "min"));
					period = (float)(num % 1.0);
				}
				else
				{
					text = string.Empty;
					period = 0f;
				}
			}));
			_buttonLabel.text = ((!info.IsSoftDeleted) ? T._("[icon=icon_x]  캐릭터 삭제") : T._("삭제 취소"));
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(info.PlayerEntityId, delegate(Durango.Player.PlayerInfo playerInfo)
			{
				_islandLabel.gameObject.SetActive(value: true);
				_islandLabel.text = string.Format("{0} {1}\n{2} {3}", (!string.IsNullOrEmpty(playerInfo.RegionName)) ? playerInfo.RegionName : T._("알 수 없음"), "icon_popup_player_island".ToEncodedIcon(), (!string.IsNullOrEmpty(playerInfo.ReturningRegionName)) ? playerInfo.ReturningRegionName : T._("알 수 없음"), "icon_popup_player_house".ToEncodedIcon());
				if (_uiModelRender == null)
				{
					_uiModelRender = UIModelRenderBuilder.Make();
				}
				if (_previewModel == null || _previewModel.IsMale != playerInfo.IsMale)
				{
					_previewModel = Singleton<PlayerManager>.Instance().MakePreview(playerInfo.IsMale);
					_uiModelRender.SetModel(_previewModel.gameObject, 35f);
				}
				PlayerManager.SetDisplay(_previewModel, playerInfo.Display);
				_uiModelRender.FillTexture(_playerPreview);
				_uiModelRender.FillTexture(_playerShadow);
			});
		}
		else
		{
			_deleteHelperLabel.gameObject.SetActive(value: false);
			_button.gameObject.SetActive(value: false);
		}
	}

	private void Preview_Drag(GameObject go, Vector2 delta)
	{
		if (_previewModel != null)
		{
			Transform transform = _previewModel.transform;
			transform.Rotate(transform.up, 0f - delta.x, Space.World);
		}
	}

	private void OnButtonClicked()
	{
		if (_selectedPlayerInfo == null || _isWaitingRequests)
		{
			return;
		}
		if (_selectedPlayerInfo.IsSoftDeleted)
		{
			MessageBox messageBox = UIManager.MessageBox;
			double seconds = ((!_selectedPlayerInfo.DeletesAt.HasValue) ? 0.0 : (_selectedPlayerInfo.DeletesAt.Value - Connections.Frontend.GetPredictedServerTime()));
			messageBox.Show(T._("캐릭터 삭제를 취소하시겠습니까?"), T._("<alert>삭제까지 {0} 남음<alert>", TimedeltaFormatter.Format(seconds, 2, "min")), delegate(bool ok)
			{
				if (ok)
				{
					UIManager.Popup.LoadingRing.AttachToWidget(_button.gameObject);
					_isWaitingRequests = true;
					GameSystem<PlayerSelectionSystem>.Instance().RequestCancelDeletion(_selectedPlayerInfo, delegate(bool success)
					{
						if (!success)
						{
							UIManager.SystemMsg(T._("캐릭터 삭제 취소 요청이 실패했습니다. 잠시 후 다시 시도해주세요."));
						}
						UIManager.Popup.LoadingRing.DetachFromWidget(_button.gameObject);
						_isWaitingRequests = false;
					});
				}
			});
			return;
		}
		PlayerSelectionSystem playerSelectionSystem = GameSystem<PlayerSelectionSystem>.Instance();
		if (playerSelectionSystem.PlayerSlotExceeded)
		{
			MessageBox messageBox2 = UIManager.MessageBox;
			messageBox2.AddKeyValueInfo(T._("삭제시 보유한 캐릭터 수"), (GameSystem<PlayerSelectionSystem>.Instance().PlayersCount - 1).ToString().ToEncodedColor(PresetColor.UIRed));
			messageBox2.AddKeyValueInfo(T._("캐릭터 슬롯의 수"), GameSystem<PlayerSelectionSystem>.Instance().PlayerSlotCount.ToString());
			messageBox2.Show(T._("캐릭터를 삭제하시겠습니까?"), T._("[icon=icon_make_alert] 슬롯의 수가 캐릭터 수보다 많아야만 새 캐릭터를 생성할 수 있습니다.\n[icon=icon_make_alert] 지금 이 캐릭터를 삭제해도 슬롯의 수가 캐릭터의 수보다 많아지지 않으므로, [B82E2EFF]즉시 새 캐릭터를 생성할 수는 없습니다.[-]").ToEncodedColor("D4CEBE"), delegate(bool ok)
			{
				if (ok)
				{
					OnRequestDeletePlayer();
				}
			});
			return;
		}
		MessageBox messageBox3 = UIManager.MessageBox;
		messageBox3.Show(T._("캐릭터를 삭제하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				OnRequestDeletePlayer();
			}
		});
	}

	private void OnRequestDeletePlayer()
	{
		MessageBox messageBox = UIManager.MessageBox;
		messageBox.Show(T._("보유한 [B82E2E]듀랑고 코인[-] 및 상점 보관함의 [B82E2E]모든 아이템들[-]을 잃게 됩니다.\n그래도 삭제하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				UIManager.Popup.LoadingRing.AttachToWidget(_button.gameObject);
				_isWaitingRequests = true;
				GameSystem<PlayerSelectionSystem>.Instance().RequestDeletePlayer(_selectedPlayerInfo, delegate(bool isSuccess)
				{
					UIManager.Popup.LoadingRing.DetachFromWidget(_button.gameObject);
					_isWaitingRequests = false;
					if (isSuccess)
					{
						if (_selectedPlayerInfo.PlayerEntityId == GameManager.PlayerId)
						{
							KUtility.DelayedCall(this, Singleton<GameManager>.Instance().MoveToTitle, 0.1f);
						}
					}
					else
					{
						UIManager.SystemMsg(T._("캐릭터 삭제 요청이 실패했습니다. 잠시 후 다시 시도해주세요."));
					}
				});
			}
		});
	}
}
