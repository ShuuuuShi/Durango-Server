using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class TriggerTrainCoverController : MonoBehaviour
{
	public int positionalValue;

	private void OnTriggerEnter(Collider other)
	{
		PlayerBehavior component = other.gameObject.GetComponent<PlayerBehavior>();
		if ((bool)component)
		{
			Singleton<PrologueManager>.Instance().TrainManager.SetTrainShow(positionalValue);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerBehavior component = other.gameObject.GetComponent<PlayerBehavior>();
		if ((bool)component)
		{
			Singleton<PrologueManager>.Instance().TrainManager.SetTrainCover(positionalValue);
		}
	}
}
