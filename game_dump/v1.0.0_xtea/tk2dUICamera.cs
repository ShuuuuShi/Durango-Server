using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUICamera")]
public class tk2dUICamera : MonoBehaviour
{
	public enum tk2dRaycastType
	{
		Physics3D,
		Physics2D
	}

	[SerializeField]
	private LayerMask raycastLayerMask = LayerMask.op_Implicit(-1);

	[SerializeField]
	private tk2dRaycastType raycastType;

	public tk2dRaycastType RaycastType => raycastType;

	public LayerMask FilteredMask => LayerMask.op_Implicit(LayerMask.op_Implicit(raycastLayerMask) & ((Component)this).GetComponent<Camera>().cullingMask);

	public Camera HostCamera => ((Component)this).GetComponent<Camera>();

	public void AssignRaycastLayerMask(LayerMask mask)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		raycastLayerMask = mask;
	}

	private void OnEnable()
	{
		if ((Object)(object)((Component)this).GetComponent<Camera>() == (Object)null)
		{
			Debug.LogError((object)"tk2dUICamera should only be attached to a camera.");
			((Behaviour)this).enabled = false;
		}
		else if (!((Component)this).GetComponent<Camera>().orthographic && raycastType == tk2dRaycastType.Physics2D)
		{
			Debug.LogError((object)"tk2dUICamera - Physics2D raycast only works with orthographic cameras.");
			((Behaviour)this).enabled = false;
		}
		else
		{
			tk2dUIManager.RegisterCamera(this);
		}
	}

	private void OnDisable()
	{
		tk2dUIManager.UnregisterCamera(this);
	}
}
