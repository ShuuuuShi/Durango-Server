using System.Collections;
using Durango.Network;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.Prologue;

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
		GameObject target = KUtility.FindObjectByName(Singleton<PrologueManager>.Instance().TriggersGroup, _targetDoorName, includeInactive: true);
		if ((bool)target)
		{
			_targetDoor = target.GetComponent<TriggerDoorController>();
		}
		Singleton<PrologueTunnelController>.Instance().TransitionBgm();
		yield return new WaitForSeconds(_beforeAttackedDelay);
		yield return new WaitForSeconds(_doorDelayBegin);
		if ((bool)_targetDoor)
		{
			_targetDoor.DoorOpen();
		}
		PlayerBehavior.LocalPlayer.ChangeWeaponType(PlayerBehavior.WeaponFramework.SCARED);
		Connections.Frontend.PushPacket(new SetBaseMoveSpeed
		{
			EntityId = GameManager.PlayerId,
			NormalSpeed = 250,
			BattleSpeed = 250
		});
		yield return new WaitForSeconds(_doorOpeningDelay);
		if ((bool)_targetDoor)
		{
			_targetDoor.DoorClose();
		}
		yield return new WaitForSeconds(_getAxeMsgDelay);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.GetAxe);
	}

	private void ShowObject(string objName)
	{
		GameObject gameObject = KUtility.FindObjectByName(base.gameObject, objName, includeInactive: true);
		if ((bool)gameObject)
		{
			gameObject.SetActive(value: true);
		}
	}

	private void HideObject(string objName)
	{
		GameObject gameObject = KUtility.FindObjectByName(base.gameObject, objName, includeInactive: true);
		if ((bool)gameObject)
		{
			gameObject.SetActive(value: false);
		}
	}
}
