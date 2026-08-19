using System;
using L10N;
using UnityEngine;

public class LevelRangeSearchWidget : MonoBehaviour
{
	private class RangeButton
	{
		public readonly UIWidget Widget;

		private UILabel _label;

		private UISprite[] _arrows = new UISprite[2];

		private int _value;

		public int Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
				_label.text = ((_value <= 0) ? T._("∞") : $"Lv.\n[size=30]{_value}");
				_arrows[0].alpha = ((_value == 1) ? 0f : 1f);
				_arrows[1].alpha = ((_value == 0) ? 0f : 1f);
			}
		}

		public RangeButton(GameObject obj)
		{
			Widget = obj.GetComponent<UIWidget>();
			_label = ((Component)obj.transform.FindChild("Text")).GetComponent<UILabel>();
			_arrows[0] = ((Component)obj.transform.FindChild("ArrowL")).GetComponent<UISprite>();
			_arrows[1] = ((Component)obj.transform.FindChild("ArrowR")).GetComponent<UISprite>();
		}
	}

	[SerializeField]
	private ListObjectPool _buttons;

	[SerializeField]
	private UIWidget _upperWidget;

	[SerializeField]
	private UIWidget _slideBg;

	private RangeButton _minBtn;

	private RangeButton _maxBtn;

	private int _max;

	private bool _isInit;

	public int Min => _minBtn.Value;

	public int Max => _maxBtn.Value;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_buttons.Set(2);
			_minBtn = new RangeButton(_buttons[0]);
			_maxBtn = new RangeButton(_buttons[1]);
			_max = (GameSystem<StatisticsSystem>.HasInstance() ? GameSystem<StatisticsSystem>.Instance().Level : 0) + 5;
			for (int i = 0; i < _buttons.Count; i++)
			{
				UIEventListener uIEventListener = UIEventListener.Get(_buttons[i]);
				uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragButton));
				UIEventListener uIEventListener2 = UIEventListener.Get(_buttons[i]);
				uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(OnPressButton));
			}
		}
	}

	public void Set(int min, int max)
	{
		Init();
		_minBtn.Value = min;
		_maxBtn.Value = max;
		Refresh();
	}

	private void Refresh()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		_minBtn.Value = Mathf.Clamp(_minBtn.Value, 1, _max);
		_maxBtn.Value = ((_maxBtn.Value > 0) ? Mathf.Clamp(_maxBtn.Value, _minBtn.Value + 1, _max) : 0);
		if (_maxBtn.Value > _max)
		{
			_maxBtn.Value = 0;
		}
		float num = (float)(_minBtn.Value - 1) / (float)_max;
		float num2 = ((_maxBtn.Value <= 0) ? 1f : ((float)(_maxBtn.Value - 1) / (float)_max));
		float num3 = _slideBg.localCorners[0].x + (float)_minBtn.Widget.width * _minBtn.Widget.pivotOffset.x + ((Component)_slideBg).transform.localPosition.x;
		float num4 = _slideBg.localCorners[3].x - (float)_maxBtn.Widget.width * (1f - _maxBtn.Widget.pivotOffset.x) + ((Component)_slideBg).transform.localPosition.x;
		float num5 = num4 - num3;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(num3, _buttons.BaseObject.transform.localPosition.y);
		((Component)_minBtn.Widget).transform.localPosition = val + Vector3.right * num5 * num;
		((Component)_maxBtn.Widget).transform.localPosition = val + Vector3.right * num5 * num2;
		RefreshUpperBg();
	}

	private void RefreshUpperBg()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)_upperWidget).transform.localPosition;
		float num = _minBtn.Widget.localCorners[0].x + ((Component)_minBtn.Widget).transform.localPosition.x;
		float num2 = _maxBtn.Widget.localCorners[3].x + ((Component)_maxBtn.Widget).transform.localPosition.x;
		localPosition.x = Mathf.Lerp(num, num2, 0.5f);
		((Component)_upperWidget).transform.localPosition = localPosition;
		_upperWidget.width = (int)(num2 - num);
	}

	private void OnDragButton(GameObject obj, Vector2 vec)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		RangeButton rangeButton = ((_buttons.IndexOf(obj) != 0) ? _maxBtn : _minBtn);
		float num = _slideBg.localCorners[0].x + (float)_minBtn.Widget.width * _minBtn.Widget.pivotOffset.x + ((Component)_slideBg).transform.localPosition.x;
		float num2 = _slideBg.localCorners[3].x - (float)_maxBtn.Widget.width * (1f - _maxBtn.Widget.pivotOffset.x) + ((Component)_slideBg).transform.localPosition.x;
		float num3 = ((rangeButton != _minBtn) ? ((float)_minBtn.Value / (float)_max) : 0f);
		float num4 = ((rangeButton != _minBtn) ? 1f : ((float)(((_maxBtn.Value <= 0) ? _max : (_maxBtn.Value - 1)) - 1) / (float)_max));
		Vector3 localPosition = obj.transform.localPosition;
		localPosition.x += vec.x;
		localPosition.x = Mathf.Clamp(localPosition.x, num, num2);
		float num5 = (localPosition.x - num) / (num2 - num);
		num5 = Mathf.Clamp(num5, num3, num4);
		obj.transform.localPosition = new Vector3(num + (num2 - num) * num5, localPosition.y);
		int num6 = Mathf.RoundToInt((float)_max * num5);
		num6 = ((num6 != _max) ? (num6 + 1) : 0);
		rangeButton.Value = num6;
		RefreshUpperBg();
	}

	private void OnPressButton(GameObject obj, bool press)
	{
		if (!press)
		{
			Refresh();
		}
	}
}
