using UnityEngine;

public class LagPosition : MonoBehaviour
{
	public Vector3 speed = new Vector3(10f, 10f, 10f);

	public bool ignoreTimeScale;

	private Transform mTrans;

	private Vector3 mRelative;

	private Vector3 mAbsolute;

	private bool mStarted;

	public void OnRepositionEnd()
	{
		Interpolate(1000f);
	}

	private void Interpolate(float delta)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = mTrans.parent;
		if ((Object)(object)parent != (Object)null)
		{
			Vector3 val = parent.position + parent.rotation * mRelative;
			mAbsolute.x = Mathf.Lerp(mAbsolute.x, val.x, Mathf.Clamp01(delta * speed.x));
			mAbsolute.y = Mathf.Lerp(mAbsolute.y, val.y, Mathf.Clamp01(delta * speed.y));
			mAbsolute.z = Mathf.Lerp(mAbsolute.z, val.z, Mathf.Clamp01(delta * speed.z));
			mTrans.position = mAbsolute;
		}
	}

	private void Awake()
	{
		mTrans = ((Component)this).transform;
	}

	private void OnEnable()
	{
		if (mStarted)
		{
			ResetPosition();
		}
	}

	private void Start()
	{
		mStarted = true;
		ResetPosition();
	}

	public void ResetPosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		mAbsolute = mTrans.position;
		mRelative = mTrans.localPosition;
	}

	private void Update()
	{
		Interpolate((!ignoreTimeScale) ? Time.deltaTime : RealTime.deltaTime);
	}
}
