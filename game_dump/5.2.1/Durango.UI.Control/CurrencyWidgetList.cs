using System;
using System.Collections.Generic;
using Durango.UI.Popup;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI.Control;

public class CurrencyWidgetList : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private CurrencyWidget_PC _baseNode;

	[SerializeField]
	private UIWidget _walletPopupButton;

	[SerializeField]
	private GameObject _walletFolded;

	[SerializeField]
	private GameObject _walletUnfolded;

	[SerializeField]
	[Tooltip("배경 왼쪽 마진")]
	private int _leftMargin;

	[SerializeField]
	[Tooltip("배경 오른쪽 마진")]
	private int _rightMargin;

	[SerializeField]
	[Tooltip("화폐 사이 마진")]
	private int _currencyMargin;

	private readonly ListObjectPool<CurrencyWidget_PC> _widgetList = new ListObjectPool<CurrencyWidget_PC>();

	private readonly List<UIWidget> _activeList = new List<UIWidget>();

	public event Action<bool> Opened;

	void IUIInitializable.Init()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_walletPopupButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			_walletFolded.SetActive(value: false);
			_walletUnfolded.SetActive(value: true);
			WalletInfoPopup walletInfoPopup = UIManager.Popup.Tooltip<WalletInfoPopup>();
			walletInfoPopup.AddOnFinished(delegate
			{
				_walletFolded.SetActive(value: true);
				_walletUnfolded.SetActive(value: false);
			});
			walletInfoPopup.Show(base.gameObject.transform, Vector2.down * 60f);
		});
		_widgetList.BaseObject = _baseNode;
		_widgetList.UseBase = false;
	}

	public void Add(IEnumerable<CurrencyData> currencies)
	{
		foreach (CurrencyData currency in currencies)
		{
			if (currency.CurrencyType != Currency.Invalid)
			{
				Add(currency.CurrencyType);
			}
			else if (currency.IsSkillPoint)
			{
				AddSkillPoint();
			}
		}
	}

	public void Remove(IEnumerable<CurrencyData> currencies)
	{
		foreach (CurrencyData currency in currencies)
		{
			if (currency.CurrencyType != Currency.Invalid)
			{
				Remove(currency.CurrencyType);
			}
			else if (currency.IsSkillPoint)
			{
				RemoveSkillPoint();
			}
		}
	}

	private void Add(Currency currency)
	{
		GetWidget(currency).ReferenceCount++;
		Refresh();
	}

	private void Remove(Currency currency)
	{
		CurrencyWidget_PC widget = GetWidget(currency);
		widget.ReferenceCount = ((widget.ReferenceCount != 0) ? (widget.ReferenceCount - 1) : 0);
		Refresh();
	}

	private void AddSkillPoint()
	{
		GetSkillWidget().ReferenceCount++;
		Refresh();
	}

	private void RemoveSkillPoint()
	{
		CurrencyWidget_PC skillWidget = GetSkillWidget();
		skillWidget.ReferenceCount = ((skillWidget.ReferenceCount != 0) ? (skillWidget.ReferenceCount - 1) : 0);
		Refresh();
	}

	private CurrencyWidget_PC GetWidget(Currency currency)
	{
		UIManager.SystemMsg("GetWidget");
		for (int i = 0; i < _widgetList.Count; i++)
		{
			if (_widgetList[i].CurrencyType == currency)
			{
				return _widgetList[i];
			}
		}
		CurrencyWidget_PC currencyWidget_PC = MakeWidget();
		currencyWidget_PC.SetCurrencyType(currency);
		return currencyWidget_PC;
	}

	private CurrencyWidget_PC GetSkillWidget()
	{
		UIManager.SystemMsg("GetSkillWidget");
		for (int i = 0; i < _widgetList.Count; i++)
		{
			if (_widgetList[i].IsSkillPoint)
			{
				return _widgetList[i];
			}
		}
		CurrencyWidget_PC currencyWidget_PC = MakeWidget();
		currencyWidget_PC.SetSkillPoint();
		return currencyWidget_PC;
	}

	private CurrencyWidget_PC MakeWidget()
	{
		UIManager.SystemMsg("MakeWidget");
		CurrencyWidget_PC currencyWidget_PC = _widgetList.Add();
		currencyWidget_PC.LayoutUpdated = (Action)Delegate.Combine(currencyWidget_PC.LayoutUpdated, new Action(Reposition));
		return currencyWidget_PC;
	}

	private void Refresh()
	{
		UpdateActiveList();
		Reposition();
		if (this.Opened != null)
		{
			this.Opened(_activeList.Count > 1);
		}
		GetComponent<TweenerPlayer>().Play();
	}

	private void UpdateActiveList()
	{
		_activeList.Clear();
		foreach (CurrencyWidget_PC widget in _widgetList)
		{
			if (widget.ReferenceCount > 0)
			{
				widget.gameObject.SetActive(value: true);
				UIWidget component = widget.GetComponent<UIWidget>();
				if (component != null)
				{
					_activeList.Add(component);
				}
			}
			else
			{
				widget.gameObject.SetActive(value: false);
			}
		}
		_activeList.Add(_walletPopupButton);
	}

	private void Reposition()
	{
		if (_activeList.Count == 0)
		{
			return;
		}
		int num = _leftMargin + _rightMargin + _currencyMargin * (_activeList.Count - 1);
		foreach (UIWidget active in _activeList)
		{
			num += active.width;
		}
		UIWidget component = GetComponent<UIWidget>();
		component.SetDimensions(num, component.height);
		UIUtility.WidgetsReposition(_activeList, component, Vector3.right, _currencyMargin, (float)_leftMargin / (float)num * -1f);
		component.UpdateAnchors();
	}
}
