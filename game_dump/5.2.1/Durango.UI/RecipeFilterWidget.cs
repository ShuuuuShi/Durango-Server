using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class RecipeFilterWidget : MonoBehaviour, IUICursorChangable
{
	[SerializeField]
	private UIInput _inputFilter;

	[SerializeField]
	private GameObject _clearButton;

	[CanBeNull]
	[SerializeField]
	private GameObject _focusingEffect;

	private bool _isInit;

	private readonly List<KeyValuePair<string, RecipeListWidget.SubList>> _recipeSubLists = new List<KeyValuePair<string, RecipeListWidget.SubList>>();

	public string SearchText
	{
		get
		{
			return _inputFilter.value;
		}
		set
		{
			_inputFilter.value = value;
			RefreshClearButton();
		}
	}

	public event Action SearchTextSubmitted;

	public void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			EventDelegate.Add(_inputFilter.onSubmit, InputFilter_Submitted);
			EventDelegate.Add(_inputFilter.onChange, InputFilter_Changed);
			UIEventListener.Get(_clearButton).onClick = SearchClearButton_Clicked;
			_inputFilter.onSelect = InputFilter_ChangeSelection;
			_inputFilter.defaultText = T._("검색");
			SearchText = string.Empty;
		}
	}

	public void SetRecipeItems(IEnumerable<KeyValuePair<string, RecipeListWidget.SubList>> subLists)
	{
		_recipeSubLists.Clear();
		_recipeSubLists.AddRange(subLists);
		_recipeSubLists.Sort(RecipeListWidget.SubList.SortComparison);
	}

	public IEnumerable<KeyValuePair<string, RecipeListWidget.SubList>> EnumerateFilteredLists()
	{
		if (string.IsNullOrEmpty(SearchText))
		{
			return _recipeSubLists;
		}
		return _recipeSubLists.Where((KeyValuePair<string, RecipeListWidget.SubList> item) => item.Value.ContainsFilteredItem(SearchText));
	}

	private void RefreshClearButton()
	{
		_clearButton.SetActive(!string.IsNullOrEmpty(_inputFilter.value));
	}

	private void InputFilter_Submitted()
	{
		if (this.SearchTextSubmitted != null)
		{
			this.SearchTextSubmitted();
		}
	}

	private void InputFilter_Changed()
	{
		RefreshClearButton();
	}

	private void SearchClearButton_Clicked(GameObject go)
	{
		_inputFilter.value = string.Empty;
		RefreshClearButton();
		if (this.SearchTextSubmitted != null)
		{
			this.SearchTextSubmitted();
		}
	}

	private void InputFilter_ChangeSelection(bool select)
	{
		if (_focusingEffect != null)
		{
			_focusingEffect.SetActive(select);
		}
	}

	bool IUICursorChangable.IsCursorChangable()
	{
		return true;
	}

	bool IUICursorChangable.IsCursorSpecified(ref GameCursorType cursorType)
	{
		cursorType = GameCursorType.Chatting;
		return true;
	}
}
