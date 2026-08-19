using System;
using System.Collections.Generic;
using Durango.Logic.Encyclopedia;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class EncyclopediaSubMemoList : MonoBehaviour
{
	[SerializeField]
	private KScrollView _scrollList;

	private bool _isInit;

	private MemoType _type;

	public event Action<MemoType, Submemo> SubMemoClicked;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			ListObjectPool nodes = _scrollList.Nodes;
			nodes.Init(OnInitListNode);
		}
	}

	private void UpdateSubMemos(MemoType type, ListObjectPool nodes)
	{
		if (!_isInit)
		{
			return;
		}
		List<Submemo> subMemos = GameSystem<MemoSystem>.Instance().GetSubMemos(type);
		if (subMemos != null)
		{
			int num = Mathf.Min(subMemos.Count, nodes.Count);
			for (int i = 0; i < num; i++)
			{
				nodes.Get<EncyclopediaSubMemoNode>(i).Set(type, subMemos[i]);
			}
		}
	}

	private void Awake()
	{
		GameSystem<MemoSystem>.Instance().MemoCollected += MemoCollected;
	}

	private void OnDisable()
	{
		GameSystem<MemoSystem>.Instance().MemoCollected -= MemoCollected;
	}

	private void MemoCollected(MemoType type, int memoId)
	{
		if (_isInit)
		{
			UpdateSubMemos(type, _scrollList.Nodes);
		}
	}

	private void OnInitListNode(GameObject obj)
	{
		EncyclopediaSubMemoNode component = obj.GetComponent<EncyclopediaSubMemoNode>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickNode));
	}

	private void OnClickNode()
	{
		EncyclopediaSubMemoNode encyclopediaSubMemoNode = Selectable.Current as EncyclopediaSubMemoNode;
		if (!(encyclopediaSubMemoNode == null) && this.SubMemoClicked != null)
		{
			this.SubMemoClicked(encyclopediaSubMemoNode.MemoType, encyclopediaSubMemoNode.Memo);
		}
	}

	public void Show(MemoType type, int initIndex = -1)
	{
		Init();
		base.gameObject.SetActive(value: true);
		List<Submemo> subMemos = GameSystem<MemoSystem>.Instance().GetSubMemos(type);
		if (subMemos == null)
		{
			return;
		}
		bool resetPosition = _type != type;
		_type = type;
		ListObjectPool nodes = _scrollList.Nodes;
		nodes.BeginLoad();
		int num = -1;
		for (int i = 0; i < subMemos.Count; i++)
		{
			if (initIndex == i)
			{
				num = nodes.Count;
			}
			EncyclopediaSubMemoNode component = nodes.GetNext().GetComponent<EncyclopediaSubMemoNode>();
			component.Set(type, subMemos[i]);
		}
		nodes.EndLoad();
		_scrollList.Reposition(resetPosition, tween: false);
		if (num != -1)
		{
			_scrollList.MoveToNode(num, instant: true);
		}
		int num2 = ((initIndex != -1) ? GameSystem<MemoSystem>.Instance().SubMemoIndexOf(type, initIndex) : (-1));
		_scrollList.MoveToNode((num2 != -1) ? num2 : 0, instant: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
