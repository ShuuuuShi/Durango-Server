using System;
using L10N;
using Shared.System;
using UnityEngine;

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
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		MarkerSetting markerSetting;
		switch (poiType)
		{
		default:
			return;
		case PointOfInterest.Port:
			markerSetting = _portSetting;
			break;
		case PointOfInterest.Warphole:
			markerSetting = _warpHoleSetting;
			break;
		case PointOfInterest.Crater:
			markerSetting = _craterSetting;
			break;
		case PointOfInterest.Crack:
			markerSetting = _crackSetting;
			break;
		}
		_texture.color = markerSetting.Color;
		_markerPositioner.alpha = markerSetting.Alpha;
		((Component)_markerPositioner).transform.localScale = Vector3.one * markerSetting.Scale;
		_tweenPosition.duration = markerSetting.PulseDuration;
		_targetPosition = target;
		((Component)this).gameObject.SetActive(true);
		UIManager.SystemMsg(T._(markerSetting.SystemMsg), markerSetting.SystemMsgTime);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void UpdatePosition(Vector3 position)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).gameObject.activeSelf)
		{
			int num = (int)(Vector3.Distance(_targetPosition, position) / 100f);
			float num2 = KMathUtil.CalcYawWithTarget(_targetPosition, position);
			((Component)this).gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - num2);
			((Component)_markerPositioner).gameObject.SetActive(num >= _minDistance);
		}
	}
}
