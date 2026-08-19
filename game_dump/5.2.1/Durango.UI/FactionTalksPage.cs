using System;
using System.Collections.Generic;
using Durango.Logic.Faction;
using Durango.UI.Control;
using NestedPrefab;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class FactionTalksPage : UIWidget, IUIInitializable
{
	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	[SerializeField]
	private FactionTalksList _talksList;

	[SerializeField]
	private FactionTalksViewer _talksViewer;

	private IconTabList _tabs;

	private FactionType _type;

	private readonly List<FactionType> _tabList = new List<FactionType>();

	private FactionGroup _parent;

	void IUIInitializable.Init()
	{
		_parent = GetComponentInParent<FactionGroup>();
		FactionTalksList talksList = _talksList;
		talksList.TalksClicked = (Action<Talks>)Delegate.Combine(talksList.TalksClicked, (Action<Talks>)delegate(Talks talks)
		{
			Show(_type, talks);
		});
		FactionTalksViewer talksViewer = _talksViewer;
		talksViewer.MoveToClicked = (Action<Talks>)Delegate.Combine(talksViewer.MoveToClicked, (Action<Talks>)delegate(Talks talks)
		{
			Show(_type, talks);
		});
		_tabs = _tabLinker.Object.GetComponent<IconTabList>();
		_tabs.Clicked += OnTabClick;
	}

	public void Refresh()
	{
		_tabList.Clear();
		_tabs.BeginLoad();
		for (int i = 0; i < FactionGroup.FactionOrder.Length; i++)
		{
			FactionType factionType = FactionGroup.FactionOrder[i];
			Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(factionType);
			if (faction != null && faction.Level != 0)
			{
				_tabList.Add(factionType);
				Yaml.Faction faction2 = SingletonDict<FactionType, Yaml.Faction>.Get(factionType);
				string text = ((faction2 != null) ? faction2.Name.ToString() : string.Empty);
				_tabs.Add(IconMap.Get(factionType), text);
			}
		}
		_tabs.EndLoad();
	}

	private void SelectTab(FactionType type)
	{
		_type = type;
		for (int i = 0; i < _tabList.Count; i++)
		{
			if (_tabList[i] == _type)
			{
				_tabs.Select(i);
				break;
			}
		}
	}

	public void Show(FactionType type, Talks talks)
	{
		base.gameObject.SetActive(value: true);
		SelectTab(type);
		Set(talks);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public bool Back()
	{
		if (_talksViewer.gameObject.activeSelf)
		{
			Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(_type);
			if (faction != null && faction.Level > 0)
			{
				_talksList.Show(faction);
				_talksViewer.Hide();
				return false;
			}
		}
		Hide();
		return true;
	}

	private void Set(Talks talks)
	{
		Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(_type);
		if (faction == null || faction.Level == 0)
		{
			_parent.Close();
		}
		else if (talks == null)
		{
			_talksList.Show(faction);
			_talksViewer.Hide();
		}
		else
		{
			_talksList.Hide();
			_talksViewer.Show(_type, talks);
			talks.IsRead = true;
		}
	}

	private void OnTabClick(int index)
	{
		if (index != -1)
		{
			Show(_tabList[index], null);
		}
	}
}
