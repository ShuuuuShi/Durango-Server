using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Algorithms;
using MapData;
using Shared.Battle;
using UnityEngine;

public class NpcAIDog : StateBasedAI<NpcAIDog.State>
{
	public enum State
	{
		Invalid = -1,
		FirstIntroStates = 0,
		PrepareIntroToMMO = 0,
		IntroToMMO = 1,
		AfterCure = 2,
		IntroduceDog = 3,
		LastIntroStates = 3,
		Normal = 4,
		Chase = 5,
		MoveToPOI = 6,
		Aggress = 7,
		Bark = 8,
		Happy = 9,
		Idle = 10,
		Farewell = 11,
		Count = 12
	}

	public class StateCandidate : WeightedCandidate
	{
		public State NextState;
	}

	private static StateCandidate[] _randomStateCandidatesAtNormal;

	private IPathFinder _pathFinder;

	private bool _isMapIndicatorAdded;

	[SerializeField]
	private string _bikeLickMotion = "Dog_Bike_Lick";

	[SerializeField]
	private Vector3 _introPosFromPlayer = new Vector3(81f, 0f, 65f);

	[SerializeField]
	private float _introYaw = 118f;

	[SerializeField]
	private string _introMotion = "Dog_Bike_Begin";

	[SerializeField]
	private string _introSitMotion = "Wolf_Sit_Looping_A";

	[SerializeField]
	private float _introSitDuringTime = 7f;

	[SerializeField]
	private string _introSitEndMotion = "Dog_Sit_End";

	[SerializeField]
	private Vector3 _locationOffsetAfterCure = new Vector3(-52f, 0f, 300f);

	[SerializeField]
	private string _standMotion = "Dog_Stand";

	[SerializeField]
	private string _runMotion = "Dog_Run";

	[SerializeField]
	private string _walkMotion = "Dog_Walk";

	[SerializeField]
	private string _barkMotion = "Dog_Bark";

	[SerializeField]
	private string _happyMotion = "Dog_Jump";

	[SerializeField]
	private float _reactDistance = 500f;

	[SerializeField]
	private string _turnMotion = "Dog_Turn";

	[SerializeField]
	private string _idleMotion = "Dog_Idle";

	[SerializeField]
	private float _followDistanceMin = 50f;

	[SerializeField]
	private float _followDistanceMax = 700f;

	[SerializeField]
	private float _walkToPOIDistance = 500f;

	[SerializeField]
	private float _runSpeed = 500f;

	[SerializeField]
	private float _walkSpeed = 200f;

	[SerializeField]
	private float _appearDiatanceFromPlayer = 1000f;

	[SerializeField]
	private float _distanceThreshould = 200f;

	[SerializeField]
	private Vector2 _poiTilePos = Vector2.op_Implicit(Vector3.zero);

	[SerializeField]
	private TextAsset _navigationGridAsset;

	[SerializeField]
	private string _beaconName = "Beacon";

	[SerializeField]
	private float _barkDurationMin = 1f;

	[SerializeField]
	private float _barkDurationMax = 3f;

	[SerializeField]
	private float _barkAtNormalProbability = 0.1f;

	[SerializeField]
	private float _chaseAtNormalProbability = 0.3f;

	[SerializeField]
	private float _idleAtNormalProbability = 0.1f;

	[SerializeField]
	private float _standAtNormalProbability = 0.5f;

	[SerializeField]
	private float _barkAfterChaseProbability = 0.3f;

	[SerializeField]
	private float _barkAfterAggressProbability = 0.3f;

	[SerializeField]
	private string _farewellSoundPath = "Sound/Effect/Voice/Animals/Direwolf/VO_Direwolf_Idle_02_B.wav";

	[SerializeField]
	private float _turnMotionAcivateAngle = 120f;

	private Vector3 _initialPos;

	private AnimalBehavior _targetAnimal;

	private byte[,] _navGrid;

	private GameObject _beacon;

	private bool IsMasterMoreCloseToPOI => (Object)(object)base.Master != (Object)null && DistanceMasterToPOI < DistanceToPOI;

