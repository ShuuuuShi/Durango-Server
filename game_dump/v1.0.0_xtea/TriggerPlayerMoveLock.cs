using UnityEngine;

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
		KSingleton<PlayerController>.Instance().MoveLock = true;
		KSingleton<PlayerController>.Instance().StopMove();
		KSingleton<PlayerController>.Instance().EndMove();
		KSingleton<UIManager>.Instance().PlayGuideHelper.ShowArrowTargetIfEnabled(bVisible: false);
		((MonoBehaviour)this).Invoke("EndEvent", _duration);
	}

	private void EndEvent()
	{
		KSingleton<PlayerController>.Instance().MoveLock = false;
		KSingleton<UIManager>.Instance().PlayGuideHelper.ShowArrowTargetIfEnabled(bVisible: true);
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}
}
