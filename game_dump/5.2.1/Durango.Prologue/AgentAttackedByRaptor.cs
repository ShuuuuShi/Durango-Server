using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Network;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.Prologue;

public class AgentAttackedByRaptor : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CStart_003Ed__7 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AgentAttackedByRaptor _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CStart_003Ed__7(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			AgentAttackedByRaptor agentAttackedByRaptor = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				GameObject gameObject = KUtility.FindObjectByName(Singleton<PrologueManager>.Instance().TriggersGroup, agentAttackedByRaptor._targetDoorName, includeInactive: true);
				if ((bool)gameObject)
				{
					agentAttackedByRaptor._targetDoor = gameObject.GetComponent<TriggerDoorController>();
				}
				Singleton<PrologueTunnelController>.Instance().TransitionBgm();
				_003C_003E2__current = new WaitForSeconds(agentAttackedByRaptor._beforeAttackedDelay);
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(agentAttackedByRaptor._doorDelayBegin);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				if ((bool)agentAttackedByRaptor._targetDoor)
				{
					agentAttackedByRaptor._targetDoor.DoorOpen();
				}
				PlayerBehavior.LocalPlayer.ChangeWeaponType(PlayerBehavior.WeaponFramework.SCARED);
				Connections.Frontend.PushPacket(new SetBaseMoveSpeed
				{
					EntityId = GameManager.PlayerId,
					NormalSpeed = 250,
					BattleSpeed = 250
				});
				_003C_003E2__current = new WaitForSeconds(agentAttackedByRaptor._doorOpeningDelay);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				if ((bool)agentAttackedByRaptor._targetDoor)
				{
					agentAttackedByRaptor._targetDoor.DoorClose();
				}
				_003C_003E2__current = new WaitForSeconds(agentAttackedByRaptor._getAxeMsgDelay);
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.GetAxe);
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStart_003Ed__7(0)
		{
			_003C_003E4__this = this
		};
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
