using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

[AddComponentMenu("Pigeon Coop Toolkit/Effects/Smoke Plume")]
public class SmokePlume : TrailRenderer_Base
{
	public float TimeBetweenPoints = 0.1f;

	public Vector3 ConstantForce = Vector3.up * 0.5f;

	public float RandomForceScale = 0.05f;

	private float _timeSincePoint;

	protected override void Start()
	{
		base.Start();
		_timeSincePoint = 0f;
	}

	protected override void OnStartEmit()
	{
		_timeSincePoint = 0f;
	}

	protected override void Reset()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		base.Reset();
		TrailData.SizeOverLife = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
		{
			new Keyframe(0f, 0f),
			new Keyframe(0.5f, 0.2f),
			new Keyframe(1f, 0.2f)
		});
		TrailData.Lifetime = 6f;
		ConstantForce = Vector3.up * 0.5f;
		TimeBetweenPoints = 0.1f;
		RandomForceScale = 0.05f;
	}

	protected override void Update()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (_emit)
		{
			_timeSincePoint += Time.deltaTime;
			if (_timeSincePoint >= TimeBetweenPoints)
			{
				AddPoint(new SmokeTrailPoint(), _t.position);
				_timeSincePoint = 0f;
			}
		}
		base.Update();
	}

	protected override void InitialiseNewPoint(PCTrailPoint newPoint)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((SmokeTrailPoint)newPoint).RandomVec = Random.onUnitSphere * RandomForceScale;
	}

	protected override void UpdatePoint(PCTrailPoint point, float deltaTime)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		point.Position += ConstantForce * deltaTime;
	}
}
