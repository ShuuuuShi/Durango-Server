using System;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Region;
using Shared.System;
using UnityEngine;

namespace Durango.UI.Popup;

[RequireComponent(typeof(InfoTooltip))]
public class RouteInfoTooltip : MonoBehaviour
{
	private InfoTooltip _tooltip;

	private Route _route;

	private string _regionId;

	private bool _isUnknown;

	private string _notice;

	private double _destroyAt;

	private float _timeLabelUpdateAt;

	private int _timeLabelIndex;

	private Action<Route> _onClick;

	private int _allExplorables;

	private int _exploredCount;

	private InfoTooltip Tooltip
	{
		get
		{
			if (_tooltip == null)
			{
				_tooltip = GetComponent<InfoTooltip>();
			}
			return _tooltip;
		}
	}

	public bool IsVisible { get; private set; }

	public Transform ButtonTransform => Tooltip.Button.transform;

	private void Update()
	{
		if (_timeLabelIndex != -1 && _timeLabelUpdateAt > 0f && _timeLabelUpdateAt < Time.time)
		{
			UpdateTimerLabel();
		}
	}

	private void SetUnknown(Action<Route> onClick)
	{
		_isUnknown = true;
		_destroyAt = 0.0;
		_onClick = onClick;
		Refresh();
	}

	private void Set(Route route, Action<Route> onClick)
	{
		_route = route;
		_regionId = route.RegionId;
		_isUnknown = false;
		_destroyAt = _route.Region().DestroyAt;
		_onClick = onClick;
		MapSystem.GetPOICount(_regionId, OnPOICount);
		MapSystem.GetExploredPOICount(_regionId, OnExploredPOICount);
	}

	private void OnPOICount(POICount msg, PacketHeader header)
	{
		_allExplorables = msg.WarpholeCount + msg.CraterCount + msg.RiftCount;
		Refresh();
	}

	private void OnExploredPOICount(ExploredPOIs msg, PacketHeader header)
	{
		int i = 0;
		for (int size = KUtility.GetSize(msg.POIs); i < size; i++)
		{
			if (msg.POIs[i].Type == Shared.System.PointOfInterest.Warphole || msg.POIs[i].Type == Shared.System.PointOfInterest.CargoWarphole || msg.POIs[i].Type == Shared.System.PointOfInterest.Crater || msg.POIs[i].Type == Shared.System.PointOfInterest.Rift)
			{
				_exploredCount++;
			}
		}
		Refresh();
	}

	private void OnFinish()
	{
		UnityEngine.Object.Destroy(this);
	}

	private void Refresh()
	{
		if (_isUnknown)
		{
			Tooltip.SetTitle(T._("미지의 땅"));
			Tooltip.SetSubtitle(null);
		}
		else
		{
			string text = T._("{0:lv:} {1}", _route.Region().Level, LocalizeUtil.Get(_route.Region().Role()));
			string text2 = "[size=20][c][9F9784]" + text + "[-][/c][/size]\n" + _route.Region().Name;
			Tooltip.SetTitle(text2);
			Biome biome = _route.Region().MajorBiome();
			Tooltip.SetSubtitle(biome.GetName());
		}
		int num = 0;
		_timeLabelUpdateAt = num;
		if (UpdateTimerLabel())
		{
			num++;
		}
		else
		{
			_timeLabelIndex = -1;
		}
		Tooltip.SetNotice(_notice);
		if (_isUnknown)
		{
			Tooltip.SetButton(T._("찾기"), null, OnClickButton);
		}
		else
		{
			if (_allExplorables > 0)
			{
				Tooltip.SetInfo(num++, T._("발견한 탐험지점"), new KeyGaugeLabel.Gauge(_exploredCount, _allExplorables));
			}
			string regionId = ((_route.Region().Role() != Role.Urban) ? null : _route.RegionId);
			Money money = ((!_route.Price.HasValue) ? Money.ForFree : _route.Price.Value);
			string text3 = Durango.Logic.Item.Inventory.ToCurrencyButtonText(T._("항해"), money.Amount, money.Currency);
			Tooltip.SetButton(text3, regionId, OnClickButton);
		}
		if (!IsVisible)
		{
			IsVisible = true;
			_tooltip.AddOnFinished(OnFinish);
			Tooltip.Show(3600f);
		}
	}

	private void OnClickButton()
	{
		if (_onClick != null)
		{
			_onClick(_route);
		}
		Tooltip.Hide();
	}

	private bool UpdateTimerLabel()
	{
		if (_isUnknown)
		{
			return false;
		}
		if (_destroyAt > 0.0)
		{
			double num = _destroyAt - Connections.Frontend.GetPredictedServerTime();
			if (num > 0.0)
			{
				string text = ((!(num < 60.0)) ? TimedeltaFormatter.Format(num, 2, "min") : T._("곧 파괴됨"));
				Tooltip.SetInfo(_timeLabelIndex, (SyncString)T._("남은 수명"), (SyncString)text);
				_timeLabelUpdateAt = Time.time + 60f;
				return true;
			}
		}
		_timeLabelUpdateAt = 0f;
		return false;
	}

	public Role GetRouteRole()
	{
		if (_isUnknown)
		{
			return Role.Invalid;
		}
		return _route.Region().Role();
	}

	public static void ShowUnknown(InfoTooltip tooltip, Action<Route> onClick)
	{
		tooltip.gameObject.AddComponent<RouteInfoTooltip>().SetUnknown(onClick);
	}

	public static void Show(InfoTooltip tooltip, Route route, Action<Route> onClick, [CanBeNull] string notice = null)
	{
		RouteInfoTooltip routeInfoTooltip = tooltip.gameObject.AddComponent<RouteInfoTooltip>();
		routeInfoTooltip._notice = notice;
		routeInfoTooltip.Set(route, onClick);
	}
}
