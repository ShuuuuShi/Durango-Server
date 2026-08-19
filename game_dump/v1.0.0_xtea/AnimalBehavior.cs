using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using L10N;
using Messages;
using NetworkEnums;
using Shared.Battle;
using UnityEngine;
using Yaml;

public class AnimalBehavior : CharacterBehavior, IAnimationEventPlayable, IMotionPlayable, IMeshCloner
{
	private enum EyeStatus
	{
		Normal,
		Closed,
		Night
	}

	private const float InitRotateSpeed = 300f;

	[SerializeField]
	private string _motionName;

	[SerializeField]
	public AnimationClipResource AnimationClipResource;

	[SerializeField]
	private bool _playAnimationOnStart = true;

	[SerializeField]
	private int _pauseAtFrame = -1;

	[SerializeField]
	private float _animationStartDelay = -1f;

	[SerializeField]
	private string _bodyPartHead = "Bip001_Head";

	[SerializeField]
	private string _bodyPartBody = "Bip001";

	[SerializeField]
	private string _bodyPartArm = "Bip001_L_Hand";

	[SerializeField]
	private string _bodyPartArmR = "Bip001_R_Hand";

	[SerializeField]
	private string _bodyPartLeg = "Bip001_L_Foot";

	[SerializeField]
	private string _bodyPartLegR = "Bip001_R_Foot";

	[SerializeField]
	private string _bodyPartTail = "Bip001_Tail3";

	[SerializeField]
	private string _bodyPartBack = "Bip001_Spine";

	[SerializeField]
	private string _bodyPartBip001 = "Bip001";

	[SerializeField]
	private List<string> _collidableJointNames = new List<string>();

	private List<GameObject> _collidableJoints = new List<GameObject>();

	[SerializeField]
	private float _collisionRadius = 50f;

	[SerializeField]
	private bool _isCarnivore;

	[SerializeField]
	private bool _shadowOff;

	[SerializeField]
	private bool _processDepth = true;

	[SerializeField]
	private Vector2 _defaultPitchOverride;

	[SerializeField]
	private Renderer[] _closedEyes;

	[SerializeField]
	private GameObject[] _nightEyes;

	protected bool ShowSelectEffect = true;

	private float _rotateSpeed = 300f;

	private float _prevMoveTime = -1f;

	private Vector3 _prevPosition;

	private float _curYaw;

	private float _destYaw;

	private BoneFlinchingController _boneFlinchingController;

	private BoneLookAtTarget _lookAtController;

	private float _fadeBeginTime = -1f;

	private float _prevShakeTime;

	private AnimationBlendingController _animBlendingController;

	private float _prevDebugShowPathTime;

	private Renderer _mainRenderer;

	private bool _isFirstFade = true;

	private bool _needToCallOnDieAtTakeDamage;

	private AnimationClipInfo _curInfo;

	private string _lastMotionName;

	private readonly RendererProxy _rendererProxy = new RendererProxy();

	private Color _color = Color.white;

	private Color _colorTint = Color.white;

	private Color _tensionColor = Color.white;

	private EyeStatus _eyeStatus;

	private float _eyeClosedRatio;

	private float _nextEyeCloseTime;

	private Transform _headTransform;

	private Transform _bodyTransform;

	private Transform _leftArmTransform;

	private Transform _rightArmTransform;

	private Transform _leftLegTransform;

	private Transform _rightLegTransform;

	private Transform _tailTransform;

	private Transform _backTransform;

	private Transform _bip001Transform;

	[SerializeField]
	[HideInInspector]
	private GameObject _meshObject;

	private Transform _meshObjectTransform;

	private Transform _rootBone;

	private PathMovable _pathMovable;

	private Animation _anim;

	private BoneMergeable _boneMergeable;

	private GameObject _markingParticle;

	private double _markerGaugeEndTime = -1.0;

	private GameObject _groggyParticle;

	private AnimationState _curAnimState;

	private string _curAnimStateName;

	private string _currentAnimEventName;

	private bool _isAimTarget;

	private bool _selected;

	private float _selectedAt;

	private float _glitterAt;

	private string _animalName;

	private static readonly string[] DeathMsgAttackAnimalList = new string[5]
	{
		T.N_("에게 물어뜯겨"),
		T.N_(" 발톱에 찍혀"),
		T.N_(" 꼬리에 맞아"),
		T.N_(" 엉덩이에 깔려"),
		T.N_(" 발에 차여")
	};

