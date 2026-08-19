using System;
using System.Collections.Generic;
using Messages;
using Shared.Faction;
using UnityEngine;

public class FactionListContainer : MonoBehaviour
{
	[SerializeField]
	private KScrollView _kScrollView;

	public event Action<FactionType> SelectionChanged;

	public void Init()
	{
		_kScrollView.Nodes.Init(delegate(GameObject gameObject)
		{
			UIEventListener.Get(gameObject).onClick = OnClickFactionListItem;
		});
	}

	public void Refresh(IList<Faction?> factions)
	{
		_kScrollView.Nodes.Clear();
		for (int i = 0; i < factions.Count; i++)
		{
			Faction? faction = factions[i];
			if (faction.HasValue)
			{
				FactionListItem factionListItem = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Add<FactionListItem>();
				factionListItem.SetFaction(faction.Value);
			}
		}
		_kScrollView.Reposition();
	}

	public void SetSelection(FactionType type)
	{
		RefreshSelectionStates(type);
		if (this.SelectionChanged != null)
		{
			this.SelectionChanged(type);
		}
	}

	private void OnClickFactionListItem(GameObject obj)
	{
		FactionListItem component = obj.GetComponent<FactionListItem>();
		if ((Object)(object)component != (Object)null)
		{
			SetSelection(component.FactionType);
		}
	}

	private void RefreshSelectionStates(FactionType currentFaction)
	{
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			FactionListItem factionListItem = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Get<FactionListItem>(i);
			factionListItem.IsSelected = factionListItem.FactionType == currentFaction;
		}
	}
}
