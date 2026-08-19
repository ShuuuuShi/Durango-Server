using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Clan;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Shared.Estate;
using UnityEngine;

namespace Durango.UI.Popup;

public class PlayerInfoPopup : TooltipBase
{
	private enum ButtonType
	{
		Chat,
		Timeline,
		Friend,
		Follow,
		Block,
		Report,
		ViewClan,
		VisitUrbanEstate,
		VisitPersonalEstate,
		Invite
	}

	[SerializeField]
	private UIWidget _previewPane;

	[SerializeField]
	private UITexture _previewTexture;

	[SerializeField]
	private UIWidget _upperPane;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UILabel _textLevel;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private UILabel _textRadio;

	[SerializeField]
	private UILabel _textClanName;

	[SerializeField]
	private UILabel _textCurrentRegionName;

	[SerializeField]
	private GameObject _iconReturningRegionName;

	[SerializeField]
	private UILabel _textReturningRegionName;

	[SerializeField]
	private UIWidget _lowerPane;

	[SerializeField]
	private SelectableButton _buttonBase;

	[SerializeField]
	private int _buttonsCountPerLine;

	[SerializeField]
	private Texture2D _portraitMaskTexture;

	private PlayerInfo _playerInfo;

	private UIModelRender _uiModelRender;

	private PlayerBehavior _previewModel;

	private readonly ListObjectPool<SelectableButton> _buttons = new ListObjectPool<SelectableButton>();

	private readonly List<ButtonType> _buttonTypes = new List<ButtonType>();

	private int _defaultLowerPaneHeight;

	private float _lastButtonClickedAt;

