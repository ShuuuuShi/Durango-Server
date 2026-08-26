using System;
using System.Collections.Generic;
using Durango.Environment;
using Durango.Model;
using Durango.Network;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.Render.Screen;
using Durango.System;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using InteractionData;
using L10N;
using Messages;
using Shared.Battle;
using Shared.Region;
using Shared.Teleport;
using UnityEngine;
using Yaml;
using Yaml.Util;

[RequireComponent(typeof(TerrainWater))]
public class PlayerController : Durango.Utils.Singleton<PlayerController>
{
	public const float DefaultMoveSpeed = 500f;

	public const float DefaultRotSpeed = 540f;

	public const float MoveSpeedTired = 180f;

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
	private ParticleType _dieParticleType;

	[SerializeField]
	private ParticleType _moveCursorParticle;

	private float _waterRetardingSpeedRatio = 1f;

	private bool _isCutScenPlayMode;

	private readonly HashSet<string> _waterFlowRegisterSet = new HashSet<string>();

	private int _dieParticleId;

	private float _dragThreshold;

	private bool _manualDirectionalMove;

	private bool _manualPickingMove;

	private Vector3 _manualMoveDir;

	private bool _manualMoveAllowed = true;

	private bool _ignoreSimilarMoveDir;

	private Vector3? _lastMoveDir;

	private bool _stopMoveReserved;

	private LocalMoveNavigator _localMoveNavigator;

	private LocalMoveOperator _localMoveOperator;

	private LocalMotionUpdater _localMotionUpdater;

	private readonly Observable<bool> _occluded = new Observable<bool>();

	private float _rotateSpeed;

	private SetBaseMoveSpeed? _baseMoveSpeed;

	public static LocalMoveOperator MoveOperator => Durango.Utils.Singleton<PlayerController>.Instance()._localMoveOperator;

	public static LocalMotionUpdater MotionUpdater => Durango.Utils.Singleton<PlayerController>.Instance()._localMotionUpdater;

	public bool IgnoreOcclusionCheck { get; set; }

	public float DragThreshold => (!(_dragThreshold > 0f)) ? (_dragThreshold = Mathf.Max(32f, Screen.dpi * 0.1f)) : _dragThreshold;

	private static PlayerBehavior Player => PlayerBehavior.LocalPlayer;

	public float MoveSpeed
	{
		get
		{
			SetBaseMoveSpeed? baseMoveSpeed = _baseMoveSpeed;
			float num = ((!baseMoveSpeed.HasValue) ? 500f : ((!GameSystem<CombatSystem>.Instance().CombatMode) ? ((float)_baseMoveSpeed.Value.NormalSpeed) : ((float)_baseMoveSpeed.Value.BattleSpeed)));
			if (!GameManager.IsPrologueMode)
			{
				if (Player.IsRiding)
				{
					num = Player.Driver.MoveSpeed;
				}
				else if (Player.IsTired)
				{
					num = 180f;
				}
			}
			return num * CheatMoveSpeedMultiply;
		}
	}

	public float RotateSpeed
	{
		get
		{
			return (!Player.IsRiding) ? _rotateSpeed : Player.Driver.RotateSpeed;
		}
		set
		{
			_rotateSpeed = value;
		}
	}

	public float WaterRetardingSpeedRatio
	{
		get
		{
			return _waterRetardingSpeedRatio;
		}
		set
		{
			_waterRetardingSpeedRatio = value;
		}
	}

	public float CheatMoveSpeedMultiply { get; set; }

	public bool CutScenePlayMode
	{
		get
		{
			return _isCutScenPlayMode;
		}
		set
		{
			_isCutScenPlayMode = value;
			GameSystem<InputSystem>.Instance().MoveLock = value;
		}
	}

	public bool IsProhibitAnimRefresh { get; set; }

	public HashSet<string> WaterFlowRegisterSet => _waterFlowRegisterSet;

	public event Action MoveStarted;

	public event Action TryMoveWhenLocked;

	public event Action MoveEnded;

