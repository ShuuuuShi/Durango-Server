using System;
using System.Collections.Generic;
using EncyclopediaData;
using UnityEngine;

public class EncyclopediaMemoWidget : MonoBehaviour
{
	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private UILabel _memoLabel;

	[SerializeField]
	private UILabel _pageLabel;

	[SerializeField]
	private UILabel _indexLabel;

	[SerializeField]
	private SelectableWidget _nextButton;

	[SerializeField]
	private SelectableWidget _prevButton;

	private AnimationWidget _widget;

	private string _titleFormat;

	private string _memoPageFormat;

	private MemoType _memoType;

	private int _currentIndex;

	private List<int> _memoList;

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

	public MemoType MemoType => _memoType;

	public int Index => _memoList[_currentIndex];

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			SelectableWidget nextButton = _nextButton;
			nextButton.Clicked = (Action)Delegate.Combine(nextButton.Clicked, new Action(NextMemo));
			SelectableWidget prevButton = _prevButton;
			prevButton.Clicked = (Action)Delegate.Combine(prevButton.Clicked, new Action(PrevMemo));
			_titleFormat = _titleLabel.text;
			_memoPageFormat = _pageLabel.text;
		}
	}

	public void ShowMemos(MemoType type, int index = -1)
	{
		Init();
		((Component)this).gameObject.SetActive(true);
		Widget.Delay = Widget.Duration * 0.5f;
		Widget.Alpha = 1f;
		IsOpen = true;
		List<int> availableMemoList = GameSystem<EncyclopediaSystem>.Instance().GetAvailableMemoList(type);
		_titleLabel.text = string.Format(_titleFormat, LocalizeUtil.Get(type));
		_memoType = type;
		_memoList = availableMemoList;
		if (index != -1)
		{
			index = availableMemoList.IndexOf(index);
		}
		if (index == -1)
		{
			index = 0;
		}
		ShowMemos(index);
	}

	public void Hide()
	{
		IsOpen = false;
		Widget.Delay = 0f;
		Widget.Alpha = 0f;
	}

	private void ShowMemos(int index)
	{
		if (_memoList == null || _memoList.Count == 0)
		{
			_memoType = MemoType.Fiction;
			_currentIndex = 0;
			_memoList = null;
			((Component)this).gameObject.SetActive(false);
		}
		else
		{
			index = Mathf.Clamp(index, 0, _memoList.Count - 1);
			_currentIndex = index;
			_indexLabel.text = $"#{_memoList[index]}";
			_memoLabel.text = EncyclopediaSystem.GetMemoText(_memoType, _memoList[index]);
			_pageLabel.text = string.Format(_memoPageFormat, index + 1, _memoList.Count);
			UpdateButtonVisibleState();
		}
	}

	private void UpdateButtonVisibleState()
	{
		int num = ((_memoList != null) ? _memoList.Count : 0);
		((Component)_nextButton).gameObject.SetActive(_currentIndex < num - 1);
		((Component)_prevButton).gameObject.SetActive(_currentIndex > 0);
	}

	private void NextMemo()
	{
		ShowMemos(++_currentIndex);
	}

	private void PrevMemo()
	{
		ShowMemos(--_currentIndex);
	}
}
