using System.Collections.Generic;
using Durango.Logic.Faction;
using Durango.Logic.Notification;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using NestedPrefab;
using Shared.Faction;
using UnityEngine;

namespace Durango.UI;

public class FactionSupportRequestPage : UIWidget
{
	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	[SerializeField]
	private FactionSupportRequestList _requestList;

	private IconTabList _tabList;

	private readonly List<FactionType> _tabValues = new List<FactionType>();

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tabList = _tabLinker.Object.GetComponent<IconTabList>();
			_tabList.Clicked += OnTabSelected;
		}
	}

	private void RefreshTab()
	{
		_tabValues.Clear();
		_tabList.BeginLoad();
		for (int i = 0; i < FactionGroup.FactionOrder.Length; i++)
		{
			FactionType factionType = FactionGroup.FactionOrder[i];
			Faction f = GameSystem<FactionSystem>.Instance().GetFaction(factionType);
			if (f == null || !f.IsAvailable() || !f.HasSupportRequest())
			{
				continue;
			}
			int count = _tabValues.Count;
			bool on = false;
			_tabValues.Add(factionType);
			string icon = IconMap.Get(f.Type);
			SyncString text2;
			if (f.EndsAt > 0.0)
			{
				text2 = new SyncString(delegate(out string text, out float period)
				{
					double num = f.EndsAt - Connections.Frontend.GetPredictedServerTime();
					if (num > 0.0)
					{
						text = T._("{0} 남음", TimedeltaFormatter.Format(num, 1, "min"));
						period = (float)(num % (double)TimedeltaFormatter.CurrentMinUnit());
					}
					else
					{
						text = string.Empty;
						period = 0f;
					}
				});
			}
			else if (f.HasAvailableSupportRequest())
			{
				text2 = T._("지원 가능!");
				on = true;
			}
			else
			{
				text2 = new SyncString(delegate(out string text, out float period)
				{
					SyncString.UpdateRemainTimeMsg(f.SupportRequestAvailableAt, "[icon=icon_timer] {0}", out text, out period, string.Empty);
				});
			}
			_tabList.Add(icon, text2);
			_tabList.SetNotification(count, on, Type.Normal);
		}
		_tabList.EndLoad();
	}

	private void SelectTab(FactionType type)
	{
		int index = _tabValues.IndexOf(type);
		_tabList.Select(index);
	}

	public void Refresh()
	{
		Init();
		RefreshTab();
		_requestList.Refresh();
	}

	public void Show(FactionType type)
	{
		Init();
		_tabList.ScrollView.ResetPosition();
		ShowPage(type);
		base.gameObject.SetActive(value: true);
	}

	private void ShowPage(FactionType type)
	{
		SelectTab(type);
		_requestList.Set(type);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnTabSelected(int index)
	{
		if (index >= 0 && index < _tabValues.Count)
		{
			ShowPage(_tabValues[index]);
		}
	}

	public Transform GetRequestAvailableButtonTransform()
	{
		return _requestList.GetRequestAvailableButtonTransform();
	}
}
