using UnityEngine;

public class TriggersManager : KSingleton<TriggersManager>
{
	private void Start()
	{
		GameObject trainModel = KSingleton<PrologueManager>.Instance().TrainModel;
		TriggerDoorController[] componentsInChildren = ((Component)this).GetComponentsInChildren<TriggerDoorController>();
		TriggerDoorController[] array = componentsInChildren;
		foreach (TriggerDoorController triggerDoorController in array)
		{
			string name = ((Object)((Component)triggerDoorController).gameObject).name.Replace("Trigger_", string.Empty);
			GameObject val = KUtility.FindObjectByName(trainModel, name, includeInactive: true);
			if ((Object)null != (Object)(object)val)
			{
				triggerDoorController._targetDoor = val;
			}
		}
	}
}
