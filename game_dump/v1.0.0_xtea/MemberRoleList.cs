using System;
using System.Collections.Generic;
using ClanData;
using L10N;
using Messages;
using UnityEngine;

public class MemberRoleList : MonoBehaviour
{
	[SerializeField]
	private KScrollView _roleList;

	[SerializeField]
	private Selectable _addGradeButton;

	private bool _isChanged;

	private Clan _clan;

	private MemberRole? _myRole;

	private bool _isInit;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		GameObject baseObject = _roleList.Nodes.BaseObject;
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(baseObject.transform);
		int num = int.MaxValue;
		int num2 = int.MinValue;
		while (stack.Count > 0)
		{
			Transform val = stack.Pop();
			UIWidget component = ((Component)val).GetComponent<UIWidget>();
			num = Mathf.Min(num, component.depth);
			num2 = Mathf.Max(num2, component.depth);
			for (int i = 0; i < val.childCount; i++)
			{
				stack.Push(val.GetChild(i));
			}
		}
		_roleList.Nodes.Init(InitRoleNode);
		Selectable addGradeButton = _addGradeButton;
		addGradeButton.Clicked = (Action)Delegate.Combine(addGradeButton.Clicked, new Action(OnAddNewGrade));
	}

	private void OnEnable()
	{
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated += OnUpdateClan;
	}

	private void OnDisable()
	{
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated -= OnUpdateClan;
		if (_isChanged)
		{
			Submit();
		}
	}

	private void InitRoleNode(GameObject o)
	{
		MemberRoleNode component = o.GetComponent<MemberRoleNode>();
		component.RoleInfoOpned += OnOpenRoleNode;
		component.OnRoleEditAction += OnRoleEditAction;
		component.PermissionChanged += OnPermissionChanged;
	}

	public void Set(Clan clan)
	{
		_clan = clan;
		Refresh();
		_roleList.ResetPosition();
	}

	private void OnUpdateClan()
	{
		if (((Behaviour)this).enabled)
		{
			Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
			if (_clan == playerClan)
			{
				Refresh();
				_roleList.Reposition();
			}
		}
	}

	private void Refresh()
	{
		Init();
		List<MemberRole> list = new List<MemberRole>();
		foreach (KeyValuePair<int, MemberRole> roleInfo in _clan.RoleInfos)
		{
			list.Add(roleInfo.Value);
		}
		list.Sort((MemberRole r1, MemberRole r2) => r1.Grade - r2.Grade);
		ListObjectPool nodes = _roleList.Nodes;
		nodes.Set(list.Count);
		_myRole = null;
		ClanData.Member member = _clan.GetMember(GameManager.PlayerId);
		if (member != null && _clan.TryGetRole(member.RoleId, out var role))
		{
			_myRole = role;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			MemberRoleNode component = nodes[i].GetComponent<MemberRoleNode>();
			component.Set(list[i], _myRole);
		}
	}

	private void OnOpenRoleNode(MemberRoleNode node, bool open)
	{
		_roleList.UpdateLayout(instant: false);
		if (open)
		{
			int index = _roleList.Nodes.IndexOf(((Component)node).gameObject);
			_roleList.MoveToNode(index, instant: false);
		}
		else
		{
			float currentOffset = _roleList.CurrentOffset;
			_roleList.MoveTo(currentOffset, instant: false);
		}
	}

	private void OnRoleEditAction(MemberRoleNode node, RoleEditAction action)
	{
		int num = _roleList.Nodes.IndexOf(((Component)node).gameObject);
		switch (action)
		{
		case RoleEditAction.MoveToFront:
			if (!SwapNode(num, num - 1))
			{
				return;
			}
			break;
		case RoleEditAction.MoveToBack:
			if (!SwapNode(num, num + 1))
			{
				return;
			}
			break;
		case RoleEditAction.ChangeName:
			UIManager.Popup.TextInput.Show(node.ChangeName, T._("{0} 의 이름을 변경합니다", node.Role.Name), node.Role.Name);
			break;
		case RoleEditAction.Delete:
		{
			int removeId = node.Role.Id;
			MemberRoleSelector memberRoleSelector = UIManager.Popup.Tooltip<MemberRoleSelector>();
			memberRoleSelector.Set(_clan, (MemberRole role, MemberRole? myRole) => myRole.HasValue && removeId != role.Id && role.Grade >= myRole.Value.Grade, T._("부족 등급 삭제"), T._("<em>{0}</em> 등급 부족원의 새로운 등급을 정해야 합니다.", node.Role.Name), delegate(int id)
			{
				if (id != -1)
				{
					RemoveRole(removeId, id);
				}
			});
			memberRoleSelector.Show();
			return;
		}
		}
		_isChanged = true;
	}

	private void OnPermissionChanged(MemberRoleNode node)
	{
		_isChanged = true;
	}

	private void RemoveRole(int id, int moveTo)
	{
		ClanData.Member[] members = _clan.Members;
		int i = 0;
		for (int size = KUtility.GetSize(members); i < size; i++)
		{
			if (members[i].RoleId == id)
			{
				ClanSystem.SetMemberRole(members[i], moveTo);
			}
		}
		int num = -1;
		for (int j = 0; j < _roleList.Nodes.Count; j++)
		{
			MemberRoleNode component = _roleList.Nodes[j].GetComponent<MemberRoleNode>();
			if (component.Role.Id == id)
			{
				num = j;
				break;
			}
		}
		if (num != -1)
		{
			_roleList.Nodes.Remove(num);
			_roleList.Reposition();
			Submit();
		}
	}

	private bool SwapNode(int i1, int i2)
	{
		if (!_myRole.HasValue)
		{
			return false;
		}
		int num = Mathf.Max(1, _myRole.Value.Grade);
		if (i1 < num || i1 >= _roleList.Nodes.Count || i2 < num || i2 >= _roleList.Nodes.Count)
		{
			return false;
		}
		_roleList.Nodes.Swap(i1, i2);
		_roleList.UpdateLayout(instant: false);
		return true;
	}

	private void AddRole(string text)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < _roleList.Nodes.Count; i++)
		{
			MemberRoleNode component = _roleList.Nodes[i].GetComponent<MemberRoleNode>();
			MemberRole role = component.Role;
			num = Mathf.Max(num, role.Grade);
			num2 = Mathf.Max(num2, role.Id);
		}
		MemberRole memberRole = default(MemberRole);
		memberRole.Name = text;
		memberRole.Id = num2 + 1;
		memberRole.Grade = num + 1;
		MemberRole role2 = memberRole;
		MemberRoleNode memberRoleNode = ((ListObjectPoolBase<GameObject>)_roleList.Nodes).Add<MemberRoleNode>();
		memberRoleNode.Set(role2, _myRole);
		_roleList.Reposition();
		Submit();
	}

	private void Submit()
	{
		Dictionary<int, MemberRole> dictionary = new Dictionary<int, MemberRole>();
		int num = 0;
		for (int i = 0; i < _roleList.Nodes.Count; i++)
		{
			MemberRoleNode component = _roleList.Nodes[i].GetComponent<MemberRoleNode>();
			MemberRole role = component.Role;
			role.Grade = num++;
			dictionary[role.Id] = role;
		}
		ClanSystem.SubmitRoleInfos(dictionary);
		_isChanged = false;
	}

	private void OnAddNewGrade()
	{
		UIManager.Popup.TextInput.Show(AddRole, T._("추가할 등급의 이름을 적으세요"));
	}
}
