using System;
using System.Collections.Generic;
using Durango.Logic.Faction;
using Durango.UI.Control;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class FactionSummaryPage : KGridScrollView
{
	private readonly List<Durango.Logic.Faction.Faction> _factionOrders = new List<Durango.Logic.Faction.Faction>();

	[SerializeField]
	private FactionPortraits _factionPortraits;

	private bool _isShow;

	private bool _isInit;

	public event Action<FactionType> TalksClicked;

	public event Action<FactionType> SupportRequestClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		base.Nodes.Init(delegate(GameObject obj)
		{
			FactionSummary component = obj.GetComponent<FactionSummary>();
			component.TalksClicked += OnFactionTalksClicked;
			component.SupportRequestClicked += OnFactionSupportRequestClicked;
		});
		foreach (Durango.Logic.Faction.Faction faction in GameSystem<FactionSystem>.Instance().GetFactions())
		{
			_factionOrders.Add(faction);
		}
	}

	public void Refresh()
	{
		Init();
		_factionOrders.Sort(delegate(Durango.Logic.Faction.Faction f1, Durango.Logic.Faction.Faction f2)
		{
			if (f1.Level == 0 != (f2.Level == 0))
			{
				if (f1.Level == 0)
				{
					return 1;
				}
				if (f2.Level == 0)
				{
					return -1;
				}
			}
			int num = Array.IndexOf(FactionGroup.FactionOrder, f1.Type);
			int num2 = Array.IndexOf(FactionGroup.FactionOrder, f2.Type);
			return num - num2;
		});
		Vector2 viewSize = base.ViewSize;
		Point2 size = new Point2(viewSize);
		if (UIManager.IsPortraitWidget(base.gameObject))
		{
			size.y = 330;
			size.x = 660;
		}
		else
		{
			size.x = 320;
		}
		base.Nodes.BaseObject.GetComponent<UIWidget>().SetDimensions(size.x, size.y);
		base.Nodes.BeginLoad();
		for (int i = 0; i < _factionOrders.Count; i++)
		{
			Durango.Logic.Faction.Faction faction = _factionOrders[i];
			string text = SingletonDict<FactionType, Yaml.Faction>.Get(faction.Type)?.UnknownText ?? ((Gettext)null);
			if (faction.IsAvailable() || !string.IsNullOrEmpty(text))
			{
				FactionSummary component = base.Nodes.GetNext().GetComponent<FactionSummary>();
				PortraitMaterial portraitMaterial = _factionPortraits.Get(_factionOrders[i].Type);
				component.Set(_factionOrders[i], portraitMaterial.Material, portraitMaterial.Uv, text);
				component.UpdateLayout(size);
			}
		}
		base.Nodes.EndLoad();
		Reposition();
	}

	public void Show()
	{
		Init();
		if (!_isShow)
		{
			_isShow = true;
			base.gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		if (_isShow)
		{
			_isShow = false;
			base.gameObject.SetActive(value: false);
		}
	}

	public Transform GetSupportAvailableButtonTransform(bool containsPeriodFaction = false)
	{
		for (int i = 0; i < base.Nodes.Count; i++)
		{
			FactionSummary factionSummary = base.Nodes.Get<FactionSummary>(i);
			if (!(factionSummary == null) && factionSummary.Faction.HasAvailableSupportRequest() && (containsPeriodFaction || !(factionSummary.Faction.StartsAt > 0.0)))
			{
				SelectableButton supportButton = factionSummary.SupportButton;
				if (!supportButton.Disabled)
				{
					return supportButton.transform;
				}
			}
		}
		return null;
	}

	private void OnFactionTalksClicked(FactionSummary comp)
	{
		if (this.TalksClicked != null)
		{
			this.TalksClicked(comp.Faction.Type);
		}
	}

	private void OnFactionSupportRequestClicked(FactionSummary comp)
	{
		if (this.SupportRequestClicked != null)
		{
			this.SupportRequestClicked(comp.Faction.Type);
		}
	}
}
