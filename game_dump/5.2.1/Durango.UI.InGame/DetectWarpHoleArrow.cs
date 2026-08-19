using System;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.InGame;

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

	public float CurrentAngle => Maths.PositiveAngDeg(base.transform.localEulerAngles.z);

	public void SetTarget(Vector3 target, Color color)
	{
		_targetPosition = target;
		_tweenPosition.GetComponent<UITexture>().color = color;
		_currentSettingIndex = -1;
		UpdateArrowSetting();
	}

	public void UpdatePosition(Vector3 position)
	{
		int distance = (int)(Vector3.Distance(_targetPosition, position) / 100f);
		float num = Maths.CalcYawWithTarget(_targetPosition, position);
		base.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - num);
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
		if (0 <= _currentSettingIndex && _currentSettingIndex < _arrowSettings.Length)
		{
			ArrowSetting arrowSetting = _arrowSettings[_currentSettingIndex];
			_tweenPosition.duration = arrowSetting.pulseDuration;
			_arrowPositioner.alpha = arrowSetting.alpha;
			_arrowPositioner.transform.localScale = Vector3.one * arrowSetting.scale;
			_arrowPositioner.transform.localPosition = Vector3.up * ((float)_widgetSelf.height * arrowSetting.positionRatio);
			_arrowPositioner.gameObject.SetActive(value: true);
		}
		else
		{
			_arrowPositioner.gameObject.SetActive(value: false);
		}
	}
}
