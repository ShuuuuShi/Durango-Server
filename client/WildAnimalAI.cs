using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Durango.Model;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using Messages;
using Shared.Battle;
using Shared.System;
using UnityEngine;

public class WildAnimalAI : StateBasedAI<WildAnimalAI.State>
{
	public enum State
	{
		Invalid = -1,
		Idle = 0,
		Roaming = 1,
		Chase = 4,
		Blow = 6,
		Dead = 8,
		Normal = 9,
		Attack = 10,
		Collapse = 11,
		Groggy = 12,
		WaitForBattleBegin = 3,
		Evade = 7,
		Damaged = 5,
		BattleBegin = 13,
		Count = 2
	}

	public enum Type
	{
		Invalid = -1,
		TRex,
		Allo,
		Tarbo,
		Raptor
	}

	[SerializeField]
	private string _standMotion;

	[SerializeField]
	private string _walkMotion;

	[SerializeField]
	private float _walkSpeed;

	[SerializeField]
	private Vector3 _minArea;

	[SerializeField]
	private Vector3 _maxArea;

	[SerializeField]
	private string _runMotion;

	[SerializeField]
	private float _runSpeed;

	[SerializeField]
	private float _moveSpeed;

	public bool _isDoingOnePhase;

	public bool _isDoingTwoPhase;

	public bool _isDoingThreePhase;

	private bool _isHealing;

	private bool _isDeadPhase;

	private string _sleepMotion;

	private string _sleepLoopMotion;

	private string _sleepEndMotion;

	private string _runStopMotion;

	private string _limpMotion;

	private string _eatMotion;

	private string _evadeMotion;

	private string _battleIdleMotion;

	private string _battleStandMotion;

	private string _turnMotion;

	[SerializeField]
	private float _engageDistance = 350f;

	[SerializeField]
	private string _blowMotion;

	[SerializeField]
	private Vector3 _deadDestPos;

	private float _attackCooldownEnd;

	private GameObject _victim;

	[SerializeField]
	private List<string> _damagedMotions;

	[SerializeField]
	private float _damagedEndTime;

	[SerializeField]
	private float _blowDistance;

	[SerializeField]
	private float _blowTime;

	[SerializeField]
	private string _groggyMotion;

	[SerializeField]
	private string _threatMotion;

	[SerializeField]
	private float _maxLeapDist;

	[SerializeField]
	private float _attackCoolTime;

	[SerializeField]
	private Vector3 _deadDestPosOriginOffset;

	[SerializeField]
	private float _maxAttackDist;

	[SerializeField]
	private string _blowEndMotion;

	[SerializeField]
	private string _deadMotion;

	[SerializeField]
	private float _deadTime;

	[SerializeField]
	private string _collapseMotion;

	public float _defenseValue;

	[SerializeField]
	private float _deadDistance;

	[SerializeField]
	private float _collapseEndTime;

	[SerializeField]
	private float _groggyEndTime;

	[SerializeField]
	private float _blowEndTime;

	[SerializeField]
	private List<string> _attackMotions;

	private float _walkDistance;

	[SerializeField]
	private bool _isBattleBegin;

	[SerializeField]
	private float _healCooltime;

	private bool _isCanHeal;

	[SerializeField]
	private System.Timers.Timer _healTimer;

	public bool _isBlowing;

	public AnimalBehavior TargetAnimal { get; private set; }

	protected override State InvalidState => State.Invalid;

	protected override int StateEnumCount => 12;

	public AppearAnimal Animal { get; set; }

	private float PlaybackRate => 1f;

	public static Type CurType { get; set; }

	protected override void OnAwake()
	{
		TargetAnimal = GetComponent<AnimalBehavior>();
		TargetAnimal.SetActivateRootMotion(active: false);
	}

	protected override IEnumerator OnStart()
	{
		TargetAnimal.EntityId = Animal.EntityId;
		GetComponent<BoneLookAtTarget>().AutoChangeTarget = false;
		PinUpRootBone();
		Initialization();
		yield break;
	}

