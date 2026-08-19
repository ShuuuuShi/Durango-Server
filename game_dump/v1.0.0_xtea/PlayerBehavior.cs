using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using L10N;
using Messages;
using MusicData;
using NetworkEnums;
using Player;
using Shared.Ability;
using Shared.Battle;
using TerrainData;
using UnityEngine;

public class PlayerBehavior : CharacterBehavior, IAnimationEventPlayable, ICostumable, IMeshCloner
{
	public enum RunStateEnum
	{
		None,
		Run,
		Sprint,
		Aim,
		BattleRun,
		AimLoaded
	}

	public enum StandStateEnum
	{
		None,
		Stand,
		BattleStand,
		Hiding,
		HoldMode,
		Cold,
		Hot
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum WeaponFramework
	{
		BAREHAND,
		ONEHAND,
		TWOHAND,
		BOW,
		SLING,
		CROSSBOW,
		SHIELD,
		LANCE,
		CHAINSAW,
		SCARED,
		PROLOGUE_WEAPON,
		NONE
	}

	private class MotionEquipmentInfo
	{
		private enum State
		{
			None,
			Reserved,
			Equipped
		}

		public string EquipItemPath { get; private set; }

		public ItemColor Color { get; private set; }

		private State CurState { get; set; }

		public void Reserve(string equipPath, ItemColor color)
		{
			if (equipPath != null)
			{
				EquipItemPath = equipPath;
				Color = color;
				CurState = State.Reserved;
			}
		}

		public void Equipped()
		{
			CurState = State.Equipped;
		}

		public void Reset()
		{
			EquipItemPath = null;
			CurState = State.None;
		}

		public bool IsEquipped()
		{
			return CurState == State.Equipped;
		}

		public bool IsReserved()
		{
			return CurState == State.Reserved;
		}
	}

	private struct PlayClipArgument
	{
		public bool IsValid;

		public string PlayAnimationClipName;

		public PlayerAnimationBlendTree BlendTree;

		public PlayerAnimationClipInfo ClipInfo;

		public float PlaybackRate;

		public void Set(string clipName)
		{
			Reset();
			IsValid = true;
			PlayAnimationClipName = clipName;
		}

		public void Set(PlayerAnimationBlendTree blendTree)
		{
			Reset();
			IsValid = true;
			BlendTree = blendTree;
		}

		public void Reset()
		{
			IsValid = false;
			PlayAnimationClipName = null;
			BlendTree = null;
			ClipInfo = null;
			PlaybackRate = 1f;
		}

		public override string ToString()
		{
			if (IsValid)
			{
				if (BlendTree != null)
				{
					return BlendTree.Name;
				}
				return PlayAnimationClipName;
			}
			return "Not Valid";
		}
	}

	public delegate void AnimationClipInfo2Delegate(PlayerAnimationClipInfo prev, PlayerAnimationClipInfo next);

	public const float MinBodySizeRatio = 0.85f;

	public const float MaxBodySizeRatio = 1.1f;

	private static PlayerBehavior _localPlayer;

	private static string[] _deathMsgAttackPlayerList = new string[5]
	{
		T.N_("에게 급소를 가격당해"),
		T.N_("의 펀치에 맞아"),
		T.N_("의 칼날에 베어"),
		T.N_("의 이빨에 물려"),
		T.N_("의 몸통 박치기에 당해")
	};

	private BoneMergeable _boneMergeable;

	private ProjectileController _projectileController;

	[SerializeField]
	private bool _isMale;

	[SerializeField]
	private float _rotateSpeed;

	[SerializeField]
	private Transform _aimBasis;

	[SerializeField]
	private Transform _headTransform;

	[SerializeField]
	private Transform _bodyTransform;

	[SerializeField]
	private Transform _leftArmTransform;

	[SerializeField]
	private Transform _leftLegTransform;

	[SerializeField]
	private Transform _spineTransform;

	[SerializeField]
	private Transform _bip001Transform;

	private AmbientLighting _ambientLighting;

	private bool _isCombatMode;

	private string _fatigueEffect;

	private GameObject _target;

	private Transform _meshObjectTransform;

	private bool _motionStarted;

	private bool _shouldCallOnChangePlayerPosition = true;

	private PlayerController _controller;

	private Player.PlayerVoice _voice;

	private MusicController _musicController;

	private float _motionInterruptTimer;

	private float _motionFadeTimer;

	private float _motionTransitionTime;

	private string _currentAnimClipName;

	private PlayerAnimationBlendTree _currentAnimClipBlendTree;

	private readonly List<KeyValuePair<AnimationState, float>> _fadeoutStates = new List<KeyValuePair<AnimationState, float>>();

	private string _currentEquipmentsPath = string.Empty;

	private ItemColor _currentEquipmentsColors;

	private readonly MotionEquipmentInfo _motionEquipment = new MotionEquipmentInfo();

	private float _bodySize = 1f;

	private WaterRipple _rippleOnRiver;

	private WaterRipple _rippleOnOcean;

	private StandStateEnum _baseStandState;

	private StandStateEnum _standState;

	private RunStateEnum _runState;

	private SkinnedMeshRenderer[] _renderes;

	private Transform _rootBone;

	private WeaponFramework _currentWeaponFramework = WeaponFramework.NONE;

	private TerrainWater.WaterDepthLevel _waterDepthLevel;

	private bool _isWaterCarried;

	private bool _isMoving;

	private readonly List<DrawLineBase> _drawLineBuffer = new List<DrawLineBase>();

	private GameObject _equipmentObj;

	private Animation _equipmentAnim;

	private WorldLineRenderer _worldLineRenderer;

	private int _portraitBg;

	private Color _portriatBgColor;

	private CharacterCostume _costume;

	private bool _prevHeadVisible;

	private readonly Dictionary<KeyValuePair<string, string>, GameObject> _effects = new Dictionary<KeyValuePair<string, string>, GameObject>();

	private BoneFlinchingController _boneFlinchingController;

	private bool _selected;

	private bool _occluded;

	private bool _isOutlineEnabled = true;

	private Outline _outline;

	private bool _isPlaneShadowEnabled = true;

	private PlaneShadows _planeShadows;

	private Renderer _mainRenderer;

	private bool _isRendererEnabled = true;

	private Transform _weaponTipTransform;

	private PathMovable _pathMovable;

	private int _needAnimationRefresh;

	private AnimationClipInfo _curInfo;

	private int _lastAnimClipInfoCheckFrame;

	private Vector3 _prevRootBoneLocalPos;

	private float _prevMoveTime = -1f;

	private Vector3 _prevPosition = Vector3.zero;

	private float _prevLife;

	private ChatablePlayer _chatable;

	private PlayerAnimationClipManager _animationManager;

	private PlayClipArgument _playClipArgument;

	private PlayerAnimationClipInfo _prevAnimClipInfo;

	private PlayerAnimationClipInfo _nextAnimClipInfo;

	private string _reserveAnim;

	private float _reserveAnimTime;

	[SerializeField]
	private bool _useAnimBlendTreeInterp;

	private readonly List<KeyValuePair<GameObject, float>> _particleEffectList = new List<KeyValuePair<GameObject, float>>();

	private Driver _driver;

	public override BoneMergeable BoneMergeable
	{
		get
		{
			if (_boneMergeable == null)
			{
				_boneMergeable = new BoneMergeable(((Component)this).gameObject, this, MeshObjectTransform, _rootBone);
			}
			return _boneMergeable;
		}
	}

	public ProjectileController ProjectileController
	{
		get
		{
			if (_projectileController == null)
			{
				_projectileController = new ProjectileController(_aimBasis);
				_projectileController.ProjectileDetonated += delegate
				{
					IsAttackFramePassed = true;
				};
			}
			return _projectileController;
		}
	}

	public float RotateSpeed
	{
		get
		{
			return (!IsRiding) ? _rotateSpeed : Driver.RotateSpeed;
		}
		set
		{
			_rotateSpeed = value;
		}
	}

	public override Transform Bip001Transform => _bip001Transform;

	public bool IgnoreMotionState { get; set; }

	public override bool IsPlayer => true;

	public static PlayerBehavior LocalPlayer
	{
		get
		{
			return _localPlayer;
		}
		set
		{
			if (!((Object)(object)_localPlayer == (Object)(object)value) && !((Object)(object)value == (Object)null))
			{
				if ((Object)(object)_localPlayer != (Object)null)
				{
					value.TransferEvent(_localPlayer);
				}
				_localPlayer = value;
				_localPlayer._controller = KSingleton<PlayerController>.Instance();
			}
		}
	}

	public string PlayerName { get; set; }

	public ulong ClanId => Clan.ClanId;

	public Member Clan { get; set; }

	public Title Title { get; set; }

	public TileObject CurrentTileObject { get; private set; }

	public bool IsCombatMode
	{
		get
		{
			return _isCombatMode;
		}
		private set
		{
			if (_isCombatMode != value)
			{
				_isCombatMode = value;
				RestoreStandState();
			}
		}
	}

	public string FatigueEffect
	{
		get
		{
			return _fatigueEffect;
		}
		set
		{
			if (_fatigueEffect != value)
			{
				_fatigueEffect = value;
				RestoreStandState();
			}
		}
	}

	public ulong TargetEntityId { get; private set; }

	public CharacterBehavior CharacterTarget { get; private set; }

	public GameObject Target
	{
		get
		{
			return _target;
		}
		set
		{
			if ((Object)(object)CharacterTarget != (Object)null)
			{
				CharacterTarget.IsAimTarget = false;
			}
			CharacterTarget = ((!((Object)(object)value != (Object)null)) ? null : value.GetComponent<CharacterBehavior>());
			if ((Object)(object)CharacterTarget == (Object)null)
			{
				TargetEntityId = 0uL;
			}
			else
			{
				CharacterTarget.IsAimTarget = true;
				TargetEntityId = CharacterTarget.EntityId;
			}
			GameObject target = _target;
			_target = value;
			if ((Object)(object)target != (Object)(object)_target && this.TargetChanged != null)
			{
				this.TargetChanged(target, _target);
			}
		}
	}

	public BoneLookAtTarget LookAtController { get; private set; }

	public Transform MainTransform => ((Component)this).transform;

	public override Transform MeshObjectTransform => _meshObjectTransform;

	public bool IsLoaded => _costume.IsCostumeLoaded && _motionStarted;