	protected override void OnAwake()
	{
		ParticleManager.Cache(_dieParticleType);
		PlayerBehavior playerBehavior2 = (PlayerBehavior.LocalPlayer = Durango.Utils.Singleton<PlayerManager>.Instance().MakePlayerObject(male: false, null, GameManager.PlayerId, "Barehand_Stand", loadClips: false));
		playerBehavior2.gameObject.name = "Player";
		playerBehavior2.Revived += Player_Revived;
		playerBehavior2.Died += Player_Died;
		playerBehavior2.TileChanged += Player_TileChanged;
		playerBehavior2.MotionConditionChanged += Player_MotionConditionChanged;
		CheatMoveSpeedMultiply = 1f;
		_rotateSpeed = 540f;
		Durango.Utils.Singleton<TerrainBase>.Instance().OnReady(OnInitialized);
		_localMoveNavigator = new LocalMoveNavigator();
		_localMotionUpdater = new LocalMotionUpdater();
		_localMoveOperator = new LocalMoveOperator();
		_localMoveOperator.SetMotionUpdater(_localMotionUpdater);
		_localMoveOperator.MovingBlocked += _localMoveNavigator.MoveOperator_MovingBlocked;
		_localMoveOperator.TargetYawReached += _localMoveNavigator.MoveOperator_TargetYawReached;
		_localMoveOperator.CollisionSlideOccurred += _localMoveNavigator.MoveOperator_CollisionSlideOccurred;
		Observable<bool> isBattleStand = _localMotionUpdater.IsBattleStand;
		isBattleStand.Changed = (Action<bool>)Delegate.Combine(isBattleStand.Changed, (Action<bool>)delegate
		{
			RefreshPlayerOutline();
		});
		GameSystem<InputSystem>.Instance().On(InputCommand.MoveToPositionByDragging, delegate(InputCommandMessage message)
		{
			ProcessInputPickingMove(message, showMoveCursor: false);
		});
		// [แก้เอง] 23 ส.ค. 2026 — เจ้าของสั่งเพิ่ม "คลิกเพื่อเดิน" ในโหมด UI มือถือด้วย เดิม handler นี้
		// ผูกไว้เฉพาะ Platform.Instance.UsePCUI (PC เท่านั้น) ทั้งที่ InputMouse.cs ผูกปุ่มขวาคลิก →
		// InputCommand.MoveToPosition แบบไม่มีเงื่อนไขแพลตฟอร์มอยู่แล้ว (ดู RegisterCommands()) ⇒ ตัด
		// เงื่อนไข UsePCUI ออก ให้สมัคร handler เสมอ ไม่ต้องพึ่ง UI ชุดไหนก็คลิกขวาเพื่อเดินได้
		GameSystem<InputSystem>.Instance().On(InputCommand.MoveToPosition, delegate(InputCommandMessage message)
		{
			ProcessInputPickingMove(message, showMoveCursor: true);
		});
		AkAudioListener akAudioListener = UnityEngine.Object.FindObjectOfType<AkAudioListener>();
		if (akAudioListener != null)
		{
			SoundManager.SetListenerObject(akAudioListener.gameObject);
		}
	}

	private void Start()
	{
		ParticleManager.Cache(_moveCursorParticle);
		GameSystem<InputSystem>.Instance().On(InputCommand.PlayerJump, InputPlayerJumped);
		GameSystem<InputSystem>.Instance().On(InputCommand.Move, InputMoved);
		GameSystem<InputSystem>.Instance().On(InputCommand.TestInstrument, InputTestInstrumented);
		GameSystem<InputSystem>.Instance().On(InputCommand.Draw, InputDrawed);
		GameSystem<InputSystem>.Instance().On(InputCommand.Mount, MountKeyPressed);
		GameSystem<InputSystem>.Instance().MoveLocked += InputMoveLocked;
		GameSystem<InputSystem>.Instance().DrawLineSegmentAdded += InputDrawLineSegmentAdded;
		GameSystem<InputSystem>.Instance().DrawLinePointAdded += InputDrawLinePointAdded;
		Durango.Utils.Singleton<GameManager>.Instance().PreReconnect += GameManager_PreReconnect;
		GameSystem<InventorySystem>.Instance().UseItemStarted += delegate
		{
			StopMove();
		};
		Observable<bool> occluded = _occluded;
		occluded.Changed = (Action<bool>)Delegate.Combine(occluded.Changed, (Action<bool>)delegate
		{
			RefreshPlayerOutline();
		});
	}

