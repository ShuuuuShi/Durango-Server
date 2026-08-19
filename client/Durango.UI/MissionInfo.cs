using Durango.Logic.Explore;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class MissionInfo : DiscoveryInfo
{
	public override void ShowUnknown()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Set([NotNull] ArchipelagoRegionInfo[] includedRegions)
	{
		int size = KUtility.GetSize(includedRegions);
		if (size == 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		int num = 0;
		ArchipelagoRegionInfo? archipelagoRegionInfo = null;
		_nodes.BeginLoad();
		for (int i = 0; i < includedRegions.Length; i++)
		{
			ArchipelagoRegionInfo value = includedRegions[i];
			num += value.Progess;
			Durango.Logic.Explore.Region region = GameSystem<ExploreSystem>.Instance().GetRegion(value.Id);
			bool isLocked = archipelagoRegionInfo.HasValue && archipelagoRegionInfo.Value.Progess < 100;
			archipelagoRegionInfo = value;
			DiscoverMissionNode component = _nodes.GetNext().GetComponent<DiscoverMissionNode>();
			component.Set(region.Name, value.Progess, isLocked);
		}
		_nodes.EndLoad();
		string countLabel = $"<em>{(float)num / ((float)size * 100f):P0}</em>";
		SetCountLabel(countLabel);
		float f = UIUtility.WidgetsReposition(_nodes, _nodesWidget, Vector3.down);
		_nodesWidget.height = Mathf.RoundToInt(f);
		_layout.UpdateLayout();
		base.gameObject.SetActive(value: true);
	}
}
