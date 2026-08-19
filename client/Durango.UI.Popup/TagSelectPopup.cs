using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class TagSelectPopup : TooltipBase, IUIInitializable
{
	[SerializeField]
	private RectLayoutComponent _layout;

	[SerializeField]
	private UIPanel _selectedTagPanel;

	[SerializeField]
	private KScrollView _selectedTagScroll;

	[SerializeField]
	private KGridInfiniteScrollView _entireTagScroll;

	[SerializeField]
	private UIWidget _selectedTagWidget;

	[SerializeField]
	private UIInput _textInput;

	[SerializeField]
	private SelectableWidget _removeSearchLabelButton;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private SelectableButton _closeButton;

	private Action<HashSet<string>> _onOk;

	private List<string> _entireTags;

	private List<string> _availableTags;

	private List<string> _searchedTags;

	private HashSet<string> _duplicatedNameTags;

	private readonly HashSet<string> _selectedTags = new HashSet<string>();

	private HashSet<string> _confirmedTags = new HashSet<string>();

	private KGridInfiniteScrollView.View<string, TagButtonWidget> _tagScrollView;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	void IUIInitializable.Init()
	{
		Dictionary<string, Tag> instance = SingletonDict<string, Tag>.Instance;
		_entireTags = (from elem in (from elem in instance
				where elem.Value.Visible && !elem.Value.Unsearchable
				orderby elem.Value.Grade descending
				select elem).ThenBy((KeyValuePair<string, Tag> elem) => elem.Key, StringComparer.Ordinal)
			select elem.Key).ToList();
		_duplicatedNameTags = new HashSet<string>(_entireTags.GroupBy(TagData.GetTagName).SelectMany((IGrouping<string, string> grp) => grp.Skip(1)));
		_removeSearchLabelButton.gameObject.SetActive(value: false);
		_tagScrollView = _entireTagScroll.Initialize(delegate(TagButtonWidget comp, string tagId)
		{
			comp.Set((!_duplicatedNameTags.Contains(tagId)) ? TagData.GetTagName(tagId) : TagData.GetTagNameAndPurpose(tagId));
			comp.GetComponent<Selectable>().Selected = _selectedTags.Contains(tagId);
		}, delegate(TagButtonWidget comp)
		{
			Selectable component = comp.GetComponent<Selectable>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickTagButton));
		});
		_closeButton.Clicked = Hide;
		_confirmButton.Clicked = OnTryConfirmOnModal;
		_removeSearchLabelButton.Clicked = delegate
		{
			_searchedTags = _availableTags;
			_tagScrollView.SetList(_searchedTags);
			_entireTagScroll.Reposition();
			_textInput.value = string.Empty;
			_textInput.isSelected = false;
			_removeSearchLabelButton.gameObject.SetActive(value: false);
		};
		UIEventListener uIEventListener = UIEventListener.Get(_textInput.gameObject);
		uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, (UIEventListener.BoolDelegate)delegate(GameObject obj, bool selected)
		{
			if (selected)
			{
				_textInput.label.text = string.Empty;
			}
		});
		EventDelegate.Add(_textInput.onSubmit, delegate
		{
			string searchText = _textInput.value.Trim();
			if (!string.IsNullOrEmpty(searchText))
			{
				_searchedTags = _availableTags.Where((string elem) => TagData.GetTagName(elem).ContainsIgnoreCase(searchText)).ToList();
				_tagScrollView.SetList(_searchedTags);
				_entireTagScroll.Reposition(resetPosition: true, tween: false);
				if (_searchedTags.Count < _availableTags.Count)
				{
					_removeSearchLabelButton.gameObject.SetActive(value: true);
				}
			}
		});
		_textInput.defaultText = T._("속성 검색");
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (_onOk != null)
		{
			_onOk(_confirmedTags);
		}
	}

	protected override void OnTryConfirmOnModal()
	{
		_confirmedTags = new HashSet<string>(_selectedTags);
		Hide();
	}

	protected override void OnTryCancelOnModal()
	{
		_confirmedTags.Clear();
		Hide();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}

	protected override SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = true;
		return _closeButton;
	}

	private void OnClickTagButton()
	{
		Selectable current = Selectable.Current;
		TagButtonWidget component = current.GetComponent<TagButtonWidget>();
		if (component == null)
		{
			return;
		}
		int num = _tagScrollView.IndexOf(component);
		if (num != -1)
		{
			string item = _searchedTags[num];
			if (!_selectedTags.Contains(item))
			{
				_selectedTags.Add(item);
				current.Selected = true;
			}
			else
			{
				_selectedTags.Remove(item);
				current.Selected = false;
			}
			UpdateSelectedTagsWidget();
			_selectedTagScroll.MoveToNode(_selectedTags.Count, instant: false);
		}
	}

	public void Set([NotNull] IList<string> searchOption, Action<HashSet<string>> hidePopupCalled, [CanBeNull] HashSet<string> tagsToShow = null)
	{
		_confirmedTags.Clear();
		_selectedTags.Clear();
		_selectedTags.AddRange(searchOption);
		_onOk = hidePopupCalled;
		_availableTags = FilterTags(_entireTags, tagsToShow);
		_searchedTags = _availableTags;
		_textInput.value = string.Empty;
		_removeSearchLabelButton.gameObject.SetActive(value: false);
		UpdateSelectedTagsWidget();
		UpdateTagsScroll(_searchedTags);
	}

	private List<string> FilterTags(List<string> tags, HashSet<string> filter)
	{
		if (filter == null)
		{
			return tags;
		}
		List<string> list = new List<string>();
		foreach (string entireTag in _entireTags)
		{
			if (filter.Contains(entireTag))
			{
				list.Add(entireTag);
			}
		}
		return list;
	}

	private void UpdateSelectedTagsWidget()
	{
		List<string> list = _selectedTags.ToList();
		ListObjectPool nodes = _selectedTagScroll.Nodes;
		nodes.BeginLoad();
		for (int i = 0; i < list.Count; i++)
		{
			string tagId = list[i];
			ItemTagWidget component = nodes.GetNext().GetComponent<ItemTagWidget>();
			component.Set((!_duplicatedNameTags.Contains(tagId)) ? TagData.GetTagName(tagId) : TagData.GetTagNameAndPurpose(tagId), delegate
			{
				_selectedTags.Remove(tagId);
				UpdateSelectedTagsWidget();
				UpdateTagsScroll(_searchedTags);
			});
		}
		nodes.EndLoad();
		if (list.Count == 0)
		{
			_selectedTagWidget.height = 14;
		}
		else
		{
			Vector4 baseClipRegion = _selectedTagPanel.baseClipRegion;
			baseClipRegion.w = 76f;
			_selectedTagPanel.baseClipRegion = baseClipRegion;
			_selectedTagWidget.height = 76;
			_selectedTagScroll.UpdateLayout();
		}
		Vector2 vector = _layout.UpdateLayout(Mathf.Min(UIManager.ScreenWidth * 3 / 4, 960), Mathf.Min(UIManager.ScreenHeight * 3 / 4, 960));
		base.Widget.SetDimensions((int)vector.x, (int)vector.y);
		UIUtility.UpdateAnchors(base.transform);
		_confirmButton.Disabled = _selectedTags.Count == 0;
	}

	private void UpdateTagsScroll(List<string> list)
	{
		_tagScrollView.SetList(list);
		_entireTagScroll.ResetPosition();
	}
}
