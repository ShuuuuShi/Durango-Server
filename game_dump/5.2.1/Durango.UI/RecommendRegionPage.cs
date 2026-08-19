using System;
using Durango.Logic.Explore;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;

namespace Durango.UI;

public class RecommendRegionPage : MonoBehaviour
{
	[SerializeField]
	private UIWidget _emptyPage;

	[SerializeField]
	private UIWidget _mainPage;

	[SerializeField]
	private ExploreRegionNode _baseRegion;

	[SerializeField]
	private UIWidget[] _regionPositions;

	[SerializeField]
	private UIWidget _baseHelpLabel;

	[SerializeField]
	private SelectableWidget _nextRecommendButton;

	[SerializeField]
	private UIWidget _mapBackground;

	[SerializeField]
	private UnstableRoutesWaveSprite _leftWaveSprite;

	[SerializeField]
	private UnstableRoutesWaveSprite _rightWaveSprite;

	private ListObjectPool<ExploreRegionNode> _regions;

	private ListObjectPool _helpLabels;

	private bool _isWaitRecommendRoutes;

	private bool _isSuitableRegionRole;

	private bool _isInit;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		Role role = GameManager.Region.Role();
		_isSuitableRegionRole = role == Role.Rural || role == Role.Urban;
		_emptyPage.gameObject.SetActive(!_isSuitableRegionRole);
		_mainPage.gameObject.SetActive(_isSuitableRegionRole);
		if (_isSuitableRegionRole)
		{
			_regions = new ListObjectPool<ExploreRegionNode>();
			_regions.BaseObject = _baseRegion;
			_regions.UseBase = true;
			_regions.Init(delegate(ExploreRegionNode node)
			{
				node.Clicked = (Action<ExploreRegionNode>)Delegate.Combine(node.Clicked, new Action<ExploreRegionNode>(OnClickRouteNode));
			});
			_regions.Clear();
			_helpLabels = new ListObjectPool();
			_helpLabels.BaseObject = _baseHelpLabel.gameObject;
			_helpLabels.UseBase = true;
			_helpLabels.Init(delegate(GameObject obj)
			{
				UIEventListener uIEventListener = UIEventListener.Get(obj);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickHelpLabel));
			});
			_helpLabels.Clear();
			SelectableWidget nextRecommendButton = _nextRecommendButton;
			nextRecommendButton.Clicked = (Action)Delegate.Combine(nextRecommendButton.Clicked, new Action(NextRecommendStableRegions));
			_mapBackground.AddOnChange(delegate
			{
				int a = (int)((float)_mapBackground.width * 0.3f);
				_leftWaveSprite.rightAnchor.absolute = Mathf.Max(a, (int)((float)_leftWaveSprite.leftAnchor.absolute + _leftWaveSprite.MinimumWidth + 20f));
				_rightWaveSprite.leftAnchor.absolute = -Mathf.Max(a, (int)((float)(-_rightWaveSprite.rightAnchor.absolute) + _rightWaveSprite.MinimumWidth + 20f));
				_leftWaveSprite.UpdateAnchors();
				_rightWaveSprite.UpdateAnchors();
			});
		}
	}

	public void Show(bool refreshRegions)
	{
		Init();
		base.gameObject.SetActive(value: true);
		if (_isSuitableRegionRole)
		{
			_regions.Clear();
			_helpLabels.Clear();
			if (refreshRegions)
			{
				_isWaitRecommendRoutes = false;
				NextRecommendStableRegions();
			}
		}
	}

	private void NextRecommendStableRegions()
	{
		if (!_isWaitRecommendRoutes)
		{
			_isWaitRecommendRoutes = true;
			UIManager.Popup.LoadingRing.AttachToWidget(_mainPage.gameObject);
			_nextRecommendButton.Disabled = true;
			MapSystem.RecommendStableRegions(SetRoutes);
		}
	}

	private void SetRoutes(Route[] routes)
	{
		UIManager.Popup.LoadingRing.DetachFromWidget(_mainPage.gameObject);
		_nextRecommendButton.Disabled = false;
		_isWaitRecommendRoutes = false;
		_regions.BeginLoad();
		_helpLabels.BeginLoad();
		int size = KUtility.GetSize(_regionPositions);
		int num = 0;
		int i = 0;
		for (int size2 = KUtility.GetSize(routes); i < size2; i++)
		{
			Route route = routes[i];
			Vector3 position = _regionPositions[i].GetPosition(0f, 0f);
			Vector3 position2 = _regionPositions[i].GetPosition(1f, 1f);
			Vector3 vector = new Vector3(Mathf.Lerp(position.x, position2.x, UnityEngine.Random.value), Mathf.Lerp(position.y, position2.y, UnityEngine.Random.value));
			ExploreRegionNode next = _regions.GetNext();
			next.Set(route, (Durango.Logic.Explore.Region region) => region.Role() == Role.Urban);
			next.transform.localPosition = vector;
			GameObject next2 = _helpLabels.GetNext();
			UIWidget component = next2.GetComponent<UIWidget>();
			UILabel component2 = next2.transform.Find("Label").GetComponent<UILabel>();
			component2.text = route.Region().Role().GetName() + " [icon=icon_question_big]";
			component.width = (int)component2.printedSize.x + 10;
			UIUtility.UpdateAnchors(next2.transform);
			next2.transform.localPosition = vector + Vector3.up * 60f;
			num++;
			if (num >= size)
			{
				break;
			}
		}
		_regions.EndLoad();
		_helpLabels.EndLoad();
	}

	private void OnClickHelpLabel(GameObject obj)
	{
		int num = _helpLabels.IndexOf(obj);
		if (num >= 0 && num < _regions.Count)
		{
			string text = T._("사유지 확장에 제한이 없으며, 부족 영토를 선언할 수 있습니다. 무법섬으로 항해할 수 있습니다.");
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, text, 350);
			widgetTooltipControl.Sign = 1;
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show();
			UIWidget component = obj.GetComponent<UIWidget>();
			UISprite componentInChildren = obj.GetComponentInChildren<UISprite>();
			Vector3 position = new Vector3(componentInChildren.transform.position.x, component.worldCorners[2].y);
			position = widgetTooltipControl.transform.parent.InverseTransformPoint(position);
			widgetTooltipControl.UpdateArrowPosition(position);
		}
	}

	private static void OnClickRouteNode(ExploreRegionNode node)
	{
		if (!(node == null) && !node.Route.IsUnknownRoute())
		{
			RouteInfoTooltip.Show(UIManager.Popup.Tooltip<InfoTooltip>(), node.Route, OnTravelRegion);
		}
	}

	private static void OnTravelRegion(Route route)
	{
		GameSystem<MapSystem>.Instance().TravelToStableRegion(route.RegionId, route.Region().Role());
	}

	public Transform GetRegionNodeTransform(Role role)
	{
		if (_regions == null)
		{
			return null;
		}
		foreach (ExploreRegionNode region2 in _regions)
		{
			Durango.Logic.Explore.Region region = region2.Route.Region();
			if (region != Durango.Logic.Explore.Region.UnknownRegion && region.Role() == role)
			{
				return region2.transform;
			}
		}
		return null;
	}
}
