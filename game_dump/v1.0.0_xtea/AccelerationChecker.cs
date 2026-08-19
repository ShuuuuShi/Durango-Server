using System;
using UnityEngine;

public class AccelerationChecker : KSingleton<AccelerationChecker>
{
	private const int AccFilterSize = 20;

	private const int DiffBufferSize = 20;

	private Vector3 _defaultCameraAngle = new Vector3(0f, 0f, 0f);

	private Vector3 _filteredAcc;

	private Vector3 _curAcc;

	private Vector3 _prevAcc;

	private Vector3 _deltaAcc;

	private Transform _mainCamera;

	private int _accFilterIndex;

	private Vector3[] _accFilterArray;

	private Vector3[] _diffArray;

	private int _curDiffIndex;

	private Vector3 _differentialAcc;

	private Vector3 _relativeAcc;

	private bool _isLoading = true;

	[SerializeField]
	private float _equilibriumThershold;

	private bool _isEquilibrium;

	[SerializeField]
	private Vector2 _acceleractionCameraLean;

	[SerializeField]
	private Vector2 _moveDirectionCameraLean;

	[SerializeField]
	private float _cameraLeanSpeed;

	private Vector3 _moveCameraLeanAngle;

	[SerializeField]
	private float _popThershold;

	[SerializeField]
	private float _popDelayTime;

	private int _popCheckIndex;

	private float _popLevel;

	private float _popDelayTimer;

	private int _shakeLevel;

	private float _deltaShakeLevel;

	private bool _isShaken;

	public static Vector3 Acceleration => KSingleton<AccelerationChecker>.Instance()._filteredAcc;

	public event Action BrokenEquilibrium;

	public event Action ComebackEquilibrium;

