using System;
using Durango.Player;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Shared.Player;
using UnityEngine;

namespace Durango.UI;

public class PlayerInfoWidget : MonoBehaviour
{
	[Flags]
	public enum Visible
	{
		[T.EnumName("부족")]
		Clan = 1,
		[T.EnumName("마지막 접속 시간")]
		Connected = 2,
		RegionName = 4,
		RegionInfo = 8,
		[T.EnumName("개인섬")]
		PioneerGrade = 0x10,
		All = -1
	}

	public Action<string> Clicked;

	[SerializeField]
	protected UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMaskTexture;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _clanLabel;

	[SerializeField]
	private UILabel _connectLabel;

	[SerializeField]
	private UILabel _regionNameLabel;

	[SerializeField]
	private UILabel _regionInfoLabel;

	[SerializeField]
	private SelectableWidget _relationButton;

	[SerializeField]
	private UILabel _relationLabel;

	[SerializeField]
	private UILabel _pioneerGradeLabel;

	[SerializeField]
	private bool _showProfileToolTip = true;

	private bool _valid;

	private bool _connectedValid;

	private Visible _visibleFlag = Visible.All;

	public string EntityId { get; private set; }

	private void Start()
	{
		if (_relationButton != null)
		{
			SelectableWidget relationButton = _relationButton;
			relationButton.Clicked = (Action)Delegate.Combine(relationButton.Clicked, new Action(RelationButton_Clicked));
		}
	}

	private void RelationButton_Clicked()
	{
		AccessRightsSettingPopup accessRightsSettingPopup = UIManager.Popup.Tooltip<AccessRightsSettingPopup>();
		accessRightsSettingPopup.Set(EntityId);
		accessRightsSettingPopup.Show();
	}

	public void Set(string entityId, Visible visibleFlag = Visible.All)
	{
		_valid = false;
		_connectedValid = false;
		_visibleFlag = visibleFlag;
		EntityId = entityId;
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(PlayerInfo info)
		{
			if (!(EntityId != entityId))
			{
				OnPlayer(info);
			}
		});
		if ((bool)_connectLabel && (_visibleFlag & Visible.Connected) != 0)
		{
			Singleton<PlayerInfoManager>.Instance().GetPlayerConnected(entityId, delegate(PlayerConnected connected)
			{
				if (!(EntityId != entityId))
				{
					OnConnectedInfo(connected);
				}
			});
		}
		if (!_valid)
		{
			if ((bool)_portraitTexture)
			{
				_portraitTexture.gameObject.SetActive(value: false);
			}
			if ((bool)_levelLabel)
			{
				_levelLabel.gameObject.SetActive(value: false);
			}
			if ((bool)_nameLabel)
			{
				_nameLabel.gameObject.SetActive(value: false);
			}
			if ((bool)_clanLabel)
			{
				_clanLabel.gameObject.SetActive(value: false);
			}
			if ((bool)_regionNameLabel)
			{
				_regionNameLabel.gameObject.SetActive(value: false);
			}
			if ((bool)_regionInfoLabel)
			{
				_regionInfoLabel.gameObject.SetActive(value: false);
			}
			if ((bool)_relationButton)
			{
				_relationButton.gameObject.SetActive(value: false);
			}
			if ((bool)_pioneerGradeLabel)
			{
				_pioneerGradeLabel.gameObject.SetActive(value: false);
			}
		}
		if ((bool)_connectLabel && !_connectedValid)
		{
			_connectLabel.gameObject.SetActive(value: false);
		}
	}

	private void OnPlayer(PlayerInfo player)
	{
		if (!player.Valid)
		{
			return;
		}
		_valid = true;
		if ((bool)_portraitTexture)
		{
			_portraitTexture.gameObject.SetActive(value: true);
			PortraitBuilder.Argument portraitArgument = player.GetPortraitArgument();
			portraitArgument.Mask = _portraitMaskTexture;
			PortraitBuilder.Set(portraitArgument, _portraitTexture);
		}
		if ((bool)_levelLabel)
		{
			_levelLabel.gameObject.SetActive(value: true);
			_levelLabel.text = T._("{0:lv:}", player.Level);
		}
		if ((bool)_nameLabel)
		{
			_nameLabel.gameObject.SetActive(value: true);
			_nameLabel.text = player.GetNameFreq(21, "FFFFFF7F");
		}
		if ((bool)_clanLabel)
		{
			_clanLabel.gameObject.SetActive((_visibleFlag & Visible.Clan) != 0);
			_clanLabel.text = player.ClanName;
		}
		if ((bool)_regionNameLabel)
		{
			_regionNameLabel.gameObject.SetActive((_visibleFlag & Visible.RegionName) != 0);
			_regionNameLabel.text = ((player.Region == null) ? string.Empty : player.Region.Name);
		}
		if ((bool)_regionInfoLabel)
		{
			_regionInfoLabel.gameObject.SetActive((_visibleFlag & Visible.RegionInfo) != 0);
			_regionInfoLabel.text = ((player.Region == null) ? string.Empty : T._("{0:lv:} {1}", player.Region.Level, LocalizeUtil.Get(player.Region.MajorBiome())));
		}
		if ((bool)_pioneerGradeLabel)
		{
			_pioneerGradeLabel.gameObject.SetActive((_visibleFlag & Visible.PioneerGrade) != 0);
			_pioneerGradeLabel.text = ((player.PioneerGrade == 0) ? T._("없음") : $"[icon=icon_pg] {player.PioneerGrade}");
		}
		if ((bool)_relationButton)
		{
			_relationButton.gameObject.SetActive(value: true);
			switch (GameSystem<SocialSystem>.Instance().GetFriendly(player.EntityId))
			{
			case FriendType.BestFriend:
				_relationLabel.text = T._("친한 친구");
				break;
			case FriendType.JustFriend:
				_relationLabel.text = T._("친구");
				break;
			}
		}
	}

	private void OnConnectedInfo(PlayerConnected info)
	{
		_connectedValid = true;
		_connectLabel.gameObject.SetActive(value: true);
		_connectLabel.text = info.GetConnectedString();
	}

	private void OnClick()
	{
		if (_showProfileToolTip)
		{
			ShowProfileTooltip();
		}
		if (Clicked != null)
		{
			Clicked(EntityId);
		}
	}

	protected void ShowProfileTooltip()
	{
		PlayerInfoPopup.RequestShow(EntityId, delegate(PlayerInfoPopup tooltip)
		{
			tooltip.AutoPosition = false;
			tooltip.Show();
			tooltip.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
		});
	}
}
