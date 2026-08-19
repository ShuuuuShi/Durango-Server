using System;
using Durango.Render.Effect;
using Durango.Utils;
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

	private CharacterBehavior _character;

	private float _pushBase;

	private Vector3 _prevCenter;

	private Vector3 _tipLocalPosition;

	private AnimationState _animationState;

	private float _elapsedTime;

	private int _index;

	public FixOption Option { get; set; }

	public Transform TipTransform { get; set; }

	public void SetBaked(TrailBaker.TrailData data, CharacterBehavior character, float pushBase, float timePassed)
	{
		_bakedData = data;
		_character = character;
		_pushBase = pushBase;
		_elapsedTime = timePassed;
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
		if (!(_character == null) && !(_character.RootMotionTransform == null))
		{
			float elapsedTime = _elapsedTime;
			float deltaTime = GetDeltaTime();
			_elapsedTime = elapsedTime + deltaTime;
			while (_index < _bakedData.Times.Length && !(_elapsedTime < _bakedData.Times[_index]))
			{
				Quaternion bakedRotation = new Quaternion(_bakedData.BaseRotations[_index * 4], _bakedData.BaseRotations[_index * 4 + 1], _bakedData.BaseRotations[_index * 4 + 2], _bakedData.BaseRotations[_index * 4 + 3]);
				Vector3 bakedPosition = new Vector3(_bakedData.BasePoints[_index * 3], _bakedData.BasePoints[_index * 3 + 1], _bakedData.BasePoints[_index * 3 + 2]);
				Vector3 centerPosition = Vector3.Lerp(t: (_bakedData.Times[_index] - elapsedTime) / deltaTime, a: _prevCenter, b: _character.RootMotionTransform.position);
				Vector3 basePosition = TrailBaker.GetBasePosition(bakedPosition, _character.transform.rotation, centerPosition);
				Vector3 tipPosition = TrailBaker.GetTipPosition(basePosition, centerPosition, _character.transform.rotation, bakedRotation, _tipLocalPosition);
				basePosition += (tipPosition - basePosition).normalized * _pushBase;
				AddSegment(basePosition, tipPosition);
				_index++;
			}
			_prevCenter = _character.RootMotionTransform.position;
		}
	}

	private void LateUpdateNormal()
	{
		Vector3 vector = ((!(TipTransform != null)) ? _t.position : TipTransform.position);
		_distanceMoved += Mathf.Max(Vector3.Distance(_t.position, _lastPosition), Vector3.Distance(vector, _lastPosition2));
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
				Vector3 next2 = vector + (vector - _lastPosition2);
				for (float num = _interpDeltaDistance; num < _distanceMoved; num += _interpDeltaDistance)
				{
					float percentComplete = num / _distanceMoved;
					Vector3 posBase = Maths.CatmullRom(_prevInterPosition, _lastPosition, _t.position, next, percentComplete);
					Vector3 posTip = Maths.CatmullRom(_prevInterPosition2, _lastPosition2, vector, next2, percentComplete);
					AddSegment(posBase, posTip);
				}
			}
			AddSegment(_t.position, vector);
			_distanceMoved = 0f;
		}
		_prevInterPosition = _lastPosition;
		_prevInterPosition2 = _lastPosition2;
		_lastPosition = _t.position;
		_lastPosition2 = vector;
	}

	private void AddSegment(Vector3 posBase, Vector3 posTip)
	{
		if (_activeTrail == null)
		{
			return;
		}
		int count = _activeTrail.Points.Count;
		if (count > 0)
		{
			PCTrailPoint pCTrailPoint = _activeTrail.Points[count - 1];
			if (Option == FixOption.LessOverlap && Maths.LineLineIntersect(posBase, posTip, pCTrailPoint.Position, pCTrailPoint.Position2, out var nearestPoint))
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
			pCTrailPoint2.Forward = ((!TrailData.ForwardOverrideRelative) ? TrailData.ForwardOverride.normalized : _t.TransformDirection(TrailData.ForwardOverride.normalized));
		}
		_activeTrail.Points.Add(pCTrailPoint2);
	}

	protected override void OnStartEmit()
	{
		_lastPosition = _t.position;
		_lastPosition2 = TipTransform.position;
		_prevInterPosition = _lastPosition;
		_prevInterPosition2 = _lastPosition2;
		_distanceMoved = 0f;
		_elapsedTime = 0f;
		_index = 0;
		if (_character != null)
		{
			_prevCenter = _character.RootMotionTransform.position;
			_animationState = _character.Anim[_character.CurrentAnimClipName];
		}
	}

	protected override float GetDeltaTime()
	{
		float num = base.GetDeltaTime();
		if (_animationState != null)
		{
			num *= _animationState.speed;
		}
		return num;
	}
}
