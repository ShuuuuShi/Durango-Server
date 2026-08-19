using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class FriendListPage : MonoBehaviour
{
	[SerializeField]
	private UIInput _searchInput;

	[SerializeField]
	private GameObject _searchClearButton;

	[SerializeField]
	private FriendListColumnHeaderWidget _friendHeader;

	[SerializeField]
	private FriendListColumnHeaderWidget _clanHeader;

	[SerializeField]
	private FriendListColumnHeaderWidget _connectHeader;

	[SerializeField]
	private FriendListColumnHeaderWidget _relationHeader;

	[SerializeField]
	private GameObject _noData;

	[SerializeField]
	private KInfiniteScrollView _friendList;

	private string _sortedKey;

	private SortableColumnWidget<string>.State _sortState;

	private string _searchName;

	private SocialGroup _parent;

	private KInfiniteScrollView.View<string, PlayerInfoWidget> _view;

	private FriendListColumnHeaderWidget[] _headers;

	private Social _social;

	private bool _isLoading;

	private void Awake()
	{
		UIManager.AddOnScreenResized(OnScreenResize);
		_view = _friendList.Initialize<string, PlayerInfoWidget>(PlayerInfoSetter);
		_friendHeader.Value = "name";
		_clanHeader.Value = "clan";
		_clanHeader.SetText(T._("부족"));
		_connectHeader.Value = "connect";
		_connectHeader.SetText(T._("마지막 접속"));
		_relationHeader.Value = "relation";
		_relationHeader.SetText(T._("관계"));
		_searchInput.defaultText = T._("검색");
		_headers = new FriendListColumnHeaderWidget[4] { _friendHeader, _clanHeader, _connectHeader, _relationHeader };
		FriendListColumnHeaderWidget[] headers = _headers;
		foreach (FriendListColumnHeaderWidget friendListColumnHeaderWidget in headers)
		{
			friendListColumnHeaderWidget.Clicked = (Action<string>)Delegate.Combine(friendListColumnHeaderWidget.Clicked, new Action<string>(OnListSort));
		}
		_parent = GetComponentInParent<SocialGroup>();
		_parent.AddOnUpdated(Refresh);
	}

	private void Start()
	{
		EventDelegate.Add(_searchInput.onSubmit, SearchInputSubmitted);
		EventDelegate.Add(_searchInput.onChange, SearchInputChanged);
		UIEventListener.Get(_searchClearButton).onClick = SearchClearButtonClicked;
	}

	private void OnScreenResize()
	{
		bool flag = UIManager.IsPortraitWidget(base.gameObject);
		_clanHeader.gameObject.SetActive(!flag);
		_connectHeader.gameObject.SetActive(!flag);
	}

	private void SearchInputSubmitted()
	{
		if (!_isLoading)
		{
			string value = _searchInput.value;
			if (!string.IsNullOrEmpty(value))
			{
				_sortState = SortableColumnWidget<string>.State.None;
				_sortedKey = null;
			}
			_searchName = value;
			Refresh();
			_searchInput.isSelected = false;
		}
	}

	private void SearchInputChanged()
	{
		string value = _searchInput.value;
		bool flag = string.IsNullOrEmpty(value);
		_searchClearButton.SetActive(!flag);
	}

	private void SearchClearButtonClicked(GameObject obj)
	{
		_searchInput.value = null;
		_searchName = null;
		Refresh();
	}

	private void OnEnable()
	{
		_friendList.ResetPosition();
	}

	private void OnDisable()
	{
		_isLoading = false;
		_searchInput.value = null;
		_searchName = null;
	}

	private void PlayerInfoSetter(PlayerInfoWidget comp, string entityId)
	{
		comp.Set(entityId);
	}

	private void Refresh()
	{
		Refresh(_social);
	}

	private void Refresh(Social social)
	{
		_social = social;
		RefershHeaderSortState();
		int size = KUtility.GetSize(social.FriendEntities);
		if (size > 0)
		{
			_friendList.gameObject.SetActive(value: true);
			_noData.gameObject.SetActive(value: false);
			string[] ids = social.FriendEntities.Keys.ToArray();
			if (!string.IsNullOrEmpty(_searchName))
			{
				_isLoading = true;
				Singleton<PlayerInfoManager>.Instance().RequestPlayerInfos(ids, delegate(Durango.Player.PlayerInfo[] infos)
				{
					UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
					_friendList.Panel.alpha = 1f;
					_isLoading = false;
					_view.SetList((from info in infos
						where string.IsNullOrEmpty(_searchName) || info.Name.Contains(_searchName)
						select info.EntityId).ToArray());
					_friendList.Reposition();
				});
				if (_isLoading)
				{
					UIManager.Popup.LoadingRing.AttachToWidget(base.gameObject);
					_friendList.Panel.alpha = 0f;
				}
			}
			else if (_sortState == SortableColumnWidget<string>.State.None)
			{
				_view.SetList(ids);
				_friendList.Reposition();
			}
			else if (_sortedKey == _friendHeader.Value || _sortedKey == _clanHeader.Value)
			{
				_isLoading = true;
				Singleton<PlayerInfoManager>.Instance().RequestPlayerInfos(ids, delegate(Durango.Player.PlayerInfo[] infos)
				{
					UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
					_friendList.Panel.alpha = 1f;
					_isLoading = false;
					_view.SetList(GetSortedList(infos, _sortedKey, _sortState).ToArray());
					_friendList.Reposition();
				});
				if (_isLoading)
				{
					UIManager.Popup.LoadingRing.AttachToWidget(base.gameObject);
					_friendList.Panel.alpha = 0f;
				}
			}
			else if (_sortedKey == _connectHeader.Value)
			{
				_isLoading = true;
				int watingCount = ids.Length;
				Dictionary<string, PlayerConnected> dict = new Dictionary<string, PlayerConnected>();
				foreach (string entityId in ids)
				{
					Singleton<PlayerInfoManager>.Instance().GetPlayerConnected(entityId, delegate(PlayerConnected connected)
					{
						watingCount--;
						dict.Add(entityId, connected);
						if (watingCount <= 0)
						{
							UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
							_friendList.Panel.alpha = 1f;
							_isLoading = false;
							_view.SetList(((_sortState != SortableColumnWidget<string>.State.Descending) ? ids.OrderBy((string id) => dict.Get(id)) : ids.OrderByDescending((string id) => dict.Get(id))).ToArray());
							_friendList.Reposition();
						}
					});
				}
				if (_isLoading)
				{
					UIManager.Popup.LoadingRing.AttachToWidget(base.gameObject);
					_friendList.Panel.alpha = 0f;
				}
			}
			else if (_sortedKey == _relationHeader.Value)
			{
				_view.SetList(((_sortState != SortableColumnWidget<string>.State.Descending) ? ids.OrderBy((string id) => GameSystem<SocialSystem>.Instance().GetFriendly(id)) : ids.OrderByDescending((string id) => GameSystem<SocialSystem>.Instance().GetFriendly(id))).ToArray());
				_friendList.Reposition();
			}
		}
		else
		{
			_friendList.gameObject.SetActive(value: false);
			_noData.gameObject.SetActive(value: true);
		}
		_friendHeader.SetText(T._("친구 {0}", size));
	}

	private void OnListSort(string key)
	{
		if (!_isLoading)
		{
			_searchInput.value = null;
			_searchName = null;
			if (_sortState == SortableColumnWidget<string>.State.None)
			{
				_sortState = SortableColumnWidget<string>.State.Ascending;
				_sortedKey = key;
			}
			else if (_sortedKey == key)
			{
				_sortState = (SortableColumnWidget<string>.State)((_sortState != SortableColumnWidget<string>.State.Ascending) ? SortableColumnWidget<string>.State.Ascending : SortableColumnWidget<string>.State.Descending);
			}
			else
			{
				_sortedKey = key;
			}
			Refresh();
		}
	}

	private IEnumerable<string> GetSortedList(Durango.Player.PlayerInfo[] infos, string sortedKey, SortableColumnWidget<string>.State state)
	{
		IEnumerable<Durango.Player.PlayerInfo> source = infos;
		if (state != 0)
		{
			bool flag = state == SortableColumnWidget<string>.State.Descending;
			if (sortedKey == _friendHeader.Value)
			{
				source = ((!flag) ? infos.OrderBy((Durango.Player.PlayerInfo info) => info.Name) : infos.OrderByDescending((Durango.Player.PlayerInfo info) => info.Name));
			}
			else if (sortedKey == _clanHeader.Value)
			{
				source = ((!flag) ? infos.OrderBy((Durango.Player.PlayerInfo info) => info.ClanName) : infos.OrderByDescending((Durango.Player.PlayerInfo info) => info.ClanName));
			}
			else if (!(sortedKey == _connectHeader.Value) && !(sortedKey == _relationHeader.Value))
			{
			}
		}
		return source.Select((Durango.Player.PlayerInfo info) => info.EntityId);
	}

	private void RefershHeaderSortState()
	{
		FriendListColumnHeaderWidget[] headers = _headers;
		foreach (FriendListColumnHeaderWidget friendListColumnHeaderWidget in headers)
		{
			if (_sortState != 0 && friendListColumnHeaderWidget.Value == _sortedKey)
			{
				friendListColumnHeaderWidget.SetState(_sortState);
			}
			else
			{
				((SortableColumnWidget<string>)friendListColumnHeaderWidget).SetState(SortableColumnWidget<string>.State.None);
			}
		}
	}
}
