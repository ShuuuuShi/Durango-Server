using System;
using Durango.Logic.Clan;
using Durango.Player;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ClanMemberNode : SelectableWidget
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _connectionLabel;

	[SerializeField]
	private UILabel _rankLabel;

	[SerializeField]
	private UITexture _portrait;

	[SerializeField]
	private Texture2D _portraitMaskTexture;

	[SerializeField]
	private SelectableButton _acceptButton;

	[SerializeField]
	private SelectableButton _rejectButton;

	[SerializeField]
	private RectLayout _layout;

	[CanBeNull]
	private PlayerInfo _playerInfo;

	public Member Member { get; private set; }

	private void Start()
	{
		_acceptButton.Text = T._("수락");
		_rejectButton.Text = T._("거절");
		SelectableButton acceptButton = _acceptButton;
		acceptButton.Clicked = (Action)Delegate.Combine(acceptButton.Clicked, new Action(OnAcceptClicked));
		SelectableButton rejectButton = _rejectButton;
		rejectButton.Clicked = (Action)Delegate.Combine(rejectButton.Clicked, new Action(OnRejectClicked));
		UIEventListener uIEventListener = UIEventListener.Get(_portrait.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickPortrait));
		_layout.UpdateOnSizeChange(delegate
		{
			UIUtility.UpdateAnchors(base.transform);
		});
	}

	public void Set(Member member)
	{
		Init();
		Member = member;
		SetRoleInfos();
		_playerInfo = null;
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(member.EntityId, OnPlayerInfo);
		if (_playerInfo == null)
		{
			OnPlayerInfo(null);
		}
	}

	private void OnPlayerInfo(PlayerInfo player)
	{
		_playerInfo = player;
		if (_playerInfo != null && _playerInfo.Valid)
		{
			_nameLabel.text = _playerInfo.GetNameFreq(21, "FFFFFF7F");
			_nameLabel.color = ((!(_playerInfo.EntityId == GameManager.PlayerId)) ? Color.white : PresetColor.UIYellow);
			_levelLabel.text = LocalizeUtil.FormatLevel(_playerInfo.Level);
			PortraitBuilder.Argument portraitArgument = _playerInfo.GetPortraitArgument();
			portraitArgument.Mask = _portraitMaskTexture;
			PortraitBuilder.Set(portraitArgument, _portrait);
			_connectionLabel.text = string.Empty;
			Singleton<PlayerInfoManager>.Instance().GetPlayerConnected(_playerInfo.EntityId, OnConnectedInfo);
		}
		else
		{
			_nameLabel.text = string.Empty;
			_connectionLabel.text = string.Empty;
			_levelLabel.gameObject.SetActive(value: false);
			_portrait.gameObject.SetActive(value: false);
		}
	}

	private void OnConnectedInfo(PlayerConnected info)
	{
		_connectionLabel.text = info.GetConnectedString();
	}

	private void SetRoleInfos()
	{
		bool flag = false;
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (Member != null && playerClan != null && playerClan.TryGetRole(Member.RoleId, out var role))
		{
			flag = true;
			_rankLabel.text = role.GetName();
		}
		_rankLabel.gameObject.SetActive(flag);
		_acceptButton.gameObject.SetActive(!flag);
		_rejectButton.gameObject.SetActive(!flag);
	}

	private void OnAcceptClicked()
	{
		ClanSystem.ApproveApplier(Member.EntityId);
	}

	private void OnRejectClicked()
	{
		DropApplier();
	}

	private void DropApplier()
	{
		UIManager.MessageBox.Show(T._("<em>{0}</em> 님의 부족 가입 신청을 거절하시겠습니까?", (_playerInfo == null) ? string.Empty : _playerInfo.GetNameFreq(24, string.Empty)), delegate(bool ok)
		{
			if (ok)
			{
				ClanSystem.DropApplier(Member.EntityId);
			}
		});
	}

	private void OnClickPortrait(GameObject obj)
	{
		if (_playerInfo != null)
		{
			PlayerInfoPopup playerInfoPopup = UIManager.Popup.Tooltip<PlayerInfoPopup>();
			playerInfoPopup.Set(_playerInfo);
			playerInfoPopup.Show();
		}
	}
}
