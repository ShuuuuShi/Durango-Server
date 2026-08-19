using System;
using Durango.Utils;
using L10N;
using Shared.System;
using UnityEngine;

namespace Durango.UI.InGame;

public class DetectWarpHoleNearbyMarker : MonoBehaviour
{
	[Serializable]
	public struct MarkerSetting
	{
		public Color Color;

		public float Scale;

		public float Alpha;

		public float PulseDuration;

		[LocalizableString]
		public string SystemMsg;

		public float SystemMsgTime;
	}

	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private TweenPosition _tweenPosition;

	[SerializeField]
	private UIWidget _markerZoomer;

	[SerializeField]
	private UIWidget _markerPositioner;

	[SerializeField]
	private int _minDistance;

	[SerializeField]
	private MarkerSetting _portSetting;

	[SerializeField]
	private MarkerSetting _warpHoleSetting;

	[SerializeField]
	private MarkerSetting _craterSetting;

	[SerializeField]
	private MarkerSetting _crackSetting;

	private Vector3 _targetPosition;

	public void Show(PointOfInterest poiType, Vector3 target)
	{
		MarkerSetting markerSetting;
		switch (poiType)
		{
		default:
			return;
		case PointOfInterest.Port:
			markerSetting = _portSetting;
			break;
		case PointOfInterest.Warphole:
		case PointOfInterest.CargoWarphole:
			markerSetting = _warpHoleSetting;
			break;
		case PointOfInterest.Crater:
			markerSetting = _craterSetting;
			break;
		case PointOfInterest.Crack:
			markerSetting = _crackSetting;
			break;
		case PointOfInterest.FactionProp:
			return;
		}
		_texture.color = markerSetting.Color;
		_markerPositioner.alpha = markerSetting.Alpha;
		_markerPositioner.transform.localScale = Vector3.one * markerSetting.Scale;
		_tweenPosition.duration = markerSetting.PulseDuration;
		_targetPosition = target;
		base.gameObject.SetActive(value: true);
		UIManager.SystemMsg(T._(markerSetting.SystemMsg), markerSetting.SystemMsgTime);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void UpdatePosition(Vector3 position)
	{
		if (base.gameObject.activeSelf)
		{
			int num = (int)(Vector3.Distance(_targetPosition, position) / 100f);
			float num2 = Maths.CalcYawWithTarget(_targetPosition, position);
			base.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - num2);
			_markerPositioner.gameObject.SetActive(num >= _minDistance);
		}
	}
}
