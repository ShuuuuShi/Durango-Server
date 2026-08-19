using System;
using L10N;
using UnityEngine;

public class PriceInputWidget : MonoBehaviour
{
	[SerializeField]
	private UIInput _priceInput;

	[SerializeField]
	private SelectableWidget _clearBtn;

	[SerializeField]
	private ListObjectPool _priceUnitButtons;

	[SerializeField]
	private Selectable _taxTooltip;

	[SerializeField]
	private int _unitButtonMargin;

	private int _price;

	private int[] _units;

	public void Init(params int[] units)
	{
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		_priceUnitButtons.Init(OnInitUnitButton);
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_priceInput).gameObject);
		uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, new UIEventListener.BoolDelegate(OnSelectPriceInput));
		SelectableWidget clearBtn = _clearBtn;
		clearBtn.Clicked = (Action)Delegate.Combine(clearBtn.Clicked, new Action(ClearPrice));
		_taxTooltip.Selected += delegate(bool select)
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			if (select)
			{
				LineTooltipControl lineTooltipControl = UIManager.Popup.Tooltip<LineTooltipControl>();
				lineTooltipControl.Set(null, T._("물품 등록시 수수료를 5% 지불합니다"));
				lineTooltipControl.MaxWidth = 200;
				lineTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
				lineTooltipControl.AddOnFinished(delegate
				{
					_taxTooltip.Select = false;
				});
				lineTooltipControl.Show(((Component)((Component)_taxTooltip).transform.FindChild("Sprite")).gameObject, Vector2.zero, 10f);
			}
		};
		_units = units;
		int num = ((units != null) ? units.Length : 0);
		_priceUnitButtons.Set(num);
		int width = _priceUnitButtons.BaseObject.GetComponent<UIWidget>().width;
		int width2 = (width - (_unitButtonMargin * num - 1)) / num;
		for (int i = 0; i < num; i++)
		{
			DefaultSelectableButton component = _priceUnitButtons[i].GetComponent<DefaultSelectableButton>();
			component.Text = units[i].ToString("N0");
			component.Widget.SetAnchor((Transform)null);
			component.Widget.width = width2;
		}
		_priceUnitButtons.Reposition(Vector3.right, _unitButtonMargin);
		ClearPrice();
	}

	private void OnInitUnitButton(GameObject obj)
	{
		obj.GetComponent<UIRect>().SetAnchor((Transform)null);
		UIEventListener.Get(obj).onClick = OnClickUnitButton;
	}

	private void OnClickUnitButton(GameObject obj)
	{
		int num = _priceUnitButtons.IndexOf(obj);
		if (num != -1)
		{
			_price += _units[num];
			UpdatePriceLabel();
		}
	}

	private void OnSelectPriceInput(GameObject obj, bool isSelect)
	{
		if (isSelect)
		{
			_priceInput.value = _price.ToString();
			return;
		}
		if (int.TryParse(_priceInput.value, out var result))
		{
			_price = result;
		}
		UpdatePriceLabel();
	}

	private void UpdatePriceLabel()
	{
		_priceInput.value = _price.ToString("N0");
		_clearBtn.Widget.alpha = ((_price <= 0) ? 0f : 1f);
	}

	public int GetPrice()
	{
		return _price;
	}

	public void SetPrice(int price)
	{
		_price = price;
		UpdatePriceLabel();
	}

	private void ClearPrice()
	{
		SetPrice(0);
	}
}
