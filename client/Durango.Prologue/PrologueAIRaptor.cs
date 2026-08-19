using System.Collections;
using Durango.Model;
using Durango.Network;
using Durango.Render.Particle;
using Durango.Utils;
using Messages;
using Shared.Battle;
using UnityEngine;

namespace Durango.Prologue;

public class PrologueAIRaptor : StateBasedAI<PrologueAIRaptor.State>
{
	public enum State
	{
		Invalid = -1,
		WaitForBattleBegin,
		WaitForPreparePlayer,
		Normal,
		Chase,
		Flinch,
		Blow,
		Leap,
		Roaming,
		Dead,
		Count
	}

	public class MoveInfo : WeightedCandidate
	{
		public string Motion;

		public float MoveSpeed;
	}

	public static readonly string FakeEntityId = "666";

	[SerializeField]
	private string _standMotion = "Raptor_Stand";

	[SerializeField]
	private string _walkMotion = "Raptor_Walk";

	[SerializeField]
	private float _walkSpeed = 120f;

	[SerializeField]
	private float _moveWalkRatio = 0.33f;

	[SerializeField]
	private string _runMotion = "Raptor_Run";

	[SerializeField]
	private float _runSpeed = 360f;

	[SerializeField]
	private float _moveRunRatio = 0.33f;

	[SerializeField]
	private string _limpMotion = "Raptor_Limp";

	[SerializeField]
	private float _limpSpeed = 120f;

	[SerializeField]
	private float _moveLimpRatio = 0.33f;

	[SerializeField]
	private float _attackCoolTime = 4f;

	[SerializeField]
	private float _engageDistance = 300f;

	[SerializeField]
	private float _hitRatio = 0.5f;

	[SerializeField]
	private float _criticalRatio = 0.1f;

	[SerializeField]
	private int _damageMin = 1;

	[SerializeField]
	private int _damageMax = 20;

	[SerializeField]
	private int _damageCritical = 100;

	[SerializeField]
	private string _leapMotion = "Raptor_Attack_Jump";

	[SerializeField]
	private string _flinchMotion = "Raptor_Flinch";

	[SerializeField]
	private string _blowMotion = "Raptor_DamageBlow";

	[SerializeField]
	private string _groggyMotion = "Raptor_Groggy";

	[SerializeField]
	private string _threatMotion = "Raptor_Threat";

	[SerializeField]
	private Vector3 _minArea = Vector3.zero;

	[SerializeField]
	private Vector3 _maxArea = Vector3.zero;

	[SerializeField]
	private Vector3 _deadDestPos;

	[SerializeField]
	private Vector3 _deadDestPosOriginOffset = new Vector3(0f, 0f, 3000f);

	[SerializeField]
	private float _roamingRadiusFromPlayer = 500f;

	[SerializeField]
	private float _maxLeapDist = 500f;

	[SerializeField]
	private float _playTrexCutSceneDelay = 1f;

	[SerializeField]
	private float _deadYaw = -90f;

	private float _attackCooldownEnd;

	private GameObject _victim;

	private AnimalBehavior _targetAnimal;

	private int _attackCount;

	private bool _isDeadPhase;

	private static MoveInfo[] _randomMoveCandidates;

	private float _flinchEndTime;

	[SerializeField]
	private float _blowDistance = 500f;

	[SerializeField]
	private float _blowTime = 0.3f;

	private bool _threatedFlagForGuide;

	protected override State InvalidState => State.Invalid;

	private AnimalBehavior TargetAnimal
	{
		get
		{
			if (null == _targetAnimal)
			{
				_targetAnimal = GetComponent<AnimalBehavior>();
				Gauge life = new Gauge(100f, 0f, new GaugeNode[1]
				{
					new GaugeNode
					{
						Time = 0.0,
						Value = 100f
					}
				});
				_targetAnimal.SetSurvivalGauge(life, null);
			}
			return _targetAnimal;
		}
	}

	protected override int StateEnumCount => 9;

