using System;
using L10N;
using UnityEngine;

public class CurrencyRangeSearchWidget : MonoBehaviour
{
	private class InputItem
	{
		public SelectableWidget Widget;

		public UIInput Input;

		public int Value;
	}

	[SerializeField]
	private ListObjectPool _inputs;

	[SerializeField]
	private UIWidget _hyphen;

	[SerializeField]
	private Selectable _inputButton;

	[SerializeField]
	private ListObjectPool _currencyButtons;

	[SerializeField]
	private int[] _currencyButtonValues;

	private InputItem _minInput;

	private InputItem _maxInput;

	private bool _isInit;

	public int Min => _minInput.Value;

	public int Max => _maxInput.Value;

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnPressScreen));
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnPressScreen));
	}

	private void Init()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_inputs.Set(2);
			Vector3 localPosition = _inputs[0].transform.localPosition;
			UIWidget component = _inputs[0].GetComponent<UIWidget>();
			localPosition.x += (float)component.width;
			float num = _hyphen.localCorners[0].x + ((Component)_hyphen).transform.localPosition.x - (component.localCorners[2].x + ((Component)component).transform.localPosition.x);
			localPosition.x += num * 2f + (float)_hyphen.width;
			_inputs[1].transform.localPosition = localPosition;
			_minInput = MakeInputItem(_inputs[0]);
			_maxInput = MakeInputItem(_inputs[1]);
			_minInput.Input.defaultText = T._("최소 가격");
			_maxInput.Input.defaultText = T._("최대 가격");
			Selectable inputButton = _inputButton;
			inputButton.Clicked = (Action)Delegate.Combine(inputButton.Clicked, new Action(OnClickKeyboardInput));
			_currencyButtons.Set(_currencyButtonValues.Length);
			for (int i = 0; i < _currencyButtons.Count; i++)
			{
				DefaultSelectableButton component2 = _currencyButtons[i].GetComponent<DefaultSelectableButton>();
				component2.Text = _currencyButtonValues[i].ToString("N0");
				component2.Clicked = (Action)Delegate.Combine(component2.Clicked, new Action(OnClickCurrencyButton));
			}
			_currencyButtons.Reposition(Vector3.right, 5);
		}
	}

	private InputItem MakeInputItem(GameObject parent)
	{
		InputItem inputItem = new InputItem();
		inputItem.Widget = parent.GetComponent<SelectableWidget>();
		inputItem.Input = parent.GetComponentInChildren<UIInput>();
		InputItem inputItem2 = inputItem;
		inputItem2.Widget.Selected += CurrencyLabelSelected;
		UIEventListener uIEventListener = UIEventListener.Get(((Component)inputItem2.Widget).gameObject);
		uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, new UIEventListener.BoolDelegate(OnSelectCurrencyLabel));
		return inputItem2;
	}

	private void OnSelectCurrencyLabel(GameObject obj, bool isSelect)
	{
		if (isSelect)
		{
			if ((Object)(object)((Component)_minInput.Widget).gameObject == (Object)(object)obj)
			{
				_minInput.Widget.Select = true;
				_maxInput.Widget.Select = false;
			}
			else
			{
				_maxInput.Widget.Select = true;
				_minInput.Widget.Select = false;
			}
		}
	}

	private void CurrencyLabelSelected(bool isSelect)
	{
		if (!isSelect)
		{
			int result;
			if (string.IsNullOrEmpty(_minInput.Input.value))
			{
				result = 0;
			}
			else if (!int.TryParse(_minInput.Input.value, out result))
			{
				result = _minInput.Value;
			}
			_minInput.Value = result;
			if (string.IsNullOrEmpty(_maxInput.Input.value))
			{
				result = 0;
			}
			else if (!int.TryParse(_maxInput.Input.value, out result))
			{
				result = _maxInput.Value;
			}
			_maxInput.Value = result;
			Refresh(clamp: true);
		}
	}

	private void OnClickKeyboardInput()
	{
		if (_minInput.Widget.Select || _maxInput.Widget.Select)
		{
			InputItem inputItem = ((!_minInput.Widget.Select) ? _maxInput : _minInput);
			inputItem.Input.value = inputItem.Value.ToString();
			inputItem.Input.isSelected = true;
		}
	}

	private void OnClickCurrencyButton()
	{
		if (_minInput.Widget.Select || _maxInput.Widget.Select)
		{
			GameObject gameObject = ((Component)Selectable.Current).gameObject;
			int num = _currencyButtons.IndexOf(gameObject);
			if (num != -1)
			{
				int num2 = _currencyButtonValues[num];
				InputItem inputItem = ((!_minInput.Widget.Select) ? _maxInput : _minInput);
				inputItem.Value += num2;
				Refresh(clamp: false);
			}
		}
	}

	public void Set(int min, int max)
	{
		Init();
		_minInput.Value = min;
		_maxInput.Value = max;
		Refresh(clamp: true);
	}

	private void Refresh(bool clamp)
	{
		if (clamp && _maxInput.Value > 0)
		{
			_minInput.Value = Mathf.Clamp(_minInput.Value, 0, _maxInput.Value);
		}
		_minInput.Input.value = ((_minInput.Value != 0) ? _minInput.Value.ToString("N0") : string.Empty);
		_maxInput.Input.value = ((_maxInput.Value != 0) ? _maxInput.Value.ToString("N0") : string.Empty);
	}

	private void OnPressScreen(GameObject obj, bool press)
	{
		if (press && !NGUITools.IsChild(((Component)this).transform, obj.transform))
		{
			_minInput.Widget.Select = false;
			_maxInput.Widget.Select = false;
		}
	}
}
