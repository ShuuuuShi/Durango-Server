using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Explore;
using Durango.Logic.Party;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ExploreAreaNode : ExploreNode<ExploreAreaNode>
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _decorationBg;

	[SerializeField]
	private GameObject _unstableMarker;

	[SerializeField]
	private ExploreAreaMissionMarker _missionMarker;

	public RegionTemplate Template { get; set; }

	protected override void SetMarkers()
	{
		if (GameSystem<ExploreSystem>.Instance().HasRoutes() && Template != null)
		{
			SetMarkersAndGetAreaType();
		}
	}

	public void Set()
	{
		RoutesViewer.AreaType iconsAndBackground = SetMarkersAndGetAreaType();
		SetIconsAndBackground(iconsAndBackground);
	}

	private RoutesViewer.AreaType SetMarkersAndGetAreaType()
	{
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		bool active = false;
		bool flag = false;
		bool flag2 = false;
		IEnumerable<Durango.Logic.Party.Member> enumerable = null;
		_missionMarker.ResetState();
		string arg;
		RoutesViewer.AreaType result;
		if (Template.Role == Role.Outpost)
		{
			Route[] outpostRoute = GameSystem<ExploreSystem>.Instance().GetOutpostRoute(Template);
			arg = T._("{0} 무법섬 해역", Template.ApparentClimate);
			if (KUtility.GetSize(outpostRoute) > 0)
			{
				result = RoutesViewer.AreaType.Outpost;
				Route[] array = outpostRoute;
				for (int i = 0; i < array.Length; i++)
				{
					Route route = array[i];
					IEnumerable<Durango.Logic.Party.Member> enumerable2 = GameSystem<PartySystem>.Instance().FindMembersInRegion(route.RegionId);
					enumerable = ((enumerable != null) ? enumerable.Concat(enumerable2) : enumerable2);
					Durango.Logic.Explore.Region region = route.Region();
					if (region.IsNew())
					{
						active = true;
					}
				}
			}
			else
			{
				result = ((level >= Template.AvailableLevel) ? RoutesViewer.AreaType.OutpostUnlink : RoutesViewer.AreaType.OutpostLock);
			}
		}
		else
		{
			bool flag3 = true;
			List<ArchipelagoRoute> archipelagoRoutes = GameSystem<ExploreSystem>.Instance().GetArchipelagoRoutes(Template.Level, Template.MajorBiome());
			foreach (ArchipelagoRoute item in archipelagoRoutes)
			{
				if (item.UnstableFactor > 1)
				{
					flag = true;
				}
				if (!string.IsNullOrEmpty(item.EpicRegionId))
				{
					flag2 = true;
				}
				Messages.Archipelago archipelago = GameSystem<ExploreSystem>.Instance().GetArchipelago(item.ArchipelagoId);
				if (KUtility.GetSize(item.IncludedRoutes) == 0 || archipelago.IncludedRegions == null)
				{
					_missionMarker.TrySetState(ExploreAreaMissionMarker.MissionState.NotInProgress);
					continue;
				}
				flag3 = false;
				int num = 0;
				int num2 = archipelago.IncludedRegions.Length * 100;
				ArchipelagoRegionInfo[] includedRegions = archipelago.IncludedRegions;
				for (int j = 0; j < includedRegions.Length; j++)
				{
					ArchipelagoRegionInfo archipelagoRegionInfo = includedRegions[j];
					num += archipelagoRegionInfo.Progess;
					IEnumerable<Durango.Logic.Party.Member> enumerable3 = GameSystem<PartySystem>.Instance().FindMembersInRegion(archipelagoRegionInfo.Id);
					enumerable = ((enumerable != null) ? enumerable.Concat(enumerable3) : enumerable3);
					Durango.Logic.Explore.Region region2 = GameSystem<ExploreSystem>.Instance().GetRegion(archipelagoRegionInfo.Id);
					if (region2.IsNew())
					{
						active = true;
						break;
					}
				}
				_missionMarker.TrySetState((num == 0) ? ExploreAreaMissionMarker.MissionState.NotInProgress : ((num != num2) ? ExploreAreaMissionMarker.MissionState.InProgress : ExploreAreaMissionMarker.MissionState.Completed));
			}
			arg = T._("{0} 해역", Template.ApparentClimate);
			result = (flag3 ? RoutesViewer.AreaType.RiskyUnknown : ((level < Template.AvailableLevel) ? RoutesViewer.AreaType.RiskyLock : RoutesViewer.AreaType.Risky));
		}
		_missionMarker.Show();
		_newMarker.SetActive(active);
		_unstableMarker.SetActive(!flag2 && flag);
		if (_storyEmblem != null)
		{
			_storyEmblem.SetActive(flag2);
		}
		ShowPartyMembersMarker(enumerable);
		_nameLabel.text = $"{arg}\n{LocalizeUtil.FormatLevel(Template.Level)}";
		SeasonUtil.SetSmallIcon(_seasonEmblem, Template.Season);
		return result;
	}

	private void SetIconsAndBackground(RoutesViewer.AreaType type)
	{
		Biome biome = Template.MajorBiome();
		RoutesViewer.RegionLayout regionLayout = RoutesViewer.BiomeLayouts.Get(biome);
		RoutesViewer.AreaLayout areaLayout = RoutesViewer.TypeLayouts.Get(type);
		if (!string.IsNullOrEmpty(areaLayout.Sprite.sprite))
		{
			areaLayout.Sprite.Set(_biomeSprite);
		}
		else
		{
			regionLayout.Sprite.Set(_biomeSprite);
		}
		Color color = Color.Lerp(_biomeSprite.color, areaLayout.SpriteColor, areaLayout.SpriteColor.a);
		color.a = _biomeSprite.color.a;
		_biomeSprite.color = color;
		Color color2 = Color.Lerp((!(areaLayout.Color.a > 0f)) ? regionLayout.Color : areaLayout.Color, areaLayout.SpriteBgColor, areaLayout.SpriteBgColor.a);
		color2.a = ((!(areaLayout.Color.a > 0f)) ? regionLayout.Color : areaLayout.Color).a;
		_background.color = color2;
		areaLayout.Background.Set(_decorationBg);
		if (areaLayout.SpriteSize == Vector2.zero)
		{
			_biomeSprite.MakePixelPerfect();
		}
		else
		{
			_biomeSprite.SetDimensions((int)areaLayout.SpriteSize.x, (int)areaLayout.SpriteSize.y);
		}
		if (areaLayout.BackgroundSize == Vector2.zero)
		{
			_decorationBg.MakePixelPerfect();
		}
		else
		{
			_decorationBg.SetDimensions((int)areaLayout.BackgroundSize.x, (int)areaLayout.BackgroundSize.y);
		}
		_decorationBg.transform.localPosition = areaLayout.BackgroundOffset;
		if (areaLayout.BackgroundRotateSpeed > 0f)
		{
			TweenRotation tweenRotation = TweenRotation.Begin(_decorationBg.gameObject, 1f / areaLayout.BackgroundRotateSpeed, Quaternion.identity);
			tweenRotation.from = Vector3.zero;
			tweenRotation.to = new Vector3(0f, 0f, -360f);
			tweenRotation.style = UITweener.Style.Loop;
			tweenRotation.tweenFactor = Random.value;
		}
		else
		{
			_decorationBg.SetEnable<TweenRotation>(enable: false);
			_decorationBg.transform.localEulerAngles = Vector3.zero;
		}
	}
}
