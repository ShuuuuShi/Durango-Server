using System;
using System.Collections.Generic;
using EncyclopediaData;
using UnityEngine;

public class EncyclopediaMemoList : MonoBehaviour
{
	public Action<MemoType, int> MemoSelected;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private UILabel _memoCountLabel;

	[SerializeField]
	private KGridScrollView _memoList;

	[SerializeField]
	private GameObject _noData;

	private string _memoTitleFormat;

	private string _memoCountFormat;

	private AnimationWidget _widget;

	private MemoType _type;

	private bool _isInit;

	public AnimationWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _widget;
		}
	}

	public bool IsOpen { get; private set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_memoList.Nodes.Init(InitMemoItem);
			_memoTitleFormat = _titleLabel.text;
			_memoCountFormat = _memoCountLabel.text;
		}
	}

	private void InitMemoItem(GameObject obj)
	{
		Selectable component = obj.GetComponent<Selectable>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnSelectMemoItem));
	}

	public void ShowMemos(MemoType type, int initIndex = -1)
	{
		Init();
		((Component)this).gameObject.SetActive(true);
		IsOpen = true;
		Widget.Delay = Widget.Duration * 0.5f;
		Widget.Alpha = 1f;
		bool resetPosition = _type != type;
		_type = type;
		List<int> availableMemoList = GameSystem<EncyclopediaSystem>.Instance().GetAvailableMemoList(type);
		ListObjectPool nodes = _memoList.Nodes;
		nodes.Clear();
		for (int i = 0; i < availableMemoList.Count; i++)
		{
			EncyclopediaMemoItem encyclopediaMemoItem = ((ListObjectPoolBase<GameObject>)nodes).Add<EncyclopediaMemoItem>();
			encyclopediaMemoItem.Set(availableMemoList[i]);
		}
		_memoList.Reposition(resetPosition, tween: false);
		if (initIndex != -1)
		{
			int num = availableMemoList.IndexOf(initIndex);
			if (num != -1)
			{
				_memoList.MoveToNode(num, instant: true);
			}
		}
		_noData.gameObject.SetActive(nodes.Count == 0);
		_titleLabel.text = string.Format(_memoTitleFormat, LocalizeUtil.Get(type));
		_memoCountLabel.text = string.Format(_memoCountFormat, availableMemoList.Count);
	}

	public void Hide()
	{
		IsOpen = false;
		Widget.Delay = 0f;
		Widget.Alpha = 0f;
	}

	private void OnSelectMemoItem()
	{
		EncyclopediaMemoItem encyclopediaMemoItem = Selectable.Current as EncyclopediaMemoItem;
		if (!((Object)(object)encyclopediaMemoItem == (Object)null) && !encyclopediaMemoItem.Disable && MemoSelected != null)
		{
			MemoSelected(_type, encyclopediaMemoItem.Index);
		}
	}
}
