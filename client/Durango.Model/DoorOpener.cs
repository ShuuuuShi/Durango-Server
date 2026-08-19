using System.Collections;
using Durango.Utils;
using UnityEngine;

namespace Durango.Model;

public class DoorOpener : MonoBehaviour
{
	[SerializeField]
	private Transform _doorTarget;

	private int _overlappedCount;

	private float _openTargetYaw;

	private ICoroutineBinder _closeRoutine;

	private bool _fullyOpen;

	private void OnTriggerEnter(Collider other)
	{
		if (!(other.tag != "Player"))
		{
			_overlappedCount++;
			Vector3 lhs = base.transform.position - other.transform.position;
			float num = Vector3.Dot(lhs, base.transform.localToWorldMatrix * Vector3.right);
			_openTargetYaw = ((!(num < 0f)) ? (-80f) : 80f);
			this.StopCoroutine(_closeRoutine);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!_fullyOpen)
		{
			_doorTarget.localRotation = Quaternion.RotateTowards(_doorTarget.localRotation, Quaternion.Euler(_openTargetYaw * Vector3.up), Time.deltaTime * 300f);
			if (Mathf.Approximately((_doorTarget.localRotation.eulerAngles.y - _openTargetYaw) % 360f, 0f))
			{
				_fullyOpen = true;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!(other.tag != "Player"))
		{
			_overlappedCount--;
			if (_overlappedCount == 0)
			{
				this.StartCoroutine(ref _closeRoutine, CoClose());
			}
		}
	}

	private IEnumerator CoClose()
	{
		_fullyOpen = false;
		float initialYaw2 = _doorTarget.localRotation.eulerAngles.y % 360f;
		initialYaw2 = ((!(initialYaw2 > 180f)) ? initialYaw2 : (initialYaw2 - 360f));
		float maxYaw = initialYaw2;
		float elapsed = 0f;
		while (true)
		{
			maxYaw = ((initialYaw2 != 0f) ? Mathf.MoveTowards(maxYaw, 0f, Time.deltaTime * 90f * (maxYaw / initialYaw2)) : 0f);
			elapsed += Time.deltaTime * 5f;
			float targetYaw = Mathf.Cos(elapsed) * maxYaw;
			_doorTarget.localRotation = Quaternion.Euler(Vector3.up * targetYaw);
			if (Mathf.Approximately(maxYaw, 0f))
			{
				break;
			}
			yield return null;
		}
	}
}
