using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("2D Toolkit/Camera/tk2dCamera")]
[ExecuteInEditMode]
public class tk2dCamera : MonoBehaviour
{
	private static int CURRENT_VERSION = 1;

	public int version;

	[SerializeField]
	private tk2dCameraSettings cameraSettings = new tk2dCameraSettings();

	public tk2dCameraResolutionOverride[] resolutionOverride = new tk2dCameraResolutionOverride[1] { tk2dCameraResolutionOverride.DefaultOverride };

	[SerializeField]
	private tk2dCamera inheritSettings;

	public int nativeResolutionWidth = 960;

	public int nativeResolutionHeight = 640;

	[SerializeField]
	private Camera _unityCamera;

	private static tk2dCamera inst;

	private static List<tk2dCamera> allCameras = new List<tk2dCamera>();

	public bool viewportClippingEnabled;

	public Vector4 viewportRegion = new Vector4(0f, 0f, 100f, 100f);

	private Vector2 _targetResolution = Vector2.zero;

	[SerializeField]
	private float zoomFactor = 1f;

	[HideInInspector]
	public bool forceResolutionInEditor;

	[HideInInspector]
	public Vector2 forceResolution = new Vector2(960f, 640f);

	private Rect _screenExtents;

	private Rect _nativeScreenExtents;

	private Rect unitRect = new Rect(0f, 0f, 1f, 1f);

	private tk2dCamera _settingsRoot;

	public tk2dCameraSettings CameraSettings => cameraSettings;

	public tk2dCameraResolutionOverride CurrentResolutionOverride
	{
		get
		{
			tk2dCamera settingsRoot = SettingsRoot;
			Camera screenCamera = ScreenCamera;
			float num = screenCamera.pixelWidth;
			float num2 = screenCamera.pixelHeight;
			tk2dCameraResolutionOverride tk2dCameraResolutionOverride2 = null;
			if (tk2dCameraResolutionOverride2 == null || (tk2dCameraResolutionOverride2 != null && ((float)tk2dCameraResolutionOverride2.width != num || (float)tk2dCameraResolutionOverride2.height != num2)))
			{
				tk2dCameraResolutionOverride2 = null;
				if (settingsRoot.resolutionOverride != null)
				{
					tk2dCameraResolutionOverride[] array = settingsRoot.resolutionOverride;
					foreach (tk2dCameraResolutionOverride tk2dCameraResolutionOverride3 in array)
					{
						if (tk2dCameraResolutionOverride3.Match((int)num, (int)num2))
						{
							tk2dCameraResolutionOverride2 = tk2dCameraResolutionOverride3;
							break;
						}
					}
				}
			}
			return tk2dCameraResolutionOverride2;
		}
	}

	public tk2dCamera InheritConfig
	{
		get
		{
			return inheritSettings;
		}
		set
		{
			if ((Object)(object)inheritSettings != (Object)(object)value)
			{
				inheritSettings = value;
				_settingsRoot = null;
			}
		}
	}

	private Camera UnityCamera
	{
		get
		{
			if ((Object)(object)_unityCamera == (Object)null)
			{
				_unityCamera = ((Component)this).GetComponent<Camera>();
				if ((Object)(object)_unityCamera == (Object)null)
				{
					Debug.LogError((object)"A unity camera must be attached to the tk2dCamera script");
				}
			}
			return _unityCamera;
		}
	}

	public static tk2dCamera Instance => inst;

	public Rect ScreenExtents => _screenExtents;

	public Rect NativeScreenExtents => _nativeScreenExtents;

	public Vector2 TargetResolution => _targetResolution;

	public Vector2 NativeResolution => new Vector2((float)nativeResolutionWidth, (float)nativeResolutionHeight);

