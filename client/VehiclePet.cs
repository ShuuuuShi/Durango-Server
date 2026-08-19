using System.Collections.Generic;
using Durango.Model;
using InteractionData;
using Shared.Battle;
using UnityEngine;
using Yaml;

public class VehiclePet : VehicleBase
{
	[SerializeField]
	private string _stopMotion;

	[SerializeField]
	private string _moveSet;

	[SerializeField]
	private float _eatDistance = 250f;

	[SerializeField]
	private GameObject _saddlePrefab;

	[SerializeField]
	private GameObject _helmetPrefab;

	[SerializeField]
	private float _walkSpeed = 200f;

	[Tooltip("플레이어를 따라가는 거리")]
	[SerializeField]
	private float _followDistance = 500f;

	[Tooltip("플레이어를 따라가는 거리 체크에 사용하는 dead band")]
	[SerializeField]
	private float _distanceThreshould = 200f;

	[Tooltip("이 거리를 벗어나면 텔레포트해서 주인 가까이로 옵니다.")]
	[SerializeField]
	private float _maxFollowDistance = 3200f;

	[SerializeField]
	private float _headMaxYaw;

	[SerializeField]
	private float _tailMaxYaw;

	private float _curHeadYaw;

	private float _curTailYaw;

	private float _prevYaw;

	private IMotionPlayable _motionPlayable;

	private GameObject _saddle;

	private GameObject _helmet;

	private BoneLookAtTarget _lookAtController;

	private AnimalBehavior _animal;

	private Transform _rootTailBone;

	private Transform _neckBone;

	public override string EntityId => _animal.EntityId;

	public float WalkSpeed => _walkSpeed;

	public bool IsRidable { get; set; }

	public IMotionPlayable MotionPlayable
	{
		get
		{
			if (_motionPlayable == null)
			{
				_motionPlayable = base.gameObject.GetComponent<IMotionPlayable>();
			}
			return _motionPlayable;
		}
	}

	public string StopMotion => _stopMotion;

	public string MoveSet => _moveSet;

	public float EatDistance => _eatDistance;

	public override bool IsRemoveWithDriver => true;

	public override string Name => _animal.GetName();

	public CharacterBehavior.SizeLevel Size => _animal.Size;

	public string PrefabName
	{
		get
		{
			string modelPath = AnimalYaml.GetModelPath(_animal.EntityTypeId);
			return modelPath.Substring(Mathf.Min(modelPath.LastIndexOf('/') + 1, modelPath.Length - 1));
		}
	}

	public override bool IgnoreWaterFlow => Size >= CharacterBehavior.SizeLevel.Medium;

	public override bool IsAlive => _animal.IsAlive;

	public float FollowDistance => _followDistance;

	public float DistanceThreshould => _distanceThreshould;

	public float MaxFollowDistance => _maxFollowDistance;

	private void Awake()
	{
		_lookAtController = base.gameObject.GetComponent<BoneLookAtTarget>();
		_animal = GetComponent<AnimalBehavior>();
		_animal.BoneMergeable.PreUpdate += LeanHeadAndTail;
	}

	private void OnDestroy()
	{
		_animal.BoneMergeable.PreUpdate -= LeanHeadAndTail;
	}

	public override void SetDriver(Driver driver)
	{
		base.SetDriver(driver);
		if ((bool)_lookAtController)
		{
			_lookAtController.SetLookTarget(driver.gameObject, findHead: true);
			_lookAtController.AutoChangeTarget = true;
		}
	}

	public override void AttachDriver(Driver driver, GameObject saddle = null)
	{
		if (saddle == null)
		{
			saddle = _saddle;
		}
		base.AttachDriver(driver, saddle);
		if (base.Driver.IsLocalPlayer)
		{
			PlayerController.MotionUpdater.UpdateRideMotionSet(PrefabName);
		}
		if ((bool)_lookAtController)
		{
			_lookAtController.SetLookTarget(null);
			_lookAtController.AutoChangeTarget = false;
		}
	}

