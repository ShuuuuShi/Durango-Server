using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.Player;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Clan;
using UnityEngine;

namespace Durango.UI;

public class ClanMembersPage : ClanMenuPage, IUIInitializable
{
	private enum Tab
	{
		[T.EnumName("부족원")]
		Member,
		[T.EnumName("가입대기")]
		Applier
	}

	private enum ClanAction
	{
		[T.EnumName("추방")]
		Kick,
		[T.EnumName("탈퇴")]
		Leave,
		[T.EnumName("등급 변경")]
		PromoteMember,
		[T.EnumName("캐릭터 정보")]
		PlayerInfo
	}

	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	[SerializeField]
	private GameObject _body;

	[SerializeField]
	private KInfiniteScrollView _membersScroll;

	[SerializeField]
	private UIWidget _bottomBar;

	[SerializeField]
	private SelectableButton _buttonBase;

	[SerializeField]
	private SelectableButton _manageRoleButton;

	[SerializeField]
	private GameObject[] _onlyLandcapeColumn;

	[SerializeField]
	private GameObject _rankField;

	[SerializeField]
	private GameObject _noApplier;

	private readonly ListObjectPool<SelectableButton> _buttons = new ListObjectPool<SelectableButton>();

	private ClanGroup _parent;

	private Durango.Logic.Clan.Member _selected;

	private readonly List<ClanAction> _buttonActions = new List<ClanAction>();

	private HorizontalTabList _tabList;

	private KInfiniteScrollView.View<Durango.Logic.Clan.Member, ClanMemberNode> _memberView;

	private bool _validRole;

	private MemberRole _role;

	private bool _resetFlag;

	private Tab _currentTab;

	private readonly ClanMemberSorter _memberSorter = new ClanMemberSorter();

	void IUIInitializable.Init()
	{
		_tabList = _tabLinker.Object.GetComponent<HorizontalTabList>();
		_tabList.BeginLoad();
		Tab[] array = Enums<Tab>.All();
		foreach (Tab tab in array)
		{
			_tabList.AddText(tab.GetName(), string.Empty);
		}
		_tabList.EndLoadByFixedSize(260);
		_tabList.Clicked += delegate(int index)
		{
			_currentTab = Enums<Tab>.All()[index];
			RefreshTab();
		};
		_memberView = _membersScroll.Initialize(delegate(ClanMemberNode node, Durango.Logic.Clan.Member member)
		{
			node.Set(member);
			node.Selected = object.Equals(_selected, member);
		}, delegate(ClanMemberNode node)
		{
			node.Clicked = (Action)Delegate.Combine(node.Clicked, new Action(OnClickMemberNode));
		});
		_buttons.BaseObject = _buttonBase;
		_buttons.UseBase = true;
		_buttons.Init(delegate(SelectableButton btn)
		{
			if (btn != _buttons.BaseObject)
			{
				btn.Widget.SetAnchor((Transform)null);
			}
			btn.Clicked = OnClickActionButtons;
		});
		_manageRoleButton.Text = T._("등급 관리");
		_manageRoleButton.Clicked = OnClickManageRole;
		_parent = GetComponentInParent<ClanGroup>();
		_parent.OnCloseSucceed += delegate
		{
			_resetFlag = false;
		};
	}

