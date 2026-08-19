using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class TriggerPlayerMoveLock : TriggerOnce
{
	public float _duration = 3f;

	public GameObject _onFinishListener;

	public string _onFinishCmd;

	protected override bool TriggerEntered(Collider other)
	{
		BeginEvent();
		return true;
	}

	private void BeginEvent()
	{
		GameSystem<InputSystem>.Instance().MoveLock = true;
		Singleton<PlayerController>.Instance().StopMove();
		Singleton<PrologueManager>.Instance().PlayGuideHelper.ShowTargetIfEnabled(visible: false);
		Invoke("EndEvent", _duration);
	}

	private void EndEvent()
	{
		GameSystem<InputSystem>.Instance().MoveLock = false;
		Singleton<PrologueManager>.Instance().PlayGuideHelper.ShowTargetIfEnabled(visible: true);
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}
}
