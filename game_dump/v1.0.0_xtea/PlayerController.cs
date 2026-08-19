using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using CameraEffects;
using ExploreData;
using ItemSystem;
using KCollisionData;
using L10N;
using Messages;
using MsgPack;
using MusicData;
using NetworkEnums;
using Player;
using Shared.Teleport;
using TerrainData;
using UnityEngine;

[RequireComponent(typeof(TerrainWater))]
public class PlayerController : KSingleton<PlayerController>
{
	public enum Gesture
	{
		Panning,
		Zoom
	}

	public enum MoveActionType
	{
		None,
		VirtualStick
	}

	public class TouchEvent
	{
		public enum UsedBy
		{
			None,
			Move,
			Joystick,
			Gesture,
			Draw
		}

		public int TouchId;

		public Vector2 BeginPos;

		public Vector2 CurrentPos;

		public Vector2 LastPos;

		public float BeginTime;

		public float LastActivateTime;

		public int TapCount;

		public bool IsTouchBegan;

		public bool IsNguiTouched;

		public UsedBy Used;
	}

	private class MotionMovementInfo
	{
		public string MotionName;

		public byte MotionOption;

		public float PlaybackRate;

		public float RotSpeed = -1f;

		public List<Location> Path = new List<Location>();
	}

	public class MoveTargetParam
	{
		public static int InvalidYaw = -999;

		public float DistanceThresh;

		public Vector3 TargetPos;

		public int DestYaw = InvalidYaw;

		public bool HasGoal;

		private GameObject _targetObj;

		public GameObject TargetObj
		{
			get
			{
				return _targetObj;
			}
			set
			{
				_targetObj = value;
				if (Object.op_Implicit((Object)(object)PlayerBehavior.LocalPlayer.LookAtController) && Object.op_Implicit((Object)(object)_targetObj))
				{
					PlayerBehavior.LocalPlayer.LookAtController.SetLookTarget(_targetObj);
				}
			}
		}

		public bool HasYawTaget => DestYaw != InvalidYaw;

		public void Reset()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			HasGoal = false;
			TargetObj = null;
			DistanceThresh = 0f;
			TargetPos = default(Vector3);
			DestYaw = InvalidYaw;
		}