	protected override void DefineStates()
	{
		AddState(State.WaitForBattleBegin, new StateElem
		{
			Entered = WaitForBattleBeginEntered,
			Doing = WaitForBattleBeginDoing,
			Exited = WaitForBattleBeginExited
		});
		AddState(State.WaitForPreparePlayer, new StateElem
		{
			Doing = WaitForPreparePlayerDoing
		});
		AddState(State.Normal, new StateElem
		{
			Entered = NormalEntered,
			Doing = NormalDoing,
			Exited = NormalExited
		});
		AddState(State.Chase, new StateElem
		{
			Entered = ChaseEntered,
			Doing = ChaseDoing,
			Exited = ChaseExited
		});
		AddState(State.Flinch, new StateElem
		{
			Entered = FlinchEntered,
			Doing = FlinchDoing,
			Exited = FlinchExited
		});
		AddState(State.Blow, new StateElem
		{
			Entered = BlowEntered,
			Doing = BlowDoing,
			Exited = BlowExited
		});
		AddState(State.Leap, new StateElem
		{
			Entered = LeapEntered,
			Doing = LeapDoing,
			Exited = LeapExited
		});
		AddState(State.Roaming, new StateElem
		{
			Entered = RoamingEntered,
			Doing = RoamingDoing,
			Exited = RoamingExited
		});
		AddState(State.Dead, new StateElem
		{
			Entered = DeadEntered,
			Doing = DeadDoing,
			Exited = DeadExited
		});
	}

	protected override bool IsAIEnded()
	{
		return _isDeadPhase;
	}

	protected override bool IsTerminalState(State state)
	{
		return state == State.Dead;
	}

	protected override IEnumerator OnStart()
	{
		TargetAnimal.EntityId = FakeEntityId;
		Singleton<AnimalManager>.Instance().ForceAddAnimal(TargetAnimal.EntityId, GetComponent<AnimalBehavior>());
		base.CurState = State.WaitForBattleBegin;
		BoneLookAtTarget component = GetComponent<BoneLookAtTarget>();
		component.AutoChangeTarget = false;
		TargetAnimal.Play(_standMotion);
		_deadDestPos += _deadDestPosOriginOffset;
		PinUpRootBone();
		yield break;
	}

	protected override IEnumerator OnBeforeDoingState()
	{
		_victim = PlayerBehavior.LocalPlayer.gameObject;
		if (_victim == null)
		{
			yield return new WaitForSeconds(1f);
			yield break;
		}
		BoneLookAtTarget lookAt = GetComponent<BoneLookAtTarget>();
		lookAt.SetLookTarget(_victim);
	}

	protected override IEnumerator OnAfterDoingState()
	{
		yield break;
	}

	public void SetAiActivated()
	{
		base.CurState = State.WaitForPreparePlayer;
	}

	private void WaitForBattleBeginEntered()
	{
		TargetAnimal.CrossFade(_standMotion, 0.1f);
	}

	private void WaitForBattleBeginExited()
	{
	}

	private IEnumerator WaitForBattleBeginDoing()
	{
		yield return null;
	}

	private IEnumerator WaitForPreparePlayerDoing()
	{
		TargetAnimal.CrossFade(_threatMotion);
		yield return new WaitForSeconds(1.5f);
		base.CurState = State.Normal;
	}

	private void NormalEntered()
	{
		TargetAnimal.CrossFade(_standMotion, 0.1f);
	}

	private void NormalExited()
	{
	}

	private IEnumerator NormalDoing()
	{
		if (null != _victim)
		{
			float magnitude = (_victim.transform.position - base.transform.position).magnitude;
			if (magnitude > _engageDistance)
			{
				base.CurState = State.Chase;
			}
			else if (Time.time > _attackCooldownEnd)
			{
				base.CurState = State.Leap;
			}
			else
			{
				base.CurState = State.Roaming;
			}
		}
		yield return new WaitForSeconds(0.3f);
	}

	private void ChaseEntered()
	{
	}

	private void ChaseExited()
	{
	}

	private IEnumerator ChaseDoing()
	{
		TargetAnimal.CrossFade(_walkMotion, 0.1f);
		float prevTime = Time.time;
		while (true)
		{
			if (null == _victim || base.IsInterrupted)
			{
				yield break;
			}
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			Vector3 disp = Maths.Make2D(_victim.transform.position - base.transform.position);
			float distance = disp.magnitude;
			if (distance <= _engageDistance)
			{
				break;
			}
			float destYaw = Maths.CalcYawWithTarget(_victim.transform.position, base.transform.position);
			TargetAnimal.TurnToYaw(destYaw, bSnap: false);
			Vector3 velocity = disp.normalized * _walkSpeed;
			TargetAnimal.CurrentPosition += velocity * dt;
			TargetAnimal.CurrentPosition = new Vector3(Mathf.Clamp(TargetAnimal.CurrentPosition.x, _minArea.x, _maxArea.x), 0f, Mathf.Clamp(TargetAnimal.CurrentPosition.z, _minArea.z, _maxArea.z));
			yield return null;
		}
		if (Time.time > _attackCooldownEnd)
		{
			base.CurState = State.Leap;
		}
		else
		{
			base.CurState = State.Roaming;
		}
	}

