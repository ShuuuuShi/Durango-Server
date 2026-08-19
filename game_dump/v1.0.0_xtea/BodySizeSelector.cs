using System;
using UnityEngine;

public class BodySizeSelector : MonoBehaviour
{
	[SerializeField]
	private Transform _maker;

	private UIWidget _widget;

	private float _min;

	private float _max;

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

	public float Value { get; private set; }

	public event Action<float> ValueChanged;

	private void OnPress(bool press)
	{
		UpdateSelector();
	}

	private void OnDrag(Vector2 delta)
	{
		UpdateSelector();
	}

	public void Init(float minRatio, float maxRatio, float ratio)
	{
		_min = minRatio;
		_max = maxRatio;
		Value = _min - 1f;
		Set(ratio);
	}

	public void Set(float value, bool sendEvent = false)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Value = Mathf.Clamp(value, _min, _max);
		float num = (Value - _min) / (_max - _min);
		Vector2 pivotOffset = Widget.pivotOffset;
		Vector3 localPosition = _maker.localPosition;
		float num2 = Widget.height;
		float y = num2 * (num - pivotOffset.y);
		localPosition.y = y;
		_maker.localPosition = localPosition;
		if (sendEvent && this.ValueChanged != null)
		{
			this.ValueChanged(Value);
		}
	}

	private void UpdateSelector()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformPoint(UICamera.lastWorldPosition);
		Vector2 pivotOffset = Widget.pivotOffset;
		float num = Widget.height;
		float num2 = val.y / num + pivotOffset.y;
		float value = _min + (_max - _min) * num2;
		Set(value, sendEvent: true);
	}
}
