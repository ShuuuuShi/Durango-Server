using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class CutsceneCameraController : MonoBehaviour
{
	public GameObject _cameraOrigin;

	public GameObject _cameraTarget;

	public GameObject _cameraFov;

	public float _nearPlane;

	public float _farPlane;

	public string _cameraOriginName = "Main_Camera_003";

	public string _cameraTargetName = "Main_Camera_003.Target";

	public string _cameraFovName = "attach_fov";

	public float _interpTime = 0.5f;

	private Camera _curCamera;

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

	private bool _isChase;

	private Camera CurCamera
	{
		get
		{
			if (null == _curCamera)
			{
				_curCamera = GetComponent<Camera>();
			}
			return _curCamera;
		}
	}

	public void Begin(GameObject target)
	{
		_beginTime = Time.realtimeSinceStartup;
		_prevCameraPos = CurCamera.transform.position;
		_prevCameraRot = CurCamera.transform.rotation;
		_prevCameraFov = CurCamera.fieldOfView;
		_prevNearClipPlane = CurCamera.nearClipPlane;
		_prevFarClipPlane = CurCamera.farClipPlane;
		_prevCameraZoom = Singleton<MainCamera>.Instance().Zoom;
		_prevCameraDistance = Singleton<MainCamera>.Instance().CameraDistance;
		_cameraOrigin = KUtility.FindObjectByName(target, _cameraOriginName, includeInactive: true);
		_cameraTarget = KUtility.FindObjectByName(target, _cameraTargetName, includeInactive: true);
		_cameraFov = KUtility.FindObjectByName(target, _cameraFovName, includeInactive: true);
		_isChase = true;
		base.enabled = true;
	}

	public void End()
	{
		_isChase = false;
		_outBeginTime = Time.realtimeSinceStartup;
		_lastCameraOriginPos = _cameraOrigin.transform.position;
		_lastCameraTargetPos = _cameraTarget.transform.position;
		_lastCameraRot = CurCamera.transform.rotation;
		_lastCameraFov = CurCamera.fieldOfView;
		if ((bool)Singleton<PlayerController>.Instance())
		{
			Singleton<CameraController>.Instance().Target(null, 1f).Offset(Vector3.zero, 1f)
				.ZoomRatio(1f, 1f);
		}
	}

	private void LateUpdate()
	{
		if (_isChase)
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
		float t = Mathf.Clamp((Time.realtimeSinceStartup - _beginTime) / _interpTime, 0f, 1f);
		Vector3 position = _cameraOrigin.transform.position;
		Vector3 position2 = _cameraTarget.transform.position;
		float b = 360f - _cameraFov.transform.localEulerAngles.y;
		float magnitude = (position2 - position).magnitude;
		Vector3 position3 = CurCamera.transform.position;
		Vector3 a = CurCamera.transform.position + CurCamera.transform.forward * magnitude;
		float fieldOfView = CurCamera.fieldOfView;
		float nearPlane = _nearPlane;
		float farPlane = _farPlane;
		CurCamera.transform.position = Vector3.Lerp(position3, position, t);
		CurCamera.transform.LookAt(Vector3.Lerp(a, position2, t));
		CurCamera.fieldOfView = Mathf.Lerp(fieldOfView, b, t);
		CurCamera.nearClipPlane = nearPlane;
		CurCamera.farClipPlane = farPlane;
		Singleton<MainCamera>.Instance().UpdateCameraNearFar();
	}

	private void ChaseOut()
	{
		float num = Mathf.Clamp((Time.realtimeSinceStartup - _outBeginTime) / _interpTime, 0f, 1f);
		Vector3 lastCameraOriginPos = _lastCameraOriginPos;
		Vector3 lastCameraTargetPos = _lastCameraTargetPos;
		float lastCameraFov = _lastCameraFov;
		float magnitude = (lastCameraTargetPos - lastCameraOriginPos).magnitude;
		Vector3 position = CurCamera.transform.position;
		Vector3 b = CurCamera.transform.position + CurCamera.transform.forward * magnitude;
		float fieldOfView = CurCamera.fieldOfView;
		float prevNearClipPlane = _prevNearClipPlane;
		float prevFarClipPlane = _prevFarClipPlane;
		CurCamera.transform.position = Vector3.Lerp(lastCameraOriginPos, position, num);
		CurCamera.transform.LookAt(Vector3.Lerp(lastCameraTargetPos, b, num));
		CurCamera.fieldOfView = Mathf.Lerp(lastCameraFov, fieldOfView, num);
		CurCamera.nearClipPlane = prevNearClipPlane;
		CurCamera.farClipPlane = prevFarClipPlane;
		if (num >= 1f)
		{
			base.enabled = false;
		}
		Singleton<MainCamera>.Instance().UpdateCameraNearFar();
	}
}
