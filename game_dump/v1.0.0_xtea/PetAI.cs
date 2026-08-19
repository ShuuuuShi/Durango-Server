using System;
using System.Collections;
using System.Runtime.InteropServices;
using ItemSystem;
using KCollisionData;
using L10N;
using MapData;
using Shared.Animal;
using UnityEngine;

public class PetAI : StateBasedAI<PetAI.State>
{
	public enum State
	{
		Invalid = -1,
		SpawnInCage,
		RoamingInCage,
		IdleInCage,
		SpawnNearMaster,
		Normal,
		Chase,
		Idle,
		Riding,
		Return,
		EatOut,
		Count
	}

	private static int _globalPosIndex;

	private AnimalBehavior _targetAnimal;

	[SerializeField]
	private float _followDistance = 500f;

	[SerializeField]
	[Tooltip("이 거리를 벗어나면 텔레포트해서 주인 가까이로 옵니다.")]
	private float _maxFollowDistance = 3200f;

	[SerializeField]
	private float _distanceThreshould = 200f;

	private Vehicle _vehicle;

	private int _myPosIndex;

	private bool _isMapIndicatorAdded;

	private Vector3 _minArea = Vector3.zero;

	private Vector3 _maxArea = Vector3.zero;

	private bool _cancelRemove;

	private double _hungryTime = -1.0;

	private GameObject _mealProp;

	protected override State InvalidState => State.Invalid;

	protected override int StateEnumCount => 10;

	private AnimalBehavior TargetAnimal
	{
		get
		{
			if ((Object)null == (Object)(object)_targetAnimal)
			{
				_targetAnimal = ((Component)this).GetComponent<AnimalBehavior>();
			}
			return _targetAnimal;
		}
	}

	public string WalkMotion => _vehicle.WalkMotion;

	public string RunMotion => _vehicle.RunMotion;

	public string StandMotion => _vehicle.StandMotion;

	public string IdleMotion => _vehicle.IdleMotion;

	public string EatMotion => _vehicle.EatMotion;

	public bool InCage { get; private set; }

	private bool IsHungry
	{
		get
		{
			if (_hungryTime < 0.0)
			{
				return false;
			}
			return _hungryTime <= Connections.Frontend.GetPredictedServerTime();
		}
	}

	public string OwnerName => _vehicle.OwnerName;

	public event Action<CharacterBehavior> NameChanged;

