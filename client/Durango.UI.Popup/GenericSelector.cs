using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class GenericSelector : TooltipBase
{
	private const string BlurKey = "GenericSelector";

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _infoWidget;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private RectLayout _layout;

	private KInfiniteScrollView.View<string, GenericSelectorItem> _view;

	private readonly List<int> _selected = new List<int>();

	private readonly List<int> _defaultSelected = new List<int>();

	private bool _isBlur;

	private string _titleText;

	private string _infoText;

	private string _confirmText;

	private readonly List<string> _itemList = new List<string>();

	private int _selectableCount;

	private Action<int> _onSelected;

	private Action<int[]> _onMultiSelected;

	protected override void OnAwake()
	{
		base.OnAwake();
		_view = _scrollView.Initialize(delegate(GenericSelectorItem comp, string text)
		{
			comp.Set(text);
			comp.Select(_selected.Contains(_view.CurrentIndex));
		}, delegate(GenericSelectorItem comp)
		{
			comp.Clicked += OnClickItemNode;
		});
		_view.SetList(_itemList);
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnConfirmed));
		ResetArguments();
	}

	private void OnClickItemNode(GenericSelectorItem comp)
	{
		int item = _view.IndexOf(comp);
		int num = _selected.IndexOf(item);
		if (num == -1)
		{
			_selected.Add(item);
		}
		else
		{
			_selected.RemoveAt(num);
		}
		RefreshSelectedView();
	}

	private void OnConfirmed()
	{
		if (_selected.Count == 0)
		{
			return;
		}
		if (_selected.SequenceEqual(_defaultSelected))
		{
			Hide();
			return;
		}
		if (_onSelected != null)
		{
			_onSelected(_selected.First());
		}
		if (_onMultiSelected != null)
		{
			_onMultiSelected(_selected.ToArray());
		}
		Hide();
	}

	public void ResetArguments()
	{
		_selected.Clear();
		_defaultSelected.Clear();
		_isBlur = false;
		_titleText = null;
		_infoText = null;
		_selectableCount = 1;
		_confirmText = null;
		_itemList.Clear();
		_onSelected = null;
		_onMultiSelected = null;
	}

	public void SetTitle(string title)
	{
		_titleText = title;
	}

	public void SetInfo(string info)
	{
		_infoText = info;
	}

	public void AddItem(string text)
	{
		_itemList.Add(text);
	}

	public void DefaultSelectedIndex(int index)
	{
		if (!_defaultSelected.Contains(index))
		{
			_defaultSelected.Add(index);
		}
	}

	public void BlurOn()
	{
		_isBlur = true;
	}

	public void SetConfirmText(string text)
	{
		_confirmText = text;
	}

	public void SetSelected(Action<int> onSelected)
	{
		_onSelected = onSelected;
	}

	public void SetSelected(Action<int[]> onMultiSelected)
	{
		_onMultiSelected = onMultiSelected;
	}

	public void SetSelectableCount(int selectableCount)
	{
		_selectableCount = Mathf.Max(1, selectableCount);
	}

	protected override void FillData()
	{
		_confirmButton.Text = ((!string.IsNullOrEmpty(_confirmText)) ? _confirmText : T._("확인"));
		if (string.IsNullOrEmpty(_titleText))
		{
			_titleWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_titleWidget.gameObject.SetActive(value: true);
			_titleLabel.text = _titleText;
		}
		if (string.IsNullOrEmpty(_infoText))
		{
			_infoWidget.gameObject.SetActive(value: false);
			return;
		}
		_infoWidget.gameObject.SetActive(value: true);
		_infoLabel.text = _infoText;
		_infoWidget.height = (int)_infoLabel.printedSize.y + 40;
	}

	private void RefreshSelectedView()
	{
		int num = _selected.Count - _selectableCount;
		if (num > 0)
		{
			_selected.RemoveRange(0, num);
		}
		int num2 = 0;
		foreach (GenericSelectorItem item in _view.List)
		{
			item.Select(_selected.Contains(_view.Begin + num2));
			num2++;
		}
	}

	protected override void UpdateLayout()
	{
		base.transform.localPosition = Vector3.zero;
		int safeHeight = UIManager.SafeHeight;
		_layout.UpdateLayout(null, Mathf.Min((int)((float)safeHeight * 0.8f), safeHeight - 160));
		UIUtility.UpdateAnchors(base.transform);
		_view.Redraw();
	}

	protected override void OnShow()
	{
		base.OnShow();
		if (_isBlur)
		{
			BlurController.BlurOn("GenericSelector", BlurController.Mask.UI);
		}
		_selected.Clear();
		_scrollView.ResetPosition();
		if (_defaultSelected.Count > 0)
		{
			int num = _defaultSelected.Count - _selectableCount;
			if (num > 0)
			{
				_defaultSelected.RemoveRange(0, num);
			}
			_selected.AddRange(_defaultSelected);
			RefreshSelectedView();
			_scrollView.MoveToVisibleArea(_selected.FirstOrDefault(), instant: true);
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		BlurController.BlurOff("GenericSelector");
		ResetArguments();
	}

	protected override void OnTryConfirmOnModal()
	{
		OnConfirmed();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}
}
