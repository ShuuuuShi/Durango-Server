using UnityEngine;

public class Train3 : MonoBehaviour
{
	private void OnTriggerEnter()
	{
		Animation component = ((Component)((Component)this).transform.GetChild(0)).GetComponent<Animation>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.Play();
		}
	}

	private void OnTriggerExit()
	{
		Animation component = ((Component)((Component)this).transform.GetChild(0)).GetComponent<Animation>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.Stop();
		}
	}
}
