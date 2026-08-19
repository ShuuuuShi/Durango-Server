using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using JetBrains.Annotations;
using Messages;
using PlayerExtensionsPrologue;
using Shared.Battle;
using UnityEngine;

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
		Groggy,
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

	private const int MaxGroggy = 100;

	public static readonly ulong FakeEntityId = 666uL;

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
	private float _offenseRatio = 0.8f;

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
	private float _groggyTime = 7f;

	[SerializeField]
	private float _playTrexCutSceneDelay = 1f;

	[SerializeField]
	private float _deadYaw = -90f;

	private float _attackCoolTimeEnd;

	private GameObject _victim;

	private AnimalBehavior _targetAnimal;

	private int _curGroggy = 100;

	private TweenScale _dodgeTweenScale;

	private TweenScale _strongAttackTweenScale;

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
			if ((Object)null == (Object)(object)_targetAnimal)
			{
				_targetAnimal = ((Component)this).GetComponent<AnimalBehavior>();
			}
			return _targetAnimal;
		}
	}

	[CanBeNull]
	private TweenScale DodgeTweenScale
	{
		get
		{
			if ((Object)(object)_dodgeTweenScale != (Object)null)
			{
				return _dodgeTweenScale;
			}
			_dodgeTweenScale = CreateTweenScale("ActionButtonGroup/Actions/ActiveAction_2");
			return _dodgeTweenScale;
		}
	}

	[CanBeNull]
	private TweenScale StrongAttackTweenScale
	{
		get
		{
			if ((Object)(object)_strongAttackTweenScale != (Object)null)
			{
				return _strongAttackTweenScale;
			}
			_strongAttackTweenScale = CreateTweenScale("ActionButtonGroup/Actions/ActiveAction_1");
			return _strongAttackTweenScale;
		}
	}

	protected override int StateEnumCount => 10;

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
		AddState(State.Groggy, new StateElem
		{
			Entered = GroggyEntered,
			Doing = GroggyDoing,
			Exited = GroggyExited
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
		KSingleton<ObjectManager>.Instance().ForceAddAnimal(TargetAnimal.EntityId, ((Component)this).GetComponent<AnimalBehavior>());
		base.CurState = State.WaitForBattleBegin;
		BoneLookAtTarget lookAt = ((Component)this).GetComponent<BoneLookAtTarget>();
		lookAt.AutoChangeTarget = false;
		TargetAnimal.Play(_standMotion);
		PrologueAIRaptor prologueAIRaptor = this;
		prologueAIRaptor._deadDestPos += _deadDestPosOriginOffset;
		PinUpRootBone();
		yield break;
	}

	protected override IEnumerator OnBeforeDoingState()
	{
		_victim = ((Component)PlayerBehavior.LocalPlayer).gameObject;
		if ((Object)(object)_victim == (Object)null)
		{
			yield return (object)new WaitForSeconds(1f);
			yield break;
		}
		BoneLookAtTarget lookAt = ((Component)this).GetComponent<BoneLookAtTarget>();
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

	public void SetAiAllAttack()
	{
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
		yield return (object)new WaitForSeconds(1.5f);
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
		if ((Object)null != (Object)(object)_victim)
		{
			Vector3 val = _victim.transform.position - ((Component)this).transform.position;
			float distance = ((Vector3)(ref val)).magnitude;
			if (distance > _engageDistance)
			{
				base.CurState = State.Chase;
			}
			else if (Random.value < _offenseRatio && Time.time > _attackCoolTimeEnd)
			{
				base.CurState = State.Leap;
			}
			else
			{
				base.CurState = State.Roaming;
			}
		}
		yield return (object)new WaitForSeconds(0.3f);
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
			if ((Object)null == (Object)(object)_victim || base.IsInterrupted)
			{
				yield break;
			}
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			Vector3 disp = KMathUtil.Make2D(_victim.transform.position - ((Component)this).transform.position);
			float distance = ((Vector3)(ref disp)).magnitude;
			if (distance <= _engageDistance)
			{
				break;
			}
			float destYaw = KMathUtil.CalcYawWithTarget(_victim.transform.position, ((Component)this).transform.position);
			TargetAnimal.TurnToYaw(destYaw, bSnap: false);
			Vector3 velocity = ((Vector3)(ref disp)).normalized * _walkSpeed;
			AnimalBehavior targetAnimal = TargetAnimal;
			targetAnimal.CurrentPosition += velocity * dt;
			TargetAnimal.CurrentPosition = new Vector3(Mathf.Clamp(TargetAnimal.CurrentPosition.x, _minArea.x, _maxArea.x), 0f, Mathf.Clamp(TargetAnimal.CurrentPosition.z, _minArea.z, _maxArea.z));
			yield return null;
		}
		if (Random.value < _offenseRatio && Time.time > _attackCoolTimeEnd)
		{
			base.CurState = State.Leap;
		}
		else
		{
			base.CurState = State.Roaming;
		}
	}

	private void CheckAndMakeDamageToPlayer(bool isBlow)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Damage damage = default(Damage);
		if (PrologueManager.PlayerBattleAi.IsDodgeable())
		{
			damage.Value = 0;
			damage.Result = DamageResult.Dodged;
		}
		else
		{
			Vector3 val = KMathUtil.Make2D(PlayerBehavior.LocalPlayer.CurrentPosition - ((Component)this).transform.position);
			if (((Vector3)(ref val)).magnitude < 300f && Random.value < _hitRatio)
			{
				damage.Result = DamageResult.Hit;
			}
			else
			{
				damage.Result = DamageResult.Missed;
			}
		}
		if (damage.Result == DamageResult.Hit)
		{
			if (Random.value < _criticalRatio)
			{
				damage.Effects |= DamageEffects.Critical;
			}
			damage.Value = CalcDamage((damage.Effects & DamageEffects.Critical) != 0);
		}
		damage.AttackType = AttackType.SmallTear;
		damage.Direction = DamageDirection.Front;
		damage.Effects = ((!isBlow) ? DamageEffects.KnockBack : DamageEffects.Blow);
		damage.Part = BodyPart.Body;
		PlayerBehavior.LocalPlayer.ReceiveDamage(((Component)this).gameObject, damage);
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
		yield return (object)new WaitForSeconds(_flinchEndTime - Time.time);
		base.CurState = State.Roaming;
	}

	public void EventGroggy()
	{
		base.CurState = State.Groggy;
		_curGroggy = 100;
	}

	private void GroggyEntered()
	{
		UnPinUpRootBone();
		TargetAnimal.Play(_groggyMotion);
	}

	private void GroggyExited()
	{
		PinUpRootBone();
	}

	private IEnumerator GroggyDoing()
	{
		BeginStrongAttackTween();
		ShowStrongAttackHelper(isShow: true);
		yield return (object)new WaitForSeconds(1.5f);
		float endTime = Time.time + _groggyTime - 1.5f;
		while (true)
		{
			if ((Object)null == (Object)(object)_victim)
			{
				yield break;
			}
			if (base.IsInterrupted)
			{
				EndStrongAttackTween();
				yield break;
			}
			if (Time.time > endTime)
			{
				break;
			}
			yield return null;
		}
		EndStrongAttackTween();
		ShowStrongAttackHelper(isShow: false);
		base.CurState = State.Normal;
	}

	private static void ShowStrongAttackHelper(bool isShow)
	{
		if (isShow)
		{
			GameSystem<PrologueGuideSystem>.Instance().SetGuideMask(new PrologueGuideSystem.GuideMask
			{
				Id = "ActionButtonGroup/Actions/ActiveAction_1",
				Type = "Click"
			}, helperOnly: true);
		}
		else
		{
			GameSystem<PrologueGuideSystem>.Instance().HideGuideMask();
		}
	}

	public void EventBlow()
	{
		base.CurState = State.Blow;
	}

	private void BlowEntered()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		TargetAnimal.Play(_blowMotion, loop: false);
		Vector3 val = KMathUtil.ProjectDirection(((Component)PlayerBehavior.LocalPlayer).transform);
		val.y = 0f;
		val.z = 0f;
		val = ((Vector3)(ref val)).normalized;
		TargetAnimal.TurnToYaw(KMathUtil.CalcYaw(val) + 180f, bSnap: true);
		Vector3 destPos = TargetAnimal.CurrentPosition + val * _blowDistance;
		destPos = ClampPos(destPos);
		TweenParms val2 = new TweenParms();
		val2.Prop("position", (object)destPos);
		val2.Ease((EaseType)6);
		HOTween.To((object)((Component)TargetAnimal).transform, _blowTime, val2);
		PinUpRootBone();
	}

	private Vector3 ClampPos(Vector3 destPos)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		destPos.x = Mathf.Clamp(destPos.x, _minArea.x, _maxArea.x);
		destPos.z = Mathf.Clamp(destPos.z, _minArea.z, _maxArea.z);
		return destPos;
	}

	private void BlowExited()
	{
	}

	private IEnumerator BlowDoing()
	{
		yield return (object)new WaitForSeconds(TargetAnimal.CurAnimState.length);
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
			if ((Object)null == (Object)(object)_victim || base.IsInterrupted)
			{
				yield break;
			}
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			Vector3 disp = KMathUtil.Make2D(newPos - ((Component)this).transform.position);
			float distance = ((Vector3)(ref disp)).magnitude;
			if (distance <= 50f)
			{
				break;
			}
			float destYaw = KMathUtil.CalcYawWithTarget(newPos, ((Component)this).transform.position);
			TargetAnimal.TurnToYaw(destYaw, bSnap: false);
			Vector3 velocity = ((Vector3)(ref disp)).normalized * moveSpeed;
			AnimalBehavior targetAnimal = TargetAnimal;
			targetAnimal.CurrentPosition += velocity * dt;
			yield return null;
		}
		base.CurState = State.Normal;
	}

	private void LeapEntered()
	{
		_attackCoolTimeEnd = Time.time + _attackCoolTime;
	}

	private void LeapExited()
	{
	}

	private IEnumerator LeapDoing()
	{
		Vector3 newPos = Vector3.MoveTowards(((Component)TargetAnimal).transform.position, PlayerBehavior.LocalPlayer.CurrentPosition, _maxLeapDist);
		float destYaw = KMathUtil.CalcYawWithTarget(newPos, ((Component)this).transform.position);
		TargetAnimal.TurnToYaw(destYaw, bSnap: false);
		UnPinUpRootBone();
		TargetAnimal.Play(_threatMotion, loop: false);
		ParticleManager.Emit("Particle/FX_SkillActivated_Common_01.prefab", TargetAnimal.CurrentPosition, Quaternion.identity);
		BeginDodgeTween();
		yield return (object)new WaitForSeconds(0.3f);
		if (!_threatedFlagForGuide)
		{
			GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.LearnDodge);
			_threatedFlagForGuide = true;
		}
		yield return (object)new WaitForSeconds(TargetAnimal.CurAnimState.length - 0.3f);
		EndDodgeTween();
		if (base.IsInterrupted)
		{
			yield break;
		}
		PinUpRootBone();
		TargetAnimal.Play(_leapMotion, loop: false);
		TweenParms parms = new TweenParms();
		parms.Prop("position", (object)newPos);
		parms.Ease((EaseType)6);
		Tweener tweener = HOTween.To((object)((Component)TargetAnimal).transform, TargetAnimal.CurAnimState.length, parms);
		float attackAt = Time.time + 0.6f;
		bool attacked = false;
		while (!((ABSTweenComponent)tweener).isComplete)
		{
			if (base.IsInterrupted)
			{
				HOTween.Kill(tweener);
				yield break;
			}
			if (!attacked && Time.time >= attackAt)
			{
				CheckAndMakeDamageToPlayer(isBlow: true);
				attacked = true;
			}
			yield return null;
		}
		base.CurState = State.Normal;
	}

	private static TweenScale CreateTweenScale(string buttonPath)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Transform val = KSingleton<UIManager>.Instance().FindTransform(buttonPath);
		if (Object.op_Implicit((Object)(object)val))
		{
			TweenScale tweenScale = ((Component)val).gameObject.AddComponent<TweenScale>();
			tweenScale.to = new Vector3(1.2f, 1.2f, 1.2f);
			tweenScale.duration = 0.2f;
			tweenScale.style = UITweener.Style.PingPong;
			tweenScale.ignoreTimeScale = true;
			return tweenScale;
		}
		return null;
	}

	private void BeginDodgeTween()
	{
		if ((Object)(object)DodgeTweenScale != (Object)null)
		{
			DodgeTweenScale.PlayForward();
		}
	}

	private void EndDodgeTween()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)DodgeTweenScale != (Object)null)
		{
			DodgeTweenScale.ResetToBeginning();
			((Behaviour)DodgeTweenScale).enabled = false;
			((Component)DodgeTweenScale).transform.localScale = Vector3.one;
		}
	}

	private void BeginStrongAttackTween()
	{
		if ((Object)(object)StrongAttackTweenScale != (Object)null)
		{
			StrongAttackTweenScale.PlayForward();
		}
	}

	private void EndStrongAttackTween()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)StrongAttackTweenScale != (Object)null)
		{
			StrongAttackTweenScale.ResetToBeginning();
			((Behaviour)StrongAttackTweenScale).enabled = false;
			((Component)StrongAttackTweenScale).transform.localScale = Vector3.one;
		}
	}

	public void EventDead()
	{
		base.CurState = State.Dead;
		_isDeadPhase = true;
	}

	private void DeadEntered()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		PinUpRootBone();
		TargetAnimal.Play(_blowMotion, loop: false);
		TargetAnimal.TurnToYaw(_deadYaw, bSnap: false);
		TweenParms val = new TweenParms();
		val.Prop("position", (object)_deadDestPos);
		val.Ease((EaseType)6);
		HOTween.To((object)((Component)TargetAnimal).transform, _playTrexCutSceneDelay, val);
		GameSystem<PrologueGuideSystem>.Instance().HideGuideMask();
		KSingleton<PrologueManager>.Instance().DelayedCall(BeginFinalCutScene, _playTrexCutSceneDelay);
	}

	private void DeadExited()
	{
		UnPinUpRootBone();
	}

	private IEnumerator DeadDoing()
	{
		yield return (object)new WaitForSeconds(_playTrexCutSceneDelay);
	}

	private void BeginFinalCutScene()
	{
		KSingleton<TrainTrexController>.Instance().PlayTrexCutScene();
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private void PinUpRootBone()
	{
		TargetAnimal.SetServerSideRootMotionEnable(serverSideRootMotionEnabled: true);
	}

	private void UnPinUpRootBone()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		TargetAnimal.SetServerSideRootMotionEnable(serverSideRootMotionEnabled: false);
		TargetAnimal.MeshObjectTransform.localPosition = Vector3.zero;
	}

	public void OnTakeDamage(Damage damage, bool isDead)
	{
		if (damage.Value > 0 && !_isDeadPhase)
		{
			if (isDead)
			{
				EventDead();
				PlayerBehavior.LocalPlayer.OnKilledAnimal(((Component)this).gameObject.GetComponent<AnimalBehavior>());
				PlayerBehavior.LocalPlayer.Target = null;
			}
			else if ((damage.Effects & DamageEffects.Blow) > DamageEffects.None)
			{
				EventBlow();
				_curGroggy -= 70;
			}
			else if (_curGroggy < 0)
			{
				EventGroggy();
			}
			else if ((damage.Effects & DamageEffects.KnockBack) > DamageEffects.None)
			{
				EventFlinch();
				_curGroggy -= 60;
			}
		}
	}
}
