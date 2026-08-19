using Durango.Model;
using UnityEngine;

public class TrapString : TrapBase
{
	public GameObject _breakModelPrefab;

	private void Start()
	{
	}

	public override void OnTrapped()
	{
		base.OnTrapped();
		if ((bool)_breakModelPrefab)
		{
			GameObject gameObject = Object.Instantiate(_breakModelPrefab);
			gameObject.transform.parent = base.gameObject.transform.parent;
			gameObject.transform.localRotation = base.gameObject.transform.localRotation;
			gameObject.transform.localPosition = base.gameObject.transform.localPosition;
			gameObject.transform.localScale = base.gameObject.transform.localScale;
			AnimatingModel componentInChildren = gameObject.GetComponentInChildren<AnimatingModel>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Play("on_trapped", loop: false);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