	private void Update()
	{
		if (TargetAnimal.Life.Goal() < TargetAnimal.Life.Max())
		{
			_isCanHeal = true;
		}
		else
		{
			_isCanHeal = false;
		}
		Vector3 currentPosition = TargetAnimal.CurrentPosition;
		currentPosition.y = TargetAnimal.ProcessWaterDepth(currentPosition);
		TargetAnimal.CurrentPosition = currentPosition;
	}

	protected override void DefineStates()
	{
		AddState(State.Idle, new StateElem
		{
			Doing = OnIdle
		});
		AddState(State.Roaming, new StateElem
		{
			Doing = OnRoaming
		});
		AddState(State.BattleBegin, new StateElem
		{
			Entered = BattleBeginEntered,
			Doing = BattleBeginDoing,
			Exited = BattleBeginExited
		});
		AddState(State.Normal, new StateElem
		{
			Entered = NormalEntered,
			Doing = NormalDoing,
			Exited = NormalExited
		});
		AddState(State.WaitForBattleBegin, new StateElem
		{
			Doing = WaitForBattleBeginDoing
		});
		AddState(State.Attack, new StateElem
		{
			Entered = AttackEntered,
			Doing = AttackDoing,
			Exited = AttackExited
		});
		AddState(State.Chase, new StateElem
		{
			Entered = ChaseEntered,
			Doing = ChaseDoing,
			Exited = ChaseExited
		});
		AddState(State.Damaged, new StateElem
		{
			Entered = DamagedEntered,
			Doing = DamagedDoing,
			Exited = DamagedExited
		});
		AddState(State.Collapse, new StateElem
		{
			Entered = CollapseEntered,
			Doing = CollapseDoing,
			Exited = CollapseExited
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
		AddState(State.Dead, new StateElem
		{
			Entered = DeadEntered,
			Doing = DeadDoing,
			Exited = DeadExited
		});
	}

	private IEnumerator OnIdle()
	{
		while (true)
		{
			float value = UnityEngine.Random.value;
			if (value > 0.2f)
			{
				break;
			}
			float seconds = 1f;
			if (value < 0.2f)
			{
				if (TargetAnimal.AnimalFrameworkResource.GetAnimationElements("idle") is AnimationElem animationElem)
				{
					TargetAnimal.Play(animationElem.motion, loop: true, 0f, PlaybackRate);
					seconds = animationElem.Clip.length / PlaybackRate;
				}
			}
			else if (TargetAnimal.AnimalFrameworkResource.GetAnimationElements("stand") is AnimationElem animationElem2)
			{
				TargetAnimal.Play(animationElem2.motion, loop: true, 0f, PlaybackRate);
				float num = animationElem2.Clip.length / PlaybackRate;
				seconds = UnityEngine.Random.Range(num, num * 3f);
			}
			yield return new WaitForSeconds(seconds);
		}
		base.CurState = State.Roaming;
	}

	private Vector3 ProcessCollisionWithSliding(Vector3 beginPos, Vector3 delta)
	{
		if (delta == Vector3.zero)
		{
			return beginPos;
		}
		delta = Collisions.ProcessSimpleSliding(Collisions.CreateCollisionParam(beginPos, delta));
		return beginPos + delta;
	}

	protected override bool IsAIEnded()
	{
		return false;
	}

	protected override bool IsTerminalState(State state)
	{
		return false;
	}

	private Vector3 ClampPos(Vector3 destPos)
	{
		destPos.x = Mathf.Clamp(destPos.x, _minArea.x, _maxArea.x);
		destPos.z = Mathf.Clamp(destPos.z, _minArea.z, _maxArea.z);
		return destPos;
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

	public void SetAiActivated()
	{
		base.CurState = State.WaitForBattleBegin;
	}

	private IEnumerator OnRoaming()
	{
		while (UnityEngine.Random.value <= 0.85f)
		{
			MoveSet moveSet = ((TargetAnimal.AnimalFrameworkResource.GetAnimationElements("move_motion_sets") is AnimationElemMoveSet animationElemMoveSet) ? animationElemMoveSet.elems.FirstOrDefault() : null);
			if (moveSet != null)
			{
				MoveMotionInfo moveMotion = moveSet.GetMoveMotion(0f);
				TargetAnimal.SetRotateSpeed(moveMotion.rot_speed);
				TargetAnimal.Play(moveMotion.motion, loop: true, 0f, PlaybackRate);
				float moveSpeed = moveMotion.base_move_speed;
				float yaw = UnityEngine.Random.value * 360f;
				float timer = UnityEngine.Random.Range(2f, 7f);
				TargetAnimal.TurnToYaw(yaw, bSnap: false);
				while (timer > 0f)
				{
					float currentYaw = TargetAnimal.CurrentYaw;
					Vector3 delta = new Vector3(Mathf.Sin(currentYaw * ((float)Math.PI / 180f)), 0f, Mathf.Cos(currentYaw * ((float)Math.PI / 180f))) * moveSpeed * Time.deltaTime;
					Vector3 currentPosition = TargetAnimal.CurrentPosition;
					Vector3 vector = ProcessCollisionWithSliding(TargetAnimal.CurrentPosition, delta);
					Vector2 floatTile = Util.ClientPositionToTilePosition(vector);
					if (TerrainWater.IsTooDeepToSwim(Singleton<TerrainBase>.Instance().GetTileDepth(floatTile), 0f))
					{
						base.CurState = State.Idle;
						yield break;
					}
					if ((vector - currentPosition).magnitude / Time.deltaTime / moveSpeed < 0.7f)
					{
						base.CurState = State.Idle;
						yield break;
					}
					TargetAnimal.CurrentPosition = vector;
					timer -= Time.deltaTime;
					yield return null;
				}
			}
			yield return null;
		}
		base.CurState = State.Idle;
	}

	public void RemoveActivatedAi()
	{
		if (base.CurState != State.Dead)
		{
			base.CurState = State.WaitForBattleBegin;
		}
	}

	private void Initialization()
	{
		_minArea = Vector3.zero;
		_maxArea = Vector3.zero;
		_blowDistance = 500f;
		_maxLeapDist = 500f;
		_deadDestPosOriginOffset = new Vector3(0f, 0f, 3000f);
		_maxAttackDist = 500f;
		_deadDistance = 500f;
		_walkDistance = 750f;
		_attackMotions = new List<string>();
		_damagedMotions = new List<string>();
		switch (CurType)
		{
		case Type.TRex:
			_defenseValue = 0.6f;
			_standMotion = "TRex_Stand";
			_turnMotion = "";
			_walkMotion = "TRex_Walk";
			_walkSpeed = 420f;
			_runMotion = "TRex_Run";
			_runStopMotion = "TRex_Run_Stop";
			_runSpeed = 580f;
			_eatMotion = "TRex_Eat";
			_sleepMotion = "TRex_Sleep_Begin";
			_sleepLoopMotion = "TRex_Sleep_Looping";
			_sleepEndMotion = "TRex_Sleep_End";
			_battleIdleMotion = "Trex_Battle_Idle";
			_battleStandMotion = "";
			_threatMotion = "TRex_Attack_Roar";
			_blowMotion = "Trex_Damage_Blow_Begin";
			_blowEndMotion = "Trex_Damage_Blow_End";
			_limpMotion = "TRex_Limp";
			_groggyMotion = "TRex_Groggy";
			_attackMotions.AddRange(new string[12]
			{
				"TRex_Attack03_Wounded_Tail", "TRex_Attack02_Bite", "TRex_Attack01_Tail", "TRex_Attack_Turn_Tail", "TRex_Attack_Tail_Back", "TRex_Attack_Stamp_Right", "TRex_Attack_Stamp_Left", "TRex_Attack_Stamp_Foot", "TRex_Attack_Stamp", "TRex_Attack_Roar",
				"TRex_Attack_Head", "TRex_Attack_Bite_Normal"
			});
			_evadeMotion = "";
			_damagedMotions.AddRange(new string[4] { "Trex_Damage_W", "Trex_Damage_N", "Trex_Damage_E", "Trex_Damage_S" });
			_deadMotion = "TRex_Die";
			_healCooltime = 0.075f;
			break;
		case Type.Allo:
			_defenseValue = 0.8f;
			_standMotion = "Allo_Stand";
			_turnMotion = "Allo_Turn";
			_walkMotion = "Allo_Walk";
			_walkSpeed = 480f;
			_runMotion = "Allo_Run";
			_runStopMotion = "";
			_runSpeed = 650f;
			_eatMotion = "Allo_Eat";
			_sleepMotion = "Allo_Sleep_Begin";
			_sleepLoopMotion = "Allo_Sleep_Looping";
			_sleepEndMotion = "Allo_Sleep_End";
			_battleIdleMotion = "Allo_Battle_Idle";
			_battleStandMotion = "Allo_BattleStand";
			_threatMotion = "TRex_Attack_Roar";
			_blowMotion = "Allo_Damage_Blow_Begin";
			_blowEndMotion = "Allo_Damage_Blow_End";
			_limpMotion = "Allo_Limp";
			_groggyMotion = "Allo_Groggy";
			_attackMotions.AddRange(new string[8] { "Allo_Attack_Bite", "Allo_Attack_Bite_Normal", "Allo_Attack_Dash", "Allo_Attack_Stamp", "Allo_Attack_Tail_Back", "Allo_Attack_Tail_L", "Allo_Attack_Tail_R", "Allo_Attack_Tail_Turn" });
			_evadeMotion = "Allo_Evade";
			_damagedMotions.AddRange(new string[4] { "Allo_Damage_W", "Allo_Damage_N", "Allo_Damage_E", "Allo_Damage_S" });
			_deadMotion = "Allo_Die";
			_healCooltime = 0.1f;
			break;
		case Type.Tarbo:
			_defenseValue = 0.8f;
			_standMotion = "Tarbo_Stand";
			_walkMotion = "Allo_Walk";
			_walkSpeed = 480f;
			_runMotion = "Allo_Run";
			_runSpeed = 650f;
			_healCooltime = 0.1f;
			break;
		case Type.Raptor:
			_standMotion = "Allo_Stand";
			_walkMotion = "Allo_Walk";
			_walkSpeed = 480f;
			_runMotion = "Allo_Run";
			_runSpeed = 650f;
			break;
		default:
			UIManager.SystemMsg("Error");
			break;
		}
		_healTimer = new System.Timers.Timer((double)_healCooltime * 1000.0);
		_healTimer.Enabled = true;
		_healTimer.AutoReset = true;
		_healTimer.Elapsed += OnHealTimedEvent;
	}

	private IEnumerator WaitForBattleBeginDoing()
	{
		TargetAnimal.CrossFade(_standMotion, 0.1f);
		GetComponent<BoneLookAtTarget>().SetLookTarget(_victim);
		yield break;
	}

	private void ChaseEntered()
	{
	}

	private void ChaseExited()
	{
	}

	private IEnumerator ChaseDoing()
	{
		if (Maths.Make2D(_victim.transform.position - base.transform.position).magnitude <= _walkDistance)
		{
			TargetAnimal.CrossFade(_walkMotion, 0.1f);
		}
		else
		{
			TargetAnimal.CrossFade(_runMotion, 0.1f);
		}
		while (!(null == _victim) && !base.IsInterrupted)
		{
			if (Maths.Make2D(_victim.transform.position - base.transform.position).magnitude <= _engageDistance)
			{
				if (Time.time > _attackCooldownEnd)
				{
					base.CurState = State.Attack;
				}
				else
				{
					base.CurState = State.Normal;
				}
				break;
			}
			float yaw = Maths.CalcYawWithTarget(_victim.transform.position, base.transform.position);
			MoveMotionInfo moveMotion = ((TargetAnimal.AnimalFrameworkResource.GetAnimationElements("move_motion_sets") is AnimationElemMoveSet animationElemMoveSet) ? animationElemMoveSet.elems.FirstOrDefault() : null).GetMoveMotion(0f);
			TargetAnimal.SetRotateSpeed(moveMotion.rot_speed);
			TargetAnimal.TurnToYaw(yaw, bSnap: false);
			float currentYaw = TargetAnimal.CurrentYaw;
			Vector3 delta = ((!(Maths.Make2D(_victim.transform.position - base.transform.position).magnitude <= _walkDistance)) ? (new Vector3(Mathf.Sin(currentYaw * ((float)Math.PI / 180f)), 0f, Mathf.Cos(currentYaw * ((float)Math.PI / 180f))) * _runSpeed * Time.deltaTime) : (new Vector3(Mathf.Sin(currentYaw * ((float)Math.PI / 180f)), 0f, Mathf.Cos(currentYaw * ((float)Math.PI / 180f))) * _walkSpeed * Time.deltaTime));
			Vector3 currentPosition = TargetAnimal.CurrentPosition;
			Vector3 vector = ProcessCollisionWithSliding(TargetAnimal.CurrentPosition, delta);
			Vector2 floatTile = Util.ClientPositionToTilePosition(vector);
			if (TerrainWater.IsTooDeepToSwim(Singleton<TerrainBase>.Instance().GetTileDepth(floatTile), 0f))
			{
				base.CurState = State.Normal;
				break;
			}
			if (Maths.Make2D(_victim.transform.position - base.transform.position).magnitude <= _walkDistance)
			{
				if ((vector - currentPosition).magnitude / Time.deltaTime / _walkSpeed < 0.7f)
				{
					base.CurState = State.Normal;
					break;
				}
			}
			else if ((vector - currentPosition).magnitude / Time.deltaTime / _runSpeed < 0.7f)
			{
				base.CurState = State.Normal;
				break;
			}
			TargetAnimal.CurrentPosition = vector;
			yield return null;
		}
	}

	public void EventDamaged()
	{
		base.CurState = State.Damaged;
	}

	private void DamagedEntered()
	{
		int index = UnityEngine.Random.Range(0, _damagedMotions.Count);
		TargetAnimal.Play(_damagedMotions[index], loop: false);
		_damagedEndTime = Time.time + TargetAnimal.CurAnimState.length;
	}

	private void DamagedExited()
	{
	}

	private IEnumerator DamagedDoing()
	{
		yield return new WaitForSeconds(_damagedEndTime - Time.time);
		base.CurState = State.Normal;
	}

	public void EventBlow()
	{
		float value = UnityEngine.Random.value;
		if (value <= 0.5f && base.CurState != State.Groggy && base.CurState != State.Collapse)
		{
			base.CurState = State.Damaged;
		}
		if (value > 0.5f && value <= 0.9f)
		{
			base.CurState = State.Blow;
		}
		if (value > 0.9f && value <= 0.95f)
		{
			base.CurState = State.Groggy;
		}
		if (value > 0.95f && value <= 1f)
		{
			base.CurState = State.Collapse;
		}
	}

	private void BlowEntered()
	{
		TargetAnimal.Play(_blowMotion, loop: false);
		_blowEndTime = Time.time + TargetAnimal.CurAnimState.length;
		_isBlowing = true;
	}

	private void BlowExited()
	{
	}

	private IEnumerator BlowDoing()
	{
		yield return new WaitForSeconds(_blowEndTime - Time.time);
		TargetAnimal.Play(_blowEndMotion, loop: false);
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length);
		_isBlowing = false;
		base.CurState = State.Normal;
	}

	public void EventDead()
	{
		_isDeadPhase = true;
		_isCanHeal = false;
		_healTimer.Stop();
		_isHealing = false;
		TargetAnimal.SetAsDead();
		TargetAnimal.SetAlive(alive: false);
		Gauge life = new Gauge(TargetAnimal.Life.Max(), 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = 0f
			}
		});
		TargetAnimal.SetSurvivalGauge(life, null);
		HuntRewardEffect huntRewardEffect = default(HuntRewardEffect);
		huntRewardEffect.TargetAnimal = TargetAnimal.GetName();
		huntRewardEffect.TargetEntityType = TargetAnimal.EntityTypeId;
		huntRewardEffect.Type = Shared.System.RewardEffect.Hunted;
		HuntRewardEffect huntRewardEffect2 = huntRewardEffect;
		UIManager.FindScript<AlarmGroup>().ShowRewardAlarm(huntRewardEffect2);
		CombatSystem.ExitBattle();
		base.CurState = State.Dead;
	}

