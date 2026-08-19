using UnityEngine;

public class TrapString : TrapBase
{
	public GameObject _breakModelPrefab;

	private void Start()
	{
	}

	public override void OnTrapped()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		base.OnTrapped();
		if (Object.op_Implicit((Object)(object)_breakModelPrefab))
		{
			GameObject val = Object.Instantiate<GameObject>(_breakModelPrefab);
			val.transform.parent = ((Component)this).gameObject.transform.parent;
			val.transform.localRotation = ((Component)this).gameObject.transform.localRotation;
			val.transform.localPosition = ((Component)this).gameObject.transform.localPosition;
			val.transform.localScale = ((Component)this).gameObject.transform.localScale;
			AnimatingProp componentInChildren = val.GetComponentInChildren<AnimatingProp>();
			if (Object.op_Implicit((Object)(object)componentInChildren))
			{
				componentInChildren.Play("on_trapped", loop: false);
			}
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
