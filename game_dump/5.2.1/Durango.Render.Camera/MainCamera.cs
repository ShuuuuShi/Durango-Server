using System;
using Durango.System;
using Durango.System.Config;
using Durango.Utils;
using Shared.Battle;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Durango.Render.Camera;

public class MainCamera : Singleton<MainCamera>
{
	[SerializeField]
	public Transform TargetTransform;

	[SerializeField]
	private float _maxHeightForNearPlane = 4000f;

	[SerializeField]
	private float _additionalHeightForFarPlane = 200f;

	private float _fov = 5f;

	[SerializeField]
	private float _manualFov;

	private float _prevFinalZoom;

	private float _zoom = 1f;

	private bool _needUpdateCamera;

	private Vector3 _defaultCameraAngle;

	private Vector3 _angleOffset;

	public float Distance
	{
		get
		{
			if (_manualFov > 0f && _fov > 0f)
			{
				return 50000f / _fov;
			}
			return 10000f;
		}
	}

	public UnityEngine.Camera Camera { get; private set; }

	public float CameraDistance { get; private set; }

	public float Fov
	{
		get
		{
			return _fov;
		}
		set
		{
			_fov = value;
			Camera.fieldOfView = _fov;
		}
	}

	public Vector3 LastCameraTargetPos { get; private set; }

	[ExposedInEditor(null)]
	public float Zoom
	{
		get
		{
			return _zoom;
		}
		set
		{
			if (_zoom != value)
			{
				_zoom = value;
				_needUpdateCamera = true;
			}
		}
	}

	public Vector3 AngleOffset
	{
		get
		{
			return _angleOffset;
		}
		set
		{
			_angleOffset = value;
		}
	}

	public Vector3 CameraAngle { get; private set; }

	public RenderTexture TargetTexture
	{
		get
		{
			return Camera.targetTexture;
		}
		set
		{
			Camera.targetTexture = value;
		}
	}

	public event Action CameraUpdated;

	public static float NGUIScale()
	{
		return (float)UIManager.ScreenWidth / (float)UnityEngine.Screen.width;
	}

	public static Vector3 WorldToScreenPos(Vector3 world)
	{
		return Singleton<MainCamera>.Instance().Camera.WorldToScreenPoint(world);
	}

	public static Vector3 WorldToNGUIPos(Vector3 world, Transform relativeTo = null)
	{
		return ScreenPosToNGUIPos(Singleton<MainCamera>.Instance().Camera.WorldToScreenPoint(world), relativeTo);
	}

	public static Vector3 NGUIPosToWorldPos(Vector3 nguiPos)
	{
		return ScreenPosToWorldPos(NGUIPosToScreenPos(nguiPos));
	}

	public static Vector3 NGUIPosToScreenPos(Vector3 nguiPos)
	{
		float num = NGUIScale();
		nguiPos.x /= num;
		nguiPos.y /= num;
		nguiPos.x += (float)UnityEngine.Screen.width / 2f;
		nguiPos.y += (float)UnityEngine.Screen.height / 2f;
		return nguiPos;
	}

	public static Vector3 ScreenPosToWorldPos(Vector3 unityPos)
	{
		return ScreenPosToWorldPos(unityPos, 0f);
	}

	public static Vector3 ScreenPosToWorldPos(Vector3 unityPos, float height)
	{
		return RayToWorldPos(Singleton<MainCamera>.Instance().Camera.ScreenPointToRay(unityPos), height);
	}

	public static Vector3 RayToWorldPos(Ray ray)
	{
		return RayToWorldPos(ray, 0f);
	}

	public static Vector3 RayToWorldPos(Ray ray, float height)
	{
		return ray.origin - (ray.origin.y - height) / ray.direction.y * ray.direction;
	}

	public static Vector3 ScreenPosToNGUIPos(Vector3 nguiPos, Transform relativeTo = null)
	{
		nguiPos.x -= (float)UnityEngine.Screen.width / 2f;
		nguiPos.y -= (float)UnityEngine.Screen.height / 2f;
		float num = NGUIScale();
		nguiPos.x *= num;
		nguiPos.y *= num;
		nguiPos.z = 0f;
		if (relativeTo == null)
		{
			return nguiPos;
		}
		GameObject root = NGUITools.GetRoot(relativeTo.gameObject);
		return relativeTo.InverseTransformPoint(root.transform.TransformPoint(nguiPos));
	}

	public static Ray WorldToScreenRay(Vector3 pos)
	{
		Vector3 position = Singleton<MainCamera>.Instance().Camera.WorldToScreenPoint(pos);
		return Singleton<MainCamera>.Instance().Camera.ScreenPointToRay(position);
	}