		public void CopyFrom(MoveTargetParam src)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			HasGoal = src.HasGoal;
			_targetObj = src._targetObj;
			DistanceThresh = src.DistanceThresh;
			TargetPos = src.TargetPos;
			DestYaw = src.DestYaw;
		}
	}

	public delegate void GestureProcessDelegate(Gesture gesture, Vector3 pos, bool touchUI, ref bool result);

	public delegate void PickObjectDelegate(Ray ray, TouchEvent touch, ref bool result);

	public delegate void TouchProcessDelegate(List<TouchEvent> touches, ref bool result);

	public const float DefaultMoveSpeed = 500f;

	private const float MoveSpeedTired = 180f;

	public const int MouseFingerId = -10;

	private const float MovementBufferTime = 1f;

	private const float MovementSendPeriod = 0.5f;

	public static TouchEvent CurrentTouchEvent;

	public bool AllowJoystickMove = true;

	public bool AllowVirtualStickMove = true;

	[SerializeField]
	private bool _autoAimZoom = true;

	[SerializeField]
	private float _bushMoveSpeedRatioTiny = 0.75f;

	[SerializeField]
	private float _bushMoveSpeedRatioSmall = 0.75f;

	[SerializeField]
	private float _bushMoveSpeedRatioMedium = 0.75f;

	[SerializeField]
	private float _bushMoveSpeedRatioLarge = 0.75f;

	[SerializeField]
	private float _roadMoveSpeedRatio = 1.2f;

	[SerializeField]
	private AudioClipType _touchAudio;

	[SerializeField]
	private ParticleType _dieParticleType;

	[SerializeField]
	private float _maxMoveCursorDist = 800f;

	private float _moveSpeed;

	private float _waterRetardingSpeedRatio = 1f;

	private float _lastDrawLineBufferSendTime;

	private List<DrawLineBase> _drawLineBuffer = new List<DrawLineBase>();

	private Vector2 _previousLinePoint;

	private bool _drawMode;

	private UIManager _uiManager;

	private Camera _mainCamera;

	private Action<GameObject> _onCompleteMoveToTarget;

	private GameObject _onCompleteMovoToTargetObj;

	private List<MotionMovementInfo> _sendMovementsBuffer = new List<MotionMovementInfo>();

	private List<Location> _curPathBuffer;

	private float _lastMoveSendTime;

	private float _lastAddMoveTime;

	private bool _requestSendMoveMsg;

	private readonly List<TouchEvent> _touchEvents = new List<TouchEvent>();

	private readonly List<int> _toDeleteEvents = new List<int>();

	private bool _isMouseDown;

	private Vector3 _lastMouseDownPos;

	private float _lastMouseDownTime;

	private int _mouseTapCount;

	private TouchEvent[] _gestureTouches = new TouchEvent[2];

	private int _lastSelectedPolicy;

	private float _policyLockTime;

	private int _prevHotControl;

	private float _lastSentColosseumMove;

	private Vector3 _lastMoveDir;

	private bool _lastMoving;

	private Vector3 _lastSentPosition = Vector3.zero;

	private float _lastSentYaw;

	private bool _moveLock;

	private bool _departSent;

	private HashSet<string> _waterFlowRegisterSet = new HashSet<string>();

	private bool _useWaterHeight = true;

	private bool _useTileMoveSpeedRatio = true;

	private bool _isInServerSideBattle;

	private Vector3 _prevSliding = Vector3.zero;

	private float _slidingKeepLengthCounter;

	private readonly MoveTargetParam _moveTargetParam = new MoveTargetParam();

	private bool _isImmovableState;

	private int _immovableStateUpdateFrame;

	private GameObject _dieParticle;

	private bool _ignoreDraw;

	private float _lastNoticeTime;

	private Vector3 _lastSafePosition = Vector3.zero;

	private Vector3 _battleMoveDestination;

	private float _dragThreshold;

	private float _lastVirtualStickMoveTimeAtCombat;

	public float DragThreshold => (!(_dragThreshold > 0f)) ? (_dragThreshold = Mathf.Max(32f, Screen.dpi * 0.1f)) : _dragThreshold;

	private PlayerBehavior Player => PlayerBehavior.LocalPlayer;

	public float MoveSpeed
	{
		get
		{
			float num = _moveSpeed;
			if (Player.IsTired)
			{
				num = 180f;
			}
			if (Player.IsRiding)
			{
				num = Player.Driver.MoveSpeed;
			}
			return num * _waterRetardingSpeedRatio * CheatMoveSpeedMultiply;
		}
		set
		{
			_moveSpeed = value;
		}
	}

	public float CheatMoveSpeedMultiply { get; set; }

	public bool IsPlayerMoveStart { get; private set; }

	public bool MoveLock
	{
		get
		{
			return _moveLock;
		}
		set
		{
			_moveLock = value;
			if (_moveLock)
			{
				Player.IsMoving = false;
			}
		}
	}

	public bool CutScenePlayMode
	{
		set
		{
			MoveLock = value;
			Player.YawLock = value;
			Player.IgnoreMotionState = value;
		}
	}

	public HashSet<string> WaterFlowRegisterSet => _waterFlowRegisterSet;

	public bool UseWaterHeight
	{
		get
		{
			return _useWaterHeight;
		}
		set
		{
			_useWaterHeight = value;
		}
	}

	public bool UseTileMoveSpeedRatio
	{
		get
		{
			return _useTileMoveSpeedRatio;
		}
		set
		{
			_useTileMoveSpeedRatio = value;
		}
	}

	public bool DrawMode
	{
		get
		{
			return _drawMode;
		}
		set
		{
			if (_drawMode != value)
			{
				_drawMode = value;
				if (this.DrawModeChanged != null)
				{
					this.DrawModeChanged(value);
				}
			}
		}
	}

	public MoveActionType MoveType { get; private set; }

	public MainStatus MainStatus { get; private set; }

	public bool IsInServerSideBattle
	{
		get
		{
			if (GameManager.IsPrologueMode)
			{
				return false;
			}
			return _isInServerSideBattle;
		}
		set
		{
			_isInServerSideBattle = value;
			Connections.Frontend.IsFastResponseMode = value;
		}
	}

	private bool IsInPrologueBattle => GameManager.IsPrologueMode && Player.IsCombatMode;

	public MoveTargetParam MoveTarget => _moveTargetParam;

	public bool IsSafePositionCheck { get; set; }

	public bool AutoAimZoom => _autoAimZoom;

	public event Action MoveStarted;

	public event Action MoveEnded;

	public event GestureProcessDelegate IsGestureProcessed;

	public event TouchProcessDelegate IsTouchProcessed;

	public event PickObjectDelegate OnPickObject;

	public event Action<bool> DrawModeChanged;

	public event Action<Vector3> OnRequestCombatMoveTo;

	private void OnEnable()
	{
		GameSystem<PlayerStatusEffectSystem>.Instance().StatusEffectsUpdated += ApplyVisualEffects;
		KSingleton<PlayerManager>.Instance().PlayerAppeared += delegate(PlayerBehavior player)
		{
			if (player.IsLocalPlayer)
			{
				ApplyVisualEffects();
			}
		};
	}

	private void OnDisable()
	{
		GameSystem<PlayerStatusEffectSystem>.Instance().StatusEffectsUpdated -= ApplyVisualEffects;
	}

	protected override void OnAwake()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		SoundManager.Cache(_touchAudio);
		ParticleManager.Cache(_dieParticleType);
		PlayerBehavior playerBehavior2 = (PlayerBehavior.LocalPlayer = KSingleton<PlayerManager>.Instance().MakePlayerObject(male: false, Vector3.zero, GameManager.PlayerId));
		((Object)((Component)playerBehavior2).gameObject).name = "Player";
		playerBehavior2.Respawned += Player_Respawned;
		playerBehavior2.Died += Player_Died;
		playerBehavior2.KilledAnimal += Player_KilledAnimal;
		playerBehavior2.TileChanged += Player_TileChanged;
		playerBehavior2.TargetChanged += Player_TargetChanged;
		this.MoveStarted = (Action)Delegate.Combine(this.MoveStarted, new Action(StopMusic));
		CheatMoveSpeedMultiply = 1f;
		MoveSpeed = 500f;
		_mainCamera = Camera.main;
		_uiManager = KSingleton<UIManager>.Instance();
		MainStatus = new MainStatus();
		DrawMode = false;
		InitPathBuffer();
		TerrainA6.OnInitTerrain(OnInitialized);
	}

	private void Update()
	{
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		if (!TerrainA6.IsPlayerInitialized && !GameManager.IsPrologueMode)
		{
			return;
		}
		BeginProcessTouches();
		if (GUIUtility.hotControl == 0 && _prevHotControl == 0 && (!DrawMode || !ProcessDrawMode()))
		{
			bool result = false;
			if (this.IsTouchProcessed != null)
			{
				this.IsTouchProcessed(_touchEvents, ref result);
			}
			if (!result && Player.IsAlive && !ProcessPickingAction() && !ProcessGesture())
			{
				if (IsInServerSideBattle)
				{
					ProcessCombatMoveRequests();
				}
				else if (!IsInPrologueBattle && (DrawMode || !AllowJoystickMove || !ProcessJoystickMove()) && (!AllowVirtualStickMove || !ProcessVirtualStickMove()) && MoveType == MoveActionType.VirtualStick)
				{
					StopMove();
				}
			}
		}
		_prevHotControl = GUIUtility.hotControl;
		ProcessLocalPlayerMovements();
		EndProcessTouches();
		ProcessHardwareButton();
		ProcessJoystickButton();
		ProcessMouseWheel();
		if (!IsInServerSideBattle)
		{
			SendMoveMsg();
		}
		ProcessDrawLineBuffers();
		KSingleton<CameraController>.Instance().Process();
		((Component)this).transform.position = Player.MainTransform.position;
	}

	private void ProcessCombatMoveRequests()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		if (!DrawMode && AllowJoystickMove)
		{
			val += CalcJoystickMoveDir();
		}
		if (AllowVirtualStickMove)
		{
			TouchEvent touchEvent = ProcessVirtualStickInput();
			if (touchEvent != null)
			{
				val += CalcVirtualMoveDir();
			}
		}
		float num = 650f;
		if (((Vector3)(ref val)).magnitude > float.Epsilon)
		{
			((Vector3)(ref val)).Normalize();
			if (_battleMoveDestination == KMathUtil.InvalidVector)
			{
				_battleMoveDestination = Player.CurrentPosition;
			}
			_battleMoveDestination += val * num * Time.deltaTime;
			_battleMoveDestination = KMathUtil.ClampEndWithDistance(Player.CurrentPosition, _battleMoveDestination, _maxMoveCursorDist);
			if (this.OnRequestCombatMoveTo != null)
			{
				this.OnRequestCombatMoveTo(_battleMoveDestination);
			}
			_lastVirtualStickMoveTimeAtCombat = Time.time;
		}
		else if (Time.time - _lastVirtualStickMoveTimeAtCombat > 1.2f)
		{
			_battleMoveDestination = KMathUtil.InvalidVector;
		}
		if (Time.time - _lastSentColosseumMove > 0.5f && _battleMoveDestination != KMathUtil.InvalidVector)
		{
			RequestCombatMoveTo(_battleMoveDestination);
			_lastSentColosseumMove = Time.time;
		}
	}

	private void RequestCombatMoveTo(Vector3 pos)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		GameSystem<CombatSystem>.Instance().RequestMoveTo(pos);
	}

	private void OnInitialized()
	{
		Player.CheckCurrentTile(forceUpdate: true);
	}

	public void OnChangeAimTaget(GameObject target)
	{
		PlayerAimTarget msg = default(PlayerAimTarget);
		FillPlayerInfo(out msg.PlayerInfo);
		msg.SentAt = Connections.Frontend.GetPredictedServerTime();
		msg.Target = ObjectIdentifier.GetEntityId(target);
		Connections.Frontend.Send(msg);
	}

	public void StopMove(bool sendCompleteEvent = false)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		_moveTargetParam.Reset();
		Player.IsMoving = false;
		Player.MoveDir = Vector3.zero;
		if (sendCompleteEvent && _onCompleteMoveToTarget != null)
		{
			_onCompleteMoveToTarget(_onCompleteMovoToTargetObj);
		}
		OnPlayerJoystickMove(Vector3.zero);
		_onCompleteMoveToTarget = null;
	}

	public void OnMoveComplete()
	{
		StopMove(sendCompleteEvent: true);
	}

	public void FillPlayerInfo(out Messages.PlayerInfo info)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		info.PlayerId = GameManager.PlayerId;
		Vector3 val = TerrainA6.ClientPositionToWorldPosition(Player.CurrentPosition);
		info.Position.x = val.x;
		info.Position.y = val.z;
	}

	private void AddCurrentMove()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!GameManager.IsPrologueMode)
		{
			if (_curPathBuffer == null)
			{
				Debug.LogError((object)"Movement buffer is null!");
				return;
			}
			Location item = default(Location);
			item.Position.SetFromClientPosition(Player.CurrentPosition);
			item.Floor = Player.Floor;
			item.Yaw = Player.TargetYaw;
			item.Time = Connections.Frontend.GetPredictedServerTime();
			_curPathBuffer.Add(item);
			_lastAddMoveTime = Time.time;
		}
	}

	public void RotateToTarget()
	{
		RotateToTarget(Player.GetTargetObjectByType());
	}

	public void RotateToTarget(GameObject target, bool bSnap = false)
	{
		Player.RotateToTarget(target, bSnap);
	}

	public void RotateToPosition(Vector3 pos, bool bSnap = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Player.RotateToPosition(pos, bSnap);
	}

	public void TurnToYaw(float yaw, bool bSnap = false)
	{
		Player.TurnToYaw(yaw, bSnap);
	}

	public void JoystickMoveTo(Vector3 dir, bool bTargetObjRemove = false)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!MoveLock && OnPlayerJoystickMove(dir))
		{
			SetMoveTo(dir, bTargetObjRemove);
		}
	}

	public void MoveToTarget(Vector3 pos, Action<GameObject> onComplete = null, float distanceThresh = 0f, GameObject targetObj = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!MoveLock)
		{
			Vector3 val = Player.CurrentPosition - pos;
			float magnitude = ((Vector3)(ref val)).magnitude;
			if (magnitude < distanceThresh)
			{
				RotateToPosition(pos);
				onComplete?.Invoke(null);
			}
			else
			{
				_onCompleteMovoToTargetObj = targetObj;
				_onCompleteMoveToTarget = onComplete;
				SetMoveToPos(pos, distanceThresh);
			}
		}
	}

	public void MoveToTarget(GameObject targetObj, Action<GameObject> onComplete = null, float distanceThresh = 0f)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!MoveLock && !((Object)(object)targetObj == (Object)null))
		{
			Vector3 val = KUtility.GetInteractionPosition(targetObj);
			float yaw = KMathUtil.CalcYawWithTarget(val, Player.CurrentPosition);
			MoveTargetParam moveTargetParam = MakeAttachMoveTarget(targetObj);
			if (moveTargetParam != null)
			{
				val = moveTargetParam.TargetPos;
				yaw = moveTargetParam.DestYaw;
				distanceThresh = moveTargetParam.DistanceThresh;
			}
			Vector3 val2 = val - Player.CurrentPosition;
			float magnitude = ((Vector3)(ref val2)).magnitude;
			if (magnitude < distanceThresh)
			{
				TurnToYaw(yaw);
				onComplete?.Invoke(targetObj);
			}
			else
			{
				_onCompleteMovoToTargetObj = targetObj;
				_onCompleteMoveToTarget = onComplete;
				SetMoveTarget(targetObj, distanceThresh);
			}
		}
	}

	private void InitPathBuffer()
	{
		if (_sendMovementsBuffer.Count == 0)
		{
			MotionMovementInfo motionMovementInfo = new MotionMovementInfo();
			motionMovementInfo.MotionName = string.Empty;
			motionMovementInfo.MotionOption = 0;
			motionMovementInfo.PlaybackRate = 1f;
			MotionMovementInfo item = motionMovementInfo;
			_sendMovementsBuffer.Add(item);
		}
		if (_curPathBuffer == null)
		{
			_curPathBuffer = _sendMovementsBuffer[0].Path;
		}
	}

	private void FlushPastLocations()
	{
		if (_curPathBuffer != null)
		{
			AddCurrentMove();
		}
	}

	public void MotionBegined(string motionName, MotionOption motionOption, float playbackRate)
	{
		if (!GameManager.IsPrologueMode && TerrainA6.IsPlayerInitialized)
		{
			FlushPastLocations();
			MotionMovementInfo motionMovementInfo = new MotionMovementInfo();
			motionMovementInfo.MotionName = motionName;
			motionMovementInfo.MotionOption = (byte)motionOption;
			motionMovementInfo.PlaybackRate = playbackRate;
			MotionMovementInfo motionMovementInfo2 = motionMovementInfo;
			_sendMovementsBuffer.Add(motionMovementInfo2);
			_curPathBuffer = motionMovementInfo2.Path;
			AddCurrentMove();
			_requestSendMoveMsg = true;
		}
	}

	private void EnsurePathBuffer()
	{
		if (_sendMovementsBuffer.Count == 0)
		{
			MotionBegined(Player.CurrentAnimKeyName, MotionOption.NORMAL, 1f);
		}
		if (_curPathBuffer == null)
		{
			_curPathBuffer = _sendMovementsBuffer[_sendMovementsBuffer.Count - 1].Path;
		}
	}

	private void SendMoveMsg(bool forciblySend = false)
	{
		if (!GameManager.IsPrologueMode && _sendMovementsBuffer.Count != 0 && (forciblySend || !(_lastMoveSendTime > 0f) || !(_lastMoveSendTime + 0.5f > Time.time)) && _requestSendMoveMsg)
		{
			EnsurePathBuffer();
			RemoveObsolatedPaths();
			Move msg = default(Move);
			msg.EntityId = GameManager.PlayerId;
			msg.Movements = new Movement[_sendMovementsBuffer.Count];
			for (int i = 0; i < _sendMovementsBuffer.Count; i++)
			{
				MotionMovementInfo motionMovementInfo = _sendMovementsBuffer[i];
				ref Movement reference = ref msg.Movements[i];
				reference = new Movement
				{
					MotionName = motionMovementInfo.MotionName,
					MotionOption = (byte)(motionMovementInfo.MotionOption | 0x20u),
					Path = motionMovementInfo.Path.ToArray(),
					PlaybackRate = motionMovementInfo.PlaybackRate,
					RotSpeed = motionMovementInfo.RotSpeed
				};
			}
			Connections.Frontend.Send(msg);
			_lastMoveSendTime = Time.time;
			_requestSendMoveMsg = false;
		}
	}

	private void RemoveObsolatedPaths()
	{
		double at = Connections.Frontend.GetPredictedServerTime() - 1.0;
		for (int num = _sendMovementsBuffer.Count - 1; num >= 0; num--)
		{
			ForgetPastLocationBuffer(_sendMovementsBuffer[num].Path, at);
			if (_sendMovementsBuffer[num].Path.Count == 0 && num != _sendMovementsBuffer.Count - 1)
			{
				_sendMovementsBuffer.RemoveAt(num);
			}
		}
	}

	private void ForgetPastLocationBuffer(List<Location> list, double at)
	{
		int count = list.Count;
		int count2 = count;
		for (int i = 0; i < count; i++)
		{
			if (list[i].Time > at)
			{
				count2 = i;
				break;
			}
		}
		list.RemoveRange(0, count2);
	}

	private void ProcessDrawLineBuffers()
	{
		if (_drawLineBuffer.Count != 0 && (!(_lastDrawLineBufferSendTime > 0f) || !(_lastDrawLineBufferSendTime + 0.5f > Time.time)))
		{
			PlayerDrawLine msg = default(PlayerDrawLine);
			msg.DrawCommands = _drawLineBuffer.ToArray();
			msg.PlayerId = GameManager.PlayerId;
			Connections.Frontend.Send(msg);
			_drawLineBuffer.Clear();
			_lastDrawLineBufferSendTime = Time.time;
		}
	}

	public void CPR(ulong targetId, string state)
	{
		PlayerCPR msg = default(PlayerCPR);
		msg.RescuerId = GameManager.PlayerId;
		msg.TargetId = targetId;
		msg.State = state;
		Connections.Frontend.Send(msg);
	}

	public void ResurrectionRequest(Point2? warpholeTile = null)
	{
		Connections.Frontend.Send(new Revive
		{
			WarpholeTile = warpholeTile
		});
	}

	public void RefreshMotion(string targetAnimKey = "")
	{
		if (string.IsNullOrEmpty(targetAnimKey) || Player.CurrentAnimKeyName == targetAnimKey)
		{
			Motion("Stand");
		}
	}

	public void Motion(string motionState, float time = 0f, float playbackRate = 1f, bool forceTransition = false, string equip = null, [Optional] ItemColor color)
	{
		if (!IsPlayerMoveStart)
		{
			StopMove();
		}
		Player.PlayAnimation(motionState, time, playbackRate, forceTransition, equip, color);
	}

	public void MotionParam(string param, int value)
	{
		if (!IsPlayerMoveStart)
		{
			StopMove();
		}
		Player.SetMotionParam(param, value);
	}

	public void ParticleEffect(string path, float time = 0f)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Player.SetParticleEffect(path, time);
		Relay msg = default(Relay);
		msg.Method = "Particle";
		MessagePackObjectDictionary val = new MessagePackObjectDictionary();
		val.Add(MessagePackObject.op_Implicit("entity_id"), MessagePackObject.op_Implicit(GameManager.PlayerId));
		val.Add(MessagePackObject.op_Implicit("path"), MessagePackObject.op_Implicit(path));
		val.Add(MessagePackObject.op_Implicit("time"), MessagePackObject.op_Implicit(time));
		msg.Data = val;
		Connections.Frontend.Send(msg);
	}

	public void PlaySound(string path)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Player.PlaySound(path);
		Relay msg = default(Relay);
		msg.Method = "Sound";
		MessagePackObjectDictionary val = new MessagePackObjectDictionary();
		val.Add(MessagePackObject.op_Implicit("entity_id"), MessagePackObject.op_Implicit(GameManager.PlayerId));
		val.Add(MessagePackObject.op_Implicit("path"), MessagePackObject.op_Implicit(path));
		msg.Data = val;
		Connections.Frontend.Send(msg);
	}

	public void PlayMusic(Music music, string instrument)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Player.PlayMusic(music, instrument);
		MemoryStream memoryStream = new MemoryStream();
		music.Save(memoryStream);
		byte[] array = memoryStream.ToArray();
		memoryStream.Close();
		Relay msg = default(Relay);
		msg.Method = "Music";
		MessagePackObjectDictionary val = new MessagePackObjectDictionary();
		val.Add(MessagePackObject.op_Implicit("player"), MessagePackObject.op_Implicit(GameManager.PlayerId));
		val.Add(MessagePackObject.op_Implicit("IsPlay"), MessagePackObject.op_Implicit(true));
		val.Add(MessagePackObject.op_Implicit("music"), MessagePackObject.op_Implicit(array));
		val.Add(MessagePackObject.op_Implicit("instrument"), MessagePackObject.op_Implicit(instrument));
		msg.Data = val;
		Connections.Frontend.Send(msg);
	}

	public void StopMusic()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (Player.StopMusic())
		{
			Relay msg = default(Relay);
			msg.Method = "Music";
			MessagePackObjectDictionary val = new MessagePackObjectDictionary();
			val.Add(MessagePackObject.op_Implicit("player"), MessagePackObject.op_Implicit(GameManager.PlayerId));
			val.Add(MessagePackObject.op_Implicit("IsPlay"), MessagePackObject.op_Implicit(false));
			msg.Data = val;
			Connections.Frontend.Send(msg);
			RefreshMotion(string.Empty);
		}
	}

	public void ApplyVisualEffects()
	{
		HashSet<string> visualEffects = GameSystem<PlayerStatusEffectSystem>.Instance().VisualEffects;
		if (visualEffects.Contains("dirtier"))
		{
			SetDirty(CharacterCostume.SkinDirty.VeryDirty);
		}
		else if (visualEffects.Contains("dirty"))
		{
			SetDirty(CharacterCostume.SkinDirty.Dirty);
		}
		else
		{
			SetDirty(CharacterCostume.SkinDirty.Clean);
		}
	}

	public void SetDirty(CharacterCostume.SkinDirty dirty)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (Player.SkinDirtyLevel != dirty)
		{
			Player.SkinDirtyLevel = dirty;
			Relay msg = default(Relay);
			msg.Method = "Dirty";
			MessagePackObjectDictionary val = new MessagePackObjectDictionary();
			val.Add(MessagePackObject.op_Implicit("player"), MessagePackObject.op_Implicit(GameManager.PlayerId));
			val.Add(MessagePackObject.op_Implicit("Dirty"), MessagePackObject.op_Implicit((int)dirty));
			msg.Data = val;
			Connections.Frontend.Send(msg);
		}
	}

	public bool OnPlayerJoystickMove(Vector3 delta)
	{
		if (((Vector3)(ref delta)).sqrMagnitude > 0f)
		{
			if (MoveType != MoveActionType.VirtualStick)
			{
				MoveType = MoveActionType.VirtualStick;
				StartMove();
			}
		}
		else
		{
			if (MoveType != MoveActionType.VirtualStick)
			{
				return false;
			}
			MoveType = MoveActionType.None;
			EndMove();
		}
		return true;
	}

	public void StartMove()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		IsPlayerMoveStart = true;
		_onCompleteMoveToTarget = null;
		_prevSliding = Vector3.zero;
		if (this.MoveStarted != null)
		{
			this.MoveStarted();
		}
	}

	public void EndMove()
	{
		IsPlayerMoveStart = false;
		if (this.MoveEnded != null)
		{
			this.MoveEnded();
		}
	}

	private void Player_Respawned(PlayerBehavior player)
	{
		UIManager.MessageBox.Hide();
		KSingleton<CustomColorCorrectionEffect>.Instance().NightEffectMin = 0f;
		MoveSpeed = 500f;
		Player.RunState = PlayerBehavior.RunStateEnum.Run;
		ExploreData.Region region = KSingleton<GameManager>.Instance().Region;
		DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Respawn, region.Name);
		if ((Object)(object)_dieParticle != (Object)null)
		{
			ParticleManager.Stop(_dieParticle);
			_dieParticle = null;
		}
	}

	private void Player_Died(PlayerBehavior player)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_dieParticle != (Object)null)
		{
			ParticleManager.Stop(_dieParticle);
			_dieParticle = null;
		}
		KSingleton<CustomColorCorrectionEffect>.Instance().NightEffectMin = 1f;
		_dieParticle = ParticleManager.EmitSync(_dieParticleType, player.CurrentPosition, Quaternion.identity);
	}

	private void Player_KilledAnimal(AnimalBehavior victim)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.BattleKill, victim.GetName());
		KSingleton<CameraController>.Instance().AddCameraEffect(new CeremonyCameraEffect(Player, victim.CurrentPosition, 2.5f));
	}

	private void Player_TileChanged(Point2 prev, Point2 current)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!GameManager.IsPrologueMode)
		{
			Biome biome = Player.GetBiome();
			KSingleton<AmbientSoundManager>.Instance().SetBiome(biome);
			KSingleton<AmbientSoundManager>.Instance().SetRiverAudio(Player.CurrentPosition);
		}
	}

	private void Player_TargetChanged(GameObject old, GameObject current)
	{
		OnChangeAimTaget(current);
	}

	public void CombatMode(bool combatMode)
	{
		Player.SetCombatMode(combatMode, 0f);
		PlayerBattle msg = default(PlayerBattle);
		msg.IsAimMode = combatMode;
		FillPlayerInfo(out msg.PlayerInfo);
		msg.SentAt = Connections.Frontend.GetPredictedServerTime();
		Connections.Frontend.Send(msg);
	}

	private bool HasTouch(int id)
	{
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			if (_touchEvents[i].TouchId == id)
			{
				return true;
			}
		}
		return false;
	}

	public void ResetTouchEvents()
	{
		_touchEvents.Clear();
	}

	private TouchEvent GetTouch(int id, Vector2 pos)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			if (_touchEvents[i].TouchId == id)
			{
				_touchEvents[i].LastPos = _touchEvents[i].CurrentPos;
				_touchEvents[i].CurrentPos = pos;
				_touchEvents[i].LastActivateTime = Time.timeSinceLevelLoad;
				return _touchEvents[i];
			}
		}
		TouchEvent touchEvent = new TouchEvent();
		touchEvent.TouchId = id;
		touchEvent.IsTouchBegan = true;
		touchEvent.BeginPos = pos;
		touchEvent.CurrentPos = pos;
		touchEvent.LastPos = pos;
		touchEvent.BeginTime = Time.timeSinceLevelLoad;
		touchEvent.LastActivateTime = Time.timeSinceLevelLoad;
		TouchEvent touchEvent2 = touchEvent;
		_touchEvents.Add(touchEvent2);
		CurrentTouchEvent = touchEvent2;
		return touchEvent2;
	}

	public TouchEvent FindTouch(int id)
	{
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			if (_touchEvents[i].TouchId == id)
			{
				return _touchEvents[i];
			}
		}
		return null;
	}

	public int TouchCount()
	{
		return _touchEvents.Count;
	}

	private bool ProcessDrawMode()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		int count = _touchEvents.Count;
		TouchEvent touchEvent = null;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			TouchEvent touchEvent2 = _touchEvents[i];
			if (touchEvent2.Used == TouchEvent.UsedBy.Joystick)
			{
				flag = true;
			}
			if (touchEvent2.LastActivateTime == Time.timeSinceLevelLoad && !touchEvent2.IsNguiTouched && (touchEvent2.Used == TouchEvent.UsedBy.None || touchEvent2.Used == TouchEvent.UsedBy.Draw) && touchEvent2.TapCount < 2 && (touchEvent == null || touchEvent2.Used == TouchEvent.UsedBy.Draw))
			{
				touchEvent = touchEvent2;
			}
		}
		if (touchEvent == null)
		{
			_ignoreDraw = false;
			return false;
		}
		bool flag2 = touchEvent.Used == TouchEvent.UsedBy.None;
		Vector2 lastPos = touchEvent.LastPos;
		if (_ignoreDraw)
		{
			return false;
		}
		if (flag2)
		{
			Rect fixedModeContainerRect = _uiManager.VirtualStick.GetFixedModeContainerRect();
			if (((Rect)(ref fixedModeContainerRect)).Contains(lastPos))
			{
				_ignoreDraw = true;
				return false;
			}
			touchEvent.Used = TouchEvent.UsedBy.Draw;
			AddLineSegment();
			AddLinePoint(lastPos);
		}
		else
		{
			Vector2 val = lastPos - _previousLinePoint;
			if (((Vector2)(ref val)).sqrMagnitude > 25f)
			{
				AddLinePoint(lastPos);
			}
		}
		return !flag;
	}

	private void AddLineSegment()
	{
		Player.WorldLineRenderer.AddLineSegment();
		DrawLineBase item = default(DrawLineBase);
		item.Time = (ulong)Connections.Frontend.GetPredictedServerTime();
		item.Position.x = 0f;
		item.Position.y = 0f;
		item.Position.z = 0f;
		_drawLineBuffer.Add(item);
	}

	private void AddLinePoint(Vector2 mousePos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = MainCamera.ScreenPosToWorldPos(Vector2.op_Implicit(mousePos));
		Player.WorldLineRenderer.AddLinePoint(val);
		_previousLinePoint = mousePos;
		Vector3 val2 = TerrainA6.ClientPositionToWorldPosition(val);
		DrawLineBase item = default(DrawLineBase);
		item.Time = (ulong)Connections.Frontend.GetPredictedServerTime();
		item.Position.x = val2.x;
		item.Position.y = val2.y;
		item.Position.z = val2.z;
		_drawLineBuffer.Add(item);
	}

	private bool ProcessPickingAction()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		TouchEvent touchEvent = null;
		int num = 0;
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			TouchEvent touchEvent2 = _touchEvents[i];
			if (touchEvent2.LastActivateTime == Time.timeSinceLevelLoad && !touchEvent2.IsNguiTouched && touchEvent2.Used == TouchEvent.UsedBy.None)
			{
				touchEvent = touchEvent2;
				num++;
			}
		}
		if (touchEvent == null || num >= 2)
		{
			return false;
		}
		Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
		bool result = false;
		if (this.OnPickObject != null)
		{
			this.OnPickObject(ray, touchEvent, ref result);
		}
		return result;
	}

	private void BeginProcessTouches()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			TouchEvent touch2 = GetTouch(((Touch)(ref touch)).fingerId, ((Touch)(ref touch)).position);
			touch2.TapCount = ((Touch)(ref touch)).tapCount;
			bool flag = (int)((Touch)(ref touch)).phase == 4 || (int)((Touch)(ref touch)).phase == 3;
			if (((int)((Touch)(ref touch)).phase == 0 || flag) && UICamera.Raycast(Vector2.op_Implicit(((Touch)(ref touch)).position)))
			{
				if ((int)((Touch)(ref touch)).phase == 0)
				{
					SoundManager.Play((string)_touchAudio, loop: false, default(SoundManager.PitchRange));
				}
				touch2.IsNguiTouched = true;
			}
			if (flag)
			{
				touch2.IsTouchBegan = false;
				_toDeleteEvents.Add(((Touch)(ref touch)).fingerId);
			}
		}
		if (_touchEvents.Count >= 1 && !HasTouch(-10))
		{
			return;
		}
		bool mouseButtonDown = Input.GetMouseButtonDown(0);
		bool mouseButtonUp = Input.GetMouseButtonUp(0);
		if (!mouseButtonDown && !_isMouseDown)
		{
			return;
		}
		TouchEvent touch3 = GetTouch(-10, Vector2.op_Implicit(Input.mousePosition));
		if (mouseButtonDown && !_isMouseDown)
		{
			if (UICamera.Raycast(Input.mousePosition))
			{
				SoundManager.Play((string)_touchAudio, loop: false, default(SoundManager.PitchRange));
				touch3.IsNguiTouched = true;
			}
			_isMouseDown = true;
			if ((double)(Time.timeSinceLevelLoad - _lastMouseDownTime) <= 0.2)
			{
				Vector3 val = _lastMouseDownPos - Input.mousePosition;
				if (((Vector3)(ref val)).magnitude < 30f)
				{
					_mouseTapCount++;
					goto IL_01ce;
				}
			}
			_mouseTapCount = 1;
			goto IL_01ce;
		}
		if (mouseButtonUp)
		{
			touch3.IsTouchBegan = false;
			_toDeleteEvents.Add(-10);
			_isMouseDown = false;
			_lastMouseDownPos = Input.mousePosition;
			_lastMouseDownTime = Time.timeSinceLevelLoad;
		}
		return;
		IL_01ce:
		touch3.TapCount = _mouseTapCount;
	}

	private void EndProcessTouches()
	{
		int i = 0;
		for (int count = _toDeleteEvents.Count; i < count; i++)
		{
			int j = 0;
			for (int count2 = _touchEvents.Count; j < count2; j++)
			{
				if (_touchEvents[j].TouchId == _toDeleteEvents[i])
				{
					_touchEvents.RemoveAt(j);
					CurrentTouchEvent = null;
					break;
				}
			}
		}
		_toDeleteEvents.Clear();
	}

	private void ProcessLocalPlayerMovements()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		if (IsInServerSideBattle)
		{
			return;
		}
		bool addMove = false;
		Vector3 val = ProcessLocomotionLocalPlayer(ref addMove);
		Vector3 val2 = ProcessRootmotionLocalPlayer(ref addMove);
		Vector3 val3 = ProcessWaterFlowLocalPlayer(ref addMove);
		Player.IsWaterCarried = val3 != Vector3.zero;
		Vector3 delta = val + val2 + val3;
		delta = ProcessTileMoveSpeedRatio(delta);
		delta = CancelMoveIfNotLoaded(delta);
		Vector3 newPos = ProcessCollisionWithSliding(delta, ref addMove);
		ProcessWaterLevel(newPos);
		float num = Vector3.Dot(_lastMoveDir, Player.MoveDir);
		addMove |= (Player.IsMoving && num < 0.99f) || _lastMoving != Player.IsMoving;
		if (Player.IsMoving && Player.IsAlive && Time.time - _lastAddMoveTime >= 0.5f)
		{
			addMove = true;
		}
		if (addMove)
		{
			if (!_lastMoving)
			{
				_departSent = false;
			}
			if ((val != Vector3.zero || val2 != Vector3.zero) && !_departSent)
			{
				Connections.Frontend.Send(default(Depart));
				_departSent = true;
			}
			AddCurrentMove();
			_lastSentPosition = Player.CurrentPosition;
			_lastSentYaw = Player.TargetYaw;
			_lastMoveDir = Player.MoveDir;
			_lastMoving = Player.IsMoving;
			_requestSendMoveMsg = true;
		}
	}

	private Vector3 CancelMoveIfNotLoaded(Vector3 delta)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.IsPrologueMode)
		{
			return delta;
		}
		Biome tileBiome = TerrainA6.GetTileBiome(TerrainA6.ClientPositionToWorldPosition(Player.CurrentPosition + delta));
		if (tileBiome == Biome.Unspecified)
		{
			delta = Vector3.zero;
		}
		return delta;
	}

	private Vector3 ProcessLocomotionLocalPlayer(ref bool addMove)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		if (!Player.IsMoving)
		{
			return Vector3.zero;
		}
		if (IsImmovableState())
		{
			return Vector3.zero;
		}
		if (_moveTargetParam.HasGoal || Object.op_Implicit((Object)(object)_moveTargetParam.TargetObj))
		{
			if (Object.op_Implicit((Object)(object)_moveTargetParam.TargetObj))
			{
				_moveTargetParam.TargetPos = KUtility.GetInteractionPosition(_moveTargetParam.TargetObj);
				Player.RotateToPosition(_moveTargetParam.TargetPos);
			}
			Vector3 moveDir = _moveTargetParam.TargetPos - Player.CurrentPosition;
			moveDir.y = 0f;
			float magnitude = ((Vector3)(ref moveDir)).magnitude;
			if (magnitude < 10f || magnitude < _moveTargetParam.DistanceThresh)
			{
				if (_moveTargetParam.HasYawTaget)
				{
					TurnToYaw(_moveTargetParam.DestYaw);
				}
				OnMoveComplete();
				addMove = true;
				return Vector3.zero;
			}
			((Vector3)(ref moveDir)).Normalize();
			Player.MoveDir = moveDir;
		}
		Vector3 result = Player.MoveDir * Time.deltaTime * MoveSpeed;
		if (_moveTargetParam.HasGoal)
		{
			Vector3 val = _moveTargetParam.TargetPos - Player.CurrentPosition;
			if (((Vector3)(ref val)).sqrMagnitude < ((Vector3)(ref result)).sqrMagnitude)
			{
				if (_moveTargetParam.HasYawTaget)
				{
					TurnToYaw(_moveTargetParam.DestYaw);
				}
				OnMoveComplete();
				addMove = true;
				return Vector3.zero;
			}
		}
		return result;
	}

	private Vector3 ProcessRootmotionLocalPlayer(ref bool addMove)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (IsInServerSideBattle)
		{
			return Vector3.zero;
		}
		if (Player.IsMoving)
		{
			return Vector3.zero;
		}
		Vector3 lastRootMotionDelta = Player.LastRootMotionDelta;
		Vector3 val = Player.CurrentPosition - _lastSentPosition;
		if (((Vector3)(ref val)).sqrMagnitude > 100f)
		{
			addMove = true;
		}
		if (KMathUtil.DistanceAngDeg(Player.TargetYaw, _lastSentYaw) > 10f)
		{
			addMove = true;
		}
		return lastRootMotionDelta;
	}

	private Vector3 ProcessWaterFlowLocalPlayer(ref bool addMove)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.IsPrologueMode)
		{
			return Vector3.zero;
		}
		if (!UseWaterHeight || !TerrainA6.IsPlayerInitialized)
		{
			return Vector3.zero;
		}
		if (IsInServerSideBattle)
		{
			return Vector3.zero;
		}
		if (Player.WaterDepthLevel <= TerrainWater.WaterDepthLevel.Foot)
		{
			return Vector3.zero;
		}
		if (Player.IsCurrentPlayAnimTag(PlayerAnimationClipTag.WaterFlowResist) || _waterFlowRegisterSet.Count > 0)
		{
			return Vector3.zero;
		}
		if (Player.IsRiding && Player.Driver.Vehicle.Size >= CharacterBehavior.SizeLevel.Medium)
		{
			return Vector3.zero;
		}
		Vector3 worldPosition = TerrainA6.ClientPositionToWorldPosition(Player.CurrentPosition);
		Vector2 waterFlow = TerrainA6.GetWaterFlow(worldPosition);
		if (waterFlow == Vector2.zero)
		{
			return Vector3.zero;
		}
		addMove = true;
		return Time.deltaTime * new Vector3(waterFlow.x, 0f, waterFlow.y) * TerrainWater.RiverSpeed * Player.WaterDepth;
	}

	private Vector3 ProcessTileMoveSpeedRatio(Vector3 delta)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!UseTileMoveSpeedRatio || !TerrainA6.IsPlayerInitialized)
		{
			return delta;
		}
		if (IsInServerSideBattle)
		{
			return delta;
		}
		Vector3 worldPos = TerrainA6.ClientPositionToWorldPosition(Player.CurrentPosition);
		float tileMoveSpeedRatio = GetTileMoveSpeedRatio(worldPos);
		delta *= tileMoveSpeedRatio;
		return delta;
	}

	private float GetTileMoveSpeedRatio(Vector3 worldPos)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Player.IsRiding)
		{
			return 1f;
		}
		return TerrainA6.GetTileMoveType(worldPos) switch
		{
			TerrainA6.TileMoveType.None => 1f, 
			TerrainA6.TileMoveType.Road => _roadMoveSpeedRatio, 
			TerrainA6.TileMoveType.Tiny => _bushMoveSpeedRatioTiny, 
			TerrainA6.TileMoveType.Small => _bushMoveSpeedRatioSmall, 
			TerrainA6.TileMoveType.Medium => _bushMoveSpeedRatioMedium, 
			TerrainA6.TileMoveType.Large => _bushMoveSpeedRatioLarge, 
			_ => 1f, 
		};
	}

	private static bool IsInOceanMargin(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = TerrainA6.WorldPositionToTilePosition(worldPos);
		int tileCount = TerrainMeta.TileCount;
		return val.x >= (float)(tileCount - 20) || val.y >= (float)(tileCount - 20) || val.x < 20f || val.y < 20f;
	}

	private static bool IsNotMovable(Vector3 newPos, Vector3 oldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		bool flag = IsInOceanMargin(oldPos);
		bool flag2 = IsInOceanMargin(newPos);
		if (flag && flag2)
		{
			float num = (float)TerrainMeta.TileCount * 0.5f * 200f;
			newPos.x -= num;
			newPos.z -= num;
			oldPos.x -= num;
			oldPos.z -= num;
			return ((Vector3)(ref newPos)).sqrMagnitude > ((Vector3)(ref oldPos)).sqrMagnitude;
		}
		return flag2;
	}

	private void ProcessWaterLevel(Vector3 newPos)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (!UseWaterHeight || !TerrainA6.IsPlayerInitialized)
		{
			newPos.y = 0f;
			Player.CurrentPosition = newPos;
		}
		else
		{
			if (Player.CurrentPosition == newPos && Player.WaterDepthLevel != TerrainWater.WaterDepthLevel.Swim)
			{
				return;
			}
			Vector3 val = TerrainA6.ClientPositionToWorldPosition(newPos);
			Vector3 oldPos = TerrainA6.ClientPositionToWorldPosition(Player.CurrentPosition);
			Vector2 floatTile = TerrainA6.WorldPositionToTilePosition(val);
			byte floor = Player.Floor;
			float tileDepth = TerrainA6.GetTileDepth(floatTile, ref floor);
			Player.Floor = floor;
			bool flag = IsTooDeepToSwim(tileDepth);
			if (IsNotMovable(val, oldPos) || flag)
			{
				newPos = Player.CurrentPosition;
				if (flag)
				{
					NoticeDeepWater();
				}
			}
			else
			{
				_waterRetardingSpeedRatio = TerrainWater.GetRelativeSpeed(tileDepth);
			}
			Player.CurrentPosition = newPos;
		}
	}

	private void NoticeDeepWater()
	{
		if (!(Time.time - _lastNoticeTime < 3f))
		{
			_lastNoticeTime = Time.time;
			UIManager.SystemMsg(T._("깊어서 더이상 이동할 수 없습니다."), 2f);
		}
	}

	private bool IsTooDeepToSwim(float depth)
	{
		if (GameManager.IsPrologueMode)
		{
			return false;
		}
		if (TerrainWater.GetWaterDepthLevel(depth) <= TerrainWater.WaterDepthLevel.Waist)
		{
			return false;
		}
		if (TerrainWater.GetWaterDepthLevel(depth) == TerrainWater.WaterDepthLevel.Deep)
		{
			return true;
		}
		if (!Player.IsSwimmable)
		{
			return true;
		}
		return !TerrainWater.IsMovableDepth(depth, Player.SwimmableDepthRatio);
	}

	private Vector3 ProcessCollisionWithSliding(Vector3 delta, ref bool addMove)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		if (delta == Vector3.zero)
		{
			return Player.CurrentPosition;
		}
		CollisionParam param = KCollisionUtility.CreateCollisionParam(Player.CurrentPosition, delta);
		delta = ((!(Player.LastRootMotionDelta != Vector3.zero) && MoveType != MoveActionType.VirtualStick) ? ProcessPathFindSliding(param, ref addMove) : KCollisionUtility.ProcessSimpleSliding(param));
		if (GameManager.IsPrologueMode && param.Distance < 0.3f)
		{
			StopMove();
			EndMove();
		}
		if (IsSafePositionCheck)
		{
			CollisionParam param2 = KCollisionUtility.CreateCollisionParam(Player.CurrentPosition, delta);
			if (KCollisionUtility.TryCapsuleCast(param2, out var _) == RayCastResult.Pass)
			{
				_lastSafePosition = Player.CurrentPosition;
			}
			if (_lastSafePosition != Vector3.zero && ((Vector3)(ref delta)).magnitude <= Mathf.Epsilon)
			{
				return _lastSafePosition;
			}
		}
		return Player.CurrentPosition + delta;
	}

	private Vector3 ProcessPathFindSliding(CollisionParam param, ref bool addMove)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = param.Direction * param.Distance;
		if (KCollisionUtility.CheckCollision(param, collideOnOverlapped: false, out var normal))
		{
			if (_prevSliding == Vector3.zero)
			{
				float num = Mathf.Atan2(param.Direction.z, param.Direction.x);
				float num2 = Mathf.Atan2(normal.z, normal.x);
				float num3 = num - num2;
				int num4 = ((Mathf.Abs(num3) < (float)Math.PI) ? 1 : (-1));
				float num5 = num2 + Mathf.Sign(num3) * (float)num4 * (float)Math.PI / 2f;
				_prevSliding.x = Mathf.Cos(num5);
				_prevSliding.z = Mathf.Sin(num5);
				Player.TargetYaw = KMathUtil.CalcYaw(_prevSliding);
				_slidingKeepLengthCounter = 50f;
				addMove = true;
			}
			param.Direction = _prevSliding;
			if (KCollisionUtility.CheckCollision(param, collideOnOverlapped: true, out normal))
			{
				_prevSliding = Vector3.zero;
				StopMove();
				return Vector3.zero;
			}
			val = _prevSliding * param.Distance * 1f;
		}
		else
		{
			if (_prevSliding == Vector3.zero)
			{
				return val;
			}
			if (_slidingKeepLengthCounter < 0f)
			{
				_prevSliding = Vector3.zero;
				Player.TargetYaw = KMathUtil.CalcYaw(val);
				addMove = true;
			}
			else
			{
				param.Direction = _prevSliding;
				if (!KCollisionUtility.CheckCollision(param, collideOnOverlapped: true, out normal))
				{
					val = _prevSliding * param.Distance;
					_slidingKeepLengthCounter -= param.Distance;
				}
			}
		}
		return val;
	}

	private void SetMoveTarget(GameObject targetObj, float distanceThresh)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		Vector3 interactionPosition = KUtility.GetInteractionPosition(targetObj);
		MoveTargetParam moveTargetParam = MakeAttachMoveTarget(targetObj);
		if (moveTargetParam == null)
		{
			float num = KMathUtil.CalcYawWithTarget(_moveTargetParam.TargetPos, Player.CurrentPosition);
			Player.MainTransform.localRotation = Quaternion.Euler(0f, num, 0f);
			SetMoveToPos(interactionPosition, distanceThresh, targetObj);
			return;
		}
		_moveTargetParam.CopyFrom(moveTargetParam);
		Player.IsMoving = true;
		if (!IsYawLockState())
		{
			float targetYaw = KMathUtil.CalcYawWithTarget(_moveTargetParam.TargetPos, Player.CurrentPosition);
			Player.TargetYaw = targetYaw;
		}
	}

	private MoveTargetParam MakeAttachMoveTarget(GameObject targetObj)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		MoveTargetParam moveTargetParam = new MoveTargetParam();
		Transform val = KUtility.FindTransformByName(targetObj, "Attachment_Player");
		if (Object.op_Implicit((Object)(object)val))
		{
			moveTargetParam.TargetPos = val.position;
			moveTargetParam.DestYaw = (int)KMathUtil.CalcYaw(val.forward);
			moveTargetParam.DistanceThresh = 10f;
			moveTargetParam.TargetObj = ((Component)val).gameObject;
			moveTargetParam.HasGoal = true;
			return moveTargetParam;
		}
		return null;
	}

	private void SetMoveTo(Vector3 dir, bool bTargetObjRemove = false)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (bTargetObjRemove)
		{
			_moveTargetParam.TargetObj = null;
		}
		if (Math.Abs(((Vector3)(ref dir)).sqrMagnitude) < float.Epsilon)
		{
			if (!_moveTargetParam.HasGoal)
			{
				Player.IsMoving = false;
				Player.MoveDir = Vector3.zero;
			}
			return;
		}
		Player.MoveDir = dir;
		_moveTargetParam.Reset();
		Player.IsMoving = true;
		if (!IsYawLockState())
		{
			float targetYaw = KMathUtil.CalcYaw(Player.MoveDir);
			Player.TargetYaw = targetYaw;
		}
	}

	private void SetMoveToPos(Vector3 targetPos, float distanceThresh, GameObject targetObj = null)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Player.IsMoving = true;
		_moveTargetParam.HasGoal = true;
		_moveTargetParam.TargetPos = targetPos;
		_moveTargetParam.DestYaw = MoveTargetParam.InvalidYaw;
		_moveTargetParam.DistanceThresh = distanceThresh;
		_moveTargetParam.TargetObj = targetObj;
		if (!IsYawLockState())
		{
			float targetYaw = KMathUtil.CalcYawWithTarget(_moveTargetParam.TargetPos, Player.CurrentPosition);
			Player.TargetYaw = targetYaw;
		}
	}

	public void Teleport(Vector3 pos, TeleportType type = TeleportType.Unknown)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.IsPrologueMode)
		{
			SetPlayerLocation(pos);
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(CoTeleportAndFadeScreen(pos, type));
		}
	}

	private void SetPlayerLocation(Vector3 pos)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<CameraController>.Instance().ResetCameraTarget();
		_lastSafePosition = Vector3.zero;
		Player.Teleport(pos);
		OnMoveComplete();
	}

	private IEnumerator CoTeleportAndFadeScreen(Vector3 pos, TeleportType type)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		LoadingCurtainGroup loadingCurtain = UIManager.FindScript<LoadingCurtainGroup>();
		yield return ((MonoBehaviour)this).StartCoroutine(loadingCurtain.CoTakeScreenShot());
		loadingCurtain.ShowTeleportScreen();
		SetPlayerLocation(pos);
		float remainTime = 1f;
		while (!KSingleton<TerrainA6>.Instance().IsChunkLoading && remainTime > 0f)
		{
			remainTime -= Time.deltaTime;
			yield return null;
		}
		while (KSingleton<TerrainA6>.Instance().IsChunkLoading)
		{
			yield return null;
		}
		loadingCurtain.EndLoading();
		if (type == TeleportType.Warp || type == TeleportType.WarpBack)
		{
			Motion("Warp_End", 0f, 1f, forceTransition: true);
		}
	}

	private Vector3 CalcJoystickMoveDir()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (MoveLock)
		{
			return Vector3.zero;
		}
		float num = Input.GetAxis("Horizontal");
		float num2 = Input.GetAxis("Vertical");
		if (Mathf.Abs(num) <= 0.1f)
		{
			num = 0f;
		}
		if (Mathf.Abs(num2) <= 0.1f)
		{
			num2 = 0f;
		}
		if (num == 0f && num2 == 0f)
		{
			return Vector3.zero;
		}
		float num3 = Mathf.Sin(-(float)Math.PI / 4f);
		float num4 = Mathf.Cos(-(float)Math.PI / 4f);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(num, num2);
		((Vector2)(ref val)).Normalize();
		Vector3 result = default(Vector3);
		((Vector3)(ref result))._002Ector(val.x * num4 - val.y * num3, 0f, val.x * num3 + val.y * num4);
		((Vector3)(ref result)).Normalize();
		return result;
	}

	private bool ProcessJoystickMove()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = CalcJoystickMoveDir();
		if (val == Vector3.zero)
		{
			return false;
		}
		JoystickMoveTo(val, bTargetObjRemove: true);
		return true;
	}

	private TouchEvent ProcessVirtualStickInput()
	{
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		if (MoveLock)
		{
			if (_uiManager.VirtualStick.Pressed)
			{
				_uiManager.VirtualStick.Release();
			}
			return null;
		}
		TouchEvent touchEvent = null;
		int num = 0;
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			TouchEvent touchEvent2 = _touchEvents[i];
			if (touchEvent2.LastActivateTime == Time.timeSinceLevelLoad && !touchEvent2.IsNguiTouched && touchEvent2.Used != TouchEvent.UsedBy.Gesture && touchEvent2.TapCount < 2 && (!touchEvent2.IsTouchBegan || touchEvent2.Used != 0 || !((double)(touchEvent2.LastActivateTime - touchEvent2.BeginTime) <= 0.1)))
			{
				if (touchEvent == null || touchEvent2.Used == TouchEvent.UsedBy.Joystick)
				{
					touchEvent = touchEvent2;
				}
				num++;
			}
		}
		if (!((Component)_uiManager.VirtualStick).gameObject.activeInHierarchy)
		{
			return null;
		}
		if (touchEvent == null || (!DrawMode && num >= 2) || !touchEvent.IsTouchBegan)
		{
			_uiManager.VirtualStick.Release();
			return null;
		}
		if (_uiManager.VirtualStick.Pressed)
		{
			_uiManager.VirtualStick.Drag(Vector2.op_Implicit(touchEvent.CurrentPos));
		}
		else
		{
			_uiManager.VirtualStick.Press(Vector2.op_Implicit(touchEvent.CurrentPos));
		}
		if (!_uiManager.VirtualStick.IsVisible)
		{
			return null;
		}
		touchEvent.Used = TouchEvent.UsedBy.Joystick;
		return touchEvent;
	}

	private bool ProcessVirtualStickMove()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		TouchEvent touchEvent = ProcessVirtualStickInput();
		if (touchEvent == null)
		{
			return false;
		}
		Vector3 dir = CalcVirtualMoveDir();
		JoystickMoveTo(dir, bTargetObjRemove: true);
		return true;
	}

	private Vector3 CalcVirtualMoveDir()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Sin(-(float)Math.PI / 4f);
		float num2 = Mathf.Cos(-(float)Math.PI / 4f);
		Vector2 position = _uiManager.VirtualStick._position;
		Vector3 result = default(Vector3);
		((Vector3)(ref result))._002Ector(position.x * num2 - position.y * num, 0f, position.x * num + position.y * num2);
		((Vector3)(ref result)).Normalize();
		return result;
	}

	private Vector3 CalcTouchedWorldPos()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		TouchEvent touchEvent = null;
		int num = 0;
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			TouchEvent touchEvent2 = _touchEvents[i];
			if (touchEvent2.LastActivateTime == Time.timeSinceLevelLoad && !touchEvent2.IsNguiTouched && touchEvent2.Used != TouchEvent.UsedBy.Gesture && touchEvent2.Used != TouchEvent.UsedBy.Joystick && !touchEvent2.IsTouchBegan)
			{
				touchEvent = touchEvent2;
				num++;
			}
		}
		if (touchEvent == null || num >= 2)
		{
			return KMathUtil.InvalidVector;
		}
		return MainCamera.ScreenPosToWorldPos(Vector2.op_Implicit(touchEvent.CurrentPos));
	}

	private void ProcessHardwareButton()
	{
		if (Input.GetKeyDown((KeyCode)27))
		{
			UIBase.CloseUI();
		}
	}

	private void ProcessJoystickButton()
	{
		if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			if (Input.GetKeyDown((KeyCode)330))
			{
				GameSystem<CombatSystem>.Instance().SelectAction(2);
			}
			else if (Input.GetKeyDown((KeyCode)331))
			{
				GameSystem<CombatSystem>.Instance().SelectAction(1);
			}
			else if (Input.GetKeyDown((KeyCode)332))
			{
				GameSystem<CombatSystem>.Instance().SelectAction(3);
			}
			else if (Input.GetKeyDown((KeyCode)333))
			{
				GameSystem<CombatSystem>.Instance().SelectAction(0);
			}
			float axis = Input.GetAxis("Vertical Cross");
			if (Mathf.Abs(axis) > 0.1f && !(Time.time < _policyLockTime))
			{
				if (axis > 0f)
				{
					_lastSelectedPolicy++;
				}
				else if (axis < 0f)
				{
					_lastSelectedPolicy--;
				}
				_lastSelectedPolicy = ((_lastSelectedPolicy < 0) ? 2 : ((_lastSelectedPolicy <= 2) ? _lastSelectedPolicy : 0));
				GameSystem<CombatSystem>.Instance().SelectPolicy(_lastSelectedPolicy);
				_policyLockTime = Time.time + 0.5f;
			}
		}
		else if (Input.GetKeyDown((KeyCode)330))
		{
			InteractionFront();
		}
		else if (Input.GetKeyDown((KeyCode)331))
		{
			UIBase.CloseUI();
		}
		else if (Input.GetKeyDown((KeyCode)332))
		{
			InventoryGroup inventoryGroup = UIManager.FindScript<InventoryGroup>();
			if (inventoryGroup.IsOpen)
			{
				inventoryGroup.Close();
				return;
			}
			GameSystem<InventorySystem>.Instance().SortItemList(Util.SortOption.Default);
			inventoryGroup.Open();
		}
		else if (Input.GetKeyDown((KeyCode)333))
		{
			RecipeSelectorGroup recipeSelectorGroup = UIManager.FindScript<RecipeSelectorGroup>();
			if (recipeSelectorGroup.IsOpen)
			{
				recipeSelectorGroup.Close();
			}
			else
			{
				recipeSelectorGroup.Open();
			}
		}
	}

	private void ProcessMouseWheel()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (!(Math.Abs(axis) < float.Epsilon))
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = axis;
			DoGesture(Gesture.Zoom, mousePosition, touchedUI: false);
		}
	}

	private bool ProcessGesture()
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		bool flag = true;
		int count = _touchEvents.Count;
		for (int i = 0; i < count; i++)
		{
			TouchEvent touchEvent = _touchEvents[i];
			if (touchEvent.IsTouchBegan && touchEvent.Used != TouchEvent.UsedBy.Move && touchEvent.Used != TouchEvent.UsedBy.Joystick)
			{
				if (num >= 2)
				{
					return false;
				}
				_gestureTouches[num] = touchEvent;
				num++;
				if (touchEvent.LastActivateTime == Time.timeSinceLevelLoad)
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			return false;
		}
		float dragThreshold = DragThreshold;
		switch (num)
		{
		case 1:
		{
			if (_gestureTouches[0].Used != TouchEvent.UsedBy.Gesture)
			{
				Vector2 val4 = _gestureTouches[0].CurrentPos - _gestureTouches[0].BeginPos;
				if (((Vector2)(ref val4)).magnitude < dragThreshold)
				{
					return false;
				}
			}
			Vector3 vector = Vector2.op_Implicit(_gestureTouches[0].CurrentPos - _gestureTouches[0].LastPos);
			if (!DoGesture(Gesture.Panning, vector, _gestureTouches[0].IsNguiTouched))
			{
				return false;
			}
			break;
		}
		case 2:
		{
			Vector2 val = _gestureTouches[0].LastPos - _gestureTouches[1].LastPos;
			float magnitude = ((Vector2)(ref val)).magnitude;
			Vector2 val2 = _gestureTouches[0].CurrentPos - _gestureTouches[1].CurrentPos;
			float magnitude2 = ((Vector2)(ref val2)).magnitude;
			if (magnitude2 < dragThreshold && _gestureTouches[0].Used != TouchEvent.UsedBy.Gesture && _gestureTouches[1].Used != TouchEvent.UsedBy.Gesture)
			{
				return false;
			}
			float num2 = (magnitude2 - magnitude) / (float)Screen.height * 2f;
			Vector2 val3 = (_gestureTouches[0].CurrentPos + _gestureTouches[1].CurrentPos) / 2f;
			bool touchedUI = _gestureTouches[0].IsNguiTouched || _gestureTouches[1].IsNguiTouched;
			if (!DoGesture(Gesture.Zoom, new Vector3(val3.x, val3.y, num2), touchedUI))
			{
				return false;
			}
			break;
		}
		}
		for (int j = 0; j < num; j++)
		{
			_gestureTouches[j].Used = TouchEvent.UsedBy.Gesture;
		}
		return true;
	}

	private bool DoGesture(Gesture gesture, Vector3 vector, bool touchedUI)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		if (this.IsGestureProcessed != null)
		{
			this.IsGestureProcessed(gesture, vector, touchedUI, ref result);
		}
		return result;
	}

	private bool IsImmovableState()
	{
		if (Time.frameCount != _immovableStateUpdateFrame)
		{
			_isImmovableState = !Player.IsCurrentAnimTagContainCrossFade(PlayerAnimationClipTag.Run);
		}
		return _isImmovableState;
	}

	private bool IsYawLockState()
	{
		return IsImmovableState();
	}

	private void InteractionFront()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		float num = Player.CurrentYaw * ((float)Math.PI / 180f);
		Vector3 currentPosition = Player.CurrentPosition;
		currentPosition.x += Mathf.Sin(num) * 50f;
		currentPosition.z += Mathf.Cos(num) * 50f;
		Vector3 val = _mainCamera.WorldToScreenPoint(currentPosition);
		val.y += (float)Screen.height * 0.025f;
		val.z = 0f;
		Ray ray = UICamera.mainCamera.ScreenPointToRay(val);
		int mask = 1 << LayerMask.NameToLayer("NGUI");
		if (KUtility.RayCastContextAction(ray, mask, "Selectable", out var pickingObject))
		{
			UICamera.currentTouch = UICamera.GetTouch(1, createIfMissing: true);
			UICamera.currentTouch.pos.x = val.x;
			UICamera.currentTouch.pos.y = val.y;
			pickingObject.SendMessage("OnClick");
		}
	}

	public static void PlayRewardMotion(RewardAlarmGroup.RewardReason reason, float delay = 0.5f)
	{
		if (KSingleton<PlayerController>.HasInstance() && !PlayerBehavior.LocalPlayer.IsMoving && !KSingleton<PlayerController>.Instance().IsInServerSideBattle)
		{
			string motion = "Avatar_Levelup";
			switch (reason)
			{
			case RewardAlarmGroup.RewardReason.GrownUp:
				motion = "Avatar_Levelup";
				break;
			case RewardAlarmGroup.RewardReason.Success:
				motion = "Craft_Success";
				break;
			}
			KUtility.DelayedCall((MonoBehaviour)(object)KSingleton<PlayerController>.Instance(), delegate
			{
				KSingleton<PlayerController>.Instance().Motion(motion);
			}, delay);
		}
	}
}
