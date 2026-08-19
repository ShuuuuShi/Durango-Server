using System;
using System.Collections.Generic;
using Messages;
using Shared.Faction;
using UnityEngine;

public class FactionSummaryContainer : FadeInOutContainer
{
	[SerializeField]
	private KScrollView _kScrollView;

	public event Action<FactionType> SummaryClicked;

	public override void Init()
	{
		base.Init();
		_kScrollView.Nodes.Init(delegate(GameObject gameObject)
		{
			FactionSummary component = gameObject.GetComponent<FactionSummary>();
			component.Init();
			UIEventListener.Get(gameObject).onClick = OnClickFactionSummary;
		});
	}

	public void Refresh(IList<Faction?> factions)
	{
		_kScrollView.Nodes.Clear();
		for (int i = 0; i < factions.Count; i++)
		{
			FactionSummary factionSummary = ((ListObjectPoolBase<GameObject>)_kScrollView.Nodes).Add<FactionSummary>();
			factionSummary.SetSummary(factions[i]);
		}
		_kScrollView.Reposition();
	}

	private void OnClickFactionSummary(GameObject obj)
	{
		FactionSummary component = obj.GetComponent<FactionSummary>();
		if ((Object)(object)component != (Object)null && component.IsActivated && this.SummaryClicked != null)
		{
			this.SummaryClicked(component.FactionType);
		}
	}
}
