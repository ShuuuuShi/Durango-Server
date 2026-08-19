using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI;

public class ClanMakePage : ClanMenuPage, IUIInitializable
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UIWidget[] _iconWidgets;

	[SerializeField]
	private SelectableButton _makeButton;

	[SerializeField]
	private ListObjectPool _iconSeperator;

	private ListObjectPool<SelectableButton> _buttons = new ListObjectPool<SelectableButton>();

	private bool _validCostData;

	private KeyValuePair<Currency, long>[] _costs;

	private KeyValuePair<Currency, long> _selectedCostType;

	void IUIInitializable.Init()
	{
		_buttons.BaseObject = _makeButton;
		_buttons.UseBase = true;
		_buttons.Init(delegate(SelectableButton btn)
		{
			if (_buttons.BaseObject != btn)
			{
				btn.Widget.SetAnchor((Transform)null);
			}
			btn.Clicked = (Action)Delegate.Combine(btn.Clicked, new Action(OnClickMakeButton));
		});
		UIManager.AddOnScreenResized(OnScreenResize);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_validCostData = false;
		UpdateButtonState();
		ClanSystem.GetClanMakeCost(OnMakeClanCost);
	}

	private void OnMakeClanCost(Costs costs)
	{
		_validCostData = true;
		if (_costs == null || _costs.Length != costs._Costs.Count)
		{
			_costs = new KeyValuePair<Currency, long>[costs._Costs.Count];
		}
		int num = 0;
		foreach (KeyValuePair<Currency, long> cost in costs._Costs)
		{
			_costs[num++] = cost;
		}
		UpdateButtonState();
	}

	private void UpdateButtonState()
	{
		int num = (_validCostData ? _costs.Length : 0);
		_buttons.Set(num);
		for (int i = 0; i < num; i++)
		{
			KeyValuePair<Currency, long> keyValuePair = _costs[i];
			SelectableButton selectableButton = _buttons[i];
			Currency key = keyValuePair.Key;
			long balance = InventorySystem.Wallet.GetBalance(key);
			long value = keyValuePair.Value;
			selectableButton.Text = string.Format((balance >= value) ? "{0} {1}" : "{0} <alert>{1}</alert>", T._("부족 창설"), Durango.Logic.Item.Inventory.CurrencyFormat(value, key));
		}
		_buttons.Reposition(Vector3.right, 5);
	}

	private void OnClickMakeButton()
	{
		int num = _buttons.IndexOf((SelectableButton)Selectable.Current);
		KeyValuePair<Currency, long> selectedCostType = _costs[num];
		if (InventorySystem.Wallet.GetBalance(selectedCostType.Key) < selectedCostType.Value)
		{
			UIManager.SystemMsg(T._("{0} 이 필요합니다", Durango.Logic.Item.Inventory.CurrencyFormat(selectedCostType.Value, selectedCostType.Key)));
		}
		else
		{
			_selectedCostType = selectedCostType;
			UIManager.Popup.Tooltip<TextInputPopup>().Show(OnSubmitClanName, T._("부족의 이름을 입력해주세요"));
		}
	}

	private void OnSubmitClanName(string clanName)
	{
		if (string.IsNullOrEmpty(clanName))
		{
			return;
		}
		Currency type = _selectedCostType.Key;
		long value = _selectedCostType.Value;
		string mainText = T._("<em>{0}</em> 부족을 {1} 을 사용하여 만드시겠습니까?", clanName, Durango.Logic.Item.Inventory.CurrencyFormat(value, type));
		UIManager.MessageBox.Show(mainText, delegate(bool ok)
		{
			if (ok)
			{
				ClanSystem.MakeClan(type, clanName);
			}
		});
	}

	private void OnScreenResize()
	{
		bool flag = UIManager.IsPortraitWidget(base.gameObject);
		_label.alignment = ((!flag) ? NGUIText.Alignment.Left : NGUIText.Alignment.Center);
		float num = (float)base.Widget.height / ((!flag) ? 620f : 926f);
		int num2 = (int)((float)((!flag) ? 45 : 70) * num);
		int num3 = ((!flag) ? 4 : 2);
		int num4 = _iconWidgets.Length / num3;
		int width = _iconWidgets[0].width;
		int height = _iconWidgets[0].height;
		int num5 = (flag ? 46 : 0);
		int num6 = (int)(50f * num);
		int num7 = (int)(_label.printedSize.y + (float)(num2 * 2) + (float)(num6 * (num4 - 1)) + (float)(height * num4) + (float)_makeButton.Widget.height);
		int num8 = (base.Widget.height - num7) / 2;
		_label.transform.localPosition = new Vector3((!flag) ? 60 : ((int)(((float)base.Widget.width - _label.printedSize.x) / 2f)), -num8);
		int num9 = (flag ? ((base.Widget.width - (width * num3 + num5 * (num3 - 1))) / 2) : 0);
		int num10 = (int)(_label.transform.localPosition.y - _label.printedSize.y - (float)num2);
		_iconSeperator.Clear();
		for (int i = 0; i < _iconWidgets.Length; i++)
		{
			int num11 = i / num3;
			int num12 = i % num3;
			_iconWidgets[i].transform.localPosition = new Vector3(num9 + num12 * (width + num5), num10 - num11 * height - num6 * num11, 0f);
			if (i % num3 != 0)
			{
				_iconSeperator.Add().transform.localPosition = _iconWidgets[i].transform.localPosition - Vector3.right * ((float)num5 / 2f - 1f);
			}
		}
		_makeButton.transform.localPosition = new Vector3((!flag) ? 60 : ((int)((float)(base.Widget.width - _makeButton.Widget.width) / 2f)), _iconWidgets[_iconWidgets.Length - 1].transform.localPosition.y - (float)height - (float)num2);
	}
}
