using System.Collections;
using UnityEngine;

public class AgentAttackedByRaptor : MonoBehaviour
{
	[SerializeField]
	private float _beforeAttackedDelay = 2f;

	[SerializeField]
	private float _doorDelayBegin = 0.5f;

	[SerializeField]
	private float _doorOpeningDelay = 9f;

	[SerializeField]
	private float _getAxeMsgDelay = 2f;

	[SerializeField]
	private string _targetDoorName = "Trigger_train_06_door_02";

	[SerializeField]
	private string _motionName = "AgentAttackedByRaptor";

	private TriggerDoorController _targetDoor;

	private IEnumerator Start()
	{
		GameObject target = KUtility.FindObjectByName(KSingleton<PrologueManager>.Instance().TriggersGroup, _targetDoorName, includeInactive: true);
		if (Object.op_Implicit((Object)(object)target))
		{
			_targetDoor = target.GetComponent<TriggerDoorController>();
		}
		yield return (object)new WaitForSeconds(_beforeAttackedDelay);
		yield return (object)new WaitForSeconds(_doorDelayBegin);
		if (Object.op_Implicit((Object)(object)_targetDoor))
		{
			_targetDoor.DoorOpen();
		}
		PrologueManager.PlayerBattleAi.GetScared();
		yield return (object)new WaitForSeconds(_doorOpeningDelay);
		if (Object.op_Implicit((Object)(object)_targetDoor))
		{
			_targetDoor.DoorClose();
		}
		yield return (object)new WaitForSeconds(_getAxeMsgDelay);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.GetAxe);
	}

	private void ShowObject(string objName)
	{
		GameObject val = KUtility.FindObjectByName(((Component)this).gameObject, objName, includeInactive: true);
		if (Object.op_Implicit((Object)(object)val))
		{
			val.SetActive(true);
		}
	}

	private void HideObject(string objName)
	{
		GameObject val = KUtility.FindObjectByName(((Component)this).gameObject, objName, includeInactive: true);
		if (Object.op_Implicit((Object)(object)val))
		{
			val.SetActive(false);
		}
	}
}
