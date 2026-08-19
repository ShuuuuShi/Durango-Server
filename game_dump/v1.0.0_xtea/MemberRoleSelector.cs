using System;
using System.Collections.Generic;
using ClanData;
using Messages;
using UnityEngine;

public class MemberRoleSelector : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private KScrollView _targetButtons;

	[SerializeField]
	private Selectable _okButotn;

	[SerializeField]
	private Selectable _cancelButton;

	private List<MemberRole> _roleList = new List<MemberRole>();

	private string _title;

	private string _comment;

	private Action<int> _onResult;

	private int _selectedIndex = -1;

	protected override void OnAwake()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Selectable okButotn = _okButotn;
		okButotn.Clicked = (Action)Delegate.Combine(okButotn.Clicked, new Action(OnOk));
		Selectable cancelButton = _cancelButton;
		cancelButton.Clicked = (Action)Delegate.Combine(cancelButton.Clicked, new Action(OnCancel));
		_targetButtons.Nodes.Init(OnInitRoleButtons);
		((Component)this).transform.localPosition = Vector3.zero;
	}

	protected override void FillData()
	{
		_titleLabel.text = _title;
		_commentLabel.text = _comment;
		ListObjectPool nodes = _targetButtons.Nodes;
		nodes.Set(_roleList.Count);
		for (int i = 0; i < nodes.Count; i++)
		{
			DefaultSelectableButton component = nodes[i].GetComponent<DefaultSelectableButton>();
			component.Text = _roleList[i].Name;
		}
		_targetButtons.ResetPosition();
		SelectRole(_selectedIndex);
	}

	protected override void UpdateLayout()
	{
		((Component)this).GetComponent<WidgetLayoutController>().UpdateLayout();
	}

	protected override void OnShow()
	{
		base.OnShow();
		if (_roleList.Count == 0)
		{
			Hide();
		}
	}

	private void OnInitRoleButtons(GameObject obj)
	{
		DefaultSelectableButton component = obj.GetComponent<DefaultSelectableButton>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickRoleButton));
	}

	private void OnClickRoleButton()
	{
		int index = _targetButtons.Nodes.IndexOf(((Component)Selectable.Current).gameObject);
		SelectRole(index);
	}

	private void SelectRole(int index)
	{
		for (int i = 0; i < _targetButtons.Nodes.Count; i++)
		{
			DefaultSelectableButton component = _targetButtons.Nodes[i].GetComponent<DefaultSelectableButton>();
			component.Select = i == index;
		}
		_selectedIndex = index;
	}

	public void Set(Clan clan, Func<MemberRole, MemberRole?, bool> filter, string title, string comment, Action<int> onResult)
	{
		ClanData.Member member = clan.GetMember(GameManager.PlayerId);
		MemberRole? arg = null;
		if (member != null && clan.TryGetRole(member.RoleId, out var role))
		{
			arg = role;
		}
		_roleList.Clear();
		foreach (KeyValuePair<int, MemberRole> roleInfo in clan.RoleInfos)
		{
			if (filter == null || filter(roleInfo.Value, arg))
			{
				_roleList.Add(roleInfo.Value);
			}
		}
		_roleList.Sort((MemberRole r1, MemberRole r2) => r1.Grade - r2.Grade);
		_onResult = onResult;
		_title = title;
		_comment = comment;
		_selectedIndex = -1;
	}

	private void OnOk()
	{
		Hide();
		if (_onResult != null)
		{
			_onResult((_selectedIndex != -1) ? _roleList[_selectedIndex].Id : (-1));
		}
	}

	private void OnCancel()
	{
		Hide();
		if (_onResult != null)
		{
			_onResult(-1);
		}
	}
}
