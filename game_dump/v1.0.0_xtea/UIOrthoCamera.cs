using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("NGUI/UI/Orthographic Camera")]
public class UIOrthoCamera : MonoBehaviour
{
	private Camera mCam;

	private Transform mTrans;

	private void Start()
	{
		mCam = ((Component)this).GetComponent<Camera>();
		mTrans = ((Component)this).transform;
		mCam.orthographic = true;
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = mCam.rect;
		float num = ((Rect)(ref rect)).yMin * (float)Screen.height;
		Rect rect2 = mCam.rect;
		float num2 = ((Rect)(ref rect2)).yMax * (float)Screen.height;
		float num3 = (num2 - num) * 0.5f * mTrans.lossyScale.y;
		if (!Mathf.Approximately(mCam.orthographicSize, num3))
		{
			mCam.orthographicSize = num3;
		}
	}
}
