using System;
using System.Collections.Generic;
using Messages;
using Shared.Faction;
using UnityEngine;

public class FactionHistoryContainer : FadeInOutContainer
{
	[SerializeField]
	private FactionListContainer _factionListContainer;

	[SerializeField]
	private FactionCommListContainer _factionCommListContainer;

	public event Action<FactionType, int> FactionCommListItemClicked
	{
		add
		{
			_factionCommListContainer.ListItemClicked += value;
		}
		remove
		{
			_factionCommListContainer.ListItemClicked -= value;
		}
	}

	public override void Init()
	{
		base.Init();
		_factionListContainer.Init();
		_factionCommListContainer.Init();
		GameSystem<FactionSystem>.Instance().FactionRecordUpdated += FactionHistoryContainerFactionRecordUpdated;
		_factionListContainer.SelectionChanged += _factionListContainer_SelectionChanged;
	}

	public void Refresh(IList<Faction?> factions)
	{
		_factionListContainer.Refresh(factions);
	}

	public void SetSelection(FactionType type)
	{
		_factionListContainer.SetSelection(type);
	}

	private void FactionHistoryContainerFactionRecordUpdated(FactionType type)
	{
		if (type == _factionCommListContainer.CurrentFaction)
		{
			_factionCommListContainer.Refresh(type);
		}
	}

	private void _factionListContainer_SelectionChanged(FactionType type)
	{
		_factionCommListContainer.Refresh(type);
	}
}