	private Coroutine _curChangeColorCoroutine;

	public override bool IsVisible
	{
		get
		{
			if ((Object)(object)_mainRenderer == (Object)null)
			{
				_mainRenderer = (Renderer)(object)((Component)this).gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
			}
			return (Object)(object)_mainRenderer == (Object)null || _mainRenderer.isVisible;
		}
	}

	private Transform HeadTransform
	{
		get
		{
			if ((Object)(object)_headTransform == (Object)null)
			{
				_headTransform = FindTransformByName(_bodyPartHead);
			}
			return _headTransform;
		}
	}

	private Transform BodyTransform
	{
		get
		{
			if ((Object)(object)_bodyTransform == (Object)null)
			{
				_bodyTransform = FindTransformByName(_bodyPartBody);
			}
			return _bodyTransform;
		}
	}

	private Transform LeftArmTransform
	{
		get
		{
			if ((Object)(object)_leftArmTransform == (Object)null)
			{
				_leftArmTransform = FindTransformByName(_bodyPartArm);
			}
			return _leftArmTransform;
		}
	}

	private Transform RightArmTransform
	{
		get
		{
			if ((Object)(object)_rightArmTransform == (Object)null)
			{
				_rightArmTransform = FindTransformByName(_bodyPartArmR);
			}
			return _rightArmTransform;
		}
	}

	private Transform LeftLegTransform
	{
		get
		{
			if ((Object)(object)_leftLegTransform == (Object)null)
			{
				_leftLegTransform = FindTransformByName(_bodyPartLeg);
			}
			return _leftLegTransform;
		}
	}

	private Transform RightLegTransform
	{
		get
		{
			if ((Object)(object)_rightLegTransform == (Object)null)
			{
				_rightLegTransform = FindTransformByName(_bodyPartLegR);
			}
			return _rightLegTransform;
		}
	}

	private Transform TailTransform
	{
		get
		{
			if ((Object)(object)_tailTransform == (Object)null)
			{
				_tailTransform = FindTransformByName(_bodyPartTail);
			}
			return _tailTransform;
		}
	}

	private Transform BackTransform
	{
		get
		{
			if ((Object)(object)_backTransform == (Object)null)
			{
				_backTransform = FindTransformByName(_bodyPartBack);
			}
			return _backTransform;
		}
	}

	public override Transform Bip001Transform
	{
		get
		{
			if ((Object)(object)_bip001Transform == (Object)null)
			{
				_bip001Transform = FindTransformByName(_bodyPartBip001);
			}
			return _bip001Transform;
		}
	}

	public bool IsCarnivore => _isCarnivore;

	public GameObject MeshObject
	{
		get
		{
			if ((Object)(object)_meshObject != (Object)null)
			{
				return _meshObject;
			}
			Animation componentInChildren = ((Component)this).gameObject.GetComponentInChildren<Animation>(true);
			if ((Object)(object)componentInChildren == (Object)null)
			{
				return null;
			}
			_meshObject = ((Component)componentInChildren).gameObject;
			return _meshObject;
		}
	}

	public override Transform MeshObjectTransform
	{
		get
		{
			if ((Object)(object)_meshObjectTransform == (Object)null)
			{
				_meshObjectTransform = MeshObject.transform;
			}
			return _meshObjectTransform;
		}
	}

