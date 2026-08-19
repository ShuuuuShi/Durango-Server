using System.Collections.Generic;
using CombatData;
using Messages;
using MsgPack;
using PlayGuide;
using PlayerExtensionsPrologue;
using Shared.Battle;
using UnityEngine;

public class PrologueAIPlayer : MonoBehaviour
{
	public class ActionInfo
	{
		public bool IsAutoAction;

		public string ActionId;

		public EfxType EfxType;
	}

	private const string DodgeActionSetName = "onehand_sword_dodge_set";

	private const float ServerResponseDelay = 1f;

	private const int StrongAttackCountGoal = 2;

	[SerializeField]
	private float _scaredPlayerMoveSpeed = 150f;

	[SerializeField]
	private float _battleMoveSpeed = 260f;

	[SerializeField]
	private float _avoidDistanceAtBegin = 300f;

	[SerializeField]
	private float _attackableRangeMin = 100f;

	[SerializeField]
	private float _attackableRangeMax = 200f;

	[SerializeField]
	private float _attackCoolTime = 4f;

	[SerializeField]
	private float _hitRatio = 0.7f;

	[SerializeField]
	private float _strongAttackHitRatio = 0.9f;

	[SerializeField]
	private float _criticalRatio = 0.1f;

	[SerializeField]
	private float _avoidCollisionDistance = 500f;

	[SerializeField]
	private int _damageMin = 1;

	[SerializeField]
	private int _damageMax = 20;

	[SerializeField]
	private int _damageCritical = 100;

	[SerializeField]
	private string _defaultAttackClipName = "Prologue_Twohand_Attack_A";

	[SerializeField]
	private string _strongAttackClipName = "Prologue_Twohand_AttackStrong";

	[SerializeField]
	private string _dodgeClipName = "Novice_Dodge";

	[SerializeField]
	private string _blowBeginClipName = "Barehand_Damage_Blow_Begin";

	[SerializeField]
	private string _blowEndClipName = "Barehand_Damage_Blow_End";

	private Dictionary<string, ActionInfo> _actionSets = new Dictionary<string, ActionInfo>();

	private List<string> _defaultActions = new List<string>();

	private PrologueConnectionHook _connectionHook;

	private Vector3 _moveToPos = Vector3.zero;

	private float _lastMoveReserveTime;

	private string _nextActionSetId;

	private string _nextActionMotion;

	private float _attackCoolTimeEnd = -1f;

	private AnimalBehavior _enemy;

	private float _lastUpdateTime;

	private float _nextScheduleTime;

	private float _dodgeReserveTimeBegin;

	private float _dodgeReserveTimeEnd;

	private int _dodgeCount;

	private bool _dodgeGuideCompleted;

	private bool _isAttackable;

	private int _normalAttackCount;

	private int _strongAttackCount;

	private bool _battleMode;

	[SerializeField]
	private float _cutScenePosThreshould = 300f;

	private float AttackableRangeMin
	{
		get
		{
			if (IsAttackablePhase())
			{
				return _attackableRangeMin;
			}
			return _avoidDistanceAtBegin + _attackableRangeMin;
		}
	}

	private float AttackableRangeMax
	{
		get
		{
			if (IsAttackablePhase())
			{
				return _attackableRangeMax;
			}
			return _avoidDistanceAtBegin + _attackableRangeMax;
		}
	}

	public float HitRatio => _hitRatio;

	public float StrongAttackHitRatio => _strongAttackHitRatio;

	public float CriticalRatio => _criticalRatio;

	public Dictionary<string, ActionInfo> ActionSets
	{
		get
		{
			if (_actionSets.Count == 0)
			{
				_actionSets.Add("onehand_sword_default_set", new ActionInfo
				{
					ActionId = _defaultAttackClipName,
					EfxType = EfxType.Attack,
					IsAutoAction = true
				});
				_actionSets.Add("onehand_sword_smash_set", new ActionInfo
				{
					ActionId = _strongAttackClipName,
					EfxType = EfxType.Attack,
					IsAutoAction = false
				});
				_actionSets.Add("onehand_sword_dodge_set", new ActionInfo
				{
					ActionId = "Dodge",
					EfxType = EfxType.Defense,
					IsAutoAction = false
				});
			}
			return _actionSets;
		}
	}

	private bool IsFinalAttackTrying => _strongAttackCount == 1;

	private void Start()
	{
		PlayerBehavior.LocalPlayer.DamageTaken += LocalPlayer_DamageTaken;
		_defaultActions.Add(_defaultAttackClipName);
	}

	public void GetScared()
	{
		PlayerBehavior.LocalPlayer.ChangeWeaponType(PlayerBehavior.WeaponFramework.SCARED);
		KSingleton<PlayerController>.Instance().MoveSpeed = _scaredPlayerMoveSpeed;
	}

