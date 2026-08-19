using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class PunchRankingItemListWidget : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private KScrollView _kScrollView;

	[SerializeField]
	private UIWidget _leaderboard;

	[SerializeField]
	private UIWidget _myScore;

	[SerializeField]
	private PunchRankingItemWidget _myRankingItemWidget;

	[SerializeField]
	private GameObject _noData;

	private bool _showLoadingRing;

	void IUIInitializable.Init()
	{
		_kScrollView.Nodes.Init(delegate(GameObject obj)
		{
			obj.GetComponent<PunchRankingItemWidget>().Init();
		});
		_myRankingItemWidget.Init();
		ClearLeaderboards();
	}

	public void ClearLeaderboards()
	{
		_kScrollView.Nodes.Clear();
		_kScrollView.Reposition();
		RefreshPanes(showMyScore: false);
		_noData.SetActive(value: false);
	}

	public void RefreshLeaderboards(PunchingLeaderboardSystem.Category category)
	{
		RefreshRankingItems(category);
		RefreshPanes(RefreshMyScore(category));
		_noData.SetActive(!_showLoadingRing && _kScrollView.Nodes.Count == 0);
	}

	public void ShowLoadingRing(bool show)
	{
		_showLoadingRing = show;
		if (_showLoadingRing)
		{
			UIManager.Popup.LoadingRing.AttachToWidget(base.gameObject);
		}
		else
		{
			UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
		}
	}

	private void RefreshRankingItems(PunchingLeaderboardSystem.Category category)
	{
		_kScrollView.Nodes.BeginLoad();
		LeaderboardContent[] leaderboard = GameSystem<PunchingLeaderboardSystem>.Instance().GetLeaderboard(category);
		if (category == PunchingLeaderboardSystem.Category.Recently)
		{
			foreach (LeaderboardContent content in leaderboard)
			{
				if (TryGetPlayerInfo(content, out var playerInfo))
				{
					PunchRankingItemWidget component = _kScrollView.Nodes.GetNext().GetComponent<PunchRankingItemWidget>();
					component.Refresh(content, playerInfo);
				}
			}
		}
		else
		{
			int num = 0;
			int num2 = -1;
			for (int j = 0; j < leaderboard.Length; j++)
			{
				LeaderboardContent content2 = leaderboard[j];
				if (TryGetPlayerInfo(content2, out var playerInfo2))
				{
					int num3 = (content2.Damage.HasValue ? content2.Damage.Value : 0);
					if (num2 == -1 || num2 > num3)
					{
						num++;
						num2 = num3;
					}
					PunchRankingItemWidget component2 = _kScrollView.Nodes.GetNext().GetComponent<PunchRankingItemWidget>();
					component2.Refresh(content2, playerInfo2, num);
				}
			}
		}
		_kScrollView.Nodes.EndLoad();
		_kScrollView.Reposition();
	}

	private bool RefreshMyScore(PunchingLeaderboardSystem.Category category)
	{
		LeaderboardContent? myScore = GameSystem<PunchingLeaderboardSystem>.Instance().MyScore;
		if (myScore.HasValue && TryGetPlayerInfo(myScore.Value, out var playerInfo))
		{
			int? rankingIndex = null;
			if (category != 0)
			{
				rankingIndex = GetMyRankingIndex(myScore.Value);
			}
			_myRankingItemWidget.Refresh(myScore.Value, playerInfo, rankingIndex);
		}
		return myScore.HasValue;
	}

	private void RefreshPanes(bool showMyScore)
	{
		_leaderboard.bottomAnchor.absolute = (showMyScore ? _myScore.topAnchor.absolute : 0);
		UIUtility.UpdateAnchors(_leaderboard.transform);
		_myScore.gameObject.SetActive(showMyScore);
	}

	private int GetMyRankingIndex(LeaderboardContent myContent)
	{
		string userId = myContent.UserId;
		int num = (myContent.Damage.HasValue ? myContent.Damage.Value : 0);
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			PunchRankingItemWidget punchRankingItemWidget = _kScrollView.Nodes.Get<PunchRankingItemWidget>(i);
			if (punchRankingItemWidget != null && punchRankingItemWidget.UserId == userId && punchRankingItemWidget.Score == num)
			{
				return punchRankingItemWidget.RankingIndex;
			}
		}
		return 0;
	}

	private static bool TryGetPlayerInfo(LeaderboardContent content, [NotNull] out Durango.Player.PlayerInfo playerInfo)
	{
		playerInfo = Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(content.UserId);
		return playerInfo.Valid;
	}
}
