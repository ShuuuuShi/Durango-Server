using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Clan;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ClanAllyPage : ClanMenuPage, IUIInitializable
{
	private enum AllyState
	{
		Ally,
		Suggested,
		BeenSuggested,
		BreakSuggested,
		BeenBreakSuggested,
		Lock,
		Empty,
		Sealed
	}

	[Serializable]
	[EnumType(typeof(AllyState))]
	private class AllyWidgetList : EnumKeyList
	{
		[SerializeField]
		private List<ObjectPool> _values;

		public List<ObjectPool> Values => _values;
	}

	[Serializable]
	private class ObjectPool
	{
		public ListObjectPool Pool;

		[HideInInspector]
		public int Count;
	}

	[SerializeField]
	private UILabel _allyCountLabel;

	[SerializeField]
	private KWidgetScrollView _allySlotList;

	[SerializeField]
	private AllyWidgetList _allyWidgetPool;

	private ClanGroup _parent;

	void IUIInitializable.Init()
	{
		_parent = GetComponentInParent<ClanGroup>();
		List<ObjectPool> values = _allyWidgetPool.Values;
		for (int i = 0; i < values.Count; i++)
		{
			switch ((AllyState)i)
			{
			case AllyState.Ally:
				values[i].Pool.Init(delegate(GameObject obj)
				{
					obj.GetComponent<ClanAllyWidget>().ButtonClicked = _parent.SuggestBreak;
				});
				break;
			case AllyState.Suggested:
				values[i].Pool.Init(delegate(GameObject obj)
				{
					obj.GetComponent<ClanAllyWidget>().ButtonClicked = _parent.SuggestAllyCancel;
				});
				break;
			case AllyState.BeenSuggested:
				values[i].Pool.Init(delegate(GameObject obj)
				{
					obj.GetComponent<ClanAllyWidget>().ButtonClicked = _parent.BeenSuggestedAlly;
				});
				break;
			case AllyState.BreakSuggested:
				values[i].Pool.Init(delegate(GameObject obj)
				{
					obj.GetComponent<ClanAllyWidget>().ButtonClicked = _parent.BreakAlly;
				});
				break;
			case AllyState.BeenBreakSuggested:
				values[i].Pool.Init(delegate(GameObject obj)
				{
					obj.GetComponent<ClanAllyWidget>().ButtonClicked = _parent.BeenBreakSuggestedAlly;
				});
				break;
			case AllyState.Empty:
				values[i].Pool.Init(delegate(GameObject obj)
				{
					obj.GetComponent<ClanAllyEmptyWidget>().ButtonClicked = OnClickEmptyWidget;
				});
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case AllyState.Lock:
			case AllyState.Sealed:
				break;
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GameSystem<ClanSystem>.Instance().AlliesUpdated += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		GameSystem<ClanSystem>.Instance().AlliesUpdated -= Refresh;
	}

	private void Refresh()
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan != null)
		{
			Set(playerClan, GameSystem<ClanSystem>.Instance().Allies);
		}
	}

	private void Set([NotNull] Clan clan, AllySlot[] slots)
	{
		int level = clan.Level;
		Ally ally = Singleton<Constants>.Instance.Ally;
		int num = ally.SlotOpensAt.Length;
		for (int i = 0; i < ally.SlotOpensAt.Length; i++)
		{
			if (ally.SlotOpensAt[i] > level)
			{
				num = i;
				break;
			}
		}
		int num2 = Mathf.Min(ally.MaxSlotCount, ally.DefaultSlotCount + num);
		int num3 = 0;
		int j = 0;
		for (int size = KUtility.GetSize(slots); j < size; j++)
		{
			if (slots[j].IsAlly)
			{
				num3++;
			}
		}
		_allyCountLabel.text = string.Format("{0}    <em>{1}</em> [FFFFFF7F]/[-] {2}", T._("동맹 현황"), num3, num2);
		Messages.Member clan2 = PlayerBehavior.LocalPlayer.Clan;
		bool hasPermission = clan.Id == clan2.ClanId && clan2.RoleId == 0;
		BeginLoad();
		int k = 0;
		for (int size2 = KUtility.GetSize(slots); k < size2; k++)
		{
			AllySlot slot = slots[k];
			UIWidget uIWidget = null;
			if (slot.State == AllySlotState.Locked)
			{
				uIWidget = AddSlot(AllyState.Lock);
				uIWidget.GetComponent<ClanAllyLockWidget>().Set(slot);
				continue;
			}
			if (slot.IsAlly)
			{
				switch (slot.State)
				{
				case AllySlotState.Solid:
					uIWidget = AddSlot(AllyState.Ally);
					break;
				case AllySlotState.Suggested:
					uIWidget = AddSlot(AllyState.BreakSuggested);
					break;
				case AllySlotState.BeenSuggested:
					uIWidget = AddSlot(AllyState.BeenBreakSuggested);
					break;
				}
			}
			else
			{
				switch (slot.State)
				{
				case AllySlotState.Suggested:
					uIWidget = AddSlot(AllyState.Suggested);
					break;
				case AllySlotState.BeenSuggested:
					uIWidget = AddSlot(AllyState.BeenSuggested);
					break;
				}
			}
			if (!(uIWidget == null))
			{
				uIWidget.GetComponent<ClanAllyWidget>().Set(slot, hasPermission);
			}
		}
		int l = 0;
		for (int size3 = KUtility.GetSize(slots); l < size3; l++)
		{
			AllySlot allySlot = slots[l];
			if (!allySlot.IsAlly && allySlot.State == AllySlotState.Solid)
			{
				AddSlot(AllyState.Empty).GetComponent<ClanAllyEmptyWidget>().Set(hasPermission);
			}
		}
		for (int m = num; m < ally.SlotOpensAt.Length; m++)
		{
			AddSlot(AllyState.Sealed).GetComponent<ClanAllySealedWidget>().Set(ally.SlotOpensAt[m]);
		}
		EndLoad();
	}

	private void BeginLoad()
	{
		_allySlotList.Widgets.Clear();
		for (int i = 0; i < _allyWidgetPool.Values.Count; i++)
		{
			_allyWidgetPool.Values[i].Count = 0;
		}
	}

	private void EndLoad()
	{
		for (int i = 0; i < _allyWidgetPool.Values.Count; i++)
		{
			ObjectPool objectPool = _allyWidgetPool.Values[i];
			objectPool.Pool.Set(objectPool.Count);
		}
		_allySlotList.Reposition();
	}

	private UIWidget AddSlot(AllyState state)
	{
		ObjectPool objectPool = _allyWidgetPool.Values[(int)state];
		UIWidget component = objectPool.Pool.GetOrAdd(objectPool.Count++).GetComponent<UIWidget>();
		_allySlotList.Widgets.Add(component);
		return component;
	}

	private void OnClickEmptyWidget()
	{
		_parent.SearchClansForAlliance();
	}
}
