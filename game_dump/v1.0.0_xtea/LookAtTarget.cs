using UnityEngine;

[AddComponentMenu("NGUI/Examples/Look At Target")]
public class LookAtTarget : MonoBehaviour
{
	public int level;

	public Transform target;

	public float speed = 8f;

	private Transform mTrans;

	private void Start()
	{
		mTrans = ((Component)this).transform;
	}

	private void LateUpdate()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target != (Object)null)
		{
			Vector3 val = target.position - mTrans.position;
			float magnitude = ((Vector3)(ref val)).magnitude;
			if (magnitude > 0.001f)
			{
				Quaternion val2 = Quaternion.LookRotation(val);
				mTrans.rotation = Quaternion.Slerp(mTrans.rotation, val2, Mathf.Clamp01(speed * Time.deltaTime));
			}
		}
	}
}
