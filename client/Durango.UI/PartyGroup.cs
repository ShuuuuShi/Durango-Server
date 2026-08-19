using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Party;
using Durango.Player;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

[Uri("Party")]
public class PartyGroup : UIBase
{
	private const int MaxPartyCount = 5;

	[SerializeField]
	private GameObject _contents;

	[SerializeField]
	private GameObject _noParty;

	[SerializeField]
	private UILabel _noPartyLabel;

	[SerializeField]
	private UILabel _partyCount;

	[SerializeField]
	private UILabel _partyName;

	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private SelectableButton _makeParty;

	[SerializeField]
	private SelectableButton _acceptParty;

	[SerializeField]
	private SelectableButton _rejectParty;

	[SerializeField]
	private SelectableButton _leaveParty;

	[SerializeField]
	private SelectableButton _electLeader;

	[SerializeField]
	private SelectableButton _showPartyHud;

	[SerializeField]
	private SpriteData _showPartyHudSprite;

	[SerializeField]
	private SpriteData _hidePartyHudSprite;

	[SerializeField]
	private SelectableButton _inviteParty;

	[SerializeField]
	private RectLayoutComponent _bottomBar;

	protected override bool TryOpen()
	{
		if (!base.TryOpen())
		{
			if (GameSystem<PartySystem>.Instance().NotInParty)
			{
				return false;
			}
			SetChildrenActive(activated: true);
		}
		GameSystem<PartySystem>.Instance().GetParty();
		Refresh();
		return true;
	}

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Party;
		GameSystem<PartySystem>.Instance().MembersUpdated += PartySystem_MembersUpdated;
		GameSystem<PartySystem>.Instance().Invited += PartySystem_Invited;
		_scrollView.Nodes.Init(delegate(GameObject obj)
		{
			PartyPlayerInfoWidget component2 = obj.GetComponent<PartyPlayerInfoWidget>();
			component2.Kicked += PartyPlayerInfoWidget_Kicked;
			component2.ButtonClicked += PartyPlayerInfoWidget_ButtonClicked;
			component2.Clicked += PartyPlayerInfoWidget_Clicked;
		});
		SelectableButton makeParty = _makeParty;
		makeParty.Clicked = (Action)Delegate.Combine(makeParty.Clicked, (Action)delegate
		{
			SoundManager.PlayEvent("ui_party_create");
			GameSystem<PartySystem>.Instance().MakeParty();
		});
		SelectableButton leaveParty = _leaveParty;
		leaveParty.Clicked = (Action)Delegate.Combine(leaveParty.Clicked, (Action)delegate
		{
			UIManager.MessageBox.Show(T._("파티를 떠나시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<PartySystem>.Instance().LeaveParty();
				}
			});
		});
		SelectableButton acceptParty = _acceptParty;
		acceptParty.Clicked = (Action)Delegate.Combine(acceptParty.Clicked, (Action)delegate
		{
			GameSystem<PartySystem>.Instance().JoinIntoParty();
		});
		SelectableButton rejectParty = _rejectParty;
		rejectParty.Clicked = (Action)Delegate.Combine(rejectParty.Clicked, (Action)delegate
		{
			GameSystem<PartySystem>.Instance().RejectPartyInvitation();
		});
		SelectableButton electLeader = _electLeader;
		electLeader.Clicked = (Action)Delegate.Combine(electLeader.Clicked, (Action)delegate
		{
			bool flag2 = !_electLeader.Selected;
			_electLeader.Selected = flag2;
			for (int i = 0; i < _scrollView.Nodes.Count; i++)
			{
				GameObject gameObject = _scrollView.Nodes[i];
				PartyPlayerInfoWidget component = gameObject.GetComponent<PartyPlayerInfoWidget>();
				component.ToggleElectLeader(flag2);
			}
		});
		UpdateShowPartyHud(GameSystem<PartySystem>.Instance().ShowPartyHud);
		SelectableButton showPartyHud = _showPartyHud;
		showPartyHud.Clicked = (Action)Delegate.Combine(showPartyHud.Clicked, (Action)delegate
		{
			bool flag = !GameSystem<PartySystem>.Instance().ShowPartyHud;
			GameSystem<PartySystem>.Instance().ShowPartyHud = flag;
			UpdateShowPartyHud(flag);
			string text = ((!flag) ? T._("HUD에 파티 슬롯을 표시하지 않습니다.") : T._("HUD에 파티 슬롯을 표시합니다."));
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, text);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show(_showPartyHud.gameObject, Vector2.zero, 4f);
		});
		SelectableButton inviteParty = _inviteParty;
		inviteParty.Clicked = (Action)Delegate.Combine(inviteParty.Clicked, new Action(InviteParty));
		SetChildrenActive(activated: false);
	}

	private void UpdateShowPartyHud(bool show)
	{
		_showPartyHud.Icon = ((!show) ? _hidePartyHudSprite.sprite : _showPartyHudSprite.sprite);
		_showPartyHud.Color = ((!show) ? _hidePartyHudSprite.color : _showPartyHudSprite.color);
	}

	private static void InviteParty()
	{
		PartySystem partySystem = GameSystem<PartySystem>.Instance();
		int memberCount = partySystem.MemberCount;
		int num = 5 - memberCount;
		if (num <= 0)
		{
			return;
		}
		List<string> list2 = null;
		if (memberCount > 0)
		{
			list2 = new List<string>();
			for (int i = 0; i < memberCount; i++)
			{
				Member member = partySystem.GetMember(i);
				list2.Add(member.EntityId);
			}
		}
		SoundManager.PlayEvent("ui_party_invite_01");
		PlayerSearchGroup playerSearchGroup = UIManager.FindScript<PlayerSearchGroup>();
		playerSearchGroup.OpenForMultiple(num, T._("파티 초대"), list2, delegate(IList<string> list)
		{
			if (list == null)
			{
				return;
			}
			SoundManager.PlayEvent("ui_party_invite_02");
			foreach (string item in list)
			{
				GameSystem<PartySystem>.Instance().InviteIntoParty(item);
			}
		}, T._("초대"));
	}

	private void PartyPlayerInfoWidget_Kicked(string entityId)
	{
		PlayerInfo cachedPlayerInfoOrEmpty = Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(entityId);
		UIManager.MessageBox.Show(T._("<em>{0}</em>{0:-을} 파티에서 추방하시겠습니까?", cachedPlayerInfoOrEmpty.Name), delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<PartySystem>.Instance().KickMember(entityId);
			}
		});
	}

	private void PartyPlayerInfoWidget_ButtonClicked(string entityId, PartyPlayerInfoWidget.ActionMode mode)
	{
		switch (mode)
		{
		case PartyPlayerInfoWidget.ActionMode.CancelInvitaion:
			GameSystem<PartySystem>.Instance().CancelPartyInvitation(entityId);
			break;
		case PartyPlayerInfoWidget.ActionMode.ElectLeader:
		{
			PlayerInfo cachedPlayerInfoOrEmpty = Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(entityId);
			UIManager.MessageBox.Show(T._("<em>{0}</em>에게 파티장을 위임하시겠습니까?", cachedPlayerInfoOrEmpty.Name), delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<PartySystem>.Instance().ElectPartyLeader(entityId);
				}
			});
			break;
		}
		}
	}

	private void PartyPlayerInfoWidget_Clicked(PartyPlayerInfoWidget widget)
	{
		if (GameSystem<PartySystem>.Instance().IsLeader && string.IsNullOrEmpty(widget.EntityId))
		{
			InviteParty();
		}
	}

	private void PartySystem_MembersUpdated()
	{
		if (base.IsOpened)
		{
			Refresh();
		}
	}

	private void PartySystem_Invited()
	{
		if (UIBase.HasOpenedUI)
		{
			UIManager.Alarm.ShowNotify(T._("<em>{0}</em>님이 파티에 초대 하셨습니다", GameSystem<PartySystem>.Instance().LeaderName), "icon_mainhud_party", major: true, 1.8f, delegate
			{
				Open();
			}, "InviteParty");
		}
		SoundManager.PlayEvent("ui_notice_pop_up_party");
	}

	private void Refresh()
	{
		_electLeader.Selected = false;
		PartySystem partySystem = GameSystem<PartySystem>.Instance();
		if (!GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Party) && partySystem.NotInParty)
		{
			UIManager.SystemMsg(T._("지금은 파티를 이용 할 수 없습니다."));
			Close();
			return;
		}
		if (partySystem.NotInParty || partySystem.IsInvited)
		{
			_contents.SetActive(value: false);
			_noParty.SetActive(value: true);
			_makeParty.gameObject.SetActive(partySystem.NotInParty);
			_acceptParty.gameObject.SetActive(partySystem.IsInvited);
			_rejectParty.gameObject.SetActive(partySystem.IsInvited);
			if (partySystem.IsInvited)
			{
				_noPartyLabel.text = T._("<em>{0}</em> 님이 파티에 초대하셨습니다.\n[size=22][FFFFFF90]파티 초대에 응답하기 전까지는 새로운 파티를 만들 수 없습니다.[/size]", partySystem.LeaderName);
			}
			else
			{
				_noPartyLabel.text = T._("파티를 생성하고 파티원을 초대할 수 있습니다.");
			}
			_noParty.GetComponent<RectLayoutComponent>().UpdateLayout();
			return;
		}
		_partyName.text = T._("<em>{0}</em>의 파티", GameSystem<PartySystem>.Instance().LeaderName);
		_contents.SetActive(value: true);
		_noParty.SetActive(value: false);
		int memberCount = partySystem.MemberCount;
		_inviteParty.Disabled = 5 <= memberCount;
		_electLeader.gameObject.SetActive(partySystem.IsLeader);
		_inviteParty.gameObject.SetActive(partySystem.IsLeader);
		_scrollView.Nodes.BeginLoad();
		bool flag = false;
		for (int i = 0; i < 5; i++)
		{
			GameObject next = _scrollView.Nodes.GetNext();
			PartyPlayerInfoWidget component = next.GetComponent<PartyPlayerInfoWidget>();
			if (i < memberCount)
			{
				Member member = partySystem.GetMember(i);
				component.Set(member);
				component.ToggleElectLeader(_electLeader.Selected);
				flag |= !member.IsLeader && member.IsAccepted;
			}
			else
			{
				component.Set(null);
			}
		}
		_scrollView.Nodes.EndLoad();
		_scrollView.ResetPosition();
		_electLeader.Disabled = !flag;
		_partyCount.text = $"[E8E5DFDF]{memberCount} / [74716A]{5}";
		_bottomBar.UpdateLayout();
	}
}
