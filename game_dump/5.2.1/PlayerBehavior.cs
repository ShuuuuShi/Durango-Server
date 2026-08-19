using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Durango.Logic;
using Durango.Logic.Music;
using Durango.Logic.Social;
using Durango.Model;
using Durango.Network;
using Durango.Player;
using Durango.Player.Animation;
using Durango.Render;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Battle;
using Shared.Display;
using Shared.Region;
using UnityEngine;

public class PlayerBehavior : CharacterBehavior, IAnimationEventPlayable, ICostumable, IMotionPlayable
{
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
		NONE
	}

	private struct PlayClipArgument
	{
		public bool IsValid;

		public string PlayAnimationClipName;

		public PlayerAnimationClipInfo ClipInfo;

		public float PlaybackRate;

		public void Set(string clipName)
		{
			Reset();
			IsValid = true;
			PlayAnimationClipName = clipName;
		}

		public void Reset()
		{
			IsValid = false;
			PlayAnimationClipName = null;
			ClipInfo = null;
			PlaybackRate = 1f;
		}

		public override string ToString()
		{
			if (IsValid)
			{
				return PlayAnimationClipName;
			}
			return "Not Valid";
		}
	}

	[CompilerGenerated]
	private sealed class _003CCoReservedMountTarget_003Ed__302 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PlayerBehavior _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoReservedMountTarget_003Ed__302(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PlayerBehavior playerBehavior = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			}
			if (string.IsNullOrEmpty(playerBehavior._reservedMountTargetId))
			{
				return false;
			}
			if (!playerBehavior.TryMountTarget(playerBehavior._reservedMountTargetId))
			{
				_003C_003E2__current = playerBehavior._waitForHalfSeconds;
				_003C_003E1__state = 2;
				return true;
			}
			playerBehavior._reservedMountTargetId = string.Empty;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CPlayAnimationClipsSequence_003Ed__295 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public IEnumerable<PlayerAnimationClipInfo> clips;

		public PlayerBehavior _003C_003E4__this;

		public float playBackRates;

		private IEnumerator<PlayerAnimationClipInfo> _003C_003E7__wrap1;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CPlayAnimationClipsSequence_003Ed__295(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				PlayerBehavior playerBehavior = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = clips.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					PlayerAnimationClipInfo current = _003C_003E7__wrap1.Current;
					if (current != null && playerBehavior.TryPlayClip(current, playBackRates))
					{
						_003C_003E2__current = new WaitForSeconds(current.Length - current.FadeOutTime);
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public const string DefaultMotion = "Barehand_Stand";

	public const float MinBodySizeRatio = 0.85f;

	public const float MaxBodySizeRatio = 1.1f;

	private static PlayerBehavior _localPlayer;

	[NonSerialized]
	public Transform MainTransform;

	[NonSerialized]
	public PlayerDisplay Display;

	private BoneMergeable _boneMergeable;

	private ProjectileController _projectileController;

	[SerializeField]
	private bool _isMale;

	[SerializeField]
	private GameObject _framework;

	[SerializeField]
	[EnumList(typeof(BodyPart), true, 1, 6)]
	private TransformResolver[] _bodyPartTransforms;

	[SerializeField]
	private TransformResolver _aimBasis = new TransformResolver("Attachment_RH");

	[SerializeField]
	private TransformResolver _bip001Transform = new TransformResolver("Bip001");

	private Vector3 _currentPosition;

	private TileObject _currentTileObject;

	private RidingStabilizer _ridingStabilizer;

	private Transform _meshObjectTransform;

	private bool _motionStarted;

	private bool _shouldCallOnChangePlayerPosition = true;

	private Pair<uint, double>? _playingMusic;

	private float? _musicEndedAt;

	private Pair<string, GameObject> _instrumentObject;

	private float _motionFadeTimer;

	private float _motionTransitionTime;

	private readonly PlayerEquipment _playerEquipment = new PlayerEquipment();

	private float _bodySize = 1f;

	private WaterRipple _rippleOnRiver;

	private WaterRipple _rippleOnOcean;

	private Transform _rootBone;

	private WeaponFramework _currentWeaponFramework = WeaponFramework.NONE;

	private readonly List<DrawLineBase> _drawLineBuffer = new List<DrawLineBase>();

	private GameObject _equipmentObj;

	private bool _equipmentVisible = true;

	private Animation _equipmentAnim;

	private string _loadingEquipmentPath;

	private string _equipmentPath;

	private ItemColor _equipmentColor;

	private int _portraitBg;

	private Color _portraitBgColor;

	private readonly CharacterCostume _costume = new CharacterCostume();

	private bool _prevHeadVisible;

	private ItemColor _prevBodyColor;

	private readonly Dictionary<Pair<string, string>, int> _effectDict = new Dictionary<Pair<string, string>, int>();

	private BoneFlinchingController _boneFlinchingController;

	private Renderer _mainRenderer;

	private bool _visible = true;

	private Transform _weaponTipTransform;

	private PathMovable _pathMovable;

	private AnimationClipInfo _curInfo;

	private int _lastAnimClipInfoCheckFrame;

	private float _prevMoveTime = -1f;

	private Vector3 _prevPosition = Vector3.zero;

	private PlayClipArgument _playClipArgument;

	private PlayerAnimationClipInfo _nextPlayerClipInfo;

	private ICoroutineBinder _playerAnimationCoroutineBinder;

	private readonly PlayerBufferTime _playerBufferTime = new PlayerBufferTime();

	private string _reservedMountTargetId;

	private readonly WaitForSeconds _waitForHalfSeconds = new WaitForSeconds(0.5f);

	private bool _isIndoor;

	private Animation _anim;

	public override BoneMergeable BoneMergeable
	{
		get
		{
			if (_boneMergeable == null)
			{
				_boneMergeable = new BoneMergeable(base.gameObject, MeshObjectTransform, _rootBone);
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
				_projectileController = new ProjectileController(_aimBasis, base.transform);
			}
			return _projectileController;
		}
	}

	public override Transform Bip001Transform => _bip001Transform;

	public static PlayerBehavior LocalPlayer
	{
		get
		{
			return _localPlayer;
		}
		set
		{
			if (!(_localPlayer == value) && !(value == null))
			{
				if (_localPlayer != null)
				{
					value.TransferEvent(_localPlayer);
				}
				_localPlayer = value;
				if (_localPlayer.Shadows != null)
				{
					_localPlayer.Shadows.RefreshOption();
				}
			}
		}
	}

	public string PlayerName { get; set; }

	public int Freq { get; set; }

	public string ClanId => Clan.ClanId;

	public bool HasClan
	{
		get
		{
			if (!string.IsNullOrEmpty(ClanId))
			{
				return Clan.RoleId != -1;
			}
			return false;
		}
	}

	public bool IsClanOwner
	{
		get
		{
			if (!string.IsNullOrEmpty(ClanId))
			{
				return Clan.RoleId == 0;
			}
			return false;
		}
	}

	public Member Clan { get; set; }

	public Title Title { get; set; }

	public BoneLookAtTarget LookAtController { get; private set; }

	public override Transform MeshObjectTransform => _meshObjectTransform;

	public bool IsLoaded
	{
		get
		{
			if (_costume.IsCostumeLoaded)
			{
				return _motionStarted;
			}
			return false;
		}
	}

	public override Vector3 CurrentPosition
	{
		get
		{
			if (MainTransform.hasChanged)
			{
				_currentPosition = MainTransform.localPosition;
				MainTransform.hasChanged = false;
			}
			return _currentPosition;
		}
		set
		{
			bool flag = _currentPosition != value;
			_currentPosition = value;
			MainTransform.localPosition = value;
			MainTransform.hasChanged = false;
			if (flag)
			{
				CheckCurrentTile();
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
			Vector3 vector = CurrentPosition + Vector3.up * 100f;
			if (IsRiding)
			{
				return Driver.CalcCameraOrigin(vector);
			}
			return vector;
		}
	}

	public GameObject Framework => _framework;

	public override Vector3 InteractionPosition => CameraOrigin;

	public Vector3 FloatingUIPosition
	{
		get
		{
			if (IsRiding && Driver.Vehicle != null)
			{
				return Driver.Vehicle.transform.position;
			}
			return CurrentPosition;
		}
	}

	[NotNull]
	public Driver Driver { get; private set; }

	public bool IsRiding => Driver.IsRiding;

	public bool IsLocalPlayer => _localPlayer == this;

	public SoundSwitch VoiceSoundSwitch { get; set; }

	public string DefaultBodyCostume => Display.DefaultBody;

	public string CurrentBodyCostume
	{
		get
		{
			if (string.IsNullOrEmpty(Display.Body))
			{
				return DefaultBodyCostume;
			}
			return Display.Body;
		}
	}

	public override Animation Anim => _anim;

	public WeaponFramework CurrentWeaponFramework => _currentWeaponFramework;

	[CanBeNull]
	public Gauge Stamina => GetGauge("stamina");

	[CanBeNull]
	public Gauge Fatigue => GetGauge("fatigue");

	public bool IsTired
	{
		get
		{
			if (Fatigue == null)
			{
				return false;
			}
			return GameSystem<FatigueSystem>.Instance().Fatigue.GetState() == Durango.Logic.Fatigue.State.Danger;
		}
	}

	public float SwimmableDepthRatio => GameSystem<StatisticsSystem>.Instance().GetDeriveds(Derived.Swimming) * 0.01f;

	public bool IsReceivingCPR { get; set; }

	public bool IsIndoor
	{
		get
		{
			return _isIndoor;
		}
		set
		{
			bool flag = value;
			if (Driver.IsHovering)
			{
				flag = false;
			}
			if (_isIndoor == flag)
			{
				return;
			}
			_isIndoor = flag;
			if (Shadows != null)
			{
				if (flag)
				{
					Shadows.SetVisible(visible: false, VisibleObject.Mask.Inside);
					ContactShadowModel contactShadowModel = Singleton<ContactShadowManager>.Instance().Create(base.gameObject, IsLocalPlayer, !IsLocalPlayer);
					contactShadowModel.FootShadowOffset.y = 10f;
					contactShadowModel.CenterShadowOffset.y = 10f;
				}
				else
				{
					Shadows.SetVisible(visible: true, VisibleObject.Mask.Inside);
					Singleton<ContactShadowManager>.Instance().Remove(base.gameObject);
				}
			}
			SoundManager.SetState(new SoundStates("house", (!flag) ? "outside" : "inside"));
			if (this.IsIndoorChanged != null)
			{
				this.IsIndoorChanged();
			}
		}
	}

	[NotNull]
	public WorldLineRenderer WorldLineRenderer { get; private set; }

	public ItemColor[] CostumeColors => _costume.CostumeColors;

	public bool IsPreview { get; set; }

	public bool OutlineEnabled
	{
		set
		{
			if (Outline != null)
			{
				Outline.SetVisible(value, VisibleObject.Mask.Enabled);
			}
		}
	}

	public override bool WillBeRendered => _mainRenderer.isVisible;

	public override Transform WeaponTipTransform => _weaponTipTransform;

	public bool RescueRequested { get; set; }

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

	public override bool IsAnimPlaying => Anim.isPlaying;

	public int PortraitType { get; private set; }

	public PlayerAnimationClipInfo CurrentPlayerClipInfo { get; private set; }

	public PlayerAnimationClipInfo LastPlayerClipInfo
	{
		get
		{
			if (_nextPlayerClipInfo != null)
			{
				return _nextPlayerClipInfo;
			}
			return CurrentPlayerClipInfo;
		}
	}

	public bool AttachedReady { get; private set; }

	public string MotionPrefix
	{
		get
		{
			if (IsMale)
			{
				return "M_";
			}
			return "F_";
		}
	}

	public bool AnimationEventProhibited => Display.Invisible;

	public bool IsMale => _isMale;

	public string SkinEffect
	{
		get
		{
			return _costume.GetSkinEffect();
		}
		set
		{
			_costume.SetSkinEffect(value);
		}
	}

	public event Action Started;

	public event Action MotionConditionChanged;

	public event Action<bool> VisibleChanged;

	public event Action IsIndoorChanged;

	public AnimationClipInfo GetCurrentAnimationClipInfo()
	{
		if (_lastAnimClipInfoCheckFrame == Time.frameCount)
		{
			return _curInfo;
		}
		_lastAnimClipInfoCheckFrame = Time.frameCount;
		if (string.IsNullOrEmpty(base.CurrentAnimClipName))
		{
			_curInfo = AnimationEventController.InvalidAnimationClipInfo;
			return _curInfo;
		}
		AnimationState animationState = Anim[base.CurrentAnimClipName];
		if (animationState == null || !animationState.enabled)
		{
			return AnimationEventController.InvalidAnimationClipInfo;
		}
		_curInfo.Name = base.CurrentAnimKeyName;
		_curInfo.State = animationState;
		return _curInfo;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public Vector3 GetCurrentPosition()
	{
		return CurrentPosition;
	}

	public void ChangeCostume(CharacterCostume.CostumeType type, string assetBundlePath)
	{
		_costume.ChangeCostume(type, assetBundlePath);
	}

	public string GetCostumeName(CharacterCostume.CostumeType type)
	{
		return _costume.GetCostumeName(type);
	}

	public void ChangeCostumeColor(CharacterCostume.CostumeType type, ItemColor color)
	{
		_costume.ChangeCostumeColor(type, color);
	}

	public ItemColor GetCostumeColor(CharacterCostume.CostumeType type)
	{
		return _costume.CostumeColors[(int)type];
	}

	public void ChangeAccessory(string bone, string path)
	{
		_costume.SetAccessoryModel(bone, path);
	}

	public void ChangeEquipment(string path)
	{
		_playerEquipment.ChangePath(path);
		RefreshEquipmentModel();
	}

	public string GetEquipmentName()
	{
		return _playerEquipment.GetCurrentPath();
	}

	public void ChangeEquipmentColor(ItemColor color)
	{
		_playerEquipment.ChangeColor(color);
		RefreshEquipmentModel();
	}

	public ItemColor GetEquipmentColor()
	{
		return _playerEquipment.GetCurrentColor();
	}

	AnimationState IMotionPlayable.GetCurAnimState()
	{
		if (Anim != null)
		{
			return Anim[base.CurrentAnimClipName];
		}
		return null;
	}

	float IMotionPlayable.Play(string motionName, bool loop, float beginTime, float playbackRate)
	{
		base.CurrentAnimClipName = motionName;
		_playerEquipment.ResetMotionEquipment();
		return PlayAnim(motionName, loop, beginTime, playbackRate, -1f);
	}

	float IMotionPlayable.CrossFade(string motionName, float fadeTime, bool loop, float beginTime, float playbackRate)
	{
		base.CurrentAnimClipName = motionName;
		_playerEquipment.ResetMotionEquipment();
		return PlayAnim(motionName, loop, beginTime, playbackRate, 0.1f);
	}

	WrapMode IMotionPlayable.GetWrapMode(string motionName)
	{
		AnimationClip clip = Anim.GetClip(motionName);
		if (!clip)
		{
			return WrapMode.Default;
		}
		return clip.wrapMode;
	}

	void IMotionPlayable.SetActivateRootMotion(bool active)
	{
		base.RootMotionMovable.SetActivateRootMotion(active);
	}

	protected override ChatableBase CreateChatableBase()
	{
		return new ChatablePlayer(this);
	}

	public bool HasAnimTag(PlayerAnimationClipTag clipTag)
	{
		return LastPlayerClipInfo?.HasAnimTag(clipTag) ?? false;
	}

	private float PlayAnim(string animationClipName, bool loop, float beginTime, float playbackRate, float transitionTime)
	{
		AnimationState animationState = Anim[animationClipName];
		animationState.wrapMode = (loop ? WrapMode.Loop : WrapMode.Default);
		animationState.enabled = true;
		if (Anim.IsPlaying(animationClipName) && !loop)
		{
			Anim.Stop(animationClipName);
		}
		if (transitionTime > 0f)
		{
			Anim.CrossFade(animationClipName, transitionTime);
			animationState.speed = playbackRate;
		}
		else
		{
			Anim.Play(animationClipName);
			animationState.speed = playbackRate;
		}
		if (beginTime > 0f)
		{
			animationState.time = beginTime;
		}
		return animationState.length;
	}

	public void ChangeGender(bool isMale)
	{
		_isMale = isMale;
	}

	protected new void Awake()
	{
		base.Awake();
		Init();
		Observable<TerrainWater.WaterDepthLevel> waterDepthLevel = base.WaterDepthLevel;
		waterDepthLevel.Changed = (Action<TerrainWater.WaterDepthLevel>)Delegate.Combine(waterDepthLevel.Changed, (Action<TerrainWater.WaterDepthLevel>)delegate
		{
			OnMotionConditionChanged();
		});
		WorldLineRenderer = GetComponent<WorldLineRenderer>();
		if (WorldLineRenderer == null)
		{
			WorldLineRenderer = base.gameObject.AddComponent<WorldLineRenderer>();
		}
		Driver = GetComponent<Driver>();
		_boneFlinchingController = base.gameObject.GetComponent<BoneFlinchingController>();
		LookAtController = base.gameObject.GetComponent<BoneLookAtTarget>();
		_ridingStabilizer = GetComponent<RidingStabilizer>();
		AnimationEventController component = base.gameObject.GetComponent<AnimationEventController>();
		if (component != null)
		{
			component.AnimEventMotionChanged += AnimEventMotionChanged;
		}
		_rippleOnRiver = new WaterRipple("Particle/FX_WaterRipple_Moving_01.prefab", isRiver: true);
		_rippleOnOcean = new WaterRipple("Particle/FX_WaterRipple_standing_01.prefab");
	}

	public void Init()
	{
		MainTransform = base.transform;
		if (!(_meshObjectTransform != null))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_framework);
			gameObject.name = "Reference";
			SetFramework(gameObject.transform);
		}
	}

	public void SetFramework(Transform newFramework)
	{
		if (MainTransform == null || _meshObjectTransform == newFramework)
		{
			return;
		}
		MainTransform.DestroyChildren();
		newFramework.parent = MainTransform;
		_anim = GetComponentInChildren<Animation>();
		Anim.wrapMode = WrapMode.Default;
		Anim.cullingType = AnimationCullingType.AlwaysAnimate;
		_meshObjectTransform = newFramework;
		_rootBone = _meshObjectTransform.Find("Bip001");
		if (_rootBone == null)
		{
			_rootBone = _meshObjectTransform.Find("Dummy_root");
		}
		_mainRenderer = _meshObjectTransform.Find("Body").gameObject.GetComponent<Renderer>();
		List<Transform> list = new List<Transform> { _meshObjectTransform };
		_meshObjectTransform.GetComponentsInChildren(list);
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		foreach (Transform item in list)
		{
			dictionary[item.name] = item;
		}
		JiggleBonesController component = MainTransform.GetComponent<JiggleBonesController>();
		if (component != null)
		{
			component.UpdateFramework(dictionary);
		}
		_aimBasis.Resolve(dictionary);
		_bip001Transform.Resolve(dictionary);
		TransformResolver[] bodyPartTransforms = _bodyPartTransforms;
		for (int i = 0; i < bodyPartTransforms.Length; i++)
		{
			bodyPartTransforms[i]?.Resolve(dictionary);
		}
		_costume.Init(_isMale, _meshObjectTransform.gameObject, GetComponent<JiggleBonesController>(), Shadows);
		_costume.ModelChanged += Costume_ModelChanged;
	}

	private void OnDisable()
	{
		_rippleOnRiver.Stop();
		_rippleOnOcean.Stop();
	}

	protected new void Start()
	{
		base.Start();
		if (IsLocalPlayer)
		{
			Outline.SetRendererQueueOffset(-1);
		}
		base.RootMotionMovable.SetLocalRootMotionYawMode(isIgnoreYaw: true);
		if (!IsPreview && this.Started != null)
		{
			this.Started();
		}
	}

	private void Update()
	{
		if (!IsPreview)
		{
			if (!IsLocalPlayer)
			{
				ProcessDrawLines();
			}
			if (!Singleton<PlayerController>.Instance().CutScenePlayMode)
			{
				PathMovable.Process();
			}
			UpdateVelocity();
			if (WillBeRendered)
			{
				ProcessWaterRipple();
			}
			ProjectileController.UpdateProjectiles();
			CheckMotionState();
			if (IsLocalPlayer)
			{
				ProcessAffectNearObject();
			}
			if (_playingMusic.HasValue && !IsPlayingMusic())
			{
				StopMusic();
			}
		}
	}

	private void LateUpdate()
	{
		if (WillBeRendered)
		{
			LateMotionUpdate();
			UpdateBodyScale();
			_boneFlinchingController.ForceUpdateFirst();
			BoneMergeable.UpdateBoneMergeSet();
			if (!IsPreview)
			{
				ProcessRootMotionMovements();
			}
		}
		else if (!base.IsAlive)
		{
			LateMotionUpdate();
		}
	}

	public override float ProcessWaterDepth(Vector3 pos)
	{
		if (Driver.IsHovering)
		{
			return 0f;
		}
		float num = base.ProcessWaterDepth(pos);
		if ((TerrainWater.WaterDepthLevel)base.WaterDepthLevel >= TerrainWater.WaterDepthLevel.Swim && !base.IsAlive && HasAnimTag(PlayerAnimationClipTag.Dead) && GetCurrentAnimationClipInfo().Time == 0f)
		{
			num -= 30f + Mathf.Sin(Time.time * 2f) * 5f;
		}
		return num;
	}

	private void Costume_ModelChanged()
	{
		if (Application.isPlaying)
		{
			if (Outline != null)
			{
				Outline.RefreshModel();
			}
			if (Shadows != null)
			{
				Shadows.RefreshModel();
			}
			if (!IsPreview && AmbientLighting != null)
			{
				AmbientLighting.SetupMaterials(Renderers);
			}
		}
	}

	private void TransferEvent(PlayerBehavior oldPlayer)
	{
		TransferEvent((CharacterBehavior)oldPlayer);
		Started += oldPlayer.Started;
		MotionConditionChanged += oldPlayer.MotionConditionChanged;
		VisibleChanged += oldPlayer.VisibleChanged;
		IsIndoorChanged += oldPlayer.IsIndoorChanged;
		oldPlayer.Started = null;
		oldPlayer.MotionConditionChanged = null;
		oldPlayer.VisibleChanged = null;
		oldPlayer.IsIndoorChanged = null;
		Driver.TransferEvent(oldPlayer.Driver);
	}

	private void ProcessWaterRipple()
	{
		Biome biome = GetBiome();
		if ((bool)base.IsMoving || IsRiding)
		{
			_rippleOnRiver.Stop();
			_rippleOnOcean.Stop();
		}
		else
		{
			_rippleOnRiver.Process(biome, base.WaterDepthLevel, CurrentPosition);
			_rippleOnOcean.Process(biome, base.WaterDepthLevel, CurrentPosition);
		}
	}

	public void ChangePortraitType(int type, int bg, Color bgColor)
	{
		PortraitBuilder.FillEmptyBackground(base.EntityId, ref bg, ref bgColor);
		PortraitType = type;
		_portraitBg = bg;
		_portraitBgColor = bgColor;
	}

	public float ChangeBodySize(float bodySize)
	{
		_bodySize = Mathf.Clamp(bodySize, 0.85f, 1.1f);
		return _bodySize;
	}

	public PortraitBuilder.Argument GetPortraitArgument()
	{
		if (string.IsNullOrEmpty(Display.PortraitIcon))
		{
			return PortraitBuilder.MakeArgument(PortraitType, _portraitBg, _portraitBgColor, IsMale, PortraitEmotion.Normal, _costume.CostumeColors[2][0], _costume.CostumeColors[3][0], _costume.CostumeColors[5][0], _costume.CostumeColors[6][0]);
		}
		PortraitBuilder.Argument result = default(PortraitBuilder.Argument);
		result.Preset = Display.PortraitIcon;
		return result;
	}

	public void SetParticleEffects([NotNull] Pair<string, string>[] effects)
	{
		List<Pair<string, string>> list = null;
		foreach (KeyValuePair<Pair<string, string>, int> item in _effectDict)
		{
			if (effects.IndexOf(item.Key) == -1)
			{
				ParticleManager.Stop(item.Value);
				if (list == null)
				{
					list = new List<Pair<string, string>>();
				}
				list.Add(item.Key);
			}
		}
		int i = 0;
		for (int num = effects.Length; i < num; i++)
		{
			Pair<string, string> key = effects[i];
			if (!_effectDict.ContainsKey(key))
			{
				int num2 = ParticleManager.Emit(base.gameObject, key.Item1, key.Item2);
				if (num2 != 0)
				{
					_effectDict.Add(key, num2);
				}
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				_effectDict.Remove(list[j]);
			}
		}
	}

	[ExposedInEditor(null)]
	public void SetVisible(bool visible)
	{
		if (_visible != visible)
		{
			_visible = visible;
			_costume.SetVisible(_visible);
			ApplyEquipmentVisible();
			if (Outline != null)
			{
				Outline.SetVisible(visible, VisibleObject.Mask.Render);
				Outline.SkipFade();
			}
			if (Shadows != null)
			{
				Shadows.SetVisible(visible, VisibleObject.Mask.Render);
			}
			if (this.VisibleChanged != null)
			{
				this.VisibleChanged(_visible);
			}
		}
	}

	public bool GetVisible()
	{
		return _visible;
	}

	public void ChangeWeaponType(WeaponFramework wt)
	{
		if (_currentWeaponFramework != wt)
		{
			_currentWeaponFramework = wt;
			OnMotionConditionChanged();
		}
	}

	private void OnMotionConditionChanged()
	{
		if (this.MotionConditionChanged != null)
		{
			this.MotionConditionChanged();
		}
	}

	private void AnimEventMotionChanged()
	{
		RefreshEquipmentModel();
	}

	public void ReEquipCurrentWeapon()
	{
		_playerEquipment.ResetMotionEquipment();
		RefreshEquipmentModel();
	}

	public void ChangeEquipmentWhileCurrentAnimation(string equipPath)
	{
		_playerEquipment.SetMotionEquipImmediately(equipPath);
		RefreshEquipmentModel();
	}

	private void RefreshEquipmentModel()
	{
		string path = _playerEquipment.GetCurrentPath();
		if (string.IsNullOrEmpty(path))
		{
			DetachEquipmentModel();
			UpdateWeaponTip();
		}
		else if (path == _equipmentPath)
		{
			ApplyEquipmentColor();
		}
		else
		{
			if (_loadingEquipmentPath == path)
			{
				return;
			}
			_loadingEquipmentPath = path;
			Singleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(GameObject), delegate(UnityEngine.Object asset)
			{
				if (!(this == null) && !(path != _loadingEquipmentPath))
				{
					_loadingEquipmentPath = null;
					DetachEquipmentModel();
					if ((GameObject)asset == null || string.IsNullOrEmpty(_playerEquipment.GetCurrentPath()))
					{
						UpdateWeaponTip();
					}
					else
					{
						AttachEquipmentModel((GameObject)UnityEngine.Object.Instantiate(asset), path);
					}
				}
			});
		}
	}

	private void ApplyEquipmentVisible()
	{
		if (!(_equipmentObj == null))
		{
			_equipmentObj.SetActive(_equipmentVisible && _visible);
			if (Outline != null)
			{
				Outline.RefreshModel();
			}
			if (Shadows != null)
			{
				Shadows.RefreshModel();
			}
		}
	}

	private void ApplyEquipmentColor(bool force = false)
	{
		ItemColor currentColor = _playerEquipment.GetCurrentColor();
		if (!(_equipmentObj == null) && (force || !(_equipmentColor == currentColor)))
		{
			SkinnedMeshRenderer componentInChildren = _equipmentObj.GetComponentInChildren<SkinnedMeshRenderer>();
			if (!(componentInChildren == null))
			{
				_equipmentColor = currentColor;
				CharacterCostume.ApplyColorToRenderer(currentColor, componentInChildren);
			}
		}
	}

	private void AttachEquipmentModel(GameObject equipObj, string equipPath)
	{
		if (equipObj.layer != base.gameObject.layer)
		{
			NGUITools.SetLayer(equipObj, base.gameObject.layer);
		}
		_equipmentObj = equipObj;
		_equipmentAnim = _equipmentObj.GetComponent<Animation>();
		_equipmentPath = equipPath;
		BoneMergeable.AttachBoneMerge(_equipmentObj);
		UpdateWeaponTip();
		ApplyEquipmentVisible();
		ApplyEquipmentColor(force: true);
		RefreshEquipmentAnim();
	}

	private void DetachEquipmentModel()
	{
		if (!(_equipmentObj == null))
		{
			_equipmentObj.transform.parent = null;
			BoneMergeable.DetachBoneMerge(_equipmentObj);
			UnityEngine.Object.Destroy(_equipmentObj);
			_equipmentObj = null;
			_equipmentAnim = null;
			_equipmentPath = null;
		}
	}

	private void UpdateWeaponTip()
	{
		if (_equipmentObj == null)
		{
			_weaponTipTransform = MeshObjectTransform;
			return;
		}
		GameObject gameObject = KUtility.FindObjectByName(_equipmentObj, "Weapon_Tip");
		if (gameObject != null)
		{
			_weaponTipTransform = gameObject.transform;
			return;
		}
		Transform transform = KUtility.FindTransformByName(_equipmentObj, "Attachment_RH");
		if ((bool)transform)
		{
			gameObject = new GameObject("Weapon_Tip");
			gameObject.transform.parent = transform;
			gameObject.transform.localPosition = new Vector3(-100f, 0f, 0f);
			_weaponTipTransform = gameObject.transform;
		}
		else
		{
			_weaponTipTransform = MeshObjectTransform;
		}
	}

	public void SetWeaponData(WeaponDisplayInfo weaponDisplayInfo)
	{
		ProjectileController.SetWeaponData(weaponDisplayInfo);
		if (string.IsNullOrEmpty(weaponDisplayInfo.WeaponFramework))
		{
			ChangeWeaponType(WeaponFramework.NONE);
			return;
		}
		WeaponFramework wt = weaponDisplayInfo.WeaponFramework.ToEnum(WeaponFramework.ONEHAND);
		ChangeWeaponType(wt);
	}

	private void RefreshEquipmentAnim()
	{
		if (_equipmentAnim == null || CurrentPlayerClipInfo == null)
		{
			return;
		}
		string equipAnimation = CurrentPlayerClipInfo.EquipAnimation;
		if (!string.IsNullOrEmpty(equipAnimation))
		{
			equipAnimation = MotionPrefix + equipAnimation;
			if (!(_equipmentAnim.GetClip(equipAnimation) == null))
			{
				_equipmentAnim.Play(equipAnimation);
			}
		}
	}

	protected override void OnTileChanged(Point2 prev, Point2 current)
	{
		CurrentBiome = Singleton<TerrainBase>.Instance().TilePositionToBiome(current);
		TileObject tileObject = Singleton<TerrainBase>.Instance().GetTileObject(current, warning: false);
		_currentTileObject = tileObject;
	}

	public void UpdateBodyScale()
	{
		MainTransform.localScale = Vector3.one * _bodySize;
		Transform transform = _bodyPartTransforms[1];
		if (!(transform == null))
		{
			transform.localScale = Vector3.one * ((!(_bodySize <= 1f)) ? (1f - (_bodySize - 1f) / 0.1f * 0.1f) : (1.1f - (_bodySize - 0.85f) / 0.15f * 0.1f));
		}
	}

	public override void TurnToYaw(float yaw, bool bSnap)
	{
		if (bSnap)
		{
			MainTransform.localRotation = Quaternion.Euler(0f, yaw, 0f);
			return;
		}
		float y = Mathf.MoveTowardsAngle(base.CurrentYaw, yaw, Time.deltaTime * 500f);
		MainTransform.localRotation = Quaternion.Euler(0f, y, 0f);
	}

	private void ProcessRootMotionMovements()
	{
		if (CurrentPlayerClipInfo != null && CurrentPlayerClipInfo.HasAnimTag(PlayerAnimationClipTag.RootMotion))
		{
			base.RootMotionMovable.LateUpdateRootMotion();
		}
		else if (_nextPlayerClipInfo != null && _nextPlayerClipInfo.HasAnimTag(PlayerAnimationClipTag.RootMotion))
		{
			base.RootMotionMovable.LateUpdateRootMotion();
		}
		else
		{
			base.RootMotionMovable.ResetRootMotionOffset();
		}
	}

	private void UpdateVelocity()
	{
		float time = Time.time;
		if (_prevMoveTime < 0f)
		{
			_prevMoveTime = time;
		}
		Vector3 vector = CurrentPosition - _prevPosition;
		base.IsMoving.Value = vector.sqrMagnitude > float.Epsilon;
		if ((bool)base.IsMoving)
		{
			float num = time - _prevMoveTime;
			_prevMoveTime = time;
			if (num > 0f)
			{
				base.CurrentVelocity = vector / num;
				_prevPosition = CurrentPosition;
			}
		}
		else
		{
			base.CurrentVelocity = Vector3.zero;
		}
	}

	private void ProcessDrawLines()
	{
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
				Vector3 worldPos = Util.WorldPositionToClientPosition(drawLineBase.Position);
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

	public override void OnTakeDamage(Damage damage, DamageableEntity attacker)
	{
		base.OnTakeDamage(damage, attacker);
		if (base.IsAlive)
		{
			if (IsLocalPlayer && damage.Value > 0)
			{
				SoundManager.PlayEvent("v_damage_exception", SoundPosition.Chase(base.gameObject), VoiceSoundSwitch);
			}
			if (attacker != null && (bool)LookAtController)
			{
				LookAtController.SetLookTarget(attacker.GameObject, findHead: true);
			}
		}
		if (damage.Value > 0)
		{
			TakeBoneFlinching(damage.Part);
		}
	}

	public override void TakeBoneFlinching(BodyPart part)
	{
		if (null != _boneFlinchingController)
		{
			_boneFlinchingController.TakeBoneFlinching(GetBodyPartTransform(BodyPart.Back));
		}
	}

	protected override void OnDie(bool fromInit)
	{
		base.OnDie(fromInit);
		OnMotionConditionChanged();
		if (!fromInit)
		{
			SoundManager.PlayEvent("v_die_exception", SoundPosition.Chase(base.gameObject), VoiceSoundSwitch);
		}
		if (LastAttacker != null)
		{
			CharacterDamageableEntity characterDamageableEntity = LastAttacker as CharacterDamageableEntity;
			if (characterDamageableEntity != null)
			{
				characterDamageableEntity.OwnerComponent.OnKilledPlayer(this);
			}
		}
	}

	protected override void OnRevive()
	{
		base.OnRevive();
		OnMotionConditionChanged();
	}

	public void SetMusician(Musician? musician)
	{
		if (!musician.HasValue || !musician.Value.Music.HasValue || !musician.Value.PlayedAt.HasValue)
		{
			StopMusic();
			return;
		}
		Messages.Music value = musician.Value.Music.Value;
		double? playedAt = musician.Value.PlayedAt;
		float num = (float)((!playedAt.HasValue) ? null : new double?(Connections.Frontend.GetBufferedServerTime() - playedAt.GetValueOrDefault())).Value;
		float num2 = value.Duration - num;
		if (num2 <= 0f)
		{
			StopMusic();
			return;
		}
		if (_playingMusic.HasValue)
		{
			if (_playingMusic.Value.Item2 == musician.Value.PlayedAt.Value)
			{
				return;
			}
			MusicManager.StopMidi(_playingMusic.Value.Item1);
			_playingMusic = null;
		}
		_musicEndedAt = null;
		string timbre = musician.Value.Timbre;
		Durango.Logic.Music.Music music = Durango.Logic.Music.Music.Create(musician.Value.Music.Value);
		uint num3 = MusicManager.PlayMidi(timbre, music, SoundPosition.Chase(base.gameObject), num);
		if (num3 == 0)
		{
			return;
		}
		_playingMusic = new Pair<uint, double>(num3, musician.Value.PlayedAt.Value);
		_musicEndedAt = Time.time + num2;
		SetInstrument(timbre);
		if (IsLocalPlayer)
		{
			if (IsRiding)
			{
				Driver.Unmount(null, immediately: true);
				Connections.Frontend.Send(default(Unmount));
			}
			GameSystem<InputSystem>.Instance().MoveLockTimer(num2);
			string text = null;
			switch (timbre)
			{
			case "guitar":
				text = "Play_Guitar_A";
				break;
			case "xylophone":
				text = "Play_Synth";
				break;
			case "horn":
				text = "Play_Horn";
				break;
			case "bass":
				text = "Play_Guitar_B";
				break;
			case "drum":
				text = "Play_Drum_A";
				break;
			case "smalldrum":
				text = "Play_Drum_B";
				break;
			case "pianoelec":
				text = "Play_Keyboard";
				break;
			case "piano":
				text = "Play_Keyboard";
				break;
			}
			if (!string.IsNullOrEmpty(text))
			{
				PlayerController.MotionUpdater.Motion(text, num2, 1f, forceTransition: true);
			}
		}
	}

	public void StopMusic()
	{
		SetInstrument(null);
		Pair<uint, double>? playingMusic = _playingMusic;
		if (!playingMusic.HasValue)
		{
			_musicEndedAt = null;
			return;
		}
		MusicManager.StopMidi(_playingMusic.Value.Item1);
		if (IsLocalPlayer)
		{
			bool num = _musicEndedAt.HasValue && Time.time > _musicEndedAt.Value - 1f;
			_playingMusic = null;
			_musicEndedAt = null;
			PlayerController.MotionUpdater.ClearReservedMotions();
			PlayerController.MotionUpdater.Motion("Stand");
			GameSystem<InputSystem>.Instance().MoveLock = false;
			if (num)
			{
				UIManager.SystemMsg(T._("만족스러운 연주였다."));
			}
		}
		else
		{
			_playingMusic = null;
			_musicEndedAt = null;
		}
	}

	public bool IsPlayingMusic()
	{
		if (_musicEndedAt.HasValue)
		{
			return Time.time < _musicEndedAt.Value;
		}
		return false;
	}

	private void SetInstrument(string timbre)
	{
		if (_instrumentObject.Item1 == timbre)
		{
			return;
		}
		MusicManager.Instrument instrument = Singleton<MusicManager>.Instance().GetInstrument(timbre);
		if ((bool)_instrumentObject.Item2)
		{
			UnityEngine.Object.Destroy(_instrumentObject.Item2);
		}
		_instrumentObject = new Pair<string, GameObject>(timbre, null);
		string text = instrument?.InstrumentObject.Path;
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		Singleton<AssetBundleManager>.Instance().RequestAsset(text, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (this == null)
			{
				_instrumentObject = new Pair<string, GameObject>(null, null);
			}
			else if (!(_instrumentObject.Item1 != timbre))
			{
				if ((GameObject)asset == null)
				{
					_instrumentObject = new Pair<string, GameObject>(null, null);
				}
				else
				{
					Vector3 position = base.transform.position;
					position.y = (byte)base.Floor * 200;
					GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(asset);
					gameObject.transform.SetPositionAndRotation(position, base.transform.rotation);
					_instrumentObject = new Pair<string, GameObject>(timbre, gameObject);
				}
			}
		});
	}

	public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3))
	{
		if (_bodyPartTransforms.Length <= (int)part)
		{
			if (bAllowNull)
			{
				return null;
			}
			return MainTransform;
		}
		Transform transform = _bodyPartTransforms[(int)part];
		if (null != transform)
		{
			return transform;
		}
		if (bAllowNull)
		{
			return null;
		}
		return MainTransform;
	}

	public override string GetName()
	{
		if (string.IsNullOrEmpty(PlayerName))
		{
			return T._("알수없음");
		}
		return PlayerName;
	}

	public void SetEquipmentVisible(bool visible)
	{
		if (_equipmentVisible != visible)
		{
			_equipmentVisible = visible;
			ApplyEquipmentVisible();
		}
	}

	[UsedImplicitly]
	private void Cmd_PutOnInnerCostume()
	{
		_prevBodyColor = _costume.CostumeColors[0];
		_costume.ChangeCostume(CharacterCostume.CostumeType.Body, DefaultBodyCostume);
		Color32 color = new Color32(208, 208, 208, byte.MaxValue);
		_costume.ChangeCostumeColor(CharacterCostume.CostumeType.Body, new ItemColor(color, color, color));
		_prevHeadVisible = _costume.GetCostumeVisible(CharacterCostume.CostumeType.Head);
		_costume.SetCostumeVisible(CharacterCostume.CostumeType.Head, isVisible: false);
		_costume.SetAccessoryVisible(visible: false);
	}

	[UsedImplicitly]
	private void Cmd_PutOnCurrentCostume()
	{
		_costume.ChangeCostume(CharacterCostume.CostumeType.Body, CurrentBodyCostume);
		_costume.ChangeCostumeColor(CharacterCostume.CostumeType.Body, _prevBodyColor);
		_costume.SetAccessoryVisible(visible: true);
		if (_prevHeadVisible)
		{
			_costume.SetCostumeVisible(CharacterCostume.CostumeType.Head, isVisible: true);
		}
	}

	[UsedImplicitly]
	private void Cmd_PlayPropAnimation(string targetAnimationName)
	{
		AnimatingModel currentTileComponent = GetCurrentTileComponent<AnimatingModel>();
		if (currentTileComponent != null)
		{
			currentTileComponent.Play(targetAnimationName);
		}
	}

	[UsedImplicitly]
	private void Cmd_AttachToProp()
	{
		PlayerAttachedProp currentTileComponent = GetCurrentTileComponent<PlayerAttachedProp>();
		if (currentTileComponent != null)
		{
			currentTileComponent.Attach(this);
		}
		AttachedReady = true;
	}

	[UsedImplicitly]
	private void Cmd_DetachFromProp()
	{
		PlayerAttachedProp currentTileComponent = GetCurrentTileComponent<PlayerAttachedProp>();
		if (currentTileComponent != null)
		{
			currentTileComponent.Detach(this, snapToExit: true);
		}
		AttachedReady = false;
	}

	[CanBeNull]
	public TileObject GetTileObject(bool reloadIfNull = false)
	{
		if (_currentTileObject == null && reloadIfNull)
		{
			CheckCurrentTile();
			_currentTileObject = Singleton<TerrainBase>.Instance().GetTileObject(base.CurrentTile, warning: false);
		}
		return _currentTileObject;
	}

	[CanBeNull]
	private TC GetCurrentTileComponent<TC>() where TC : MonoBehaviour
	{
		TileObject tileObject = GetTileObject(reloadIfNull: true);
		if (tileObject == null)
		{
			return null;
		}
		Artifact artifact = tileObject.Artifact;
		if (artifact == null)
		{
			return null;
		}
		Artifact interior = artifact.GetInterior(base.CurrentTile - artifact.WorldTile, (byte)base.Floor);
		if (interior != null)
		{
			artifact = interior;
		}
		TC componentInChildren = artifact.GetComponentInChildren<TC>();
		if (componentInChildren != null)
		{
			return componentInChildren;
		}
		return null;
	}

	protected override void ProcessAffectNearObject()
	{
		if (!Driver.IsHovering)
		{
			bool isBushWhacking = base.IsBushWhacking;
			bool isRoadRunning = base.IsRoadRunning;
			base.ProcessAffectNearObject();
			if (isBushWhacking != base.IsBushWhacking || isRoadRunning != base.IsRoadRunning)
			{
				OnMotionConditionChanged();
			}
		}
	}

	[UsedImplicitly]
	private void OnChargedProjectile()
	{
		ProjectileController.ChargedProjectile();
	}

	[UsedImplicitly]
	private void OnShootProjectile()
	{
		ProjectileController.ShootProjectile();
	}

	[UsedImplicitly]
	private void OnAttack()
	{
	}

	private void MovementProcessed(Movement movement)
	{
		if (!string.IsNullOrEmpty(movement.MotionName))
		{
			string motionName = movement.MotionName;
			PlayerAnimationClipInfo playerAnimationClipInfo = Singleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationClipInfo(motionName);
			if (playerAnimationClipInfo != null)
			{
				TryPlayClip(playerAnimationClipInfo, movement.PlaybackRate);
			}
		}
	}

	public void HandleMoveMsg(Move msg)
	{
		_playerBufferTime.MoveReceived(msg);
		PathMovable.HandleMoveMsg(msg);
	}

	private void CheckMotionState()
	{
		if (_motionFadeTimer > 0f)
		{
			_motionFadeTimer -= Time.deltaTime;
			if (_motionFadeTimer <= 0f)
			{
				OnMotionChangeFinished();
			}
		}
	}

	private void OnMotionChangeFinished()
	{
		CurrentPlayerClipInfo = _nextPlayerClipInfo;
		_nextPlayerClipInfo = null;
		RefreshEquipmentModel();
		RefreshEquipmentAnim();
	}

	private void LateMotionUpdate()
	{
		if (_playClipArgument.IsValid)
		{
			string playAnimationClipName = _playClipArgument.PlayAnimationClipName;
			float motionTransitionTime = _motionTransitionTime;
			if (!Singleton<PlayerController>.Instance().CutScenePlayMode)
			{
				PlayAnim(playAnimationClipName, _playClipArgument.ClipInfo.IsLoop, 0f, _playClipArgument.PlaybackRate, motionTransitionTime);
				_ridingStabilizer.SetActive(_playClipArgument.ClipInfo.HasAnimTag(PlayerAnimationClipTag.SpineStabilize));
			}
			_playClipArgument.Reset();
			if (!_motionStarted)
			{
				_motionStarted = true;
			}
		}
	}

	public void PlayStateForcely(string stateName, float playbackRate = 1f, bool immediately = false)
	{
		string text = Singleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationClip(stateName, (int)_currentWeaponFramework);
		if (string.IsNullOrEmpty(text))
		{
			text = "Barehand_Stand";
		}
		PlayMotionForcely(text, playbackRate, immediately);
	}

	public void PlayMotionForcely(string clipName, float playbackRate = 1f, bool immediately = false)
	{
		PlayerAnimationClipInfo playerAnimationClipInfo = Singleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationClipInfo(clipName);
		if (playerAnimationClipInfo != null)
		{
			TryPlayClip(playerAnimationClipInfo, playbackRate);
			if (immediately)
			{
				LateMotionUpdate();
			}
		}
	}

	public void PlayMotionsForcely(float playbackRate, params string[] clipNames)
	{
		if (clipNames != null)
		{
			IEnumerable<PlayerAnimationClipInfo> clips = clipNames.Select((string elem) => Singleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationClipInfo(elem));
			this.StartCoroutine(ref _playerAnimationCoroutineBinder, PlayAnimationClipsSequence(clips, playbackRate));
		}
	}

	private IEnumerator PlayAnimationClipsSequence(IEnumerable<PlayerAnimationClipInfo> clips, float playBackRates)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CPlayAnimationClipsSequence_003Ed__295(0)
		{
			_003C_003E4__this = this,
			clips = clips,
			playBackRates = playBackRates
		};
	}

	private bool TryPlayClip([NotNull] PlayerAnimationClipInfo clipInfo, float playbackRate)
	{
		string text = MotionPrefix + clipInfo.Clip;
		float num = ((clipInfo.FadeInTime != 0f) ? Mathf.Max((CurrentPlayerClipInfo != null) ? CurrentPlayerClipInfo.FadeOutTime : (-1f), clipInfo.FadeInTime) : 0f);
		if (!WillBeRendered)
		{
			num = 0f;
		}
		else if (num < 0f)
		{
			num = 0.1f;
		}
		if (Anim[text] != null)
		{
			_playClipArgument.Set(text);
			_playClipArgument.PlaybackRate = playbackRate;
			_playClipArgument.ClipInfo = clipInfo;
			base.CurrentAnimClipName = text;
			_nextPlayerClipInfo = clipInfo;
			_motionTransitionTime = num;
			_motionFadeTimer = num;
			_playerEquipment.AnimMotionChanged();
			if (_motionFadeTimer == 0f)
			{
				OnMotionChangeFinished();
			}
			return true;
		}
		return false;
	}

	public void ReserveMotionEquipment(string equipment = null, ItemColor equipColor = default(ItemColor))
	{
		if (equipment != null)
		{
			_playerEquipment.ReserveMotionEquipment(equipment, equipColor);
		}
	}

	public override double GetMoveServerTime()
	{
		if (GameManager.IsPrologueMode)
		{
			return Time.time;
		}
		double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
		if (IsLocalPlayer)
		{
			return Connections.Frontend.GetPredictedServerTime();
		}
		return bufferedServerTime - (double)_playerBufferTime.BufferTime;
	}

	public void SetBoardingOn(BoardingOn boardingOn, string vehicleEntityId, bool fromAppear)
	{
		switch (boardingOn)
		{
		case BoardingOn.AirBalloon:
			if (IsLocalPlayer)
			{
				MountAirBalloon(fromAppear);
			}
			break;
		case BoardingOn.Pet:
		case BoardingOn.Vehicle:
			_reservedMountTargetId = vehicleEntityId;
			StartCoroutine(CoReservedMountTarget());
			break;
		case BoardingOn.None:
			if (Driver.IsRiding)
			{
				Driver.Unmount();
			}
			_reservedMountTargetId = string.Empty;
			break;
		}
	}

	private void MountAirBalloon(bool fromAppear)
	{
		if (Driver.IsVehicleKindOf<VehicleAirBalloon>())
		{
			if (!Driver.IsRiding)
			{
				Driver.Mount(Driver.Vehicle);
			}
			return;
		}
		VehicleAirBalloon.Spawn(MainTransform.position, delegate(VehicleAirBalloon vehicle)
		{
			KUtility.DelayedCall(this, delegate
			{
				Driver.Mount(vehicle);
				if (fromAppear)
				{
					vehicle.StartInTheAir();
				}
			}, 0f);
		});
	}

	private bool TryMountTarget(string targetId)
	{
		VehicleBase vehicleBase = Singleton<ObjectManager>.Instance().FindVehicle(targetId);
		if (vehicleBase != null)
		{
			Driver.Mount(vehicleBase);
			return true;
		}
		return false;
	}

	private IEnumerator CoReservedMountTarget()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoReservedMountTarget_003Ed__302(0)
		{
			_003C_003E4__this = this
		};
	}
}