	private void CheckAndMakeDamageToPlayer()
	{
		Damage damage = default(Damage);
		if (_attackCount == 0)
		{
			damage.Value = 0;
			damage.Result = DamageResult.Dodged;
		}
		else if (Maths.Make2D(PlayerBehavior.LocalPlayer.CurrentPosition - base.transform.position).magnitude < 300f && Random.value < _hitRatio)
		{
			damage.Result = DamageResult.Hit;
		}
		else
		{
			damage.Result = DamageResult.Missed;
		}
		if (damage.Result == DamageResult.Hit)
		{
			if (Random.value < _criticalRatio)
			{
				damage.Effects |= DamageEffects.Critical;
			}
			damage.Value = CalcDamage((damage.Effects & DamageEffects.Critical) != 0);
			damage.Effects |= DamageEffects.Blow;
		}
		damage.AttackType = AttackType.SmallTear;
		damage.Direction = DamageDirection.Front;
		damage.Part = BodyPart.Body;
		_attackCount++;
		Connections.Frontend.PushPacket(new Damaged
		{
			AttackerId = FakeEntityId,
			Damage = damage,
			VictimId = GameManager.PlayerId
		});
	}

	private int CalcDamage(bool isCriticalHit)
	{
		if (isCriticalHit)
		{
			return _damageCritical;
		}
		return Random.Range(_damageMin, _damageMax);
	}

	public void EventFlinch()
	{
		base.CurState = State.Flinch;
	}

	private void FlinchEntered()
	{
		TargetAnimal.Play(_flinchMotion, loop: false);
		_flinchEndTime = Time.time + TargetAnimal.CurAnimState.length;
	}

	private void FlinchExited()
	{
	}

	private IEnumerator FlinchDoing()
	{
		yield return new WaitForSeconds(_flinchEndTime - Time.time);
		base.CurState = State.Roaming;
	}

	public void EventBlow()
	{
		base.CurState = State.Blow;
	}

	private void BlowEntered()
	{
		TargetAnimal.Play(_blowMotion, loop: false);
		Vector3 vector = Maths.ProjectDirection(PlayerBehavior.LocalPlayer.transform);
		vector.y = 0f;
		vector.z = 0f;
		vector = vector.normalized;
		TargetAnimal.TurnToYaw(Maths.CalcYaw(vector) + 180f, bSnap: true);
		Vector3 destPos = TargetAnimal.CurrentPosition + vector * _blowDistance;
		destPos = ClampPos(destPos);
		TweenPosition tweenPosition = TweenPosition.Begin(TargetAnimal.gameObject, _blowTime, destPos);
		tweenPosition.method = UITweener.Method.EaseInOut;
		tweenPosition.PlayForward();
		PinUpRootBone();
	}

	private Vector3 ClampPos(Vector3 destPos)
	{
		destPos.x = Mathf.Clamp(destPos.x, _minArea.x, _maxArea.x);
		destPos.z = Mathf.Clamp(destPos.z, _minArea.z, _maxArea.z);
		return destPos;
	}

	private void BlowExited()
	{
	}

