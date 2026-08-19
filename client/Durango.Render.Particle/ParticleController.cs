using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Particle;

public class ParticleController : MonoBehaviour
{
	public enum ParticlePositionMode
	{
		Default,
		Ground,
		ForwardToCamera
	}

	[SerializeField]
	private ParticlePositionMode _positionMode;

	private void OnEnable()
	{
		switch (_positionMode)
		{
		case ParticlePositionMode.Ground:
		{
			Vector3 position = base.gameObject.transform.position;
			position.y = 5f;
			base.gameObject.transform.position = position;
			break;
		}
		case ParticlePositionMode.ForwardToCamera:
			base.gameObject.transform.position = base.gameObject.transform.position - Singleton<MainCamera>.Instance().transform.forward * 500f;
			break;
		}
	}
}