	private void Update()
	{
		if (TerrainBase.IsPlayerInitialized || GameManager.IsPrologueMode)
		{
			if (_stopMoveReserved)
			{
				_localMoveNavigator.Stop();
				_stopMoveReserved = false;
				_manualMoveAllowed = false;
			}
			else
			{
				ProcessInputDirectionalMove();
			}
			_localMoveNavigator.UpdateTargetPosition(_localMoveOperator.LastSentPosition);
			_localMoveOperator.ProcessLocalPlayerMovements(_localMoveNavigator.GetMoveParam(), _localMoveNavigator.GetYawParam());
			if (!IgnoreOcclusionCheck && Durango.Utils.Singleton<OccluderVisibleManager>.HasInstance())
			{
				OccluderVisibleManager occluderVisibleManager = Durango.Utils.Singleton<OccluderVisibleManager>.Instance();
				occluderVisibleManager.PushRayCastPosition(Player.GetBodyPartTransform(BodyPart.Head).position);
				bool isOccluded = occluderVisibleManager.IsOccluded;
				_occluded.Value = isOccluded;
			}
			base.transform.position = Player.GetBodyPartTransform(BodyPart.Body).position;
		}
	}

	private void GameManager_PreReconnect()
	{
		_localMotionUpdater = new LocalMotionUpdater();
		_localMoveOperator.SetMotionUpdater(_localMotionUpdater);
		Observable<bool> isBattleStand = _localMotionUpdater.IsBattleStand;
		isBattleStand.Changed = (Action<bool>)Delegate.Combine(isBattleStand.Changed, (Action<bool>)delegate
		{
			RefreshPlayerOutline();
		});
	}

	private void ProcessInputDirectionalMove()
	{
		if (InputMoveStopped())
		{
			_lastMoveDir = null;
			_manualMoveDir = Vector3.zero;
		}
		if (GameSystem<InputSystem>.Instance().MoveLock)
		{
			bool flag = _manualMoveDir.sqrMagnitude > 0f;
			_lastMoveDir = null;
			_manualMoveDir = Vector3.zero;
			if (flag && this.TryMoveWhenLocked != null)
			{
				this.TryMoveWhenLocked();
			}
		}
		Vector3 vector = _manualMoveDir;
		bool flag2 = Math.Abs(vector.sqrMagnitude) > 0f;
		if (flag2 && _ignoreSimilarMoveDir)
		{
			_ignoreSimilarMoveDir = false;
			Vector3? lastMoveDir = _lastMoveDir;
			if (lastMoveDir.HasValue)
			{
				float num = Vector2.Angle(new Vector2(vector.x, vector.z), new Vector2(_lastMoveDir.Value.x, _lastMoveDir.Value.z));
				if (num < 10f)
				{
					vector = Vector3.zero;
					flag2 = false;
					_ignoreSimilarMoveDir = true;
				}
			}
		}
		if (flag2 != _manualDirectionalMove)
		{
			_manualDirectionalMove = flag2;
			if (_manualDirectionalMove)
			{
				if (this.MoveStarted != null)
				{
					this.MoveStarted();
				}
				_manualMoveAllowed = true;
				_manualPickingMove = false;
			}
			else
			{
				if (this.MoveEnded != null)
				{
					this.MoveEnded();
				}
				_localMoveNavigator.Stop();
			}
		}
		if (_manualDirectionalMove && _manualMoveAllowed)
		{
			_localMoveNavigator.MoveInDirection(vector);
			_lastMoveDir = vector;
		}
	}

	private void ProcessInputPickingMove(InputCommandMessage message, bool showMoveCursor)
	{
		if (message.PickingTouchEvent.IsNguiTouched || message.PickingTouchEvent.IsNguiPressed || message.PickingTouchEvent.IsObjectPressed || (GameSystem<InputSystem>.Instance().DrawMode && message.MouseButton == KeyCode.Mouse0) || InputMoveStopped())
		{
			return;
		}
		if (GameSystem<InputSystem>.Instance().MoveLock)
		{
			if (this.TryMoveWhenLocked != null)
			{
				this.TryMoveWhenLocked();
			}
			return;
		}
		if (!_manualPickingMove)
		{
			_manualPickingMove = true;
			if (this.MoveStarted != null)
			{
				this.MoveStarted();
			}
			_manualMoveAllowed = true;
		}
		MoveToPosition(message.PickingRay, delegate
		{
			if (this.MoveEnded != null)
			{
				this.MoveEnded();
			}
			_manualPickingMove = false;
		}, 10f, completeIfBlocked: false, showMoveCursor);
	}

