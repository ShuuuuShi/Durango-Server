using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using L10N;
using Shared.Region;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class RoutesViewer : MonoBehaviour, IUIInitializable
{
	public enum AreaType
	{
		Risky,
		RiskyUnlink,
		RiskyLock,
		Outpost,
		OutpostUnlink,
		OutpostLock,
		RiskyUnknown
	}

	[Serializable]
	[EnumType(typeof(Biome))]
	public class RegionBiomeLayouts : EnumKeyList
	{
		[SerializeField]
		private List<RegionLayout> _values;

		public RegionLayout Get(Biome biome)
		{
			int num = IndexOf((int)biome);
			if (num == -1)
			{
				return default(RegionLayout);
			}
			return _values[num];
		}
	}

	[Serializable]
	[EnumType(typeof(AreaType))]
	public class AreaTypeLayouts : EnumKeyList
	{
		[SerializeField]
		private List<AreaLayout> _values;

		public AreaLayout Get(AreaType type)
		{
			int num = IndexOf((int)type);
			if (num == -1)
			{
				return default(AreaLayout);
			}
			return _values[num];
		}
	}

	[Serializable]
	public struct RegionLayout
	{
		public SpriteData Sprite;

		public Color Color;
	}

	[Serializable]
	public struct AreaLayout
	{
		public SpriteData Sprite;

		public Color SpriteColor;

		public Color SpriteBgColor;

		public Vector2 SpriteSize;

		public SpriteData Background;

		public Vector2 BackgroundSize;

		public Vector2 BackgroundOffset;

		public float BackgroundRotateSpeed;

		public Color Color;
	}

	[SerializeField]
	private RegionBiomeLayouts _regionBiomeLayouts;

	[SerializeField]
	private AreaTypeLayouts _areaTypeLayouts;

	[SerializeField]
	private WorldRoutesViewer _worldViewer;

	[SerializeField]
	private UnstableRoutesViewer _unstableViewer;

	private RegionTemplate _selectedTemplate;

	private bool _isResetLayout = true;

	private bool _canGoBack = true;

	public static RegionBiomeLayouts BiomeLayouts { get; private set; }

	public static AreaTypeLayouts TypeLayouts { get; private set; }

	void IUIInitializable.Init()
	{
		InitializeStaticData();
		_worldViewer.gameObject.SetActive(value: true);
		_unstableViewer.gameObject.SetActive(value: true);
		ViewerReset();
	}

	public void InitializeStaticData()
	{
		BiomeLayouts = _regionBiomeLayouts;
		TypeLayouts = _areaTypeLayouts;
	}

	private void OnDisable()
	{
		ViewerReset();
	}

	public void ViewerReset()
	{
		_selectedTemplate = null;
		_worldViewer.SetAlpha(0f, useTween: false);
		_unstableViewer.SetAlpha(0f, useTween: false);
		_isResetLayout = true;
	}

	public bool Back()
	{
		if (!HasBack() || !_canGoBack)
		{
			return true;
		}
		_selectedTemplate = null;
		RefreshPage();
		return false;
	}

	public bool HasBack()
	{
		return _selectedTemplate != null;
	}

	public void OnLoad(ExploreGroup.RouteType routeType)
	{
		switch (routeType)
		{
		case ExploreGroup.RouteType.Normal:
			_worldViewer.Set(_isResetLayout);
			break;
		case ExploreGroup.RouteType.Shared:
			_worldViewer.SetArchipelagoOnly(_isResetLayout);
			break;
		case ExploreGroup.RouteType.Neighbor:
			_canGoBack = false;
			_selectedTemplate = GameManager.Region.Template;
			ShowUnstableRoutes();
			return;
		}
		_isResetLayout = false;
		RefreshPage();
	}

	private void RefreshPage()
	{
		if (_selectedTemplate == null)
		{
			ShowWorldRoutes();
		}
		else
		{
			ShowUnstableRoutes();
		}
	}

	private void ShowWorldRoutes()
	{
		_worldViewer.Show(0.2f, 0.2f);
		_unstableViewer.Hide(0.2f);
	}

	private void ShowUnstableRoutes()
	{
		_worldViewer.Hide(0.3f);
		_unstableViewer.Set(_selectedTemplate);
		_unstableViewer.Show(0.3f, 0.3f);
	}

	public void SelectUnstableRoutes([CanBeNull] RegionTemplate template)
	{
		if (template == null)
		{
			return;
		}
		bool flag = false;
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		flag = level >= template.AvailableLevel;
		if (!flag)
		{
			UIManager.SystemMsg(T._("{0} 레벨부터 항해 가능한 해역입니다", template.AvailableLevel));
		}
		if (flag && template.Role == Role.Outpost)
		{
			flag = GameSystem<ExploreSystem>.Instance().HasOutpostRoute(template);
			if (!flag)
			{
				UIManager.SystemMsg(T._("갈 수 없는 곳입니다."));
			}
		}
		_canGoBack = true;
		_selectedTemplate = ((!flag) ? null : template);
		RefreshPage();
	}

	[CanBeNull]
	public Transform GetIslandTransoform(Role role, Biome biome, int level)
	{
		bool flag = role == Role.Risky || role == Role.Outpost;
		if (_selectedTemplate == null)
		{
			return (!flag) ? _worldViewer.GetIslandTransform(role, biome, level) : _worldViewer.GetUnstableRoutesTransform(role, biome, level);
		}
		return (!flag) ? null : _unstableViewer.GetIslandTransform();
	}
}
