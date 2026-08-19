using System;
using System.Collections.Generic;
using CameraEffects;
using Holoville.HOTween;
using UnityEngine;

public class CameraController : KSingleton<CameraController>
{
	private float _prevZoomScale = 1f;

	private float _playerZoom = 1f;

	private GameObject _curCameraTarget;

	private readonly List<CameraEffect> _cameraEffects = new List<CameraEffect>();

	private readonly List<KeyValuePair<CameraEffect, float>> _reservedCameraEffects = new List<KeyValuePair<CameraEffect, float>>();

	[SerializeField]
	private float _battleBeginCameraDistance = 6000f;

	[SerializeField]
	private float _battleBeginCameraDuration = 2f;

	private MoveArrowGroup _moveArrowGroup;

	public GameObject CurrentCameraTarget => _curCameraTarget;

	public Vector3 CameraOffset { get; set; }

	public float PlayerZoom
	{
		get
		{
			return _playerZoom;
		}
		set
		{
			_playerZoom = Mathf.Clamp(value, KSingleton<MainCamera>.Instance().MinZoom, KSingleton<MainCamera>.Instance().MaxZoom);
		}
	}

	private MoveArrowGroup MoveArrowGroup
	{
		get
		{
			if ((Object)(object)_moveArrowGroup == (Object)null)
			{
				_moveArrowGroup = UIManager.FindScript<MoveArrowGroup>();
			}
			return _moveArrowGroup;
		}
	}

	protected override void OnAwake()
	{
		if ((Object)(object)PlayerBehavior.LocalPlayer != (Object)null)
		{
			_curCameraTarget = ((Component)PlayerBehavior.LocalPlayer).gameObject;
		}
		KSingleton<PlayerController>.Instance().IsGestureProcessed += delegate(PlayerController.Gesture gesture, Vector3 vector3, bool touchedUI, ref bool result)
		{
			if (gesture == PlayerController.Gesture.Zoom)
			{
				ModifyZoom(vector3.z);
				result = true;
			}
		};
	}

	public void Process()
	{
		UpdateAutoCombatCamera();
		UpdateCamera();
	}

	public void ModifyZoom(float offset)
	{
		offset *= 2f;
		if (Math.Abs(offset) > float.Epsilon)
		{
			PlayerZoom += offset;
			if (!IsAutoCombatCamera())
			{
				KSingleton<MainCamera>.Instance().Zoom = PlayerZoom;
			}
		}
	}

