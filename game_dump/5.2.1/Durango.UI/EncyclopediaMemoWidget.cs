using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Logic.Encyclopedia;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class EncyclopediaMemoWidget : MonoBehaviour, IScreenResizeReceiver
{
	[SerializeField]
	private UILabel _titleLabel;

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

	private int _currentIndex;

	private readonly List<int> _memoList = new List<int>();

	private bool _isInit;

	public bool IsOpen { get; private set; }

	public MemoType MemoType { get; private set; }

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
		}
	}

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		_memoLabel.pivot = ((!UIManager.IsPortraitWidget(base.gameObject)) ? UIWidget.Pivot.Left : UIWidget.Pivot.TopLeft);
	}

	public void ShowMemos(MemoType type, int index = -1)
	{
		Init();
		base.gameObject.SetActive(value: true);
		IsOpen = true;
		_titleLabel.text = type.GetName();
		MemoType = type;
		_memoList.Clear();
		BitArray activeMemoFlags = GameSystem<MemoSystem>.Instance().GetActiveMemoFlags(type);
		int num = -1;
		int i = 0;
		for (int length = activeMemoFlags.Length; i < length; i++)
		{
			if (activeMemoFlags[i])
			{
				if (i == index)
				{
					num = _memoList.Count;
				}
				_memoList.Add(i);
			}
		}
		if (num == -1)
		{
			num = 0;
		}
		ShowMemos(num);
	}

	public void Hide()
	{
		IsOpen = false;
		base.gameObject.SetActive(value: false);
	}

	private void ShowMemos(int index)
	{
		if (_memoList.Count == 0)
		{
			MemoType = MemoType.Fiction;
			_currentIndex = 0;
			base.gameObject.SetActive(value: false);
			return;
		}
		index = Mathf.Clamp(index, 0, _memoList.Count - 1);
		_currentIndex = index;
		_indexLabel.text = $"#{_memoList[index]}";
		_memoLabel.text = MemoSystem.GetMemoText(MemoType, _memoList[index]);
		_pageLabel.text = $"<em>{index + 1}</em> [555555]/[-] {_memoList.Count}";
		UpdateButtonVisibleState();
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void UpdateButtonVisibleState()
	{
		int num = ((_memoList != null) ? _memoList.Count : 0);
		_nextButton.gameObject.SetActive(_currentIndex < num - 1);
		_prevButton.gameObject.SetActive(_currentIndex > 0);
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