	public void Init(GameObject master, bool inCage, bool isRiding, [Optional] Vector3 minArea, [Optional] Vector3 maxArea)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		base.Master = master;
		_vehicle = ((Component)this).GetComponent<Vehicle>();
		base.CurState = (isRiding ? State.Riding : ((!inCage) ? State.SpawnNearMaster : State.SpawnInCage));
		InCage = inCage;
		_minArea = minArea;
		_maxArea = maxArea;
	}

	protected override void OnAwake()
	{
		_myPosIndex = _globalPosIndex;
		_globalPosIndex++;
		TargetAnimal.SetServerSideRootMotionEnable(serverSideRootMotionEnabled: false);
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += PlayerInventoryUpdated;
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= PlayerInventoryUpdated;
	}

	protected override IEnumerator OnStart()
	{
		BoneLookAtTarget lookAt = ((Component)this).GetComponent<BoneLookAtTarget>();
		if ((Object)(object)lookAt != (Object)null)
		{
			lookAt.AutoChangeTarget = false;
		}
		while (!TerrainA6.IsPlayerInitialized)
		{
			yield return null;
		}
		if ((Object)(object)lookAt != (Object)null)
		{
			lookAt.SetLookTarget(base.Master, bFindHead: true);
		}
		AddToMapIndicator();
		PlayerInventoryUpdated();
	}

	private void PlayerInventoryUpdated()
	{
		ItemData itemData = GameSystem<InventorySystem>.Instance().FindItem(TargetAnimal.EntityId);
		if (itemData != null && itemData.Reins != null)
		{
			_hungryTime = itemData.Reins.Hungry.When(0f);
			TargetAnimal.SetName(itemData.Reins.PetName);
			UpdateHungryState();
			if (this.NameChanged != null)
			{
				this.NameChanged(TargetAnimal);
			}
		}
	}

	protected override IEnumerator OnBeforeDoingState()
	{
		UpdateHungryState();
		yield break;
	}

	private void UpdateHungryState()
	{
		TargetAnimal.Status = ((!IsHungry) ? AnimalStatus.Invalid : AnimalStatus.Hungry);
	}

	private void AddToMapIndicator()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!_isMapIndicatorAdded)
		{
			MapIconIndicator mapIconIndicator = MapIndicators.Add<MapIconIndicator>(TargetAnimal.EntityId, IndicatorType.Pet);
			mapIconIndicator.SetTarget(((Component)TargetAnimal).gameObject);
			mapIconIndicator.SetIcon("icon_map_otherplayer", PresetColor.UISkyBlue, 20, 30);
			_isMapIndicatorAdded = true;
		}
	}

	protected override void DefineStates()
	{
		AddState(State.SpawnInCage, new StateElem
		{
			Doing = SpawnInCageDoing
		});
		AddState(State.RoamingInCage, new StateElem
		{
			Doing = RoamingInCageDoing
		});
		AddState(State.IdleInCage, new StateElem
		{
			Doing = IdleInCageDoing
		});
		AddState(State.SpawnNearMaster, new StateElem
		{
			Doing = SpawnNearMasterDoing
		});
		AddState(State.Normal, new StateElem
		{
			Entered = NormalEntered,
			Doing = NormalDoing
		});
		AddState(State.Chase, new StateElem
		{
			Doing = ChaseDoing
		});
		AddState(State.Idle, new StateElem
		{
			Doing = IdleDoing
		});
		AddState(State.Riding, new StateElem
		{
			Doing = RidingDoing
		});
		AddState(State.Return, new StateElem
		{
			Doing = ReturnDoing
		});
		AddState(State.EatOut, new StateElem
		{
			Entered = EatOutEntered,
			Doing = EatOutDoing,
			Exited = EatOutExited
		});
	}

	protected override bool IsAIEnded()
	{
		return false;
	}

	protected override bool IsTerminalState(State state)
	{
		if (state == State.Return)
		{
			return true;
		}
		return false;
	}

	private IEnumerator SpawnNearMasterDoing()
	{
		while (!TerrainA6.IsPlayerInitialized)
		{
			yield return (object)new WaitForSeconds(0.5f);
		}
		Vector3 newPos = default(Vector3);
		for (int i = 0; i < 30; i++)
		{
			newPos = GetRandomMasterSurroundingPos(1000f);
			while (!TerrainA6.IsChunkLoaded(TerrainA6.ClientPositionToWorldPosition(newPos)))
			{
				yield return (object)new WaitForSeconds(0.5f);
			}
			if (!TerrainA6.IsCollidableMasked(TerrainA6.ClientPositionToWorldPosition(newPos)))
			{
				break;
			}
			if (i == 29)
			{
			}
		}
		TargetAnimal.CurrentPosition = newPos;
		TargetAnimal.TurnToYaw(Random.Range(0, 360), bSnap: true);
		base.CurState = State.Normal;
	}

	public void Tamed()
	{
		base.CurState = State.Normal;
	}

	private void NormalEntered()
	{
		TargetAnimal.CrossFade(playbackRate: _vehicle.PlaybackRate, motionName: StandMotion, fadeTime: 0.1f);
	}

	private IEnumerator NormalDoing()
	{
		if (!((Object)null == (Object)(object)base.Master))
		{
			base.CurState = ((!NeedToChaseMaster()) ? State.Idle : State.Chase);
			yield return (object)new WaitForSeconds(1f);
		}
	}

	private IEnumerator SpawnInCageDoing()
	{
		while (!TerrainA6.IsPlayerInitialized)
		{
			yield return null;
		}
		TargetAnimal.CurrentPosition = CalcRoamingPositionInCage();
		TargetAnimal.TurnToYaw(Random.Range(0, 360), bSnap: true);
		base.CurState = State.IdleInCage;
	}

	private IEnumerator RoamingInCageDoing()
	{
		TargetAnimal.CrossFade(playbackRate: _vehicle.PlaybackRate, motionName: WalkMotion, fadeTime: 0.1f);
		float prevTime = Time.time;
		Vector3 destPos = CalcRoamingPositionInCage();
		while (true)
		{
			if ((Object)null == (Object)(object)base.Master || base.IsInterrupted)
			{
				yield break;
			}
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			Vector3 disp = KMathUtil.Make2D(destPos - ((Component)this).transform.position);
			float distance = ((Vector3)(ref disp)).magnitude;
			if (distance <= 100f)
			{
				break;
			}
			Vector3 val = KMathUtil.Make2D(destPos - ((Component)this).transform.position);
			Vector3 dir = ((Vector3)(ref val)).normalized;
			TargetAnimal.TurnToYaw(KMathUtil.CalcYaw(dir), bSnap: false);
			Vector3 velocity = dir * _vehicle.WalkSpeed;
			AnimalBehavior targetAnimal = TargetAnimal;
			targetAnimal.CurrentPosition += velocity * dt;
			yield return null;
		}
		base.CurState = State.IdleInCage;
	}

	private IEnumerator IdleInCageDoing()
	{
		yield return ((MonoBehaviour)this).StartCoroutine(CoPlayMotion(playbackRate: Random.Range(0.8f, 1.2f) * _vehicle.PlaybackRate, motionName: IdleMotion, funcTransition: null, length: Random.Range(1, 10)));
		if (!base.IsInterrupted)
		{
			base.CurState = State.RoamingInCage;
		}
	}

	private Vector3 CalcRoamingPositionInCage()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Random.Range(_minArea.x, _maxArea.x), 0f, Random.Range(_minArea.z, _maxArea.z));
	}

	private bool NeedToChaseMaster()
	{
		return base.DistanceToMaster > _followDistance + _distanceThreshould;
	}

	private IEnumerator ChaseDoing()
	{
		TargetAnimal.CrossFade(playbackRate: _vehicle.PlaybackRate, motionName: RunMotion, fadeTime: 0.1f);
		float prevTime = Time.time;
		while (true)
		{
			if ((Object)null == (Object)(object)base.Master || base.IsInterrupted)
			{
				yield break;
			}
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			Vector3 disp = KMathUtil.Make2D(base.Master.transform.position - ((Component)this).transform.position);
			float distance = ((Vector3)(ref disp)).magnitude;
			if (distance <= _followDistance)
			{
				base.CurState = State.Normal;
				yield break;
			}
			if (distance > _maxFollowDistance)
			{
				break;
			}
			Vector3 destPos = CalcChasePosition();
			Vector3 val = KMathUtil.Make2D(destPos - ((Component)this).transform.position);
			Vector3 dir = ((Vector3)(ref val)).normalized;
			TargetAnimal.TurnToYaw(KMathUtil.CalcYaw(dir), bSnap: false);
			Vector3 velocity = dir * _vehicle.MoveSpeed;
			TargetAnimal.CurrentPosition = ProcessCollisionWithSliding(TargetAnimal.CurrentPosition, velocity * dt);
			yield return null;
		}
		base.CurState = State.SpawnNearMaster;
	}

	private Vector3 CalcChasePosition()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		int num = (_myPosIndex + 1) / 2;
		float num2 = (float)num * 20f;
		if (_myPosIndex % 2 == 0)
		{
			num2 = 0f - num2;
		}
		Vector3 val = Quaternion.Euler(0f, num2, 0f) * base.Master.transform.forward;
		Vector3 val2 = base.Master.transform.position + val * _followDistance;
		DebugExtension.DebugCircle(val2, 50f, 5f);
		return val2;
	}

	private IEnumerator IdleDoing()
	{
		yield return ((MonoBehaviour)this).StartCoroutine(CoPlayMotion(playbackRate: _vehicle.PlaybackRate, motionName: IdleMotion, funcTransition: delegate
		{
			if (NeedToChaseMaster())
			{
				base.CurState = State.Chase;
				return true;
			}
			return false;
		}));
		if (!base.IsInterrupted)
		{
			base.CurState = State.Normal;
		}
	}

	public void BeginRide()
	{
		base.CurState = State.Riding;
	}

	public void EndRide()
	{
		base.CurState = State.Normal;
	}

	private IEnumerator RidingDoing()
	{
		while (true)
		{
			if (base.IsInterrupted)
			{
				yield break;
			}
			if (IsHungry)
			{
				break;
			}
			yield return null;
		}
		Vehicle.RequestUnmountIfRiding(immediately: true);
		UIManager.SystemMsg(T._("음식을 먹여야 탑승할 수 있습니다."));
		base.CurState = State.Normal;
	}

	public void Return()
	{
		if (!InCage)
		{
			_cancelRemove = false;
			base.CurState = State.Return;
		}
	}

	public bool IsReturning()
	{
		return base.CurState == State.Return;
	}

	private IEnumerator ReturnDoing()
	{
		TargetAnimal.CrossFade(playbackRate: _vehicle.PlaybackRate, motionName: RunMotion, fadeTime: 0.1f);
		Vector3 val = KMathUtil.Make2D(((Component)this).transform.position - base.Master.transform.position);
		Vector3 dir2 = ((Vector3)(ref val)).normalized;
		if (dir2 == Vector3.zero)
		{
			dir2 = Vector3.right;
		}
		Vector3 returnPos = ((Component)this).transform.position + dir2 * 3000f;
		float prevTime = Time.time;
		while (true)
		{
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			if (_cancelRemove)
			{
				yield break;
			}
			Vector3 val2 = KMathUtil.Make2D(returnPos - ((Component)this).transform.position);
			float distance = ((Vector3)(ref val2)).magnitude;
			if (distance <= 100f)
			{
				break;
			}
			Vector3 val3 = KMathUtil.Make2D(returnPos - ((Component)this).transform.position);
			dir2 = ((Vector3)(ref val3)).normalized;
			TargetAnimal.TurnToYaw(KMathUtil.CalcYaw(dir2), bSnap: false);
			Vector3 velocity = dir2 * _vehicle.MoveSpeed;
			AnimalBehavior targetAnimal = TargetAnimal;
			targetAnimal.CurrentPosition += velocity * dt;
			yield return null;
		}
		if (!_cancelRemove)
		{
			RemovePet();
		}
		_cancelRemove = false;
	}

	public void ReturnToCage(GameObject cageOwner)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Artifact component = cageOwner.GetComponent<Artifact>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		Cage artifactComponent = component.GetArtifactComponent<Cage>();
		if (artifactComponent != null)
		{
			if (IsReturning())
			{
				_cancelRemove = true;
			}
			Init(cageOwner, inCage: true, isRiding: false, artifactComponent.MinArea, artifactComponent.MaxArea);
			TransitionTo(State.RoamingInCage, force: true);
		}
	}

	private void RemovePet()
	{
		if (Object.op_Implicit((Object)(object)TargetAnimal))
		{
			KSingleton<AnimalManager>.Instance().RemoveAnimal(TargetAnimal);
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void EatOut()
	{
		base.CurState = State.EatOut;
	}

	private void EatOutEntered()
	{
		string path = "Models/Prop/tool/basket_feed_01.prefab";
		KSingleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(GameObject), delegate(Object asset)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = (GameObject)asset;
			if (!((Object)(object)val == (Object)null))
			{
				Animation componentInChildren = ((Component)this).GetComponentInChildren<Animation>();
				componentInChildren.Sample();
				Vector3 position = ((Component)this).transform.position + ((Component)this).transform.forward * _vehicle.EatDistance;
				position.y = 0f;
				_mealProp = (GameObject)Object.Instantiate(asset);
				_mealProp.transform.position = position;
				_mealProp.transform.rotation = Quaternion.identity;
			}
		});
	}

	private IEnumerator EatOutDoing()
	{
		yield return ((MonoBehaviour)this).StartCoroutine(CoPlayMotion(playbackRate: _vehicle.PlaybackRate, motionName: EatMotion, funcTransition: null, length: 10f));
		if (!base.IsInterrupted)
		{
			base.CurState = State.Normal;
		}
	}

	private void EatOutExited()
	{
		Object.Destroy((Object)(object)_mealProp);
	}

	public bool IsMyMaster(GameObject gameObj)
	{
		return (Object)(object)base.Master == (Object)(object)gameObj;
	}

	private IEnumerator CoPlayMotion(string motionName, Func<bool> funcTransition, float length = -1f, float fadeInTime = 0.1f, float playbackRate = 1f)
	{
		bool isLoop = length > 0f;
		TargetAnimal.CrossFade(motionName, fadeInTime, isLoop, 0f, playbackRate);
		length = Mathf.Max(length, TargetAnimal.CurAnimState.length);
		float prevTime = Time.time;
		while (!(Time.time - prevTime >= length) && !base.IsInterrupted && (funcTransition == null || !funcTransition()))
		{
			yield return null;
		}
	}

	private Vector3 ProcessCollisionWithSliding(Vector3 beginPos, Vector3 delta)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (delta == Vector3.zero)
		{
			return beginPos;
		}
		CollisionParam param = KCollisionUtility.CreateCollisionParam(beginPos, delta);
		delta = KCollisionUtility.ProcessSimpleSliding(param);
		return beginPos + delta;
	}
}
