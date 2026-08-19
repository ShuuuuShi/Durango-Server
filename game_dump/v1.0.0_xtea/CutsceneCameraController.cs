using UnityEngine;

public class CutsceneCameraController : MonoBehaviour
{
	public GameObject _cameraOrigin;

	public GameObject _cameraTarget;

	public GameObject _cameraFov;

	private Camera _curCamera;

	public float _nearPlane;

	public float _farPlane;

	public string _cameraOriginName = "Main_Camera_003";

	public string _cameraTargetName = "Main_Camera_003.Target";

	public string _cameraFovName = "attach_fov";

	private Vector3 _prevCameraPos;

	private Quaternion _prevCameraRot;

	private float _prevCameraFov;

	private float _prevNearClipPlane;

	private float _prevFarClipPlane;

	private float _prevCameraZoom;

	private float _prevCameraDistance;

	private Vector3 _lastCameraOriginPos;

	private Vector3 _lastCameraTargetPos;

	private Quaternion _lastCameraRot;

	private float _lastCameraFov;

	private float _beginTime;

	private float _outBeginTime;

	public float _interpTime = 0.5f;

	private bool bChase;

	private Camera CurCamera
	{
		get
		{
			if ((Object)null == (Object)(object)_curCamera)
			{
				_curCamera = ((Component)this).GetComponent<Camera>();
			}
			return _curCamera;
		}
	}

	public void Begin(GameObject target)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		_beginTime = Time.realtimeSinceStartup;
		_prevCameraPos = ((Component)CurCamera).transform.position;
		_prevCameraRot = ((Component)CurCamera).transform.rotation;
		_prevCameraFov = CurCamera.fieldOfView;
		_prevNearClipPlane = CurCamera.nearClipPlane;
		_prevFarClipPlane = CurCamera.farClipPlane;
		_prevCameraZoom = KSingleton<MainCamera>.Instance().Zoom;
		_prevCameraDistance = KSingleton<MainCamera>.Instance().CameraDistance;
		_cameraOrigin = KUtility.FindObjectByName(target, _cameraOriginName, includeInactive: true);
		_cameraTarget = KUtility.FindObjectByName(target, _cameraTargetName, includeInactive: true);
		_cameraFov = KUtility.FindObjectByName(target, _cameraFovName, includeInactive: true);
		bChase = true;
		((Behaviour)this).enabled = true;
	}

	public void End()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		bChase = false;
		_outBeginTime = Time.realtimeSinceStartup;
		_lastCameraOriginPos = _cameraOrigin.transform.position;
		_lastCameraTargetPos = _cameraTarget.transform.position;
		_lastCameraRot = ((Component)CurCamera).transform.rotation;
		_lastCameraFov = CurCamera.fieldOfView;
		if (Object.op_Implicit((Object)(object)KSingleton<PlayerController>.Instance()))
		{
			KSingleton<MainCamera>.Instance().ForceZoomChanged(_prevCameraZoom);
			KSingleton<CameraController>.Instance().PlayerZoom = _prevCameraZoom;
			KSingleton<CameraController>.Instance().ResetCameraTarget(1f);
		}
	}

	private void LateUpdate()
	{
		if (bChase)
		{
			ChaseIn();
		}
		else
		{
			ChaseOut();
		}
	}

	public void ForceUpdate()
	{
		LateUpdate();
	}

	private void ChaseIn()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp((Time.realtimeSinceStartup - _beginTime) / _interpTime, 0f, 1f);
		Vector3 position = _cameraOrigin.transform.position;
		Vector3 position2 = _cameraTarget.transform.position;
		float num2 = 360f - _cameraFov.transform.localEulerAngles.y;
		Vector3 val = position2 - position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		Vector3 position3 = ((Component)CurCamera).transform.position;
		Vector3 val2 = ((Component)CurCamera).transform.position + ((Component)CurCamera).transform.forward * magnitude;
		float fieldOfView = CurCamera.fieldOfView;
		float nearPlane = _nearPlane;
		float farPlane = _farPlane;
		((Component)CurCamera).transform.position = Vector3.Lerp(position3, position, num);
		((Component)CurCamera).transform.LookAt(Vector3.Lerp(val2, position2, num));
		CurCamera.fieldOfView = Mathf.Lerp(fieldOfView, num2, num);
		CurCamera.nearClipPlane = nearPlane;
		CurCamera.farClipPlane = farPlane;
		KSingleton<MainCamera>.Instance().UpdateCameraNearFar(Mathf.Lerp(_prevCameraDistance, magnitude, num));
	}

	private void ChaseOut()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp((Time.realtimeSinceStartup - _outBeginTime) / _interpTime, 0f, 1f);
		Vector3 lastCameraOriginPos = _lastCameraOriginPos;
		Vector3 lastCameraTargetPos = _lastCameraTargetPos;
		float lastCameraFov = _lastCameraFov;
		Vector3 val = lastCameraTargetPos - lastCameraOriginPos;
		float magnitude = ((Vector3)(ref val)).magnitude;
		Vector3 position = ((Component)CurCamera).transform.position;
		Vector3 val2 = ((Component)CurCamera).transform.position + ((Component)CurCamera).transform.forward * magnitude;
		float fieldOfView = CurCamera.fieldOfView;
		float prevNearClipPlane = _prevNearClipPlane;
		float prevFarClipPlane = _prevFarClipPlane;
		((Component)CurCamera).transform.position = Vector3.Lerp(lastCameraOriginPos, position, num);
		((Component)CurCamera).transform.LookAt(Vector3.Lerp(lastCameraTargetPos, val2, num));
		CurCamera.fieldOfView = Mathf.Lerp(lastCameraFov, fieldOfView, num);
		CurCamera.nearClipPlane = prevNearClipPlane;
		CurCamera.farClipPlane = prevFarClipPlane;
		if (num >= 1f)
		{
			((Behaviour)this).enabled = false;
		}
		KSingleton<MainCamera>.Instance().UpdateCameraNearFar(magnitude);
	}
}