	private IEnumerator BlowDoing()
	{
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length);
		base.CurState = State.Normal;
	}

	private void RoamingEntered()
	{
	}

	private void RoamingExited()
	{
	}

	private IEnumerator RoamingDoing()
	{
		Vector3 newPos = PlayerBehavior.LocalPlayer.CurrentPosition + new Vector3(Random.Range(0f - _roamingRadiusFromPlayer, _roamingRadiusFromPlayer), 0f, Random.Range(0f - _roamingRadiusFromPlayer, _roamingRadiusFromPlayer));
		newPos = ClampPos(newPos);
		if (_randomMoveCandidates == null)
		{
			_randomMoveCandidates = new MoveInfo[3]
			{
				new MoveInfo
				{
					Weight = _moveRunRatio,
					Motion = _runMotion,
					MoveSpeed = _runSpeed
				},
				new MoveInfo
				{
					Weight = _moveWalkRatio,
					Motion = _walkMotion,
					MoveSpeed = _walkSpeed
				},
				new MoveInfo
				{
					Weight = _moveLimpRatio,
					Motion = _limpMotion,
					MoveSpeed = _limpSpeed
				}
			};
		}
		MoveInfo candidate = WeightedCandidate.Select(_randomMoveCandidates);
		if (candidate == null)
		{
			yield break;
		}
		string moveMotion = candidate.Motion;
		float moveSpeed = candidate.MoveSpeed;
		TargetAnimal.CrossFade(moveMotion, 0.1f);
		float prevTime = Time.time;
		while (true)
		{
			if (null == _victim || base.IsInterrupted)
			{
				yield break;
			}
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			Vector3 disp = Maths.Make2D(newPos - base.transform.position);
			float distance = disp.magnitude;
			if (distance <= 50f)
			{
				break;
			}
			float destYaw = Maths.CalcYawWithTarget(newPos, base.transform.position);
			TargetAnimal.TurnToYaw(destYaw, bSnap: false);
			Vector3 velocity = disp.normalized * moveSpeed;
			TargetAnimal.CurrentPosition += velocity * dt;
			yield return null;
		}
		base.CurState = State.Normal;
	}

	private void LeapEntered()
	{
		_attackCooldownEnd = Time.time + _attackCoolTime;
	}

	private void LeapExited()
	{
	}

	private IEnumerator LeapDoing()
	{
		Vector3 newPos = Vector3.MoveTowards(TargetAnimal.transform.position, PlayerBehavior.LocalPlayer.CurrentPosition, _maxLeapDist);
		float destYaw = Maths.CalcYawWithTarget(newPos, base.transform.position);
		TargetAnimal.TurnToYaw(destYaw, bSnap: false);
		UnPinUpRootBone();
		TargetAnimal.Play(_threatMotion, loop: false);
		ParticleManager.Emit("Particle/FX_SkillActivated_Common_01.prefab", TargetAnimal.CurrentPosition, Quaternion.identity);
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length);
		if (base.IsInterrupted)
		{
			yield break;
		}
		if (!_threatedFlagForGuide)
		{
			GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.LearnDodge);
			_threatedFlagForGuide = true;
		}
		yield return new WaitForSeconds(0.01f);
		PinUpRootBone();
		TargetAnimal.Play(_leapMotion, loop: false);
		TweenPosition tween = TweenPosition.Begin(TargetAnimal.gameObject, TargetAnimal.CurAnimState.length, newPos);
		tween.method = UITweener.Method.EaseInOut;
		tween.PlayForward();
		float attackAt = Time.time + 0.6f;
		bool attacked = false;
		while (tween.enabled)
		{
			if (base.IsInterrupted)
			{
				tween.enabled = false;
				yield break;
			}
			if (!attacked && Time.time >= attackAt)
			{
				CheckAndMakeDamageToPlayer();
				attacked = true;
			}
			yield return null;
		}
		base.CurState = State.Normal;
	}

	private static TweenScale CreateTweenScale(string buttonPath)
	{
		Transform transform = Singleton<UIManager>.Instance().FindTransform(buttonPath);
		if ((bool)transform)
		{
			TweenScale tweenScale = transform.gameObject.AddComponent<TweenScale>();
			tweenScale.to = new Vector3(1.2f, 1.2f, 1.2f);
			tweenScale.duration = 0.2f;
			tweenScale.style = UITweener.Style.PingPong;
			return tweenScale;
		}
		return null;
	}

	public void EventDead()
	{
		base.CurState = State.Dead;
		_isDeadPhase = true;
	}

	private void DeadEntered()
	{
		PinUpRootBone();
		TargetAnimal.Play(_blowMotion, loop: false);
		TargetAnimal.TurnToYaw(_deadYaw, bSnap: false);
		TweenPosition tweenPosition = TweenPosition.Begin(TargetAnimal.gameObject, _playTrexCutSceneDelay, _deadDestPos);
		tweenPosition.method = UITweener.Method.EaseInOut;
		tweenPosition.PlayForward();
		GameSystem<PrologueGuideSystem>.Instance().HideGuideMask();
		Singleton<PrologueManager>.Instance().DelayedCall(BeginFinalCutScene, _playTrexCutSceneDelay);
	}

	private void DeadExited()
	{
		UnPinUpRootBone();
	}

	private IEnumerator DeadDoing()
	{
		yield return new WaitForSeconds(_playTrexCutSceneDelay);
	}

	private void BeginFinalCutScene()
	{
		Singleton<TrainTrexController>.Instance().PlayTrexCutScene();
		Object.Destroy(base.gameObject);
	}

	private void PinUpRootBone()
	{
		TargetAnimal.SetActivateRootMotion(active: true);
	}

	private void UnPinUpRootBone()
	{
		TargetAnimal.SetActivateRootMotion(active: false);
		TargetAnimal.MeshObjectTransform.localPosition = Vector3.zero;
	}

	public void OnTakeDamage(Damage damage, bool isDead)
	{
		if (damage.Value > 0 && !_isDeadPhase)
		{
			if (isDead)
			{
				EventDead();
				PlayerBehavior.LocalPlayer.OnKilledAnimal(base.gameObject.GetComponent<AnimalBehavior>());
			}
			else if ((damage.Effects & DamageEffects.Blow) > DamageEffects.None)
			{
				EventBlow();
			}
			else if ((damage.Effects & DamageEffects.KnockBack) > DamageEffects.None)
			{
				EventFlinch();
			}
		}
	}
}
