using System.Collections;
using UnityEngine;

public class TriggerActing : TriggerOnce
{
	public NPCActorBehavior _actor;

	public string _walkMotion;

	public string _afterWalkMotion;

	public float _moveSpeed = 200f;

	public Vector3 _moveDestPosition;

	public float _destYaw;

	public GameObject _onFinishListener;

	public string _onFinishCmd;

	protected override bool TriggerEntered(Collider other)
	{
		BeginEvent();
		return true;
	}

	private void BeginEvent()
	{
		((MonoBehaviour)this).StartCoroutine(coWalkToSit());
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}

	public void RotateToPosition(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		float num = KMathUtil.CalcYawWithTarget(pos, ((Component)_actor).transform.position);
		((Component)_actor).transform.localRotation = Quaternion.Euler(0f, num, 0f);
	}

	private IEnumerator coWalkToSit()
	{
		_actor.CrossFade(_walkMotion, 0.5f);
		RotateToPosition(_moveDestPosition);
		Vector3 val;
		do
		{
			((Component)_actor).transform.position = Vector3.MoveTowards(((Component)_actor).transform.position, _moveDestPosition, _moveSpeed * Time.deltaTime);
			yield return null;
			val = _moveDestPosition - ((Component)_actor).transform.position;
		}
		while (!(((Vector3)(ref val)).magnitude < 10f));
		((Component)_actor).transform.position = _moveDestPosition;
		((Component)_actor).transform.localRotation = Quaternion.Euler(0f, _destYaw, 0f);
		_actor.Play(_afterWalkMotion);
		if (Object.op_Implicit((Object)(object)_onFinishListener))
		{
			if (!_onFinishListener.activeSelf)
			{
				_onFinishListener.SetActive(true);
			}
			if (_onFinishCmd != string.Empty)
			{
				_onFinishListener.SendMessage(_onFinishCmd);
			}
		}
	}
}
