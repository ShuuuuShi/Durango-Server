using System;
using ExploreData;
using ItemSystem;
using K1Network;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Region;
using Shared.System;
using TimerData;
using UnityEngine;

[RequireComponent(typeof(InfoTooltip))]
public class RouteInfoTooltip : MonoBehaviour
{
	private struct CountStruct
	{
		public int Value;

		public int Max;
	}

	private InfoTooltip _tooltip;

	private ExploreData.Route _route;

	private bool _isUnknownRegion;

	private double _destroyAt;

	private float _timeLabelUpdateAt;

	private int _timeLabelIndex;

	private Action<ExploreData.Route> _onClick;

	private CountStruct _warpholeCount;

	private CountStruct _warpmarkCount;

	private bool _isShow;

	private void Update()
	{
		if (_timeLabelIndex != -1 && _timeLabelUpdateAt > 0f && _timeLabelUpdateAt < Time.time)
		{
			UpdateTimerLabel();
		}
	}

	private void Set(ExploreData.Route route, Action<ExploreData.Route> onClick)
	{
		_route = route;
		_isUnknownRegion = route.Region.Id == 0;
		_destroyAt = ((!_isUnknownRegion) ? (_route.Region.CreatedAt + _route.Region.Template.expires_in) : 0.0);
		_onClick = onClick;
		_tooltip = ((Component)this).GetComponent<InfoTooltip>();
		ulong id = route.Region.Id;
		if (id != 0)
		{
			MapSystem.GetPOICount(id, OnPOICount);
			MapSystem.GetExploredPOICount(id, OnExploredPOICount);
		}
		else
		{
			Refresh();
		}
	}

	private void OnPOICount(POICount msg, PacketHeader header)
	{
		_warpholeCount.Max = msg.WarpholeCount;
		_warpmarkCount.Max = msg.CraterCount;
		Refresh();
	}

	private void OnExploredPOICount(ExploredPOIs msg, PacketHeader header)
	{
		int i = 0;
		for (int size = KUtility.GetSize(msg.POIs); i < size; i++)
		{
			switch (msg.POIs[i].Type)
			{
			case Shared.System.PointOfInterest.Warphole:
				_warpholeCount.Value++;
				break;
			case Shared.System.PointOfInterest.Crater:
				_warpmarkCount.Value++;
				break;
			}
		}
		Refresh();
	}

	private void OnFinish()
	{
		Object.Destroy((Object)(object)this);
	}

	private void Refresh()
	{
		_tooltip.SetTitle((!_isUnknownRegion) ? _route.Region.Name : T._("미지의 섬"));
		Biome biome = (Biome)_route.Region.MajorBiome();
		_tooltip.SetSubtitle($"{biome.GetName()} {LocalizeUtil.FormatLevel(_route.Region.Level)}");
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
		if (_route.Region.Id != 0)
		{
			if (_warpholeCount.Max > 0)
			{
				_tooltip.SetInfo(num++, T._("발견한 워프홀"), $"{_warpholeCount.Value} / {_warpholeCount.Max}");
			}
			if (_warpmarkCount.Max > 0)
			{
				_tooltip.SetInfo(num++, T._("조사한 크레이터"), $"{_warpmarkCount.Value} / {_warpmarkCount.Max}");
			}
		}
		if (_isUnknownRegion)
		{
			_tooltip.SetButton(T._("찾기"), OnClickButton);
		}
		else
		{
			_tooltip.SetButton(string.Format("{0}\n{1}", ItemSystem.Inventory.CurrencyFormat(_route.Price, Currency.TStone), T._("항해")), OnClickButton);
		}
		if (!_isShow)
		{
			_isShow = true;
			_tooltip.AddOnFinished(OnFinish);
			_tooltip.Show(3600f);
		}
	}

	private void OnClickButton()
	{
		if (!Selectable.Current.Disable)
		{
			if (_onClick != null)
			{
				_onClick(_route);
			}
			_tooltip.Hide();
		}
	}

	private bool UpdateTimerLabel()
	{
		if (_isUnknownRegion)
		{
			return false;
		}
		string empty = string.Empty;
		if (_destroyAt > 0.0)
		{
			double num = _destroyAt - Connections.Frontend.GetPredictedServerTime();
			if (num > 0.0)
			{
				if (num < 60.0)
				{
					empty = T._("곧 파괴됨");
				}
				else
				{
					string text = TimerSystem.TimeToString(num, TimePeriod.Min, 3);
					empty = T._("[icon=icon_skill_time] {0} 남음", text);
				}
				_tooltip.SetInfo(_timeLabelIndex, empty, null);
				_timeLabelUpdateAt = Time.time + 60f;
				return true;
			}
		}
		_timeLabelUpdateAt = 0f;
		return false;
	}

	public static void Show(InfoTooltip tooltip, ExploreData.Route route, Action<ExploreData.Route> onClick)
	{
		if (route != null)
		{
			RouteInfoTooltip routeInfoTooltip = ((Component)tooltip).gameObject.AddComponent<RouteInfoTooltip>();
			routeInfoTooltip.Set(route, onClick);
		}
	}
}