	private bool InputMoveStopped()
	{
		return Player.IsAlive && _manualDirectionalMove && !GameSystem<InputSystem>.Instance().MovedByManual();
	}

	public void StopMove()
	{
		_stopMoveReserved = true;
	}

	private void InputMoved(InputCommandMessage message)
	{
		if (Player.IsAlive)
		{
			_manualMoveDir = message.MoveDirection;
		}
	}

	private void OnInitialized()
	{
		Player.CheckCurrentTile(forceUpdate: true);
	}

	private void RefreshPlayerOutline()
	{
		if ((bool)_localMotionUpdater.IsBattleStand)
		{
			Player.Select(selected: true, Color.white, 1f);
		}
		else if ((bool)_occluded)
		{
			Player.Select(selected: true, Color.white, 1f);
		}
		else
		{
			Player.Select(selected: false);
		}
	}

	public void SetBaseMoveSpeed(SetBaseMoveSpeed speed)
	{
		_baseMoveSpeed = speed;
	}

	public float GetCurrentMoveSpeed()
	{
		Vector3 worldPos = Util.ClientPositionToWorldPosition(Player.CurrentPosition);
		return MoveSpeed * _waterRetardingSpeedRatio * GetTileMoveSpeedRatio(worldPos);
	}

	private float GetTileMoveSpeedRatio(Vector3 worldPos)
	{
		if (Player.IsRiding)
		{
			return 1f;
		}
		TerrainBase.TileMoveType tileMoveType = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileMoveType(worldPos);
		if (tileMoveType == TerrainBase.TileMoveType.Road)
		{
			return _roadMoveSpeedRatio;
		}
		if (!Player.HasAnimTag(PlayerAnimationClipTag.BushResist))
		{
			switch (tileMoveType)
			{
			case TerrainBase.TileMoveType.None:
				return 1f;
			case TerrainBase.TileMoveType.Tiny:
				return _bushMoveSpeedRatioTiny;
			case TerrainBase.TileMoveType.Small:
				return _bushMoveSpeedRatioSmall;
			case TerrainBase.TileMoveType.Medium:
				return _bushMoveSpeedRatioMedium;
			case TerrainBase.TileMoveType.Large:
				return _bushMoveSpeedRatioLarge;
			}
		}
		return 1f;
	}

	public void RotateToObject(GameObject target, bool snap = false)
	{
		_stopMoveReserved = false;
		_localMoveNavigator.RotateToObject(target, snap);
	}

	public void RotateToPosition(Vector3 pos, bool snap = false)
	{
		_stopMoveReserved = false;
		_localMoveNavigator.RotateToPosition(pos, snap);
	}

	public void TurnToYaw(float yaw, bool snap = false)
	{
		_stopMoveReserved = false;
		_localMoveNavigator.SetTargetYaw(yaw, snap);
	}

	private void MoveToPosition(Ray ray, Action onComplete = null, float distance = 10f, bool completeIfBlocked = false, bool showMoveCursor = false)
	{
		MoveToPosition(MainCamera.RayToWorldPos(ray), onComplete, distance, completeIfBlocked, showMoveCursor);
	}

	public void MoveToPosition(Vector3 pos, Action onComplete = null, float distance = 10f, bool completeIfBlocked = false, bool showMoveCursor = false)
	{
		_stopMoveReserved = false;
		_localMoveNavigator.MoveToPosition(pos, onComplete, distance, completeIfBlocked);
		if (showMoveCursor)
		{
			ShowMoveCursor(pos);
		}
	}

	public void MoveToTarget(GameObject targetObj, Action onComplete = null, float distance = 10f, bool completeIfBlocked = false)
	{
		_stopMoveReserved = false;
		_localMoveNavigator.MoveToTarget(targetObj, onComplete, distance, completeIfBlocked);
	}

