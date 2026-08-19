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
			GameObject obj = Object.Instantiate(_breakModelPrefab);
			obj.transform.parent = base.gameObject.transform.parent;
			obj.transform.localRotation = base.gameObject.transform.localRotation;
			obj.transform.localPosition = base.gameObject.transform.localPosition;
			obj.transform.localScale = base.gameObject.transform.localScale;
			AnimatingModel componentInChildren = obj.GetComponentInChildren<AnimatingModel>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Play("on_trapped", loop: false);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