	private float DistanceToPOI
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = POIClientPosition - ((Component)this).transform.position;
			return ((Vector3)(ref val)).magnitude;
		}
	}

	private float DistanceMasterToPOI
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = POIClientPosition - base.MasterPos;
			return ((Vector3)(ref val)).magnitude;
		}
	}

	protected override State InvalidState => State.Invalid;

	protected override int StateEnumCount => 12;

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

	private Vector3 POIClientPosition => TerrainA6.WorldPositionToClientPosition(TerrainA6.TilePositionToWorldPosition(_poiTilePos));

	protected override void DefineStates()
	{
		AddState(State.FirstIntroStates, new StateElem
		{
			Entered = PrepareIntroToMMOEntered,
			Doing = PrepareIntroToMMODoing,
			Exited = PrepareIntroToMMOExited
		});
		AddState(State.IntroToMMO, new StateElem
		{
			Entered = IntroToMMOEntered,
			Doing = IntroToMMODoing,
			Exited = IntroToMMOExited
		});
		AddState(State.AfterCure, new StateElem
		{
			Doing = AfterCureDoing
		});
		AddState(State.IntroduceDog, new StateElem
		{
			Doing = IntroduceDogDoing
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
		AddState(State.MoveToPOI, new StateElem
		{
			Doing = MoveToPOIDoing
		});
		AddState(State.Aggress, new StateElem
		{
			Doing = AggressDoing
		});
		AddState(State.Bark, new StateElem
		{
			Doing = BarkDoing
		});
		AddState(State.Happy, new StateElem
		{
			Doing = HappyDoing
		});
		AddState(State.Idle, new StateElem
		{
			Doing = IdleDoing
		});
		AddState(State.Farewell, new StateElem
		{
			Doing = FarewellDoing
		});
	}

	protected override void OnAwake()
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(_navigationGridAsset.bytes))
		{
			_navGrid = (byte[,])binaryFormatter.Deserialize(serializationStream);
		}
		_pathFinder = new PathFinderFast(_navGrid);
		TargetAnimal.SetServerSideRootMotionEnable(serverSideRootMotionEnabled: false);
		_beacon = KUtility.FindObjectByName(((Component)this).gameObject, _beaconName);
		SoundManager.Cache(_farewellSoundPath);
	}

	protected override IEnumerator OnStart()
	{
		TargetAnimal.EntityId = 666uL;
		if (!IsInIntroStates())
		{
			base.CurState = State.Chase;
			BoneLookAtTarget lookAt = ((Component)this).GetComponent<BoneLookAtTarget>();
			lookAt.AutoChangeTarget = false;
			while (!TerrainA6.IsPlayerInitialized)
			{
				yield return null;
			}
			Vector3 worldCenter = TerrainA6.WorldPositionToClientPosition(new Vector3(512f, 512f));
			NpcAIDog npcAIDog = this;
			Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
			Vector3 val = worldCenter - PlayerBehavior.LocalPlayer.CurrentPosition;
			npcAIDog._initialPos = currentPosition + ((Vector3)(ref val)).normalized * _appearDiatanceFromPlayer;
			_initialPos.y = 0f;
			TargetAnimal.CurrentPosition = _initialPos;
			AddToMapIndicator();
		}
	}

	protected override IEnumerator OnBeforeDoingState()
	{
		BoneLookAtTarget lookAt = ((Component)this).GetComponent<BoneLookAtTarget>();
		base.Master = ((Component)PlayerBehavior.LocalPlayer).gameObject;
		if ((Object)(object)base.Master == (Object)null)
		{
			yield return (object)new WaitForSeconds(1f);
		}
		else
		{
			lookAt.SetLookTarget(((Component)PlayerBehavior.LocalPlayer).gameObject, bFindHead: true);
		}
	}

	protected override IEnumerator OnAfterDoingState()
	{
		yield break;
	}

	protected override bool IsAIEnded()
	{
		return false;
	}

	protected override bool IsTerminalState(State state)
	{
		return false;
	}

	private void OnDestroy()
	{
		MapIndicators.Remove(TargetAnimal.EntityId, IndicatorType.GuideDog);
	}

	private bool IsInIntroStates()
	{
		return State.FirstIntroStates <= base.CurState && base.CurState <= State.IntroduceDog;
	}

	public void MoveCloseToPlayer()
	{
		if (!IsInIntroStates())
		{
			base.CurState = State.Chase;
		}
	}

	private void AddToMapIndicator()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!_isMapIndicatorAdded)
		{
			MapIconIndicator mapIconIndicator = MapIndicators.Add<MapIconIndicator>(TargetAnimal.EntityId, IndicatorType.GuideDog);
			mapIconIndicator.SetTarget(((Component)TargetAnimal).gameObject);
			mapIconIndicator.SetIcon("icon_map_otherplayer", PresetColor.UISkyBlue, 20, 30);
			_isMapIndicatorAdded = true;
		}
	}

	public void SetPOIPosTile(Vector2 tilePos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		AddToMapIndicator();
		_poiTilePos = tilePos;
		if (!IsInIntroStates())
		{
			base.CurState = ((!IsMasterMoreCloseToPOI) ? State.Chase : State.MoveToPOI);
		}
	}

	public Vector2 GetPOIPosTile()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _poiTilePos;
	}

	public void SetPOIPos(Vector3 clientPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SetPOIPosTile(TerrainA6.ClientPositionToTilePosition(clientPos));
	}

	public void SetFarewellTile(Vector2 tilePos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_poiTilePos = tilePos;
		base.CurState = State.Farewell;
	}

	public void PrepareIntroMMO()
	{
		base.CurState = State.FirstIntroStates;
	}

	private void PrepareIntroToMMOEntered()
	{
		TargetAnimal.Play(_bikeLickMotion);
		RepositionToIntro();
	}

	private void PrepareIntroToMMOExited()
	{
	}

	private IEnumerator PrepareIntroToMMODoing()
	{
		yield return null;
	}

	[ExposedInEditor("Intro 위치 새로 잡기")]
	public void RepositionToIntro()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = ((Component)PlayerBehavior.LocalPlayer.GetBodyPartTransform(BodyPart.Head)).gameObject;
		Vector3 position = gameObject.transform.position + _introPosFromPlayer;
		position.y = 0f;
		((Component)this).transform.position = position;
		((Component)this).transform.rotation = Quaternion.Euler(0f, _introYaw, 0f);
	}

	public void PlayIntroAnim()
	{
		base.CurState = State.IntroToMMO;
	}

	private void IntroToMMOEntered()
	{
		if ((Object)(object)_beacon != (Object)null)
		{
			_beacon.SetActive(false);
		}
		RepositionToIntro();
	}

	private void IntroToMMOExited()
	{
	}

	private IEnumerator IntroToMMODoing()
	{
		TargetAnimal.Play(_introMotion, loop: false);
		yield return (object)new WaitForSeconds(TargetAnimal.CurAnimState.length);
		PlayAndFitLocation(_introSitMotion);
		yield return (object)new WaitForSeconds(_introSitDuringTime);
		base.CurState = State.AfterCure;
	}

	public void RestoreStandingKCutScene()
	{
		RepositionToIntro();
		base.CurState = State.AfterCure;
	}

	private IEnumerator AfterCureDoing()
	{
		PlayAndFitLocation(_introSitEndMotion, loop: false);
		yield return (object)new WaitForSeconds(TargetAnimal.CurAnimState.length - 0.15f);
		CrossFadeAndFitLocation(_standMotion, 0.1f);
		yield return (object)new WaitForSeconds(2f);
		Vector3 destPos = ((Component)this).transform.position + _locationOffsetAfterCure;
		yield return ((MonoBehaviour)this).StartCoroutine(CoMoveTo(() => destPos, null, _walkMotion, _walkSpeed, endAtReached: true, 0.2f));
		yield return ((MonoBehaviour)this).StartCoroutine(CoTurnAndCrossFadeMotion(_standMotion, 0.2f));
		while (!base.IsInterrupted)
		{
			yield return null;
		}
	}

	public void Dog_Introduce()
	{
		base.CurState = State.IntroduceDog;
	}

	private IEnumerator IntroduceDogDoing()
	{
		base.CurState = State.Normal;
		if ((Object)(object)_beacon != (Object)null)
		{
			_beacon.SetActive(true);
		}
		AddToMapIndicator();
		yield break;
	}

	public void NormalEntered()
	{
		TargetAnimal.CrossFade(_standMotion, 0.1f);
	}

	private IEnumerator NormalDoing()
	{
		if ((Object)null == (Object)(object)base.Master)
		{
			yield break;
		}
		if (NeedToChaseMaster())
		{
			base.CurState = State.Chase;
		}
		else if (NeedToTransitionMoveToPOI())
		{
			base.CurState = State.MoveToPOI;
		}
		else
		{
			if (_randomStateCandidatesAtNormal == null)
			{
				_randomStateCandidatesAtNormal = new StateCandidate[4]
				{
					new StateCandidate
					{
						Weight = _barkAtNormalProbability,
						NextState = State.Bark
					},
					new StateCandidate
					{
						Weight = _chaseAtNormalProbability,
						NextState = State.Chase
					},
					new StateCandidate
					{
						Weight = _idleAtNormalProbability,
						NextState = State.Idle
					},
					new StateCandidate
					{
						Weight = _standAtNormalProbability,
						NextState = State.Normal
					}
				};
			}
			StateCandidate candidate = WeightedCandidate.Select(_randomStateCandidatesAtNormal);
			if (candidate == null)
			{
				yield break;
			}
			if (candidate.NextState == State.Normal)
			{
				TargetAnimal.CrossFade(_standMotion, 0.1f);
				float newYaw = KMathUtil.CalcYawWithTarget(base.MasterPos, ((Component)this).transform.position);
				if (KMathUtil.DistanceAngDeg(newYaw, TargetAnimal.CurrentYaw) > _turnMotionAcivateAngle)
				{
					yield return ((MonoBehaviour)this).StartCoroutine(CoTurnAndCrossFadeMotion(_standMotion, 0.1f));
				}
				else
				{
					CrossFadeAndFitLocation(_standMotion, 0.1f);
				}
				TargetAnimal.TurnToYaw(newYaw, bSnap: false);
			}
			else
			{
				base.CurState = candidate.NextState;
			}
		}
		yield return (object)new WaitForSeconds(1f);
	}

	private bool NeedToChaseMaster()
	{
		return base.DistanceToMaster > _followDistanceMax + _distanceThreshould;
	}

	private bool NeedToEndChaseMaster()
	{
		return base.DistanceToMaster < _followDistanceMin;
	}

	private bool NeedToTransitionMoveToPOI()
	{
		return base.DistanceToMaster < _followDistanceMax && DistanceToPOI > 500f;
	}

	private bool NeedToEndMoveToPOI()
	{
		return base.DistanceToMaster >= _followDistanceMax || DistanceToPOI < 100f;
	}

	private bool NeedToUnAgressToMaster()
	{
		return base.DistanceToMaster < _followDistanceMin;
	}

	private IEnumerator ChaseDoing()
	{
		yield return ((MonoBehaviour)this).StartCoroutine(CoMoveTo(() => base.MasterPos, ChaseTransitions, _runMotion, _runSpeed));
	}

	private bool ChaseTransitions()
	{
		if (NeedToEndChaseMaster())
		{
			if (IsMasterMoreCloseToPOI)
			{
				base.CurState = State.MoveToPOI;
				return true;
			}
			base.CurState = ((!(Random.value < _barkAfterChaseProbability)) ? State.Normal : State.Bark);
			return true;
		}
		return false;
	}

	private bool CheckWalk(bool wasLastMoveWalk)
	{
		float num = 100f;
		if (wasLastMoveWalk)
		{
			num = -100f;
		}
		if (DistanceMasterToPOI > DistanceToPOI && base.DistanceToMaster > _walkToPOIDistance + num)
		{
			return true;
		}
		return false;
	}

	private IEnumerator MoveToPOIDoing()
	{
		yield return ((MonoBehaviour)this).StartCoroutine(CoMoveToWithPathFind(MoveToPOIDestPos, MoveToPOITransitions, CheckWalk, _runMotion, _runSpeed, _walkMotion, _walkSpeed));
	}

	private Vector3 MoveToPOIDestPos()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return POIClientPosition;
	}

	private bool MoveToPOITransitions()
	{
		if (NeedToEndMoveToPOI())
		{
			base.CurState = State.Normal;
			return true;
		}
		if (NeedToChaseMaster())
		{
			base.CurState = State.Aggress;
			return true;
		}
		return false;
	}

	private IEnumerator AggressDoing()
	{
		yield return ((MonoBehaviour)this).StartCoroutine(CoMoveTo(() => base.MasterPos, AggressTransitions, _runMotion, _runSpeed));
	}

	private bool AggressTransitions()
	{
		if (NeedToUnAgressToMaster())
		{
			base.CurState = ((!(Random.value < _barkAfterAggressProbability)) ? State.Normal : State.Bark);
			return true;
		}
		return false;
	}

	private IEnumerator BarkDoing()
	{
		float newYaw2 = KMathUtil.CalcYawWithTarget(base.MasterPos, ((Component)this).transform.position);
		float duration = Random.Range(_barkDurationMin, _barkDurationMax);
		if (KMathUtil.DistanceAngDeg(newYaw2, TargetAnimal.CurrentYaw) > _turnMotionAcivateAngle)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(CoTurnAndCrossFadeMotion(_barkMotion, 0.1f));
		}
		else
		{
			CrossFadeAndFitLocation(_barkMotion, 0.1f);
		}
		float endTime = Time.time + duration;
		while (Time.time < endTime)
		{
			if (IsMasterMoreCloseToPOI)
			{
				base.CurState = State.MoveToPOI;
				yield break;
			}
			if (base.IsInterrupted)
			{
				yield break;
			}
			newYaw2 = KMathUtil.CalcYawWithTarget(base.MasterPos, ((Component)this).transform.position);
			TargetAnimal.TurnToYaw(newYaw2, bSnap: false);
			yield return null;
		}
		base.CurState = State.Normal;
	}

	public void Dog_Happy()
	{
		base.CurState = State.Happy;
	}

	private IEnumerator HappyDoing()
	{
		if (base.DistanceToMaster > _reactDistance)
		{
			Vector3 destPos = CalcMasterNearestPos(_reactDistance);
			yield return ((MonoBehaviour)this).StartCoroutine(CoMoveTo(() => destPos, null, _runMotion, _runSpeed, endAtReached: true));
		}
		TargetAnimal.CrossFade(_happyMotion, 0.1f, loop: false);
		yield return (object)new WaitForSeconds(TargetAnimal.CurAnimState.length);
		base.CurState = State.Normal;
	}

	private IEnumerator IdleDoing()
	{
		if (base.DistanceToMaster > _reactDistance)
		{
			Vector3 destPos = CalcMasterNearestPos(_reactDistance);
			yield return ((MonoBehaviour)this).StartCoroutine(CoMoveTo(() => destPos, null, _runMotion, _runSpeed, endAtReached: true));
		}
		TargetAnimal.CrossFade(_idleMotion, 0.1f, loop: false);
		yield return (object)new WaitForSeconds(TargetAnimal.CurAnimState.length);
		base.CurState = State.Normal;
	}

	private IEnumerator FarewellDoing()
	{
		Vector3 masterPos = base.MasterPos;
		Vector3 val = ((Component)this).transform.position - base.MasterPos;
		Vector3 goodByePos = masterPos + ((Vector3)(ref val)).normalized * 200f;
		yield return ((MonoBehaviour)this).StartCoroutine(CoMoveTo(() => goodByePos, null, _runMotion, _runSpeed, endAtReached: true));
		if ((Object)(object)_beacon != (Object)null)
		{
			_beacon.SetActive(false);
		}
		SoundManager.Play(_farewellSoundPath, ((Component)this).transform.position);
		for (int j = 0; j < 2; j++)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(BarkToPlayer(1.5f));
			Vector3 destPos = GetRandomMasterSurroundingPos(300f);
			yield return ((MonoBehaviour)this).StartCoroutine(CoMoveTo(() => destPos, null, _walkMotion, _walkSpeed, endAtReached: true));
		}
		SoundManager.Play(_farewellSoundPath, ((Component)this).transform.position);
		for (int i = 0; i < 2; i++)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(BarkToPlayer(1.5f));
			Vector3 position = ((Component)this).transform.position;
			Vector3 val2 = POIClientPosition - ((Component)this).transform.position;
			Vector3 lastGoodByePos = position + ((Vector3)(ref val2)).normalized * 250f;
			yield return ((MonoBehaviour)this).StartCoroutine(CoMoveTo(() => lastGoodByePos, null, _walkMotion, _walkSpeed, endAtReached: true));
		}
		yield return ((MonoBehaviour)this).StartCoroutine(BarkToPlayer(5f));
		yield return ((MonoBehaviour)this).StartCoroutine(CoMoveTo(() => POIClientPosition, null, _runMotion, _runSpeed, endAtReached: true));
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private IEnumerator BarkToPlayer(float duration)
	{
		float newYaw2 = KMathUtil.CalcYawWithTarget(base.MasterPos, ((Component)this).transform.position);
		if (KMathUtil.DistanceAngDeg(newYaw2, TargetAnimal.CurrentYaw) > _turnMotionAcivateAngle)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(CoTurnAndCrossFadeMotion(_barkMotion, 0.1f));
		}
		else
		{
			CrossFadeAndFitLocation(_barkMotion, 0.1f);
		}
		float endTime = Time.time + duration;
		while (Time.time < endTime)
		{
			newYaw2 = KMathUtil.CalcYawWithTarget(base.MasterPos, ((Component)this).transform.position);
			TargetAnimal.TurnToYaw(newYaw2, bSnap: false);
			yield return null;
		}
	}

	private IEnumerator CoMoveTo(Func<Vector3> funcTargetPos, Func<bool> funcTransition, string moveMotion, float moveSpeed, bool endAtReached = false, float fadeInTime = 0.1f)
	{
		bool isMoving = false;
		float prevTime = Time.time;
		while (!((Object)null == (Object)(object)base.Master) && (funcTransition == null || !funcTransition()))
		{
			float newYaw = KMathUtil.CalcYawWithTarget(funcTargetPos(), ((Component)this).transform.position);
			if (!isMoving && KMathUtil.DistanceAngDeg(newYaw, TargetAnimal.CurrentYaw) > _turnMotionAcivateAngle)
			{
				yield return ((MonoBehaviour)this).StartCoroutine(CoTurnAndCrossFadeMotion(moveMotion, fadeInTime));
				isMoving = true;
				prevTime = Time.time;
			}
			else if (!isMoving)
			{
				CrossFadeAndFitLocation(moveMotion, fadeInTime);
				isMoving = true;
			}
			if (base.IsInterrupted)
			{
				break;
			}
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			Vector3 disp = KMathUtil.Make2D(funcTargetPos() - ((Component)this).transform.position);
			if (endAtReached && ((Vector3)(ref disp)).magnitude < 100f)
			{
				break;
			}
			float destYaw = KMathUtil.CalcYawWithTarget(funcTargetPos(), ((Component)this).transform.position);
			TargetAnimal.TurnToYaw(destYaw, bSnap: false);
			Vector3 velocity = ((Vector3)(ref disp)).normalized * moveSpeed;
			AnimalBehavior targetAnimal = TargetAnimal;
			targetAnimal.CurrentPosition += velocity * dt;
			yield return null;
		}
	}

	private IEnumerator CoMoveToWithPathFind(Func<Vector3> funcTargetPos, Func<bool> funcTransition, Func<bool, bool> funcCheckWalk, string runMotion, float runSpeed, string walkMotion, float walkSpeed)
	{
		float prevTime = Time.time;
		Vector2 startTilePos = TerrainA6.ClientPositionToTilePosition(((Component)this).transform.position);
		Vector2 endTilePos = TerrainA6.ClientPositionToTilePosition(funcTargetPos());
		List<PathFinderNode> pathNodes = _pathFinder.FindPath(new Point((int)startTilePos.x, (int)startTilePos.y), new Point((int)endTilePos.x, (int)endTilePos.y));
		if (pathNodes == null)
		{
			base.CurState = State.Chase;
			yield break;
		}
		List<Vector3> paths = new List<Vector3>();
		foreach (PathFinderNode node in pathNodes)
		{
			Vector3 pos = TerrainA6.TilePositionToClientPosition(new Vector2((float)node.X, (float)node.Y));
			paths.Add(pos);
		}
		paths.Reverse();
		paths.RemoveAt(0);
		paths.Add(funcTargetPos());
		bool isMoving = false;
		bool wasLastMoveWalk = false;
		Vector3 destPos = paths[0];
		paths.RemoveAt(0);
		while (!((Object)null == (Object)(object)base.Master))
		{
			bool isWalk = funcCheckWalk?.Invoke(wasLastMoveWalk) ?? false;
			bool moveMotionChanged = wasLastMoveWalk != isWalk;
			wasLastMoveWalk = isWalk;
			string moveMotion = ((!isWalk) ? runMotion : walkMotion);
			float moveSpeed = ((!isWalk) ? runSpeed : walkSpeed);
			float newYaw = KMathUtil.CalcYawWithTarget(destPos, ((Component)this).transform.position);
			if (!isMoving && KMathUtil.DistanceAngDeg(newYaw, TargetAnimal.CurrentYaw) > _turnMotionAcivateAngle)
			{
				yield return ((MonoBehaviour)this).StartCoroutine(CoTurnAndCrossFadeMotion(moveMotion, 0.1f));
				isMoving = true;
				prevTime = Time.time;
			}
			else if (!isMoving || moveMotionChanged)
			{
				CrossFadeAndFitLocation(moveMotion, 0.1f);
				isMoving = true;
			}
			if (base.IsInterrupted)
			{
				break;
			}
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			if (funcTransition != null && funcTransition())
			{
				break;
			}
			float destYaw = KMathUtil.CalcYawWithTarget(destPos, ((Component)this).transform.position);
			TargetAnimal.TurnToYaw(destYaw, bSnap: false);
			Vector3 disp = KMathUtil.Make2D(destPos - ((Component)this).transform.position);
			if (((Vector3)(ref disp)).magnitude < 200f && paths.Count > 0)
			{
				destPos = paths[0];
				paths.RemoveAt(0);
			}
			Vector3 velocity = ((Vector3)(ref disp)).normalized * moveSpeed;
			AnimalBehavior targetAnimal = TargetAnimal;
			targetAnimal.CurrentPosition += velocity * dt;
			yield return null;
		}
	}

	private IEnumerator CoTurnAndCrossFadeMotion(string afterTurnMotionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		TargetAnimal.CrossFade(_turnMotion, fadeTime, loop: false);
		yield return (object)new WaitForSeconds(TargetAnimal.CurAnimState.length);
		FixUpRootBoneAndCrossFadeMotion(afterTurnMotionName, fadeTime, loop, beginTime, playbackRate);
	}

	private void FixUpRootBoneAndCrossFadeMotion(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 localToWorldMatrix = TargetAnimal.Bip001Transform.localToWorldMatrix;
		TargetAnimal.Play(_standMotion);
		TargetAnimal.Anim.Sample();
		Matrix4x4 val = Matrix4x4.TRS(TargetAnimal.Bip001Transform.localPosition, TargetAnimal.Bip001Transform.localRotation, TargetAnimal.Bip001Transform.localScale);
		Matrix4x4 m = localToWorldMatrix * Matrix4x4.Inverse(val);
		KMathUtil.DecomposeMatrix(m, out var position, out var rotation, out var _);
		TargetAnimal.TurnToYaw(((Quaternion)(ref rotation)).eulerAngles.y, bSnap: true);
		TargetAnimal.CurrentPosition = position;
		TargetAnimal.CrossFade(motionName, fadeTime, loop, beginTime, playbackRate);
	}

	private void CrossFadeAndFitLocation(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (!loop || !((TrackedReference)(object)TargetAnimal.CurAnimState != (TrackedReference)null) || !(TargetAnimal.CurAnimState.name == motionName))
		{
			Vector3 position = TargetAnimal.Bip001Transform.position;
			TargetAnimal.CrossFade(motionName, fadeTime, loop, beginTime, playbackRate);
			TargetAnimal.Anim.Sample();
			Vector3 pos = position - TargetAnimal.Bip001Transform.position;
			AnimalBehavior targetAnimal = TargetAnimal;
			targetAnimal.CurrentPosition += KMathUtil.Make2D(pos);
		}
	}

	private void PlayAndFitLocation(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!loop || !((TrackedReference)(object)TargetAnimal.CurAnimState != (TrackedReference)null) || !(TargetAnimal.CurAnimState.name == motionName))
		{
			Vector3 position = TargetAnimal.Bip001Transform.position;
			TargetAnimal.Play(motionName, loop, beginTime, playbackRate);
			TargetAnimal.Anim.Sample();
			Vector3 pos = position - TargetAnimal.Bip001Transform.position;
			AnimalBehavior targetAnimal = TargetAnimal;
			targetAnimal.CurrentPosition += KMathUtil.Make2D(pos);
		}
	}
}
