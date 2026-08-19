using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Algorithms;
using Durango.Logic.Map;
using Durango.Model;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
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

	private static readonly WaitForSeconds WaitForOneSecond = new WaitForSeconds(1f);

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
	private Vector2 _poiTilePos = Vector3.zero;

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
	private SoundEventType _farewellSound;

	[SerializeField]
	private float _turnMotionAcivateAngle = 120f;

	private Vector3 _initialPos;

	private AnimalBehavior _targetAnimal;

	private byte[,] _navGrid;

	private GameObject _beacon;

	private bool IsMasterMoreCloseToPOI => base.Master != null && DistanceMasterToPOI < DistanceToPOI;

	private float DistanceToPOI => (POIClientPosition - base.transform.position).magnitude;

	private float DistanceMasterToPOI => (POIClientPosition - base.MasterPos).magnitude;

	protected override State InvalidState => State.Invalid;

	protected override int StateEnumCount => 12;

	private AnimalBehavior TargetAnimal
	{
		get
		{
			if (null == _targetAnimal)
			{
				_targetAnimal = GetComponent<AnimalBehavior>();
			}
			return _targetAnimal;
		}
	}

	private Vector3 POIClientPosition => Util.WorldPositionToClientPosition(Util.TilePositionToWorldPosition(_poiTilePos));

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
		TargetAnimal.SetActivateRootMotion(active: false);
		_beacon = KUtility.FindObjectByName(base.gameObject, _beaconName);
		SoundManager.PrepareEvent(_farewellSound);
	}

	protected override IEnumerator OnStart()
	{
		TargetAnimal.EntityId = "666";
		if (!IsInIntroStates())
		{
			base.CurState = State.Chase;
			BoneLookAtTarget lookAt = GetComponent<BoneLookAtTarget>();
			lookAt.AutoChangeTarget = false;
			while (!TerrainBase.IsPlayerInitialized)
			{
				yield return null;
			}
			Vector3 worldCenter = Util.WorldPositionToClientPosition(new Vector3(512f, 512f));
			_initialPos = PlayerBehavior.LocalPlayer.CurrentPosition + (worldCenter - PlayerBehavior.LocalPlayer.CurrentPosition).normalized * _appearDiatanceFromPlayer;
			_initialPos.y = 0f;
			TargetAnimal.CurrentPosition = _initialPos;
			AddToMapIndicator();
		}
	}

	protected override IEnumerator OnBeforeDoingState()
	{
		BoneLookAtTarget lookAt = GetComponent<BoneLookAtTarget>();
		base.Master = PlayerBehavior.LocalPlayer.gameObject;
		if (base.Master == null)
		{
			yield return WaitForOneSecond;
		}
		else
		{
			lookAt.SetLookTarget(PlayerBehavior.LocalPlayer.gameObject, findHead: true);
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
		return state == State.Farewell;
	}

	private void OnDisable()
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
		if (!_isMapIndicatorAdded)
		{
			MapIconIndicator orAdd = MapIndicators.GetOrAdd<MapIconIndicator>(TargetAnimal.EntityId, IndicatorType.GuideDog);
			orAdd.SetTarget(TargetAnimal.gameObject);
			orAdd.SetIcon("icon_map_animal", PresetColor.UISkyBlue, 16, 30);
			_isMapIndicatorAdded = true;
		}
	}

	public void SetPOIPosTile(Vector2 tilePos)
	{
		AddToMapIndicator();
		_poiTilePos = tilePos;
		if (!IsInIntroStates())
		{
			base.CurState = ((!IsMasterMoreCloseToPOI) ? State.Chase : State.MoveToPOI);
		}
	}

	public Vector2 GetPOIPosTile()
	{
		return _poiTilePos;
	}

	public void SetPOIPos(Vector3 clientPos)
	{
		SetPOIPosTile(Util.ClientPositionToTilePosition(clientPos));
	}

	public void SetFarewellTile(Vector2 tilePos)
	{
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
		GameObject gameObject = PlayerBehavior.LocalPlayer.GetBodyPartTransform(BodyPart.Head).gameObject;
		Vector3 position = gameObject.transform.position + _introPosFromPlayer;
		position.y = 0f;
		base.transform.position = position;
		base.transform.rotation = Quaternion.Euler(0f, _introYaw, 0f);
	}

	public void PlayIntroAnim()
	{
		base.CurState = State.IntroToMMO;
	}

	private void IntroToMMOEntered()
	{
		if (_beacon != null)
		{
			_beacon.SetActive(value: false);
		}
		RepositionToIntro();
	}

	private void IntroToMMOExited()
	{
	}

	private IEnumerator IntroToMMODoing()
	{
		TargetAnimal.Play(_introMotion, loop: false);
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length);
		TargetAnimal.PlayAndFitLocation(_introSitMotion);
		yield return new WaitForSeconds(_introSitDuringTime);
		base.CurState = State.AfterCure;
	}

	public void RestoreStandingKCutScene()
	{
		RepositionToIntro();
		base.CurState = State.AfterCure;
	}

	private IEnumerator AfterCureDoing()
	{
		TargetAnimal.PlayAndFitLocation(_introSitEndMotion, loop: false);
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length - 0.15f);
		CrossFadeAndFitLocation(_standMotion, 0.1f);
		yield return new WaitForSeconds(2f);
		Vector3 destPos = base.transform.position + _locationOffsetAfterCure;
		yield return StartCoroutine(CoMoveTo(() => destPos, null, _walkMotion, _walkSpeed, endAtReached: true, 0.2f));
		yield return StartCoroutine(CoTurnAndCrossFadeMotion(_standMotion, 0.2f));
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
		if (_beacon != null)
		{
			_beacon.SetActive(value: true);
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
		if (null == base.Master)
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
				float newYaw = Maths.CalcYawWithTarget(base.MasterPos, base.transform.position);
				if (Maths.DistanceAngDeg(newYaw, TargetAnimal.CurrentYaw) > _turnMotionAcivateAngle)
				{
					yield return StartCoroutine(CoTurnAndCrossFadeMotion(_standMotion, 0.1f));
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
		yield return WaitForOneSecond;
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
		yield return StartCoroutine(CoMoveTo(() => base.MasterPos, ChaseTransitions, _runMotion, _runSpeed));
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
			base.CurState = ((!(UnityEngine.Random.value < _barkAfterChaseProbability)) ? State.Normal : State.Bark);
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
		yield return StartCoroutine(CoMoveToWithPathFind(MoveToPOIDestPos, MoveToPOITransitions, CheckWalk, _runMotion, _runSpeed, _walkMotion, _walkSpeed));
	}

	private Vector3 MoveToPOIDestPos()
	{
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
		yield return StartCoroutine(CoMoveTo(() => base.MasterPos, AggressTransitions, _runMotion, _runSpeed));
	}

	private bool AggressTransitions()
	{
		if (NeedToUnAgressToMaster())
		{
			base.CurState = ((!(UnityEngine.Random.value < _barkAfterAggressProbability)) ? State.Normal : State.Bark);
			return true;
		}
		return false;
	}

	private IEnumerator BarkDoing()
	{
		float newYaw2 = Maths.CalcYawWithTarget(base.MasterPos, base.transform.position);
		float duration = UnityEngine.Random.Range(_barkDurationMin, _barkDurationMax);
		if (Maths.DistanceAngDeg(newYaw2, TargetAnimal.CurrentYaw) > _turnMotionAcivateAngle)
		{
			yield return StartCoroutine(CoTurnAndCrossFadeMotion(_barkMotion, 0.1f));
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
			newYaw2 = Maths.CalcYawWithTarget(base.MasterPos, base.transform.position);
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
			yield return StartCoroutine(CoMoveTo(() => destPos, null, _runMotion, _runSpeed, endAtReached: true));
		}
		TargetAnimal.CrossFade(_happyMotion, 0.1f, loop: false);
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length);
		base.CurState = State.Normal;
	}

	private IEnumerator IdleDoing()
	{
		if (base.DistanceToMaster > _reactDistance)
		{
			Vector3 destPos = CalcMasterNearestPos(_reactDistance);
			yield return StartCoroutine(CoMoveTo(() => destPos, null, _runMotion, _runSpeed, endAtReached: true));
		}
		TargetAnimal.CrossFade(_idleMotion, 0.1f, loop: false);
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length);
		base.CurState = State.Normal;
	}

	private IEnumerator FarewellDoing()
	{
		Vector3 goodByePos = base.MasterPos + (base.transform.position - base.MasterPos).normalized * 200f;
		yield return StartCoroutine(CoMoveTo(() => goodByePos, null, _runMotion, _runSpeed, endAtReached: true));
		if (_beacon != null)
		{
			_beacon.SetActive(value: false);
		}
		SoundManager.PlayEvent(_farewellSound, SoundPosition.Chase(base.transform.gameObject));
		for (int j = 0; j < 2; j++)
		{
			yield return StartCoroutine(BarkToPlayer(1.5f));
			Vector3 destPos = GetRandomMasterSurroundingPos(300f);
			yield return StartCoroutine(CoMoveTo(() => destPos, null, _walkMotion, _walkSpeed, endAtReached: true));
		}
		SoundManager.PlayEvent(_farewellSound, SoundPosition.Chase(base.transform.gameObject));
		for (int i = 0; i < 2; i++)
		{
			yield return StartCoroutine(BarkToPlayer(1.5f));
			Vector3 lastGoodByePos = base.transform.position + (POIClientPosition - base.transform.position).normalized * 250f;
			yield return StartCoroutine(CoMoveTo(() => lastGoodByePos, null, _walkMotion, _walkSpeed, endAtReached: true));
		}
		yield return StartCoroutine(BarkToPlayer(5f));
		yield return StartCoroutine(CoMoveTo(() => POIClientPosition, null, _runMotion, _runSpeed, endAtReached: true));
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator BarkToPlayer(float duration)
	{
		float newYaw2 = Maths.CalcYawWithTarget(base.MasterPos, base.transform.position);
		if (Maths.DistanceAngDeg(newYaw2, TargetAnimal.CurrentYaw) > _turnMotionAcivateAngle)
		{
			yield return StartCoroutine(CoTurnAndCrossFadeMotion(_barkMotion, 0.1f));
		}
		else
		{
			CrossFadeAndFitLocation(_barkMotion, 0.1f);
		}
		float endTime = Time.time + duration;
		while (Time.time < endTime)
		{
			newYaw2 = Maths.CalcYawWithTarget(base.MasterPos, base.transform.position);
			TargetAnimal.TurnToYaw(newYaw2, bSnap: false);
			yield return null;
		}
	}

	private IEnumerator CoMoveTo(Func<Vector3> funcTargetPos, Func<bool> funcTransition, string moveMotion, float moveSpeed, bool endAtReached = false, float fadeInTime = 0.1f)
	{
		bool isMoving = false;
		float prevTime = Time.time;
		while (!(null == base.Master) && (funcTransition == null || !funcTransition()))
		{
			float newYaw = Maths.CalcYawWithTarget(funcTargetPos(), base.transform.position);
			if (!isMoving && Maths.DistanceAngDeg(newYaw, TargetAnimal.CurrentYaw) > _turnMotionAcivateAngle)
			{
				yield return StartCoroutine(CoTurnAndCrossFadeMotion(moveMotion, fadeInTime));
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
			Vector3 disp = Maths.Make2D(funcTargetPos() - base.transform.position);
			if (endAtReached && disp.magnitude < 100f)
			{
				break;
			}
			float destYaw = Maths.CalcYawWithTarget(funcTargetPos(), base.transform.position);
			TargetAnimal.TurnToYaw(destYaw, bSnap: false);
			Vector3 velocity = disp.normalized * moveSpeed;
			TargetAnimal.CurrentPosition += velocity * dt;
			yield return null;
		}
	}

	private IEnumerator CoMoveToWithPathFind(Func<Vector3> funcTargetPos, Func<bool> funcTransition, Func<bool, bool> funcCheckWalk, string runMotion, float runSpeed, string walkMotion, float walkSpeed)
	{
		float prevTime = Time.time;
		Vector2 startTilePos = Util.ClientPositionToTilePosition(base.transform.position);
		Vector2 endTilePos = Util.ClientPositionToTilePosition(funcTargetPos());
		List<PathFinderNode> pathNodes = _pathFinder.FindPath(new Point((int)startTilePos.x, (int)startTilePos.y), new Point((int)endTilePos.x, (int)endTilePos.y));
		if (pathNodes == null)
		{
			base.CurState = State.Chase;
			yield break;
		}
		List<Vector3> paths = new List<Vector3>();
		foreach (PathFinderNode item2 in pathNodes)
		{
			Vector3 item = Util.TilePositionToClientPosition(new Vector2(item2.X, item2.Y));
			paths.Add(item);
		}
		paths.Reverse();
		paths.RemoveAt(0);
		paths.Add(funcTargetPos());
		bool isMoving = false;
		bool wasLastMoveWalk = false;
		Vector3 destPos = paths[0];
		paths.RemoveAt(0);
		while (!(null == base.Master))
		{
			bool isWalk = funcCheckWalk?.Invoke(wasLastMoveWalk) ?? false;
			bool moveMotionChanged = wasLastMoveWalk != isWalk;
			wasLastMoveWalk = isWalk;
			string moveMotion = ((!isWalk) ? runMotion : walkMotion);
			float moveSpeed = ((!isWalk) ? runSpeed : walkSpeed);
			float newYaw = Maths.CalcYawWithTarget(destPos, base.transform.position);
			if (!isMoving && Maths.DistanceAngDeg(newYaw, TargetAnimal.CurrentYaw) > _turnMotionAcivateAngle)
			{
				yield return StartCoroutine(CoTurnAndCrossFadeMotion(moveMotion, 0.1f));
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
			float destYaw = Maths.CalcYawWithTarget(destPos, base.transform.position);
			TargetAnimal.TurnToYaw(destYaw, bSnap: false);
			Vector3 disp = Maths.Make2D(destPos - base.transform.position);
			if (disp.magnitude < 200f && paths.Count > 0)
			{
				destPos = paths[0];
				paths.RemoveAt(0);
			}
			Vector3 velocity = disp.normalized * moveSpeed;
			TargetAnimal.CurrentPosition += velocity * dt;
			yield return null;
		}
	}

	private IEnumerator CoTurnAndCrossFadeMotion(string afterTurnMotionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		TargetAnimal.CrossFade(_turnMotion, fadeTime, loop: false);
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length);
		FixUpRootBoneAndCrossFadeMotion(afterTurnMotionName, fadeTime, loop, beginTime, playbackRate);
	}

	private void FixUpRootBoneAndCrossFadeMotion(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		Matrix4x4 localToWorldMatrix = TargetAnimal.Bip001Transform.localToWorldMatrix;
		TargetAnimal.Play(_standMotion);
		TargetAnimal.Anim.Sample();
		Matrix4x4 m = Matrix4x4.TRS(TargetAnimal.Bip001Transform.localPosition, TargetAnimal.Bip001Transform.localRotation, TargetAnimal.Bip001Transform.localScale);
		Matrix4x4 m2 = localToWorldMatrix * Matrix4x4.Inverse(m);
		Maths.DecomposeMatrix(m2, out var position, out var rotation, out var _);
		TargetAnimal.TurnToYaw(rotation.eulerAngles.y, bSnap: true);
		TargetAnimal.CurrentPosition = position;
		TargetAnimal.CrossFade(motionName, fadeTime, loop, beginTime, playbackRate);
	}

	private void CrossFadeAndFitLocation(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		if (!loop || !(TargetAnimal.CurAnimState != null) || !(TargetAnimal.CurAnimState.name == motionName))
		{
			Vector3 position = TargetAnimal.Bip001Transform.position;
			TargetAnimal.CrossFade(motionName, fadeTime, loop, beginTime, playbackRate);
			TargetAnimal.Anim.Sample();
			Vector3 pos = position - TargetAnimal.Bip001Transform.position;
			TargetAnimal.CurrentPosition += Maths.Make2D(pos);
		}
	}
}
