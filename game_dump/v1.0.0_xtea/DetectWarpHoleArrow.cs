using System;
using UnityEngine;

public class DetectWarpHoleArrow : MonoBehaviour
{
	[Serializable]
	public struct ArrowSetting
	{
		public float scale;

		public float alpha;

		public float pulseDuration;

		public float positionRatio;
	}

	[SerializeField]
	private UIWidget _widgetSelf;

	[SerializeField]
	private UIWidget _arrowPositioner;

	[SerializeField]
	private TweenPosition _tweenPosition;

	[SerializeField]
	private int[] _distanceValues;

	[SerializeField]
	private ArrowSetting[] _arrowSettings;

	private Vector3 _targetPosition;

	private int _currentSettingIndex = -1;

	public float CurrentAngle => KMathUtil.PositiveAngDeg(((Component)this).transform.localEulerAngles.z);

	public void SetTarget(Vector3 target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_targetPosition = target;
		_currentSettingIndex = -1;
		UpdateArrowSetting();
	}

	public void UpdatePosition(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		int distance = (int)(Vector3.Distance(_targetPosition, position) / 100f);
		float num = KMathUtil.CalcYawWithTarget(_targetPosition, position);
		((Component)this).gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - num);
		int settingIndexByDistance = GetSettingIndexByDistance(distance);
		if (_currentSettingIndex != settingIndexByDistance)
		{
			_currentSettingIndex = settingIndexByDistance;
			UpdateArrowSetting();
		}
	}

	private int GetSettingIndexByDistance(int distance)
	{
		for (int i = 0; i < _distanceValues.Length; i++)
		{
			if (distance >= _distanceValues[i])
			{
				return i;
			}
		}
		return -1;
	}

	private void UpdateArrowSetting()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (0 <= _currentSettingIndex && _currentSettingIndex < _arrowSettings.Length)
		{
			ArrowSetting arrowSetting = _arrowSettings[_currentSettingIndex];
			_tweenPosition.duration = arrowSetting.pulseDuration;
			_arrowPositioner.alpha = arrowSetting.alpha;
			((Component)_arrowPositioner).transform.localScale = Vector3.one * arrowSetting.scale;
			((Component)_arrowPositioner).transform.localPosition = Vector3.up * ((float)_widgetSelf.height * arrowSetting.positionRatio);
			((Component)_arrowPositioner).gameObject.SetActive(true);
		}
		else
		{
			((Component)_arrowPositioner).gameObject.SetActive(false);
		}
	}
}
