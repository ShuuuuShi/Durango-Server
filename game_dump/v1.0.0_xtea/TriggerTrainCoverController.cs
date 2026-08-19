using UnityEngine;

public class TriggerTrainCoverController : MonoBehaviour
{
	public int positionalValue;

	private void OnTriggerEnter(Collider other)
	{
		PlayerBehavior component = ((Component)other).gameObject.GetComponent<PlayerBehavior>();
		if (Object.op_Implicit((Object)(object)component))
		{
			KSingleton<PrologueManager>.Instance().TrainManager.SetTrainShow(positionalValue);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerBehavior component = ((Component)other).gameObject.GetComponent<PlayerBehavior>();
		if (Object.op_Implicit((Object)(object)component))
		{
			KSingleton<PrologueManager>.Instance().TrainManager.SetTrainCover(positionalValue);
		}
	}
}
