using System;
using System.Collections.Generic;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class ResistanceInfoPopup : TooltipBase
{
	[SerializeField]
	private ResistanceInfo _baseNode;

	[SerializeField]
	private UIWidget _header;

	[SerializeField]
	private GameObject _infoButton;

	[SerializeField]
	private UIWidget _items;

	private ListObjectPool<ResistanceInfo> _itemPool;

	protected override void OnAwake()
	{
		_itemPool = new ListObjectPool<ResistanceInfo>
		{
			BaseObject = _baseNode,
			UseBase = true
		};
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GameSystem<StatisticsSystem>.Instance().ResistanceExpCapUpdated += FillData;
		GameSystem<StatisticsSystem>.Instance().StatisticsUpdated += FillData;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GameSystem<StatisticsSystem>.Instance().ResistanceExpCapUpdated -= FillData;
		GameSystem<StatisticsSystem>.Instance().StatisticsUpdated -= FillData;
	}

	protected override void FillData()
	{
		StatisticsSystem statisticsSystem = GameSystem<StatisticsSystem>.Instance();
		Biome biome = GameManager.Region.MajorBiome();
		bool active = false;
		_itemPool.BeginLoad();
		foreach (KeyValuePair<Biome, Derived> item in Singleton<Constants>.Instance.Resistance.TypeByBiome)
		{
			Biome key = item.Key;
			Derived resistanceType = item.Value;
			string typeName = T._("신체 {0} 레벨", LocalizeUtil.Get(resistanceType));
			int resistanceLevel = statisticsSystem.GetResistanceLevel(resistanceType);
			string iconName = IconMap.Get(key);
			ResistanceExpCap resistanceExpCap = GameSystem<StatisticsSystem>.Instance().GetResistanceExpCap(resistanceType);
			Pair<int, int> currentAndMaxResistanceExp = statisticsSystem.GetCurrentAndMaxResistanceExp(resistanceType);
			_itemPool.GetNext().GetComponent<ResistanceInfo>().Set(key, typeName, iconName, resistanceLevel, currentAndMaxResistanceExp.Item1, currentAndMaxResistanceExp.Item2, resistanceExpCap.ExpRate, biome == key);
			if (key != biome)
			{
				continue;
			}
			active = true;
			UIEventListener uIEventListener = UIEventListener.Get(_header.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate(GameObject go)
			{
				SyncString title = new SyncString(delegate(out string text, out float period)
				{
					ResistanceExpCap resistanceExpCap2 = GameSystem<StatisticsSystem>.Instance().GetResistanceExpCap(resistanceType);
					SyncString.UpdateRemainTimeMsg(format: T._("신체 저항 레벨의 경험치는 일일 획득량이 제한됩니다.\n초기화까지 남은 시간 {0}"), endAt: resistanceExpCap2.ExpiresAt, text: out text, period: out period, expired: string.Empty);
				});
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set(title, null, null, 320);
				widgetTooltipControl.Direction = TooltipDirection.Horizontal;
				widgetTooltipControl.Sign = 1;
				widgetTooltipControl.Show(go, Vector2.zero, 10f);
			});
		}
		_infoButton.SetActive(active);
		_itemPool.EndLoad();
	}

	protected override void UpdateLayout()
	{
		float f = UIUtility.WidgetsReposition(_itemPool, _items, Vector3.down);
		GetComponent<RectLayoutComponent>().UpdateLayout(null, Mathf.RoundToInt(f) + _header.height);
	}
}