	protected Transform RootBone
	{
		get
		{
			if ((Object)(object)_rootBone == (Object)null)
			{
				_rootBone = MeshObjectTransform.FindChild("Bip001");
			}
			return _rootBone;
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

	public bool IsCompletelyFadedOut => _color.a <= 0f;

	public Animation Anim
	{
		get
		{
			if ((Object)(object)_anim == (Object)null)
			{
				_anim = MeshObject.GetComponent<Animation>();
			}
			return _anim;
		}
	}

	public Vector2 DefaultPitchOverride => _defaultPitchOverride;

	public override BoneMergeable BoneMergeable
	{
		get
		{
			if (_boneMergeable == null)
			{
				_boneMergeable = new BoneMergeable(((Component)this).gameObject, this, MeshObjectTransform, RootBone);
			}
			return _boneMergeable;
		}
	}

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

	public override Vector3 CurrentPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Component)this).transform.position = value;
		}
	}

	public AnimationState CurAnimState
	{
		get
		{
			return _curAnimState;
		}
		set
		{
			_curAnimState = value;
			_curAnimStateName = ((!((TrackedReference)(object)_curAnimState == (TrackedReference)null)) ? _curAnimState.name : null);
			if (string.IsNullOrEmpty(_curAnimStateName))
			{
				_currentAnimEventName = _curAnimStateName;
			}
			else if (_curAnimStateName.StartsWith("M_") || _curAnimStateName.StartsWith("F_"))
			{
				_currentAnimEventName = _curAnimStateName.Substring(2, _curAnimStateName.Length - 2);
			}
			else
			{
				_currentAnimEventName = _curAnimStateName;
			}
		}
	}

	public override bool IsAimTarget
	{
		get
		{
			return _isAimTarget;
		}
		set
		{
			bool flag = _isAimTarget != value;
			_isAimTarget = value;
			if (flag)
			{
				UpdateOutline();
			}
		}
	}

	public override ChatableBase ChatableBase
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	private void Awake()
	{
		LoadAnimationClips();
	}

	public void LoadAnimationClips()
	{
		if ((Object)(object)AnimationClipResource != (Object)null)
		{
			int i = 0;
			for (int size = KUtility.GetSize(AnimationClipResource.Clips); i < size; i++)
			{
				AnimationClip val = AnimationClipResource.Clips[i];
				Anim.AddClip(val, ((Object)val).name);
			}
		}
	}

	public void ClearAnimationClips()
	{
		if ((Object)(object)AnimationClipResource != (Object)null)
		{
			int i = 0;
			for (int size = KUtility.GetSize(AnimationClipResource.Clips); i < size; i++)
			{
				AnimationClip val = AnimationClipResource.Clips[i];
				Anim.RemoveClip(((Object)val).name);
			}
		}
	}

	private void Start()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		AddMeshCloners(((Component)this).GetComponentsInChildren<SkinnedMeshRenderer>());
		ParticleManager.Cache("Particle/FX_BloodBurst_Strike_01.prefab");
		ParticleManager.Cache("Particle/FX_Marking_01.prefab");
		ParticleManager.Cache("Particle/FX_Stunned_01.prefab");
		_boneFlinchingController = ((Component)this).gameObject.GetComponent<BoneFlinchingController>();
		_lookAtController = ((Component)this).gameObject.GetComponent<BoneLookAtTarget>();
		_collidableJoints.Clear();
		for (int i = 0; i < _collidableJointNames.Count; i++)
		{
			string objectName = _collidableJointNames[i];
			GameObject item = FindObjectByName(objectName);
			_collidableJoints.Add(item);
		}
		Anim.cullingType = (AnimationCullingType)1;
		base.CurrentVelocity = Vector3.zero;
		_animBlendingController = ((Component)this).GetComponent<AnimationBlendingController>();
		if (!((Object)(object)_animBlendingController != (Object)null) || !_animBlendingController.ReadClipJson())
		{
		}
		TryPlayDefaultMotion();
	}

	private void TryPlayDefaultMotion()
	{
		if (Connections.Frontend.Connected() || string.IsNullOrEmpty(_motionName) || !_playAnimationOnStart)
		{
			return;
		}
		if (_pauseAtFrame >= 0)
		{
			float num = (float)_pauseAtFrame / 30f;
			float beginTime = num;
			Play(_motionName, loop: true, beginTime, 0f);
			Anim.Sample();
		}
		else
		{
			if (_animationStartDelay < 0f)
			{
				_animationStartDelay = Random.Range(0f, 2f);
			}
			((MonoBehaviour)this).StartCoroutine(DelayedPlay(_motionName, _animationStartDelay));
		}
	}

	public void AddMeshCloners(SkinnedMeshRenderer[] renderers)
	{
		if (renderers != null)
		{
			SetupOutline(renderers);
			SetupShadows(renderers);
			SetupAmbientLighting(renderers);
			_rendererProxy.UpdateRenderers((IList<Renderer>)(object)renderers);
		}
	}

	private void SetupOutline(SkinnedMeshRenderer[] renderers)
	{
		if (!GameManager.IsPrologueMode)
		{
			Outline component = ((Component)this).GetComponent<Outline>();
			if ((Object)(object)component != (Object)null)
			{
				component.Add(renderers);
			}
		}
	}

	private void SetupShadows(SkinnedMeshRenderer[] renderers)
	{
		if (_shadowOff)
		{
			return;
		}
		if (GameManager.IsPrologueMode)
		{
			KSingleton<ContactShadowManager>.Instance().Create(((Component)this).gameObject);
			return;
		}
		PlaneShadows component = ((Component)this).GetComponent<PlaneShadows>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.Add(renderers);
		}
	}

	private void SetupAmbientLighting(SkinnedMeshRenderer[] renderers)
	{
		AmbientLighting component = ((Component)this).GetComponent<AmbientLighting>();
		if ((Object)(object)component != (Object)null)
		{
			component.UpdateMaterials(renderers);
		}
	}

	public void RemoveMeshCloners(SkinnedMeshRenderer[] renderers)
	{
		Outline component = ((Component)this).GetComponent<Outline>();
		if ((Object)(object)component != (Object)null)
		{
			component.Remove(renderers);
		}
		PlaneShadows component2 = ((Component)this).GetComponent<PlaneShadows>();
		if ((Object)(object)component2 != (Object)null)
		{
			component2.Remove(renderers);
		}
	}

	public void SetColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_colorTint = color;
	}

	private void Update()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (_prevShakeTime + 1f < Time.time)
		{
			SelectShakeTrees();
			_prevShakeTime = Time.time;
		}
		PreProcessMovements();
		ProcessMovements();
		ProcessRotation();
		CheckCurrentTile();
		ProcessSelected();
		ProcessMarkingParticle();
		ProcessMotionStateAffectedByObject();
		ProcessEyes();
		_rendererProxy.Color = _color * _colorTint * _tensionColor;
	}

	protected void LateUpdate()
	{
		if (IsVisible)
		{
			if (Object.op_Implicit((Object)(object)_lookAtController) && ((Behaviour)_lookAtController).enabled)
			{
				_lookAtController.ForaceUpdate();
			}
			ProcessDepth();
			base.RootMotionMovable.LateUpdateRootMotion(MeshObjectTransform);
			BoneMergeable.UpdateBoneMergeSet();
		}
		if (IsCompletelyFadedOut)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void ProcessSelected()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		if (!ShowSelectEffect)
		{
			return;
		}
		float time = Time.time;
		if (_fadeBeginTime >= 0f && time >= _fadeBeginTime && _color.a > 0f)
		{
			ref Color color = ref _color;
			color.a -= Time.deltaTime * 0.5f;
			_color.a = Mathf.Max(0f, _color.a);
			if (_isFirstFade)
			{
				_isFirstFade = false;
				if (IsAimTarget || _selected)
				{
					UpdateOutline();
				}
			}
		}
		float a = _color.a;
		_color = Color.white;
		_color.a = a;
		if (_selected)
		{
			float num = 0.5f + 0.25f * (Mathf.Sin((time - _selectedAt) * 5f) + 1f);
			ref Color color2 = ref _color;
			color2.g *= num;
			ref Color color3 = ref _color;
			color3.b *= num;
		}
		float num2 = time - _glitterAt;
		if (num2 >= 0f && num2 < 1f)
		{
			float num3 = 0.5f * (Mathf.Cos((Time.time - _glitterAt) * (float)Math.PI * 2f) + 1f);
			float a2 = _color.a;
			_color = Color.Lerp(Color.clear, _color, num3);
			_color.a = a2;
		}
	}

	private static Vector3 CalcParticlePosition(Transform parent, Transform baseAxis, Vector3 baseOffset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		return parent.position + baseAxis.forward * baseOffset.z + baseAxis.right * baseOffset.x + baseAxis.up * baseOffset.y;
	}

	private void ProcessMarkingParticle()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
		if (_markerGaugeEndTime > bufferedServerTime)
		{
			if (!((Object)(object)_markingParticle != (Object)null))
			{
				Vector3 pos = CalcParticlePosition(base.InteractionTransform, ((Component)this).transform, Vector3.up * 150f);
				_markingParticle = ParticleManager.EmitSync("Particle/FX_Marking_01.prefab", pos, Quaternion.identity, base.InteractionTransform, useLocalPosition: false);
				if ((Object)(object)_markingParticle != (Object)null)
				{
					float num = (float)(_markerGaugeEndTime - bufferedServerTime);
					_markingParticle.GetComponent<ParticleSystem>().time = 60f - num;
				}
			}
		}
		else if (!((Object)(object)_markingParticle == (Object)null))
		{
			ParticleManager.Stop(_markingParticle);
			_markingParticle = null;
		}
	}

	private void ProcessEyes()
	{
		bool flag = !IsAlive;
		if (flag | (_lastMotionName != null && _lastMotionName.ContainsIgnoreCase("Sleep")))
		{
			SetEyeStatus(EyeStatus.Closed);
			return;
		}
		if (_nextEyeCloseTime + 0.3f <= Time.time)
		{
			_nextEyeCloseTime = Time.time + (3f + Random.value * 4f);
		}
		else if (_nextEyeCloseTime <= Time.time)
		{
			float num = Time.time - _nextEyeCloseTime;
			float num2 = num / 0.075f;
			if (num2 >= 3f)
			{
				num2 = 1f - (num2 - 3f);
			}
			num2 = Mathf.Clamp01(num2);
			SetEyeStatus(EyeStatus.Closed, num2);
			return;
		}
		bool flag2 = TimeGauge.IsDay();
		SetEyeStatus((!flag2) ? EyeStatus.Night : EyeStatus.Normal);
	}

	[ExposedInEditor(null)]
	private void SetEyeClosed()
	{
		_nextEyeCloseTime = Time.time + 0.1f;
	}

	private void SetEyeStatus(EyeStatus status, float closedRatio = 1f)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (status == EyeStatus.Closed && _eyeClosedRatio != closedRatio)
		{
			_eyeClosedRatio = closedRatio;
			for (int i = 0; i < _closedEyes.Length; i++)
			{
				_closedEyes[i].material.color = new Color(1f, 1f, 1f, closedRatio);
			}
		}
		if (_eyeStatus != status)
		{
			_eyeStatus = status;
			for (int j = 0; j < _closedEyes.Length; j++)
			{
				((Component)_closedEyes[j]).gameObject.SetActive(_eyeStatus == EyeStatus.Closed);
			}
			for (int k = 0; k < _nightEyes.Length; k++)
			{
				_nightEyes[k].SetActive(_eyeStatus == EyeStatus.Night);
			}
		}
	}

	protected override void ProcessDepth()
	{
		if (!GameManager.IsPrologueMode && _processDepth)
		{
			base.ProcessDepth();
		}
	}

	public void Suicide()
	{
		_fadeBeginTime = Time.time + 0.1f;
	}

	public override void OnTakeDamage(Damage damage, GameObject attacker)
	{
		BodyPart part = damage.Part;
		part = BodyPart.Body;
		base.OnTakeDamage(damage, attacker);
		TakeBoneFlinching(part);
		PlayDamagerEffectSet(attacker, damage);
		SetEyeClosed();
		if (_needToCallOnDieAtTakeDamage)
		{
			OnDie();
		}
	}

	public override void TakeBoneFlinching(BodyPart part)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)null != (Object)(object)_boneFlinchingController && IsAnimPlaying)
		{
			_boneFlinchingController.TakeBoneFlinching(GetBodyPartTransform(part));
		}
	}

	public override void SetSurvivalGauge(Gauge life, Dictionary<string, Gauge> gauges)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (life.Get() <= 0f)
		{
			OnDie();
		}
		if (gauges.TryGetValue("target_marker", out var value) && value.Get() > 0f)
		{
			_markerGaugeEndTime = value.When(0f);
		}
		if (gauges.TryGetValue("groggy", out var value2))
		{
			if (value2.Get() <= Mathf.Epsilon && (Object)null == (Object)(object)_groggyParticle)
			{
				Vector3 pos = CalcParticlePosition(HeadTransform, ((Component)this).transform, Vector3.up * 50f);
				_groggyParticle = ParticleManager.EmitSync("Particle/FX_Stunned_01.prefab", pos, Quaternion.identity, HeadTransform, useLocalPosition: false);
			}
			else if (value2.Get() > Mathf.Epsilon && Object.op_Implicit((Object)(object)_groggyParticle))
			{
				ParticleManager.Stop(_groggyParticle);
				_groggyParticle = null;
			}
		}
		base.SetSurvivalGauge(life, gauges);
	}

	private void OnDie()
	{
		((Component)this).gameObject.layer = LayerHelper.PropLayer;
		if ((Object)null == (Object)(object)LastAttacker)
		{
			_needToCallOnDieAtTakeDamage = true;
			return;
		}
		CharacterBehavior component = LastAttacker.GetComponent<CharacterBehavior>();
		if ((Object)(object)component != (Object)null)
		{
			component.OnKilledAnimal(this);
		}
		_needToCallOnDieAtTakeDamage = false;
	}

	public AnimationClipInfo GetCurrentAnimationClipInfo()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Invalid comparison between Unknown and I4
		if ((TrackedReference)null == (TrackedReference)(object)CurAnimState)
		{
			return _curInfo;
		}
		if (!CurAnimState.enabled)
		{
			return AnimationEventController.InvalidAnimationClipInfo;
		}
		_curInfo.Name = _currentAnimEventName;
		_curInfo.AnimTime = Mathf.Repeat(CurAnimState.time, CurAnimState.length);
		_curInfo.Length = CurAnimState.length;
		_curInfo.IsLoop = (CurAnimState.wrapMode & 2) > 0;
		_curInfo.PlaybackRate = CurAnimState.speed;
		_curInfo.Clip = CurAnimState.clip;
		return _curInfo;
	}

	public override Transform GetHeadTransform()
	{
		return HeadTransform;
	}

	private BodyPart GetRandomBodyPart()
	{
		return (BodyPart)Random.Range(1, 7);
	}

	public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, [Optional] Vector3 nearPos)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		Transform val = null;
		switch (part)
		{
		case BodyPart.Head:
			val = HeadTransform;
			break;
		case BodyPart.Body:
			val = BodyTransform;
			break;
		case BodyPart.Arm:
		{
			if (nearPos == default(Vector3))
			{
				val = ((!(Random.value > 0.5f)) ? RightArmTransform : LeftArmTransform);
				break;
			}
			Vector3 val2 = nearPos - LeftArmTransform.position;
			float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
			Vector3 val3 = nearPos - RightArmTransform.position;
			float sqrMagnitude2 = ((Vector3)(ref val3)).sqrMagnitude;
			val = ((!(sqrMagnitude < sqrMagnitude2)) ? RightArmTransform : LeftArmTransform);
			break;
		}
		case BodyPart.Leg:
		{
			if (nearPos == default(Vector3))
			{
				val = ((!(Random.value > 0.5f)) ? RightLegTransform : LeftLegTransform);
				break;
			}
			Vector3 val4 = nearPos - LeftLegTransform.position;
			float sqrMagnitude3 = ((Vector3)(ref val4)).sqrMagnitude;
			Vector3 val5 = nearPos - RightLegTransform.position;
			float sqrMagnitude4 = ((Vector3)(ref val5)).sqrMagnitude;
			val = ((!(sqrMagnitude3 < sqrMagnitude4)) ? RightLegTransform : LeftLegTransform);
			break;
		}
		case BodyPart.Tail:
			val = TailTransform;
			break;
		case BodyPart.Back:
			val = BackTransform;
			break;
		case BodyPart.Auto:
		{
			BodyPart randomBodyPart = GetRandomBodyPart();
			if (randomBodyPart != 0)
			{
				return GetBodyPartTransform(randomBodyPart);
			}
			break;
		}
		}
		if ((Object)null != (Object)(object)val || bAllowNull)
		{
			return val;
		}
		return ((Component)this).transform;
	}

	public AnimationState GetCurAnimState()
	{
		return CurAnimState;
	}

	public void SetDefaultMotionName(string motionName)
	{
		_motionName = motionName;
	}

	public string GetDefaultMotionName()
	{
		return _motionName;
	}

	public GameObject GetGameObject()
	{
		return ((Component)this).gameObject;
	}

	public WrapMode GetWrapMode(string motionName)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		AnimationClip clip = Anim.GetClip(motionName);
		return (WrapMode)(Object.op_Implicit((Object)(object)clip) ? ((int)clip.wrapMode) : 0);
	}

	public void SetAnimationCullingType(AnimationCullingType type)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Anim.cullingType = type;
	}

	public void SetServerSideRootMotionEnable(bool serverSideRootMotionEnabled)
	{
		base.RootMotionMovable.SetServerSideRootMotionEnable(serverSideRootMotionEnabled);
	}

	public bool HasMovingPath()
	{
		return PathMovable.HasMovingPath();
	}

	public void HandleMoveMsg(Move msg)
	{
		PathMovable.HandleMoveMsg(msg);
	}

	private void MovementProcessed(Movement movement)
	{
		float rotSpeed = movement.RotSpeed;
		_rotateSpeed = ((!(rotSpeed > 0f)) ? 300f : rotSpeed);
		double sequenceBeginTick = ((movement.Path.Length <= 0) ? 0.0 : movement.Path[0].Time);
		PlayAnimationMovement(movement.MotionName, (MotionOption)movement.MotionOption, movement.PlaybackRate, sequenceBeginTick);
	}

	public float GetFadeTime(string motionName)
	{
		return (!((Object)(object)_animBlendingController != (Object)null)) ? 0.3f : _animBlendingController.GetFadeTime(motionName, _lastMotionName);
	}

	public void PlayAnimationMovement(string motionName, MotionOption motionOption, float playbackRate, double sequenceBeginTick)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Invalid comparison between Unknown and I4
		if ((Object)(object)Anim == (Object)null || string.IsNullOrEmpty(motionName))
		{
			return;
		}
		bool flag = _lastMotionName != motionName;
		flag = flag || (motionOption & MotionOption.LOOPING) == 0;
		if (flag | !IsAnimPlaying)
		{
			float fadeTime = GetFadeTime(motionName);
			Anim.CrossFade(motionName, fadeTime);
			_lastMotionName = motionName;
			CurAnimState = Anim[motionName];
			CurAnimState.wrapMode = (WrapMode)(((motionOption & MotionOption.LOOPING) > MotionOption.NORMAL) ? 2 : 0);
			float num = Connections.Frontend.CheckBufferedTimePassed_Enhanced(sequenceBeginTick);
			if (CurAnimState.length <= num && (int)CurAnimState.wrapMode != 2)
			{
				num = CurAnimState.length;
			}
			CurAnimState.time = num;
		}
		CurAnimState.speed = playbackRate;
		if ((motionOption & MotionOption.REVERSE) > MotionOption.NORMAL)
		{
			CurAnimState.speed = -1f;
		}
		base.RootMotionMovable.SetInPlaceMotionMode((motionOption & MotionOption.IN_PLACE_MOTION) > MotionOption.NORMAL);
		base.RootMotionMovable.SetLocalRootMotionYawMode((motionOption & MotionOption.USE_LOCAL_ROOT_YAW) > MotionOption.NORMAL);
	}

	public override void TurnToYaw(float yaw, bool bSnap)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (bSnap)
		{
			_destYaw = yaw;
			_curYaw = yaw;
			((Component)this).transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
		}
		else
		{
			_destYaw = yaw;
		}
	}

	private void ProcessRotation()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!(Mathf.Abs(_destYaw - _curYaw) < 1f))
		{
			float curYaw = Mathf.MoveTowardsAngle(_curYaw, _destYaw, Time.deltaTime * _rotateSpeed);
			_curYaw = curYaw;
			((Component)this).transform.localRotation = Quaternion.Euler(0f, _curYaw, 0f);
		}
	}

	private void PreProcessMovements()
	{
		PathMovable.ProcessMovementQueue();
	}

	private void ProcessMovements()
	{
		UpdateVelocity();
		PathMovable.ProcessMovements();
	}

	private void UpdateVelocity()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		if (_prevMoveTime < 0f)
		{
			_prevMoveTime = time;
		}
		float num = time - _prevMoveTime;
		_prevMoveTime = time;
		if (num > 0f)
		{
			base.CurrentVelocity = (CurrentPosition - _prevPosition) / num;
			_prevPosition = CurrentPosition;
		}
	}

	private void SelectShakeTrees()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		int mask = LayerMask.op_Implicit(LayerHelper.PropMask);
		int count = _collidableJoints.Count;
		for (int i = 0; i < count; i++)
		{
			GameObject val = _collidableJoints[i];
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			int count2;
			Collider[] array = KCollisionUtility.OverlapSphere(val.transform.position, _collisionRadius, mask, out count2);
			if (count2 == 0)
			{
				continue;
			}
			for (int j = 0; j < count2; j++)
			{
				NaturalObject component = ((Component)array[j]).GetComponent<NaturalObject>();
				if (!((Object)(object)component == (Object)null))
				{
					TreeComponent treeComponent = component.NaturalComponent as TreeComponent;
					if (!(treeComponent == null))
					{
						treeComponent.BeginShake(j == 0);
					}
				}
			}
		}
	}

	private void UpdateOutline()
	{
		bool show = (IsAimTarget || _selected) && _color.a >= 1f;
		Outline component = ((Component)this).GetComponent<Outline>();
		if ((Object)(object)component != (Object)null)
		{
			component.Fade(show);
		}
	}

	[UsedImplicitly]
	private void OnSelected(bool selected)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (_selected != selected)
		{
			_selected = selected;
			UpdateOutline();
			if (_selected)
			{
				_selectedAt = Time.time;
				return;
			}
			float a = _color.a;
			_color = Color.white;
			_color.a = a;
		}
	}

	[UsedImplicitly]
	private void OnGlitter(float delay)
	{
		_glitterAt = Time.time + delay;
	}

	public void SetName(string animalName)
	{
		_animalName = animalName;
	}

	public override string GetName()
	{
		return (!string.IsNullOrEmpty(_animalName)) ? _animalName : AnimalYaml.GetName(base.EntityTypeId);
	}

	private IEnumerator DelayedPlay(string motionName, float delay)
	{
		yield return (object)new WaitForSeconds(delay);
		Play(motionName);
	}

	public float Play(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		return DoPlayAnimation(crossFade: false, motionName, 0f, (WrapMode)(loop ? 2 : 0), beginTime, playbackRate);
	}

	public float CrossFade(string motionName, float fadeTime = -1f, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		if (fadeTime < 0f)
		{
			fadeTime = GetFadeTime(motionName);
		}
		return DoPlayAnimation(crossFade: true, motionName, fadeTime, (WrapMode)(loop ? 2 : 0), beginTime, playbackRate);
	}

	private float DoPlayAnimation(bool crossFade, string motionName, float fadeTime, WrapMode wrapMode, float beginTime, float playbackRate)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Anim == (Object)null)
		{
			return 0f;
		}
		if (crossFade)
		{
			Anim.CrossFade(motionName, fadeTime);
		}
		else
		{
			Anim.Play(motionName);
		}
		_lastMotionName = motionName;
		Anim.wrapMode = wrapMode;
		CurAnimState = Anim[motionName];
		CurAnimState.enabled = true;
		CurAnimState.wrapMode = wrapMode;
		CurAnimState.time = beginTime;
		CurAnimState.speed = playbackRate;
		return CurAnimState.length;
	}

	public void Stop()
	{
		if ((Object)(object)Anim != (Object)null)
		{
			Anim.Stop();
		}
		CurAnimState = null;
	}

	public override string GetCurrentAnimationClipName()
	{
		return (!string.IsNullOrEmpty(_curAnimStateName)) ? _curAnimStateName : string.Empty;
	}

	public Vector3 GetCurrentPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return CurrentPosition;
	}

	public override string GetAttackNameForDeathMsg()
	{
		return LocalizeSystem.GetRandom(DeathMsgAttackAnimalList);
	}

	public void AttackNotice(double attackAt)
	{
		float delay = (float)(attackAt - Connections.Frontend.GetBufferedServerTime()) - 2f;
		KUtility.DelayedCall((MonoBehaviour)(object)this, EmitAttackNoticeEfx, delay);
	}

	private void EmitAttackNoticeEfx()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Min(base.XRadius, base.YRadius);
		string assetPath = ((!(num >= 100f)) ? "Particle/FX_SkillActivated_Common_01.prefab" : "Particle/FX_SkillActivated_Common_01_Large.prefab");
		ParticleManager.Emit(assetPath, Vector3.zero, Quaternion.identity, ((Component)this).transform);
	}

	public void ApplyTensionColor(Color color)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (_curChangeColorCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_curChangeColorCoroutine);
		}
		_curChangeColorCoroutine = ((MonoBehaviour)this).StartCoroutine(CoChangeTensionColor(color));
	}

	private IEnumerator CoChangeTensionColor(Color color, float duration = 2f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		float beginTime = Time.time;
		Color beginColor = _tensionColor;
		while (true)
		{
			float t = Mathf.Clamp01((Time.time - beginTime) / duration);
			_tensionColor = Color.Lerp(beginColor, color, t);
			if (t >= 1f)
			{
				break;
			}
			yield return null;
		}
	}
}
