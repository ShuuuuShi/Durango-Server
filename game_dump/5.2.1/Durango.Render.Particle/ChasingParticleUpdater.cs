using UnityEngine;

namespace Durango.Render.Particle;

public class ChasingParticleUpdater : MonoBehaviour
{
	public Transform ChasingTarget { get; set; }

	public Vector3 FollowingOffset { get; set; }

	public bool ToGround { get; set; }

	private void LateUpdate()
	{
		if (ChasingTarget == null || base.transform.parent == null)
		{
			Deactive();
			return;
		}
		Vector3 position = ChasingTarget.transform.position + base.transform.parent.TransformDirection(FollowingOffset);
		if (ToGround)
		{
			position.y = 5f;
		}
		base.transform.position = position;
	}

	private void Deactive()
	{
		base.enabled = false;
		ChasingTarget = null;
	}

	private void OnDisable()
	{
		Deactive();
	}
}