	private void DeadEntered()
	{
		TargetAnimal.Play(_deadMotion, loop: false);
		Vector3 vector = Maths.ProjectDirection(PlayerBehavior.LocalPlayer.transform);
		vector.y = 0f;
		vector.z = 0f;
		vector = vector.normalized;
		Vector3 currentPosition = TargetAnimal.CurrentPosition;
		_deadTime = TargetAnimal.CurAnimState.length;
		TweenPosition tweenPosition = TweenPosition.Begin(TargetAnimal.gameObject, _deadTime, currentPosition);
		tweenPosition.method = UITweener.Method.EaseInOut;
		tweenPosition.PlayForward();
		PinUpRootBone();
	}

	private void DeadExited()
	{
		if (base.CurState != State.Dead)
		{
			base.CurState = State.Dead;
		}
		if (TargetAnimal.IsAlive)
		{
			base.CurState = State.Dead;
		}
	}

	private IEnumerator DeadDoing()
	{
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length);
		if (TargetAnimal.IsAlive)
		{
			TargetAnimal.SetAsDead();
			TargetAnimal.SetAlive(alive: false);
		}
	}

	public void OnTakeDamage(Damage damage, bool isDead)
	{
		if (damage.Value <= 0)
		{
			return;
		}
		if (isDead)
		{
			EventDead();
			PlayerBehavior.LocalPlayer.OnKilledAnimal(TargetAnimal);
			return;
		}
		Gauge life = new Gauge(TargetAnimal.Life.Max(), 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = TargetAnimal.Life.Goal() - (float)damage.Value
			}
		});
		TargetAnimal.SetSurvivalGauge(life, null);
		if (CurType == Type.TRex)
		{
			if (TargetAnimal.Life.Goal() + (float)damage.Value >= TargetAnimal.Life.Max() / 2f && TargetAnimal.Life.Goal() <= TargetAnimal.Life.Max() / 2f)
			{
				UIManager.SystemMsg("BattleAction", TargetAnimal.GetName() + "가 화가 났습니다.");
				if (_isDoingOnePhase)
				{
					_isDoingOnePhase = false;
				}
				else if (!_isDoingTwoPhase)
				{
					_isDoingTwoPhase = true;
				}
				else if (_isDoingThreePhase)
				{
					_isDoingThreePhase = false;
				}
			}
			if (TargetAnimal.Life.Goal() + (float)damage.Value >= TargetAnimal.Life.Max() / 2f / 2.5f && TargetAnimal.Life.Goal() <= TargetAnimal.Life.Max() / 2f / 2.5f)
			{
				UIManager.SystemMsg("BattleAction", TargetAnimal.GetName() + "가 최후의 발악을 합니다.");
				if (_isDoingOnePhase)
				{
					_isDoingOnePhase = false;
				}
				else if (_isDoingTwoPhase)
				{
					_isDoingTwoPhase = false;
				}
				else if (!_isDoingThreePhase)
				{
					_isDoingThreePhase = true;
				}
			}
		}
		if (damage.Effects == DamageEffects.Blow)
		{
			EventBlow();
		}
		if (damage.Effects == DamageEffects.Critical || damage.Effects == DamageEffects.CrossCounter)
		{
			EventCritical();
		}
		if (damage.Effects == DamageEffects.KnockBack)
		{
			EventCollapse();
		}
		if (damage.Effects == DamageEffects.None)
		{
			EventMiss();
		}
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
			if ((_victim.transform.position - base.transform.position).magnitude > _engageDistance)
			{
				base.CurState = State.Chase;
			}
			else if (Time.time > _attackCooldownEnd)
			{
				base.CurState = State.Attack;
			}
			else
			{
				base.CurState = State.Normal;
			}
		}
		yield return new WaitForSeconds(0.3f);
	}

	protected override IEnumerator OnBeforeDoingState()
	{
		_victim = PlayerBehavior.LocalPlayer.gameObject;
		if (_victim == null)
		{
			yield return new WaitForSeconds(1f);
		}
	}

	protected override IEnumerator OnAfterDoingState()
	{
		yield break;
	}

	private void AttackEntered()
	{
		_attackCooldownEnd = Time.time + _attackCoolTime;
	}

	private void AttackExited()
	{
	}

	private IEnumerator AttackDoing()
	{
		Vector3 newPos = Vector3.MoveTowards(TargetAnimal.transform.position, PlayerBehavior.LocalPlayer.CurrentPosition, _maxAttackDist);
		float yaw = Maths.CalcYawWithTarget(newPos, base.transform.position);
		TargetAnimal.TurnToYaw(yaw, bSnap: false);
		UnPinUpRootBone();
		ParticleManager.Emit("Particle/FX_SkillActivated_Common_01.prefab", TargetAnimal.CurrentPosition, Quaternion.identity);
		if (base.IsInterrupted)
		{
			yield break;
		}
		yield return new WaitForSeconds(0.1f);
		int index = UnityEngine.Random.Range(0, _attackMotions.Count);
		TargetAnimal.Play(_attackMotions[index], loop: false);
		TweenPosition tween = TweenPosition.Begin(TargetAnimal.gameObject, TargetAnimal.CurAnimState.length, newPos);
		tween.method = UITweener.Method.EaseInOut;
		tween.PlayForward();
		PinUpRootBone();
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
				Singleton<AnimalManager>.Instance().CheckAndMakeDamageToPlayer(Animal);
				attacked = true;
			}
			yield return null;
		}
		base.CurState = State.Normal;
	}

	public void EventCollapse()
	{
		base.CurState = State.Collapse;
	}

	private void CollapseEntered()
	{
		UIManager.SystemMsg("BattleAction", TargetAnimal.GetName() + "가 쓰러졌습니다!");
		TargetAnimal.Play(_collapseMotion, loop: false);
		_collapseEndTime = Time.time + TargetAnimal.CurAnimState.length;
	}

	private void CollapseExited()
	{
	}

	private IEnumerator CollapseDoing()
	{
		yield return new WaitForSeconds(_collapseEndTime - Time.time);
		UIManager.SystemMsg("BattleAction", TargetAnimal.GetName() + "가 기운을 되찾았습니다.");
		TargetAnimal.Play(_blowEndMotion, loop: false);
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length);
		base.CurState = State.Normal;
	}

	private void GroggyEntered()
	{
		UIManager.SystemMsg("BattleAction", TargetAnimal.GetName() + "가 정신을 차리지 못하고 있습니다.");
		TargetAnimal.Play(_groggyMotion, loop: false);
		_groggyEndTime = Time.time + TargetAnimal.CurAnimState.length;
	}

	private void GroggyExited()
	{
	}

	private IEnumerator GroggyDoing()
	{
		yield return new WaitForSeconds(_groggyEndTime - Time.time);
		UIManager.SystemMsg("BattleAction", TargetAnimal.GetName() + "가 기운을 되찾았습니다.");
		base.CurState = State.Normal;
	}

	public void EventCritical()
	{
		float value = UnityEngine.Random.value;
		if (value <= 0.75f)
		{
			base.CurState = State.Blow;
		}
		if (value > 0.75f && value <= 0.9f)
		{
			base.CurState = State.Collapse;
		}
		if (value > 0.9f && value <= 1f)
		{
			base.CurState = State.Groggy;
		}
	}

	public void EventMiss()
	{
	}

	public void SetCombatAiActivated()
	{
		base.CurState = State.BattleBegin;
	}

	private IEnumerator BattleBeginDoing()
	{
		yield return new WaitForSeconds(TargetAnimal.CurAnimState.length);
		base.CurState = State.Normal;
	}

	private void BattleBeginEntered()
	{
		float yaw = Maths.CalcYawWithTarget(_victim.transform.position, base.transform.position);
		MoveMotionInfo moveMotion = ((TargetAnimal.AnimalFrameworkResource.GetAnimationElements("move_motion_sets") is AnimationElemMoveSet animationElemMoveSet) ? animationElemMoveSet.elems.FirstOrDefault() : null).GetMoveMotion(0f);
		TargetAnimal.SetRotateSpeed(moveMotion.rot_speed);
		TargetAnimal.TurnToYaw(yaw, bSnap: false);
		TargetAnimal.CrossFade(_threatMotion);
	}

	private void BattleBeginExited()
	{
	}

	private void OnHealTimedEvent(object p0, ElapsedEventArgs p1)
	{
		if (_isCanHeal)
		{
			_isHealing = true;
			Gauge life = new Gauge(TargetAnimal.Life.Max(), 0f, new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = TargetAnimal.Life.Goal() + 1f
				}
			});
			TargetAnimal.SetSurvivalGauge(life, null);
		}
	}
}
