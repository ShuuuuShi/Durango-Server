using Durango.UI.Control;
using InteractionData;
using L10N;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class PunchingLeaderboardGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private NestedPrefabLinker _tabPrefabLinker;

	[SerializeField]
	private PunchRankingItemListWidget _punchRankingItemListWidget;

	[SerializeField]
	private float _delaySecForOpen;

	private IconTabList _tabList;

	private PunchingLeaderboardSystem.Category _currentCategory;

	private void Start()
	{
		_openCloseSound = UISound.GroupType.PunchingRanking;
		_titleWidget.Object.SetTitle(T._("랭킹"));
		GameSystem<PunchingLeaderboardSystem>.Instance().LeaderboardsUpdated += OnLeaderboardsUpdated;
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ViewPunchRanking, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if (!(targetComponent == null))
			{
				GameSystem<PunchingLeaderboardSystem>.Instance().UpdateLeaderboards(targetComponent);
				Open();
				ClearLeaderboards();
			}
		});
		_tabList = _tabPrefabLinker.Object.GetComponent<IconTabList>();
		_tabList.Clicked += LeaderboardCategoryListWidget_SelectionChanged;
		_tabList.BeginLoad();
		_tabList.Add("punch_cate_recent", PunchingLeaderboardSystem.Category.Recently.GetName());
		_tabList.Add("punch_cate_location", PunchingLeaderboardSystem.Category.Region.GetName());
		_tabList.Add("punch_cate_all", PunchingLeaderboardSystem.Category.Global.GetName());
		_tabList.EndLoad();
		base.OnOpenSucceed += delegate
		{
			SelectCategory(_currentCategory);
		};
		SetChildrenActive(activated: false);
	}

	private void ClearLeaderboards()
	{
		_punchRankingItemListWidget.ClearLeaderboards();
		_punchRankingItemListWidget.ShowLoadingRing(show: true);
	}

	private void RefreshLeaderboards()
	{
		_punchRankingItemListWidget.ShowLoadingRing(show: false);
		_punchRankingItemListWidget.RefreshLeaderboards(_currentCategory);
	}

	private void SelectCategory(PunchingLeaderboardSystem.Category category)
	{
		_currentCategory = category;
		_tabList.Select((int)category);
		_punchRankingItemListWidget.RefreshLeaderboards(category);
	}

	private void OnLeaderboardsUpdated()
	{
		if (base.IsOpened)
		{
			RefreshLeaderboards();
			return;
		}
		KUtility.DelayedCall(this, delegate
		{
			Open();
			RefreshLeaderboards();
		}, _delaySecForOpen);
	}

	private void LeaderboardCategoryListWidget_SelectionChanged(int index)
	{
		SelectCategory((PunchingLeaderboardSystem.Category)index);
	}
}
