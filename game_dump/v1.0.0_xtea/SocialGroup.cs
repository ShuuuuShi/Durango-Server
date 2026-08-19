using System.Collections.Generic;
using L10N;
using Player;
using UnityEngine;

public class SocialGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleBar;

	[SerializeField]
	private SocialTitleWidget _titleWidget;

	[SerializeField]
	private SocialPlayerListWidget _playerListWidget;

	private bool _isDefaultView;

	private void Start()
	{
		_titleBar.OnClose += base.ForceClose;
		_titleWidget.OnSearchPlayer += OnSearchPlayers;
		_playerListWidget.CardCountUpdated += OnUpdateCardCount;
		base.OnClose();
	}

	private void OnEnable()
	{
		GameSystem<SocialSystem>.Instance().FollowingListUpdated += OnFollowingListUpdated;
	}

	private void OnDisable()
	{
		GameSystem<SocialSystem>.Instance().FollowingListUpdated -= OnFollowingListUpdated;
	}

	protected override bool OnOpen()
	{
		bool result = base.OnOpen();
		Set(GameSystem<SocialSystem>.Instance().FollowingList, isDefault: true);
		return result;
	}

	public void Set(IList<ulong> playerList)
	{
		Set(playerList, isDefault: false);
	}

	public void Set(IList<ulong> playerList, bool isDefault)
	{
		_isDefaultView = isDefault;
		OnUpdateCardCount(0);
		_playerListWidget.Set(playerList);
	}

	public void Set(IList<PlayerInfo> playerList)
	{
		Set(playerList, isDefault: false);
	}

	public void Set(IList<PlayerInfo> playerList, bool isDefault)
	{
		_isDefaultView = isDefault;
		OnUpdateCardCount(0);
		_playerListWidget.Set(playerList);
	}

	private void OnUpdateCardCount(int count)
	{
		_titleWidget.SetCount((!_isDefaultView) ? T._("친구 검색 결과") : T._("친구 목록"), count);
	}

	private void OnSearchPlayers(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			Set(GameSystem<SocialSystem>.Instance().FollowingList, isDefault: true);
		}
		else
		{
			KSingleton<PlayerInfoManager>.Instance().SearchPlayerInfos(key, Set);
		}
	}

	private void OnFollowingListUpdated()
	{
		if (_isDefaultView)
		{
			Set(GameSystem<SocialSystem>.Instance().FollowingList, isDefault: true);
		}
	}
}
