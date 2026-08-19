using System;
using UnityEngine;

public class MainCamera : KSingleton<MainCamera>
{
	[SerializeField]
	public Transform TargetTransform;

	[SerializeField]
	private float _lookatHeightOffset = 100f;

	[SerializeField]
	private float _maxHeightForNearPlane = 4000f;

	[SerializeField]
	private float _minDist = 4500f;

	[SerializeField]
	private float _maxDist = 15300f;

	[SerializeField]
	private float _portaitMinDist = 4500f;

	[SerializeField]
	private float _portaitMaxDist = 19300f;

	[SerializeField]
	private float _minZoom = 0.5f;

	[SerializeField]
	private float _maxZoom = 2.5f;

	private Camera _camera;

	private float _fovY = 5f;

	private float _zoomScale = 1f;

	private float _prevFinalZoom;

	private float _zoom;

	private bool _needUpdateCamera;

	private float _cameraDistanceOverride = -1f;

	public float CameraDistance { get; private set; }

	public float ZoomScreenRatio { get; private set; }

	public float FovX
	{
		get
		{
			return FovY * DeviceInfo.AspectRatio;
		}
		set
		{
			FovY = value / DeviceInfo.AspectRatio;
		}
	}

	public float FovY
	{
		get
		{
			return _fovY;
		}
		set
		{
			_fovY = value;
			_camera.fieldOfView = _fovY;
		}
	}

	public Vector3 LastCameraTargetPos { get; private set; }

	public float MinZoom
	{
		get
		{
			return _minZoom;
		}
		set
		{
			_minZoom = value;
		}
	}

	public float MaxZoom => _maxZoom;

	[ExposedInEditor(null)]
	public float Zoom
	{
		get
		{
			return _zoom;
		}
		set
		{
			float zoom = _zoom;
			_zoom = Mathf.Clamp(value, MinZoom, MaxZoom);
			if (zoom != _zoom)
			{
				UpdateCameraPosition();
			}
		}
	}

	public float ZoomScale
	{
		get
		{
			return _zoomScale;
		}
		set
		{
			float zoomScale = _zoomScale;
			_zoomScale = value;
			if (zoomScale != _zoomScale)
			{
				UpdateCameraPosition();
			}
		}
	}

	public float FinalZoom => _zoom * ZoomScale;

	public float CameraDistanceOverride
	{
		get
		{
			return _cameraDistanceOverride;
		}
		set
		{
			if (Mathf.Abs(_cameraDistanceOverride - value) > Mathf.Epsilon)
			{
				_needUpdateCamera = true;
			}
			_cameraDistanceOverride = value;
		}
	}

	public RenderTexture TargetTexture
	{
		get
		{
			return _camera.targetTexture;
		}
		set
		{
			_camera.targetTexture = value;
		}
	}

	public void ForceZoomChanged(float zoom)
	{
		_prevFinalZoom = 0f;
		Zoom = zoom;
	}

	public static float NGUIScale()
	{
		return (float)UIManager.ScreenWidth / (float)Screen.width;
	}

