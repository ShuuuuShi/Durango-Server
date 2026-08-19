using UnityEngine;

[AddComponentMenu("NGUI/Examples/Window Drag Tilt")]
public class WindowDragTilt : MonoBehaviour
{
	public int updateOrder;

	public float degrees = 30f;

	private Vector3 mLastPos;

	private Transform mTrans;

	private float mAngle;

	private void OnEnable()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		mTrans = ((Component)this).transform;
		mLastPos = mTrans.position;
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = mTrans.position - mLastPos;
		mLastPos = mTrans.position;
		mAngle += val.x * degrees;
		mAngle = NGUIMath.SpringLerp(mAngle, 0f, 20f, Time.deltaTime);
		mTrans.localRotation = Quaternion.Euler(0f, 0f, 0f - mAngle);
	}
}
