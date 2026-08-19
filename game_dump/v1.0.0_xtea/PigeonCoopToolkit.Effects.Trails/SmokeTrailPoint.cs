using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

public class SmokeTrailPoint : PCTrailPoint
{
	public Vector3 RandomVec;

	public override void Update(float deltaTime)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		base.Update(deltaTime);
		Position += RandomVec * deltaTime;
	}
}
