using UnityEngine;

public class KeyValueLabel : MonoBehaviour
{
	[SerializeField]
	private UILabel _keyLabel;

	[SerializeField]
	private UILabel _valueLabel;

	[SerializeField]
	private int _minWidth;

	[SerializeField]
	private int _keyValueMargin;

	private UISpriteLabel _keySpriteLabel;

	private UISpriteLabel _valueSpriteLabel;

	private UIWidget _widget;

	private SyncString _key;

	private SyncString _value;

	private float _topPadding;

	private float _bottomPadding;

	private float _leftPadding;

	private float _rightPadding;

	private bool _isInit;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Init()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_keySpriteLabel = ((Component)_keyLabel).GetComponent<UISpriteLabel>();
			_valueSpriteLabel = ((Component)_valueLabel).GetComponent<UISpriteLabel>();
			Vector3[] localCorners = Widget.localCorners;
			Vector3[] localCorners2 = _keyLabel.localCorners;
			Vector3[] localCorners3 = _valueLabel.localCorners;
			for (int i = 0; i < 4; i++)
			{
				ref Vector3 reference = ref localCorners2[i];
				reference += ((Component)_keyLabel).transform.localPosition;
				ref Vector3 reference2 = ref localCorners3[i];
				reference2 += ((Component)_valueLabel).transform.localPosition;
			}
			_leftPadding = localCorners2[0].x - localCorners[0].x;
			_topPadding = localCorners[1].y - localCorners2[1].y;
			_bottomPadding = localCorners2[0].y - localCorners[0].y;
			_rightPadding = localCorners[2].x - localCorners3[2].x;
			Vector2 pivotOffset = _keyLabel.pivotOffset;
			if (pivotOffset.y == 0f)
			{
				_topPadding = _bottomPadding;
			}
			else if (pivotOffset.y == 1f)
			{
				_bottomPadding = _topPadding;
			}
		}
	}

	public void Set(SyncString key, SyncString value)
	{
		SetKey(key);
		SetValue(value);
	}

	public void SetKey(SyncString key)
	{
		Init();
		_key = key;
		float period;
		if ((Object)(object)_keySpriteLabel != (Object)null)
		{
			_keySpriteLabel.text = _key.Get(out period);
			if (period > 0f)
			{
				LabelUpdater.Set(_keySpriteLabel, _key);
			}
		}
		else if ((Object)(object)_keyLabel != (Object)null)
		{
			_keyLabel.text = _key.Get(out period);
			if (period > 0f)
			{
				LabelUpdater.Set(_keyLabel, _key);
			}
		}
	}

	public void SetValue(SyncString value)
	{
		Init();
		_value = value;
		float period;
		if ((Object)(object)_valueSpriteLabel != (Object)null)
		{
			_valueSpriteLabel.text = _value.Get(out period);
			if (period > 0f)
			{
				LabelUpdater.Set(_valueSpriteLabel, _value);
			}
		}
		else if ((Object)(object)_valueLabel != (Object)null)
		{
			_valueLabel.text = _value.Get(out period);
			if (period > 0f)
			{
				LabelUpdater.Set(_valueLabel, _value);
			}
		}
	}

	public void UpdateLayout(int width = 0)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		Point2 point = new Point2(GetPredictSize(width));
		Widget.SetDimensions(point.x, point.y);
		NGUITools.UpdateWidgetCollider(((Component)this).gameObject);
		if (_keyLabel.isAnchored)
		{
			_keyLabel.UpdateAnchors();
		}
		else
		{
			Vector2 pivotOffset = _keyLabel.pivotOffset;
			Vector2 val = default(Vector2);
			val.x = _leftPadding + (float)_keyLabel.width * pivotOffset.x - (float)Widget.width * Widget.pivotOffset.x;
			if (pivotOffset.y == 0f)
			{
				val.y = _bottomPadding - (float)Widget.height * Widget.pivotOffset.y;
			}
			else if (pivotOffset.y == 1f)
			{
				val.y = (float)Widget.height - _topPadding - (float)Widget.height * Widget.pivotOffset.y;
			}
			else
			{
				val.y = (float)Widget.height * (0.5f - Widget.pivotOffset.y);
			}
			((Component)_keyLabel).transform.localPosition = Vector2.op_Implicit(val);
		}
		if (_valueLabel.isAnchored)
		{
			_valueLabel.UpdateAnchors();
			return;
		}
		Vector2 pivotOffset2 = _valueLabel.pivotOffset;
		Vector2 val2 = default(Vector2);
		val2.x = (float)Widget.width - (_rightPadding + (float)_valueLabel.width * (1f - pivotOffset2.x)) - (float)Widget.width * Widget.pivotOffset.x;
		if (pivotOffset2.y == 0f)
		{
			val2.y = _bottomPadding - (float)Widget.height * Widget.pivotOffset.y;
		}
		else if (pivotOffset2.y == 1f)
		{
			val2.y = (float)Widget.height - _topPadding - (float)Widget.height * Widget.pivotOffset.y;
		}
		else
		{
			val2.y = (float)Widget.height * (0.5f - Widget.pivotOffset.y);
		}
		((Component)_valueLabel).transform.localPosition = Vector2.op_Implicit(val2);
	}

	public Vector2 GetPredictSize(int width = 0)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = ((!string.IsNullOrEmpty(_keyLabel.text)) ? _keyLabel.printedSize : Vector2.zero);
		Vector2 val2 = ((!string.IsNullOrEmpty(_valueLabel.text)) ? _valueLabel.printedSize : Vector2.zero);
		Vector2 result = default(Vector2);
		if (width == 0)
		{
			result.x = val.x + val2.x + _leftPadding + _rightPadding;
			if (val.x > 0f && val2.x > 0f)
			{
				result.x += (float)_keyValueMargin;
			}
			result.x = Mathf.Max((float)_minWidth, result.x);
		}
		else
		{
			result.x = width;
		}
		result.y = Mathf.Max(val.y, val2.y) + _bottomPadding + _topPadding;
		return result;
	}
}
