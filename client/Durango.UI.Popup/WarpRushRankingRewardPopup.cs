using System;
using System.Collections.Generic;
using Durango.UI.Control;
using L10N;
using Shared.Rank;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class WarpRushRankingRewardPopup : TooltipBase
{
	private const int TopRankCount = 3;

	[SerializeField]
	private UIWidget _tabContainer;

	[SerializeField]
	private ListObjectPool _tabs;

	[SerializeField]
	private KScrollView _itemList;

	private int _currentTabIndex = -1;

	private List<WarpRushRanking.TabInfo> _tabInfos;

	protected override void OnAwake()
	{
		_itemList.Nodes.Init(delegate(GameObject go)
		{
			go.GetComponent<SupplyRewardNode>().Init();
		});
	}

	public void Set(List<WarpRushRanking.TabInfo> tabInfos)
	{
		_tabInfos = tabInfos;
		int width = ((tabInfos.Count > 0) ? (_tabContainer.width / tabInfos.Count) : 0);
		_tabs.BeginLoad();
		foreach (WarpRushRanking.TabInfo tabInfo in tabInfos)
		{
			IconTabWidget component = _tabs.GetNext().GetComponent<IconTabWidget>();
			component.Set(null, tabInfo.Revision.Name.ToString());
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnTabClicked));
			component.GetComponent<UIWidget>().width = width;
		}
		_tabs.EndLoad();
		UIUtility.WidgetsReposition(_tabs, _tabContainer, Vector3.right);
	}

	private void Select(int index)
	{
		for (int i = 0; i < _tabs.Count; i++)
		{
			IconTabWidget component = _tabs[i].GetComponent<IconTabWidget>();
			component.Selected = i == index;
		}
		_currentTabIndex = index;
	}

	protected override void FillData()
	{
		if (_currentTabIndex < 0 || _tabInfos.Count <= _currentTabIndex)
		{
			Select(0);
			return;
		}
		WarpRushRanking.TabInfo tabInfo = _tabInfos[_currentTabIndex];
		Dictionary<string, List<RankingReward>> dictionary = SingletonDict<Category, Dictionary<string, List<RankingReward>>>.Get(tabInfo.Category);
		if (dictionary == null)
		{
			return;
		}
		List<RankingReward> list = dictionary.Get(tabInfo.RevisionId);
		if (list != null)
		{
			_itemList.Nodes.BeginLoad();
			for (int i = 0; i < list.Count; i++)
			{
				RankingReward rankingReward = list[i];
				RankingReward prev = ((i != 0) ? list[i - 1] : null);
				bool isHighRank = i < 3;
				string rankingText = GetRankingText(prev, rankingReward, isHighRank);
				SupplyRewardNode component = _itemList.Nodes.GetNext().GetComponent<SupplyRewardNode>();
				component.SetNode(rankingText, rankingReward.GetRewards());
			}
			_itemList.Nodes.EndLoad();
			_itemList.UpdateLayout();
			UIUtility.UpdateAnchors(base.transform);
			_itemList.ResetPosition();
		}
	}

	private void OnTabClicked()
	{
		GameObject obj = Selectable.Current.gameObject;
		int num = _tabs.IndexOf(obj);
		if (num != -1)
		{
			Select(num);
			Refresh();
		}
	}

	private static string GetRankingText(RankingReward prev, RankingReward current, bool isHighRank)
	{
		string text = T._("{0}위", current.Ranking);
		if (prev == null)
		{
			string arg = ((current.Ranking != 0) ? text : current.RankingPecentage);
			return $"<em>[icon=crown] {arg}</em>";
		}
		if (prev.Ranking != 0)
		{
			int num = prev.Ranking + 1;
			if (current.Ranking != 0)
			{
				if (current.Ranking == num)
				{
					return (!isHighRank) ? text : $"<em>{text}</em>";
				}
				return string.Format((!isHighRank) ? "{0}~{1}" : "<em>{0}~{1}</em>", num, text);
			}
			return string.Format((!isHighRank) ? "{0}~{1}" : "<em>{0}~{1}</em>", T._("{0}위", num), current.RankingPecentage);
		}
		return string.Format((!isHighRank) ? "{0}~{1}" : "<em>{0}~{1}</em>", prev.RankingPecentage, current.RankingPecentage);
	}
}
