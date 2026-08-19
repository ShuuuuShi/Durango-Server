using UnityEngine;

[AddComponentMenu("NGUI/Examples/Lag Rotation")]
public class LagRotation : MonoBehaviour
{
	public float speed = 10f;

	public bool ignoreTimeScale;

	private Transform mTrans;

	private Quaternion mRelative;

	private Quaternion mAbsolute;

	public void OnRepositionEnd()
	{
		Interpolate(1000f);
	}

	private void Interpolate(float delta)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mTrans != (Object)null)
		{
			Transform parent = mTrans.parent;
			if ((Object)(object)parent != (Object)null)
			{
				mAbsolute = Quaternion.Slerp(mAbsolute, parent.rotation * mRelative, delta * speed);
				mTrans.rotation = mAbsolute;
			}
		}
	}

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		mTrans = ((Component)this).transform;
		mRelative = mTrans.localRotation;
		mAbsolute = mTrans.rotation;
	}

	private void Update()
	{
		Interpolate((!ignoreTimeScale) ? Time.deltaTime : RealTime.deltaTime);
	}
}
