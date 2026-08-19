using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class TriggerTrainCoverController : MonoBehaviour
{
	public int positionalValue;

	private void OnTriggerEnter(Collider other)
	{
		if ((bool)other.gameObject.GetComponent<PlayerBehavior>())
		{
			Singleton<PrologueManager>.Instance().TrainManager.SetTrainShow(positionalValue);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((bool)other.gameObject.GetComponent<PlayerBehavior>())
		{
			Singleton<PrologueManager>.Instance().TrainManager.SetTrainCover(positionalValue);
		}
	}
}
