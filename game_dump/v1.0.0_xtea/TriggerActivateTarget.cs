using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerActivateTarget : TriggerOnce
{
	public List<GameObject> _targetObjects = new List<GameObject>();

	public bool _activateTarget = true;

	public float _delay;

	protected override bool TriggerEntered(Collider other)
	{
		if (_targetObjects.Count > 0)
		{
			((MonoBehaviour)this).StartCoroutine(coTriggerBegin(_delay));
		}
		return true;
	}

	private IEnumerator coTriggerBegin(float delay)
	{
		yield return (object)new WaitForSeconds(delay);
		int count = _targetObjects.Count;
		for (int i = 0; i < count; i++)
		{
			if (Object.op_Implicit((Object)(object)_targetObjects[i]))
			{
				_targetObjects[i].SetActive(_activateTarget);
			}
		}
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}
}
