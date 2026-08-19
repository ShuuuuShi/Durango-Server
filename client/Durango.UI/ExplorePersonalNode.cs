using Durango.Logic;
using Durango.Logic.Explore;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ExplorePersonalNode : ExploreNode<ExplorePersonalNode>
{
	public Vector2 Position;

	public string ActivatedRegionId { get; private set; }

	protected override void SetMarkers()
	{
		if (ActivatedRegionId != null)
		{
			ShowPartyMembersMarker(GameSystem<PartySystem>.Instance().FindMembersInRegion(ActivatedRegionId));
		}
	}

	public void SetEmpty()
	{
		ActivatedRegionId = null;
		_newMarker.SetActive(value: true);
		_nameLabel.text = T._("선언 가능!");
		Clicked = delegate
		{
			UIManager.FindScript<EstateGroup>().Open(EstateTabWidget.Menu.Personal);
		};
	}

	public void Set(Messages.Region region)
	{
		ActivatedRegionId = region.Id;
		_newMarker.SetActive(value: false);
		RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(region.TemplateId);
		string arg = ((regionTemplate != null) ? LocalizeUtil.FormatLevel(regionTemplate.Level) : string.Empty);
		string arg2 = T._("{0}의 섬", region.Name);
		_nameLabel.text = $"{arg2}\n{arg}";
		Route route = new Route
		{
			Price = null,
			RegionId = region.Id
		};
		GameSystem<ExploreSystem>.Instance().AddRegion(new Durango.Logic.Explore.Region(region));
		Clicked = delegate
		{
			UIManager.FindScript<ExploreGroup>().ShowRouteInfoTooltip(route);
		};
		SetMarkers();
	}
}
