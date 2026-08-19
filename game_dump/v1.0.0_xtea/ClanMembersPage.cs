using System;
using System.Collections.Generic;
using ClanData;
using L10N;
using Messages;
using Player;
using Shared.Clan;
using UnityEngine;

public class ClanMembersPage : MonoBehaviour
{
	[SerializeField]
	private UISpriteLabel _memberCountLabel;

	[SerializeField]
	private ListObjectPool _titleButtons;

	[SerializeField]
	private KScrollView _membersScroll;

	[SerializeField]
	private UIWidget _bottomBar;

	[SerializeField]
	private ListObjectPool _leftButtons;

	[SerializeField]
	private ListObjectPool _rightButtons;

	private ClanData.Member _selected;

	private List<ClanAction> _titleActions = new List<ClanAction>();

	private List<ClanAction> _leftActions = new List<ClanAction>();

	private List<ClanAction> _rightActions = new List<ClanAction>();

	private bool _validRole;

	private MemberRole _role;

	private bool _resetFlag;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_membersScroll.Nodes.Init(OnInitMemberNodes);
			_leftButtons.Init(delegate(GameObject o)
			{
				o.GetComponent<UIRect>().SetAnchor((Transform)null);
				Selectable component3 = o.GetComponent<Selectable>();
				component3.Clicked = (Action)Delegate.Combine(component3.Clicked, new Action(OnClickLeftButtons));
			});
			_rightButtons.Init(delegate(GameObject o)
			{
				o.GetComponent<UIRect>().SetAnchor((Transform)null);
				Selectable component2 = o.GetComponent<Selectable>();
				component2.Clicked = (Action)Delegate.Combine(component2.Clicked, new Action(OnClickRightButtons));
			});
			_titleButtons.Init(delegate(GameObject o)
			{
				o.GetComponent<UIRect>().SetAnchor((Transform)null);
				Selectable component = o.GetComponent<Selectable>();
				component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickTitleButtons));
			});
		}
	}

	private void OnInitMemberNodes(GameObject obj)
	{
		ClanMemberNode component = obj.GetComponent<ClanMemberNode>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickMemberNode));
	}

	private void OnEnable()
	{
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated -= Refresh;
		_selected = null;
		_resetFlag = false;
	}

	private void Refresh()
	{
		Init();
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		_role = default(MemberRole);
		_validRole = false;
		ClanData.Member member = playerClan.GetMember(GameManager.PlayerId);
		_validRole = member != null && playerClan.TryGetRole(member.RoleId, out _role);
		SetTitle(playerClan);
		SetMembers(playerClan);
		UpdateLayout();
		_resetFlag = true;
	}

	private void OnClickMemberNode()
	{
		ClanMemberNode clanMemberNode = Selectable.Current as ClanMemberNode;
		if (!((Object)(object)clanMemberNode == (Object)null))
		{
			SelectMember(clanMemberNode.Member);
		}
	}

	private void SetTitle(Clan clan)
	{
		_memberCountLabel.text = $"[icon=icon_person] <em>{clan.MemberCount}</em> / {clan.Capacity}";
		_titleActions.Clear();
		if (_validRole && (_role.Permissions & Permissions.EditClanInfo) != 0)
		{
			_titleActions.Add(ClanAction.EditClanInfo);
		}
		UpdateTitleButtons();
	}

	private void SetMembers(Clan clan)
	{
		ClanData.Member[] members = clan.Members;
		ClanData.Member[] appliers = clan.Appliers;
		ListObjectPool nodes = _membersScroll.Nodes;
		int size = KUtility.GetSize(members);
		int size2 = KUtility.GetSize(appliers);
		nodes.Set(size + size2);
		for (int i = 0; i < nodes.Count; i++)
		{
			ClanMemberNode component = nodes[i].GetComponent<ClanMemberNode>();
			ClanData.Member member = ((i >= size) ? appliers[i - size] : members[i]);
			component.Select = false;
			component.Set(clan, member);
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
	}

	private void SelectMember(ClanData.Member member)
	{
		if (object.Equals(_selected, member))
		{
			if (_selected == null)
			{
				return;
			}
			_selected = null;
		}
		else
		{
			_selected = member;
		}
		ListObjectPool nodes = _membersScroll.Nodes;
		ulong num = ((_selected != null) ? _selected.EntityId : 0);
		for (int i = 0; i < nodes.Count; i++)
		{
			ClanMemberNode component = nodes[i].GetComponent<ClanMemberNode>();
			component.Select = num != 0L && ((component.Member != null) ? component.Member.EntityId : 0) == num;
		}
		if (_selected != null)
		{
			_leftActions.Clear();
			_rightActions.Clear();
			bool flag = num == GameManager.PlayerId;
			if (flag)
			{
				_leftActions.Add(ClanAction.Leave);
			}
			if (_validRole)
			{
				_rightActions.Add(ClanAction.MemberInfo);
				if (_selected.IsApplier)
				{
					if ((_role.Permissions & Permissions.ApproveMember) != 0)
					{
						_leftActions.Add(ClanAction.Approve);
						_leftActions.Add(ClanAction.DropApplier);
					}
				}
				else
				{
					bool flag2 = false;
					if (GameSystem<ClanSystem>.Instance().PlayerClan.TryGetRole(_selected.RoleId, out var role))
					{
						flag2 = _role.Grade <= role.Grade;
					}
					if ((_role.Permissions & Permissions.PromoteMember) != 0 && flag2)
					{
						_leftActions.Add(ClanAction.PromoteMember);
					}
					if (_role.Grade == 0 && !flag)
					{
						_leftActions.Add(ClanAction.Kick);
					}
				}
			}
			UpdateButtons();
		}
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		bool enabled = ((Behaviour)_bottomBar).enabled;
		if (enabled != (_selected != null))
		{
			if (!enabled)
			{
				((Component)_bottomBar).gameObject.SetActive(true);
				((Behaviour)_bottomBar).enabled = true;
				AnimationWidget.Get(((Component)_bottomBar).gameObject, 0.2f, 0f, deactiveWhenFadeout: true).Alpha = 1f;
			}
			else
			{
				((Behaviour)_bottomBar).enabled = false;
				AnimationWidget.Get(((Component)_bottomBar).gameObject, 0.2f, 0f, deactiveWhenFadeout: true).Alpha = 0f;
			}
			WidgetLayoutController component = ((Component)this).GetComponent<WidgetLayoutController>();
			component.UpdateLayout();
		}
	}

	private void UpdateTitleButtons()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		_titleButtons.Set(KUtility.GetSize(_titleActions));
		for (int i = 0; i < _titleButtons.Count; i++)
		{
			DefaultSelectableButton component = _titleButtons[i].GetComponent<DefaultSelectableButton>();
			component.Text = _titleActions[i].GetName();
		}
		_titleButtons.Reposition(Vector3.left, 5);
	}

	private void UpdateButtons()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		_leftButtons.Set(KUtility.GetSize(_leftActions));
		for (int i = 0; i < _leftButtons.Count; i++)
		{
			DefaultSelectableButton component = _leftButtons[i].GetComponent<DefaultSelectableButton>();
			component.Text = _leftActions[i].GetName();
		}
		_leftButtons.Reposition(Vector3.right, 5);
		_rightButtons.Set(KUtility.GetSize(_rightActions));
		for (int j = 0; j < _rightButtons.Count; j++)
		{
			DefaultSelectableButton component2 = _rightButtons[j].GetComponent<DefaultSelectableButton>();
			component2.Text = _rightActions[j].GetName();
		}
		_rightButtons.Reposition(Vector3.left, 5);
	}

	private void OnClickLeftButtons()
	{
		int num = _leftButtons.IndexOf(((Component)Selectable.Current).gameObject);
		if (num != -1)
		{
			DoAction(_leftActions[num]);
		}
	}

	private void OnClickRightButtons()
	{
		int num = _rightButtons.IndexOf(((Component)Selectable.Current).gameObject);
		if (num != -1)
		{
			DoAction(_rightActions[num]);
		}
	}

	private void OnClickTitleButtons()
	{
		int num = _titleButtons.IndexOf(((Component)Selectable.Current).gameObject);
		if (num != -1)
		{
			DoAction(_titleActions[num]);
		}
	}

	private void DoAction(ClanAction action)
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		switch (action)
		{
		case ClanAction.PromoteMember:
		{
			MemberRoleSelector memberRoleSelector = UIManager.Popup.Tooltip<MemberRoleSelector>();
			memberRoleSelector.Set(playerClan, (MemberRole role, MemberRole? myRole) => myRole.HasValue && myRole.Value.Grade <= role.Grade && role.Id != _selected.RoleId, T._("등급 변경"), T._("등급을 변경합니다"), delegate(int id)
			{
				if (id != -1)
				{
					ClanSystem.SetMemberRole(_selected, id);
				}
			});
			memberRoleSelector.Show();
			break;
		}
		case ClanAction.Kick:
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(_selected.EntityId, KickMember, useOldCache: true);
			break;
		case ClanAction.MemberInfo:
			if (_selected != null)
			{
				KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(_selected.EntityId, MemberInfo);
			}
			break;
		case ClanAction.Leave:
			if (playerClan == null)
			{
				break;
			}
			UIManager.MessageBox.Show(T._("정말 {0:을} 떠나시겠습니까?", playerClan.Name), delegate(bool ok)
			{
				if (ok)
				{
					ClanSystem.LeaveClan();
				}
			});
			break;
		case ClanAction.Approve:
			ClanSystem.ApproveApplier(_selected.EntityId);
			break;
		case ClanAction.DropApplier:
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(_selected.EntityId, DropApplier, useOldCache: true);
			break;
		case ClanAction.EditClanInfo:
		{
			EditClanInfoGroup editClanInfoGroup = UIManager.FindScript<EditClanInfoGroup>();
			if ((Object)(object)editClanInfoGroup != (Object)null)
			{
				editClanInfoGroup.Open(playerClan);
			}
			break;
		}
		}
	}

	private void MemberInfo(Player.PlayerInfo info)
	{
		ProfileTooltip profileTooltip = UIManager.Popup.Tooltip<ProfileTooltip>();
		profileTooltip.Set(info);
		profileTooltip.Show();
	}

	private void KickMember(Player.PlayerInfo info)
	{
		if (info.Valid)
		{
			UIManager.MessageBox.Show(T._("정말 {0:을} 추방하시겠습니까?", info.Name), delegate(bool ok)
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

	private void DropApplier(Player.PlayerInfo info)
	{
		if (info.Valid)
		{
			UIManager.MessageBox.Show(T._("{0}의 신청을 취소하시겠습니까?", info.Name), delegate(bool ok)
			{
				if (ok)
				{
					ClanSystem.DropApplier(info.EntityId);
				}
			});
		}
		else
		{
			ClanSystem.DropApplier(info.EntityId);
		}
	}
}
