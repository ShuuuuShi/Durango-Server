using UnityEngine;

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

	private void Start()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		switch (_positionMode)
		{
		case ParticlePositionMode.Default:
			break;
		case ParticlePositionMode.Ground:
		{
			Vector3 position = ((Component)this).gameObject.transform.position;
			position.y = 5f;
			((Component)this).gameObject.transform.position = position;
			break;
		}
		case ParticlePositionMode.ForwardToCamera:
			((Component)this).gameObject.transform.position = ((Component)this).gameObject.transform.position - ((Component)KSingleton<MainCamera>.Instance()).transform.forward * 500f;
			break;
		}
	}
}
