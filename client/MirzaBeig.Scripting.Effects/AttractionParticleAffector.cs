using UnityEngine;

namespace MirzaBeig.Scripting.Effects;

public class AttractionParticleAffector : ParticleAffector
{
	[Header("Affector Controls")]
	public float arrivalRadius = 1f;

	public float arrivedRadius = 0.5f;

	private float arrivalRadiusSqr;

	private float arrivedRadiusSqr;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
	}

	protected override void LateUpdate()
	{
		float x = base.transform.lossyScale.x;
		arrivalRadiusSqr = arrivalRadius * arrivalRadius * x;
		arrivedRadiusSqr = arrivedRadius * arrivedRadius * x;
		base.LateUpdate();
	}

	protected override Vector3 getForce()
	{
		if (parameters.distanceToAffectorCenterSqr < arrivedRadiusSqr)
		{
			Vector3 result = default(Vector3);
			result.x = 0f;
			result.y = 0f;
			result.z = 0f;
			return result;
		}
		if (parameters.distanceToAffectorCenterSqr < arrivalRadiusSqr)
		{
			float num = 1f - parameters.distanceToAffectorCenterSqr / arrivalRadiusSqr;
			return Vector3.Normalize(parameters.scaledDirectionToAffectorCenter) * num;
		}
		return Vector3.Normalize(parameters.scaledDirectionToAffectorCenter);
	}

	protected override void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			base.OnDrawGizmosSelected();
			float x = base.transform.lossyScale.x;
			float num = arrivalRadius * x;
			float num2 = arrivedRadius * x;
			Vector3 center = base.transform.position + offset;
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(center, num);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(center, num2);
		}
	}
}
