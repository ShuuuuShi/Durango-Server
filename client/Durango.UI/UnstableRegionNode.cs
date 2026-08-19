using System;
using Durango.Logic;
using Durango.Logic.Explore;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;

namespace Durango.UI;

public class UnstableRegionNode : ExploreNode<UnstableRegionNode>
{
	private const string UnknownIslandIconName = "explore_map_unstable_island";

	private static readonly string[] BigIslandIconNames = new string[3] { "explore_map_unstable_island_preset_big_01", "explore_map_unstable_island_preset_big_02", "explore_map_unstable_island_preset_big_03" };

	private static readonly string[] SmallIslandIconNames = new string[5] { "explore_map_unstable_island_preset_01", "explore_map_unstable_island_preset_02", "explore_map_unstable_island_preset_03", "explore_map_unstable_island_preset_04", "explore_map_unstable_island_preset_05" };

	[SerializeField]
	private UISprite _regionSprite;

	[SerializeField]
	private UIMaskedSprite _coOpSprite;

	[SerializeField]
	private GameObject _coOpInfo;

	[SerializeField]
	private RectLayoutComponent _topMarkers;

	[SerializeField]
	private GameObject _missionStateMarker;

	[SerializeField]
	private GameObject _warpSiloSprite;

	public Route Route { get; private set; }

	protected override void SetMarkers()
	{
		if (!string.IsNullOrEmpty(Route.RegionId))
		{
			ShowPartyMembersMarker(GameSystem<PartySystem>.Instance().FindMembersInRegion(Route.RegionId));
			_topMarkers.UpdateLayout();
		}
	}

	public void Set(Route route, bool hasMission = false, [CanBeNull] string coOpIcon = null, bool isFirst = false, global::System.Random random = null, bool isStory = false, bool isSilo = false)
	{
		SetCoOp(coOpIcon);
		Route = route;
		Durango.Logic.Explore.Region region = route.Region();
		string arg = region.Name;
		string arg2 = LocalizeUtil.FormatLevel(region.Level);
		_nameLabel.text = $"{arg}\n{arg2}".Trim();
		if (isSilo)
		{
			_biomeSprite.color = new Color32(108, 44, 19, 200);
			_biomeSprite.spriteName = "img_unstable_index";
			_regionSprite.color = new Color32(169, 88, 57, byte.MaxValue);
			_regionSprite.spriteName = "explore_map_unstable_island_preset_big_01";
		}
		else
		{
			RoutesViewer.RegionLayout regionLayout = RoutesViewer.BiomeLayouts.Get(region.MajorBiome());
			regionLayout.Sprite.Set(_biomeSprite);
			_regionSprite.color = regionLayout.Color;
			_regionSprite.spriteName = ((random == null) ? "explore_map_unstable_island" : ((!isFirst) ? SmallIslandIconNames.Random(random) : BigIslandIconNames.Random(random)));
		}
		_regionSprite.MakePixelPerfect();
		_biomeSprite.MakePixelPerfect();
		SetMarkers();
		SeasonUtil.SetSmallIcon(_seasonEmblem, (region.Template != null) ? region.Template.Season : null);
		_newMarker.gameObject.SetActive(region.IsNew());
		if (_storyEmblem != null)
		{
			_storyEmblem.SetActive(isStory);
		}
		_warpSiloSprite.SetActive(isSilo);
		SetMissionStateMarker(hasMission);
	}

	public void SetEmpty()
	{
		SetCoOp(null);
		_nameLabel.text = string.Empty;
		RoutesViewer.RegionLayout regionLayout = RoutesViewer.BiomeLayouts.Get(Biome.Invalid);
		_biomeSprite.spriteName = string.Empty;
		_biomeSprite.color = regionLayout.Sprite.color;
		SetDefault("explore_map_unstable_island", regionLayout.Color);
	}

	public void SetUnknown()
	{
		SetCoOp(null);
		_nameLabel.text = T._("발견가능");
		RoutesViewer.RegionLayout regionLayout = RoutesViewer.BiomeLayouts.Get(Biome.Invalid);
		regionLayout.Sprite.Set(_biomeSprite);
		SetDefault("explore_map_unstable_island", regionLayout.Color);
	}

	public void SetLocked([CanBeNull] string coOpIcon, global::System.Random random)
	{
		SetCoOp(coOpIcon);
		_nameLabel.text = "???";
		RoutesViewer.RegionLayout regionLayout = RoutesViewer.BiomeLayouts.Get(Biome.Invalid);
		regionLayout.Sprite.Set(_biomeSprite);
		SetDefault(SmallIslandIconNames.Random(random), regionLayout.Color);
	}

	private void SetCoOp([CanBeNull] string spriteName)
	{
		if (string.IsNullOrEmpty(spriteName))
		{
			_coOpInfo.SetActive(value: false);
			return;
		}
		_coOpInfo.SetActive(value: true);
		_coOpSprite.spriteName = spriteName;
		_coOpSprite.MaskedSprite = "target_masking_bg";
	}

	private void SetMissionStateMarker(bool hasMission)
	{
		_missionStateMarker.SetActive(hasMission);
		_topMarkers.UpdateLayout();
	}

	private void SetDefault(string regionIcon, Color regionColor)
	{
		_biomeSprite.MakePixelPerfect();
		_regionSprite.color = regionColor;
		_regionSprite.spriteName = regionIcon;
		_regionSprite.MakePixelPerfect();
		SeasonUtil.SetSmallIcon(_seasonEmblem, null);
		_newMarker.SetActive(value: false);
		if (_storyEmblem != null)
		{
			_storyEmblem.SetActive(value: false);
		}
		_warpSiloSprite.SetActive(value: false);
		SetMissionStateMarker(hasMission: false);
	}

	[ExposedInEditor(null)]
	private void ShowDummyCoOpIcon()
	{
		SetCoOp("target_skunkodus");
	}
}