	public void ResetCamera()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		ResetCameraTarget();
		TweenParms val = new TweenParms();
		val.Prop("CameraOffset", (object)Vector3.zero);
		val.Ease((EaseType)0);
		HOTween.To((object)this, 0.25f, val);
		TweenParms val2 = new TweenParms();
		val2.Prop("Zoom", (object)PlayerZoom);
		val2.Ease((EaseType)0);
		HOTween.To((object)KSingleton<MainCamera>.Instance(), 0.25f, val2);
	}

	public void SetCameraTarget(GameObject target, float cameraMoveTime = 0.3f, float zoomRatio = 1f, float zoomTime = 0.3f, bool forceRetarget = false)
	{
		if (forceRetarget || !((Object)(object)target == (Object)(object)_curCameraTarget))
		{
			_curCameraTarget = target;
			AddCameraEffect(new TargetObjectViewCameraEffect(target, cameraMoveTime, zoomRatio, zoomTime));
		}
	}

	public void SetCameraTargetPos(Vector3 targetPos, float cameraMoveTime = 0.3f, float zoomRatio = 1f, float zoomTime = 0.3f)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_curCameraTarget = null;
		AddCameraEffect(new TargetPosViewCameraEffect(targetPos, cameraMoveTime, zoomRatio, zoomTime));
	}

	public void SetCameraFovRatio(float zoomRatio = 1f, float zoomTime = 0.3f)
	{
		AddCameraEffect(new TargetPosViewCameraEffect(zoomRatio, zoomTime));
	}

	public void ResetCameraTarget(float cameraResetMoveTime = 0.3f, float fovResetTime = 0.3f, bool forceReset = false)
	{
		if (forceReset || !((Object)(object)_curCameraTarget == (Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject))
		{
			ResetAllCameraEffects(cameraResetMoveTime, fovResetTime);
			_curCameraTarget = ((Component)PlayerBehavior.LocalPlayer).gameObject;
		}
	}

	public void AddCameraEffect(CameraEffect cameraEffect, float delay = 0f)
	{
		if (delay <= 0f)
		{
			if (cameraEffect == null)
			{
				ResetAllCameraEffects(0.3f, 0.3f);
			}
			else
			{
				_cameraEffects.Add(cameraEffect);
			}
			return;
		}
		float num = Time.time + delay;
		int num2 = -1;
		int i = 0;
		for (int count = _reservedCameraEffects.Count; i < count; i++)
		{
			if (_reservedCameraEffects[i].Value > num)
			{
				num2 = i;
				break;
			}
		}
		KeyValuePair<CameraEffect, float> item = new KeyValuePair<CameraEffect, float>(cameraEffect, num);
		if (num2 == -1)
		{
			_reservedCameraEffects.Add(item);
		}
		else
		{
			_reservedCameraEffects.Insert(num2, item);
		}
	}

	public void PopCameraEffect()
	{
		if (_cameraEffects.Count >= 1)
		{
			_cameraEffects.RemoveAt(_cameraEffects.Count - 1);
		}
	}

	public void BeginBattleCameraEffect()
	{
		AddCameraEffect(new CameraDistanceEffect(_battleBeginCameraDistance, _battleBeginCameraDuration));
	}

	private CameraEffectOutput ApplyCameraEffects(Vector3 curCameraTargetPos)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		int count = _cameraEffects.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			if (_cameraEffects[num].IsActive())
			{
				CameraEffectOutput result = _cameraEffects[num].Apply(curCameraTargetPos);
				if (!result.IsInvalid())
				{
					return result;
				}
				_cameraEffects.RemoveAt(num);
			}
			else
			{
				_cameraEffects.RemoveAt(num);
			}
		}
		return CameraEffectOutput.Invalid;
	}

	private void ResetAllCameraEffects(float cameraResetMoveTime, float roomResetTime)
	{
		_cameraEffects.Clear();
		_reservedCameraEffects.Clear();
		AddCameraEffect(new TargetObjectViewCameraEffect(((Component)PlayerBehavior.LocalPlayer).gameObject, cameraResetMoveTime, 1f, roomResetTime, deactiveAtFinish: true));
		_curCameraTarget = ((Component)PlayerBehavior.LocalPlayer).gameObject;
	}

	private static bool IsAutoCombatCamera()
	{
		if (KSingleton<MainCamera>.Instance().CameraDistanceOverride > 0f)
		{
			return false;
		}
		return KSingleton<PlayerController>.Instance().AutoAimZoom && PlayerBehavior.LocalPlayer.IsCombatMode && (Object)(object)PlayerBehavior.LocalPlayer.Target != (Object)null && !GameSystem<CombatSystem>.Instance().RunAwayNow;
	}

	private void UpdateAutoCombatCamera()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		if (!IsAutoCombatCamera())
		{
			return;
		}
		Vector3 position = PlayerBehavior.LocalPlayer.Target.transform.position;
		float num = (float)Screen.height * 0.4f;
		float num2 = (float)Screen.width * 0.4f;
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		bool flag = false;
		Vector3 val = MainCamera.WorldToScreenPos(currentPosition);
		Vector3 val2 = MainCamera.WorldToScreenPos(position);
		Vector3 val3 = val - val2;
		float num3 = num2 / Mathf.Abs(val3.x);
		num3 = Mathf.Min(num3, num / Mathf.Abs(val3.y));
		float zoom = KSingleton<MainCamera>.Instance().Zoom;
		float minZoom = KSingleton<MainCamera>.Instance().MinZoom;
		float maxZoom = KSingleton<MainCamera>.Instance().MaxZoom;
		float num4 = maxZoom - minZoom;
		maxZoom = Mathf.Min(PlayerZoom, maxZoom);
		num3 = Mathf.Clamp(num3, minZoom / zoom, maxZoom / zoom);
		float num5 = num3 * zoom;
		if (num5 != zoom)
		{
			float num6 = num5 - zoom;
			float num7 = num4 * Time.deltaTime;
			if (Mathf.Abs(num6) > num7)
			{
				num5 = zoom + ((!(num6 > 0f)) ? (0f - num7) : num7);
			}
			KSingleton<MainCamera>.Instance().Zoom = num5;
		}
		Vector3 val4 = KMathUtil.ClampEndWithDistance(currentPosition, position, 2000f);
		Vector3 val5 = Vector3.Lerp(currentPosition, val4, 0.5f);
		Vector3 zero = Vector3.zero;
		CameraOffset = Vector3.MoveTowards(CameraOffset, zero, 500f * Time.deltaTime);
	}

	private void UpdateCamera()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = PlayerBehavior.LocalPlayer.CameraOrigin + CameraOffset;
		float num = _prevZoomScale;
		CameraEffectOutput cameraEffectOutput = ApplyCameraEffects(val);
		if (cameraEffectOutput.IsValid())
		{
			val = cameraEffectOutput.Pos;
			num = cameraEffectOutput.ZoomRatio;
			KSingleton<MainCamera>.Instance().CameraDistanceOverride = cameraEffectOutput.CameraDistance;
		}
		KSingleton<MainCamera>.Instance().ZoomScale = num;
		KSingleton<MainCamera>.Instance().UpdateCameraTarget(val);
		_prevZoomScale = num;
		if (_reservedCameraEffects.Count <= 0)
		{
			return;
		}
		int num2 = _reservedCameraEffects.Count;
		int i = 0;
		for (int count = _reservedCameraEffects.Count; i < count; i++)
		{
			if (_reservedCameraEffects[i].Value > Time.time)
			{
				num2 = i;
				break;
			}
		}
		bool flag = false;
		for (int j = 0; j < num2; j++)
		{
			CameraEffect key = _reservedCameraEffects[j].Key;
			AddCameraEffect(key);
			if (key == null)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			_reservedCameraEffects.RemoveRange(0, num2);
		}
	}
}
