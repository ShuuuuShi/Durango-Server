using System;
using System.Text;
using ClanData;
using L10N;
using Messages;
using Shared.Clan;
using UnityEngine;

public class MemberRoleNode : MonoBehaviour
{
	private static StringBuilder _builder = new StringBuilder();

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private Selectable _upButton;

	[SerializeField]
	private Selectable _downButton;

	[SerializeField]
	private UILabel _permissionLabel;

	[SerializeField]
	private GameObject _opener;

	[SerializeField]
	private UISprite _openerSprite;

	[SerializeField]
	private AnimationWidget _detailWidget;

	[SerializeField]
	private UIWidget _permissionListWidget;

	[SerializeField]
	private ListObjectPool _permissionList;

	[SerializeField]
	private ListObjectPool _buttons;

	private bool _isOpenDetailView;

	private WidgetLayoutController _widgetLayout;

	private RoleEditAction[] _buttonActions;

	private MemberRole _role;

	private bool _hasPermission;

	private bool _isInit;

	public MemberRole Role => _role;

	public event Action<MemberRoleNode, bool> RoleInfoOpned;

	public event Action<MemberRoleNode, RoleEditAction> OnRoleEditAction;

	public event Action<MemberRoleNode> PermissionChanged;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			InitPermissionList();
			UIEventListener uIEventListener = UIEventListener.Get(_opener);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickOpener));
			((Component)_detailWidget).gameObject.SetActive(false);
			_widgetLayout = ((Component)this).GetComponent<WidgetLayoutController>();
		}
	}

	private void OnDisable()
	{
		_isOpenDetailView = false;
	}

	private void InitPermissionList()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		Permissions[] permissions = Clan.Permissions;
		_permissionList.Set(permissions.Length);
		for (int i = 0; i < _permissionList.Count; i++)
		{
			MemberRolePermissionNode component = _permissionList[i].GetComponent<MemberRolePermissionNode>();
			component.Set(permissions[i]);
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickPermission));
			component.EnableSeparator(i < _permissionList.Count - 1);
		}
		float num = _permissionList.Reposition(Vector3.down);
		_permissionListWidget.height = (int)num;
		Selectable upButton = _upButton;
		upButton.Clicked = (Action)Delegate.Combine(upButton.Clicked, new Action(OnClickUpButton));
		Selectable downButton = _downButton;
		downButton.Clicked = (Action)Delegate.Combine(downButton.Clicked, new Action(OnClickDownButton));
		_buttonActions = new RoleEditAction[2]
		{
			RoleEditAction.ChangeName,
			RoleEditAction.Delete
		};
		_buttons.Init(delegate(GameObject o)
		{
			DefaultSelectableButton component3 = o.GetComponent<DefaultSelectableButton>();
			component3.Widget.SetAnchor((Transform)null);
			component3.Clicked = (Action)Delegate.Combine(component3.Clicked, new Action(OnClickActionButton));
		});
		_buttons.Set(_buttonActions.Length);
		for (int j = 0; j < _buttons.Count; j++)
		{
			DefaultSelectableButton component2 = _buttons[j].GetComponent<DefaultSelectableButton>();
			component2.Text = _buttonActions[j].GetName();
		}
		_buttons.Reposition(Vector2.op_Implicit(Vector2.left), 5);
	}

	public void Set(MemberRole role, MemberRole? myRole)
	{
		Init();
		_role = role;
		_nameLabel.text = role.Name;
		UpdatePermissionList();
		_hasPermission = myRole.HasValue && myRole.Value.Grade <= role.Grade;
		for (int i = 0; i < _buttonActions.Length; i++)
		{
			DefaultSelectableButton component = _buttons[i].GetComponent<DefaultSelectableButton>();
			RoleEditAction roleEditAction = _buttonActions[i];
			RoleEditAction roleEditAction2 = roleEditAction;
			if (roleEditAction2 == RoleEditAction.Delete)
			{
				component.Disable = role.Grade == 0 || !_hasPermission;
			}
			else
			{
				component.Disable = !_hasPermission;
			}
		}
		_upButton.Disable = role.Grade == 0 || !_hasPermission;
		_downButton.Disable = role.Grade == 0 || !_hasPermission;
		ShowDetailView(_isOpenDetailView, init: true);
	}

	public void ChangeName(string text)
	{
		_role.Name = text;
		_nameLabel.text = _role.Name;
	}

	private void OnClickOpener(GameObject obj)
	{
		ShowDetailView(!_isOpenDetailView, init: false);
	}

	private void ShowDetailView(bool show, bool init)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		_isOpenDetailView = show;
		Quaternion val = Quaternion.Euler(0f, 0f, (!show) ? 0f : 180f);
		if (init)
		{
			((Component)_openerSprite).transform.localRotation = val;
			((Behaviour)_detailWidget.Widget).enabled = show;
			((Component)_detailWidget).gameObject.SetActive(show);
		}
		else
		{
			TweenRotation.Begin(((Component)_openerSprite).gameObject, 0.2f, val);
			if (show)
			{
				((Component)_detailWidget).gameObject.SetActive(true);
				((Behaviour)_detailWidget.Widget).enabled = true;
				_detailWidget.Alpha = 1f;
			}
			else
			{
				((Behaviour)_detailWidget.Widget).enabled = false;
				_detailWidget.Alpha = 0f;
			}
		}
		_widgetLayout.UpdateLayout();
		UIUtility.UpdateAnchors(((Component)this).transform);
		if (!init && this.RoleInfoOpned != null)
		{
			this.RoleInfoOpned(this, _isOpenDetailView);
		}
	}

	private void OnClickActionButton()
	{
		if (!Selectable.Current.Disable)
		{
			int num = _buttons.IndexOf(((Component)Selectable.Current).gameObject);
			RoleEditAction arg = _buttonActions[num];
			if (this.OnRoleEditAction != null)
			{
				this.OnRoleEditAction(this, arg);
			}
		}
	}

	private void OnClickUpButton()
	{
		if (!Selectable.Current.Disable && this.OnRoleEditAction != null)
		{
			this.OnRoleEditAction(this, RoleEditAction.MoveToFront);
		}
	}

	private void OnClickDownButton()
	{
		if (!Selectable.Current.Disable && this.OnRoleEditAction != null)
		{
			this.OnRoleEditAction(this, RoleEditAction.MoveToBack);
		}
	}

	private void OnClickPermission()
	{
		if (!_hasPermission)
		{
			return;
		}
		int num = _permissionList.IndexOf(((Component)Selectable.Current).gameObject);
		if (num != -1)
		{
			Permissions[] permissions = Clan.Permissions;
			if ((_role.Permissions & permissions[num]) == 0)
			{
				_role.Permissions |= permissions[num];
			}
			else
			{
				_role.Permissions &= ~permissions[num];
			}
			UpdatePermissionList();
			if (this.PermissionChanged != null)
			{
				this.PermissionChanged(this);
			}
		}
	}

	private void UpdatePermissionList()
	{
		Permissions[] permissions = Clan.Permissions;
		_builder.Length = 0;
		foreach (Permissions permissions2 in permissions)
		{
			if ((permissions2 & _role.Permissions) != 0)
			{
				if (_builder.Length > 0)
				{
					_builder.Append(", ");
				}
				_builder.Append(permissions2.GetName());
			}
		}
		_permissionLabel.text = _builder.ToString();
		for (int j = 0; j < permissions.Length; j++)
		{
			MemberRolePermissionNode component = _permissionList[j].GetComponent<MemberRolePermissionNode>();
			Permissions permissions3 = permissions[j];
			component.Select = (_role.Permissions & permissions3) != 0;
		}
	}
}
