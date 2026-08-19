using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.Prologue;

public class TriggerActivateTarget : TriggerOnce
{
	public List<GameObject> _targetObjects = new List<GameObject>();

	public bool _activateTarget = true;

	public float _delay;

	protected override bool TriggerEntered(Collider other)
	{
		if (_targetObjects.Count > 0)
		{
			StartCoroutine(coTriggerBegin(_delay));
		}
		return true;
	}

	private IEnumerator coTriggerBegin(float delay)
	{
		yield return new WaitForSeconds(delay);
		int count = _targetObjects.Count;
		for (int i = 0; i < count; i++)
		{
			if ((bool)_targetObjects[i])
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
