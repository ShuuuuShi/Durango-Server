using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Notification;
using Durango.Logic.WarpRush;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using NestedPrefab;
using Shared.Rank;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class WarpRushRanking : MonoBehaviour, IScreenResizeReceiver
{
	public struct TabInfo
	{
		public Category Category;

		public string RevisionId;

		public bool IsExpired;

		public Revision Revision;

		public string GetTabName()
		{
			string text;
			if (IsExpired)
			{
				text = T.GetParticularString("The duration of the respective league has expired", "종료");
			}
			else
			{
				if (!Times.TryParse(Revision.StartsAt, out var result))
				{
					return string.Empty;
				}
				if (!Times.TryParse(Revision.FinishAt, out var result2))
				{
					return string.Empty;
				}
				text = string.Format(T.Culture, "{0:M.d}~{1:M.d}", result.ToLocalTime(), result2.ToLocalTime());
			}
			return Revision.Name.ToString() + " [size=20](" + text + ")[/size]";
		}

		public string GetLeftDays()
		{
			DateTime dateTime = Times.UnixTimeToDateTimeUtc(Connections.Frontend.GetPredictedServerTime());
			if (!Times.TryParse(Revision.RewardAcquireLimitAt, out var result))
			{
				return string.Empty;
			}
			return TimedeltaFormatter.Format((result - dateTime).TotalSeconds, 1, "day");
		}
	}

	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private GameObject _bottomBar;

	[SerializeField]
	private UILabel _bottomDescription;

	[SerializeField]
	private SelectableButton _bottomRewardButton;

	[SerializeField]
	private SelectableButton _rewardPopupButton;

	[SerializeField]
	private RectLayoutComponent _layout;

	[SerializeField]
	private Transform _myRankWidget;

	[SerializeField]
	private WarpRushRankingItem _rankingItemBaseObject;

	[SerializeField]
	private GameObject _noRank;

	[SerializeField]
	private GameObject _bottomDeliveryButtonBackground;

	private int _currentTabIndex = -1;

	private KInfiniteScrollView.View<Record, WarpRushRankingItem> _view;

	private HorizontalTabList _tabList;

	private readonly List<TabInfo> _tabInfos = new List<TabInfo>();

	private readonly List<bool> _isRewardLeft = new List<bool>();

	private WarpRushRankingItem _myRankingItem;

	private WarpRushRankingItem MyRankingItem
	{
		get
		{
			if (_myRankingItem == null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_rankingItemBaseObject.gameObject, _myRankWidget);
				gameObject.SetActive(value: true);
				_myRankingItem = gameObject.GetComponent<WarpRushRankingItem>();
				UIWidget component = gameObject.GetComponent<UIWidget>();
				component.leftAnchor.Set(_myRankWidget, 0f, 0f);
				component.rightAnchor.Set(_myRankWidget, 1f, 0f);
				component.bottomAnchor.Set(_myRankWidget, 0f, 0f);
				component.topAnchor.Set(_myRankWidget, 1f, 0f);
			}
			return _myRankingItem;
		}
	}

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		bool isPortraitScreen = UIManager.IsPortraitScreen;
		_bottomDescription.alignment = ((!isPortraitScreen) ? NGUIText.Alignment.Left : NGUIText.Alignment.Center);
		_bottomDeliveryButtonBackground.SetActive(isPortraitScreen);
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void Awake()
	{
		if (KUtility.GetSize(Yaml.Util.Singleton<Constants>.Instance.Season2.Rankings) <= 0)
		{
			return;
		}
		DateTime utcNow = Times.UnixTimeToDateTimeUtc(Connections.Frontend.GetPredictedServerTime());
		foreach (Category ranking2 in Yaml.Util.Singleton<Constants>.Instance.Season2.Rankings)
		{
			Ranking ranking = SingletonDict<Category, Ranking>.Get(ranking2);
			if (ranking != null)
			{
				KeyValuePair<string, string> currentAndPrevRevisionId = ranking.GetCurrentAndPrevRevisionId(utcNow);
				string key = currentAndPrevRevisionId.Key;
				Revision revision = ranking.Revisions.Get(key);
				if (revision != null)
				{
					_tabInfos.Add(new TabInfo
					{
						Category = ranking2,
						RevisionId = key,
						IsExpired = false,
						Revision = revision
					});
				}
				string value = currentAndPrevRevisionId.Value;
				Revision revision2 = ranking.Revisions.Get(value);
				if (revision2 != null)
				{
					_tabInfos.Add(new TabInfo
					{
						Category = ranking2,
						RevisionId = value,
						IsExpired = true,
						Revision = revision2
					});
				}
			}
		}
		_tabList = _tabLinker.Object.GetComponent<HorizontalTabList>();
		_tabList.BeginLoad();
		foreach (TabInfo tabInfo2 in _tabInfos)
		{
			_tabList.AddText(tabInfo2.GetTabName());
		}
		_tabList.EndLoadByFit();
		_tabList.Clicked += SelectTab;
		_view = _scrollView.Initialize(delegate(WarpRushRankingItem widget, Record record)
		{
			int rank = _view.CurrentIndex + 1;
			if (record.EntityId == PlayerBehavior.LocalPlayer.EntityId)
			{
				widget.SetMyRecord(rank, record.GetScoreText(isEmphatic: true));
			}
			else
			{
				widget.Set(rank, record);
			}
		});
		_rewardPopupButton.Text = T._("랭킹 보상");
		SelectableButton rewardPopupButton = _rewardPopupButton;
		rewardPopupButton.Clicked = (Action)Delegate.Combine(rewardPopupButton.Clicked, (Action)delegate
		{
			WarpRushRankingRewardPopup warpRushRankingRewardPopup = UIManager.Popup.Tooltip<WarpRushRankingRewardPopup>();
			warpRushRankingRewardPopup.Set(_tabInfos.Where((TabInfo info) => !info.IsExpired).ToList());
			warpRushRankingRewardPopup.Show();
		});
		SelectableButton bottomRewardButton = _bottomRewardButton;
		bottomRewardButton.Clicked = (Action)Delegate.Combine(bottomRewardButton.Clicked, (Action)delegate
		{
			TabInfo tabInfo = _tabInfos[_currentTabIndex];
			WarpRushSystem.RequestRankReward(tabInfo.Category, tabInfo.RevisionId);
		});
	}

	private void OnEnable()
	{
		GameSystem<WarpRushSystem>.Instance().RewardedRankingUpdated += WarpRushSystem_RewardedRankingUpdated;
		WarpRushSystem_RewardedRankingUpdated();
		SelectTab((_currentTabIndex != -1) ? _currentTabIndex : 0);
	}

	private void OnDisable()
	{
		GameSystem<WarpRushSystem>.Instance().RewardedRankingUpdated -= WarpRushSystem_RewardedRankingUpdated;
	}

	private void WarpRushSystem_RewardedRankingUpdated()
	{
		_isRewardLeft.Clear();
		for (int i = 0; i < _tabInfos.Count; i++)
		{
			TabInfo tabInfo = _tabInfos[i];
			bool flag = false;
			if (tabInfo.IsExpired)
			{
				flag = GameSystem<WarpRushSystem>.Instance().AnyRewardLeft(tabInfo.Category);
			}
			_tabList.SetNotification(i, flag, Durango.Logic.Notification.Type.Important);
			_isRewardLeft.Add(flag);
			if (i == _currentTabIndex)
			{
				UpdateButton(flag);
			}
		}
	}

	private void UpdateButton(bool isRewardLeft)
	{
		_bottomRewardButton.Text = ((!isRewardLeft) ? T._("보상 수령 완료") : T._("랭킹 보상 받기"));
		_bottomRewardButton.Disabled = !isRewardLeft;
		_bottomRewardButton.SetEffect(isRewardLeft ? PresetButton.Effect.Emphasis : PresetButton.Effect.None);
	}

	private void SelectTab(int index)
	{
		if (_currentTabIndex == index)
		{
			return;
		}
		_currentTabIndex = index;
		_tabList.Select(index);
		TabInfo tabInfo = _tabInfos[_currentTabIndex];
		GameSystem<WarpRushSystem>.Instance().GetRanking(tabInfo.Category, tabInfo.RevisionId, delegate(RankingInfo info)
		{
			if (info == null || !(info.RevisionId != _tabInfos[_currentTabIndex].RevisionId))
			{
				FillContents(info);
				FillBottom(info);
				_layout.UpdateLayout();
				UIUtility.UpdateAnchors(base.transform);
				_scrollView.UpdateLayout();
				float offset = 0f;
				if (!_myRankWidget.gameObject.activeSelf && info != null && info.MyRecord != null)
				{
					offset = (float)_rankingItemBaseObject.GetComponent<UIWidget>().height * ((float)info.MyRecord.Rank - 0.5f) - _scrollView.ScrollView.panel.height / 2f;
				}
				_scrollView.MoveTo(offset, instant: true);
			}
		});
	}

	private void FillContents([CanBeNull] RankingInfo info)
	{
		bool active = info == null || KUtility.GetSize(info.HighScores) <= 0;
		_noRank.SetActive(active);
		_view.SetList(info?.HighScores);
		bool flag = info != null && info.MyRecord != null && info.MyRecord.Rank > info.HighScores.Count;
		_myRankWidget.gameObject.SetActive(flag);
		if (flag)
		{
			MyRankingItem.SetMyRecord(info.MyRecord.Rank, info.MyRecord.GetScoreText(), visibleSeparator: false);
		}
	}

	private void FillBottom([CanBeNull] RankingInfo rankingInfo)
	{
		MyRecord myRecord = rankingInfo?.MyRecord;
		TabInfo tabInfo = _tabInfos[_currentTabIndex];
		bool flag = tabInfo.IsExpired && myRecord != null;
		_bottomBar.SetActive(flag);
		if (flag)
		{
			UpdateButton(_isRewardLeft[_currentTabIndex]);
			_bottomDescription.text = string.Format("[FFFFFF80]{0}[-]  <bar/>  <em>{1}</em>      [FFFFFF80]{2}[-]  <bar/>  [c]{3}[/c]", T._("최종 순위"), myRecord.Rank, T._("남은 수령 기간"), tabInfo.GetLeftDays());
		}
	}

	[ExposedInEditor(null)]
	private void ShowAllRevisions()
	{
		_tabInfos.Clear();
		DateTime dateTime = Times.UnixTimeToDateTimeUtc(Connections.Frontend.GetPredictedServerTime());
		foreach (Category ranking2 in Yaml.Util.Singleton<Constants>.Instance.Season2.Rankings)
		{
			Ranking ranking = SingletonDict<Category, Ranking>.Get(ranking2);
			if (ranking == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, Revision> revision in ranking.Revisions)
			{
				if (Times.TryParse(revision.Value.FinishAt, out var result))
				{
					_tabInfos.Add(new TabInfo
					{
						Category = ranking2,
						RevisionId = revision.Key,
						IsExpired = (dateTime > result),
						Revision = revision.Value
					});
				}
			}
		}
		_tabList.BeginLoad();
		foreach (TabInfo tabInfo in _tabInfos)
		{
			_tabList.AddText(tabInfo.GetTabName());
		}
		_tabList.EndLoadByFit();
		WarpRushSystem_RewardedRankingUpdated();
	}
}
