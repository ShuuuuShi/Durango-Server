using System;
using System.Collections.Generic;
using ClanData;
using ItemSystem;
using K1Network;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;

public class ClanGroup : UIBase
{
	[Serializable]
	[EnumType(typeof(ClanMenus))]
	private class MenuPages : EnumKeyList
	{
		[SerializeField]
		private List<GameObject> _values;

		public List<GameObject> Values => _values;

		public int IndexOf(ClanMenus menu)
		{
			return IndexOf((int)menu);
		}
	}

	[SerializeField]
	private UITitleWidget _titleBar;

	[SerializeField]
	private MenuPages _menuPages;

	[SerializeField]
	private UIWidget _fundWidget;

	[SerializeField]
	private UISpriteLabel _fundMoneyLabel;

	[SerializeField]
	private DefaultSelectableButton _fundingButton;

	[SerializeField]
	private ClanMenuTabs _menuTabs;

	private ClanMenus _selectedMenu;

	private void Start()
	{
		_titleBar.OnClose += base.ForceClose;
		_titleBar.OnBack += Close;
		_menuTabs.Selected += OnSelectMenu;
		List<GameObject> values = _menuPages.Values;
		for (int i = 0; i < values.Count; i++)
		{
			if (!((Object)(object)values[i] == (Object)null))
			{
				AnimationWidget.Get(values[i], 0f, 0f, deactiveWhenFadeout: true);
			}
		}
		base.OnOpenSucceed += InitClanPages;
		GameSystem<ClanSystem>.Instance().ClanChanged += OnChangePlayerClan;
		DefaultSelectableButton fundingButton = _fundingButton;
		fundingButton.Clicked = (Action)Delegate.Combine(fundingButton.Clicked, new Action(OnClickFundingButton));
		base.OnClose();
	}

	private void SetClanCosts(Costs costs)
	{
		_fundWidget.alpha = 1f;
		int num = ((costs._Costs != null) ? costs._Costs.Get(Currency.TStone, 0) : 0);
		_fundMoneyLabel.text = string.Format("{0}  {1}", T._("부족 자금"), ItemSystem.Inventory.CurrencyFormat(num, Currency.TStone));
	}

	private void OnClickFundingButton()
	{
		UIManager.Popup.TextInput.Show(OnClanFunding, T._("얼마를 기부하시겠습니까?"));
	}

	private void OnClanFunding(string value)
	{
		if (!int.TryParse(value, out var fund))
		{
			return;
		}
		string comment = T._("부족에 <em>{0}</em>을 기부하시겠습니까?", ItemSystem.Inventory.CurrencyFormat(fund, Currency.TStone));
		UIManager.MessageBox.Show(comment, delegate(bool ok)
		{
			if (ok)
			{
				Dictionary<Currency, int> costs2 = new Dictionary<Currency, int> { [Currency.TStone] = fund };
				Connections.Frontend.Send(new Donate_Clan_Fund
				{
					Costs = costs2
				}).On(delegate(Costs costs, PacketHeader _)
				{
					SetClanCosts(costs);
				});
			}
		});
	}

	private void InitClanPages()
	{
		List<GameObject> values = _menuPages.Values;
		for (int i = 0; i < values.Count; i++)
		{
			if (!((Object)(object)values[i] == (Object)null))
			{
				values[i].GetComponent<AnimationWidget>().SetAlpha(0f, useTween: false);
				values[i].SetActive(false);
			}
		}
		((Component)_menuTabs).GetComponent<UIWidget>().alpha = 0f;
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		_selectedMenu = ((playerClan != null) ? ClanMenus.Info : ClanMenus.ClanList);
		ClanSystem.GetClanInfo(playerClan, ResponsePlayerClan, refresh: true);
		_fundWidget.alpha = 0f;
		ClanSystem.GetClanFund(SetClanCosts);
	}

	private void OnChangePlayerClan(ulong prev, ulong current)
	{
		ClanSystem.GetClanInfo(current, ResponsePlayerClan, refresh: true);
		ClanSystem.GetClanFund(SetClanCosts);
	}

	private void ResponsePlayerClan(Clan clan)
	{
		if (!base.IsOpen)
		{
			return;
		}
		TweenAlpha.Begin(((Component)_menuTabs).gameObject, 0.3f, 1f);
		ClanMenus[] array = ((clan == null) ? new ClanMenus[2]
		{
			ClanMenus.MakeClan,
			ClanMenus.ClanList
		} : new ClanMenus[5]
		{
			ClanMenus.Info,
			ClanMenus.Members,
			ClanMenus.Level,
			ClanMenus.Timeline,
			ClanMenus.ClanList
		});
		_menuTabs.Set(array);
		int num = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == _selectedMenu)
			{
				num = i;
				break;
			}
		}
		_menuTabs.SelectMenu((num != -1) ? array[num] : array[0]);
	}

	private void OnSelectMenu(ClanMenus menu)
	{
		_selectedMenu = menu;
		int num = _menuPages.IndexOf(menu);
		List<GameObject> values = _menuPages.Values;
		for (int i = 0; i < values.Count; i++)
		{
			if (!((Object)(object)values[i] == (Object)null))
			{
				AnimationWidget component = values[i].GetComponent<AnimationWidget>();
				if (i == num)
				{
					((Component)component).gameObject.SetActive(true);
					component.Delay = 0.2f;
					component.Duration = 0.3f;
					component.Alpha = 1f;
				}
				else
				{
					component.Delay = 0f;
					component.Duration = 0.3f;
					component.Alpha = 0f;
				}
			}
		}
	}
}