	public void SetConnectionHook(PrologueConnectionHook connectionHook)
	{
		_connectionHook = connectionHook;
	}

	private string ChoiceRandomDefaultAction()
	{
		int index = Random.Range(0, _defaultActions.Count);
		return _defaultActions[index];
	}

	public void BattleEntered(AnimalBehavior enemy)
	{
		if (Object.op_Implicit((Object)(object)enemy) && ((Component)enemy).gameObject.activeInHierarchy)
		{
			_enemy = enemy;
			_battleMode = true;
		}
	}

	public void BattleExited()
	{
		_battleMode = false;
	}

	public void BattleReserveMove(Vector3 pos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_moveToPos = pos;
		_lastMoveReserveTime = Time.time;
		KSingleton<PrologueManager>.Instance().DelayedCall(delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			KSingleton<PlayerController>.Instance().MoveToTarget(pos);
		}, 1f);
	}

	public void BattleReserveAction(string actionSetId)
	{
		if (actionSetId == "onehand_sword_dodge_set")
		{
			ReserveDodge();
			_attackCoolTimeEnd = -1f;
			NotifyActionReserved(actionSetId, ActionSets[actionSetId]);
			return;
		}
		ActionInfo actionInfo = ActionSets.Get(actionSetId);
		if (actionInfo != null)
		{
			_nextActionSetId = actionSetId;
			_nextActionMotion = actionInfo.ActionId;
			if (actionInfo.EfxType == EfxType.Attack)
			{
				_attackCoolTimeEnd = -1f;
			}
			NotifyActionReserved(actionSetId, actionInfo);
		}
	}

	public void ReserveDodge()
	{
		_dodgeReserveTimeBegin = Time.time + 0.5f;
		if (Time.time <= _dodgeReserveTimeEnd)
		{
			_dodgeReserveTimeBegin = Time.time;
		}
		_dodgeReserveTimeEnd = _dodgeReserveTimeBegin + 4f;
		KSingleton<PrologueManager>.Instance().DelayedCall(delegate
		{
			_connectionHook.SendReactiveActionStandBy("onehand_sword_dodge_set", ActionSets["onehand_sword_dodge_set"].ActionId, Connections.Frontend.GetPredictedServerTime(), Connections.Frontend.GetPredictedServerTime() + 2.0, Connections.Frontend.GetPredictedServerTime() + 3.0);
		}, 1f);
	}

	public bool IsDodgeable()
	{
		return _dodgeReserveTimeBegin <= Time.time && Time.time < _dodgeReserveTimeEnd;
	}

	private void NotifyActionReserved(string actionSetId, ActionInfo actionInfo)
	{
		_connectionHook.RequestNotifyActionReserved(actionSetId, actionInfo.ActionId, actionInfo.EfxType);
	}

	private void Update()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)null == (Object)(object)_enemy)
		{
			_battleMode = false;
		}
		else
		{
			if (!_battleMode || Time.time - _lastUpdateTime < 0.3f)
			{
				return;
			}
			_lastUpdateTime = Time.time;
			if (_nextScheduleTime > Time.time)
			{
				return;
			}
			if (_moveToPos != Vector3.zero)
			{
				Vector3 val = _moveToPos - PlayerBehavior.LocalPlayer.CurrentPosition;
				if (((Vector3)(ref val)).magnitude < 150f)
				{
					_moveToPos = Vector3.zero;
				}
				if (Time.time - _lastMoveReserveTime > 5f)
				{
					_moveToPos = Vector3.zero;
				}
				return;
			}
			Vector3 val2 = _enemy.CurrentPosition - PlayerBehavior.LocalPlayer.CurrentPosition;
			float magnitude = ((Vector3)(ref val2)).magnitude;
			bool flag = Time.time > _attackCoolTimeEnd && IsAttackablePhase();
			bool flag2 = (IsFinalAttackTrying && IsFinalAttackablePos(magnitude)) || magnitude < AttackableRangeMax;
			if (flag && flag2)
			{
				string motionName;
				if (string.IsNullOrEmpty(_nextActionMotion))
				{
					motionName = ChoiceRandomDefaultAction();
				}
				else
				{
					motionName = _nextActionMotion;
					_connectionHook.SendActiveActionUsed(_nextActionSetId, _nextActionMotion, 4.5f);
					_nextActionMotion = null;
				}
				AttackTarget(motionName);
			}
			else
			{
				ChaseTarget();
			}
		}
	}

	private void AttackTarget(string motionName)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		Animation[] componentsInChildren = ((Component)PlayerBehavior.LocalPlayer).GetComponentsInChildren<Animation>(true);
		Animation val = componentsInChildren[0];
		string text = ((!PlayerBehavior.LocalPlayer.IsMale) ? "F_" : "M_");
		float length = val[text + motionName].length;
		MessagePackObjectDictionary val2 = new MessagePackObjectDictionary();
		Notify msg = default(Notify);
		msg.Method = 1512;
		val2.Add(MessagePackObject.op_Implicit("entity_id"), MessagePackObject.op_Implicit(PrologueAIRaptor.FakeEntityId));
		Vector3 val3 = TerrainA6.ClientPositionToWorldPosition(PredictEnemyPos(length));
		val2.Add(MessagePackObject.op_Implicit("pos"), new MessagePackObject((IList<MessagePackObject>)new List<MessagePackObject>
		{
			MessagePackObject.op_Implicit(val3.x),
			MessagePackObject.op_Implicit(val3.z)
		}));
		val2.Add(MessagePackObject.op_Implicit("action_name"), MessagePackObject.op_Implicit(motionName));
		val2.Add(MessagePackObject.op_Implicit("motion"), MessagePackObject.op_Implicit(motionName));
		val2.Add(MessagePackObject.op_Implicit("event_at"), MessagePackObject.op_Implicit(0));
		msg.Data = val2;
		_connectionHook.RequestNotifyMsg(msg);
		PlayerBehavior.LocalPlayer.RotateToPosition(_enemy.CurrentPosition);
		MoveLock(length);
		_attackCoolTimeEnd = Time.time + _attackCoolTime;
		_connectionHook.SendActionBegun(_nextActionSetId, Time.time + length, _attackCoolTimeEnd);
	}

	private Vector3 PredictEnemyPos(float deltaTime)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return _enemy.CurrentPosition + _enemy.CurrentVelocity * deltaTime;
	}

	private bool IsFinalAttackablePos(float distToEnemy)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = KSingleton<TrainTrexController>.Instance().PlayerTeleportPosition - PlayerBehavior.LocalPlayer.CurrentPosition;
		float magnitude = ((Vector3)(ref val)).magnitude;
		return distToEnemy < AttackableRangeMax * 2f && magnitude < _cutScenePosThreshould;
	}

	private void ChaseTarget()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Vector3 currentPosition = _enemy.CurrentPosition;
		Vector3 currentPosition2 = PlayerBehavior.LocalPlayer.CurrentPosition;
		float num = Random.Range(AttackableRangeMin, AttackableRangeMax);
		Vector3 val = KMathUtil.Make2D(currentPosition2 - currentPosition);
		if (((Vector3)(ref val)).magnitude < AttackableRangeMin)
		{
			num = _avoidCollisionDistance;
		}
		Vector3 val2 = currentPosition + ((Vector3)(ref val)).normalized * num;
		if (IsFinalAttackTrying)
		{
			val2 = KSingleton<TrainTrexController>.Instance().PlayerTeleportPosition;
		}
		val2.y = 0f;
		val2.z += Random.Range(-100f, 100f);
		Vector3 val3 = val2 - currentPosition2;
		float magnitude = ((Vector3)(ref val3)).magnitude;
		float num2 = magnitude / _battleMoveSpeed;
		MessagePackObjectDictionary val4 = new MessagePackObjectDictionary();
		Notify msg = default(Notify);
		msg.Method = 1504;
		val4.Add(MessagePackObject.op_Implicit("entity_id"), MessagePackObject.op_Implicit(PrologueAIRaptor.FakeEntityId));
		Vector3 val5 = TerrainA6.ClientPositionToWorldPosition(val2);
		val4.Add(MessagePackObject.op_Implicit("pos"), new MessagePackObject((IList<MessagePackObject>)new List<MessagePackObject>
		{
			MessagePackObject.op_Implicit(val5.x),
			MessagePackObject.op_Implicit(val5.z)
		}));
		val4.Add(MessagePackObject.op_Implicit("move_speed"), MessagePackObject.op_Implicit(_battleMoveSpeed));
		val4.Add(MessagePackObject.op_Implicit("move_type"), MessagePackObject.op_Implicit("Aim"));
		val4.Add(MessagePackObject.op_Implicit("event_at"), MessagePackObject.op_Implicit(0));
		val4.Add(MessagePackObject.op_Implicit("distance"), MessagePackObject.op_Implicit(0f));
		msg.Data = val4;
		_connectionHook.RequestNotifyMsg(msg);
		MoveLock(Mathf.Max(0.5f, num2 - 0.5f));
	}

	private void MoveLock(float during)
	{
		_nextScheduleTime = Time.time + during;
	}

	private void LocalPlayer_DamageTaken(CharacterBehavior attacker, Damage damage)
	{
		float during = 0.5f;
		if (damage.Result == DamageResult.Dodged)
		{
			during = PlayerBehavior.LocalPlayer.GetClipLength(_dodgeClipName);
		}
		else if (damage.Result == DamageResult.Hit && (damage.Effects & DamageEffects.Blow) > DamageEffects.None)
		{
			during = PlayerBehavior.LocalPlayer.GetClipLength(_blowBeginClipName);
			during += 1f;
			during += PlayerBehavior.LocalPlayer.GetClipLength(_blowEndClipName);
		}
		MoveLock(during);
	}

	public int CalcDamage(bool isCriticalHit)
	{
		if (isCriticalHit)
		{
			return _damageCritical;
		}
		return Random.Range(_damageMin, _damageMax);
	}

	public void ReceiveDamage(GameObject attacker, Damage damage)
	{
		if (damage.Result != DamageResult.Dodged)
		{
			return;
		}
		_dodgeCount++;
		if (_dodgeGuideCompleted)
		{
			return;
		}
		GameSystem<PrologueToDoListSystem>.Instance().SetProgress("dodge", _dodgeCount);
		ToDoBase toDoBase = GameSystem<PrologueToDoListSystem>.Instance().FindToDo("dodge");
		if (toDoBase != null && toDoBase.TargetProgress <= _dodgeCount)
		{
			_dodgeGuideCompleted = true;
			SetAtackable(isAttackable: true);
			attacker.GetComponent<PrologueAIRaptor>().SetAiAllAttack();
			KSingleton<PrologueManager>.Instance().DelayedCall(delegate
			{
				GameSystem<PrologueGuideSystem>.Instance().SetNextGuide("DodgeComplete");
			}, 1f);
		}
	}

	public void SetAtackable(bool isAttackable)
	{
		_isAttackable = isAttackable;
	}

	public bool IsAttackablePhase()
	{
		return _isAttackable;
	}

	public void MakeDamageToDino(CharacterBehavior character, BodyPart part, string animKeyName)
	{
		bool isDead = false;
		Damage damage = default(Damage);
		bool flag = false;
		float num = HitRatio;
		if (animKeyName.Contains("Strong"))
		{
			if (_strongAttackCount == 0)
			{
				flag = true;
				KSingleton<PrologueManager>.Instance().DelayedCall(delegate
				{
					GameSystem<PrologueGuideSystem>.Instance().SetNextGuide("FirstActiveActionHit");
				}, 1f);
			}
			num = StrongAttackHitRatio;
		}
		else
		{
			flag = _normalAttackCount == 0;
		}
		if (flag || Random.value < num)
		{
			damage.Result = DamageResult.Hit;
		}
		else
		{
			damage.Result = DamageResult.Missed;
		}
		if (damage.Result == DamageResult.Hit)
		{
			if (Random.value < CriticalRatio)
			{
				damage.Effects |= DamageEffects.Critical;
			}
			if (animKeyName.Contains("Strong"))
			{
				damage.Effects = DamageEffects.Blow;
				damage.Effects |= DamageEffects.Critical;
			}
			else
			{
				damage.Effects = DamageEffects.KnockBack;
			}
			damage.Value = CalcDamage((damage.Effects & DamageEffects.Critical) != 0);
		}
		else
		{
			damage.Value = 0;
		}
		damage.Part = part;
		damage.Direction = DamageDirection.Front;
		damage.AttackType = AttackType.Axe;
		if ((damage.Effects & DamageEffects.Blow) > DamageEffects.None)
		{
			_strongAttackCount++;
			GameSystem<PrologueToDoListSystem>.Instance().SetProgress("active_action", _strongAttackCount);
			if (_strongAttackCount >= 2)
			{
				isDead = true;
				GameSystem<PrologueToDoListSystem>.Instance().SetCompleted("active_action", completed: true);
				_connectionHook.SendActiveActionCanceled();
				KSingleton<PrologueManager>.Instance().PlayTrexCutScene();
			}
		}
		else
		{
			_normalAttackCount++;
		}
		PlayerBehavior.LocalPlayer.DamageResultReceived(damage);
		((Component)character).GetComponent<PrologueAIRaptor>().OnTakeDamage(damage, isDead);
		character.OnTakeDamage(damage, ((Component)PlayerBehavior.LocalPlayer).gameObject);
		if (damage.Value > 0)
		{
			KSingleton<CameraShaker>.Instance().DamageShake(Mathf.Max(damage.Value, 30));
		}
		UIManager.AddDamageLabel(character, damage, ((Component)PlayerBehavior.LocalPlayer).GetComponent<CharacterBehavior>());
	}

	public void DebugResetBattleCounter(int count)
	{
		_strongAttackCount = count;
	}
}
