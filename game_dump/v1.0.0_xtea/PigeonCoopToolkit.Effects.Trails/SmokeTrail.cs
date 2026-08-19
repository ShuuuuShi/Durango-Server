using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

[AddComponentMenu("Pigeon Coop Toolkit/Effects/Smoke Trail")]
public class SmokeTrail : TrailRenderer_Base
{
	public float MinVertexDistance = 0.1f;

	private Vector3 _lastPosition;

	private float _distanceMoved;

	public float RandomForceScale = 1f;

	private Transform _clampPosition;

	public void ClampFirstPointTo(Transform clampTo)
	{
		_clampPosition = clampTo;
	}

	public void ClearClamp()
	{
		_clampPosition = null;
	}

	protected override void Start()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		_lastPosition = _t.position;
	}

	protected override void Update()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (_emit)
		{
			_distanceMoved += Vector3.Distance(_t.position, _lastPosition);
			if (_distanceMoved != 0f && _distanceMoved >= MinVertexDistance)
			{
				AddPoint(new SmokeTrailPoint(), _lastPosition);
				_distanceMoved = 0f;
			}
			_lastPosition = _t.position;
		}
		base.Update();
	}

	protected override void OnStartEmit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_lastPosition = _t.position;
		_distanceMoved = 0f;
	}

	protected override void UpdatePoint(PCTrailPoint point, float deltaTime)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_clampPosition) && point.PointNumber == 0)
		{
			point.Position = _clampPosition.position;
		}
	}

	protected override void Reset()
	{
		base.Reset();
		MinVertexDistance = 0.1f;
		RandomForceScale = 1f;
	}

	protected override void InitialiseNewPoint(PCTrailPoint newPoint)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((SmokeTrailPoint)newPoint).RandomVec = Random.onUnitSphere * RandomForceScale;
	}
}
