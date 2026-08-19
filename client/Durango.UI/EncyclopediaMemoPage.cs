using System;
using System.Collections.Generic;
using Durango.Logic.Encyclopedia;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class EncyclopediaMemoPage : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private NestedPrefabLinker _tabList;

	[SerializeField]
	private EncyclopediaMemoList _memoList;

	[SerializeField]
	private EncyclopediaMemoWidget _memoViewer;

	[SerializeField]
	private EncyclopediaSubMemoList _subMemoList;

	[SerializeField]
	private EncyclopediaSubMemoViewer _subMemoViewer;

	private HorizontalTabList _tabs;

	private readonly List<MemoType> _tabTypes = new List<MemoType>();

	private bool _isInitTab;

	public event Action<bool> OnShowMemo;

	void IUIInitializable.Init()
	{
		EncyclopediaMemoList memoList = _memoList;
		memoList.MemoSelected = (Action<MemoType, int>)Delegate.Combine(memoList.MemoSelected, new Action<MemoType, int>(ShowMemo));
		_subMemoList.SubMemoClicked += OnClickSubMemo;
		_tabs = _tabList.Object.GetComponent<HorizontalTabList>();
	}

	public bool Close()
	{
		if (_memoViewer.IsOpen)
		{
			ShowMemoList(_memoViewer.MemoType, _memoViewer.Index);
			return false;
		}
		if (_subMemoViewer.IsOpen)
		{
			ShowMemoList(_subMemoViewer.MemoType);
			return false;
		}
		return true;
	}

	private void InitTab()
	{
		if (_isInitTab)
		{
			return;
		}
		_isInitTab = true;
		ListObjectPool nodes = _tabs.ScrollView.Nodes;
		nodes.Clear();
		_tabs.BeginLoad();
		MemoType[] array = Enums<MemoType>.All();
		_tabTypes.Clear();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			MemoType memoType = array[i];
			if (memoType != MemoType.Invalid)
			{
				string text = memoType.GetName();
				_tabs.AddText(text);
				_tabTypes.Add(memoType);
			}
		}
		_tabs.EndLoadByFixedSize();
		_tabs.Clicked += OnSelectTab;
	}

	private void OnSelectTab(int index)
	{
		ShowMemoList(_tabTypes[index]);
	}

	private void OnClickSubMemo(MemoType type, Submemo memo)
	{
		ShowSubMemo(type, memo, -1);
	}

	private void ShowSubMemo(MemoType type, Submemo memo, int index)
	{
		_subMemoViewer.Show(type, memo, index);
		_memoViewer.Hide();
		_memoList.Hide();
		_subMemoList.Hide();
		SelectTab(type);
		if (this.OnShowMemo != null)
		{
			this.OnShowMemo(obj: true);
		}
	}

	public void ShowMemo(MemoType type, int memoId)
	{
		InitTab();
		if (MemoSystem.IsServerMemo(type))
		{
			int num = GameSystem<MemoSystem>.Instance().SubMemoIndexOf(type, memoId);
			if (num == -1)
			{
				ShowMemoList(type);
				return;
			}
			List<Submemo> subMemos = GameSystem<MemoSystem>.Instance().GetSubMemos(type);
			if (subMemos != null && num < subMemos.Count)
			{
				ShowSubMemo(type, subMemos[num], memoId);
			}
			return;
		}
		_memoViewer.ShowMemos(type, memoId);
		_memoList.Hide();
		_subMemoList.Hide();
		_subMemoViewer.Hide();
		SelectTab(type);
		if (this.OnShowMemo != null)
		{
			this.OnShowMemo(obj: true);
		}
	}

	public void ShowMemoList(MemoType type, int initIndex = -1)
	{
		InitTab();
		if (MemoSystem.IsServerMemo(type))
		{
			_subMemoList.Show(type, initIndex);
			_memoList.Hide();
			_memoViewer.Hide();
			_subMemoViewer.Hide();
		}
		else
		{
			_memoList.ShowAvailableMemoes(type, initIndex);
			_memoViewer.Hide();
			_subMemoList.Hide();
			_subMemoViewer.Hide();
		}
		SelectTab(type);
		if (this.OnShowMemo != null)
		{
			this.OnShowMemo(obj: false);
		}
	}

	private void SelectTab(MemoType type)
	{
		int index = _tabTypes.IndexOf(type);
		_tabs.Select(index);
	}
}
