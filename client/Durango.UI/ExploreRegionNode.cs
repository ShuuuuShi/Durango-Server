using System;
using Durango.Logic;
using Durango.Logic.Explore;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;

namespace Durango.UI;

public class ExploreRegionNode : ExploreNode<ExploreRegionNode>
{
	[SerializeField]
	private UISprite _regionSprite;

	private Vector3 _baseNamePosition;

	private bool _isInit;

	public Route Route { get; private set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_baseNamePosition = _nameLabel.transform.localPosition;
		}
	}

	public void Set(Route route, Func<Durango.Logic.Explore.Region, bool> bigIslandFilter = null)
	{
		Route = route;
		_Set(route.Region(), bigIslandFilter);
	}

	public void SetCurrent()
	{
		Durango.Logic.Explore.Region region = GameManager.Region;
		Route = new Route
		{
			RegionId = region.Id
		};
		_Set(region);
	}

	private void _Set([NotNull] Durango.Logic.Explore.Region region, Func<Durango.Logic.Explore.Region, bool> bigIslandFilter = null)
	{
		Init();
		string arg = region.Name;
		string arg2 = LocalizeUtil.FormatLevel(region.Level);
		_nameLabel.text = $"{arg}\n{arg2}".Trim();
		RoutesViewer.RegionLayout regionLayout = RoutesViewer.BiomeLayouts.Get(region.MajorBiome());
		string spriteName;
		if ((bigIslandFilter != null && bigIslandFilter(region)) || region.Id == GameManager.Region.Id)
		{
			spriteName = "explore_map_myisland";
			_nameLabel.transform.localPosition = _baseNamePosition + Vector3.down * 20f;
		}
		else
		{
			spriteName = "explore_map_island";
			_nameLabel.transform.localPosition = _baseNamePosition;
		}
		_regionSprite.spriteName = spriteName;
		_regionSprite.MakePixelPerfect();
		_regionSprite.color = regionLayout.Color;
		regionLayout.Sprite.Set(_biomeSprite);
		SeasonUtil.SetSmallIcon(_seasonEmblem, (region.Template != null) ? region.Template.Season : null);
		_newMarker.gameObject.SetActive(region.IsNew());
		if (_storyEmblem != null)
		{
			_storyEmblem.SetActive(value: false);
		}
	}

	public void SetUnknown()
	{
		Init();
		_nameLabel.text = ((!ExploreSystem.ReadyToUrbanExplore) ? string.Empty : T._("발견가능"));
		_nameLabel.transform.localPosition = _baseNamePosition;
		RoutesViewer.RegionLayout regionLayout = RoutesViewer.BiomeLayouts.Get(Biome.Invalid);
		_regionSprite.spriteName = "explore_map_island";
		_regionSprite.MakePixelPerfect();
		_regionSprite.color = regionLayout.Color;
		regionLayout.Sprite.Set(_biomeSprite);
		SeasonUtil.SetSmallIcon(_seasonEmblem, null);
		_newMarker.gameObject.SetActive(value: false);
		if (_storyEmblem != null)
		{
			_storyEmblem.SetActive(value: false);
		}
	}

	protected override void SetMarkers()
	{
		if (!Route.IsUnknownRoute())
		{
			ShowPartyMembersMarker(GameSystem<PartySystem>.Instance().FindMembersInRegion(Route.RegionId));
		}
	}
}
