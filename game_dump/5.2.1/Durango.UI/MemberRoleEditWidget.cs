using System;
using Durango.Logic.Clan;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using Shared.Clan;
using UnityEngine;

namespace Durango.UI;

public class MemberRoleEditWidget : UIWidget
{
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private UIInput _roleNameInput;

	[SerializeField]
	private UILabel _roleNameHelpLabel;

	[SerializeField]
	private RectLayoutComponent _permissionLayout;

	[SerializeField]
	private UIWidget _permissionsWidget;

	[SerializeField]
	private ListObjectPool _permissions;

	[SerializeField]
	private SelectableButton _submitButton;

	[SerializeField]
	private SelectableButton _deleteRoleButton;

	private Selectable[] _permissionChecks;

	private MemberRole _role;

	private Permissions _modifiedPermissions;

	private bool _isInit;

	public event Action<MemberRole> RoleChanged;

	public event Action<int> RoleRemoved;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_roleNameInput.defaultText = T._("등급명을 입력하세요");
			_roleNameHelpLabel.text = "[icon=icon_question_big] " + LocalizeUtil.GetNameRoleHelpText();
			UIEventListener uIEventListener = UIEventListener.Get(_roleNameHelpLabel.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnRoleNameHelpClick));
			_submitButton.Text = T._("저장");
			_deleteRoleButton.Text = T._("등급 삭제");
			SelectableButton submitButton = _submitButton;
			submitButton.Clicked = (Action)Delegate.Combine(submitButton.Clicked, new Action(OnSubmit));
			SelectableButton deleteRoleButton = _deleteRoleButton;
			deleteRoleButton.Clicked = (Action)Delegate.Combine(deleteRoleButton.Clicked, new Action(OnDeleteRole));
			Permissions[] permissions = Clan.Permissions;
			_permissions.Set(permissions.Length);
			_permissionChecks = new Selectable[_permissions.Count];
			for (int i = 0; i < _permissions.Count; i++)
			{
				GameObject gameObject = _permissions[i];
				SetPermissionNode(gameObject, permissions[i]);
				_permissionChecks[i] = gameObject.GetComponentInChildren<Selectable>();
				Selectable obj = _permissionChecks[i];
				obj.Clicked = (Action)Delegate.Combine(obj.Clicked, new Action(OnClickPermissionCheck));
				gameObject.transform.Find("separator").gameObject.SetActive(i > 0);
			}
		}
	}

	public void Set(MemberRole role)
	{
		Init();
		_permissionsWidget.height = (int)UIUtility.WidgetsReposition(_permissions, _permissionsWidget, Vector3.down);
		_permissionLayout.UpdateLayout();
		_role = role;
		_roleNameInput.value = role.Name;
		SetPermissions(role.GetPermissions());
		_permissionsWidget.alpha = ((!role.IsSuperuser()) ? 1f : 0.5f);
		_scrollView.ResetPosition();
	}

	private void SetPermissions(Permissions permissions)
	{
		_modifiedPermissions = permissions;
		Permissions[] permissions2 = Clan.Permissions;
		for (int i = 0; i < permissions2.Length; i++)
		{
			_permissionChecks[i].Selected = (permissions & permissions2[i]) != 0;
		}
	}

	private void SetPermissionNode(GameObject node, Permissions permission)
	{
		string key = LocalizeUtil.GetKey(permission);
		node.transform.Find("Name").GetComponent<UILabel>().text = LocalizeSystem.Get(key);
		node.transform.Find("Description").GetComponent<UILabel>().text = LocalizeSystem.Get(key + "_description");
	}

	private void OnClickPermissionCheck()
	{
		if (_role.IsSuperuser())
		{
			UIManager.SystemMsg(T._("최고 관리자 등급은 권한을 수정할 수 없습니다."));
			return;
		}
		int num = _permissionChecks.IndexOf(Selectable.Current);
		if (num != -1)
		{
			Permissions permissions = Clan.Permissions[num];
			Permissions modifiedPermissions = _modifiedPermissions;
			modifiedPermissions = (((modifiedPermissions & permissions) != 0) ? (modifiedPermissions & ~permissions) : (modifiedPermissions | permissions));
			SetPermissions(modifiedPermissions);
		}
	}

	private void OnSubmit()
	{
		if (this.RoleChanged != null)
		{
			MemberRole role = _role;
			role.Name = _roleNameInput.value;
			role.Permissions = _modifiedPermissions;
			this.RoleChanged(role);
		}
	}

	private void OnDeleteRole()
	{
		if (this.RoleRemoved != null)
		{
			this.RoleRemoved(_role.Id);
		}
	}

	private void OnRoleNameHelpClick(GameObject obj)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, LocalizeUtil.GetNameRoleDescription());
		widgetTooltipControl.AutoPosition = false;
		widgetTooltipControl.Show();
		Vector3 pos = widgetTooltipControl.transform.parent.InverseTransformPoint(_roleNameHelpLabel.worldCorners[2]);
		pos.y -= _roleNameHelpLabel.printedSize.y + 15f;
		widgetTooltipControl.Widget.SetPosition(pos, 1f, 1f);
		widgetTooltipControl.HideArrow();
	}
}