	protected override void OnAwake()
	{
		_accFilterArray = (Vector3[])(object)new Vector3[20];
		_diffArray = (Vector3[])(object)new Vector3[20];
		LoadingCurtainGroup loadingCurtainGroup = UIManager.FindScript<LoadingCurtainGroup>();
		if ((Object)(object)loadingCurtainGroup != (Object)null && !loadingCurtainGroup.IsFadeoutStarted)
		{
			EventDelegate.Add(loadingCurtainGroup.FadeOutStarted, LoadingFinished, oneShot: true);
		}
		else
		{
			LoadingFinished();
		}
	}

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_mainCamera = ((Component)KSingleton<MainCamera>.Instance()).transform;
		_defaultCameraAngle = _mainCamera.localEulerAngles;
	}

	private void FixedUpdate()
	{
		if (!_isLoading)
		{
			CalcCurrentAcceleraction();
			CheckEquilibrium();
			CameraLean();
			if (Debug.isDebugBuild && IsShaken())
			{
				HideDevelopmentGroup();
			}
		}
	}

	private void LoadingFinished()
	{
		_isLoading = false;
	}

	private void CalcCurrentAcceleraction()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		_prevAcc = _curAcc;
		_curAcc = Input.acceleration;
		_deltaAcc = _curAcc - _prevAcc;
		ref Vector3 reference = ref _accFilterArray[_accFilterIndex];
		reference = _curAcc;
		_accFilterIndex = (_accFilterIndex + 1) % 20;
		_filteredAcc = Vector3.zero;
		for (int i = 0; i < 20; i++)
		{
			_filteredAcc += _accFilterArray[i];
		}
		_filteredAcc /= 20f;
		ref Vector3 reference2 = ref _diffArray[_curDiffIndex];
		reference2 = _deltaAcc;
		Vector3 val = Vector3.zero;
		for (int j = 0; j < 20; j++)
		{
			val += _diffArray[j];
		}
		_differentialAcc = _diffArray[_curDiffIndex] / Time.fixedDeltaTime;
		_relativeAcc = val;
		_curDiffIndex = (_curDiffIndex + 1) % 20;
	}

	private void CheckEquilibrium()
	{
		if (_isEquilibrium)
		{
			if (Mathf.Abs(_relativeAcc.x) > _equilibriumThershold || Mathf.Abs(_relativeAcc.y) > _equilibriumThershold || Mathf.Abs(_relativeAcc.z) > _equilibriumThershold)
			{
				_isEquilibrium = false;
				if (this.BrokenEquilibrium != null)
				{
					this.BrokenEquilibrium();
				}
			}
		}
		else if (Mathf.Abs(_relativeAcc.x) < _equilibriumThershold && Mathf.Abs(_relativeAcc.y) < _equilibriumThershold && Mathf.Abs(_relativeAcc.z) < _equilibriumThershold)
		{
			_isEquilibrium = true;
			if (this.ComebackEquilibrium != null)
			{
				this.ComebackEquilibrium();
			}
		}
	}

	private void CameraLean()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 moveDir = PlayerBehavior.LocalPlayer.MoveDir;
		Vector2 zero = default(Vector2);
		if (moveDir != Vector3.zero)
		{
			float num = Mathf.Atan2(moveDir.z, moveDir.x) - (float)Math.PI / 4f;
			zero.x = Mathf.Cos(num);
			zero.y = Mathf.Sin(num);
		}
		else
		{
			zero = Vector2.zero;
		}
		Vector3 val = Vector3.right * zero.x * _moveDirectionCameraLean.x + Vector3.up * zero.y * _moveDirectionCameraLean.y;
		if (val != _moveCameraLeanAngle)
		{
			float num2 = _cameraLeanSpeed * Time.fixedDeltaTime;
			float num3 = val.x - _moveCameraLeanAngle.x;
			float num4 = val.y - _moveCameraLeanAngle.y;
			if (Mathf.Abs(num3) < num2)
			{
				_moveCameraLeanAngle.x = val.x;
			}
			else
			{
				ref Vector3 moveCameraLeanAngle = ref _moveCameraLeanAngle;
				moveCameraLeanAngle.x += num2 * Mathf.Sign(num3);
			}
			if (Mathf.Abs(num4) < num2)
			{
				_moveCameraLeanAngle.y = val.y;
			}
			else
			{
				ref Vector3 moveCameraLeanAngle2 = ref _moveCameraLeanAngle;
				moveCameraLeanAngle2.y += num2 * Mathf.Sign(num4);
			}
		}
		Vector3 val2 = _defaultCameraAngle + _moveCameraLeanAngle;
		Vector3 localEulerAngles = val2 + (_filteredAcc.y - 0.5f) * 0.5f * _acceleractionCameraLean.x * Vector3.right + _filteredAcc.x * 0.5f * _acceleractionCameraLean.y * Vector3.up;
		_mainCamera.localEulerAngles = localEulerAngles;
	}

	private void PopChecker()
	{
		if (_popDelayTimer > 0f)
		{
			_popDelayTimer -= Time.fixedDeltaTime;
		}
		else if (_popCheckIndex == -1)
		{
			if (Mathf.Abs(_differentialAcc.y) < _popThershold && Mathf.Abs(_relativeAcc.y) > _popThershold)
			{
				_popCheckIndex = 20;
				_popLevel = (0f - _relativeAcc.y) * 0.5f;
			}
		}
		else if (_popCheckIndex > 0)
		{
			_popCheckIndex--;
			if (Mathf.Abs(_differentialAcc.y) < _popThershold && ((_popLevel > 0f && _popLevel < _relativeAcc.y) || (_popLevel < 0f && _popLevel > _relativeAcc.y)))
			{
				_popCheckIndex = 0;
				_popDelayTimer = _popDelayTime;
				OnPop();
			}
		}
		else if (Mathf.Abs(_differentialAcc.y) < _popThershold && Mathf.Abs(_relativeAcc.y) < _popThershold)
		{
			_popCheckIndex = -1;
		}
	}

	private void OnPop()
	{
		if (_deltaShakeLevel <= 0f)
		{
			PlayerController playerController = KSingleton<PlayerController>.Instance();
			if (!playerController.MoveLock)
			{
				playerController.Motion("Jump");
			}
		}
	}

	private bool IsShaken()
	{
		float num = _shakeLevel;
		if (((Vector3)(ref _relativeAcc)).sqrMagnitude > 5f)
		{
			_shakeLevel++;
		}
		else
		{
			_shakeLevel--;
		}
		_shakeLevel = Mathf.Clamp(_shakeLevel, 0, 10);
		_deltaShakeLevel = (float)_shakeLevel - num;
		if (_shakeLevel > 9 && (float)_shakeLevel > num && !_isShaken)
		{
			_isShaken = true;
			return true;
		}
		if (_shakeLevel == 0)
		{
			_isShaken = false;
		}
		return false;
	}

	private void HideDevelopmentGroup()
	{
		DevelopmentGroup developmentGroup = UIManager.FindScript<DevelopmentGroup>();
		if ((Object)(object)developmentGroup != (Object)null)
		{
			((Component)developmentGroup).gameObject.SetActive(!((Component)developmentGroup).gameObject.activeSelf);
		}
	}
}
