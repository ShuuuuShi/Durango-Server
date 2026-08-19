using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Model;
using Durango.Network;
using Durango.Render;
using Durango.Render.Particle;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Animal;
using Shared.Battle;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class AnimalBehavior : CharacterBehavior, IMotionPlayable, IAnimationEventPlayable, IBoneMergedObserver
{
	private enum EyeStatus
	{
		Invalid,
		Normal,
		Closed,
		Night
	}

	[SerializeField]
	public AnimalFrameworkResource AnimalFrameworkResource;

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
	private bool _processDepth = true;

	[SerializeField]
	private Renderer[] _closedEyes;

	[SerializeField]
	private GameObject[] _nightEyes;

	[SerializeField]
	private GameObject[] _eyeTrail;

	[SerializeField]
	private Texture2D _dieTexture;

	[SerializeField]
	public float UIViewScale = 1f;

	[SerializeField]
	private float _lootableRimMax = 20f;

	[SerializeField]
	private float _lootableRimMin = 5f;

	[SerializeField]
	private float _lootableRimIntensity = 2f;

	protected bool SkipSelectedProcess;

	private const float InitRotateSpeed = 300f;

	private float _rotateSpeed = 300f;

	private float _prevMoveTime = -1f;

	private Vector3 _prevPosition;

	private float _curYaw;

	private float _destYaw;

	private BoneFlinchingController _boneFlinchingController;

	private float _fadeBeginTime = -1f;

	private float _prevShakeTime;

	private AnimationBlendingController _animBlendingController;

	private Renderer _mainRenderer;

	private AnimationClipInfo _curInfo;

	private string _lastMotionName;

	private bool _isSleepMotion;

	private readonly RendererProxy _rendererProxy = new RendererProxy();

	private Color _color = Color.white;

	private Color _tensionColor = Color.gray;

	private Color _rimLight = Color.black;

	private EyeStatus _eyeStatus;

	private float _eyeClosedRatio;

	private float _nextEyeCloseTime;

	private bool _eyeTrailOn;

	private bool _initializeEyeTrail;

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

	private PathMovable _pathMovable;

	private Animation _anim;

	private BoneMergeable _boneMergeable;

	private int _markingParticleId;

	private double _markerGaugeEndTime = -1.0;

	private int _groggyParticleId;

	private AnimationState _curAnimState;

	private bool _selected;

	private string _animalName;

	private Coroutine _curChangeColorCoroutine;

	public override bool WillBeRendered
	{
		get
		{
			if (_mainRenderer == null)
			{
				_mainRenderer = base.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
			}
			return _mainRenderer == null || _mainRenderer.isVisible;
		}
	}

	public AnimalStatus Status { get; set; }

	[ExposedInEditor(null)]
	public bool IsLootable { get; set; }

	private Transform HeadTransform
	{
		get
		{
			if (_headTransform == null)
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
			if (_bodyTransform == null)
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
			if (_leftArmTransform == null)
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
			if (_rightArmTransform == null)
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
			if (_leftLegTransform == null)
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
			if (_rightLegTransform == null)
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
			if (_tailTransform == null)
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
			if (_backTransform == null)
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
			if (_bip001Transform == null)
			{
				_bip001Transform = FindTransformByName(_bodyPartBip001);
				if (_bip001Transform == null)
				{
					_bip001Transform = base.transform;
				}
			}
			return _bip001Transform;
		}
	}

	public GameObject MeshObject
	{
		get
		{
			if (_meshObject != null)
			{
				return _meshObject;
			}
			Animation componentInChildren = base.gameObject.GetComponentInChildren<Animation>(includeInactive: true);
			if (componentInChildren == null)
			{
				return null;
			}
			_meshObject = componentInChildren.gameObject;
			return _meshObject;
		}
	}

	public override Transform MeshObjectTransform => (!(MeshObject != null)) ? null : MeshObject.transform;

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

	public override Animation Anim
	{
		get
		{
			if (_anim == null)
			{
				_anim = MeshObject.GetComponent<Animation>();
			}
			return _anim;
		}
	}

	public override BoneMergeable BoneMergeable
	{
		get
		{
			if (_boneMergeable == null)
			{
				_boneMergeable = new BoneMergeable(base.gameObject, MeshObjectTransform, Bip001Transform);
			}
			return _boneMergeable;
		}
	}

	public bool AnimationEventProhibited => false;

	public override bool IsAnimPlaying => Anim.isPlaying;

	public override Vector3 CurrentPosition
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	[CanBeNull]
	public AnimationState CurAnimState
	{
		get
		{
			return _curAnimState;
		}
		set
		{
			_curAnimState = value;
			string currentAnimClipName = ((!(_curAnimState == null)) ? _curAnimState.name : null);
			base.CurrentAnimClipName = currentAnimClipName;
		}
	}

	public event Action<AnimalBehavior> Destroyed;

	protected new void Awake()
	{
		base.Awake();
		LoadAnimationClips();
		SetEyeStatus(EyeStatus.Normal);
	}

	private void OnDestroy()
	{
		if (this.Destroyed != null)
		{
			this.Destroyed(this);
		}
	}

	public virtual void LoadAnimationClips()
	{
		if (AnimalFrameworkResource == null)
		{
			return;
		}
		List<AnimationClip> list = new List<AnimationClip>();
		AnimalFrameworkResource.CollectClips(list);
		foreach (AnimationClip item in list)
		{
			if (!(item == null))
			{
				Anim.AddClip(item, item.name);
			}
		}
	}

	public virtual void ClearAnimationClips()
	{
		if (AnimalFrameworkResource == null)
		{
			return;
		}
		List<AnimationClip> list = new List<AnimationClip>();
		AnimalFrameworkResource.CollectClips(list);
		foreach (AnimationClip item in list)
		{
			if (!(item == null) && (bool)Anim.GetClip(item.name))
			{
				Anim.RemoveClip(item.name);
			}
		}
	}

	protected new void Start()
	{
		base.Start();
		_rendererProxy.Add(Renderers, isAnimal: true);
		if (GameManager.IsPrologueMode)
		{
			Durango.Utils.Singleton<ContactShadowManager>.Instance().Create(base.gameObject);
		}
		ParticleManager.Cache("Particle/FX_BloodBurst_Strike_01.prefab");
		ParticleManager.Cache("Particle/FX_Marking_01.prefab");
		ParticleManager.Cache("Particle/FX_Stunned_01.prefab");
		_boneFlinchingController = base.gameObject.GetComponent<BoneFlinchingController>();
		Anim.cullingType = AnimationCullingType.BasedOnRenderers;
		base.CurrentVelocity = Vector3.zero;
		_animBlendingController = GetComponent<AnimationBlendingController>();
		if (_animBlendingController != null && _animBlendingController.IsLoaded())
		{
		}
	}

	void IBoneMergedObserver.OnAttached(SkinnedMeshRenderer[] renderers)
	{
		_rendererProxy.Add(renderers, isAnimal: true);
	}

	void IBoneMergedObserver.OnDetached(SkinnedMeshRenderer[] renderers)
	{
		_rendererProxy.Remove(renderers);
	}

	public void PrepareRendererProxy()
	{
		if (_rendererProxy.IsEmpty())
		{
			SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
			_rendererProxy.Add(componentsInChildren, isAnimal: true);
		}
	}

	private void Update()
	{
		if (_prevShakeTime + 1f < Time.time)
		{
			SelectShakeTrees();
			_prevShakeTime = Time.time;
		}
		UpdateVelocity();
		PathMovable.Process();
		ProcessRotation();
		CheckCurrentTile();
		ProcessFade();
		ProcessSelected();
		ProcessRimLight();
		ProcessMarkingParticle();
		ProcessAffectNearObject();
		ProcessEyes();
		_rendererProxy.SetColor(_color);
		_rendererProxy.SetSubColor(_tensionColor);
		_rendererProxy.SetRimLight(_rimLight);
	}

	protected void LateUpdate()
	{
		if (WillBeRendered)
		{
			base.RootMotionMovable.LateUpdateRootMotion();
			BoneMergeable.UpdateBoneMergeSet();
		}
	}

	private void ProcessFade()
	{
		float time = Time.time;
		if (_fadeBeginTime >= 0f && time >= _fadeBeginTime && _color.a > 0f)
		{
			_color.a -= Time.deltaTime * 0.2f;
			_color.a = Mathf.Max(0f, _color.a);
			if (_color.a <= 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	private void ProcessSelected()
	{
		if (!SkipSelectedProcess)
		{
			float a = _color.a;
			_color = Color.white;
			_color.a = a;
			if (_selected)
			{
				float time = Time.time;
				float num = 1f + 0.15f * (Mathf.Sin(time * 4f) + 1f);
				_color.r *= num;
				_color.g *= num;
				_color.b *= num;
			}
		}
	}

	private void ProcessRimLight()
	{
		if (IsLootable)
		{
			_rimLight = ((!(AmbientLighting != null)) ? Color.white : AmbientLighting.CurAmbientColor);
			_rimLight *= _lootableRimIntensity;
			float time = Time.time;
			float a = _lootableRimMin + (_lootableRimMax - _lootableRimMin) * (Mathf.Sin(time * 4f) * 0.5f + 0.5f);
			_rimLight.a = a;
		}
		else
		{
			_rimLight = Color.black;
		}
	}

	private static Vector3 CalcParticlePosition(Transform parent, Transform baseAxis, Vector3 baseOffset)
	{
		return parent.position + baseAxis.forward * baseOffset.z + baseAxis.right * baseOffset.x + baseAxis.up * baseOffset.y;
	}

	private void ProcessMarkingParticle()
	{
		double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
		if (_markerGaugeEndTime > bufferedServerTime)
		{
			if (_markingParticleId != 0)
			{
				return;
			}
			Vector3 pos = CalcParticlePosition(base.InteractionTransform, base.transform, Vector3.up * 150f);
			_markingParticleId = ParticleManager.EmitFollow("Particle/FX_Marking_01.prefab", pos, Quaternion.identity, base.InteractionTransform, useLocalPosition: false);
			if (_markingParticleId != 0)
			{
				float num = (float)(_markerGaugeEndTime - bufferedServerTime);
				GameObject particleIfLoaded = Durango.Utils.Singleton<ParticleManager>.Instance().GetParticleIfLoaded(_markingParticleId);
				if (particleIfLoaded != null)
				{
					particleIfLoaded.GetComponent<ParticleSystem>().time = 60f - num;
				}
			}
		}
		else if (_markingParticleId != 0)
		{
			ParticleManager.Stop(_markingParticleId);
			_markingParticleId = 0;
		}
	}

	private void ProcessEyes()
	{
		bool flag = !base.IsAlive;
		flag |= _isSleepMotion;
		bool flag2 = _eyeTrailOn && !flag;
		if (flag)
		{
			SetEyeStatus(EyeStatus.Closed);
		}
		else
		{
			if (_nextEyeCloseTime + 0.3f <= Time.time)
			{
				_nextEyeCloseTime = Time.time + (3f + UnityEngine.Random.value * 4f);
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
			SetEyeStatus((!flag2 && TimeGauge.IsSunUp) ? EyeStatus.Normal : EyeStatus.Night);
		}
		SetEyeTrail(flag2);
	}

	[ExposedInEditor(null)]
	[UsedImplicitly]
	private void ShowEyeTrail()
	{
		_eyeTrailOn = true;
	}

	public void ShowEyeTrail(bool on)
	{
		_eyeTrailOn = on;
	}

	[ExposedInEditor(null)]
	private void SetEyeClosed()
	{
		_nextEyeCloseTime = Time.time + 0.1f;
	}

	private void SetEyeStatus(EyeStatus status, float closedRatio = 1f)
	{
		if (status == EyeStatus.Closed && _eyeClosedRatio != closedRatio)
		{
			_eyeClosedRatio = closedRatio;
			for (int i = 0; i < _closedEyes.Length; i++)
			{
				if (!(_closedEyes[i] == null) && !(_closedEyes[i].material == null))
				{
					_closedEyes[i].material.color = new Color(1f, 1f, 1f, closedRatio);
				}
			}
		}
		if (_eyeStatus == status)
		{
			return;
		}
		_eyeStatus = status;
		for (int j = 0; j < _closedEyes.Length; j++)
		{
			if (!(_closedEyes[j] == null))
			{
				_closedEyes[j].gameObject.SetActive(_eyeStatus == EyeStatus.Closed);
			}
		}
		for (int k = 0; k < _nightEyes.Length; k++)
		{
			if (!(_nightEyes[k] == null))
			{
				_nightEyes[k].SetActive(_eyeStatus == EyeStatus.Night);
			}
		}
	}

	private void SetEyeTrail(bool on)
	{
		if (on && !_initializeEyeTrail)
		{
			InitializeEyeTrail();
		}
		int i = 0;
		for (int size = KUtility.GetSize(_eyeTrail); i < size; i++)
		{
			_eyeTrail[i].SetActive(on);
		}
	}

	private void InitializeEyeTrail()
	{
		_initializeEyeTrail = true;
		if (KUtility.GetSize(_eyeTrail) == 0 && KUtility.GetSize(_nightEyes) > 0)
		{
			ResetEyeTrail();
		}
		int size = KUtility.GetSize(_eyeTrail);
		for (int i = 0; i < size; i++)
		{
			Material material = new Material(_nightEyes[i].GetComponent<Renderer>().sharedMaterial);
			material.mainTexture = Texture2D.whiteTexture;
			TrailRenderer component = _eyeTrail[i].GetComponent<TrailRenderer>();
			component.sharedMaterial = material;
		}
	}

	[ExposedInEditor("Reset Eye Trail")]
	[UsedImplicitly]
	private void ResetEyeTrail()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_eyeTrail); i < size; i++)
		{
			NGUITools.Destroy(_eyeTrail[i].gameObject);
		}
		int size2 = KUtility.GetSize(_nightEyes);
		_eyeTrail = new GameObject[size2];
		if (size2 != 0)
		{
			for (int j = 0; j < size2; j++)
			{
				_eyeTrail[j] = new GameObject("EyeTrail");
				Transform transform = _eyeTrail[j].transform;
				transform.parent = _nightEyes[j].transform.parent;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
				transform.gameObject.layer = _nightEyes[j].layer;
				TrailRenderer trailRenderer = _eyeTrail[j].AddComponent<TrailRenderer>();
				Gradient colorGradient = trailRenderer.colorGradient;
				colorGradient.SetKeys(new GradientColorKey[2]
				{
					new GradientColorKey(Color.red, 0f),
					new GradientColorKey(Color.yellow, 1f)
				}, new GradientAlphaKey[2]
				{
					new GradientAlphaKey(1f, 0f),
					new GradientAlphaKey(0f, 1f)
				});
				trailRenderer.colorGradient = colorGradient;
				trailRenderer.time = 0.3f;
				trailRenderer.widthMultiplier = _nightEyes[j].transform.lossyScale.x * 80f;
			}
		}
	}

	public override float ProcessWaterDepth(Vector3 pos)
	{
		if (GameManager.IsPrologueMode || !_processDepth)
		{
			return 0f;
		}
		return base.ProcessWaterDepth(pos);
	}

	public void Disappear()
	{
		_fadeBeginTime = Time.time + 0.1f;
	}

	public void Appear()
	{
		_fadeBeginTime = -1f;
		_color.a = 1f;
	}

	public override void OnTakeDamage(Damage damage, DamageableEntity attacker)
	{
		BodyPart part = damage.Part;
		part = BodyPart.Body;
		base.OnTakeDamage(damage, attacker);
		TakeBoneFlinching(part);
		SetEyeClosed();
	}

	public override void TakeBoneFlinching(BodyPart part)
	{
		if (null != _boneFlinchingController && IsAnimPlaying)
		{
			_boneFlinchingController.TakeBoneFlinching(GetBodyPartTransform(part));
		}
	}

	public override void SetSurvivalGauge(Gauge life, Dictionary<string, Gauge> gauges)
	{
		if (gauges != null)
		{
			if (gauges.TryGetValue("target_marker", out var value) && value.Get() > 0f)
			{
				_markerGaugeEndTime = value.When(0f);
			}
			if (gauges.TryGetValue("groggy", out var value2))
			{
				if (value2.Get() <= Mathf.Epsilon && _groggyParticleId == 0)
				{
					Vector3 pos = CalcParticlePosition(GetBodyPartTransform(BodyPart.Head), base.transform, Vector3.up * 50f);
					_groggyParticleId = ParticleManager.EmitFollow("Particle/FX_Stunned_01.prefab", pos, Quaternion.identity, HeadTransform, useLocalPosition: false);
				}
				else if (value2.Get() > Mathf.Epsilon && _groggyParticleId != 0)
				{
					ParticleManager.Stop(_groggyParticleId);
					_groggyParticleId = 0;
				}
			}
		}
		base.SetSurvivalGauge(life, gauges);
	}

	public void SetOld(bool isOld)
	{
		_rendererProxy.SetDamaged((!isOld) ? 0f : 1f);
	}

	public void SetAsDead()
	{
		SetAlive(alive: false, fromInit: true);
		if (!(AnimalFrameworkResource == null))
		{
			AnimationElemBase animationElements = AnimalFrameworkResource.GetAnimationElements("dead");
			if (animationElements != null && animationElements.TryMoveNext(0, out var res))
			{
				PlayToLast(res.Clip);
			}
		}
	}

	[ExposedInEditor(null)]
	protected override void OnDie(bool fromInit)
	{
		base.OnDie(fromInit);
		base.gameObject.layer = LayerHelper.PropLayer;
		if (LastAttacker != null)
		{
			CharacterDamageableEntity characterDamageableEntity = LastAttacker as CharacterDamageableEntity;
			if (characterDamageableEntity != null)
			{
				characterDamageableEntity.OwnerComponent.OnKilledAnimal(this);
			}
		}
		if (_dieTexture != null)
		{
			StartCoroutine(CoTransitTexture(_dieTexture, (!fromInit) ? 1f : 0f));
		}
	}

	[ExposedInEditor(null)]
	protected override void OnRevive()
	{
		base.OnRevive();
		base.gameObject.layer = LayerHelper.DefaultLayer;
		if (_dieTexture != null)
		{
			StartCoroutine(CoTransitTexture(_dieTexture, 1f, 1f, 0f));
		}
	}

	private IEnumerator CoTransitTexture(Texture2D tex, float time, float beginRatio = 0f, float endRatio = 1f)
	{
		float ratio = 0f;
		while (ratio <= 1f)
		{
			yield return null;
			ratio += ((!(time <= 0f)) ? (Time.deltaTime / time) : 1f);
			_rendererProxy.SetTransition(tex, Mathf.Lerp(beginRatio, endRatio, ratio));
		}
	}

	public AnimationClipInfo GetCurrentAnimationClipInfo()
	{
		if (CurAnimState == null)
		{
			return _curInfo;
		}
		if (!CurAnimState.enabled)
		{
			return AnimationEventController.InvalidAnimationClipInfo;
		}
		_curInfo.Name = base.CurrentAnimKeyName;
		_curInfo.State = CurAnimState;
		return _curInfo;
	}

	private BodyPart GetRandomBodyPart()
	{
		return (BodyPart)UnityEngine.Random.Range(1, 7);
	}

	public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3))
	{
		Transform transform = null;
		switch (part)
		{
		case BodyPart.Head:
			transform = HeadTransform;
			break;
		case BodyPart.Body:
			transform = BodyTransform;
			break;
		case BodyPart.Arm:
		{
			if (nearPos == default(Vector3))
			{
				transform = ((!(UnityEngine.Random.value > 0.5f)) ? RightArmTransform : LeftArmTransform);
				break;
			}
			float sqrMagnitude = (nearPos - LeftArmTransform.position).sqrMagnitude;
			float sqrMagnitude2 = (nearPos - RightArmTransform.position).sqrMagnitude;
			transform = ((!(sqrMagnitude < sqrMagnitude2)) ? RightArmTransform : LeftArmTransform);
			break;
		}
		case BodyPart.Leg:
		{
			if (nearPos == default(Vector3))
			{
				transform = ((!(UnityEngine.Random.value > 0.5f)) ? RightLegTransform : LeftLegTransform);
				break;
			}
			float sqrMagnitude3 = (nearPos - LeftLegTransform.position).sqrMagnitude;
			float sqrMagnitude4 = (nearPos - RightLegTransform.position).sqrMagnitude;
			transform = ((!(sqrMagnitude3 < sqrMagnitude4)) ? RightLegTransform : LeftLegTransform);
			break;
		}
		case BodyPart.Tail:
			transform = TailTransform;
			break;
		case BodyPart.Back:
			transform = BackTransform;
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
		if (null != transform || bAllowNull)
		{
			return transform;
		}
		return base.transform;
	}

	public AnimationState GetCurAnimState()
	{
		return CurAnimState;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	WrapMode IMotionPlayable.GetWrapMode(string motionName)
	{
		AnimationClip clip = Anim.GetClip(motionName);
		return clip ? clip.wrapMode : WrapMode.Default;
	}

	public void SetAnimationCullingType(AnimationCullingType type)
	{
		Anim.cullingType = type;
	}

	public void SetActivateRootMotion(bool active)
	{
		base.RootMotionMovable.SetActivateRootMotion(active);
	}

	public void ResetRootMotionOffset()
	{
		base.RootMotionMovable.ResetRootMotionOffset();
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
		SetRotateSpeed(movement.RotSpeed);
		double num = ((movement.Path.Length <= 0) ? 0.0 : movement.Path[0].Time);
		num *= (double)movement.PlaybackRate;
		PlayAnimationMovement(movement.MotionName, (MotionOption)movement.MotionOption, movement.PlaybackRate, num);
	}

	public void SetRotateSpeed(float speed)
	{
		_rotateSpeed = ((!(speed > 0f)) ? 300f : speed);
	}

	public float GetFadeTime(string motionName)
	{
		return (!(_animBlendingController != null)) ? 0.3f : _animBlendingController.GetFadeTime(motionName, _lastMotionName);
	}

	public void PlayAnimationMovement(string motionName, MotionOption motionOption, float playbackRate, double sequenceBeginTick)
	{
		if (Anim == null || string.IsNullOrEmpty(motionName))
		{
			return;
		}
		float fadeTime = GetFadeTime(motionName);
		float beginTime = Connections.Frontend.CheckBufferedTimePassed(sequenceBeginTick);
		CrossFade(motionName, fadeTime, (motionOption & MotionOption.LOOPING) > MotionOption.NORMAL, beginTime, playbackRate);
		if (CurAnimState != null)
		{
			CurAnimState.speed = playbackRate;
			if ((motionOption & MotionOption.REVERSE) > MotionOption.NORMAL)
			{
				CurAnimState.speed = -1f;
			}
		}
		base.RootMotionMovable.SetInPlaceMotionMode((motionOption & MotionOption.IN_PLACE_MOTION) > MotionOption.NORMAL);
		base.RootMotionMovable.SetLocalRootMotionYawMode((motionOption & MotionOption.USE_LOCAL_ROOT_YAW) > MotionOption.NORMAL);
	}

	public override void TurnToYaw(float yaw, bool bSnap)
	{
		if (bSnap)
		{
			_destYaw = yaw;
			_curYaw = yaw;
			base.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
		}
		else
		{
			_destYaw = yaw;
		}
	}

	private void ProcessRotation()
	{
		if (!(Mathf.Abs(_destYaw - _curYaw) < 1f))
		{
			float curYaw = Mathf.MoveTowardsAngle(_curYaw, _destYaw, Time.deltaTime * _rotateSpeed);
			_curYaw = curYaw;
			base.transform.localRotation = Quaternion.Euler(0f, _curYaw, 0f);
		}
	}

	private void UpdateVelocity()
	{
		float time = Time.time;
		if (_prevMoveTime < 0f)
		{
			_prevMoveTime = time;
		}
		float num = time - _prevMoveTime;
		_prevMoveTime = time;
		if (num > 0f)
		{
			Vector3 currentPosition = CurrentPosition;
			base.CurrentVelocity = (currentPosition - _prevPosition) / num;
			_prevPosition = currentPosition;
		}
		base.IsMoving.Value = base.CurrentVelocity.sqrMagnitude > 0f;
	}

	private void SelectShakeTrees()
	{
		int mask = LayerHelper.PropMask;
		int count;
		Collider[] array = Collisions.OverlapSphere(CurrentPosition, base.XRadius, mask, out count);
		if (count == 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			NaturalSpriteObject component = array[i].GetComponent<NaturalSpriteObject>();
			if (!(component == null))
			{
				TreeComponent treeComponent = component.NaturalComponent as TreeComponent;
				if (!(treeComponent == null))
				{
					treeComponent.BeginShake(i == 0);
				}
			}
		}
	}

	public override void Select(bool selected, Color outlineColor = default(Color), float outlineWidth = 0f)
	{
		if (_selected != selected)
		{
			_selected = selected;
			if (!_selected)
			{
				float a = _color.a;
				_color = Color.white;
				_color.a = a;
			}
		}
		base.Select(selected, outlineColor, outlineWidth);
	}

	public void SetName(string animalName)
	{
		_animalName = animalName;
	}

	public override string GetName()
	{
		return (!string.IsNullOrEmpty(_animalName)) ? _animalName : AnimalYaml.GetName(base.EntityTypeId);
	}

	public override float[] GetLifeGaugeRatio()
	{
		return SingletonDict<int, Animal>.Get(base.EntityTypeId)?.LifeGaugeRatio;
	}

	public void PlayAndFitLocation(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		if (!loop || !(CurAnimState != null) || !(CurAnimState.name == motionName))
		{
			Vector3 position = Bip001Transform.position;
			Play(motionName, loop, beginTime, playbackRate);
			Anim.Sample();
			Vector3 pos = position - Bip001Transform.position;
			CurrentPosition += Maths.Make2D(pos);
		}
	}

	public void PlayToLast(string motionName)
	{
		Play(motionName, loop: false);
		Anim[base.CurrentAnimClipName].normalizedTime = 1f;
		Anim.Sample();
	}

	public float Play(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		return DoPlayAnimation(crossFade: false, motionName, 0f, loop ? WrapMode.Loop : WrapMode.Default, beginTime, playbackRate);
	}

	public float CrossFade(string motionName, float fadeTime = -1f, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		if (fadeTime < 0f)
		{
			fadeTime = GetFadeTime(motionName);
		}
		return DoPlayAnimation(crossFade: true, motionName, fadeTime, loop ? WrapMode.Loop : WrapMode.Default, beginTime, playbackRate);
	}

	private float DoPlayAnimation(bool crossFade, string motionName, float fadeTime, WrapMode wrapMode, float beginTime, float playbackRate)
	{
		if (Anim == null)
		{
			return 0f;
		}
		if (wrapMode == WrapMode.Loop && _lastMotionName == motionName && CurAnimState != null && IsAnimPlaying)
		{
			return CurAnimState.length;
		}
		// [แก้เอง] เช็คก่อนว่าโมเดลตัวนี้มีคลิปชื่อนี้จริงไหม
		//
		// โมเดลบางตัวในชุด asset นี้ไม่มีคลิปครบตามที่ข้อมูลเกมระบุ (เช่น TRex_Stand)
		// เดิมเรียก Play() ตรง ๆ ⇒ Unity เตือน "could not be played because it couldn't be found"
		// **ทุกเฟรม** ⇒ log บวม 3 MB ใน 2 นาที และเฟรมตก (เจอจริงตอนเทส: 15,308 บรรทัด)
		if (Anim[motionName] == null)
		{
			SetLastMotionName(motionName);   // จำไว้ว่าเคยขอแล้ว จะได้ไม่วนขอซ้ำทุกเฟรม
			CurAnimState = null;
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
		SetLastMotionName(motionName);
		Anim.wrapMode = wrapMode;
		CurAnimState = Anim[motionName];
		if (CurAnimState == null)
		{
			return 0f;
		}
		CurAnimState.enabled = true;
		CurAnimState.wrapMode = wrapMode;
		if (CurAnimState.length < beginTime && CurAnimState.wrapMode != WrapMode.Loop)
		{
			beginTime = CurAnimState.length;
		}
		CurAnimState.time = beginTime;
		CurAnimState.speed = playbackRate;
		return CurAnimState.length;
	}

	public Vector3 GetCurrentPosition()
	{
		return CurrentPosition;
	}

	public void AttackNotice(double attackAt)
	{
		float delay = (float)(attackAt - Connections.Frontend.GetBufferedServerTime()) - 2f;
		KUtility.DelayedCall(this, EmitAttackNoticeEfx, delay);
	}

	private void EmitAttackNoticeEfx()
	{
		float num = Mathf.Min(base.XRadius, base.YRadius);
		string assetPath = ((!(num >= 100f)) ? "Particle/FX_SkillActivated_Common_01.prefab" : "Particle/FX_SkillActivated_Common_01_Large.prefab");
		ParticleManager.EmitFollow(assetPath, Vector3.zero, Quaternion.identity, base.transform);
	}

	public void ApplyTensionColor(Color color)
	{
		if (_curChangeColorCoroutine != null)
		{
			StopCoroutine(_curChangeColorCoroutine);
		}
		_curChangeColorCoroutine = StartCoroutine(CoChangeTensionColor(color));
	}

	private IEnumerator CoChangeTensionColor(Color color, float duration = 2f)
	{
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

	private void SetLastMotionName(string lastMotionName)
	{
		_lastMotionName = lastMotionName;
		_isSleepMotion = !string.IsNullOrEmpty(lastMotionName) && lastMotionName.ContainsIgnoreCase("Sleep");
	}
}