	protected override void UpdateLayout()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_onlyLandcapeColumn); i < size; i++)
		{
			_onlyLandcapeColumn[i].SetActive(!_parent.IsPortrait);
		}
		base.UpdateLayout();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated -= Refresh;
		_selected = null;
	}

	private void Refresh()
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		_role = default(MemberRole);
		_validRole = false;
		Messages.Member clan = PlayerBehavior.LocalPlayer.Clan;
		_validRole = clan.ClanId == playerClan.Id && playerClan.TryGetRole(clan.RoleId, out _role);
		SetTabInfos(playerClan);
		SetMembers(playerClan);
		RefreshLayout();
		_resetFlag = true;
	}

	private void OnClickMemberNode()
	{
		ClanMemberNode clanMemberNode = Selectable.Current as ClanMemberNode;
		if (!(clanMemberNode == null) && !clanMemberNode.Member.IsApplier)
		{
			SelectMember((!clanMemberNode.Selected) ? clanMemberNode.Member : null);
		}
	}

	private void SetTabInfos(Clan clan)
	{
		string text = $"{clan.MemberCount} / {clan.Capacity}";
		_tabList.Get(Enums<Tab>.All().IndexOf(Tab.Member)).SetValue(text);
		HorizontalTabWidget horizontalTabWidget = _tabList.Get(Enums<Tab>.All().IndexOf(Tab.Applier));
		if (_validRole && (_role.GetPermissions() & Permissions.ApproveMember) != 0)
		{
			string text2 = ((clan.Appliers != null) ? clan.Appliers.Count : 0).ToString();
			horizontalTabWidget.SetValue(text2);
		}
		else
		{
			horizontalTabWidget.gameObject.SetActive(value: false);
		}
	}

	private void SetMembers(Clan clan)
	{
		UIManager.Popup.LoadingRing.AttachToWidget(_body);
		_memberSorter.Request(clan, delegate
		{
			UIManager.Popup.LoadingRing.DetachFromWidget(_body);
			RefreshTab();
		});
	}

	private void RefreshTab()
	{
		_tabList.Select(Enums<Tab>.All().IndexOf(_currentTab));
		List<Durango.Logic.Clan.Member> list = ((_currentTab != 0) ? _memberSorter.Appliers : _memberSorter.Members);
		_rankField.gameObject.SetActive(_currentTab == Tab.Member);
		string text = ((_selected == null) ? null : _selected.EntityId);
		_selected = null;
		_memberView.SetList(list);
		for (int i = 0; i < list.Count; i++)
		{
			Durango.Logic.Clan.Member member = list[i];
			if (member != null && text == member.EntityId)
			{
				_selected = member;
			}
		}
		if (_resetFlag)
		{
			_membersScroll.Reposition();
		}
		else
		{
			_membersScroll.ResetPosition();
		}
		SelectMember(_selected);
		UIUtility.UpdateAnchors(_membersScroll.transform);
		_noApplier.SetActive(_currentTab == Tab.Applier && list.Count == 0);
		_manageRoleButton.gameObject.SetActive(_validRole && (_role.GetPermissions() & Permissions.EditClanInfo) != 0);
	}

	private void SelectMember(Durango.Logic.Clan.Member member)
	{
		_selected = member;
		_memberView.Redraw();
		string text = ((_selected != null) ? _selected.EntityId : string.Empty);
		if (_selected != null)
		{
			_buttonActions.Clear();
			_buttonActions.Add(ClanAction.PlayerInfo);
			bool flag = text == GameManager.PlayerId;
			if (flag)
			{
				_buttonActions.Add(ClanAction.Leave);
			}
			if (_validRole)
			{
				bool flag2 = false;
				Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
				if (playerClan != null && playerClan.TryGetRole(_selected.RoleId, out var role))
				{
					flag2 = _role.Grade <= role.Grade;
				}
				if ((_role.GetPermissions() & Permissions.PromoteMember) != 0 && flag2)
				{
					_buttonActions.Add(ClanAction.PromoteMember);
				}
				if (_role.Grade == 0 && !flag)
				{
					_buttonActions.Add(ClanAction.Kick);
				}
			}
			_buttonActions.Sort();
			UpdateButtons();
		}
		RefreshLayout();
	}

	private void GetButtonStyle(ClanAction action, out PresetButton.Style style, out Color tint)
	{
		style = PresetButton.Style.Border;
		tint = Color.white;
		if (action == ClanAction.Leave || action == ClanAction.Kick)
		{
			tint = new Color32(221, 92, 86, byte.MaxValue);
		}
	}

	private void RefreshLayout()
	{
		bool flag = _bottomBar.enabled;
		if (flag != (_selected != null))
		{
			if (!flag)
			{
				_bottomBar.gameObject.SetActive(value: true);
				_bottomBar.enabled = true;
				AnimationWidget.Get(_bottomBar.gameObject, 0.2f, 0f, deactiveWhenFadeout: true).Alpha = 1f;
			}
			else
			{
				_bottomBar.enabled = false;
				AnimationWidget.Get(_bottomBar.gameObject, 0.2f, 0f, deactiveWhenFadeout: true).Alpha = 0f;
			}
		}
	}

	private void UpdateButtons()
	{
		UIUtility.UpdateAnchors(_bottomBar.transform);
		_buttons.Set(KUtility.GetSize(_buttonActions));
		for (int i = 0; i < _buttons.Count; i++)
		{
			SelectableButton selectableButton = _buttons[i];
			GetButtonStyle(_buttonActions[i], out var style, out var tint);
			selectableButton.Text = _buttonActions[i].GetName();
			selectableButton.SetStyle(style);
			selectableButton.Color = tint;
		}
		_buttons.Reposition(Vector3.left, 10);
	}

	private void OnClickActionButtons()
	{
		int num = _buttons.IndexOf((SelectableButton)Selectable.Current);
		if (num != -1)
		{
			DoAction(_buttonActions[num]);
		}
	}

	private void DoAction(ClanAction action)
	{
		if (_selected == null)
		{
			return;
		}
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		switch (action)
		{
		case ClanAction.PlayerInfo:
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(_selected.EntityId, MemberInfo);
			break;
		case ClanAction.PromoteMember:
			ShowRoleSelector(_selected);
			break;
		case ClanAction.Kick:
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(_selected.EntityId, KickMember);
			break;
		case ClanAction.Leave:
			if (playerClan == null)
			{
				break;
			}
			UIManager.MessageBox.Show(T._("정말 <em>{0}</em> 부족을 떠나시겠습니까?", playerClan.Name), delegate(bool ok)
			{
				if (ok)
				{
					ClanSystem.LeaveClan();
				}
			});
			break;
		}
	}

	private void OnClickManageRole()
	{
		ClanRoleManageGroup clanRoleManageGroup = UIManager.FindScript<ClanRoleManageGroup>();
		if (clanRoleManageGroup != null)
		{
			clanRoleManageGroup.Open(GameSystem<ClanSystem>.Instance().PlayerClan);
		}
	}

	private void MemberInfo(Durango.Player.PlayerInfo info)
	{
		if (info.Valid)
		{
			PlayerInfoPopup playerInfoPopup = UIManager.Popup.Tooltip<PlayerInfoPopup>();
			playerInfoPopup.Set(info);
			playerInfoPopup.Direction = TooltipBase.TooltipDirection.Vertical;
			playerInfoPopup.AutoPosition = false;
			playerInfoPopup.Show();
			playerInfoPopup.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
		}
	}

	private void KickMember(Durango.Player.PlayerInfo info)
	{
		if (info.Valid)
		{
			UIManager.MessageBox.Show(T._("정말 <em>{0}</em>{0:-을} 추방하시겠습니까?", info.GetNameFreq(24, string.Empty)), delegate(bool ok)
			{
				if (ok)
				{
					ClanSystem.KickMember(info.EntityId);
				}
			});
		}
		else
		{
			ClanSystem.KickMember(info.EntityId);
		}
	}

	private void ShowRoleSelector(Durango.Logic.Clan.Member target)
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			return;
		}
		MemberRole role = _role;
		GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
		genericSelector.ResetArguments();
		int index = -1;
		List<MemberRole> roles = new List<MemberRole>();
		List<string> list = new List<string>();
		Permissions[] permissions = Clan.Permissions;
		foreach (KeyValuePair<int, MemberRole> roleInfo in playerClan.RoleInfos)
		{
			MemberRole value = roleInfo.Value;
			if (value.Grade < role.Grade && !role.IsSuperuser())
			{
				continue;
			}
			if (value.Id == target.RoleId)
			{
				index = roles.Count;
			}
			roles.Add(value);
			list.Clear();
			int i = 0;
			for (int size = KUtility.GetSize(permissions); i < size; i++)
			{
				Permissions permissions2 = permissions[i];
				if ((permissions2 & value.GetPermissions()) != 0)
				{
					list.Add(permissions2.GetName());
				}
			}
			genericSelector.AddItem(string.Format("{0}\n[size=20][777163]{1}", value.GetName(), (list.Count <= 0) ? T._("권한 없음") : T._("{0:l:{}|, }", list)));
		}
		genericSelector.DefaultSelectedIndex(index);
		genericSelector.SetSelected(delegate(int idx)
		{
			if (idx >= 0 && idx < roles.Count)
			{
				MemberRole role2 = roles[idx];
				if (role2.IsSuperuser())
				{
					Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(target.EntityId, delegate(Durango.Player.PlayerInfo player)
					{
						UIManager.MessageBox.Show(T._("<em>{0}</em>님을 <em>{1}</em> 등급으로 변경하시겠습니까?", player.GetNameFreq(24, string.Empty), role2.GetName()), string.Format("<alert>[icon=icon_make_alert]{0}</alert>", T._("부족 내의 모든 권한을 보유하게 되며, 당신의 등급을 변경할 수 있습니다")), delegate(bool ok)
						{
							if (ok)
							{
								ClanSystem.SetMemberRole(target, role2.Id);
							}
						});
					});
				}
				else
				{
					ClanSystem.SetMemberRole(target, role2.Id);
				}
			}
		});
		genericSelector.Show();
	}
}
