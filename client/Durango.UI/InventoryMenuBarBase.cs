using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class InventoryMenuBarBase : MonoBehaviour
{
	[SerializeField]
	private SelectableButton _sortButton;

	[SerializeField]
	protected SelectableButton _removeButton;

	[SerializeField]
	protected SelectableButton _lockButton;

	[SerializeField]
	private SelectableButton _selectAllButton;

	[SerializeField]
	protected SelectableButton _filterButton;

	private StringSelector _stringSelector;

	private List<Util.SortOption> _sortOptionList = new List<Util.SortOption>();

	private List<string> _sortOptionNames = new List<string>();

	private Util.SortOption _currentSortOption;

	private bool _descending;

	private float _sortLockTime;

	public bool IsReversedSort => _descending;

	public Util.SortOption SortOption => _currentSortOption;

	public event Action<Util.SortOption, bool> OnSort;

	public event Action OnRemove;

	public event Action OnLock;

	public event Action OnSelectAll;

	public event Action OnFilter;

	private void Start()
	{
		_sortButton.Init();
		_sortButton.Clicked = delegate
		{
			SortSelected(_currentSortOption);
		};
		_sortButton.SubClicked = delegate
		{
			ShowSortPopupList(_stringSelector == null);
		};
		SelectableButton removeButton = _removeButton;
		removeButton.Clicked = (Action)Delegate.Combine(removeButton.Clicked, new Action(OnClickRemoveButton));
		SelectableButton lockButton = _lockButton;
		lockButton.Clicked = (Action)Delegate.Combine(lockButton.Clicked, new Action(OnClickLockButton));
		SelectableButton selectAllButton = _selectAllButton;
		selectAllButton.Clicked = (Action)Delegate.Combine(selectAllButton.Clicked, new Action(OnClickSelectAll));
		SelectableButton filterButton = _filterButton;
		filterButton.Clicked = (Action)Delegate.Combine(filterButton.Clicked, new Action(OnFilterButton));
		Array values = Enum.GetValues(typeof(Util.SortOption));
		for (int i = 0; i < values.Length; i++)
		{
			Util.SortOption sortOption = (Util.SortOption)values.GetValue(i);
			_sortOptionList.Add(sortOption);
			_sortOptionNames.Add(sortOption.GetName());
		}
		_currentSortOption = Util.SortOption.Default;
		SetSortButtonText(_currentSortOption);
		ShowSortPopupList(show: false);
	}

	public void ItemRemoveEnable(bool on)
	{
		_removeButton.Disabled = !on;
	}

	public void ItemLockEnable(bool on)
	{
		_lockButton.Disabled = !on;
	}

	public void ItemFilterSelection(bool on)
	{
		_filterButton.Selected = on;
	}

	public void SetLockButtonSelection(bool on)
	{
		_lockButton.Selected = on;
	}

	protected void OnClickRemoveButton()
	{
		if (this.OnRemove != null)
		{
			this.OnRemove();
		}
	}

	protected void OnClickLockButton()
	{
		if (this.OnLock != null)
		{
			this.OnLock();
		}
	}

	private void OnClickSelectAll()
	{
		if (this.OnSelectAll != null)
		{
			this.OnSelectAll();
		}
	}

	protected void OnFilterButton()
	{
		if (this.OnFilter != null)
		{
			this.OnFilter();
		}
	}

	private void ShowSortPopupList(bool show)
	{
		if (show)
		{
			StringSelector stringSelector = UIManager.Popup.Tooltip<StringSelector>();
			stringSelector.Set(_sortOptionNames, OnSelectSortOption);
			stringSelector.MinWidth = _sortButton.Widget.width;
			stringSelector.AutoPosition = false;
			stringSelector.AddOnFinished(SelectorHided);
			stringSelector.Show();
			Vector3 pos = stringSelector.transform.parent.InverseTransformPoint(_sortButton.transform.TransformPoint(_sortButton.Widget.localCorners[1]));
			stringSelector.Widget.SetPosition(pos, 0f, 0f);
			_stringSelector = stringSelector;
		}
		else if (_stringSelector != null)
		{
			_stringSelector.Hide();
			_stringSelector = null;
		}
	}

	private void SelectorHided()
	{
		_stringSelector = null;
		ShowSortPopupList(show: false);
	}

	private void OnSelectSortOption(int index)
	{
		Util.SortOption op = _sortOptionList[index];
		SortSelected(op);
	}

	private void SortSelected(Util.SortOption op)
	{
		if (_currentSortOption != op)
		{
			SetSortButtonText(op);
			_descending = false;
		}
		_currentSortOption = op;
		if (!(Time.time < _sortLockTime))
		{
			_sortLockTime = Time.time + 0.5f;
			if (this.OnSort != null)
			{
				this.OnSort(op, _descending);
			}
			_descending = !_descending;
		}
	}

	private void SetSortButtonText(Util.SortOption op)
	{
		int index = -1;
		for (int i = 0; i < _sortOptionList.Count; i++)
		{
			if (_sortOptionList[i] == op)
			{
				index = i;
				break;
			}
		}
		_sortButton.Text = _sortOptionNames[index];
	}

	public void SetSelectAllButtonActive(bool activated)
	{
		_selectAllButton.gameObject.SetActive(activated);
	}

	public void SetSelectAllButtonSelected(bool pressed)
	{
		_selectAllButton.Selected = pressed;
	}
}
