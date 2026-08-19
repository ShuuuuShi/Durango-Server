using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag and Drop Root")]
public class UIDragDropRoot : MonoBehaviour
{
	public static Transform root;

	private void OnEnable()
	{
		root = ((Component)this).transform;
	}

	private void OnDisable()
	{
		if ((Object)(object)root == (Object)(object)((Component)this).transform)
		{
			root = null;
		}
	}
}
