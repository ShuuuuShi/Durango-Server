using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Activate")]
public class UIButtonActivate : MonoBehaviour
{
	public GameObject target;

	public bool state = true;

	private void OnClick()
	{
		if ((Object)(object)target != (Object)null)
		{
			NGUITools.SetActive(target, state);
		}
	}
}