	public static void RequestShow(string entityId, Action<PlayerInfoPopup> onShow = null)
	{
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(PlayerInfo playerInfo)
		{
			if (playerInfo.Valid)
			{
				PlayerInfoPopup playerInfoPopup = UIManager.Popup.Tooltip<PlayerInfoPopup>();
				playerInfoPopup.Set(playerInfo);
				if (onShow != null)
				{
					onShow(playerInfoPopup);
				}
				else
				{
					playerInfoPopup.Show();
				}
			}
		});
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GameSystem<SocialSystem>.Instance().SocialUpdated += base.Refresh;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GameSystem<SocialSystem>.Instance().SocialUpdated -= base.Refresh;
	}

	public void Set([NotNull] PlayerInfo playerInfo)
	{
		_playerInfo = playerInfo;
	}

	protected override void OnAwake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_previewPane.gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragPreviewModel));
		_buttons.BaseObject = _buttonBase;
		_buttons.Init(delegate(SelectableButton button)
		{
			SelectableButton selectableButton = button;
			selectableButton.Clicked = (Action)Delegate.Combine(selectableButton.Clicked, (Action)delegate
			{
				int num = _buttons.IndexOf(button);
				if (0 <= num && num < _buttonTypes.Count)
				{
					DoButtonClick(_buttonTypes[num]);
				}
			});
		});
		_defaultLowerPaneHeight = _lowerPane.height;
	}

	protected override void FillData()
	{
		FillUpperPane();
		FillLowerPane();
	}

	protected override void UpdateLayout()
	{
		Vector3 localPosition = _buttonBase.transform.localPosition;
		int num = 0;
		for (int i = 0; i < _buttons.Count; i++)
		{
			SelectableButton selectableButton = _buttons[i];
			int num2 = _buttonBase.Widget.width * (i % _buttonsCountPerLine);
			int num3 = _buttonBase.Widget.height * (i / _buttonsCountPerLine);
			num = Mathf.Max(num, num3);
			selectableButton.transform.localPosition = localPosition - Vector3.up * num3 + Vector3.right * num2;
		}
		_lowerPane.height = _defaultLowerPaneHeight + num;
		base.Widget.height = _upperPane.height + _lowerPane.height;
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void OnChangeState()
	{
		switch (base.State)
		{
		case VisibleState.Wait:
		case VisibleState.FadeIn:
		case VisibleState.Hide:
			DestoryPreviewModel();
			break;
		case VisibleState.Show:
			MakePreviewModel();
			break;
		}
	}

	private void FillUpperPane()
	{
		PortraitBuilder.Argument portraitArgument = _playerInfo.GetPortraitArgument();
		portraitArgument.Mask = _portraitMaskTexture;
		PortraitBuilder.Set(portraitArgument, _portraitTexture);
		_textLevel.text = T._("{0:lv:}", _playerInfo.Level);
		_textName.text = _playerInfo.Name;
		_textRadio.text = $"#{_playerInfo.Freq:0000} [size=16]kHz[/size]";
		_textClanName.text = ((!string.IsNullOrEmpty(_playerInfo.ClanId)) ? _playerInfo.ClanName : T._("부족이 없습니다"));
		_textCurrentRegionName.text = ((_playerInfo.Region == null) ? string.Empty : _playerInfo.Region.Name);
		if (_playerInfo.ReturningRegion != null)
		{
			_iconReturningRegionName.SetActive(value: true);
			_textReturningRegionName.text = _playerInfo.ReturningRegion.Name;
		}
		else
		{
			_iconReturningRegionName.SetActive(value: false);
			_textReturningRegionName.text = string.Empty;
		}
	}

	private void FillLowerPane()
	{
		ClearButtons();
		if (_playerInfo.HasClan)
		{
			AddButton(ButtonType.ViewClan);
		}
		if (_playerInfo.EntityId != GameManager.PlayerId)
		{
			if (!GetBlockState(_playerInfo))
			{
				AddButton(ButtonType.Friend);
				AddButton(ButtonType.Follow);
				AddButton(ButtonType.Chat);
			}
			if (EstateSystem.CanVisitEstate())
			{
				AddButton(ButtonType.VisitUrbanEstate);
				AddButton(ButtonType.VisitPersonalEstate);
			}
			AddButton(ButtonType.Report);
			AddButton(ButtonType.Block);
			if (GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Party) && GameSystem<PartySystem>.Instance().CanInvite(_playerInfo.EntityId))
			{
				AddButton(ButtonType.Invite);
			}
		}
	}

	private void ClearButtons()
	{
		_buttons.Clear();
		_buttonTypes.Clear();
	}

	private void AddButton(ButtonType type)
	{
		_buttons.Add().Text = GetButtonText(type);
		_buttonTypes.Add(type);
	}

	private void RefreshButtonText(ButtonType type)
	{
		int num = _buttonTypes.IndexOf(type);
		if (0 <= num && num < _buttons.Count)
		{
			_buttons[num].Text = GetButtonText(type);
		}
	}

	private string GetButtonText(ButtonType type)
	{
		switch (type)
		{
		case ButtonType.Chat:
			return T._("1:1 무전");
		case ButtonType.Block:
			return string.Format("<alert>{0}</alert>", (!GetBlockState(_playerInfo)) ? T._("차단") : T._("차단 해제"));
		case ButtonType.Friend:
			if (GetFriendState(_playerInfo))
			{
				return T._("친구 끊기");
			}
			if (GetSentFriendRequestedState(_playerInfo))
			{
				return T._("친구 요청중");
			}
			return T._("친구 요청");
		case ButtonType.Follow:
			if (GetFollowingState(_playerInfo))
			{
				return T._("즐겨찾기 해제");
			}
			return T._("즐겨찾기 추가");
		case ButtonType.Report:
			return string.Format("<alert>{0}</alert>", T._("신고"));
		case ButtonType.ViewClan:
			return T._("부족 보기");
		case ButtonType.Timeline:
			return T._("이력");
		case ButtonType.VisitUrbanEstate:
			return T._("사유지 방문");
		case ButtonType.VisitPersonalEstate:
			return T._("섬 방문");
		case ButtonType.Invite:
			return T._("파티 초대");
		default:
			return string.Empty;
		}
	}

	private void DoButtonClick(ButtonType type)
	{
		float time = Time.time;
		if (time < _lastButtonClickedAt + 0.5f)
		{
			return;
		}
		_lastButtonClickedAt = time;
		switch (type)
		{
		case ButtonType.Chat:
			UIManager.FindScript<ChattingGroupBase>().Open(_playerInfo.EntityId);
			Hide();
			break;
		case ButtonType.Block:
		{
			BlockPopup blockPopup = UIManager.Popup.Tooltip<BlockPopup>();
			blockPopup.Set(_playerInfo, delegate
			{
				RefreshButtonText(ButtonType.Block);
			});
			blockPopup.Show();
			break;
		}
		case ButtonType.Friend:
			if (GetFriendState(_playerInfo))
			{
				UIManager.MessageBox.Show(T._("<em>{0}</em> 님과 친구관계를 끊으시겠습니까?", _playerInfo.GetNameFreq(24, string.Empty)), delegate(bool ok)
				{
					if (ok)
					{
						GameSystem<SocialSystem>.Instance().RequestFriend(_playerInfo.EntityId, enable: false, delegate
						{
							UIManager.SystemMsg(T._("<em>{0}</em> 님과 친구관계를 해제했습니다.", _playerInfo.GetNameFreq(21, string.Empty)));
						});
					}
				});
			}
			else if (GetSentFriendRequestedState(_playerInfo))
			{
				UIManager.FindScript<SocialGroup>().CancelRequest(_playerInfo.EntityId);
			}
			else
			{
				GameSystem<SocialSystem>.Instance().RequestFriend(_playerInfo.EntityId, enable: true, delegate
				{
					UIManager.SystemMsg(T._("<em>{0}</em> 님에게 친구요청을 보냈습니다.", _playerInfo.GetNameFreq(21, string.Empty)));
					RefreshButtonText(type);
				});
			}
			break;
		case ButtonType.Follow:
		{
			bool isFollow = GameSystem<SocialSystem>.Instance().IsFollowing(_playerInfo.EntityId);
			GameSystem<SocialSystem>.Instance().Follow(_playerInfo.EntityId, !isFollow, delegate
			{
				if (isFollow)
				{
					UIManager.SystemMsg(T._("<em>{0}</em> 님을 즐겨찾기에서 제거했습니다.", _playerInfo.GetNameFreq(21, string.Empty)));
				}
				else
				{
					UIManager.SystemMsg(T._("<em>{0}</em> 님을 즐겨찾기에 추가했습니다.", _playerInfo.GetNameFreq(21, string.Empty)));
				}
				RefreshButtonText(ButtonType.Follow);
			});
			break;
		}
		case ButtonType.Report:
		{
			SendReportPopup sendReportPopup = UIManager.Popup.Tooltip<SendReportPopup>();
			sendReportPopup.SetForPlayer(_playerInfo);
			sendReportPopup.Show();
			break;
		}
		case ButtonType.ViewClan:
			if (_playerInfo.HasClan)
			{
				Vector3 pos = base.Widget.GetPosition(0f, 1f);
				ClanSystem.GetClanInfo(_playerInfo.ClanId, delegate(Clan clan)
				{
					ClanInfoPopup clanInfoPopup = UIManager.Popup.Tooltip<ClanInfoPopup>();
					clanInfoPopup.AutoPosition = false;
					clanInfoPopup.Set(clan);
					clanInfoPopup.Show();
					clanInfoPopup.Widget.SetPosition(pos, 0f, 1f);
				});
				Hide();
			}
			break;
		case ButtonType.Timeline:
			UIManager.FindScript<TimelineLogGroup>().OpenForPlayer(_playerInfo.EntityId);
			Hide();
			break;
		case ButtonType.VisitUrbanEstate:
			EstateSystem.VisitEstate(OwnerType.Player, _playerInfo.EntityId);
			Hide();
			UIBase.CloseAllUI();
			break;
		case ButtonType.VisitPersonalEstate:
			EstateSystem.VisitEstate(OwnerType.PersonalPlayer, _playerInfo.EntityId);
			Hide();
			UIBase.CloseAllUI();
			break;
		case ButtonType.Invite:
			GameSystem<PartySystem>.Instance().InviteIntoParty(_playerInfo.EntityId);
			break;
		}
	}

	private void MakePreviewModel()
	{
		if (!(_previewModel != null))
		{
			_previewModel = Singleton<PlayerManager>.Instance().MakePreview(_playerInfo.IsMale, _playerInfo.Display);
			_uiModelRender = UIModelRenderBuilder.Make();
			_uiModelRender.SetModel(_previewModel.gameObject, 35f);
			_uiModelRender.FillTexture(_previewTexture);
		}
	}

	private void DestoryPreviewModel()
	{
		_previewTexture.mainTexture = null;
		UIModelRenderBuilder.Release(_uiModelRender);
		_uiModelRender = null;
	}

	private static bool GetFriendState(PlayerInfo playerInfo)
	{
		return GameSystem<SocialSystem>.Instance().IsFriend(playerInfo.EntityId);
	}

	public static bool GetSentFriendRequestedState(PlayerInfo playerInfo)
	{
		return GameSystem<SocialSystem>.Instance().IsSentFriendRequested(playerInfo.EntityId);
	}

	private static bool GetFollowingState(PlayerInfo playerInfo)
	{
		return GameSystem<SocialSystem>.Instance().IsFollowing(playerInfo.EntityId);
	}

	private static bool GetBlockState(PlayerInfo playerInfo)
	{
		return GameSystem<SocialSystem>.Instance().IsBlocked(playerInfo.EntityId);
	}

	private void OnDragPreviewModel(GameObject obj, Vector2 delta)
	{
		if (!(_previewModel == null))
		{
			Transform mainTransform = _previewModel.MainTransform;
			mainTransform.Rotate(mainTransform.up, 0f - delta.x, Space.World);
		}
	}
}