	[ExposedInEditor(null)]
	public override bool SetupSaddle(bool setupSaddle = true, bool setupHelmet = true, bool hasRope = false)
	{
		if (setupSaddle && _saddle == null)
		{
			if (_saddlePrefab == null)
			{
				return true;
			}
			_saddle = Object.Instantiate(_saddlePrefab);
			_animal.BoneMergeable.AttachBoneMerge(_saddle);
		}
		if (setupHelmet && _helmet == null && !(_helmetPrefab == null))
		{
			_helmet = Object.Instantiate(_helmetPrefab);
			_animal.BoneMergeable.AttachBoneMerge(_helmet);
			if (hasRope)
			{
				RopeSetter component = _helmet.GetComponent<RopeSetter>();
				if (component != null)
				{
					component.InitRopes();
				}
			}
		}
		return false;
	}

	public override void DetachDriver()
	{
		base.DetachDriver();
		if (_lookAtController != null && base.Driver != null)
		{
			_lookAtController.SetLookTarget(base.Driver.gameObject, findHead: true);
			_lookAtController.AutoChangeTarget = true;
		}
	}

	public override float GetVehicleMotionLength()
	{
		AnimationState curAnimState = MotionPlayable.GetCurAnimState();
		return (!(curAnimState != null)) ? 0f : curAnimState.length;
	}

	protected override void AddInteractionMenus(InteractionMenuList menuList)
	{
		if (IsAlive)
		{
			if (IsRidable)
			{
				menuList.Add(new InteractionMenuData((!base.IsRidingMe) ? Interaction.Mount : Interaction.Dismount));
			}
			menuList.Add(new InteractionMenuData(Interaction.OpenPetInven));
			menuList.Add(new InteractionMenuData(Interaction.Feeding));
			menuList.Add(new InteractionMenuData(Interaction.ReturnPet));
			menuList.Add(new InteractionMenuData(Interaction.RenamePet));
			menuList.Add(new InteractionMenuData(Interaction.OpenPetMenu));
		}
		else
		{
			menuList.Add(new InteractionMenuData(Interaction.ResurrectPet));
		}
		string text = _animal.GetName();
		menuList.Name = text;
	}

	public override void ContextActionFinder(List<InteractionMenuData> result)
	{
		if (base.IsRidingMe)
		{
			result.Add(Interaction.Dismount);
		}
		else if (IsRidable)
		{
			result.Add(Interaction.Mount);
		}
	}

	private void LeanHeadAndTail()
	{
		if (!_animal.IsAlive || base.Driver == null || !base.Driver.IsRiding)
		{
			return;
		}
		float num = _animal.CurrentYaw - _prevYaw;
		_prevYaw = _animal.CurrentYaw;
		float num2;
		float num3;
		if (num > 0f)
		{
			num2 = _headMaxYaw;
			num3 = 0f - _tailMaxYaw;
		}
		else if (num < 0f)
		{
			num2 = 0f - _headMaxYaw;
			num3 = _tailMaxYaw;
		}
		else
		{
			num2 = 0f;
			num3 = 0f;
		}
		if (num2 != _curHeadYaw)
		{
			_curHeadYaw = Mathf.MoveTowardsAngle(_curHeadYaw, num2, Time.deltaTime * 80f);
		}
		if (_curHeadYaw != 0f)
		{
			if (_neckBone == null)
			{
				_neckBone = _animal.FindTransformByName("Bip001_Neck");
			}
			float num4 = _curHeadYaw;
			Transform transform = _animal.GetBodyPartTransform(BodyPart.Head);
			while (transform != null)
			{
				transform.Rotate(Vector3.up * num4, Space.World);
				if (transform == _neckBone)
				{
					break;
				}
				transform = transform.parent;
				num4 *= 0.8f;
			}
		}
		if (num3 != _curTailYaw)
		{
			_curTailYaw = Mathf.MoveTowardsAngle(_curTailYaw, num3, Time.deltaTime * 100f);
		}
		if (_curTailYaw != 0f)
		{
			if (_rootTailBone == null)
			{
				_rootTailBone = _animal.FindTransformByName("Bip001_Tail");
			}
			float num5 = _curTailYaw;
			Transform transform2 = _rootTailBone;
			while (transform2 != null)
			{
				transform2.Rotate(Vector3.up * num5, Space.World);
				transform2 = ((transform2.childCount <= 0) ? null : transform2.transform.GetChild(0));
				num5 *= 0.8f;
			}
		}
	}
}
