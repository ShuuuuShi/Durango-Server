using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class TriggersManager : Singleton<TriggersManager>
{
	private void Start()
	{
		GameObject trainModel = Singleton<PrologueManager>.Instance().TrainModel;
		TriggerDoorController[] componentsInChildren = GetComponentsInChildren<TriggerDoorController>();
		TriggerDoorController[] array = componentsInChildren;
		foreach (TriggerDoorController triggerDoorController in array)
		{
			string text = triggerDoorController.gameObject.name.Replace("Trigger_", string.Empty);
			GameObject gameObject = KUtility.FindObjectByName(trainModel, text, includeInactive: true);
			if (null != gameObject)
			{
				triggerDoorController._targetDoor = gameObject;
			}
		}
	}
}
