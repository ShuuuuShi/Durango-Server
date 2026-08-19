using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class MemberRoleList : UIWidget
{
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private MemberRoleNode _baseRoleNode;

	[SerializeField]
	private UIWidget _addRoleButton;

	[SerializeField]
	private UILabel _infoLabel;

	private ListObjectPool<MemberRoleNode> _roleList;

	private Clan _clan;

	private MemberRole? _myRole;

	private bool _isDirtyRoleOrder;

	private readonly List<int> _roleOrder = new List<int>();

	private bool _isInit;

	public event Action RoleAdded;

	public event Action<List<int>> RoleOrderUpdated;

	public event Action<MemberRole> EditClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_roleList = new ListObjectPool<MemberRoleNode>();
		_roleList.BaseObject = _baseRoleNode;
		_roleList.UseBase = true;
		_roleList.Init(InitRoleNode);
		_infoLabel.text = string.Format("[icon=icon_make_alert] {0}", T._("등급의 순서를 변경할 수 있습니다."));
		Selectable component = _addRoleButton.GetComponent<Selectable>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, (Action)delegate
		{
			if (this.RoleAdded != null)
			{
				this.RoleAdded();
			}
		});
		UIUtility.FindComponentInParent<UIBase>(base.gameObject).OnCloseSucceed += delegate
		{
			_scrollView.ResetPosition();
		};
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			GameSystem<ClanSystem>.Instance().ClanInfoUpdated += Refresh;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			GameSystem<ClanSystem>.Instance().ClanInfoUpdated -= Refresh;
			ApplyRoleOrder();
		}
	}

	private void InitRoleNode(MemberRoleNode node)
	{
		node.EditClicked += OnRoleEditClick;
		node.MoveClicked += OnRoleMoved;
	}

	private void OnRoleEditClick(MemberRoleNode node)
	{
		if (this.EditClicked != null)
		{
			this.EditClicked(node.Role);
		}
	}

	private void OnRoleMoved(MemberRoleNode node, int delta)
	{
		if (delta == 0)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < _roleOrder.Count; i++)
		{
			if (_roleOrder[i] == node.Role.Id)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			int num2 = num + delta;
			if (num2 >= 0 && num2 < _roleOrder.Count && (!_clan.TryGetRole(num2, out var role) || !role.IsSuperuser()))
			{
				int value = _roleOrder[num];
				_roleOrder[num] = _roleOrder[num2];
				_roleOrder[num2] = value;
				UpdateRoleOrder();
			}
		}
	}

	public void Set(Clan clan)
	{
		_clan = clan;
		Refresh();
	}

	private void Refresh()
	{
		Init();
		_myRole = null;
		Messages.Member clan = PlayerBehavior.LocalPlayer.Clan;
		if (clan.ClanId == _clan.Id && _clan.TryGetRole(clan.RoleId, out var role))
		{
			_myRole = role;
		}
		_isDirtyRoleOrder = true;
		_roleOrder.Clear();
		_roleList.BeginLoad();
		int a = 0;
		foreach (KeyValuePair<int, MemberRole> roleInfo in _clan.RoleInfos)
		{
			MemberRoleNode next = _roleList.GetNext();
			int b = next.Set(roleInfo.Value, _myRole);
			a = Mathf.Max(a, b);
			_roleOrder.Add(roleInfo.Key);
		}
		_roleList.EndLoad();
		for (int i = 0; i < _roleList.Count; i++)
		{
			_roleList[i].height = a;
		}
		UIUtility.UpdateAnchors(base.transform);
		List<UIWidget> widgets = _scrollView.Widgets;
		widgets.Clear();
		for (int j = 0; j < _roleList.Count; j++)
		{
			widgets.Add(_roleList[j]);
		}
		widgets.Add(_addRoleButton);
		RefreshScrollViewWidgets();
		_scrollView.Reposition();
	}

	private void UpdateRoleOrder()
	{
		int num = 0;
		for (int i = 0; i < _roleOrder.Count; i++)
		{
			int num2 = -1;
			for (int j = num; j < _roleList.Count; j++)
			{
				if (_roleList[j].Role.Id == _roleOrder[i])
				{
					num2 = j;
					break;
				}
			}
			if (num2 != -1)
			{
				_roleList.Swap(num, num2);
				num++;
			}
		}
		RefreshScrollViewWidgets();
		_scrollView.UpdateLayout(instant: false);
	}

	private void RefreshScrollViewWidgets()
	{
		List<UIWidget> widgets = _scrollView.Widgets;
		widgets.Clear();
		for (int i = 0; i < _roleList.Count; i++)
		{
			widgets.Add(_roleList[i]);
		}
		widgets.Add(_addRoleButton);
	}

	public void ApplyRoleOrder()
	{
		if (!_isDirtyRoleOrder)
		{
			return;
		}
		_isDirtyRoleOrder = false;
		bool flag = false;
		int num = 0;
		foreach (KeyValuePair<int, MemberRole> roleInfo in _clan.RoleInfos)
		{
			if (roleInfo.Key != _roleOrder[num])
			{
				flag = true;
				break;
			}
			num++;
		}
		if (flag && this.RoleOrderUpdated != null)
		{
			this.RoleOrderUpdated(_roleOrder);
		}
	}
}
