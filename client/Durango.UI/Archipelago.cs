using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class Archipelago : MonoBehaviour, IScreenResizeReceiver
{
	private const float Buffer = 0.1f;

	private const float Padding = 0.22f;

	private const float PivotBottom = 0.24f;

	private const float PivotTop = 0.62f;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private UnstableRegionNode _baseNode;

	[SerializeField]
	private List<UISprite> _subIslands;

	[SerializeField]
	private GameObject _currentCursor;

	private ListObjectPool<UnstableRegionNode> _nodes;

	private RegionTemplate _template;

	private string _currentCoOpId;

	private bool _isInit;

	public ArchipelagoRoute ArchipelagoRoute { get; private set; }

	public bool HasAnyMission { get; private set; }

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		RefreshWidget();
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_nodes = new ListObjectPool<UnstableRegionNode>
			{
				BaseObject = _baseNode,
				UseBase = false
			};
			RefreshWidget();
		}
	}

	public void RefreshWidget()
	{
		UIWidget component = GetComponent<UIWidget>();
		_scrollView.panel.clipSoftness = Vector2.right * ((!UIManager.IsPortraitWidget(base.gameObject)) ? 40f : 5f);
		component.width = Mathf.FloorToInt(_scrollView.panel.width - _scrollView.panel.clipSoftness.x * 2f);
		component.height = (int)_scrollView.panel.height;
		Vector4 baseClipRegion = _scrollView.panel.baseClipRegion;
		_scrollView.panel.baseClipRegion = default(Vector4);
		_scrollView.panel.baseClipRegion = baseClipRegion;
	}

	public void SetRegion(RegionTemplate template, Route[] routes)
	{
		Init();
		ArchipelagoRoute = new ArchipelagoRoute
		{
			IncludedRoutes = routes
		};
		ResetSubIslands();
		_template = template;
		_currentCoOpId = null;
		_nodes.BeginLoad();
		if (KUtility.GetSize(routes) > 0)
		{
			foreach (Route route in routes)
			{
				UnstableRegionNode next = _nodes.GetNext();
				next.Set(route);
				next.Clicked = OnNodeClick;
			}
		}
		else
		{
			UnstableRegionNode next2 = _nodes.GetNext();
			next2.SetUnknown();
			next2.Clicked = OnUnknownNodeClick;
		}
		_nodes.EndLoad();
		UpdateLayout();
	}

	public void SetArchipelago(ArchipelagoRoute archipelagoRoute)
	{
		Init();
		ArchipelagoRoute = archipelagoRoute;
		ResetSubIslands();
		_currentCoOpId = null;
		HasAnyMission = false;
		Route[] includedRoutes = ArchipelagoRoute.IncludedRoutes;
		Messages.Archipelago archipelago = GameSystem<ExploreSystem>.Instance().GetArchipelago(ArchipelagoRoute.ArchipelagoId);
		if (includedRoutes == null || archipelago.IncludedRegions == null)
		{
			_nodes.Set(1);
			UnstableRegionNode component = _nodes[0].GetComponent<UnstableRegionNode>();
			if (archipelagoRoute.IsAllConditionSatisfied())
			{
				component.SetUnknown();
			}
			else
			{
				component.SetEmpty();
			}
			component.Clicked = OnUnknownArchipelagoClick;
			SetSubIslands();
			UpdateLayout();
			return;
		}
		global::System.Random random = new global::System.Random(archipelago.Id.GetHashCode());
		_nodes.BeginLoad();
		ArchipelagoRegionInfo? archipelagoRegionInfo = null;
		for (int i = 0; i < includedRoutes.Length; i++)
		{
			Route route = includedRoutes[i];
			ArchipelagoRegionInfo[] includedRegions = archipelago.IncludedRegions;
			ArchipelagoRegionInfo value = includedRegions.FirstOrDefault((ArchipelagoRegionInfo info) => info.Id == route.RegionId);
			RegionCoOpTodo regionCoOpTodo = value.CoOpList.FirstOrDefault((RegionCoOpTodo todo) => todo.Notice.HasValue);
			string coOpIcon = null;
			if (regionCoOpTodo.Notice.HasValue)
			{
				coOpIcon = regionCoOpTodo.Notice.Value.Item1;
				_currentCoOpId = regionCoOpTodo.CoOpId;
			}
			UnstableRegionNode next = _nodes.GetNext();
			if (archipelagoRegionInfo.HasValue && archipelagoRegionInfo.Value.Progess < 100)
			{
				next.SetLocked(coOpIcon, random);
				next.Clicked = delegate
				{
					if (archipelagoRoute.IsEpic)
					{
						UIManager.SystemMsg(T._("아직 갈 수 없는 섬입니다."));
					}
					else
					{
						UIManager.SystemMsg(T._("이전 단계의 개척 임무를 완료하면 입장할 수 있습니다."));
					}
				};
			}
			else
			{
				bool flag = (i == 0 && 0 < value.Progess && value.Progess < 100) || (i > 0 && archipelagoRegionInfo.HasValue && archipelagoRegionInfo.Value.Progess == 100 && value.Progess != 100);
				bool isStory = !string.IsNullOrEmpty(archipelagoRoute.EpicRegionId) && archipelagoRoute.EpicRegionId == route.RegionId;
				bool isSilo = !string.IsNullOrEmpty(archipelagoRoute.EpicWarpSiloRegionId) && archipelagoRoute.EpicWarpSiloRegionId == route.RegionId;
				next.Set(route, flag, coOpIcon, i == 0, random, isStory, isSilo);
				next.Clicked = OnNodeClick;
				HasAnyMission = HasAnyMission || flag;
			}
			archipelagoRegionInfo = value;
		}
		_nodes.EndLoad();
		UpdateLayout();
	}

	private void ResetSubIslands()
	{
		foreach (UISprite subIsland in _subIslands)
		{
			subIsland.gameObject.SetActive(value: false);
		}
	}

	private void SetSubIslands()
	{
		int num = UnityEngine.Random.Range(2, 4);
		Color color = RoutesViewer.BiomeLayouts.Get(Biome.Invalid).Color;
		for (int i = 0; i < num; i++)
		{
			UISprite uISprite = _subIslands.Random();
			uISprite.color = color;
			uISprite.gameObject.SetActive(value: true);
		}
	}

	private void UpdateLayout()
	{
		_currentCursor.gameObject.SetActive(value: false);
		int count = _nodes.Count;
		if (count < 1)
		{
			return;
		}
		if (count == 1)
		{
			_nodes[0].transform.localPosition = Vector3.zero;
			return;
		}
		string archipelagoId = ArchipelagoRoute.ArchipelagoId;
		global::System.Random random = new global::System.Random((!string.IsNullOrEmpty(archipelagoId)) ? archipelagoId.GetHashCode() : GetHashCode());
		float num = (0.56f - 0.1f * (float)count) / (float)count;
		for (int i = 0; i < count; i++)
		{
			float num2 = 0.22f + (float)i * (0.1f + num) + ((i <= 0) ? 0f : 0.1f);
			float pivotRight = num2 + num;
			Vector3 randomPosition = GetRandomPosition(random, num2, pivotRight);
			UnstableRegionNode unstableRegionNode = _nodes[i];
			unstableRegionNode.transform.localPosition = randomPosition;
			if (unstableRegionNode.Route.RegionId == GameManager.Region.Id)
			{
				_currentCursor.transform.localPosition = randomPosition;
				_currentCursor.gameObject.SetActive(value: true);
			}
		}
	}

	private Vector3 GetRandomPosition(global::System.Random random, float pivotLeft, float pivotRight)
	{
		UIWidget component = GetComponent<UIWidget>();
		Vector3 localPosition = component.GetLocalPosition(pivotLeft, 0.24f);
		Vector3 localPosition2 = component.GetLocalPosition(pivotRight, 0.62f);
		float x = Mathf.Lerp(localPosition.x, localPosition2.x, (float)random.NextDouble());
		float y = Mathf.Lerp(localPosition.y, localPosition2.y, (float)random.NextDouble());
		return new Vector3(x, y);
	}

	private void OnNodeClick(UnstableRegionNode node)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		string notice = null;
		RegionCoOp regionCoOp = RegionCoOpDict.GetRegionCoOp(node.Route.Region().TemplateId, _currentCoOpId);
		if (regionCoOp != null)
		{
			notice = regionCoOp.Subject;
		}
		UIManager.FindScript<ExploreGroup>().ShowRouteInfoTooltip(node.Route, notice);
	}

	private void OnUnknownArchipelagoClick(UnstableRegionNode node)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		UIManager.FindScript<ExploreGroup>().ShowUnknownArchipelagoInfoTooltip(ArchipelagoRoute);
	}

	private void OnUnknownNodeClick(UnstableRegionNode _)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		UIManager.FindScript<ExploreGroup>().ShowUnknownRouteInfoTooltip(_template.Role, _template.Id);
	}

	public Transform GetIslandTransform()
	{
		return (_nodes.Count >= 1) ? _nodes[0].transform : null;
	}
}
