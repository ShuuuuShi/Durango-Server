using System;
using System.Collections.Generic;
using Durango.Logic.Clusters;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PlayerSlotList : UIWidget, IUIInitializable
{
	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private SelectableButton _button;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private GameObject _countIcon;

	[SerializeField]
	private GameObject _exceededIcon;

	public event Action<PlayerSlotNode.SlotType, PlayerInfo> SlotSelected;

	public event Action<PlayerSlotNode.SlotType, string> ButtonClicked;

	void IUIInitializable.Init()
	{
		_scrollView.Nodes.Init(delegate(GameObject go)
		{
			PlayerSlotNode component = go.GetComponent<PlayerSlotNode>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickSlotNode));
		});
		SelectableButton button = _button;
		button.Clicked = (Action)Delegate.Combine(button.Clicked, (Action)delegate
		{
			PlayerSlotNode selectedNode = GetSelectedNode();
			if (!(selectedNode == null) && this.ButtonClicked != null)
			{
				this.ButtonClicked(selectedNode.Type, selectedNode.PlayerEntityId);
			}
		});
	}

	public void Set(List<PlayerInfo> players, int emptyCount, int playerSlotCount, int lockedCount, bool exceeded)
	{
		ListObjectPool nodes = _scrollView.Nodes;
		int num = 0;
		int size = KUtility.GetSize(players);
		for (int i = 0; i < size; i++)
		{
			PlayerSlotNode component = nodes.GetOrAdd(num).GetComponent<PlayerSlotNode>();
			component.Set(PlayerSlotNode.SlotType.HasPlayer, players[num]);
			num++;
		}
		for (int j = 0; j < emptyCount; j++)
		{
			PlayerSlotNode component2 = nodes.GetOrAdd(num).GetComponent<PlayerSlotNode>();
			component2.Set(PlayerSlotNode.SlotType.Empty, null);
			num++;
		}
		for (int k = 0; k < lockedCount; k++)
		{
			PlayerSlotNode component3 = nodes.GetOrAdd(num).GetComponent<PlayerSlotNode>();
			component3.Set(PlayerSlotNode.SlotType.Locked, null);
			num++;
		}
		nodes.Set(num);
		_scrollView.Reposition(resetPosition: true, tween: false);
		_countLabel.text = string.Format("{0} / {1}", (!exceeded) ? size.ToString() : size.ToString().ToEncodedColor("BA2E2DFF"), playerSlotCount.ToString().ToEncodedColor("D4CEBEFF"));
		_countIcon.transform.localPosition = new Vector3(0f - _countLabel.printedSize.x - 12f, 0f, 0f);
		_exceededIcon.SetActive(exceeded);
	}

	public PlayerSlotNode GetSelectedNode()
	{
		foreach (GameObject node in _scrollView.Nodes)
		{
			PlayerSlotNode component = node.GetComponent<PlayerSlotNode>();
			if (component.Selected)
			{
				return component;
			}
		}
		return null;
	}

	public void Select(string entityId)
	{
		for (int i = 0; i < _scrollView.Nodes.Count; i++)
		{
			GameObject gameObject = _scrollView.Nodes[i];
			PlayerSlotNode component = gameObject.GetComponent<PlayerSlotNode>();
			if (component.Selected = component.Type == PlayerSlotNode.SlotType.HasPlayer && component.PlayerEntityId == entityId)
			{
				OnSlotNodeSelected(component);
				_scrollView.MoveToVisibleArea(i, instant: true);
			}
		}
	}

	private void Select(PlayerSlotNode node)
	{
		foreach (GameObject node2 in _scrollView.Nodes)
		{
			PlayerSlotNode component = node2.GetComponent<PlayerSlotNode>();
			if (component.Selected = component == node)
			{
				OnSlotNodeSelected(component);
			}
		}
	}

	private void OnSlotNodeSelected([NotNull] PlayerSlotNode node)
	{
		PlayerSlotNode.SlotType type = node.Type;
		bool flag = node.PlayerEntityId == GameManager.PlayerId;
		switch (type)
		{
		case PlayerSlotNode.SlotType.HasPlayer:
			_button.Disabled = flag;
			break;
		case PlayerSlotNode.SlotType.Locked:
			_button.Disabled = true;
			break;
		default:
			_button.Disabled = false;
			break;
		}
		switch (type)
		{
		case PlayerSlotNode.SlotType.HasPlayer:
			_button.Text = ((!flag) ? T._("접속") : T._("접속 중"));
			break;
		case PlayerSlotNode.SlotType.Empty:
			_button.Text = T._("캐릭터 생성");
			break;
		case PlayerSlotNode.SlotType.Locked:
			_button.Text = T._("캐릭터 슬롯 구매");
			break;
		}
		if (this.SlotSelected != null)
		{
			this.SlotSelected(type, node.PlayerInfo);
		}
	}

	private void OnClickSlotNode()
	{
		PlayerSlotNode playerSlotNode = Selectable.Current as PlayerSlotNode;
		if (!(playerSlotNode == null))
		{
			Select(playerSlotNode);
		}
	}
}