	public static Vector3 NGUILocalPositionToNGUIPosition(Vector3 localPos, Transform parent)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Transform val = parent;
		while ((Object)(object)val != (Object)null && (Object)(object)val.parent != (Object)null)
		{
			localPos = Vector3.Scale(localPos, val.localScale) + val.localPosition;
			val = val.parent;
		}
		return localPos;
	}

	public static Vector3 NGUIPositionToNGUILocalPosition(Vector3 nguiPos, Transform relativeTo)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Transform val = relativeTo;
		while ((Object)(object)val != (Object)null && (Object)(object)val.parent != (Object)null)
		{
			nguiPos -= val.localPosition;
			Vector3 localScale = val.localScale;
			if (localScale != Vector3.one)
			{
				nguiPos.x /= localScale.x;
				nguiPos.y /= localScale.y;
				nguiPos.z /= localScale.z;
			}
			val = val.parent;
		}
		return nguiPos;
	}

	public static Vector3 WorldToScreenPos(Vector3 world)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return KSingleton<MainCamera>.Instance()._camera.WorldToScreenPoint(world);
	}

	public static Vector3 WorldToNGUIPos(Vector3 world, Transform relativeTo = null)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 nguiPos = KSingleton<MainCamera>.Instance()._camera.WorldToScreenPoint(world);
		nguiPos.x -= (float)Screen.width / 2f;
		nguiPos.y -= (float)Screen.height / 2f;
		float num = NGUIScale();
		nguiPos.x *= num;
		nguiPos.y *= num;
		nguiPos.z = 0f;
		return NGUIPositionToNGUILocalPosition(nguiPos, relativeTo);
	}

	public static Vector3 NGUIPosToWorldPos(Vector3 nguiPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Vector3 unityPos = NGUIPosToScreenPos(nguiPos);
		return ScreenPosToWorldPos(unityPos);
	}

	public static Vector3 NGUIPosToScreenPos(Vector3 nguiPos)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float num = NGUIScale();
		nguiPos.x /= num;
		nguiPos.y /= num;
		nguiPos.x += (float)Screen.width / 2f;
		nguiPos.y += (float)Screen.height / 2f;
		return nguiPos;
	}

	public static Vector3 ScreenPosToWorldPos(Vector3 unityPos)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Ray val = KSingleton<MainCamera>.Instance()._camera.ScreenPointToRay(unityPos);
		unityPos = ((Ray)(ref val)).origin - ((Ray)(ref val)).origin.y / ((Ray)(ref val)).direction.y * ((Ray)(ref val)).direction;
		return unityPos;
	}

	public static Vector3 ScreenPosToNGUIPos(Vector3 nguiPos, Transform relativeTo = null)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		nguiPos.x -= (float)Screen.width / 2f;
		nguiPos.y -= (float)Screen.height / 2f;
		float num = NGUIScale();
		nguiPos.x *= num;
		nguiPos.y *= num;
		nguiPos.z = 0f;
		nguiPos = NGUIPositionToNGUILocalPosition(nguiPos, relativeTo);
		return nguiPos;
	}

	public static Ray WorldToScreenRay(Vector3 pos)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = KSingleton<MainCamera>.Instance()._camera.WorldToScreenPoint(pos);
		return KSingleton<MainCamera>.Instance()._camera.ScreenPointToRay(val);
	}

	protected override void OnAwake()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.IsPrologueMode)
		{
			Application.targetFrameRate = 60;
		}
		else
		{
			Application.targetFrameRate = 30;
		}
		ZoomScreenRatio = 1f;
		_camera = ((Component)this).GetComponent<Camera>();
		if (!_camera.orthographic)
		{
			FovX = 8f;
		}
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.y = _lookatHeightOffset;
		((Component)this).transform.localPosition = localPosition;
		Zoom = 1f;
		_camera.transparencySortMode = (TransparencySortMode)2;
	}

	private void OnEnable()
	{
		UIManager.PortraitModeChanged += UIManager_PortraitModeChanged;
	}

	private void OnDisable()
	{
		UIManager.PortraitModeChanged -= UIManager_PortraitModeChanged;
	}

	private void UIManager_PortraitModeChanged()
	{
		_needUpdateCamera = true;
	}

	public void UpdateCameraTarget(Vector3 target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		LastCameraTargetPos = target;
		UpdateCameraPosition();
	}

	private void UpdateCameraPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		Vector3 lastCameraTargetPos = LastCameraTargetPos;
		bool flag = Mathf.Abs(_prevFinalZoom - FinalZoom) > 0.001f;
		if (flag || _needUpdateCamera)
		{
			_prevFinalZoom = FinalZoom;
			float num = ((!UIManager.IsPortraitMode) ? _minDist : _portaitMinDist);
			float num2 = ((!UIManager.IsPortraitMode) ? _maxDist : _portaitMaxDist);
			float num3 = _maxZoom - _minZoom;
			float num4 = (FinalZoom - _minZoom) / num3;
			float num5 = (MinZoom - _minZoom) / num3;
			num4 = Mathf.Clamp(num4, num5, 1f);
			CameraDistance = Mathf.LerpUnclamped(num2, num, num4);
			if (CameraDistanceOverride > 0f)
			{
				CameraDistance = CameraDistanceOverride;
			}
			float num6 = num2 - num;
			float num7 = CameraDistance - _minDist;
			ZoomScreenRatio = 1f + num7 / num6;
		}
		((Component)_camera).transform.localPosition = lastCameraTargetPos + ((Component)_camera).transform.forward * (0f - CameraDistance);
		if ((Object)(object)TargetTransform != (Object)null)
		{
			TargetTransform.rotation = Quaternion.identity;
			TargetTransform.position = lastCameraTargetPos;
		}
		if (flag || _needUpdateCamera)
		{
			UpdateCameraNearFar();
		}
		_needUpdateCamera = false;
	}

	public void UpdateCameraNearFar(float curCameraDistance = -1f)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Tan((float)Math.PI * 7f / 36f) / Mathf.Tan(FovY * (float)Math.PI / 180f);
		float num2 = ((Component)_camera).transform.localPosition.y - _maxHeightForNearPlane;
		float num3 = num2 / Mathf.Sin((float)Math.PI * 7f / 36f) - 100f;
		if (curCameraDistance < 0f)
		{
			curCameraDistance = CameraDistance;
		}
		float num4 = curCameraDistance * num / (num - 1f) + 200f;
		_camera.nearClipPlane = Mathf.Max(num3, 200f);
		_camera.farClipPlane = Mathf.Max(num4, 1000f);
	}
}