	public override Vector3 CurrentPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return MainTransform.localPosition;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			bool flag = MainTransform.localPosition != value;
			MainTransform.localPosition = value;
			if (flag)
			{
				CheckCurrentTile();
				if (this.Moved != null)
				{
					this.Moved();
				}
			}
			if (IsLocalPlayer && (flag || _shouldCallOnChangePlayerPosition))
			{
				_shouldCallOnChangePlayerPosition = false;
			}
		}
	}

	public Vector3 CameraOrigin
	{
		get
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			float num = 100f;
			if (IsRiding)
			{
				num = Driver.CameraHeight;
			}
			return CurrentPosition + new Vector3(0f, num, 0f);
		}
	}

	public override Vector3 InteractionPosition => CameraOrigin;

	public Driver Driver
	{
		get
		{
			if ((Object)(object)_driver == (Object)null)
			{
				_driver = ((Component)this).GetComponent<Driver>();
			}
			return _driver;
		}
	}

	public bool IsRiding => Driver.IsRiding;

	public bool IsLocalPlayer => (Object)(object)_controller != (Object)null;

	public Player.PlayerVoice Voice
	{
		get
		{
			if (_voice == null)
			{
				_voice = new Player.PlayerVoice(this);
			}
			return _voice;
		}
	}

	public bool IsProhibitAnimRefresh { get; set; }

	public string CurrentAnimKeyName { get; private set; }

	public string CurrentAnimClipName
	{
		get
		{
			return _currentAnimClipName;
		}
		set
		{
			_currentAnimClipName = value;
			if (string.IsNullOrEmpty(value))
			{
				CurrentAnimKeyName = value;
			}
			else if (value.StartsWith("M_") || value.StartsWith("F_"))
			{
				CurrentAnimKeyName = value.Substring(2, value.Length - 2);
			}
			else
			{
				CurrentAnimKeyName = value;
			}
		}
	}

	public PlayerDisplay Display { get; set; }

	public float BodySize => _bodySize;

	public string DefaultBodyCostume => Display.DefaultBody;

	public string DefaultInnerCostume => Display.DefaultInner;

	public string CurrentBodyCostume => (!string.IsNullOrEmpty(Display.Body)) ? Display.Body : DefaultBodyCostume;

	public StandStateEnum BaseStandState
	{
		get
		{
			return _baseStandState;
		}
		set
		{
			_baseStandState = value;
			if (IsCurrentAnimState("Stand"))
			{
				SetAnimationState("Stand");
			}
		}
	}

	private StandStateEnum StandState
	{
		get
		{
			if (IsCombatMode || BaseStandState == StandStateEnum.None)
			{
				return _standState;
			}
			return BaseStandState;
		}
		set
		{
			_standState = value;
			if (IsCurrentAnimState("Stand"))
			{
				SetAnimationState("Stand");
			}
		}
	}

	public RunStateEnum RunState
	{
		get
		{
			return _runState;
		}
		set
		{
			_runState = value;
			if (IsCurrentAnimState("Run"))
			{
				SetAnimationState("Run");
			}
		}
	}

	public Animation Anim { get; private set; }

	public WeaponFramework CurrentWeaponFramework => _currentWeaponFramework;

	public override TerrainWater.WaterDepthLevel WaterDepthLevel
	{
		get
		{
			return _waterDepthLevel;
		}
		set
		{
			if (_waterDepthLevel != value)
			{
				_waterDepthLevel = value;
				if (!IsLocalPlayer || !_controller.IsInServerSideBattle)
				{
					UpdateMovingMotion();
				}
				if (this.WaterDepthLevelChanged != null)
				{
					this.WaterDepthLevelChanged();
				}
			}
		}
	}

	public bool IsInWater
	{
		get
		{
			if (IsProhibitingAdaptiveMotionChanges)
			{
				return false;
			}
			return WaterDepthLevel >= TerrainWater.WaterDepthLevel.Waist;
		}
	}

	public bool IsSwimming
	{
		get
		{
			if (IsProhibitingAdaptiveMotionChanges)
			{
				return false;
			}
			return WaterDepthLevel >= TerrainWater.WaterDepthLevel.Swim;
		}
	}

	public bool IsProhibitingAdaptiveMotionChanges
	{
		get
		{
			if (IsServerSideMoveControl)
			{
				return true;
			}
			if (IsRiding)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsRest { get; private set; }

	public bool IsSleep { get; private set; }

	public bool IsNovice => GameManager.IsPrologueMode;

	public bool IsWaterCarried
	{
		get
		{
			return _isWaterCarried;
		}
		set
		{
			if (_isWaterCarried != value)
			{
				_isWaterCarried = value;
				if (IsLocalPlayer && _controller.IsInServerSideBattle)
				{
					_isWaterCarried = false;
				}
				else if (IsMoving)
				{
					PlayAnimation("Run");
				}
				else if (!IsCurrentPlayAnimTag(PlayerAnimationClipTag.WaterFlowResist))
				{
					PlayAnimation("Stand");
				}
			}
		}
	}

	public bool IsTired
	{
		get
		{
			if (GameManager.IsPrologueMode)
			{
				return false;
			}
			Gauge gauge = SurvivalGauges.GetGauge("fatigue");
			if (gauge == null)
			{
				return false;
			}
			Dictionary<Derived, int> derivedAbilities = GameSystem<StatisticsSystem>.Instance().DerivedAbilities;
			if (derivedAbilities == null)
			{
				return false;
			}
			derivedAbilities.TryGetValue(Derived.FatigueDanger, out var value);
			return gauge.Get() > (float)value;
		}
	}

	public float SwimmableDepthRatio
	{
		get
		{
			if (GameManager.IsPrologueMode)
			{
				return 0f;
			}
			Dictionary<Derived, int> derivedAbilities = GameSystem<StatisticsSystem>.Instance().DerivedAbilities;
			if (derivedAbilities == null)
			{
				return 0f;
			}
			derivedAbilities.TryGetValue(Derived.Swimming, out var value);
			return (float)value / 100f;
		}
	}

	public bool IsSwimmable => SwimmableDepthRatio > 0f;

	public bool IsReceivingCPR { get; set; }

	public bool IsInside { get; private set; }

	public override bool IsMoving
	{
		get
		{
			return _isMoving;
		}
		set
		{
			if (_isMoving != value)
			{
				_isMoving = value;
				UpdateMovingMotion();
				if (!value)
				{
					UpdateVelocity();
				}
				if (this.ChangeMoveState != null)
				{
					this.ChangeMoveState(value);
				}
			}
		}
	}

	public float DirAngle { get; set; }

	public Vector3 MoveDir { get; set; }

	public float TargetYaw { get; set; }

	public WorldLineRenderer WorldLineRenderer
	{
		get
		{
			if ((Object)(object)_worldLineRenderer == (Object)null)
			{
				_worldLineRenderer = ((Component)this).gameObject.GetComponent<WorldLineRenderer>();
				if ((Object)(object)_worldLineRenderer == (Object)null)
				{
					_worldLineRenderer = ((Component)this).gameObject.AddComponent<WorldLineRenderer>();
				}
			}
			return _worldLineRenderer;
		}
	}

	public ItemColor[] CostumeColors => _costume.CostumeColors;

	public bool IsPreview { get; set; }

	public bool IgnoreOcclusionCheck { get; set; }

	public bool IsOutlineEnabled
	{
		get
		{
			return _isOutlineEnabled;
		}
		set
		{
			_isOutlineEnabled = value;
			UpdateOutline();
		}
	}

	private Outline OutlineComponent
	{
		get
		{
			if ((Object)(object)_outline == (Object)null)
			{
				_outline = ((Component)this).GetComponent<Outline>();
			}
			return _outline;
		}
	}

	public bool IsPlaneShadowEnabled
	{
		get
		{
			return _isPlaneShadowEnabled;
		}
		set
		{
			_isPlaneShadowEnabled = value;
			UpdatePlaneShadow();
		}
	}

	private PlaneShadows PlaneShadowsComponent
	{
		get
		{
			if ((Object)(object)_planeShadows == (Object)null)
			{
				_planeShadows = ((Component)this).GetComponent<PlaneShadows>();
			}
			return _planeShadows;
		}
	}

	public override bool IsVisible
	{
		get
		{
			if ((Object)(object)_mainRenderer == (Object)null)
			{
				_mainRenderer = ((Component)MeshObjectTransform.FindChild("Body")).gameObject.GetComponent<Renderer>();
			}
			return (Object)(object)_mainRenderer == (Object)null || _mainRenderer.isVisible;
		}
	}

	public override Transform WeaponTipTransform => _weaponTipTransform;

	public float AttackFrameExpireTime
	{
		get
		{
			if (ProjectileController.ProjectileWeaponEquipped)
			{
				return 5f;
			}
			return 1f;
		}
	}

	public PathMovable PathMovable
	{
		get
		{
			if (_pathMovable != null)
			{
				return _pathMovable;
			}
			_pathMovable = new PathMovable(this);
			_pathMovable.MovementProcessed += MovementProcessed;
			return _pathMovable;
		}
	}

	private bool IsServerSideMoveControl => !IsLocalPlayer || _controller.IsInServerSideBattle;

	public Vector3 LastRootMotionDelta { get; private set; }

	private bool CurrentlyDoingServerMotion { get; set; }

	public bool YawLock { get; set; }

	public override bool IsAlive
	{
		get
		{
			if (base.Life == null)
			{
				return true;
			}
			return base.Life.Get() > 0f;
		}
	}

	public override bool IsAnimPlaying => Anim.isPlaying;

	public override bool IsAimTarget
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public int PortraitType { get; private set; }

	public override ChatableBase ChatableBase
	{
		get
		{
			if ((Object)(object)_chatable == (Object)null)
			{
				_chatable = ((Component)this).GetComponent<ChatablePlayer>();
			}
			if ((Object)(object)_chatable == (Object)null)
			{
				_chatable = ((Component)this).gameObject.AddComponent<ChatablePlayer>();
			}
			return _chatable;
		}
	}

	public PlayerAnimationClipManager AnimManager
	{
		get
		{
			if ((Object)(object)_animationManager == (Object)null)
			{
				_animationManager = KSingleton<PlayerAnimationClipManager>.Instance();
			}
			return _animationManager;
		}
	}

	public PlayerAnimationClipInfo CurrentAnimClipInfo { get; set; }

	private PlayerAnimationClipInfo CurrentPlayAnimClipInfo
	{
		get
		{
			if (_nextAnimClipInfo != null)
			{
				return _nextAnimClipInfo;
			}
			return CurrentAnimClipInfo;
		}
	}

	private PlayerAnimationStateInfo CurrentAnimState => PlayerAnimationClipManager.GetClipState(CurrentPlayAnimClipInfo);

	private bool IsServerControlledMotionTagLevel
	{
		get
		{
			if (GameManager.IsPrologueMode && IsCombatMode)
			{
				return true;
			}
			return _controller.IsInServerSideBattle;
		}
	}

	public bool IsAttackFramePassed { get; set; }

	public bool IsMale => _isMale;

	public CharacterCostume.SkinDirty SkinDirtyLevel
	{
		get
		{
			return _costume.GetSkinDirtyLevel();
		}
		set
		{
			_costume.SetSkinDirtyLevel(value);
		}
	}

	public event Action Started;

	public event Action<CharacterBehavior, Damage> DamageTaken;

	public event Action<PlayerBehavior> Died;

	public event Action<PlayerBehavior> Respawned;

	public event Action WaterDepthLevelChanged;

	public event Action<bool> ChangeMoveState;

	public event Action Moved;

	public event AnimationClipInfo2Delegate AnimStateChanged;

	public event Action<CharacterBehavior, BodyPart, string> AttackSucceeded;

	public event Action<GameObject, GameObject> TargetChanged;

	public AnimationClipInfo GetCurrentAnimationClipInfo()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Invalid comparison between Unknown and I4
		if (_lastAnimClipInfoCheckFrame != Time.frameCount)
		{
			_lastAnimClipInfoCheckFrame = Time.frameCount;
			AnimationState val = Anim[CurrentAnimClipName];
			if ((TrackedReference)(object)val == (TrackedReference)null || !val.enabled)
			{
				return AnimationEventController.InvalidAnimationClipInfo;
			}
			_curInfo.Name = CurrentAnimKeyName;
			_curInfo.AnimTime = Mathf.Repeat(val.time, val.length);
			_curInfo.Length = val.length;
			_curInfo.IsLoop = (val.wrapMode & 2) > 0;
			_curInfo.PlaybackRate = val.speed;
			_curInfo.Clip = val.clip;
		}
		return _curInfo;
	}

	public GameObject GetGameObject()
	{
		return ((Component)this).gameObject;
	}

	public Vector3 GetCurrentPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return CurrentPosition;
	}

	public void SetCostumeVisible(CharacterCostume.CostumeType type, bool isVisible)
	{
		_costume.SetCostumeVisible(type, isVisible);
	}

	public void ChangeCostume(string fileName)
	{
		_costume.ChangeCostume(fileName);
	}

	public void ChangeCostumeColor(CharacterCostume.CostumeType type, ItemColor color)
	{
		_costume.ChangeCostumeColor(type, color);
		if (type == CharacterCostume.CostumeType.Equipment)
		{
			_currentEquipmentsColors = color;
		}
	}

	public void ChangeEquipment(string path)
	{
		_currentEquipmentsPath = path;
		if (!_motionEquipment.IsReserved() && !_motionEquipment.IsEquipped())
		{
			UpdateEquipmentModel(path);
			_motionEquipment.Reset();
		}
	}

	public void OnModelChanged()
	{
		if (Application.isPlaying)
		{
			OutlineComponent.RefreshModel();
			PlaneShadowsComponent.RefreshModel();
			_ambientLighting.UpdateMaterials(_renderes);
		}
	}

	public void AddMeshCloners(SkinnedMeshRenderer[] renderers)
	{
		OutlineComponent.Add(renderers);
		PlaneShadowsComponent.Add(renderers);
	}

	public void RemoveMeshCloners(SkinnedMeshRenderer[] renderers)
	{
		OutlineComponent.Remove(renderers);
		PlaneShadowsComponent.Remove(renderers);
	}

	private void Awake()
	{
		_costume = new CharacterCostume(_isMale);
		Anim = ((Component)this).GetComponentInChildren<Animation>();
		_meshObjectTransform = ((Component)this).transform.GetChild(0);
		_rootBone = _meshObjectTransform.FindChild("Bip001");
		_boneFlinchingController = ((Component)this).gameObject.GetComponent<BoneFlinchingController>();
		LookAtController = ((Component)this).gameObject.GetComponent<BoneLookAtTarget>();
		_ambientLighting = ((Component)this).gameObject.GetComponent<AmbientLighting>();
		AnimationEventController component = ((Component)this).gameObject.GetComponent<AnimationEventController>();
		if ((Object)(object)component != (Object)null)
		{
			component.AnimEventMotionChanged += AnimEventMotionChanged;
		}
		((Component)this).gameObject.AddComponent<PlayerFootStepEffect>();
		_costume.Init(((Component)_meshObjectTransform).gameObject);
		_costume.ModelChanged += OnModelChanged;
		_rippleOnRiver = new WaterRipple("Particle/FX_WaterRipple_Moving_01.prefab", isRiver: true);
		_rippleOnOcean = new WaterRipple("Particle/FX_WaterRipple_standing_01.prefab");
		RidingStabilizer component2 = ((Component)this).gameObject.GetComponent<RidingStabilizer>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			((Behaviour)component2).enabled = false;
		}
		_renderes = ((Component)this).GetComponentsInChildren<SkinnedMeshRenderer>();
	}

	private void OnDisable()
	{
		_rippleOnRiver.Stop();
		_rippleOnOcean.Stop();
	}

	private void Start()
	{
		if (!IsPreview)
		{
			SoundManager.Cache("Sound/Effect/metal_picking.wav");
			SoundManager.Cache("Sound/Effect/tree_axe_hit.wav");
			_ambientLighting.UpdateMaterials(_renderes);
			AddMeshCloners(_renderes);
			if (this.Started != null)
			{
				this.Started();
			}
		}
	}

	private void Update()
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		if (IsPreview)
		{
			return;
		}
		if (!IsLocalPlayer)
		{
			ProcessDrawLines();
		}
		PathMovable.ProcessMovementQueue();
		if (IsServerSideMoveControl)
		{
			PathMovable.ProcessMovements();
		}
		ProcessRotate();
		ProcessMove();
		ProjectileController.UpdateProjectiles();
		CheckMotionState();
		UpdateParticleObject();
		ProcessMotionStateAffectedByObject();
		Gauge gauge = SurvivalGauges.GetGauge("wetness");
		if (gauge != null)
		{
			_ambientLighting.Wetness = gauge.Get() / 100f;
		}
		if (IsLocalPlayer && !IgnoreOcclusionCheck)
		{
			OccluderVisibleManager occluderVisibleManager = KSingleton<OccluderVisibleManager>.Instance();
			occluderVisibleManager.PushRayCastPosition(_headTransform.position);
			bool isOccluded = occluderVisibleManager.IsOccluded;
			if (_occluded != isOccluded)
			{
				_occluded = isOccluded;
				UpdateOutline();
			}
		}
	}

	private void LateUpdate()
	{
		if (IsVisible)
		{
			if (IsAlive && _needAnimationRefresh > 0)
			{
				LateAnimationRefresh(_needAnimationRefresh == 2);
				_needAnimationRefresh = 0;
			}
			LateMotionUpdate();
			UpdateScaleBody();
			_boneFlinchingController.ForceUpdateFirst();
			BoneMergeable.UpdateBoneMergeSet();
			if (IsPreview)
			{
				return;
			}
			ProcessDepth();
			ProcessWaterRipple();
			ProcessRootMotionMovements();
		}
		CheckAlive();
	}

	private void TransferEvent(PlayerBehavior oldPlayer)
	{
		TransferEvent((CharacterBehavior)oldPlayer);
		this.Started = (Action)Delegate.Combine(this.Started, oldPlayer.Started);
		this.DamageTaken = (Action<CharacterBehavior, Damage>)Delegate.Combine(this.DamageTaken, oldPlayer.DamageTaken);
		this.Died = (Action<PlayerBehavior>)Delegate.Combine(this.Died, oldPlayer.Died);
		this.Respawned = (Action<PlayerBehavior>)Delegate.Combine(this.Respawned, oldPlayer.Respawned);
		this.AnimStateChanged = (AnimationClipInfo2Delegate)Delegate.Combine(this.AnimStateChanged, oldPlayer.AnimStateChanged);
		this.AttackSucceeded = (Action<CharacterBehavior, BodyPart, string>)Delegate.Combine(this.AttackSucceeded, oldPlayer.AttackSucceeded);
		this.WaterDepthLevelChanged = (Action)Delegate.Combine(this.WaterDepthLevelChanged, oldPlayer.WaterDepthLevelChanged);
		this.TargetChanged = (Action<GameObject, GameObject>)Delegate.Combine(this.TargetChanged, oldPlayer.TargetChanged);
		this.Moved = (Action)Delegate.Combine(this.Moved, oldPlayer.Moved);
		oldPlayer.Started = null;
		oldPlayer.DamageTaken = null;
		oldPlayer.Died = null;
		oldPlayer.Respawned = null;
		oldPlayer.AnimStateChanged = null;
		oldPlayer.AttackSucceeded = null;
		oldPlayer.WaterDepthLevelChanged = null;
		oldPlayer.TargetChanged = null;
		oldPlayer.Moved = null;
	}

	protected override void ProcessDepth()
	{
		if (!GameManager.IsPrologueMode)
		{
			base.ProcessDepth();
		}
	}

	private void ProcessWaterRipple()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Biome biome = GetBiome();
		if (IsMoving || IsRiding)
		{
			_rippleOnRiver.Stop();
			_rippleOnOcean.Stop();
		}
		else
		{
			_rippleOnRiver.Process(biome, WaterDepthLevel, CurrentPosition);
			_rippleOnOcean.Process(biome, WaterDepthLevel, CurrentPosition);
		}
	}

	public void UpdateMovingMotion()
	{
		if (IsRiding)
		{
			Driver.UpdateMovingMotion(IsMoving, updatePlayerMotion: true);
		}
		else if (IsMoving)
		{
			PlayAnimation("Run");
		}
		else if (!IsCurrentAnimState("Stand"))
		{
			PlayAnimation("Stand");
		}
	}

	public void ChangePortraitType(int type, int bg, Color bgColor)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		PortraitBuilder.FillEmptyBackground(base.EntityId, ref bg, ref bgColor);
		PortraitType = type;
		_portraitBg = bg;
		_portriatBgColor = bgColor;
	}

	public float ChangeBodySize(float bodySize)
	{
		_bodySize = Mathf.Clamp(bodySize, 0.85f, 1.1f);
		return _bodySize;
	}

	public PortraitBuilder.Argument GetPortraitArgument()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		return PortraitBuilder.MakeArgument(PortraitType, _portraitBg, _portriatBgColor, IsMale, PortraitEmotion.Normal, _costume.CostumeColors[2][0], _costume.CostumeColors[3][0], _costume.CostumeColors[5][0], _costume.CostumeColors[6][0]);
	}

	private void ChangeBodyCleanness()
	{
		Gauge gauge = SurvivalGauges.GetGauge("cleanness");
		if (gauge != null)
		{
			CharacterCostume.SkinDirty skinDirtyLevel = CharacterCostume.SkinDirty.VeryDirty;
			float num = gauge.Get();
			if (num > 80f)
			{
				skinDirtyLevel = CharacterCostume.SkinDirty.Clean;
			}
			else if (num > 50f)
			{
				skinDirtyLevel = CharacterCostume.SkinDirty.Dirty;
			}
			SkinDirtyLevel = skinDirtyLevel;
		}
	}

	public void SetEffects([NotNull] IList<KeyValuePair<string, string>> effects)
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		IList<KeyValuePair<string, string>> list = null;
		foreach (KeyValuePair<KeyValuePair<string, string>, GameObject> effect in _effects)
		{
			int num = effects.IndexOf(effect.Key);
			if (num == -1)
			{
				ParticleManager.Stop(effect.Value);
				if (list == null)
				{
					list = new List<KeyValuePair<string, string>>();
				}
				list.Add(effect.Key);
			}
		}
		int i = 0;
		for (int count = effects.Count; i < count; i++)
		{
			KeyValuePair<string, string> key = effects[i];
			if (!_effects.ContainsKey(key))
			{
				Transform followingParent = ((!string.IsNullOrEmpty(key.Value)) ? KUtility.FindTransformByName(((Component)MainTransform).gameObject, key.Value) : MainTransform);
				GameObject val = ParticleManager.EmitSync(key.Key, Vector3.zero, Quaternion.identity, followingParent);
				if ((Object)(object)val != (Object)null)
				{
					_effects.Add(key, val);
				}
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				_effects.Remove(list[j]);
			}
		}
	}

	private void UpdatePlaneShadow()
	{
		PlaneShadowsComponent.Show = _isPlaneShadowEnabled && _isRendererEnabled;
	}

	private void UpdateOutline()
	{
		bool flag = IsOutlineEnabled && (_selected || _occluded);
		flag &= _isRendererEnabled;
		OutlineComponent.Fade(flag);
	}

	public void SetRendererEnabled(bool enable)
	{
		_isRendererEnabled = enable;
		SkinnedMeshRenderer[] componentsInChildren = ((Component)MeshObjectTransform).GetComponentsInChildren<SkinnedMeshRenderer>(true);
		int size = KUtility.GetSize(componentsInChildren);
		for (int i = 0; i < size; i++)
		{
			((Renderer)componentsInChildren[i]).enabled = enable;
		}
		UpdatePlaneShadow();
		UpdateOutline();
	}

	public bool GetRenderEnabled()
	{
		return _isRendererEnabled;
	}

	public void ChangeWeaponType(WeaponFramework wt)
	{
		bool flag = _currentWeaponFramework != wt;
		_currentWeaponFramework = wt;
		if (flag && !_motionEquipment.IsEquipped() && (CurrentAnimClipInfo == null || IsCurrentAnimState("Stand") || IsCurrentAnimState("Run")))
		{
			AnimationRefresh();
		}
	}

	private void AnimationEquipEvent(string path, ItemColor color)
	{
		UpdateEquipmentModel(path, color);
		_motionEquipment.Equipped();
	}

	public void ReEquipCurrentWeapon()
	{
		_motionEquipment.Reset();
		UpdateEquipmentModel(_currentEquipmentsPath, _currentEquipmentsColors);
	}

	private void UpdateEquipmentModel(string path, [Optional] ItemColor color)
	{
		if (string.IsNullOrEmpty(path))
		{
			DetachEquipmentModel();
			_weaponTipTransform = MeshObjectTransform;
			return;
		}
		if (color.HasValue)
		{
			CostumeColors[7] = color;
		}
		if (!CheckPrevEquipment(path, _equipmentObj))
		{
			KSingleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(GameObject), OnLoadEquipmentAsset);
		}
	}

	private static bool CheckPrevEquipment(string path, GameObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return false;
		}
		string name = ((Object)obj).name;
		name = name.Substring(0, name.Length - 7);
		if (!path.Contains(name))
		{
			return false;
		}
		return true;
	}

	private void OnLoadEquipmentAsset(Object asset)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		if (!((Object)(object)this == (Object)null))
		{
			GameObject val = (GameObject)asset;
			if ((Object)(object)val == (Object)null)
			{
				UpdateEquipmentModel(string.Empty);
				return;
			}
			DetachEquipmentModel();
			AttachEquipmentModel((GameObject)Object.Instantiate(asset));
		}
	}

	private void DetachEquipmentModel()
	{
		if (!((Object)(object)_equipmentObj == (Object)null))
		{
			_equipmentObj.transform.parent = null;
			BoneMergeable.DetachBoneMerge(_equipmentObj);
			Object.Destroy((Object)(object)_equipmentObj);
			_equipmentObj = null;
			_equipmentAnim = null;
		}
	}

	private void AttachEquipmentModel(GameObject equipObj)
	{
		if (equipObj.layer != ((Component)this).gameObject.layer)
		{
			NGUITools.SetLayer(equipObj, ((Component)this).gameObject.layer);
		}
		_equipmentObj = equipObj;
		_equipmentAnim = _equipmentObj.GetComponent<Animation>();
		BoneMergeable.AttachBoneMerge(_equipmentObj);
		UpdateWeaponTip();
		ItemColor color = CostumeColors[7];
		_costume.ChangeCostumeColor(CharacterCostume.CostumeType.Equipment, color);
	}

	private void UpdateWeaponTip()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = KUtility.FindObjectByName(_equipmentObj, "Weapon_Tip");
		if ((Object)(object)val != (Object)null)
		{
			_weaponTipTransform = val.transform;
			return;
		}
		Transform val2 = KUtility.FindTransformByName(_equipmentObj, "Attachment_RH");
		if (Object.op_Implicit((Object)(object)val2))
		{
			val = new GameObject("Weapon_Tip");
			val.transform.parent = val2;
			val.transform.localPosition = new Vector3(-100f, 0f, 0f);
			_weaponTipTransform = val.transform;
		}
		else
		{
			_weaponTipTransform = MeshObjectTransform;
		}
	}

	public void SetWeaponData(WeaponDisplayInfo weaponDisplayInfo)
	{
		ProjectileController.SetWeaponData(weaponDisplayInfo);
		if (!string.IsNullOrEmpty(weaponDisplayInfo.WeaponFramework))
		{
			WeaponFramework wt = weaponDisplayInfo.WeaponFramework.ToEnum(WeaponFramework.ONEHAND);
			ChangeWeaponType(wt);
		}
	}

	private void RefreshEquipmentAnim()
	{
		if (!((Object)(object)_equipmentAnim == (Object)null) && CurrentAnimClipInfo != null)
		{
			string equipAnimation = CurrentAnimClipInfo.EquipAnimation;
			if (!string.IsNullOrEmpty(equipAnimation))
			{
				string text = ((!IsMale) ? "F_" : "M_");
				equipAnimation = text + equipAnimation;
				_equipmentAnim.Play(equipAnimation);
			}
		}
	}

	protected override void OnTileChanged(Point2 prev, Point2 current)
	{
		CurrentBiome = TerrainA6.TilePositionToBiome(current);
		bool flag = (CurrentTileObject = TerrainA6.GetTileObject(current, warning: false))?.IsInside ?? false;
		if (IsInside != flag)
		{
			IsInside = flag;
			if (flag)
			{
				PlaneShadowsComponent.Show = false;
				ContactShadowModel contactShadowModel = KSingleton<ContactShadowManager>.Instance().Create(((Component)this).gameObject, IsLocalPlayer, !IsLocalPlayer);
				contactShadowModel.FootShadowOffset.y = 10f;
				contactShadowModel.CenterShadowOffset.y = 10f;
			}
			else
			{
				PlaneShadowsComponent.Show = true;
				KSingleton<ContactShadowManager>.Instance().Remove(((Component)this).gameObject);
			}
		}
	}

	public void ChangeEquipmentWhileCurrentAnimation(string equipPath)
	{
		if (equipPath != null)
		{
			if (_motionTransitionTime > 0f)
			{
				_motionEquipment.Reserve(equipPath, default(ItemColor));
			}
			else
			{
				AnimationEquipEvent(equipPath, default(ItemColor));
			}
		}
	}

	public void UpdateScaleBody()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		((Component)MeshObjectTransform).transform.localScale = Vector3.one * _bodySize;
		((Component)_headTransform).transform.localScale = Vector3.one * ((!(_bodySize <= 1f)) ? (1f - (_bodySize - 1f) / 0.1f * 0.1f) : (1.1f - (_bodySize - 0.85f) / 0.15f * 0.1f));
	}

	public override void TurnToYaw(float yaw, bool bSnap)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		TargetYaw = yaw;
		if (bSnap)
		{
			MainTransform.localRotation = Quaternion.Euler(0f, TargetYaw, 0f);
		}
	}

	private void ProcessRootMotionMovements()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		LastRootMotionDelta = Vector3.zero;
		if (IsCurrentPlayAnimTag(PlayerAnimationClipTag.RootMotion))
		{
			AnimationState val = Anim[CurrentAnimClipName];
			if ((TrackedReference)(object)val != (TrackedReference)null && val.enabled && IsLocalPlayer && !CurrentlyDoingServerMotion)
			{
				Vector3 val2 = base.RootMotionTransform.localPosition - _prevRootBoneLocalPos;
				val2 = base.RootMotionTransform.parent.TransformDirection(val2);
				val2.y = 0f;
				LastRootMotionDelta = val2;
			}
			_prevRootBoneLocalPos = base.RootMotionTransform.localPosition;
			base.RootMotionMovable.LateUpdateRootMotion(MeshObjectTransform);
		}
		else if (IsAnimTag(_prevAnimClipInfo, PlayerAnimationClipTag.RootMotion) && _motionFadeTimer > 0f)
		{
			_prevRootBoneLocalPos = base.RootMotionTransform.localPosition;
			base.RootMotionMovable.LateUpdateRootMotion(MeshObjectTransform);
		}
		else
		{
			_prevRootBoneLocalPos = base.RootMotionTransform.localPosition;
			base.RootMotionMovable.ResetRootMotionOffset(MeshObjectTransform);
		}
	}

	private void ProcessRotate()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (YawLock)
		{
			return;
		}
		if (IsAnimTag(CurrentAnimClipInfo, PlayerAnimationClipTag.LookAtTarget) && (_nextAnimClipInfo == null || IsAnimTag(_nextAnimClipInfo, PlayerAnimationClipTag.LookAtTarget)) && Object.op_Implicit((Object)(object)Target))
		{
			Transform val = Target.transform;
			if (Object.op_Implicit((Object)(object)CharacterTarget))
			{
				val = CharacterTarget.GetBodyPartTransform(BodyPart.Body);
			}
			RotateToPosition(val.position);
		}
		Quaternion localRotation = MainTransform.localRotation;
		float y = ((Quaternion)(ref localRotation)).eulerAngles.y;
		float num = KMathUtil.DistanceAngDeg(y, TargetYaw);
		if (num <= 1f)
		{
			MainTransform.localRotation = Quaternion.Euler(0f, TargetYaw, 0f);
			return;
		}
		y = Mathf.MoveTowardsAngle(y, TargetYaw, RotateSpeed * Time.deltaTime);
		MainTransform.localRotation = Quaternion.Euler(0f, y, 0f);
	}

	private void ProcessMove()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		UpdateVelocity();
		if (IsMoving && IsCombatMode)
		{
			Vector2 val4 = default(Vector2);
			if ((Object)(object)Target != (Object)null)
			{
				Vector3 val = Target.transform.position - CurrentPosition;
				((Vector3)(ref val)).Normalize();
				Quaternion val2 = Quaternion.FromToRotation(val, Vector3.forward);
				Vector3 currentVelocity = base.CurrentVelocity;
				Vector3 val3 = val2 * ((Vector3)(ref currentVelocity)).normalized;
				val4.x = val3.x;
				val4.y = val3.z;
				((Vector2)(ref val4)).Normalize();
			}
			else
			{
				val4.x = 0f;
				val4.y = 1f;
			}
			float num = Mathf.Atan2(val4.x, val4.y) * 57.29578f;
			if (num < 0f)
			{
				num += 360f;
			}
			if (num > 360f)
			{
				num -= 360f;
			}
			DirAngle = num;
		}
	}

	private void UpdateVelocity()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		if (_prevMoveTime < 0f)
		{
			_prevMoveTime = time;
		}
		if (IsMoving)
		{
			float num = time - _prevMoveTime;
			_prevMoveTime = time;
			if (num > 0f)
			{
				base.CurrentVelocity = (CurrentPosition - _prevPosition) / num;
				_prevPosition = CurrentPosition;
			}
		}
		else
		{
			base.CurrentVelocity = Vector3.zero;
		}
	}

	public void Teleport(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		CurrentPosition = pos;
		_prevPosition = pos;
		base.CurrentVelocity = Vector3.zero;
		TurnToYaw(Random.Range(0, 360), bSnap: true);
	}

	private void ProcessDrawLines()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		ulong num = (ulong)Connections.Frontend.GetBufferedServerTime();
		int count = _drawLineBuffer.Count;
		int num2 = -1;
		for (int i = 0; i < count; i++)
		{
			DrawLineBase drawLineBase = _drawLineBuffer[i];
			if (drawLineBase.Time > num)
			{
				break;
			}
			if (drawLineBase.Position == Vector3.zero)
			{
				WorldLineRenderer.AddLineSegment();
			}
			else
			{
				Vector3 worldPos = TerrainA6.WorldPositionToClientPosition(drawLineBase.Position);
				WorldLineRenderer.AddLinePoint(worldPos);
			}
			num2 = i;
		}
		_drawLineBuffer.RemoveRange(0, num2 + 1);
	}

	public void AddDrawLineBuffer(DrawLineBase[] buffers)
	{
		_drawLineBuffer.AddRange(buffers);
	}

	public void OnVoiceMsg(byte[] buffers)
	{
	}

	public GameObject GetTargetObjectByType()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return (!((Object)(object)CharacterTarget == (Object)null)) ? ((Component)CharacterTarget.GetBodyPartTransform(BodyPart.Auto)).gameObject : Target;
	}

	public void RotateToSide(GameObject target, bool bSnap = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			Vector3 interactionPosition = KUtility.GetInteractionPosition(target);
			float yawDeg = KMathUtil.CalcYawWithTarget(interactionPosition, CurrentPosition) + 90f;
			interactionPosition += KMathUtil.CalcDirectionFromYaw(yawDeg) * 1000f;
			RotateToPosition(interactionPosition, bSnap);
		}
	}

	public void RotateToTarget(GameObject target, bool bSnap = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			Vector3 interactionPosition = KUtility.GetInteractionPosition(target);
			RotateToPosition(interactionPosition, bSnap);
		}
	}

	public void RotateDodgePrologue(GameObject target, bool bSnap = false, bool backward = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			Vector3 val = KMathUtil.Make2D(CurrentPosition - target.transform.position);
			val = ((!(((Vector3)(ref val)).magnitude < 100f)) ? ((Vector3)(ref val)).normalized : KMathUtil.ProjectDirection(target.transform));
			if (backward)
			{
				val = -val;
			}
			val.x *= 1000f;
			val.y = 0f;
			val.z = 0f;
			Vector3 pos = target.transform.position + val;
			pos.y = 0f;
			RotateToPosition(pos, bSnap);
		}
	}

	public void RotateToPosition(Vector3 pos, bool bSnap = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		float num = KMathUtil.CalcYawWithTarget(pos, CurrentPosition);
		if (bSnap)
		{
			MainTransform.localRotation = Quaternion.Euler(0f, num, 0f);
		}
		else
		{
			Quaternion localRotation = MainTransform.localRotation;
			float num2 = Mathf.MoveTowardsAngle(((Quaternion)(ref localRotation)).eulerAngles.y, num, Time.deltaTime * 300f);
			MainTransform.localRotation = Quaternion.Euler(0f, num2, 0f);
		}
		TargetYaw = num;
	}

	public override void OnTakeDamage(Damage damage, GameObject attacker)
	{
		base.OnTakeDamage(damage, attacker);
		CharacterBehavior characterBehavior = ((!((Object)(object)attacker != (Object)null)) ? null : attacker.GetComponent<CharacterBehavior>());
		if (IsAlive)
		{
			if (IsLocalPlayer)
			{
				if (damage.Value > 0)
				{
					Voice.Play(Player.PlayerVoice.Type.Hurt);
				}
				UIManager.AddDamageLabel(this, damage, characterBehavior);
				if (!_controller.IsInServerSideBattle)
				{
					ReactMotionLocalPlayer(damage, attacker);
				}
				DeathActionDescriptor.Attacked(characterBehavior, damage.Value);
			}
			if (Object.op_Implicit((Object)(object)LookAtController))
			{
				LookAtController.SetLookTarget(attacker, bFindHead: true);
			}
		}
		if (damage.Value > 0)
		{
			if (IsLocalPlayer)
			{
				UIManager.OnHitLocalPlayer();
				if ((damage.Effects & DamageEffects.Critical) != 0)
				{
					KSingleton<CameraShaker>.Instance().DamageShake(Mathf.Max(damage.Value / 3, 3));
				}
			}
			TakeBoneFlinching(damage.Part);
		}
		PlayDamagerEffectSet(attacker, damage);
		if (this.DamageTaken != null)
		{
			this.DamageTaken(characterBehavior, damage);
		}
	}

	private void ReactMotionLocalPlayer(Damage damage, GameObject attacker)
	{
		if (!IsLocalPlayer)
		{
			return;
		}
		if (damage.Result == DamageResult.Dodged || damage.Result == DamageResult.AutoDodged)
		{
			if (GameManager.IsPrologueMode)
			{
				RotateDodgePrologue(attacker, bSnap: true);
			}
			else
			{
				RotateToSide(attacker, bSnap: true);
			}
			_controller.Motion("Dodge");
		}
		else if (damage.Result == DamageResult.Guarded || damage.Result == DamageResult.AutoGuarded)
		{
			RotateToSide(attacker, bSnap: true);
			_controller.Motion("Guard");
		}
		else if (damage.Result != DamageResult.Missed)
		{
		}
		if (damage.Value <= 0)
		{
			return;
		}
		if ((damage.Effects & DamageEffects.Blow) != 0)
		{
			if (GameManager.IsPrologueMode)
			{
				RotateDodgePrologue(attacker, bSnap: true, backward: true);
				_controller.Motion("Blow_Begin");
			}
			else
			{
				RotateToTarget(attacker, bSnap: true);
				_controller.Motion("Blow");
			}
		}
		else if ((damage.Effects & DamageEffects.KnockBack) != 0)
		{
			_controller.Motion("Flinch");
		}
	}

	public override void TakeBoneFlinching(BodyPart part)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)null != (Object)(object)_boneFlinchingController)
		{
			_boneFlinchingController.TakeBoneFlinching(GetBodyPartTransform(BodyPart.Back));
		}
	}

	private void OnDie(bool bDeadMotionAtLogin = false)
	{
		if (IsCurrentAnimTag(PlayerAnimationClipTag.Dead))
		{
			return;
		}
		if (this.Died != null)
		{
			this.Died(this);
		}
		SetAnimationState("Die");
		if (bDeadMotionAtLogin)
		{
			AnimationState val = Anim[CurrentAnimClipName];
			if ((TrackedReference)(object)val != (TrackedReference)null)
			{
				val.normalizedTime = 0.7f;
			}
		}
		else
		{
			Voice.Play(Player.PlayerVoice.Type.Die);
		}
		if ((Object)(object)LastAttacker != (Object)null)
		{
			CharacterBehavior component = LastAttacker.GetComponent<CharacterBehavior>();
			if ((Object)(object)component != (Object)null)
			{
				component.OnKilledPlayer(this);
			}
		}
	}

	public void Respawn()
	{
		((Component)this).gameObject.layer = LayerHelper.DefaultLayer;
		AnimationRefresh(forceRefresh: true);
		if (this.Respawned != null)
		{
			this.Respawned(this);
		}
	}

	public void PlaySound(string path)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SoundManager.Play(path, CurrentPosition);
	}

	public void PlayMusic(Music music, string instrument)
	{
		if (_musicController == null)
		{
			_musicController = new MusicController();
			MusicController musicController = _musicController;
			musicController.OnStop = (Action)Delegate.Combine(musicController.OnStop, (Action)delegate
			{
				AnimationRefresh();
			});
		}
		MusicManager.Play(_musicController, music, instrument, loop: false, MainTransform);
		string text = null;
		string text2 = null;
		switch (instrument)
		{
		case "Guitar":
			text = "Play_Guitar_A";
			text2 = "Models/Equipment/Tool/instrument_guitar01.FBX";
			break;
		case "Horn":
			text = "Play_Horn";
			text2 = "Models/Equipment/Tool/instrument_horn01.fbx";
			break;
		case "Slam":
			text = "Play_Drum_A";
			text2 = "Models/Equipment/Tool/instrument_drumstick.fbx";
			break;
		case "Woody":
			text = "Play_Synth";
			text2 = "Models/Equipment/Tool/instrument_drumstick.fbx";
			break;
		case "PanFlute":
			text = "Play_Horn";
			text2 = "Models/Equipment/Tool/instrument_horn01.fbx";
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			string equipment = text2;
			PlayAnimation(text, 0f, 1f, forceTransition: false, equipment);
		}
	}

	public bool StopMusic()
	{
		if (_musicController != null && _musicController.IsPlay)
		{
			_musicController.IsPlay = false;
			return true;
		}
		return false;
	}

	public override Transform GetHeadTransform()
	{
		return MeshObjectTransform;
	}

	public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, [Optional] Vector3 nearPos)
	{
		Transform val = null;
		switch (part)
		{
		case BodyPart.Head:
			val = _headTransform;
			break;
		case BodyPart.Body:
			val = _bodyTransform;
			break;
		case BodyPart.Arm:
			val = _leftArmTransform;
			break;
		case BodyPart.Leg:
			val = _leftLegTransform;
			break;
		case BodyPart.Back:
			val = _spineTransform;
			break;
		}
		if ((Object)null != (Object)(object)val)
		{
			return val;
		}
		if (bAllowNull)
		{
			return null;
		}
		return MainTransform;
	}

	public override string GetName()
	{
		return (!string.IsNullOrEmpty(PlayerName)) ? PlayerName : T._("알수없음");
	}

	public override void SetSurvivalGauge(Gauge life, Dictionary<string, Gauge> gauges)
	{
		double at = Gauge.CurrentTime;
		if (base.Life == null && life.Get(at) <= 0f)
		{
			OnDie(bDeadMotionAtLogin: true);
		}
		if (IsLocalPlayer)
		{
			ReservePushNotifiaction(SurvivalGauges.GetGauge("health"), gauges.Get("health"), (Gauge gauge) => (!((double)gauge.Get(at) <= (double)gauge.Max() * 0.3)) ? 0.0 : gauge.When(gauge.Max()), PushNotification.Type.HealthRecovered, T._("부상이 모두 회복되었습니다."));
			ReservePushNotifiaction(SurvivalGauges.GetGauge("fatige"), gauges.Get("fatigue"), (Gauge gauge) => (!((double)gauge.Get(at) >= (double)gauge.Max(at) * 0.7)) ? 0.0 : gauge.When(gauge.Min()), PushNotification.Type.FatigueRecovered, T._("피로가 모두 회복되었습니다."));
		}
		base.SetSurvivalGauge(life, gauges);
	}

	private static void ReservePushNotifiaction(Gauge prev, Gauge current, Gauge.WhenDelegate when, PushNotification.Type type, string message, string messageForTommorrow = null)
	{
		double num = ((prev == null) ? 0.0 : when(prev));
		double num2 = ((current == null) ? 0.0 : when(current));
		if (num == num2)
		{
			return;
		}
		KSingleton<GameManager>.Instance().PushNotification.CancelLocalPush(type);
		double num3 = num2 - Connections.Frontend.GetPredictedServerTime();
		if (num3 > 0.0)
		{
			KSingleton<GameManager>.Instance().PushNotification.LocalPushAfter(type, message, "offline_only", (int)num3);
			if (!string.IsNullOrEmpty(messageForTommorrow))
			{
				num3 += 86400.0;
				KSingleton<GameManager>.Instance().PushNotification.LocalPushAfter(type, messageForTommorrow, "offline_only", (int)num3);
			}
		}
	}

	private void CheckAlive()
	{
		if (base.Life != null)
		{
			float num = base.Life.Get();
			if (_prevLife > 0f && num <= 0f)
			{
				OnDie();
			}
			else if (_prevLife <= 0f && num > 0f && !IsLocalPlayer)
			{
				Respawn();
			}
			_prevLife = num;
		}
	}

	public override void SetWeaponVisible(bool visible)
	{
		if (Object.op_Implicit((Object)(object)_equipmentObj))
		{
			_equipmentObj.SetActive(visible);
			OutlineComponent.RefreshModel();
			PlaneShadowsComponent.RefreshModel();
		}
	}

	public override string GetCurrentAnimationClipName()
	{
		return CurrentAnimClipName;
	}

	[UsedImplicitly]
	private void Cmd_PutOnInnerCostume()
	{
		_costume.ChangeCostume(DefaultInnerCostume);
		_prevHeadVisible = _costume.GetCostumeVisible(CharacterCostume.CostumeType.Head);
		_costume.SetCostumeVisible(CharacterCostume.CostumeType.Head, isVisible: false);
	}

	[UsedImplicitly]
	private void Cmd_PutOnCurrentCostume()
	{
		_costume.ChangeCostume(CurrentBodyCostume);
		if (_prevHeadVisible)
		{
			_costume.SetCostumeVisible(CharacterCostume.CostumeType.Head, isVisible: true);
		}
	}

	public override string GetAttackNameForDeathMsg()
	{
		return LocalizeSystem.GetRandom(_deathMsgAttackPlayerList);
	}

	protected override void MoveMotionChangedByObject()
	{
		if (!IsProhibitingAdaptiveMotionChanges)
		{
			AnimationRefresh(forceRefresh: true);
		}
	}

	public void SetCombatMode(bool combatMode, float timePassed)
	{
		if (IsCombatMode != combatMode)
		{
			IsCombatMode = combatMode;
			if (!IsCombatMode)
			{
				Target = null;
			}
			if (IsLocalPlayer)
			{
				KSingleton<UIManager>.Instance().PlayerFloatingGroup.UpdateNameColor(this);
			}
		}
	}

	[UsedImplicitly]
	private void OnSelected(bool select)
	{
		_selected = select;
		UpdateOutline();
	}

	[UsedImplicitly]
	private void PrepareArrow(float timePassed)
	{
		SetAnimationState("Charge");
	}

	private void OnChargedProjectile()
	{
		ProjectileController.OnChargedProjectile(Target);
	}

	public void ForceRemoveUnfiredArrow()
	{
		ProjectileController.ForceRemovUnfiredArrow();
	}

	[UsedImplicitly]
	private void OnShootProjectile()
	{
		ProjectileController.OnShootProjectile(Target);
	}

	private static bool IsIdleMovement(Movement movement)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (movement.Path.Length <= 1)
		{
			return true;
		}
		Vector3 val = movement.Path[movement.Path.Length - 1].Position.ToClientPosition() - movement.Path[0].Position.ToClientPosition();
		return ((Vector3)(ref val)).magnitude < 10f;
	}

	private void MovementProcessed(Movement movement)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		if (!IsServerSideMoveControl || string.IsNullOrEmpty(movement.MotionName))
		{
			return;
		}
		if (movement.MotionName == "Idle")
		{
			IsMoving = false;
			return;
		}
		if (IsRiding)
		{
			IsMoving = movement.MotionName.ContainsIgnoreCase("Run") || movement.MotionName.ContainsIgnoreCase("Walk");
			return;
		}
		try
		{
			RunState = (RunStateEnum)(int)Enum.Parse(typeof(RunStateEnum), movement.MotionName, ignoreCase: true);
			if (IsIdleMovement(movement))
			{
				IsMoving = false;
				return;
			}
			Location location = movement.Path[0];
			Location location2 = movement.Path[movement.Path.Length - 1];
			Vector3 val = location2.Position.ToClientPosition() - location.Position.ToClientPosition();
			IsMoving = ((Vector3)(ref val)).magnitude > 1f;
		}
		catch (ArgumentException)
		{
			IsMoving = false;
			string motionName = movement.MotionName;
			if (IsLocalPlayer)
			{
				bool flag = motionName.ContainsIgnoreCase("Run") || motionName.ContainsIgnoreCase("Stand") || motionName.ContainsIgnoreCase("Walk");
				PlayerController controller = _controller;
				bool forceTransition = !flag;
				controller.Motion(motionName, 0f, movement.PlaybackRate, forceTransition);
			}
			else
			{
				float playbackRate = movement.PlaybackRate;
				PlayAnimation(motionName, 0f, playbackRate, forceTransition: true);
			}
			base.RootMotionMovable.SetLocalRootMotionYawMode((movement.MotionOption & 0x80) > 0);
		}
	}

	public void HandleMoveMsg(Move msg)
	{
		if (IsLocalPlayer && IsCombatMode)
		{
			int num = msg.Movements.Length;
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				string motionName = msg.Movements[num2].MotionName;
				bool flag = motionName.ContainsIgnoreCase("Run") || motionName.ContainsIgnoreCase("Stand") || motionName.ContainsIgnoreCase("Walk_Bush");
				bool flag2 = motionName.ContainsIgnoreCase("Battle") || motionName.ContainsIgnoreCase("Aim");
				if (flag && !flag2)
				{
					return;
				}
			}
		}
		PathMovable.HandleMoveMsg(msg);
	}

	[UsedImplicitly]
	private void OnPickAxeHit()
	{
		PlaySound("Sound/Effect/metal_picking.wav");
	}

	[UsedImplicitly]
	private void OnAxeHit()
	{
		PlaySound("Sound/Effect/tree_axe_hit.wav");
	}

	private void CheckMotionState()
	{
		if (IgnoreMotionState)
		{
			return;
		}
		if (_motionInterruptTimer > 0f)
		{
			_motionInterruptTimer -= Time.deltaTime;
			if (_motionInterruptTimer <= 0f)
			{
				AnimationRefresh(forceRefresh: true);
			}
		}
		if (_reserveAnimTime > 0f && _reserveAnimTime < Time.time)
		{
			_reserveAnimTime = 0f;
			if (string.IsNullOrEmpty(_reserveAnim))
			{
				_reserveAnim = null;
				if (!IsServerSideMoveControl)
				{
					AnimationRefresh(forceRefresh: true);
				}
			}
			else
			{
				string[] array = _reserveAnim.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries);
				_reserveAnim = null;
				if (array.Length >= 2)
				{
					PlayAnimationStateClip(array[0], array[1], 0f, 1f, forceTransition: true);
				}
				else
				{
					PlayAnimation(array[0], 0f, 1f, forceTransition: true);
				}
			}
		}
		if (_motionFadeTimer > 0f)
		{
			_motionFadeTimer -= Time.deltaTime;
			if (_motionFadeTimer <= 0f)
			{
				OnMotionChangeFinished();
			}
		}
		if (_currentAnimClipBlendTree != null)
		{
			CalcAnimationBlendTree(_currentAnimClipBlendTree);
		}
	}

	private void OnMotionChangeFinished()
	{
		PlayerAnimationClipInfo currentAnimClipInfo = CurrentAnimClipInfo;
		CurrentAnimClipInfo = _nextAnimClipInfo;
		_nextAnimClipInfo = null;
		RefreshEquipmentAnim();
		OnAnimStateChanged(currentAnimClipInfo, CurrentAnimClipInfo);
	}

	public void SampleAnimImmediately()
	{
		LateMotionUpdate();
		Anim.Sample();
		OnMotionChangeFinished();
	}

	private void OnAnimStateChanged(PlayerAnimationClipInfo prev, PlayerAnimationClipInfo current)
	{
		if (this.AnimStateChanged != null)
		{
			this.AnimStateChanged(prev, current);
		}
	}

	private void AnimEventMotionChanged()
	{
		if (_motionEquipment.IsEquipped())
		{
			ReEquipCurrentWeapon();
		}
		if (_motionEquipment.IsReserved())
		{
			AnimationEquipEvent(_motionEquipment.EquipItemPath, _motionEquipment.Color);
		}
	}

	public void RestoreStandState()
	{
		if (IsCombatMode)
		{
			StandState = StandStateEnum.BattleStand;
		}
		else if (FatigueEffect == "hot")
		{
			StandState = StandStateEnum.Hot;
		}
		else if (FatigueEffect == "cold")
		{
			StandState = StandStateEnum.Cold;
		}
		else
		{
			StandState = StandStateEnum.Stand;
		}
	}

	public void SetStandState(StandStateEnum state)
	{
		StandState = state;
	}

	public bool IsCurrentAnimTag(PlayerAnimationClipTag clipTag)
	{
		return IsAnimTag(CurrentAnimClipInfo, clipTag);
	}

	public bool IsCurrentPlayAnimTag(PlayerAnimationClipTag clipTag)
	{
		return IsAnimTag(CurrentPlayAnimClipInfo, clipTag);
	}

	public bool IsCurrentAnimTagContainCrossFade(PlayerAnimationClipTag clipTag)
	{
		return IsAnimTag(CurrentAnimClipInfo, clipTag) || IsAnimTag(_nextAnimClipInfo, clipTag);
	}

	public static bool IsAnimTag(PlayerAnimationClipInfo clipInfo, PlayerAnimationClipTag clipTag)
	{
		if (clipInfo == null)
		{
			return false;
		}
		return (clipInfo.Tag & clipTag) != 0;
	}

	public bool IsCurrentAnimState(string state)
	{
		if (string.IsNullOrEmpty(state))
		{
			return false;
		}
		string text = CurrentAnimState?.State;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		return text == state;
	}

	private bool PlayAnimationClip(string clip, string state, float playbackRate, bool forceTransition)
	{
		if (string.IsNullOrEmpty(clip))
		{
			return false;
		}
		PlayerAnimationClipInfo playerAnimationClipInfo = AnimManager.GetPlayerAnimationClipInfo(clip, state);
		if (playerAnimationClipInfo == null)
		{
			return false;
		}
		int num = Mathf.Max(AnimManager.GetTagLevel(CurrentAnimClipInfo), AnimManager.GetTagLevel(_nextAnimClipInfo));
		int tagLevel = AnimManager.GetTagLevel(playerAnimationClipInfo);
		if (!forceTransition && num > tagLevel && (!IsLocalPlayer || !IsServerControlledMotionTagLevel) && IsLocalPlayer)
		{
			return false;
		}
		if (num == tagLevel)
		{
			List<PlayerAnimationClipTrasitionInfo> transitions = ((CurrentAnimClipInfo != null) ? CurrentAnimClipInfo.Transitions : null);
			PlayerAnimationClipTrasitionInfo transitionCondition = PlayerAnimationClipManager.GetTransitionCondition(transitions, TransitionCondition.OnExit);
			if (transitionCondition != null)
			{
				PlayerAnimationClipInfo playerAnimationClipInfo2 = AnimManager.GetPlayerAnimationClipInfo(transitionCondition.Clip, transitionCondition.State);
				string text = state;
				if (string.IsNullOrEmpty(text))
				{
					text = clip;
				}
				return PlayAnimationClip(playerAnimationClipInfo2, text, 0f, playbackRate, forceTransition);
			}
		}
		PlayerAnimationClipTrasitionInfo transitionCondition2 = PlayerAnimationClipManager.GetTransitionCondition(playerAnimationClipInfo.Transitions, TransitionCondition.OnFinished);
		string nextClip = null;
		float result = 0f;
		if (transitionCondition2 != null)
		{
			nextClip = transitionCondition2.State + " " + transitionCondition2.Clip;
			float.TryParse(transitionCondition2.Conditions[0].Value, out result);
		}
		return PlayAnimationClip(playerAnimationClipInfo, nextClip, result, playbackRate, forceTransition);
	}

	private bool PlayAnimationClip(PlayerAnimationClipInfo clipInfo, string nextClip, float nextClipTime, float playbackRate, bool forceTransition)
	{
		string arg = ((!IsMale) ? "F" : "M");
		string text = $"{arg}_{clipInfo.Clip}";
		float num = Mathf.Max((CurrentAnimClipInfo != null) ? CurrentAnimClipInfo.FadeOutTime : 0f, clipInfo.FadeInTime);
		if (!IsVisible)
		{
			num = 0f;
		}
		else if (num < 0f)
		{
			num = 0.1f;
		}
		if ((TrackedReference)(object)Anim[text] != (TrackedReference)null)
		{
			_playClipArgument.Set(text);
		}
		else
		{
			PlayerAnimationBlendTree blendTree = PlayerAnimationClipManager.GetBlendTree(clipInfo.Clip);
			if (blendTree == null || blendTree.Clips == null || blendTree.Clips.Count == 0)
			{
				return false;
			}
			_playClipArgument.Set(blendTree);
		}
		_playClipArgument.PlaybackRate = playbackRate;
		_playClipArgument.ClipInfo = clipInfo;
		CurrentAnimClipName = text;
		_prevAnimClipInfo = CurrentAnimClipInfo;
		_nextAnimClipInfo = clipInfo;
		_reserveAnim = nextClip;
		float num2 = ((!(clipInfo.FadeOutTime < 0f)) ? clipInfo.FadeOutTime : 0.1f);
		if ((clipInfo.IsLoop && nextClipTime == 0f) || IsAnimTag(clipInfo, PlayerAnimationClipTag.Once))
		{
			_reserveAnimTime = 0f;
		}
		else if (string.IsNullOrEmpty(_reserveAnim))
		{
			if (_reserveAnimTime > Time.time && _prevAnimClipInfo != null && _prevAnimClipInfo.Clip == clipInfo.Clip)
			{
				if (forceTransition)
				{
					_reserveAnimTime = Time.time + clipInfo.Length - num2;
				}
				else
				{
					_reserveAnimTime = Mathf.Min(Time.time + clipInfo.Length - num2, _reserveAnimTime);
				}
			}
			else
			{
				_reserveAnimTime = Time.time + clipInfo.Length - num2;
			}
		}
		else
		{
			if (nextClipTime > 0f)
			{
				_reserveAnimTime = Time.time + nextClipTime;
			}
			else
			{
				_reserveAnimTime = Time.time + clipInfo.Length + nextClipTime;
			}
			_reserveAnimTime -= num2;
		}
		_motionTransitionTime = num;
		_motionFadeTimer = num;
		if (_motionFadeTimer == 0f)
		{
			OnMotionChangeFinished();
		}
		return true;
	}

	public void LateMotionUpdate()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Invalid comparison between Unknown and I4
		if (!_playClipArgument.IsValid)
		{
			return;
		}
		string playAnimationClipName = _playClipArgument.PlayAnimationClipName;
		float motionTransitionTime = _motionTransitionTime;
		MotionOption motionOption = MotionOption.ALIGN_TO_PATH;
		if (_playClipArgument.BlendTree != null)
		{
			InitAnimationBlendTree(_playClipArgument.BlendTree, _playClipArgument.ClipInfo.IsLoop);
			motionOption |= MotionOption.LOOPING;
		}
		else
		{
			ResetAnimationBlendTree();
			AnimationState val = Anim[playAnimationClipName];
			val.wrapMode = (WrapMode)(_playClipArgument.ClipInfo.IsLoop ? 2 : 0);
			if ((int)val.wrapMode == 2)
			{
				motionOption |= MotionOption.LOOPING;
			}
			val.enabled = true;
			if (Anim.IsPlaying(playAnimationClipName) && !_playClipArgument.ClipInfo.IsLoop)
			{
				Anim.Stop();
			}
			if (motionTransitionTime > 0f)
			{
				Anim.CrossFade(playAnimationClipName, motionTransitionTime);
				Anim[playAnimationClipName].speed = _playClipArgument.PlaybackRate;
			}
			else
			{
				Anim.Play(playAnimationClipName);
				Anim[playAnimationClipName].speed = _playClipArgument.PlaybackRate;
			}
		}
		if (IsLocalPlayer && !IsServerSideMoveControl)
		{
			_controller.MotionBegined(CurrentAnimKeyName, motionOption, _playClipArgument.PlaybackRate);
		}
		_playClipArgument.Reset();
		if (!_motionStarted)
		{
			_motionStarted = true;
		}
		CurrentlyDoingServerMotion = IsServerSideMoveControl;
		if (!IsServerSideMoveControl)
		{
			base.RootMotionMovable.SetLocalRootMotionYawMode(isIgnoreYaw: true);
		}
	}

	public string CurrentActiveAnimationClipToString()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		StringBuilder stringBuilder = new StringBuilder();
		foreach (AnimationState item in Anim)
		{
			AnimationState val = item;
			if (val.enabled && val.weight > 0f)
			{
				stringBuilder.AppendLine(val.name + " " + val.weight);
			}
		}
		return stringBuilder.ToString().Trim();
	}

	private void InitAnimationBlendTree(PlayerAnimationBlendTree tree, bool isLoop)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		foreach (AnimationState item in Anim)
		{
			AnimationState val = item;
			if (val.enabled && val.weight > 0f)
			{
				_fadeoutStates.Add(new KeyValuePair<AnimationState, float>(val, val.weight));
			}
		}
		_currentAnimClipBlendTree = tree;
		string arg = ((!IsMale) ? "F" : "M");
		for (int i = 0; i < tree.Clips.Count; i++)
		{
			PlayerAnimationBlendTreeNode playerAnimationBlendTreeNode = tree.Clips[i];
			string text = $"{arg}_{playerAnimationBlendTreeNode.Clip}";
			AnimationState val2 = Anim[text];
			val2.blendMode = (AnimationBlendMode)0;
			val2.wrapMode = (WrapMode)(isLoop ? 2 : 0);
			val2.enabled = true;
			playerAnimationBlendTreeNode.SetLinkAnim(val2);
		}
	}

	private void CalcAnimationBlendTree(PlayerAnimationBlendTree tree)
	{
		float num = 1f;
		if (_motionFadeTimer > 0f)
		{
			num = 1f - _motionFadeTimer / _motionTransitionTime;
			for (int i = 0; i < _fadeoutStates.Count; i++)
			{
				_fadeoutStates[i].Key.weight = _fadeoutStates[i].Value * (1f - num);
			}
		}
		else if (_fadeoutStates.Count > 0)
		{
			for (int j = 0; j < _fadeoutStates.Count; j++)
			{
				_fadeoutStates[j].Key.weight = 0f;
				_fadeoutStates[j].Key.enabled = false;
			}
			_fadeoutStates.Clear();
		}
		if (tree.Clips.Count == 0)
		{
			return;
		}
		PlayerAnimationClipManager.CalcBlendTreeClipWeight(tree, this);
		float deltaTime = Time.deltaTime;
		float time = Mathf.Repeat(Time.time, tree.Clips[0].GetLinkAnim().length);
		for (int k = 0; k < tree.Clips.Count; k++)
		{
			PlayerAnimationBlendTreeNode playerAnimationBlendTreeNode = tree.Clips[k];
			AnimationState linkAnim = playerAnimationBlendTreeNode.GetLinkAnim();
			if (!linkAnim.enabled)
			{
				linkAnim.enabled = true;
			}
			float num2 = playerAnimationBlendTreeNode.Weight;
			if (!_useAnimBlendTreeInterp)
			{
				num2 = Mathf.Round(num2);
			}
			float num3 = num2 * num;
			float num4 = num3 - linkAnim.weight;
			if (num4 != 0f)
			{
				if (Mathf.Abs(num4) < deltaTime)
				{
					linkAnim.weight = num3;
				}
				else
				{
					linkAnim.weight += Mathf.Sign(num4) * deltaTime;
				}
			}
			linkAnim.time = time;
		}
	}

	private void ResetAnimationBlendTree()
	{
		if (_currentAnimClipBlendTree != null)
		{
			for (int i = 0; i < _fadeoutStates.Count; i++)
			{
				_fadeoutStates[i].Key.enabled = false;
			}
			_fadeoutStates.Clear();
			_currentAnimClipBlendTree = null;
		}
	}

	private bool SetAnimationState(string state, float playbackRate = 1f, bool forceTransition = false)
	{
		PlayerAnimationStateInfo playerAnimationStateInfo = AnimManager.GetPlayerAnimationStateInfo(state);
		if (playerAnimationStateInfo == null)
		{
			return false;
		}
		int size = KUtility.GetSize(playerAnimationStateInfo.StateTransitions);
		for (int i = 0; i < size; i++)
		{
			if (CheckAnimStateCondition(playerAnimationStateInfo.StateTransitions[i]) && PlayAnimationStateClip(playerAnimationStateInfo.StateTransitions[i].State, playerAnimationStateInfo.StateTransitions[i].Clip, 0f, 1f, forceTransition))
			{
				return true;
			}
		}
		PlayerAnimationStateClip playerAnimationStateClip = null;
		float num = -1f;
		size = KUtility.GetSize(playerAnimationStateInfo.Clips);
		for (int j = 0; j < size; j++)
		{
			PlayerAnimationStateClip playerAnimationStateClip2 = playerAnimationStateInfo.Clips[j];
			float num2 = CheckStateClipConditions(playerAnimationStateClip2.Conditions);
			if (num2 > num)
			{
				num = num2;
				playerAnimationStateClip = playerAnimationStateClip2;
			}
		}
		if (playerAnimationStateClip == null)
		{
			return false;
		}
		return PlayAnimationClip(playerAnimationStateClip.Clip, playerAnimationStateClip.GetParent().State, playbackRate, forceTransition);
	}

	private bool PlayAnimationStateClip(string state, string clip, float time = 0f, float playbackRate = 1f, bool forceTransition = false)
	{
		if (string.IsNullOrEmpty(state) && string.IsNullOrEmpty(clip))
		{
			return false;
		}
		if (time > 0f)
		{
			_motionInterruptTimer = time;
		}
		return (!string.IsNullOrEmpty(clip)) ? PlayAnimationClip(clip, state, playbackRate, forceTransition) : SetAnimationState(state, playbackRate, forceTransition);
	}

	public bool PlayAnimation(string motion, float time = 0f, float playbackRate = 1f, bool forceTransition = false, string equipment = null, [Optional] ItemColor equipColor)
	{
		if (string.IsNullOrEmpty(motion))
		{
			return false;
		}
		if (equipment != null)
		{
			_motionEquipment.Reserve(equipment, equipColor);
		}
		bool flag = AnimManager.GetPlayerAnimationStateInfo(motion) != null;
		string state = ((!flag) ? string.Empty : motion);
		string clip = ((!flag) ? motion : string.Empty);
		return PlayAnimationStateClip(state, clip, time, playbackRate, forceTransition);
	}

	private bool CheckAnimStateCondition(PlayerAnimationClipTrasitionInfo transitionInfo)
	{
		if (transitionInfo.Conditions == null || transitionInfo.Conditions.Count == 0)
		{
			return false;
		}
		int count = transitionInfo.Conditions.Count;
		for (int i = 0; i < count; i++)
		{
			float num = CheckStateCondition(transitionInfo.Conditions[i]);
			if (num < 0f)
			{
				return false;
			}
		}
		return true;
	}

	private float CheckStateClipConditions(List<PlayerAnimationCondition> conditions)
	{
		if (conditions == null)
		{
			return 0f;
		}
		float num = 0f;
		int count = conditions.Count;
		for (int i = 0; i < count; i++)
		{
			float num2 = CheckStateCondition(conditions[i]);
			if (num2 > 0f)
			{
				num += num2;
			}
			else if (num2 < 0f)
			{
				num = -1f;
			}
			if (num == -1f)
			{
				break;
			}
		}
		return num;
	}

	private float CheckBoolCondition(int[] condition, bool current)
	{
		if (condition == null || condition.Length == 0)
		{
			return -1f;
		}
		return (condition[0] != 0 != current) ? (-1f) : 1f;
	}

	private float CheckEnumCondition(int[] condition, int value)
	{
		if (condition == null)
		{
			return -1f;
		}
		return (Array.IndexOf(condition, value) != -1) ? 1f : (-1f);
	}

	private float CheckStateCondition(PlayerAnimationCondition condition)
	{
		float result = 0f;
		int[] values = condition.GetValues();
		switch ((StateClipCondition)condition.GetConditionType())
		{
		case StateClipCondition.Framework:
			result = CheckEnumCondition(values, (int)CurrentWeaponFramework);
			break;
		case StateClipCondition.PrevState:
		{
			if (string.IsNullOrEmpty(condition.Value))
			{
				result = -1f;
				break;
			}
			bool flag = false;
			if (condition.Value[0] == '!')
			{
				flag = true;
				condition.Value = condition.Value.Substring(1);
			}
			bool flag2 = AnimManager.CheckClip(CurrentAnimClipInfo, condition.Value);
			result = (((!flag) ? flag2 : (!flag2)) ? 1 : (-1));
			break;
		}
		case StateClipCondition.TargetSize:
			result = CheckEnumCondition(values, (int)CharacterTarget.Size);
			break;
		case StateClipCondition.Random:
			result = ((values != null && values.Length != 0) ? (Random.value * (float)values[0]) : (-1f));
			break;
		case StateClipCondition.IsInWater:
			result = CheckBoolCondition(values, IsInWater);
			break;
		case StateClipCondition.IsMoving:
			result = CheckBoolCondition(values, IsMoving);
			break;
		case StateClipCondition.IsSwimming:
			result = CheckBoolCondition(values, IsSwimming);
			break;
		case StateClipCondition.RunState:
			result = CheckEnumCondition(values, (int)RunState);
			break;
		case StateClipCondition.StandState:
			result = CheckEnumCondition(values, (int)StandState);
			break;
		case StateClipCondition.IsWaterCarried:
			result = CheckBoolCondition(values, IsWaterCarried);
			break;
		case StateClipCondition.IsBushWhack:
			result = CheckBoolCondition(values, base.IsBushWhacking);
			break;
		case StateClipCondition.IsTired:
			result = CheckBoolCondition(values, IsTired);
			break;
		case StateClipCondition.IsRoadRunning:
			result = CheckBoolCondition(values, base.IsRoadRunning);
			break;
		case StateClipCondition.IsRest:
			result = CheckBoolCondition(values, IsRest);
			break;
		case StateClipCondition.IsSleep:
			result = CheckBoolCondition(values, IsSleep);
			break;
		case StateClipCondition.IsNovice:
			result = CheckBoolCondition(values, IsNovice);
			break;
		}
		return result;
	}

	public void AnimationRefresh(bool forceRefresh = false)
	{
		if (!IsProhibitAnimRefresh)
		{
			_needAnimationRefresh = ((!forceRefresh) ? 1 : 2);
		}
	}

	private void LateAnimationRefresh(bool forceRefresh)
	{
		bool forceTransition = forceRefresh;
		PlayAnimation("Stand", 0f, 1f, forceTransition);
	}

	public void SetMotionParam(string param, int value)
	{
		switch (param)
		{
		case "Stand":
			BaseStandState = (StandStateEnum)value;
			break;
		case "IsRest":
			IsRest = value != 0;
			break;
		case "IsSleep":
			IsSleep = value != 0;
			break;
		}
	}

	public void SetParticleEffect(string path, float time)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (time > 0f)
		{
			GameObject val = ParticleManager.EmitSync(path, Vector3.zero, Quaternion.identity, MainTransform);
			if ((Object)(object)val != (Object)null)
			{
				_particleEffectList.Add(new KeyValuePair<GameObject, float>(val, Time.time + time));
			}
		}
		else
		{
			ParticleManager.Emit(path, Vector3.zero, Quaternion.identity, MainTransform);
		}
	}

	private void UpdateParticleObject()
	{
		float time = Time.time;
		for (int num = _particleEffectList.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)_particleEffectList[num].Key == (Object)null)
			{
				_particleEffectList.RemoveAt(num);
			}
			else if (_particleEffectList[num].Value < time)
			{
				ParticleManager.Stop(_particleEffectList[num].Key, immediately: false);
				_particleEffectList.RemoveAt(num);
			}
		}
	}

	public void DamageResultReceived(Damage damage)
	{
		ProjectileController.DamageResultReceived(damage);
	}

	[UsedImplicitly]
	private void OnAttack()
	{
		IsAttackFramePassed = true;
		if ((Object)(object)Target == (Object)null)
		{
			return;
		}
		CharacterBehavior component = Target.GetComponent<CharacterBehavior>();
		if (!((Object)(object)component == (Object)null))
		{
			if (IsLocalPlayer)
			{
				component.TakeBoneFlinching(BodyPart.Body);
			}
			if (this.AttackSucceeded != null)
			{
				this.AttackSucceeded(component, BodyPart.Body, CurrentAnimKeyName);
			}
		}
	}
}
