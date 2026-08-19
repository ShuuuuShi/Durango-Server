using UnityEngine;

[AddComponentMenu("NGUI/Examples/Spin")]
public class Spin : MonoBehaviour
{
	public Vector3 rotationsPerSecond = new Vector3(0f, 0.1f, 0f);

	public bool ignoreTimeScale;

	private Rigidbody mRb;

	private Transform mTrans;

	private void Start()
	{
		mTrans = ((Component)this).transform;
		mRb = ((Component)this).GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if ((Object)(object)mRb == (Object)null)
		{
			ApplyDelta((!ignoreTimeScale) ? Time.deltaTime : RealTime.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if ((Object)(object)mRb != (Object)null)
		{
			ApplyDelta(Time.deltaTime);
		}
	}

	public void ApplyDelta(float delta)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		delta *= 360f;
		Quaternion val = Quaternion.Euler(rotationsPerSecond * delta);
		if ((Object)(object)mRb == (Object)null)
		{
			mTrans.rotation *= val;
		}
		else
		{
			mRb.MoveRotation(mRb.rotation * val);
		}
	}
}