	public void ShowMoveCursor(Vector3 pos)
	{
		Vector3 one = Vector3.one;
		if (Platform.Instance.UsePCUI)
		{
			one *= 0.5f;
		}
		ParticleManager.Emit(_moveCursorParticle, pos, Quaternion.identity, comeForwardToCamera: false, groundDecal: false, one);
	}

	public void ResurrectionRequest(Point2? warpholeTile = null)
	{
		Connections.Frontend.Send(new Revive
		{
			WarpholeTile = warpholeTile
		});
	}

	public void RequestReviveImmediately(string voucherId)
	{
		Connections.Frontend.Send(new ReviveImmediately
		{
			VoucherId = voucherId
		});
	}

	public void Sleep(bool sleep)
	{
		Player.OutlineEnabled = !sleep;
		MotionUpdater.IsSleep = sleep;
	}

	private void Player_Revived(CharacterBehavior player)
	{
		UIManager.MessageBox.Hide();
		Durango.Utils.Singleton<CustomColorCorrectionEffect>.Instance().NightEffectMin = 0f;
		MotionUpdater.RefreshMotion(null, force: true, clearReservations: true);
		if (_dieParticleId != 0)
		{
			ParticleManager.Stop(_dieParticleId);
			_dieParticleId = 0;
		}
	}

	private void Player_Died(CharacterBehavior player, bool fromInit)
	{
		if (_dieParticleId != 0)
		{
			ParticleManager.Stop(_dieParticleId);
			_dieParticleId = 0;
		}
		Durango.Utils.Singleton<CustomColorCorrectionEffect>.Instance().NightEffectMin = 1f;
		_dieParticleId = ParticleManager.Emit(_dieParticleType, player.CurrentPosition, Quaternion.identity);
		StopMove();
		if (Player.IsRiding)
		{
			Player.Driver.Unmount(delegate
			{
				MotionUpdater.ClearReservedMotions();
				MotionUpdater.Motion("Die", 0f, 1f, forceTransition: true);
			});
		}
		else
		{
			MotionUpdater.ClearReservedMotions();
			MotionUpdater.Motion("Die", 0f, 1f, forceTransition: true);
		}
	}

	private void Player_TileChanged(Point2 prev, Point2 current)
	{
		if (Durango.Utils.Singleton<AmbientSoundManager>.HasInstance())
		{
			Biome biome = Player.GetBiome();
			Durango.Utils.Singleton<AmbientSoundManager>.Instance().SetBiome(biome);
			Durango.Utils.Singleton<AmbientSoundManager>.Instance().SetFlowAudio(Player.CurrentPosition);
		}
	}

	private void Player_MotionConditionChanged()
	{
		MotionUpdater.ConditionChanged();
	}

	public void UpdateLastSentTransform(Vector3 position, float height, float yaw, byte floor)
	{
		_localMoveOperator.UpdateLastSent(position, height, yaw, floor);
	}

	public bool IsMovablePosition(Vector3 clientPos)
	{
		Vector3 vector = Util.ClientPositionToWorldPosition(clientPos);
		return !Durango.Utils.Singleton<TerrainBase>.Instance().IsCollidableMasked(vector) && !TerrainWater.IsTooDeepToSwim(Durango.Utils.Singleton<TerrainBase>.Instance().GetWaterDepth(vector), Player.SwimmableDepthRatio);
	}

	public void Teleport(Vector3 pos, TeleportType type = TeleportType.Unknown, bool instance = false)
	{
		Teleport(pos, 0, type, instance);
	}

	public void Teleport(Vector3 pos, byte floor, TeleportType type = TeleportType.Unknown, bool instance = false)
	{
		if (instance || UIManager.IsLoadingCurtain)
		{
			_localMoveOperator.Teleport(pos, floor);
		}
		else
		{
			TeleportLoadingCurtain teleportLoadingCurtain = UIManager.ShowLoadingCurtain<TeleportLoadingCurtain>();
			UIManager.OnLoadingCurtainHidden(delegate
			{
				switch (type)
				{
				case TeleportType.Returning:
				case TeleportType.Warp:
				case TeleportType.WarpBack:
				case TeleportType.Visit:
					MotionUpdater.ClearReservedMotions();
					MotionUpdater.Motion("Warp_End", 0f, 1f, forceTransition: true);
					break;
				case TeleportType.Revive:
					break;
				}
			}, LoadingCurtainBase.LoadingState.Closing);
			teleportLoadingCurtain.SetReadyToTeleport(delegate
			{
				_localMoveOperator.Teleport(pos, floor);
			});
		}
		if (this.MoveEnded != null)
		{
			this.MoveEnded();
		}
	}

