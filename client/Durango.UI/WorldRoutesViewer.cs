using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.UI.Control;
using JetBrains.Annotations;
using Messages;
using Shared.Region;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class WorldRoutesViewer : AnimationWidget
{
	public static readonly Biome[][] BiomeGrid = new Biome[6][]
	{
		new Biome[1] { Biome.Desert },
		new Biome[2]
		{
			Biome.TropicalForest,
			Biome.SwampMud
		},
		new Biome[1] { Biome.Grassland },
		new Biome[1],
		new Biome[2]
		{
			Biome.Tundra,
			Biome.SnowField
		},
		new Biome[1] { Biome.Volcanic }
	};

	private const float RandomPositionOffset = 10f;

	[SerializeField]
	private ExploreRegionNode _regionNodebase;

	[SerializeField]
	private ExploreAreaNode _areaNodeBase;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private WorldRoutesBeginnerStableArea _beginnerStableArea;

	[SerializeField]
	private WorldRoutesUnstableArea _beginnerUnstableArea;

	[SerializeField]
	private WorldRoutesExpertStableArea _expertStableArea;

	[SerializeField]
	private WorldRoutesUnstableArea _expertUnstableArea;

	[SerializeField]
	private SharedRoutesInfo _sharedRoutesInfo;

	[SerializeField]
	private RecentlyVisit _recentlyVisit;

	[SerializeField]
	private GameObject _currentCursor;

	[SerializeField]
	private UIPanel[] _backgroundPanels;

	[SerializeField]
	private WorldRoutesBackground _background;

	[SerializeField]
	private UISprite _pointSpriteBase;

	[SerializeField]
	private WorldRoutesViewerShadow _shadowSprite;

	[SerializeField]
	private TweenerPlayer _selectingEffect;

	[SerializeField]
	private UIWidget _sunsetEffects;

	[SerializeField]
	private SunsetClouds _sunsetClouds;

	private float _scrollOffset;

	private int _randomSeed;

	private bool _isValidCurrentCursor;

	private ListObjectPool<UISprite> _pointSprites;

	private EstateLicenses _estateLicenses;

	private PersonalRegionInfo _personalRegionInfo;

	private bool _resetViewerScroll;

	private bool _isInit;

	public ExploreRegionNode RegionNodeBase => _regionNodebase;

	public ExploreAreaNode AreaNodeBase => _areaNodeBase;

	public UIScrollView ScrollView => _scrollView.ScrollView;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			List<UIWidget> widgets = _scrollView.Widgets;
			int i = 0;
			for (int size = KUtility.GetSize(widgets); i < size; i++)
			{
				widgets[i].gameObject.SetActive(value: true);
			}
			_beginnerUnstableArea.Init(-1, 39);
			_expertUnstableArea.Init(40, -1);
			_pointSprites = new ListObjectPool<UISprite>();
			_pointSprites.BaseObject = _pointSpriteBase;
			_pointSprites.UseBase = true;
			_pointSprites.Clear();
			_regionNodebase.gameObject.SetActive(value: false);
			_areaNodeBase.gameObject.SetActive(value: false);
			_shadowSprite.Initialize(_background.GetHashCode());
		}
	}

	private void OnEnable()
	{
		_scrollOffset = -1f;
		_selectingEffect.gameObject.SetActive(value: false);
	}

	private void LateUpdate()
	{
		float currentOffset = _scrollView.CurrentOffset;
		if (_scrollOffset != currentOffset)
		{
			_scrollOffset = currentOffset;
			SyncOffset();
		}
	}

	private void SyncOffset()
	{
		float scrollOffset = _scrollOffset;
		Vector3 localPosition = Vector3.left * scrollOffset;
		for (int i = 0; i < _backgroundPanels.Length; i++)
		{
			_backgroundPanels[i].transform.localPosition = localPosition;
			_backgroundPanels[i].clipOffset = Vector2.right * scrollOffset;
		}
	}

	private void UpdateLayout()
	{
		List<UIWidget> widgets = _scrollView.Widgets;
		int i = 0;
		for (int size = KUtility.GetSize(widgets); i < size; i++)
		{
			widgets[i].height = base.Widget.height;
		}
		global::System.Random rand = new global::System.Random(_randomSeed);
		if (_beginnerStableArea.gameObject.activeSelf)
		{
			_beginnerStableArea.UpdateLayout(rand);
		}
		_beginnerUnstableArea.UpdateLayout(rand);
		if (_expertStableArea.gameObject.activeSelf)
		{
			_expertStableArea.UpdateLayout(rand);
		}
		_expertUnstableArea.UpdateLayout(rand);
		_scrollView.UpdateLayout();
		UpdateSunsetEffects();
		UpdateBackground();
		if (_resetViewerScroll)
		{
			_resetViewerScroll = false;
			if (_isValidCurrentCursor)
			{
				MoveScrollTo(_currentCursor.transform.position);
			}
		}
		ApplyShadow();
	}

	public void SelectExploreArea([CanBeNull] RegionTemplate template)
	{
		if (template == null)
		{
			_selectingEffect.gameObject.SetActive(value: false);
			return;
		}
		Transform unstableRoutesTransform = GetUnstableRoutesTransform(template.Role, template.MajorBiome(), template.Level);
		if (unstableRoutesTransform == null)
		{
			_selectingEffect.gameObject.SetActive(value: false);
			return;
		}
		MoveScrollTo(unstableRoutesTransform.position);
		_selectingEffect.transform.position = unstableRoutesTransform.position;
		_selectingEffect.gameObject.SetActive(value: true);
		_selectingEffect.Play();
	}

	private void MoveScrollTo(Vector3 position)
	{
		position = _scrollView.transform.InverseTransformPoint(position);
		Vector3 vector = position - _scrollView.GetBasePosition();
		float num = 0f;
		switch (_scrollView.Dir)
		{
		case KScrollViewBase.Direction.Vertical:
			num = Mathf.Abs(vector.y);
			break;
		case KScrollViewBase.Direction.Horizontal:
			num = Mathf.Abs(vector.x);
			break;
		}
		_scrollView.MoveTo(num - _scrollView.ViewLength * 0.5f, instant: true);
	}

	public void SetArchipelagoOnly(bool reset)
	{
		Init();
		_sharedRoutesInfo.gameObject.SetActive(value: true);
		_sharedRoutesInfo.Set(GameSystem<PartySystem>.Instance().GetLeaderInfo());
		_recentlyVisit.Set();
		_currentCursor.gameObject.SetActive(value: false);
		_beginnerStableArea.gameObject.SetActive(value: false);
		_beginnerUnstableArea.Set(riskyOnly: true);
		_expertStableArea.gameObject.SetActive(value: false);
		_expertUnstableArea.Set(riskyOnly: true);
		_randomSeed = GameManager.Region.Id.GetHashCode();
		_resetViewerScroll = reset;
		UpdateLayout();
		_pointSprites.Clear();
	}

	public void Set(bool reset)
	{
		Init();
		_sharedRoutesInfo.gameObject.SetActive(value: false);
		_recentlyVisit.Set();
		_isValidCurrentCursor = false;
		_beginnerStableArea.gameObject.SetActive(value: true);
		_expertStableArea.gameObject.SetActive(value: true);
		_beginnerStableArea.Set();
		_beginnerUnstableArea.Set();
		_expertStableArea.Set();
		_expertUnstableArea.Set();
		_randomSeed = GameManager.Region.Id.GetHashCode();
		_resetViewerScroll = reset;
		UpdateLayout();
		if (!_isValidCurrentCursor)
		{
			_currentCursor.gameObject.SetActive(value: false);
		}
		EstateSystem.GetEstateLicenses(delegate(EstateLicenses licenses)
		{
			_estateLicenses = licenses;
			EstateSystem.GetPersonalRegionInfo(delegate(PersonalRegionInfo info)
			{
				_personalRegionInfo = info;
				_beginnerStableArea.SetPersonal(info.PersonalRegion);
				RefreshRegionPoint();
			});
		});
	}

	public void Show(float duration, float delay)
	{
		base.Delay = delay;
		base.Duration = duration;
		base.Alpha = 1f;
	}

	public void Hide(float duration)
	{
		base.Duration = duration;
		base.Alpha = 0f;
	}

	private void UpdateSunsetEffects()
	{
		if (_scrollView.Widgets.Count > 0)
		{
			_sunsetEffects.transform.localPosition = _scrollView.Widgets[0].transform.localPosition;
		}
		_sunsetEffects.width = (int)_scrollView.ContentsLength;
		_sunsetEffects.height = base.Widget.height;
		UIUtility.UpdateAnchors(_sunsetEffects.transform);
		_sunsetClouds.ArrangeRandomClouds();
	}

	private void UpdateBackground()
	{
		Point2 point = new Point2((int)_scrollView.ContentsLength + 16, base.Widget.height);
		Vector3 localPosition = Vector3.left * (_scrollView.ViewLength * 0.5f + 8f);
		_background.SetDimensions(point.x, point.y);
		_background.transform.localPosition = localPosition;
		_shadowSprite.SetDimensions(point.x, point.y);
		_shadowSprite.transform.localPosition = localPosition;
	}

	public void SetCurrentCursor(Transform node)
	{
		_currentCursor.transform.parent = node;
		_currentCursor.transform.localPosition = Vector3.zero;
		_currentCursor.gameObject.SetActive(value: true);
		_isValidCurrentCursor = true;
	}

	private void RefreshRegionPoint()
	{
		_pointSprites.BeginLoad();
		_beginnerStableArea.ProcessRegionNode(RefreshRegionPoint);
		_expertStableArea.ProcessRegionNode(RefreshRegionPoint);
		_pointSprites.EndLoad();
	}

	private void RefreshRegionPoint(Transform node, [CanBeNull] string regionId)
	{
		if (!string.IsNullOrEmpty(regionId))
		{
			int num = 0;
			EntityTile? homePoint = GameSystem<MapSystem>.Instance().Points.HomePoint;
			if (regionId == ((!homePoint.HasValue) ? GameSystem<MapSystem>.Instance().Points.ReturningPoint.Region : homePoint.Value.Region).Id)
			{
				DrawRegionPoint(node, "icon_map_home", num++);
			}
			if (_personalRegionInfo.PersonalEstate.HasValue && _personalRegionInfo.PersonalRegion.HasValue && _personalRegionInfo.PersonalRegion.Value.Region.Id == regionId)
			{
				DrawRegionPoint(node, "icon_map_domain", num++);
			}
			if (_estateLicenses.UrbanEstate.HasValue && _estateLicenses.UrbanEstate.Value.RegionId == regionId)
			{
				DrawRegionPoint(node, "icon_map_domain", num++);
			}
			if (_estateLicenses.ClanEstate.HasValue && _estateLicenses.ClanEstate.Value.RegionId == regionId)
			{
				DrawRegionPoint(node, "icon_map_clan", num++);
			}
		}
	}

	private void DrawRegionPoint(Transform node, string sprite, int index)
	{
		Vector2 zero = Vector2.zero;
		switch (index)
		{
		default:
			return;
		case 0:
			zero = new Vector2(-1f, 1f);
			break;
		case 1:
			zero = new Vector2(1f, 1f);
			break;
		case 2:
			zero = new Vector2(-1f, -1f);
			break;
		case 3:
			zero = new Vector2(1f, -1f);
			break;
		}
		UISprite next = _pointSprites.GetNext();
		next.spriteName = sprite;
		next.MakePixelPerfect();
		Vector2 vector = new Vector2(40f, 40f) + Vector2.Scale(zero, new Vector2(next.width, next.height) * 0.5f);
		vector = next.transform.parent.InverseTransformPoint(node.TransformPoint(vector));
		next.SetPosition(vector, 0.5f, 0.5f);
	}

	private void ApplyShadow()
	{
		float shadowOffset = _expertUnstableArea.GetShadowOffset();
		float num;
		if (shadowOffset > 0f)
		{
			num = (float)_expertUnstableArea.GetComponent<UIWidget>().width - shadowOffset;
		}
		else
		{
			num = _expertUnstableArea.GetComponent<UIWidget>().width;
			if (!ExploreSystem.ReadyToUrbanExplore)
			{
				num += (float)_expertStableArea.GetComponent<UIWidget>().width;
				shadowOffset = _beginnerUnstableArea.GetShadowOffset();
				num += (float)_beginnerUnstableArea.GetComponent<UIWidget>().width - shadowOffset;
			}
		}
		_shadowSprite.Set(400f, num);
	}

	public static Vector2 GetRandomPositionOffset(global::System.Random rand)
	{
		return new Vector2(Mathf.Lerp(-10f, 10f, (float)rand.NextDouble()), Mathf.Lerp(-10f, 10f, (float)rand.NextDouble()));
	}

	[CanBeNull]
	public Transform GetIslandTransform(Role role, Biome biome, int level)
	{
		Transform transform = _beginnerStableArea.FindRegionNode(role);
		if (transform != null)
		{
			return transform;
		}
		ExploreRegionNode exploreRegionNode = _expertStableArea.FindRegionNode(role, biome, level);
		return (!(exploreRegionNode != null)) ? null : exploreRegionNode.transform;
	}

	[CanBeNull]
	public Transform GetUnstableRoutesTransform(Role role, Biome biome, int level)
	{
		ExploreAreaNode exploreAreaNode = _beginnerUnstableArea.FindRoutesArea(role, biome, level);
		if (exploreAreaNode != null)
		{
			return exploreAreaNode.transform;
		}
		exploreAreaNode = _expertUnstableArea.FindRoutesArea(role, biome, level);
		return (!(exploreAreaNode != null)) ? null : exploreAreaNode.transform;
	}
}
