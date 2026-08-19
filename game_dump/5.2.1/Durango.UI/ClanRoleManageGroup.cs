using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Clan;
using UnityEngine;

namespace Durango.UI;

public class ClanRoleManageGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private MemberRoleList _roleList;

	[SerializeField]
	private MemberRoleEditWidget _roleEditWidget;

	private Clan _clan;

	private bool _isModifying;

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("부족 - 등급 관리"));
		_roleList.EditClicked += ShowRoleEdit;
		_roleList.RoleAdded += OnAddRole;
		_roleList.RoleOrderUpdated += OnUpdateRoleOrder;
		_roleEditWidget.RoleChanged += OnChangeRole;
		_roleEditWidget.RoleRemoved += OnRemoveRole;
		SetChildrenActive(activated: false);
	}

	public void Open(Clan clan)
	{
		_clan = clan;
		base.Open();
		ShowRoleList();
	}

	protected override bool TryClose()
	{
		if (_roleEditWidget.gameObject.activeSelf)
		{
			ShowRoleList();
			return false;
		}
		return base.TryClose();
	}

	private void ShowRoleList()
	{
		_roleList.Set(_clan);
		_roleList.gameObject.SetActive(value: true);
		_roleEditWidget.gameObject.SetActive(value: false);
	}

	private void ShowRoleEdit(MemberRole role)
	{
		_roleEditWidget.Set(role);
		_roleList.gameObject.SetActive(value: false);
		_roleEditWidget.gameObject.SetActive(value: true);
	}

	private void OnAddRole()
	{
		int num = -1;
		int num2 = -1;
		foreach (KeyValuePair<int, MemberRole> roleInfo in _clan.RoleInfos)
		{
			num = Mathf.Max(num, roleInfo.Key);
			num2 = Mathf.Max(num2, roleInfo.Value.Grade);
		}
		MemberRole memberRole = default(MemberRole);
		memberRole.Id = num + 1;
		memberRole.Grade = num2 + 1;
		memberRole.UserType = UserType.Normal;
		MemberRole role = memberRole;
		ShowRoleEdit(role);
	}

	private void OnUpdateRoleOrder(List<int> roleOrder)
	{
		ClanSystem.SetMemberRoleGrades(roleOrder, null);
	}

	private void OnChangeRole(MemberRole role)
	{
		if (_isModifying)
		{
			return;
		}
		_isModifying = true;
		ClanSystem.SetMemberRoleInfo(role, delegate(bool success)
		{
			_isModifying = false;
			if (success)
			{
				ShowRoleList();
			}
		});
	}

	private void OnRemoveRole(int roleId)
	{
		GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
		genericSelector.ResetArguments();
		MemberRole role = default(MemberRole);
		Messages.Member clan = PlayerBehavior.LocalPlayer.Clan;
		if (clan.ClanId != _clan.Id || !_clan.TryGetRole(clan.RoleId, out role))
		{
			return;
		}
		List<MemberRole> roles = new List<MemberRole>();
		List<string> list = new List<string>();
		Permissions[] permissions = Clan.Permissions;
		foreach (KeyValuePair<int, MemberRole> roleInfo in _clan.RoleInfos)
		{
			MemberRole value = roleInfo.Value;
			if (value.Id != roleId && value.Grade >= role.Grade && !role.IsSuperuser())
			{
				continue;
			}
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
			string text = ((list.Count <= 0) ? T._("권한 없음") : T._("{0:l:{}|, }", list));
			if (value.Id == roleId)
			{
				genericSelector.SetInfo("[size=30]<em>" + value.GetName() + "</em>[/size]\n[size=4] [/size]\n[size=20][FFFFFF7F]" + text + "[-][/size]");
			}
			else
			{
				roles.Add(value);
				genericSelector.AddItem(value.GetName() + "\n[size=20][777163]" + text);
			}
		}
		if (roles.Count == 0)
		{
			UIManager.SystemMsg(T._("해당 등급을 옮길 수 있는 곳이 없습니다"));
			return;
		}
		genericSelector.SetTitle(T._("부족원의 새로운 등급을 정해주세요."));
		genericSelector.SetSelected(delegate(int idx)
		{
			if (idx >= 0 && idx < roles.Count)
			{
				ClanSystem.RemoveMemberRole(roleId, roles[idx].Id, delegate(bool success)
				{
					if (success)
					{
						ShowRoleList();
					}
				});
			}
		});
		genericSelector.Show();
	}
}
