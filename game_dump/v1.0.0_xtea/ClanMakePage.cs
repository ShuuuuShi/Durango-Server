using System;
using System.Collections.Generic;
using ItemSystem;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;

public class ClanMakePage : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _makeButtons;

	private bool _validCostData;

	private KeyValuePair<Currency, int>[] _costs;

	private KeyValuePair<Currency, int> _selectedCostType;

	private void Awake()
	{
		_makeButtons.Init(delegate(GameObject obj)
		{
			DefaultSelectableButton component = obj.GetComponent<DefaultSelectableButton>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickMakeButton));
		});
	}

	private void OnEnable()
	{
		_validCostData = false;
		UpdateButtonState();
		ClanSystem.GetClanMakeCost(OnMakeClanCost);
	}

	private void OnMakeClanCost(Costs costs)
	{
		_validCostData = true;
		if (_costs == null || _costs.Length != costs._Costs.Count)
		{
			_costs = new KeyValuePair<Currency, int>[costs._Costs.Count];
		}
		int num = 0;
		foreach (KeyValuePair<Currency, int> cost in costs._Costs)
		{
			_costs[num++] = cost;
		}
		UpdateButtonState();
	}

	private void UpdateButtonState()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		int num = (_validCostData ? _costs.Length : 0);
		_makeButtons.Set(num);
		for (int i = 0; i < num; i++)
		{
			KeyValuePair<Currency, int> keyValuePair = _costs[i];
			DefaultSelectableButton component = _makeButtons[i].GetComponent<DefaultSelectableButton>();
			Currency key = keyValuePair.Key;
			ItemSystem.Inventory playerInventory = GameSystem<InventorySystem>.Instance().PlayerInventory;
			long balance = playerInventory.GetBalance(key);
			long num2 = keyValuePair.Value;
			if (balance < num2)
			{
				component.Text = T._("<em>부족 창설</em>\n<alert>{0}</alert>", ItemSystem.Inventory.CurrencyFormat(num2, key));
				component.Disable = true;
			}
			else
			{
				component.Text = T._("<em>부족 창설</em>\n{0}", ItemSystem.Inventory.CurrencyFormat(num2, key));
				component.Disable = false;
			}
		}
		float num3 = _makeButtons.Reposition(Vector3.right, 5);
		Vector3 pos = _makeButtons.BaseObject.transform.localPosition + Vector3.left * num3 * 0.5f;
		for (int j = 0; j < _makeButtons.Count; j++)
		{
			UIWidget component2 = _makeButtons[j].GetComponent<UIWidget>();
			component2.SetPosition(pos, 0f, 0.5f);
			pos.x += (float)(component2.width + 5);
		}
	}

	private void OnClickMakeButton()
	{
		int num = _makeButtons.IndexOf(((Component)Selectable.Current).gameObject);
		KeyValuePair<Currency, int> selectedCostType = _costs[num];
		if (Selectable.Current.Disable)
		{
			UIManager.SystemMsg(T._("{0} 이 필요합니다", ItemSystem.Inventory.CurrencyFormat(selectedCostType.Value, selectedCostType.Key)));
		}
		else
		{
			_selectedCostType = selectedCostType;
			UIManager.Popup.TextInput.Show(OnSubmitClanName, T._("부족의 이름을 입력해주세요"));
		}
	}

	private void OnSubmitClanName(string clanName)
	{
		if (string.IsNullOrEmpty(clanName))
		{
			return;
		}
		Currency type = _selectedCostType.Key;
		long amount = _selectedCostType.Value;
		string comment = T._("<em>{0}</em> 부족을 {1} 을 사용하여 만드시겠습니까?", clanName, ItemSystem.Inventory.CurrencyFormat(amount, type));
		UIManager.MessageBox.Show(comment, delegate(bool ok)
		{
			if (ok)
			{
				ClanSystem.MakeClan(type, clanName);
			}
		});
	}
}
