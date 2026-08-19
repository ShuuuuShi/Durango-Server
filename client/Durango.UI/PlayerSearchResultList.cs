using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Clan;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class PlayerSearchResultList : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _headerLabel1;

	[SerializeField]
	private UILabel _headerLabel2;

	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private UIWidget _noData;

	private KInfiniteScrollView.View<string, PlayerSearchInfoWidget> _view;

	private readonly List<string> _selectedList = new List<string>();

	private readonly HashSet<string> _disabledSet = new HashSet<string>();

	private bool _multipleSelection;

	private PlayerInfoWidget.Visible _visibleFlag;

	private int _maxSelection;

	private int _verision;

	[NotNull]
	public List<string> SelectedList => _selectedList;

	public event Action SelectionChanged;

	[UsedImplicitly]
	private void OnInitialize()
	{
		_view = _scrollView.Initialize(delegate(PlayerSearchInfoWidget widget, string entityId)
		{
			widget.EnableCheckMode(_multipleSelection);
			widget.Set(entityId, _visibleFlag);
			UpdateWidgetSelected(widget);
		}, delegate(PlayerSearchInfoWidget widget)
		{
			PlayerSearchInfoWidget playerSearchInfoWidget = widget;
			playerSearchInfoWidget.Clicked = (Action<string>)Delegate.Combine(playerSearchInfoWidget.Clicked, (Action<string>)delegate(string entityId)
			{
				bool flag = _selectedList.Contains(entityId);
				bool selected = !flag;
				if (!_multipleSelection || !widget.Check.Disabled)
				{
					UISound.PlayClick(UISound.ClickType.ButtonDefault);
					Select(entityId, selected);
				}
			});
		});
	}

	public void SetMode(bool multiple, PlayerInfoWidget.Visible visibleFlag, string header1, string header2, IList<string> disabledList = null, int maxSelection = 0)
	{
		_multipleSelection = multiple;
		_visibleFlag = visibleFlag;
		_maxSelection = maxSelection;
		_selectedList.Clear();
		if ((bool)_headerLabel1)
		{
			_headerLabel1.text = header1;
		}
		if ((bool)_headerLabel2)
		{
			_headerLabel2.text = header2;
		}
		_disabledSet.Clear();
		if (disabledList == null)
		{
			return;
		}
		foreach (string disabled in disabledList)
		{
			_disabledSet.Add(disabled);
		}
	}

	public void Select(string entityId, bool selected, bool raiseEvent = true)
	{
		if (selected)
		{
			if (_multipleSelection)
			{
				if (_maxSelection > 0 && _selectedList.Count >= _maxSelection)
				{
					return;
				}
			}
			else
			{
				_selectedList.Clear();
			}
			_selectedList.Add(entityId);
		}
		else
		{
			_selectedList.Remove(entityId);
		}
		OnSelectionChanged(raiseEvent);
	}

	private void OnSelectionChanged(bool raiseEvent)
	{
		foreach (PlayerSearchInfoWidget item in _view.List)
		{
			UpdateWidgetSelected(item);
		}
		if (raiseEvent && this.SelectionChanged != null)
		{
			this.SelectionChanged();
		}
	}

	private void UpdateWidgetSelected(PlayerSearchInfoWidget widget)
	{
		string entityId = widget.EntityId;
		bool selected = _selectedList.Contains(entityId);
		if (_multipleSelection)
		{
			widget.Check.Selected = selected;
			widget.Check.Disabled = _disabledSet.Contains(entityId);
		}
		else
		{
			widget.Selected = selected;
		}
	}

	private void SetLoading()
	{
		_titleLabel.text = T._("검색 중");
		_scrollView.gameObject.SetActive(value: false);
		_noData.gameObject.SetActive(value: false);
		UIManager.Popup.LoadingRing.AttachToWidget(base.gameObject);
	}

	private void SetResult(IList<string> list, string titleFormat)
	{
		UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
		int size = KUtility.GetSize(list);
		_titleLabel.text = T._(titleFormat, size);
		if (size > 0)
		{
			_view.SetList(list);
			_scrollView.ResetPosition();
			_scrollView.gameObject.SetActive(value: true);
			_noData.gameObject.SetActive(value: false);
		}
		else
		{
			_scrollView.gameObject.SetActive(value: false);
			_noData.gameObject.SetActive(value: true);
		}
		if (!_multipleSelection && _selectedList.Count > 0)
		{
			string entityId = _selectedList[0];
			if (list == null || !list.Contains(_selectedList[0]))
			{
				Select(entityId, selected: false);
			}
		}
	}

	public void SearchPlayers(string key, string freq, Predicate<FoundPlayerInfo> filter = null)
	{
		string title = T._("검색 결과 {0}");
		if (string.IsNullOrEmpty(key))
		{
			SetResult(null, title);
			return;
		}
		SetLoading();
		_verision++;
		int version = _verision;
		Singleton<PlayerInfoManager>.Instance().SearchPlayerInfos(key, freq, delegate(FoundPlayerInfo[] list)
		{
			if (version == _verision)
			{
				List<string> list2 = FilterPlayerList(list, filter);
				SetResult(list2, title);
			}
		});
	}

	private static List<string> FilterPlayerList(IList<FoundPlayerInfo> list, Predicate<FoundPlayerInfo> filter)
	{
		List<string> list2 = new List<string>();
		int i = 0;
		for (int size = KUtility.GetSize(list); i < size; i++)
		{
			FoundPlayerInfo obj = list[i];
			if (!(obj.EntityId == PlayerBehavior.LocalPlayer.EntityId) && (filter == null || filter(obj)))
			{
				list2.Add(obj.EntityId);
			}
		}
		return list2;
	}

	public void SearchFriends(string key, string freq, bool reload)
	{
		SetLoading();
		Social social2 = GameSystem<SocialSystem>.Instance().Social;
		if (reload || social2.FriendEntities == null)
		{
			_verision++;
			int version = _verision;
			GameSystem<SocialSystem>.Instance().GetSocial(delegate(Social social)
			{
				if (_verision == version)
				{
					OnSocial(social, key, freq);
				}
			});
		}
		else
		{
			OnSocial(social2, key, freq);
		}
	}

	private void OnSocial(Social social, string key, string freq)
	{
		List<string> list = social.FriendEntities?.Keys.ToList();
		string text = T._("친구 {0}");
		if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(freq))
		{
			SetResult(list, text);
		}
		else
		{
			RequestPlayerInfos(list, text, key, freq);
		}
	}

	private void RequestPlayerInfos(List<string> list, string title, string key, string freq)
	{
		_verision++;
		int version = _verision;
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfos(list, delegate(Durango.Player.PlayerInfo[] infos)
		{
			if (_verision == version)
			{
				List<string> list2 = new List<string>();
				int i = 0;
				for (int size = KUtility.GetSize(infos); i < size; i++)
				{
					Durango.Player.PlayerInfo playerInfo = infos[i];
					if (!(playerInfo.EntityId == PlayerBehavior.LocalPlayer.EntityId) && (string.IsNullOrEmpty(key) || playerInfo.Name.Contains(key)) && (string.IsNullOrEmpty(freq) || playerInfo.Freq.ToString("D4").Contains(freq)))
					{
						list2.Add(playerInfo.EntityId);
					}
				}
				SetResult(list2, title);
			}
		});
	}

	public void SearchClan(string key, string freq, bool reload)
	{
		if (reload || GameSystem<ClanSystem>.Instance().PlayerClan == null)
		{
			_verision++;
			int version = _verision;
			ClanSystem.GetClanInfo(PlayerBehavior.LocalPlayer.ClanId, delegate(Clan clan)
			{
				if (_verision == version)
				{
					OnClan(clan, key, freq);
				}
			});
		}
		else
		{
			OnClan(GameSystem<ClanSystem>.Instance().PlayerClan, key, freq);
		}
	}

	private void OnClan(Clan clan, string key, string freq)
	{
		string text = T._("부족원 {0}");
		if (clan == null || clan.Members == null)
		{
			SetResult(null, text);
			return;
		}
		List<string> list = clan.Members.Select((Durango.Logic.Clan.Member x) => x.EntityId).ToList();
		RequestPlayerInfos(list, text, key, freq);
	}
}