	protected override void OnAwake()
	{
		Camera = GetComponent<UnityEngine.Camera>();
		Camera.transparencySortMode = TransparencySortMode.Orthographic;
		_defaultCameraAngle = base.transform.localEulerAngles;
		if (Platform.Instance.UsePCRenderer)
		{
			PostProcessLayer component = GetComponent<PostProcessLayer>();
			if (component != null)
			{
				component.enabled = true;
			}
			PostProcessVolume component2 = GetComponent<PostProcessVolume>();
			if (component2 != null)
			{
				component2.enabled = true;
				ConfigInstance.UpdatePostProcessingSettings();
			}
		}
	}

	private void OnEnable()
	{
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(OnScreenResize));
		OnScreenResize();
	}

	private void OnDisable()
	{
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Remove(UICamera.onScreenResize, new UICamera.OnScreenResize(OnScreenResize));
	}

	private void OnScreenResize()
	{
		if (_manualFov > 0f)
		{
			Fov = _manualFov;
		}
		else if (!Camera.orthographic)
		{
			float num = UnityEngine.Screen.width;
			float num2 = Mathf.Clamp01((float)UnityEngine.Screen.height / num);
			Fov = 8f * num2;
		}
		_needUpdateCamera = true;
	}

	public void UpdateCameraTarget(Vector3 target)
	{
		if (LastCameraTargetPos.y != target.y)
		{
			_needUpdateCamera = true;
		}
		LastCameraTargetPos = target;
		UpdateCameraPosition();
		Shader.SetGlobalFloat("_ZoomRatio", 1f / _zoom);
	}

	public void PostUpdateCameraTarget(Vector3 offset)
	{
		base.transform.position += offset;
		Transform bodyPartTransform = PlayerBehavior.LocalPlayer.GetBodyPartTransform(BodyPart.Body);
		Vector3 vector = Camera.WorldToViewportPoint(bodyPartTransform.position);
		Shader.SetGlobalVector("_CameraTarget", new Vector2(vector.x, vector.y));
		if (this.CameraUpdated != null)
		{
			this.CameraUpdated();
		}
	}

	private void UpdateCameraPosition()
	{
		Vector3 angleOffset = _angleOffset;
		if (CameraAngle != angleOffset)
		{
			CameraAngle = angleOffset;
			_needUpdateCamera = true;
		}
		angleOffset += _defaultCameraAngle + Singleton<AccelerationChecker>.Instance().FinalCamLeanAngle;
		base.transform.localEulerAngles = angleOffset;
		Vector3 lastCameraTargetPos = LastCameraTargetPos;
		float num = Mathf.Max(_zoom, 0.01f);
		int num2;
		if (!_needUpdateCamera)
		{
			num2 = ((Mathf.Abs(_prevFinalZoom - num) > 0.001f) ? 1 : 0);
			if (num2 == 0)
			{
				goto IL_009a;
			}
		}
		else
		{
			num2 = 1;
		}
		_prevFinalZoom = num;
		CameraDistance = Distance / num;
		goto IL_009a;
		IL_009a:
		Transform transform = base.transform;
		transform.localPosition = lastCameraTargetPos + transform.forward * (0f - CameraDistance);
		if (TargetTransform != null)
		{
			TargetTransform.SetPositionAndRotation(lastCameraTargetPos, Quaternion.identity);
		}
		if (num2 != 0)
		{
			UpdateCameraNearFar();
		}
		_needUpdateCamera = false;
	}

	public void UpdateCameraNearFar()
	{
		float num = base.transform.localEulerAngles.x * ((float)Math.PI / 180f);
		float num2 = Fov * 0.5f * ((float)Math.PI / 180f);
		float num3 = Mathf.Cos(num2 * ((float)Math.PI / 180f));
		float f = (float)Math.PI / 2f - num2 - num;
		float a = (base.transform.localPosition.y - _maxHeightForNearPlane) / Mathf.Cos(f) * num3 - 100f;
		float num4 = base.transform.localPosition.y + _additionalHeightForFarPlane;
		float f2 = num - num2;
		float a2 = num4 / Mathf.Sin(f2) * num3;
		Camera.nearClipPlane = Mathf.Max(a, 200f);
		Camera.farClipPlane = Mathf.Max(a2, 1000f);
	}

	public Vector3 WorldPositionToScreenPos(Vector3 pos)
	{
		return Camera.WorldToScreenPoint(pos);
	}

	public Ray ScreenPointToRay(Vector3 pos)
	{
		return Camera.ScreenPointToRay(pos);
	}
}
