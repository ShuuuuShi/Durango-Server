using System;
using Durango.UI.Control;
using Messages;
using Shared.Laboratory;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ResearchTierWidget : MonoBehaviour
{
	[SerializeField]
	private KGridScrollView _researchList;

	private LaboratoryTier? _tier;

	public LaboratoryTier Tier => _tier.GetValueOrDefault(LaboratoryTier.Invalid);

	public event Action<string, int?> ResearchClicked;

	public void Init()
	{
		_researchList.Nodes.Init(delegate(GameObject obj)
		{
			ResearchNodeWidget component = obj.GetComponent<ResearchNodeWidget>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnResearchClick));
		});
	}

	private void OnResearchClick()
	{
		ResearchNodeWidget researchNodeWidget = Selectable.Current as ResearchNodeWidget;
		if (!(researchNodeWidget == null) && this.ResearchClicked != null)
		{
			this.ResearchClicked(researchNodeWidget.Key, researchNodeWidget.PioneerGrade);
		}
	}

	public void Set(AvailablePersonalResearch research, LaboratoryTier tier, string selectedResearch)
	{
		bool flag = !GetComponent<UIWidget>().isVisible || tier != Tier;
		_tier = tier;
		_researchList.Nodes.BeginLoad();
		foreach (Pair<string, int?> item2 in research.ResearchableIds())
		{
			string item = item2.Item1;
			PersonalResearch personalResearch = SingletonDict<string, PersonalResearch>.Get(item);
			if (personalResearch != null && personalResearch.Tier == tier)
			{
				ResearchNodeWidget component = _researchList.Nodes.GetNext().GetComponent<ResearchNodeWidget>();
				component.Set(item, item2.Item2, personalResearch);
				component.Selected = item == selectedResearch;
			}
		}
		_researchList.Nodes.EndLoad();
		_researchList.Reposition(flag, !flag);
	}
}
