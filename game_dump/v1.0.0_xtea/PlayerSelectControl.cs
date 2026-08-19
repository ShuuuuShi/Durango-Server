using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class PlayerSelectControl : TooltipBase
{
	[SerializeField]
	private ListObjectPool _nodes;

	private IList<PlayerInfo> _players;

	private Action<PlayerInfo> _selectedFunc;

	protected override void OnAwake()
	{
		_nodes.Init(NodeWidgetInitialize);
	}

	private void NodeWidgetInitialize(GameObject obj)
	{
		SelectablePlayerWidget component = obj.GetComponent<SelectablePlayerWidget>();
		component.PlayerSelected = PlayerSelected;
		UIEventListener.Get(((Component)component).gameObject).onDrag = delegate(GameObject go, Vector2 delta)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			OnDrag(delta);
		};
	}

	private void PlayerSelected(PlayerInfo player)
	{
		bool flag = false;
		int i = 0;
		for (int count = _nodes.Count; i < count; i++)
		{
			SelectablePlayerWidget component = _nodes[i].GetComponent<SelectablePlayerWidget>();
			bool isSelect = component.IsSelect;
			bool flag2 = _players[i].EntityId == player.EntityId;
			component.Select(flag2);
			if (isSelect && flag2)
			{
				flag = true;
			}
		}
		if (flag)
		{
			Hide();
		}
		else if (_selectedFunc != null)
		{
			_selectedFunc(player);
		}
	}

	public void Set(IList<PlayerInfo> players, Action<PlayerInfo> callback)
	{
		_players = players;
		_selectedFunc = callback;
	}

	protected override void FillData()
	{
		int num = ((_players != null) ? _players.Count : 0);
		_nodes.Clear();
		for (int i = 0; i < num; i++)
		{
			PlayerInfo playerInfo = _players[i];
			if (playerInfo != null)
			{
				SelectablePlayerWidget selectablePlayerWidget = ((ListObjectPoolBase<GameObject>)_nodes).Add<SelectablePlayerWidget>();
				selectablePlayerWidget.Set(playerInfo);
				selectablePlayerWidget.Select(i == 0);
			}
		}
	}

	protected override void UpdateLayout()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = _nodes.BaseObject.transform.localPosition;
		int num = 0;
		int i = 0;
		for (int count = _nodes.Count; i < count; i++)
		{
			SelectablePlayerWidget component = _nodes[i].GetComponent<SelectablePlayerWidget>();
			((Component)component).transform.localPosition = localPosition + Vector3.down * (float)num;
			num += component.GetHeight();
		}
		base.Widget.height = num;
	}

	protected override void OnFinish()
	{
		base.OnFinish();
		int num = -1;
		int i = 0;
		for (int count = _nodes.Count; i < count; i++)
		{
			SelectablePlayerWidget component = _nodes[i].GetComponent<SelectablePlayerWidget>();
			if (component.IsSelect)
			{
				num = i;
				break;
			}
		}
		if (num != -1 && _selectedFunc != null)
		{
			_selectedFunc(_players[num]);
		}
	}
}
