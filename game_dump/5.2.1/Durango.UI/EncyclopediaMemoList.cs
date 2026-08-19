using System;
using System.Collections;
using Durango.Logic.Encyclopedia;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class EncyclopediaMemoList : MonoBehaviour
{
	public Action<MemoType, int> MemoSelected;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _memoCountLabel;

	[SerializeField]
	private KGridScrollView _memoList;

	[SerializeField]
	private GameObject _noData;

	private MemoType _type;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_memoList.Nodes.Init(InitMemoItem);
		}
	}

	private void InitMemoItem(GameObject obj)
	{
		Selectable component = obj.GetComponent<Selectable>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnSelectMemoItem));
	}

	public void ShowAvailableMemoes(MemoType type, int initIndex = -1)
	{
		Init();
		base.gameObject.SetActive(value: true);
		bool resetPosition = _type != type;
		_type = type;
		BitArray activeMemoFlags = GameSystem<MemoSystem>.Instance().GetActiveMemoFlags(type);
		ListObjectPool nodes = _memoList.Nodes;
		nodes.Clear();
		int num = -1;
		for (int i = 0; i < activeMemoFlags.Length; i++)
		{
			if (activeMemoFlags[i])
			{
				if (initIndex == i)
				{
					num = nodes.Count;
				}
				nodes.Add<EncyclopediaMemoItem>().Set(i);
			}
		}
		_memoList.Reposition(resetPosition, tween: false);
		if (num != -1)
		{
			_memoList.MoveToNode(num, instant: true);
		}
		_noData.gameObject.SetActive(nodes.Count == 0);
		_titleLabel.text = type.GetName();
		_memoCountLabel.text = T._("<em>{0}</em> 개", nodes.Count);
		GetComponent<RectLayoutComponent>().UpdateLayout();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnSelectMemoItem()
	{
		EncyclopediaMemoItem encyclopediaMemoItem = Selectable.Current as EncyclopediaMemoItem;
		if (!(encyclopediaMemoItem == null) && MemoSelected != null)
		{
			MemoSelected(_type, encyclopediaMemoItem.Index);
		}
	}
}
