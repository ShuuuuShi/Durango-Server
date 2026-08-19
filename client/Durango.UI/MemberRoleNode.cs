using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Clan;
using UnityEngine;

namespace Durango.UI;

public class MemberRoleNode : UIWidget
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private Selectable _upButton;

	[SerializeField]
	private Selectable _downButton;

	[SerializeField]
	private UISpriteLabel _permissionLabel;

	[SerializeField]
	private Selectable _roleEditButton;

	[SerializeField]
	private RectLayout _layout;

	private MemberRole _role;

	private bool _isInit;

	public MemberRole Role => _role;

	public event Action<MemberRoleNode> EditClicked;

	public event Action<MemberRoleNode, int> MoveClicked;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			Selectable upButton = _upButton;
			upButton.Clicked = (Action)Delegate.Combine(upButton.Clicked, new Action(OnClickUpButton));
			Selectable downButton = _downButton;
			downButton.Clicked = (Action)Delegate.Combine(downButton.Clicked, new Action(OnClickDownButton));
			Selectable roleEditButton = _roleEditButton;
			roleEditButton.Clicked = (Action)Delegate.Combine(roleEditButton.Clicked, new Action(OnClickEditButton));
		}
	}

	public int Set(MemberRole role, MemberRole? myRole)
	{
		Init();
		_layout.UpdateLayout();
		_role = role;
		_nameLabel.text = role.GetName();
		UpdatePermissionList();
		bool flag = myRole.HasValue && (myRole.Value.IsSuperuser() || myRole.Value.Grade <= role.Grade);
		bool flag2 = !role.IsSuperuser() && flag;
		_upButton.Disabled = !flag2;
		_downButton.Disabled = !flag2;
		_roleEditButton.Disabled = !flag;
		return (int)_permissionLabel.printedSize.y + 116;
	}

	private void OnClickEditButton()
	{
		if (this.EditClicked != null)
		{
			this.EditClicked(this);
		}
	}

	private void OnClickUpButton()
	{
		if (this.MoveClicked != null)
		{
			this.MoveClicked(this, -1);
		}
	}

	private void OnClickDownButton()
	{
		if (this.MoveClicked != null)
		{
			this.MoveClicked(this, 1);
		}
	}

	private void UpdatePermissionList()
	{
		Permissions[] permissions = Clan.Permissions;
		using Reusable<List<string>> reusable = ReusableList<string>.Pop();
		List<string> value = reusable.Value;
		foreach (Permissions permissions2 in permissions)
		{
			if ((permissions2 & _role.GetPermissions()) != 0)
			{
				value.Add(permissions2.GetName());
			}
		}
		_permissionLabel.text = string.Format("[preset=round_box?{1}] {0}", T._("{0:l:{}|, }", value), T._("권한"));
	}
}
