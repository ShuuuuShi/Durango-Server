using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using L10N;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class PlayerSearchGroup : UIBase
{
	private enum Mode
	{
		SingleSelection,
		MultipleSelection
	}

	public enum Tab
	{
		[T.EnumName("검색")]
		Search,
		[T.EnumName("내 친구")]
		Friends,
		[T.EnumName("부족원")]
		Clan
	}

	private const int TabCount = 3;

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private NestedPrefabLinker _tabList;

	[SerializeField]
	private PlayerSearchInput _searchInput;

	[SerializeField]
	private PlayerSearchResultList _searchResultList;

	[SerializeField]
	private PlayerSearchBottomBar _searchBottomBar;

	private IconTabList _tabs;

	private Mode _mode;

	private Tab _current;

	private Action<IList<string>> _callback;

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Default;
		_tabs = _tabList.Object.GetComponent<IconTabList>();
		_tabs.Clicked += Tabs_Clicked;
		_searchInput.Submitted += SearchInput_Submitted;
		_searchResultList.SelectionChanged += SearchResultList_SelectionChanged;
		_searchBottomBar.SelectionCanceled += SearchBottomBar_SelectionCanceled;
		_searchBottomBar.Confirmed += SearchBottomBar_Confirmed;
		BroadcastMessage("OnInitialize", SendMessageOptions.DontRequireReceiver);
		SetChildrenActive(activated: false);
	}

	[ExposedInEditor(null)]
	public void OpenForMultiple(int maxCount, string title, IList<string> disabledList, Action<IList<string>> callback, string confirmText, PlayerInfoWidget.Visible second = PlayerInfoWidget.Visible.Clan)
	{
		_titleWidget.Object.SetTitle(title);
		_searchResultList.SetMode(multiple: true, PlayerInfoWidget.Visible.Connected | second, PlayerInfoWidget.Visible.Connected.GetName(), second.GetName(), disabledList, maxCount);
		LoadTabs(Mode.MultipleSelection);
		_searchBottomBar.gameObject.SetActive(value: true);
		_searchBottomBar.EnableSelectedView(enable: true);
		_searchBottomBar.SetMaxCount(maxCount);
		_searchBottomBar.SetPlayers(null);
		_searchBottomBar.SetConfirmButton(confirmText, disabled: true);
		_callback = callback;
		Open();
	}

	[ExposedInEditor(null)]
	public void OpenForPersonalSailing(Action<IList<string>> callback)
	{
		_titleWidget.Object.SetTitle(T._("개인섬 항해"));
		_searchResultList.SetMode(multiple: false, PlayerInfoWidget.Visible.Connected | PlayerInfoWidget.Visible.PioneerGrade, PlayerInfoWidget.Visible.Connected.GetName(), PlayerInfoWidget.Visible.PioneerGrade.GetName());
		LoadTabs(Mode.SingleSelection);
		_searchBottomBar.gameObject.SetActive(value: false);
		_searchBottomBar.EnableSelectedView(enable: false);
		_searchBottomBar.SetDescription(string.Empty);
		_searchBottomBar.SetConfirmButton(T._("항해"), disabled: false);
		_callback = callback;
		Open();
	}

	private void SearchInput_Submitted(string playerName, string freq)
	{
		if (_current == Tab.Search && string.IsNullOrEmpty(playerName))
		{
			freq = string.Empty;
			_searchInput.SetInput(string.Empty, string.Empty);
		}
		Search(playerName, freq, reload: false);
	}

	private void SearchResultList_SelectionChanged()
	{
		if (_mode == Mode.SingleSelection)
		{
			string value = _searchResultList.SelectedList.FirstOrDefault();
			bool active = !string.IsNullOrEmpty(value);
			_searchBottomBar.gameObject.SetActive(active);
		}
		else
		{
			_searchBottomBar.SetPlayers(_searchResultList.SelectedList);
		}
	}

	private void SearchBottomBar_SelectionCanceled(string entityId)
	{
		_searchResultList.Select(entityId, selected: false);
	}

	private void SearchBottomBar_Confirmed()
	{
		Close();
		if (_callback != null)
		{
			_callback(_searchResultList.SelectedList);
		}
	}

	private void LoadTabs(Mode mode)
	{
		_mode = mode;
		_tabs.BeginLoad();
		for (int i = 0; i < 3; i++)
		{
			Tab tab = FromIndex(i);
			if (tab != Tab.Clan || PlayerBehavior.LocalPlayer.HasClan)
			{
				_tabs.Add(null, tab.GetName());
			}
		}
		_tabs.EndLoad();
		_searchInput.SetInput(string.Empty, string.Empty);
		SelectTab(0);
	}

	private void Tabs_Clicked(int index)
	{
		if (index != -1)
		{
			SelectTab(index);
		}
	}

	public void SelectTab(int index)
	{
		if (index >= 0 && index < 3)
		{
			_tabs.Select(index);
			_current = FromIndex(index);
			KeyValuePair<string, string> input = _searchInput.GetInput();
			Search(input.Key, input.Value, reload: true);
		}
	}

	private static Tab FromIndex(int index)
	{
		return (Tab)index;
	}

	private void Search(string key, string freq, bool reload)
	{
		switch (_current)
		{
		case Tab.Friends:
			_searchResultList.SearchFriends(key, freq, reload);
			break;
		case Tab.Clan:
			_searchResultList.SearchClan(key, freq, reload);
			break;
		case Tab.Search:
			_searchResultList.SearchPlayers(key, freq);
			break;
		}
	}
}
