using System;
using System.Collections.Generic;
using Durango.Logic.Explore;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;

namespace Durango.UI;

public class WorldRoutesExpertStableArea : MonoBehaviour
{
	private const float BottomMargin = 100f;

	private const float TopMargin = 50f;

	private const float Area = 150f;

	private const float Interval = 120f;

	private ListObjectPool<ExploreRegionNode> _regions;

	private WorldRoutesViewer _parent;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_parent = UIUtility.FindComponentInParent<WorldRoutesViewer>(base.gameObject);
			_regions = new ListObjectPool<ExploreRegionNode>();
			_regions.BaseObject = base.gameObject.AddChild(_parent.RegionNodeBase.gameObject).GetComponent<ExploreRegionNode>();
			_regions.Init(delegate(ExploreRegionNode node)
			{
				node.Clicked = (Action<ExploreRegionNode>)Delegate.Combine(node.Clicked, new Action<ExploreRegionNode>(OnNodeClick));
			});
			_regions.Clear();
		}
	}

	private static void OnNodeClick(ExploreRegionNode node)
	{
		ExploreGroup exploreGroup = UIManager.FindScript<ExploreGroup>();
		if (node != null && node.Route.RegionId == GameManager.Region.Id)
		{
			return;
		}
		if (node == null || node.Route.IsUnknownRoute())
		{
			if (ExploreSystem.ReadyToUrbanExplore)
			{
				exploreGroup.ShowUnknownUrbanInfoTooltip();
				return;
			}
			UIManager.SystemMsg(T._("{0} 레벨부터 항해가능한 섬입니다", 30));
		}
		else
		{
			exploreGroup.ShowRouteInfoTooltip(node.Route);
		}
	}

	private static bool IsColdBiome(Biome biome)
	{
		int i = 0;
		for (int size = KUtility.GetSize(WorldRoutesViewer.BiomeGrid); i < size; i++)
		{
			Biome[] array = WorldRoutesViewer.BiomeGrid[i];
			int j = 0;
			for (int size2 = KUtility.GetSize(array); j < size2; j++)
			{
				if (array[j] == biome)
				{
					return i > 2;
				}
			}
		}
		return false;
	}

	public void Set()
	{
		Init();
		_regions.BeginLoad();
		if (GameManager.Region.Role() == Role.Urban)
		{
			ExploreRegionNode next = _regions.GetNext();
			next.SetCurrent();
			_parent.SetCurrentCursor(next.transform);
		}
		List<Route> urbanRoutes = GameSystem<ExploreSystem>.Instance().GetUrbanRoutes();
		int i = 0;
		for (int size = KUtility.GetSize(urbanRoutes); i < size; i++)
		{
			Route route = urbanRoutes[i];
			_regions.GetNext().Set(route);
		}
		_regions.EndLoad();
		if (_regions.Count == 0)
		{
			_regions.Set(3);
			for (int j = 0; j < 3; j++)
			{
				_regions[j].SetUnknown();
			}
		}
	}

	public void UpdateLayout(global::System.Random rand)
	{
		if (!_isInit)
		{
			return;
		}
		List<ExploreRegionNode> list = new List<ExploreRegionNode>();
		List<ExploreRegionNode> list2 = new List<ExploreRegionNode>();
		foreach (ExploreRegionNode region in _regions)
		{
			if (IsColdBiome(region.Route.Region().MajorBiome()))
			{
				list2.Add(region);
			}
			else
			{
				list.Add(region);
			}
		}
		float a = LocateNodes(rand, list2, isColdArea: true);
		float b = LocateNodes(rand, list, isColdArea: false);
		GetComponent<UIWidget>().width = (int)Mathf.Max(a, b);
		UIUtility.UpdateAnchors(base.transform);
	}

	private float LocateNodes(global::System.Random rand, List<ExploreRegionNode> nodes, bool isColdArea)
	{
		UIWidget component = GetComponent<UIWidget>();
		float num = (float)component.height * 0.5f;
		float leftMargin = 120f;
		float topMargin = num - 50f;
		foreach (ExploreRegionNode node in nodes)
		{
			Vector3 vector = ((!isColdArea) ? Vector3.zero : new Vector3(0f, num));
			Vector3 vector2 = GetRandomPosition(rand, leftMargin, topMargin) + vector;
			leftMargin = vector2.x + 120f;
			node.transform.localPosition = component.localCorners[0] + vector2;
		}
		return GetRandomPosition(rand, leftMargin, topMargin).x;
	}

	private static Vector3 GetRandomPosition(global::System.Random random, float leftMargin, float topMargin)
	{
		float b = leftMargin + 150f;
		float x = Mathf.Lerp(leftMargin, b, (float)random.NextDouble());
		float y = Mathf.Lerp(100f, topMargin, (float)random.NextDouble());
		return new Vector3(x, y);
	}

	public void ProcessRegionNode(Action<Transform, string> func)
	{
		if (_regions != null)
		{
			for (int i = 0; i < _regions.Count; i++)
			{
				func(_regions[i].transform, _regions[i].Route.RegionId);
			}
		}
	}

	public ExploreRegionNode FindRegionNode(Role role, Biome biome, int level)
	{
		for (int i = 0; i < _regions.Count; i++)
		{
			Durango.Logic.Explore.Region region = _regions[i].Route.Region();
			if (region != Durango.Logic.Explore.Region.UnknownRegion && region.Role() == role && region.Level == level && (biome == Biome.Invalid || region.MajorBiome() == biome))
			{
				return _regions[i];
			}
		}
		return null;
	}
}
