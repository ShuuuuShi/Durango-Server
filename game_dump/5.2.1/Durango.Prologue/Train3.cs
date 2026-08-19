using UnityEngine;

namespace Durango.Prologue;

public class Train3 : MonoBehaviour
{
	private void OnTriggerEnter()
	{
		Animation component = base.transform.GetChild(0).GetComponent<Animation>();
		if ((bool)component)
		{
			component.Play();
		}
	}

	private void OnTriggerExit()
	{
		Animation component = base.transform.GetChild(0).GetComponent<Animation>();
		if ((bool)component)
		{
			component.Stop();
		}
	}
}
