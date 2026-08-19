using System;
using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

[AddComponentMenu("Pigeon Coop Toolkit/Effects/PlaneTrail")]
public class PlaneTrail : TrailRenderer_Base
{
	public enum FixOption
	{
		None,
		LessOverlap,
		Baked
	}

	[SerializeField]
	private float MinVertexDistance = 0.1f;

	[SerializeField]
	private bool _interpolate = true;

	[SerializeField]
	private float _interpDeltaDistance = 10f;

	private Vector3 _lastPosition;

	private Vector3 _lastPosition2;

	private Vector3 _prevInterPosition;

	private Vector3 _prevInterPosition2;

	private float _distanceMoved;

	private TrailBaker.TrailData _bakedData;

	private Transform _centerTrans;

	private float _pushBase;

	private Vector3 _prevCenter;

	private Vector3 _tipLocalPosition;

	private float _startTime;

	private float _targetTime;

	private int _index;

	public FixOption Option { get; set; }

	public Transform TipTransform { get; set; }

	public void SetBaked(TrailBaker.TrailData data, Transform center, float pushBase, float timePassed)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_bakedData = data;
		_centerTrans = center;
		_pushBase = pushBase;
		_startTime = Time.time - timePassed;
		_tipLocalPosition = TipTransform.localPosition;
	}

	protected override void LateUpdate()
	{
		if (_emit)
		{
			if (Option == FixOption.Baked)
			{
				LateUpdateBaked();
			}
			else
			{
				LateUpdateNormal();
			}
		}
		base.LateUpdate();
	}

	private void LateUpdateBaked()
	{
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		float targetTime = Time.time - _startTime;
		float targetTime2 = _targetTime;
		_targetTime = targetTime;
		float num = _targetTime - targetTime2;
		Quaternion bakedRotation = default(Quaternion);
		Vector3 bakedPosition = default(Vector3);
		while (_index < _bakedData.Times.Length && !(_targetTime < _bakedData.Times[_index]))
		{
			((Quaternion)(ref bakedRotation))._002Ector(_bakedData.BaseRotations[_index * 4], _bakedData.BaseRotations[_index * 4 + 1], _bakedData.BaseRotations[_index * 4 + 2], _bakedData.BaseRotations[_index * 4 + 3]);
			((Vector3)(ref bakedPosition))._002Ector(_bakedData.BasePoints[_index * 3], _bakedData.BasePoints[_index * 3 + 1], _bakedData.BasePoints[_index * 3 + 2]);
			float num2 = _bakedData.Times[_index] - targetTime2;
			Vector3 centerPosition = Vector3.Lerp(_prevCenter, _centerTrans.position, num2 / num);
			Vector3 basePosition = TrailBaker.GetBasePosition(bakedPosition, _centerTrans.rotation, centerPosition);
			Vector3 tipPosition = TrailBaker.GetTipPosition(basePosition, centerPosition, _centerTrans.rotation, bakedRotation, _tipLocalPosition);
			Vector3 val = basePosition;
			Vector3 val2 = tipPosition - basePosition;
			basePosition = val + ((Vector3)(ref val2)).normalized * _pushBase;
			AddSegment(basePosition, tipPosition);
			_index++;
		}
		_prevCenter = _centerTrans.position;
	}

	private void LateUpdateNormal()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		_distanceMoved += Mathf.Max(Vector3.Distance(_t.position, _lastPosition), Vector3.Distance(TipTransform.position, _lastPosition2));
		if (Math.Abs(_distanceMoved) > Mathf.Epsilon && _distanceMoved >= MinVertexDistance)
		{
			if (_interpDeltaDistance <= 0f)
			{
				_interpDeltaDistance = 1f;
			}
			int count = _activeTrail.Points.Count;
			if (_interpolate && count > 1)
			{
				Vector3 next = _t.position + (_t.position - _lastPosition);
				Vector3 next2 = TipTransform.position + (TipTransform.position - _lastPosition2);
				for (float num = _interpDeltaDistance; num < _distanceMoved; num += _interpDeltaDistance)
				{
					float percentComplete = num / _distanceMoved;
					Vector3 posBase = KMathUtil.CatmullRom(_prevInterPosition, _lastPosition, _t.position, next, percentComplete);
					Vector3 posTip = KMathUtil.CatmullRom(_prevInterPosition2, _lastPosition2, TipTransform.position, next2, percentComplete);
					AddSegment(posBase, posTip);
				}
			}
			AddSegment(_t.position, TipTransform.position);
			_distanceMoved = 0f;
		}
		_prevInterPosition = _lastPosition;
		_prevInterPosition2 = _lastPosition2;
		_lastPosition = _t.position;
		_lastPosition2 = TipTransform.position;
	}

	private void AddSegment(Vector3 posBase, Vector3 posTip)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		if (_activeTrail == null)
		{
			return;
		}
		int count = _activeTrail.Points.Count;
		if (count > 0)
		{
			PCTrailPoint pCTrailPoint = _activeTrail.Points[count - 1];
			if (Option == FixOption.LessOverlap && KMathUtil.LineLineIntersect(posBase, posTip, pCTrailPoint.Position, pCTrailPoint.Position2, out var nearestPoint))
			{
				posBase = nearestPoint;
			}
		}
		PCTrailPoint pCTrailPoint2 = new PCTrailPoint();
		pCTrailPoint2.Position = posBase;
		pCTrailPoint2.Position2 = posTip;
		pCTrailPoint2.PointNumber = ((count != 0) ? (_activeTrail.Points[count - 1].PointNumber + 1) : 0);
		InitialiseNewPoint(pCTrailPoint2);
		pCTrailPoint2.SetDistanceFromStart((count != 0) ? (_activeTrail.Points[count - 1].GetDistanceFromStart() + Vector3.Distance(_activeTrail.Points[count - 1].Position, posBase)) : 0f);
		if (TrailData.UseForwardOverride)
		{
			pCTrailPoint2.Forward = ((!TrailData.ForwardOverrideRelative) ? ((Vector3)(ref TrailData.ForwardOverride)).normalized : _t.TransformDirection(((Vector3)(ref TrailData.ForwardOverride)).normalized));
		}
		_activeTrail.Points.Add(pCTrailPoint2);
	}

	protected override void OnStartEmit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		_lastPosition = _t.position;
		_lastPosition2 = TipTransform.position;
		_prevInterPosition = _lastPosition;
		_prevInterPosition2 = _lastPosition2;
		_distanceMoved = 0f;
		_targetTime = 0f;
		_index = 0;
		if ((Object)(object)_centerTrans != (Object)null)
		{
			_prevCenter = _centerTrans.position;
		}
	}
}
