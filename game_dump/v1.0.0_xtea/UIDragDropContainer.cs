using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag and Drop Container")]
public class UIDragDropContainer : MonoBehaviour
{
	public Transform reparentTarget;

	protected virtual void Start()
	{
		if ((Object)(object)reparentTarget == (Object)null)
		{
			reparentTarget = ((Component)this).transform;
		}
	}
}