	[Obsolete]
	public Vector2 ScreenOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			Rect screenExtents = ScreenExtents;
			float xMin = ((Rect)(ref screenExtents)).xMin;
			Rect nativeScreenExtents = NativeScreenExtents;
			float num = xMin - ((Rect)(ref nativeScreenExtents)).xMin;
			Rect screenExtents2 = ScreenExtents;
			float yMin = ((Rect)(ref screenExtents2)).yMin;
			Rect nativeScreenExtents2 = NativeScreenExtents;
			return new Vector2(num, yMin - ((Rect)(ref nativeScreenExtents2)).yMin);
		}
	}

	[Obsolete]
	public Vector2 resolution
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			Rect screenExtents = ScreenExtents;
			float xMax = ((Rect)(ref screenExtents)).xMax;
			Rect screenExtents2 = ScreenExtents;
			return new Vector2(xMax, ((Rect)(ref screenExtents2)).yMax);
		}
	}

	[Obsolete]
	public Vector2 ScreenResolution
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			Rect screenExtents = ScreenExtents;
			float xMax = ((Rect)(ref screenExtents)).xMax;
			Rect screenExtents2 = ScreenExtents;
			return new Vector2(xMax, ((Rect)(ref screenExtents2)).yMax);
		}
	}

	[Obsolete]
	public Vector2 ScaledResolution
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			Rect screenExtents = ScreenExtents;
			float width = ((Rect)(ref screenExtents)).width;
			Rect screenExtents2 = ScreenExtents;
			return new Vector2(width, ((Rect)(ref screenExtents2)).height);
		}
	}

	public float ZoomFactor
	{
		get
		{
			return zoomFactor;
		}
		set
		{
			zoomFactor = Mathf.Max(0.01f, value);
		}
	}

	[Obsolete]
	public float zoomScale
	{
		get
		{
			return 1f / Mathf.Max(0.001f, zoomFactor);
		}
		set
		{
			ZoomFactor = 1f / Mathf.Max(0.001f, value);
		}
	}

	public Camera ScreenCamera => (!viewportClippingEnabled || !((Object)(object)inheritSettings != (Object)null) || !(inheritSettings.UnityCamera.rect == unitRect)) ? UnityCamera : inheritSettings.UnityCamera;

	public tk2dCamera SettingsRoot
	{
		get
		{
			if ((Object)(object)_settingsRoot == (Object)null)
			{
				_settingsRoot = ((!((Object)(object)inheritSettings == (Object)null) && !((Object)(object)inheritSettings == (Object)(object)this)) ? inheritSettings.SettingsRoot : this);
			}
			return _settingsRoot;
		}
	}

	public static tk2dCamera CameraForLayer(int layer)
	{
		int num = 1 << layer;
		int count = allCameras.Count;
		for (int i = 0; i < count; i++)
		{
			tk2dCamera tk2dCamera2 = allCameras[i];
			if ((tk2dCamera2.UnityCamera.cullingMask & num) == num)
			{
				return tk2dCamera2;
			}
		}
		return null;
	}

	private void Awake()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Upgrade();
		if (allCameras.IndexOf(this) == -1)
		{
			allCameras.Add(this);
		}
		tk2dCamera settingsRoot = SettingsRoot;
		tk2dCameraSettings tk2dCameraSettings2 = settingsRoot.CameraSettings;
		if (tk2dCameraSettings2.projection == tk2dCameraSettings.ProjectionType.Perspective)
		{
			UnityCamera.transparencySortMode = tk2dCameraSettings2.transparencySortMode;
		}
	}

	private void OnEnable()
	{
		if ((Object)(object)UnityCamera != (Object)null)
		{
			UpdateCameraMatrix();
		}
		else
		{
			((Behaviour)((Component)this).GetComponent<Camera>()).enabled = false;
		}
		if (!viewportClippingEnabled)
		{
			inst = this;
		}
		if (allCameras.IndexOf(this) == -1)
		{
			allCameras.Add(this);
		}
	}

	private void OnDestroy()
	{
		int num = allCameras.IndexOf(this);
		if (num != -1)
		{
			allCameras.RemoveAt(num);
		}
	}

	private void OnPreCull()
	{
		tk2dUpdateManager.FlushQueues();
		UpdateCameraMatrix();
	}

	public float GetSizeAtDistance(float distance)
	{
		tk2dCameraSettings tk2dCameraSettings2 = SettingsRoot.CameraSettings;
		switch (tk2dCameraSettings2.projection)
		{
		case tk2dCameraSettings.ProjectionType.Orthographic:
			if (tk2dCameraSettings2.orthographicType == tk2dCameraSettings.OrthographicType.PixelsPerMeter)
			{
				return 1f / tk2dCameraSettings2.orthographicPixelsPerMeter;
			}
			return 2f * tk2dCameraSettings2.orthographicSize / (float)SettingsRoot.nativeResolutionHeight;
		case tk2dCameraSettings.ProjectionType.Perspective:
			return Mathf.Tan(CameraSettings.fieldOfView * ((float)Math.PI / 180f) * 0.5f) * distance * 2f / (float)SettingsRoot.nativeResolutionHeight;
		default:
			return 1f;
		}
	}

	public Matrix4x4 OrthoOffCenter(Vector2 scale, float left, float right, float bottom, float top, float near, float far)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		float num = 2f / (right - left) * scale.x;
		float num2 = 2f / (top - bottom) * scale.y;
		float num3 = -2f / (far - near);
		float num4 = (0f - (right + left)) / (right - left);
		float num5 = (0f - (bottom + top)) / (top - bottom);
		float num6 = (0f - (far + near)) / (far - near);
		Matrix4x4 result = default(Matrix4x4);
		((Matrix4x4)(ref result))[0, 0] = num;
		((Matrix4x4)(ref result))[0, 1] = 0f;
		((Matrix4x4)(ref result))[0, 2] = 0f;
		((Matrix4x4)(ref result))[0, 3] = num4;
		((Matrix4x4)(ref result))[1, 0] = 0f;
		((Matrix4x4)(ref result))[1, 1] = num2;
		((Matrix4x4)(ref result))[1, 2] = 0f;
		((Matrix4x4)(ref result))[1, 3] = num5;
		((Matrix4x4)(ref result))[2, 0] = 0f;
		((Matrix4x4)(ref result))[2, 1] = 0f;
		((Matrix4x4)(ref result))[2, 2] = num3;
		((Matrix4x4)(ref result))[2, 3] = num6;
		((Matrix4x4)(ref result))[3, 0] = 0f;
		((Matrix4x4)(ref result))[3, 1] = 0f;
		((Matrix4x4)(ref result))[3, 2] = 0f;
		((Matrix4x4)(ref result))[3, 3] = 1f;
		return result;
	}

	private Vector2 GetScaleForOverride(tk2dCamera settings, tk2dCameraResolutionOverride currentOverride, float width, float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 one = Vector2.one;
		float num = 1f;
		if (currentOverride == null)
		{
			return one;
		}
		switch (currentOverride.autoScaleMode)
		{
		case tk2dCameraResolutionOverride.AutoScaleMode.PixelPerfect:
			num = 1f;
			((Vector2)(ref one)).Set(num, num);
			break;
		case tk2dCameraResolutionOverride.AutoScaleMode.FitHeight:
			num = height / (float)settings.nativeResolutionHeight;
			((Vector2)(ref one)).Set(num, num);
			break;
		case tk2dCameraResolutionOverride.AutoScaleMode.FitWidth:
			num = width / (float)settings.nativeResolutionWidth;
			((Vector2)(ref one)).Set(num, num);
			break;
		case tk2dCameraResolutionOverride.AutoScaleMode.FitVisible:
		case tk2dCameraResolutionOverride.AutoScaleMode.ClosestMultipleOfTwo:
		{
			float num2 = (float)settings.nativeResolutionWidth / (float)settings.nativeResolutionHeight;
			float num3 = width / height;
			num = ((!(num3 < num2)) ? (height / (float)settings.nativeResolutionHeight) : (width / (float)settings.nativeResolutionWidth));
			if (currentOverride.autoScaleMode == tk2dCameraResolutionOverride.AutoScaleMode.ClosestMultipleOfTwo)
			{
				num = ((!(num > 1f)) ? Mathf.Pow(2f, Mathf.Floor(Mathf.Log(num, 2f))) : Mathf.Floor(num));
			}
			((Vector2)(ref one)).Set(num, num);
			break;
		}
		case tk2dCameraResolutionOverride.AutoScaleMode.StretchToFit:
			((Vector2)(ref one)).Set(width / (float)settings.nativeResolutionWidth, height / (float)settings.nativeResolutionHeight);
			break;
		case tk2dCameraResolutionOverride.AutoScaleMode.Fill:
			num = Mathf.Max(width / (float)settings.nativeResolutionWidth, height / (float)settings.nativeResolutionHeight);
			((Vector2)(ref one)).Set(num, num);
			break;
		default:
			num = currentOverride.scale;
			((Vector2)(ref one)).Set(num, num);
			break;
		}
		return one;
	}

	private Vector2 GetOffsetForOverride(tk2dCamera settings, tk2dCameraResolutionOverride currentOverride, Vector2 scale, float width, float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 result = Vector2.zero;
		if (currentOverride == null)
		{
			return result;
		}
		tk2dCameraResolutionOverride.FitMode fitMode = currentOverride.fitMode;
		if (fitMode != 0 && fitMode == tk2dCameraResolutionOverride.FitMode.Center)
		{
			if (settings.cameraSettings.orthographicOrigin == tk2dCameraSettings.OrthographicOrigin.BottomLeft)
			{
				((Vector2)(ref result))._002Ector(Mathf.Round(((float)settings.nativeResolutionWidth * scale.x - width) / 2f), Mathf.Round(((float)settings.nativeResolutionHeight * scale.y - height) / 2f));
			}
		}
		else
		{
			result = -currentOverride.offsetPixels;
		}
		return result;
	}

	private Matrix4x4 GetProjectionMatrixForOverride(tk2dCamera settings, tk2dCameraResolutionOverride currentOverride, float pixelWidth, float pixelHeight, bool halfTexelOffset, out Rect screenExtents, out Rect unscaledScreenExtents)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Invalid comparison between Unknown and I4
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Invalid comparison between Unknown and I4
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		Vector2 scaleForOverride = GetScaleForOverride(settings, currentOverride, pixelWidth, pixelHeight);
		Vector2 offsetForOverride = GetOffsetForOverride(settings, currentOverride, scaleForOverride, pixelWidth, pixelHeight);
		float num = offsetForOverride.x;
		float num2 = offsetForOverride.y;
		float num3 = pixelWidth + offsetForOverride.x;
		float num4 = pixelHeight + offsetForOverride.y;
		Vector2 zero = Vector2.zero;
		bool flag = false;
		Vector4 val = default(Vector4);
		Rect rect = default(Rect);
		if (viewportClippingEnabled && (Object)(object)InheritConfig != (Object)null)
		{
			float num5 = (num3 - num) / scaleForOverride.x;
			float num6 = (num4 - num2) / scaleForOverride.y;
			((Vector4)(ref val))._002Ector((float)(int)viewportRegion.x, (float)(int)viewportRegion.y, (float)(int)viewportRegion.z, (float)(int)viewportRegion.w);
			flag = true;
			float num7 = (0f - offsetForOverride.x) / pixelWidth + val.x / num5;
			float num8 = (0f - offsetForOverride.y) / pixelHeight + val.y / num6;
			float num9 = val.z / num5;
			float num10 = val.w / num6;
			if (settings.cameraSettings.orthographicOrigin == tk2dCameraSettings.OrthographicOrigin.Center)
			{
				num7 += (pixelWidth - (float)settings.nativeResolutionWidth * scaleForOverride.x) / pixelWidth / 2f;
				num8 += (pixelHeight - (float)settings.nativeResolutionHeight * scaleForOverride.y) / pixelHeight / 2f;
			}
			((Rect)(ref rect))._002Ector(num7, num8, num9, num10);
			Rect rect2 = UnityCamera.rect;
			if (((Rect)(ref rect2)).x == num7)
			{
				Rect rect3 = UnityCamera.rect;
				if (((Rect)(ref rect3)).y == num8)
				{
					Rect rect4 = UnityCamera.rect;
					if (((Rect)(ref rect4)).width == num9)
					{
						Rect rect5 = UnityCamera.rect;
						if (((Rect)(ref rect5)).height == num10)
						{
							goto IL_01de;
						}
					}
				}
			}
			UnityCamera.rect = rect;
			goto IL_01de;
		}
		if (UnityCamera.rect != CameraSettings.rect)
		{
			UnityCamera.rect = CameraSettings.rect;
		}
		if (settings.cameraSettings.orthographicOrigin == tk2dCameraSettings.OrthographicOrigin.Center)
		{
			float num11 = (num3 - num) * 0.5f;
			num -= num11;
			num3 -= num11;
			float num12 = (num4 - num2) * 0.5f;
			num4 -= num12;
			num2 -= num12;
			((Vector2)(ref zero)).Set((float)(-nativeResolutionWidth) / 2f, (float)(-nativeResolutionHeight) / 2f);
		}
		goto IL_03ba;
		IL_01de:
		float num13 = Mathf.Min(1f - ((Rect)(ref rect)).x, ((Rect)(ref rect)).width);
		float num14 = Mathf.Min(1f - ((Rect)(ref rect)).y, ((Rect)(ref rect)).height);
		float num15 = val.x * scaleForOverride.x - offsetForOverride.x;
		float num16 = val.y * scaleForOverride.y - offsetForOverride.y;
		if (settings.cameraSettings.orthographicOrigin == tk2dCameraSettings.OrthographicOrigin.Center)
		{
			num15 -= (float)settings.nativeResolutionWidth * 0.5f * scaleForOverride.x;
			num16 -= (float)settings.nativeResolutionHeight * 0.5f * scaleForOverride.y;
		}
		if (((Rect)(ref rect)).x < 0f)
		{
			num15 += (0f - ((Rect)(ref rect)).x) * pixelWidth;
			num13 = ((Rect)(ref rect)).x + ((Rect)(ref rect)).width;
		}
		if (((Rect)(ref rect)).y < 0f)
		{
			num16 += (0f - ((Rect)(ref rect)).y) * pixelHeight;
			num14 = ((Rect)(ref rect)).y + ((Rect)(ref rect)).height;
		}
		num += num15;
		num2 += num16;
		num3 = pixelWidth * num13 + offsetForOverride.x + num15;
		num4 = pixelHeight * num14 + offsetForOverride.y + num16;
		goto IL_03ba;
		IL_03ba:
		float num17 = 1f / ZoomFactor;
		bool flag2 = (int)Application.platform == 2 || (int)Application.platform == 7;
		float num18 = ((!halfTexelOffset || !flag2 || SystemInfo.graphicsShaderLevel >= 40) ? 0f : 0.5f);
		float num19 = settings.cameraSettings.orthographicSize;
		switch (settings.cameraSettings.orthographicType)
		{
		case tk2dCameraSettings.OrthographicType.OrthographicSize:
			num19 = 2f * settings.cameraSettings.orthographicSize / (float)settings.nativeResolutionHeight;
			break;
		case tk2dCameraSettings.OrthographicType.PixelsPerMeter:
			num19 = 1f / settings.cameraSettings.orthographicPixelsPerMeter;
			break;
		}
		if (!flag)
		{
			Rect rect6 = UnityCamera.rect;
			float width = ((Rect)(ref rect6)).width;
			Rect rect7 = UnityCamera.rect;
			float num20 = Mathf.Min(width, 1f - ((Rect)(ref rect7)).x);
			Rect rect8 = UnityCamera.rect;
			float height = ((Rect)(ref rect8)).height;
			Rect rect9 = UnityCamera.rect;
			float num21 = Mathf.Min(height, 1f - ((Rect)(ref rect9)).y);
			if (num20 > 0f && num21 > 0f)
			{
				scaleForOverride.x /= num20;
				scaleForOverride.y /= num21;
			}
		}
		float num22 = num19 * num17;
		((Rect)(ref screenExtents))._002Ector(num * num22 / scaleForOverride.x, num2 * num22 / scaleForOverride.y, (num3 - num) * num22 / scaleForOverride.x, (num4 - num2) * num22 / scaleForOverride.y);
		((Rect)(ref unscaledScreenExtents))._002Ector(zero.x * num22, zero.y * num22, (float)nativeResolutionWidth * num22, (float)nativeResolutionHeight * num22);
		return OrthoOffCenter(scaleForOverride, num19 * (num + num18) * num17, num19 * (num3 + num18) * num17, num19 * (num2 - num18) * num17, num19 * (num4 - num18) * num17, UnityCamera.nearClipPlane, UnityCamera.farClipPlane);
	}

	private Vector2 GetScreenPixelDimensions(tk2dCamera settings)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 result = default(Vector2);
		((Vector2)(ref result))._002Ector((float)ScreenCamera.pixelWidth, (float)ScreenCamera.pixelHeight);
		return result;
	}

	private void Upgrade()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (version == CURRENT_VERSION)
		{
			return;
		}
		if (version == 0)
		{
			cameraSettings.orthographicPixelsPerMeter = 1f;
			cameraSettings.orthographicType = tk2dCameraSettings.OrthographicType.PixelsPerMeter;
			cameraSettings.orthographicOrigin = tk2dCameraSettings.OrthographicOrigin.BottomLeft;
			cameraSettings.projection = tk2dCameraSettings.ProjectionType.Orthographic;
			tk2dCameraResolutionOverride[] array = resolutionOverride;
			foreach (tk2dCameraResolutionOverride tk2dCameraResolutionOverride2 in array)
			{
				tk2dCameraResolutionOverride2.Upgrade(version);
			}
			Camera component = ((Component)this).GetComponent<Camera>();
			if ((Object)(object)component != (Object)null)
			{
				cameraSettings.rect = component.rect;
				if (!component.orthographic)
				{
					cameraSettings.projection = tk2dCameraSettings.ProjectionType.Perspective;
					cameraSettings.fieldOfView = component.fieldOfView * ZoomFactor;
				}
				((Object)component).hideFlags = (HideFlags)3;
			}
		}
		version = CURRENT_VERSION;
	}

	public void UpdateCameraMatrix()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Invalid comparison between Unknown and I4
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Invalid comparison between Unknown and I4
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Invalid comparison between Unknown and I4
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Invalid comparison between Unknown and I4
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		Upgrade();
		if (!viewportClippingEnabled)
		{
			inst = this;
		}
		Camera unityCamera = UnityCamera;
		tk2dCamera settingsRoot = SettingsRoot;
		tk2dCameraSettings tk2dCameraSettings2 = settingsRoot.CameraSettings;
		if (unityCamera.rect != cameraSettings.rect)
		{
			unityCamera.rect = cameraSettings.rect;
		}
		_targetResolution = GetScreenPixelDimensions(settingsRoot);
		if (tk2dCameraSettings2.projection == tk2dCameraSettings.ProjectionType.Perspective)
		{
			if (unityCamera.orthographic)
			{
				unityCamera.orthographic = false;
			}
			float num = Mathf.Min(179.9f, tk2dCameraSettings2.fieldOfView / Mathf.Max(0.001f, ZoomFactor));
			if (unityCamera.fieldOfView != num)
			{
				unityCamera.fieldOfView = num;
			}
			((Rect)(ref _screenExtents)).Set(0f - unityCamera.aspect, -1f, unityCamera.aspect * 2f, 2f);
			_nativeScreenExtents = _screenExtents;
			unityCamera.ResetProjectionMatrix();
			return;
		}
		if (!unityCamera.orthographic)
		{
			unityCamera.orthographic = true;
		}
		Matrix4x4 val = GetProjectionMatrixForOverride(settingsRoot, settingsRoot.CurrentResolutionOverride, _targetResolution.x, _targetResolution.y, halfTexelOffset: true, out _screenExtents, out _nativeScreenExtents);
		if ((int)Application.platform == 21 && ((int)Screen.orientation == 3 || (int)Screen.orientation == 4))
		{
			float num2 = (((int)Screen.orientation != 4) ? (-90f) : 90f);
			Matrix4x4 val2 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, num2), Vector3.one);
			val = val2 * val;
		}
		if (unityCamera.projectionMatrix != val)
		{
			unityCamera.projectionMatrix = val;
		}
	}
}
