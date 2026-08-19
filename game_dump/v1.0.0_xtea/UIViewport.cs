using UnityEngine;

[AddComponentMenu("NGUI/UI/Viewport Camera")]
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class UIViewport : MonoBehaviour
{
	public Camera sourceCamera;

	public Transform topLeft;

	public Transform bottomRight;

	public float fullSize = 1f;

	private Camera mCam;

	private void Start()
	{
		mCam = ((Component)this).GetComponent<Camera>();
		if ((Object)(object)sourceCamera == (Object)null)
		{
			sourceCamera = Camera.main;
		}
	}

	private void LateUpdate()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)topLeft != (Object)null) || !((Object)(object)bottomRight != (Object)null))
		{
			return;
		}
		if (((Component)topLeft).gameObject.activeInHierarchy)
		{
			Vector3 val = sourceCamera.WorldToScreenPoint(topLeft.position);
			Vector3 val2 = sourceCamera.WorldToScreenPoint(bottomRight.position);
			Rect val3 = default(Rect);
			((Rect)(ref val3))._002Ector(val.x / (float)Screen.width, val2.y / (float)Screen.height, (val2.x - val.x) / (float)Screen.width, (val.y - val2.y) / (float)Screen.height);
			float num = fullSize * ((Rect)(ref val3)).height;
			if (val3 != mCam.rect)
			{
				mCam.rect = val3;
			}
			if (mCam.orthographicSize != num)
			{
				mCam.orthographicSize = num;
			}
			((Behaviour)mCam).enabled = true;
		}
		else
		{
			((Behaviour)mCam).enabled = false;
		}
	}
}
