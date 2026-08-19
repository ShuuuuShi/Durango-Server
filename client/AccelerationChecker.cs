using System;
using Durango.UI;
using Durango.Utils;
using UnityEngine;

public class AccelerationChecker : Singleton<AccelerationChecker>
{
	[SerializeField]
	private Vector2 _acceleractionCameraLean;

	[SerializeField]
	private Vector2 _moveDirectionCameraLean;

	[SerializeField]
	private float _cameraLeanSpeed;

	[SerializeField]
	private float _popThershold;

	[SerializeField]
	private float _popDelayTime;

	[SerializeField]
	private float _equilibriumThershold;

	private bool _isEquilibrium;

	private Vector3 _filteredAcc;

	private Vector3 _curAcc;

	private Vector3 _prevAcc;

	private Vector3 _deltaAcc;

	private const int AccFilterSize = 20;

	private int _accFilterIndex;

	private Vector3[] _accFilterArray;

	private const int DiffBufferSize = 20;

	private Vector3[] _diffArray;

	private int _curDiffIndex;

	private Vector3 _differentialAcc;

	private Vector3 _relativeAcc;

	private bool _isLoading = true;

	private Vector3 _moveCamLeanAngle;

	private int _popCheckIndex;

	private float _popLevel;

	private float _popDelayTimer;

	private int _shakeLevel;

	private float _deltaShakeLevel;

	private bool _isShaken;

	public static Vector3 Acceleration => Singleton<AccelerationChecker>.Instance()._filteredAcc;

	public Vector3 FinalCamLeanAngle { get; private set; }

	public event Action BrokenEquilibrium;

	public event Action ComebackEquilibrium;

	protected override void OnAwake()
	{
		_accFilterArray = new Vector3[20];
		_diffArray = new Vector3[20];
		LoadingCurtainGroup loadingCurtainGroup = UIManager.FindScript<LoadingCurtainGroup>();
		if (loadingCurtainGroup != null && !loadingCurtainGroup.IsFadeoutStarted)
		{
			EventDelegate.Add(loadingCurtainGroup.FadeOutStarted, LoadingFinished, oneShot: true);
		}
		else
		{
			LoadingFinished();
		}
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
		Vector3 zero = Vector3.zero;
		for (int j = 0; j < 20; j++)
		{
			zero += _diffArray[j];
		}
		_differentialAcc = _diffArray[_curDiffIndex] / Time.fixedDeltaTime;
		_relativeAcc = zero;
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
		Vector2 zero = default(Vector2);
		if ((bool)PlayerBehavior.LocalPlayer.IsMoving)
		{
			float f = (PlayerController.MoveOperator.LastSentYaw - 45f) * ((float)Math.PI / 180f);
			zero.x = Mathf.Cos(f);
			zero.y = Mathf.Sin(f);
		}
		else
		{
			zero = Vector2.zero;
		}
		Vector3 vector = Vector3.right * zero.x * _moveDirectionCameraLean.x + Vector3.up * zero.y * _moveDirectionCameraLean.y;
		if (vector != _moveCamLeanAngle)
		{
			float num = _cameraLeanSpeed * Time.fixedDeltaTime;
			float f2 = vector.x - _moveCamLeanAngle.x;
			float f3 = vector.y - _moveCamLeanAngle.y;
			if (Mathf.Abs(f2) < num)
			{
				_moveCamLeanAngle.x = vector.x;
			}
			else
			{
				_moveCamLeanAngle.x += num * Mathf.Sign(f2);
			}
			if (Mathf.Abs(f3) < num)
			{
				_moveCamLeanAngle.y = vector.y;
			}
			else
			{
				_moveCamLeanAngle.y += num * Mathf.Sign(f3);
			}
		}
		FinalCamLeanAngle = _moveCamLeanAngle + (_filteredAcc.y - 0.5f) * 0.5f * _acceleractionCameraLean.x * Vector3.right + _filteredAcc.x * 0.5f * _acceleractionCameraLean.y * Vector3.up;
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
		if (_deltaShakeLevel <= 0f && !GameSystem<InputSystem>.Instance().MoveLock)
		{
			PlayerController.MotionUpdater.Motion("Jump");
		}
	}

	private bool IsShaken()
	{
		float num = _shakeLevel;
		if (_relativeAcc.sqrMagnitude > 5f)
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
		if (developmentGroup != null)
		{
			developmentGroup.gameObject.SetActive(!developmentGroup.gameObject.activeSelf);
		}
	}
}
