using System;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PriceInputWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject _priceContainer;

	[SerializeField]
	private UILabel _priceInput;

	[SerializeField]
	private SelectableWidget _clearBtn;

	[SerializeField]
	private UILabel _taxTooltipLabel;

	[SerializeField]
	private UITweener _insertAlarm;

	private long _price;

	public event Action PriceChanged;

	public void Init()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_priceContainer);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickPriceInput));
		SelectableWidget clearBtn = _clearBtn;
		clearBtn.Clicked = (Action)Delegate.Combine(clearBtn.Clicked, new Action(ClearPrice));
		double listingFeeRate = Singleton<Constants>.Instance.Market.ListingFeeRate;
		_taxTooltipLabel.text = T._("수수료 {0:P0} <help>{1}</help>", listingFeeRate, T._("물품 등록시 수수료를 {0:P0} 지불합니다", listingFeeRate));
		ClearPrice();
	}

	private void OnClickPriceInput(GameObject obj)
	{
		UIManager.Popup.Tooltip<NumberInputPopup>().Show(_price, Currency.TStone, T._("가격 설정"), SetPrice);
	}

	private void UpdatePrice()
	{
		_priceInput.text = _price.ToString("N0", T.Culture);
		_clearBtn.Widget.alpha = ((_price <= 0) ? 0f : 1f);
		if (this.PriceChanged != null)
		{
			this.PriceChanged();
		}
	}

	public long GetPrice()
	{
		return _price;
	}

	public void SetPrice(long price)
	{
		_price = price;
		UpdatePrice();
	}

	private void ClearPrice()
	{
		SetPrice(0L);
	}

	public void InsertAlarm(bool on)
	{
		if (on)
		{
			if (!_insertAlarm.gameObject.activeSelf)
			{
				_insertAlarm.gameObject.SetActive(value: true);
				_insertAlarm.PlayForward();
				_insertAlarm.tweenFactor = 0f;
				_insertAlarm.Sample(0f, isFinished: false);
			}
		}
		else if (_insertAlarm.gameObject.activeSelf)
		{
			_insertAlarm.gameObject.SetActive(value: false);
		}
	}
}
