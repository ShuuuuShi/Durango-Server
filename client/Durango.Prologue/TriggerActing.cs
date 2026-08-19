using System.Collections;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class TriggerActing : TriggerOnce
{
	public CostumeActorBehavior _actor;

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
		StartCoroutine(coWalkToSit());
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}

	public void RotateToPosition(Vector3 pos)
	{
		float y = Maths.CalcYawWithTarget(pos, _actor.transform.position);
		_actor.transform.localRotation = Quaternion.Euler(0f, y, 0f);
	}

	private IEnumerator coWalkToSit()
	{
		_actor.CrossFade(_walkMotion, 0.5f);
		RotateToPosition(_moveDestPosition);
		do
		{
			_actor.transform.position = Vector3.MoveTowards(_actor.transform.position, _moveDestPosition, _moveSpeed * Time.deltaTime);
			yield return null;
		}
		while (!((_moveDestPosition - _actor.transform.position).magnitude < 10f));
		_actor.transform.position = _moveDestPosition;
		_actor.transform.localRotation = Quaternion.Euler(0f, _destYaw, 0f);
		_actor.Play(_afterWalkMotion);
		if ((bool)_onFinishListener)
		{
			if (!_onFinishListener.activeSelf)
			{
				_onFinishListener.SetActive(value: true);
			}
			if (_onFinishCmd != string.Empty)
			{
				_onFinishListener.SendMessage(_onFinishCmd);
			}
		}
	}
}