	private void InputPlayerJumped(InputCommandMessage message)
	{
		if (CanTryJump())
		{
			TryJump();
		}
	}

	public bool CanTryJump()
	{
		if (GameManager.IsPrologueMode)
		{
			return false;
		}
		if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			return false;
		}
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		TerrainWater.WaterDepthLevel waterDepthLevel = localPlayer.WaterDepthLevel;
		return localPlayer.IsAlive && waterDepthLevel <= TerrainWater.WaterDepthLevel.Foot && (bool)localPlayer.IsMoving && _manualDirectionalMove && !localPlayer.IsRiding && !localPlayer.IsTired;
	}

	public void TryJump()
	{
		if (_localMotionUpdater.CurrentClipInfo == null || !_localMotionUpdater.CurrentClipInfo.HasAnimTag(PlayerAnimationClipTag.Irrevocable))
		{
			PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
			Gauge stamina = localPlayer.Stamina;
			if (stamina == null || stamina.Get() < (float)Yaml.Util.Singleton<Constants>.Instance.Dash.Stamina)
			{
				UIManager.SystemMsg(T._("스태미나가 부족합니다."));
				return;
			}
			MotionUpdater.Motion("Jump");
			Connections.Frontend.Send(default(Dashed));
		}
	}

	public void IgnoreSimilarMoveDirection()
	{
		StopMove();
		_ignoreSimilarMoveDir = true;
	}

	private void InputTestInstrumented(InputCommandMessage message)
	{
		if (Application.isEditor)
		{
			InstrumentModelController[] array = UnityEngine.Object.FindObjectsOfType<InstrumentModelController>();
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				array[i].Test();
			}
		}
	}

	private void InputMoveLocked()
	{
		StopMove();
	}

	private void InputDrawLineSegmentAdded()
	{
		Player.WorldLineRenderer.AddLineSegment();
	}

	private void InputDrawLinePointAdded(Vector3 clientPosition)
	{
		Player.WorldLineRenderer.AddLinePoint(clientPosition);
	}

	private static void InputDrawed(InputCommandMessage message)
	{
		List<DrawLineBase> drawLineBuffer = message.DrawLineBuffer;
		PlayerDrawLine msg = default(PlayerDrawLine);
		msg.DrawCommands = drawLineBuffer.ToArray();
		msg.PlayerId = GameManager.PlayerId;
		Connections.Frontend.Send(msg);
	}

	private void MountKeyPressed(InputCommandMessage message)
	{
		if (!(PlayerBehavior.LocalPlayer.Driver.Vehicle == null))
		{
			GameSystem<InteractionSystem>.Instance().DoNoneTargetAction((!PlayerBehavior.LocalPlayer.IsRiding) ? new InteractionMenuData(Interaction.Mount) : new InteractionMenuData(Interaction.Dismount));
		}
	}

	public void PrepareCPR(CharacterBehavior target)
	{
		Vector3 sidePos = target.GetSidePos(left: true);
		if (IsMovablePosition(sidePos))
		{
			Teleport(sidePos, TeleportType.Unknown, instance: true);
		}
		Vector3 interactionPosition = InteractionObject.GetInteractionPosition(target.gameObject);
		float yaw = Maths.CalcYawWithTarget(interactionPosition, Player.CurrentPosition);
		TurnToYaw(yaw, snap: true);
		MotionUpdater.Motion("Barehand_CPR");
	}

	public void SnapToTarget(Transform target)
	{
		int num = (int)Maths.CalcYaw(target.forward);
		TurnToYaw(num, snap: true);
		byte value = Player.Floor.Value;
		Teleport(target.position, value, TeleportType.Unknown, instance: true);
	}
}
