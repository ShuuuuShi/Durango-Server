using System;
using System.Collections.Generic;
using System.Globalization;
using CombatData;
using K1Network;
using L10N;
using Messages;
using MsgPack;
using Shared.Battle;
using Shared.System;
using TimerData;
using UnityEngine;
using Yaml;

public class CombatSystem : GameSystem<CombatSystem>
{
	public enum State
	{
		None,
		Battle,
		Leaving,
		Waiting
	}

	public class CombatPolicyInfo
	{
		public string Id;

		public int Level;
	}

	private class ActionReservedTimer : TimerData.Timer
	{
		public bool IsDefensiveAction;
	}

	public const string ReservedActionNameMoveTo = "move_to";

	public const string ReservedActionNameChangeTarget = "change_target";

	public const int MaxActiveActions = 8;

	public System.Action WaitForAttackStarted;

	public Action<float> LeavingBattleStarted;

	public System.Action ServerSideBattleBegun;

	private static readonly ReplyMessageHandlerRegistrar InvalidReplyMessageHandlerRegistrar = default(ReplyMessageHandlerRegistrar);

	private static readonly Dictionary<string, TimerData.Timer> RequestedColosseumTimer = new Dictionary<string, TimerData.Timer>();

	private bool _combatMode;

	private State _combatState;

	private PlayerController _controller;

	private readonly Dictionary<string, CombatData.Action> _actionSet = new Dictionary<string, CombatData.Action>();

	private readonly List<CombatData.Action> _currentActiveActions = new List<CombatData.Action>();

	private IList<MessagePackObject> _actionsActiveInfos;

	private CharacterBehavior _lastAimTarget;

	private readonly List<TimerData.Timer> _actionTimers = new List<TimerData.Timer>();

	private EnemySelector _enemySelector;

	private ActionButtonContainer _actionButtonContainer;

	private ActionReservedTimer actionReservedTimer;

	[ExposedInEditor(null)]
	public bool CombatMode
	{
		get
		{
			return _combatMode;
		}
		set
		{
			bool flag = _combatMode != value;
			_combatMode = value;
			CombatState = (value ? State.Battle : State.None);
			if (flag)
			{
				if (value)
				{
					OnEnterCombatMode();
				}
				else
				{
					OnExitCombatMode();
				}
				if (this.ChangedCombatMode != null)
				{
					this.ChangedCombatMode(_combatMode);
				}
			}
		}
	}

	public State CombatState
	{
		get
		{
			return _combatState;
		}
		private set
		{
			_combatState = value;
			KSingleton<UIManager>.Instance().SideEffect.SetCombatEffect(value);
		}
	}

	public GameObject Target
	{
		get
		{
			return PlayerBehavior.LocalPlayer.Target;
		}
		private set
		{
			PlayerBehavior.LocalPlayer.Target = value;
		}
	}

	private PlayerController Controller
	{
		get
		{
			if ((Object)(object)_controller == (Object)null)
			{
				_controller = KSingleton<PlayerController>.Instance();
			}
			return _controller;
		}
	}

	public DamageRecorder DamageRecorder { get; private set; }

	public bool PvPEnable { get; set; }

	public static bool EnableAttackAlert { get; set; }

	public static bool EnableDamageLog { get; set; }

	public List<CombatData.Action> CurrentActiveActions => _currentActiveActions;

	public CombatPolicyInfo[] CurrentCombatPolicies { get; private set; }

	public bool RunAwayNow { get; set; }

	private EnemySelector EnemySelector
	{
		get
		{
			if ((Object)(object)_enemySelector == (Object)null)
			{
				_enemySelector = KSingleton<EnemySelector>.Instance();
			}
			return _enemySelector;
		}
	}

	private ActionButtonContainer ActionButtonContainer
	{
		get
		{
			if ((Object)(object)_actionButtonContainer == (Object)null)
			{
				_actionButtonContainer = UIManager.FindScript<ActionButtonGroup>().ActionButtons;
			}
			return _actionButtonContainer;
		}
	}

	public event Action<bool> ChangedCombatMode;

	public event Action<AttackAlert> AttackAlerted;

	public event Action<string, bool> PolicyChanged;

	public event Action<List<DamageDirection>> DirectionSelected;

	public event System.Action CombatPoliciesUpdated;

	public event System.Action ActiveActionsUpdated;

	public event System.Action TargetChanged;

	public event Action<Vector3> OnRequestMoveTo;

	private void Awake()
	{
		Connections.Frontend.On<Actions>(OnReceiveActionListMsg);
		Connections.Frontend.Legacy_RegisterNotificationHandler(1502, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnServerSideBattleBegin);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1503, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnServerSideBattleEnd);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1520, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnServerSideBattleLeaving);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1508, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnBattleTargetChanged, OnBattleTargetChangedNoDelay);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1504, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnServerSideMove);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1512, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnServerSideMotion);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1509, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnReceiveBattleActionsQueue);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1510, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnReciveActiveActionUpdate);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1511, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnAttackAlert);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1514, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnRunawayTimerStarted);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1515, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnRunawaySucceeded);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1517, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnPolicyChanged);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1518, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnDirectionSelected);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1516, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, null, OnActionReservedNoDelay);
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1519, delegate(Notify msg, PacketHeader header)
		{
			HandleNotifyAtServerTime(msg, OnTensionChanged);
		});
		Connections.Frontend.On(delegate(ActiveActionCanceled msg, PacketHeader header)
		{
			HandleMsgAtServerTime(msg, header, OnActiveActionCanceled);
		});
		Connections.Frontend.On(delegate(ActiveActionUsed msg, PacketHeader header)
		{
			HandleMsgAtServerTime(msg, header, OnActiveActionUsed);
		});
		Connections.Frontend.On(delegate(ReactiveActionStandby msg, PacketHeader header)
		{
			HandleMsgAtServerTime(msg, header, OnReactiveActionStandby);
		});
		Connections.Frontend.On(delegate(ReactiveActionActivated msg, PacketHeader header)
		{
			HandleMsgAtServerTime(msg, header, OnReactiveActionactivated);
		});
		Connections.Frontend.On(delegate(AttackCoolTimeUpdated msg, PacketHeader header)
		{
			HandleMsgAtServerTime(msg, header, OnAttackCoolTimeUpdated);
		});
		Connections.Frontend.On(delegate(ColosseumReplyUpdated msg, PacketHeader header)
		{
			HandleMsgAtServerTime(msg, header, OnColosseumReplyUpdated);
		});
		Connections.Frontend.On(delegate(ActionBegun msg, PacketHeader header)
		{
			HandleMsgAtServerTime(msg, header, OnActionBegun);
		});
		Connections.Frontend.On(delegate(WaitForAttack msg, PacketHeader header)
		{
			HandleMsgAtServerTime(msg, header, OnWaitForAttack);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Attack, delegate(InteractionObject target)
		{
			TryServerSideBattleEnter(target.EntityId);
		});
		GameSystem<EquipSystem>.Instance().OnUpdateEquipments += OnUpdateEquipments;
		DamageRecorder = ((Component)this).gameObject.AddComponent<DamageRecorder>();
		KSingleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			PlayerBehavior.LocalPlayer.Died += LocalPlayer_Died;
			KSingleton<PlayerController>.Instance().OnPickObject += OnPickObject;
			KSingleton<PlayerManager>.Instance().PlayerAppeared += HandleCharacterAppeared;
			KSingleton<PlayerManager>.Instance().PlayerDisappeared += HandleCharacterDisappeared;
			KSingleton<AnimalManager>.Instance().AnimalAppeared += HandleCharacterAppeared;
			KSingleton<AnimalManager>.Instance().AnimalDisappeared += HandleCharacterDisappeared;
			ParticleManager.Cache("Particle/FX_SkillActivated_Common_02.prefab");
			ParticleManager.Cache("Particle/FX_SkillActivated_Common_01.prefab");
			SoundManager.Cache("Sound/Effect/Action/Action_SkillActivate_01.wav");
		};
		KSingleton<GameManager>.Instance().Ready += delegate
		{
			Connections.Frontend.Send(default(GetActions));
		};
		PvPEnable = false;
	}

	private void OnEnterCombatMode()
	{
		Controller.CombatMode(combatMode: true);
		ScreenOrientationController.SetPortraitLock(ScreenOrientationController.PortraitLock.Combat);
	}

	private void OnExitCombatMode()
	{
		Controller.CombatMode(combatMode: false);
		ScreenOrientationController.SetPortraitUnlock(ScreenOrientationController.PortraitLock.Combat);
		UnSelectTarget();
		Controller.StopMove();
		Controller.MoveSpeed = 500f;
		PlayerBehavior.LocalPlayer.RunState = PlayerBehavior.RunStateEnum.Run;
		PlayerBehavior.LocalPlayer.ForceRemoveUnfiredArrow();
		StopAllActionTimers();
	}

	private void HandleNotifyAtServerTime(Notify msg, Action<Notify> handler, Action<Notify> handlerNoDelay = null)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		float delay = -1f;
		MessagePackObject val = default(MessagePackObject);
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("event_at"), ref val))
		{
			double num = ((MessagePackObject)(ref val)).AsDouble();
			double bufferedServerTime_Enhanced = Connections.Frontend.GetBufferedServerTime_Enhanced();
			delay = (float)(num - bufferedServerTime_Enhanced);
		}
		if (handler != null)
		{
			KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
			{
				handler(msg);
			}, delay);
		}
		handlerNoDelay?.Invoke(msg);
	}

	private void HandleMsgAtServerTime<TV>(TV msg, PacketHeader header, Action<TV, PacketHeader> handler)
	{
		float delay = (float)(header.Time - Connections.Frontend.GetBufferedServerTime_Enhanced());
		KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
		{
			handler(msg, header);
		}, delay);
	}

	private void HandleCharacterAppeared(CharacterBehavior target)
	{
		if (!((Object)(object)target == (Object)null) && target.EntityId == PlayerBehavior.LocalPlayer.TargetEntityId)
		{
			SelectTarget(((Component)PlayerBehavior.LocalPlayer).gameObject, ((Component)target).gameObject);
		}
	}

	private void HandleCharacterDisappeared(CharacterBehavior target)
	{
		if (!((Object)(object)target == (Object)null) && target.EntityId == PlayerBehavior.LocalPlayer.TargetEntityId)
		{
			UnSelectTarget();
		}
	}

	public void RequestBattleToTarget(GameObject target)
	{
		ulong entityId = ObjectIdentifier.GetEntityId(target);
		if (entityId != 0L)
		{
			RequestServerSideBattleEnter(entityId);
		}
	}

	public void ReEnterBattle()
	{
		Connections.Frontend.Send(default(ResumeBattle));
	}

	public void RequestAttack()
	{
		if (!Controller.IsInServerSideBattle)
		{
			Connections.Frontend.Send(default(RequestAttack));
		}
	}

	public void TogglePvPMode()
	{
		PvPEnable = !PvPEnable;
	}

	private void OnPickObject(Ray ray, PlayerController.TouchEvent touchEvent, ref bool result)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (touchEvent.IsTouchBegan)
		{
			result = false;
			return;
		}
		int mask = LayerMask.op_Implicit(LayerHelper.DefaultMask);
		if (CombatMode)
		{
			if (!KUtility.RayCastContextAction(ray, mask, "Enemy", out var pickingObject) && !KUtility.RayCastContextAction(ray, mask, "Player", out pickingObject))
			{
				mask = LayerMask.op_Implicit(LayerHelper.PropMask);
				if (!KUtility.RayCastContextAction(ray, mask, null, out pickingObject))
				{
					pickingObject = null;
				}
			}
			if (!DamageableEntity.IsDamageableEntity(pickingObject))
			{
				pickingObject = null;
			}
			if ((Object)(object)pickingObject != (Object)null && (Object)(object)pickingObject != (Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject)
			{
				UIManager.FindScript<CombatGroup>().SetFocusTarget(pickingObject);
				result = true;
				return;
			}
		}
		result = false;
	}

	public bool RequestChangeTarget(GameObject target)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (RequestedColosseumTimer.TryGetValue("change_target", out var value) && value.Remain > 0f)
		{
			return false;
		}
		if ((Object)(object)target == (Object)null)
		{
			return false;
		}
		ulong entityId = 0uL;
		Vector3 clientPosition = Vector3.zero;
		CharacterBehavior component = target.GetComponent<CharacterBehavior>();
		if ((Object)(object)component != (Object)null)
		{
			if (!component.IsAlive)
			{
				return false;
			}
			PlayerBehavior playerBehavior = component as PlayerBehavior;
			if (PlayerBehavior.LocalPlayer.ClanId != 0 && (Object)(object)playerBehavior != (Object)null && playerBehavior.ClanId == PlayerBehavior.LocalPlayer.ClanId)
			{
				return false;
			}
			entityId = component.EntityId;
			clientPosition = component.CurrentPosition;
		}
		Artifact component2 = target.GetComponent<Artifact>();
		if ((Object)(object)component2 != (Object)null)
		{
			entityId = component2.EntityId;
			clientPosition = component2.Center;
		}
		if ((Object)(object)Target != (Object)(object)target.gameObject)
		{
			Vector3 val = TerrainA6.ClientPositionToWorldPosition(clientPosition);
			RequestCombatInputReply("change_target", GameSystem<CombatSystem>.Instance().RequestTargetChange(entityId, new WorldPosition(val.x, val.z)), target);
			return true;
		}
		return false;
	}

	public void SelectTarget(GameObject attacker, GameObject enemy)
	{
		if (!((Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject == (Object)(object)attacker))
		{
			return;
		}
		if ((Object)(object)enemy == (Object)null)
		{
			UnSelectTarget();
			return;
		}
		Target = enemy;
		DamageRecorder.SelectTarget(Target);
		if (this.TargetChanged != null)
		{
			this.TargetChanged();
		}
	}

	private void PlayTargetChangedEffect(GameObject attacker, GameObject enemy)
	{
		EnemySelector.SetTarget(attacker.transform, enemy.transform);
	}

	public void UnSelectTarget()
	{
		Target = null;
		DamageRecorder.UnSelectTarget();
		if (this.TargetChanged != null)
		{
			this.TargetChanged();
		}
	}

	public ReplyMessageHandlerRegistrar ChangeCombatPolicy(string key)
	{
		return Connections.Frontend.Send(new ChangePolicy
		{
			Policy = key
		});
	}

	public ReplyMessageHandlerRegistrar SelectDirectionPolicy(DamageDirection dir)
	{
		return Connections.Frontend.Send(new SelectDirection
		{
			Direction = dir
		});
	}

	private void LocalPlayer_Died(PlayerBehavior player)
	{
		CombatMode = false;
	}

	private void OnUpdateEquipments()
	{
		UpdateActiveActions();
	}

	public void InitActionSetJson(Dictionary<string, ActionSet> dict)
	{
		_actionSet.Clear();
		foreach (KeyValuePair<string, ActionSet> item in dict)
		{
			_actionSet.Add(item.Key, new CombatData.Action(item.Key, item.Value));
		}
	}

	public CombatData.Action GetAction(string key)
	{
		if (_actionSet.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	private void OnReceiveActionListMsg(Actions msg, PacketHeader header)
	{
		foreach (KeyValuePair<string, CombatData.Action> item in _actionSet)
		{
			item.Value.InitDynamicValue();
		}
		_currentActiveActions.Clear();
		for (int i = 0; i < 8; i++)
		{
			_currentActiveActions.Add(null);
		}
		int num = 0;
		foreach (KeyValuePair<string, bool> actionSetAvailability in msg.ActionSetAvailabilities)
		{
			CombatData.Action action = GetAction(actionSetAvailability.Key);
			if (action != null)
			{
				action.IsLearned = actionSetAvailability.Value;
				_currentActiveActions[num] = GetAction(actionSetAvailability.Key);
				num++;
			}
		}
		UpdateActiveActions();
	}

	private void UpdateActiveActions()
	{
		UpdateActionStates(_actionsActiveInfos);
	}

	public void UpdateCombatPolicies(CombatPolicyInfo[] curPolicies)
	{
		CurrentCombatPolicies = curPolicies;
		if (this.CombatPoliciesUpdated != null)
		{
			this.CombatPoliciesUpdated();
		}
	}

	private void UpdateActionStates(IList<MessagePackObject> actionList)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		MessagePackObject val3 = default(MessagePackObject);
		for (int num = actionList?.Count ?? 0; i < num; i++)
		{
			MessagePackObject val = actionList[i];
			MessagePackObjectDictionary val2 = ((MessagePackObject)(ref val)).AsDictionary();
			if (val2 == null || !val2.TryGetValue(MessagePackObject.op_Implicit("id"), ref val3))
			{
				continue;
			}
			string text = ((MessagePackObject)(ref val3)).AsString();
			ActionState actionState = ActionState.Invalid;
			if (val2.TryGetValue(MessagePackObject.op_Implicit("state"), ref val3))
			{
				actionState = (ActionState)((MessagePackObject)(ref val3)).AsByte();
			}
			if (actionState == ActionState.Invalid)
			{
				Debug.LogError((object)("Invalid Action State : " + actionState));
				continue;
			}
			double since = ((!val2.TryGetValue(MessagePackObject.op_Implicit("since"), ref val3)) ? (-1.0) : ((MessagePackObject)(ref val3)).AsDouble());
			double until = ((!val2.TryGetValue(MessagePackObject.op_Implicit("until"), ref val3)) ? (-1.0) : ((MessagePackObject)(ref val3)).AsDouble());
			CombatData.Action action = GetAction(text);
			if (action == null)
			{
				Debug.LogError((object)("Invalid Action ID : " + text));
				continue;
			}
			action.State = actionState;
			action.Since = since;
			action.Until = until;
		}
		_actionsActiveInfos = actionList;
		if (this.ActiveActionsUpdated != null)
		{
			this.ActiveActionsUpdated();
		}
	}

	public void TryServerSideBattleEnter(ulong entityId)
	{
		if (Controller.IsInServerSideBattle || entityId == 0L)
		{
			return;
		}
		if (GameSystem<PlayerStatusEffectSystem>.Instance().IsActivated("newbie_shield"))
		{
			UIManager.MessageBox.Show(T._("지금 공격하면 [ffd85b][b]현대인의 향기[/b][-] 효과가 사라집니다.\n더이상 동물들이 피해가지 않습니다.\n원치 않으면 [ffd85b][b]취소[/b][-] 버튼을 누르십시오."), delegate(bool ok)
			{
				if (ok)
				{
					RequestServerSideBattleEnter(entityId);
				}
			});
		}
		else
		{
			RequestServerSideBattleEnter(entityId);
		}
	}

	public void RequestServerSideBattleEnter(ulong entityId)
	{
		if (!Controller.IsInServerSideBattle && entityId != 0L)
		{
			Connections.Frontend.Send(new EnterBattle
			{
				EntityId = entityId
			});
		}
	}

	public void RequestServerSideBattleLeaving()
	{
		if (Controller.IsInServerSideBattle)
		{
			Connections.Frontend.Send(default(ExitBattle));
		}
	}

	public ReplyMessageHandlerRegistrar SendReserveAction(string key)
	{
		return Connections.Frontend.Send(new ReserveAction
		{
			ActionSet = key
		});
	}

	public ReplyMessageHandlerRegistrar RequestTargetChange(ulong entityId, WorldPosition worldPos)
	{
		return Connections.Frontend.Send(new ReserveAction
		{
			ActionSet = "change_target",
			EntityId = entityId,
			Pos = worldPos
		});
	}

	public ReplyMessageHandlerRegistrar RequestMoveTo(Vector3 pos)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!CombatMode)
		{
			return InvalidReplyMessageHandlerRegistrar;
		}
		Vector3 val = TerrainA6.ClientPositionToWorldPosition(pos);
		ReplyMessageHandlerRegistrar result = Connections.Frontend.Send(new ReserveAction
		{
			ActionSet = "move_to",
			Pos = new WorldPosition(val.x, val.z),
			CancelMove = true
		}).On<ColosseumReply>(delegate
		{
		});
		if (this.OnRequestMoveTo != null)
		{
			this.OnRequestMoveTo(pos);
		}
		return result;
	}

	private void OnServerSideBattleBegin(Notify msg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		MessagePackObject val = default(MessagePackObject);
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("entity_id"), ref val))
		{
			ulong id = ((MessagePackObject)(ref val)).AsUInt64();
			GameObject enemy = KSingleton<ObjectManager>.Instance().FindObject(id);
			SelectTarget(((Component)PlayerBehavior.LocalPlayer).gameObject, enemy);
			_lastAimTarget = PlayerBehavior.LocalPlayer.CharacterTarget;
			string targetName = ((!Object.op_Implicit((Object)(object)_lastAimTarget)) ? null : _lastAimTarget.GetName());
			DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.BattleBegin, targetName);
		}
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("policy_infos"), ref val))
		{
			IList<MessagePackObject> list = ((MessagePackObject)(ref val)).AsList();
			int count = list.Count;
			List<CombatPolicyInfo> list2 = new List<CombatPolicyInfo>();
			for (int i = 0; i < count; i++)
			{
				MessagePackObject val2 = list[i];
				MessagePackObjectDictionary val3 = ((MessagePackObject)(ref val2)).AsDictionary();
				CombatPolicyInfo combatPolicyInfo = new CombatPolicyInfo();
				MessagePackObject val4 = val3[MessagePackObject.op_Implicit("id")];
				combatPolicyInfo.Id = ((MessagePackObject)(ref val4)).AsString();
				MessagePackObject val5 = val3[MessagePackObject.op_Implicit("level")];
				combatPolicyInfo.Level = ((MessagePackObject)(ref val5)).AsInt32();
				list2.Add(combatPolicyInfo);
			}
			UpdateCombatPolicies(list2.ToArray());
		}
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("start_damaged"), ref val))
		{
		}
		PlayerBehavior.LocalPlayer.PathMovable.RemoveClientSideMovements();
		KSingleton<PlayerController>.Instance().StopMove();
		Vehicle.RequestUnmountIfRiding();
		CombatMode = true;
		Controller.IsInServerSideBattle = true;
		KSingleton<CameraController>.Instance().BeginBattleCameraEffect();
		if (ServerSideBattleBegun != null)
		{
			ServerSideBattleBegun();
		}
	}

	public static void RequestColosseumReply<T>(string requestKey, ReplyMessageHandlerRegistrar requestMsgReturned, GameObject uiTarget, Vector3? offset = null, string icon = null, Action<ColosseumReply> onReply = null) where T : ProgressGauge
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (!ReplyMessageHandlerRegistrar.op_True(requestMsgReturned))
		{
			return;
		}
		ClearAllColosseumTimers();
		TimerData.Timer timer = new TimerData.Timer(9f, InterruptCondition.Dead);
		RequestedColosseumTimer[requestKey] = timer;
		if (Object.op_Implicit((Object)(object)uiTarget))
		{
			T val = TimerData.Timer.Play<T>(timer);
			val.SetTarget(uiTarget, (!offset.HasValue) ? Vector3.zero : offset.Value);
			IconProgressGauge iconProgressGauge = val as IconProgressGauge;
			if ((Object)(object)iconProgressGauge != (Object)null && !string.IsNullOrEmpty(icon))
			{
				iconProgressGauge.SetIcon(icon);
			}
		}
		requestMsgReturned.On(delegate(ColosseumReply msg, PacketHeader header)
		{
			float duration = (float)(msg.ScheduledAt - Connections.Frontend.GetBufferedServerTime_Enhanced());
			timer.Set(duration, InterruptCondition.Dead);
			if (onReply != null)
			{
				onReply(msg);
			}
		});
	}

	public static void RequestCombatInputReply(string requestKey, ReplyMessageHandlerRegistrar requestMsgReturned, GameObject uiTarget, Vector3? offset = null, Action<ColosseumReply> onReply = null)
	{
		RequestColosseumReply<CombatInputProgressGauge>(requestKey, requestMsgReturned, uiTarget, offset, null, onReply);
	}

	private static void ClearAllColosseumTimers()
	{
		Dictionary<string, TimerData.Timer>.Enumerator enumerator = RequestedColosseumTimer.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value?.Stop();
		}
	}

	private void OnServerSideBattleLeaving(Notify msg)
	{
		string targetName = ((!Object.op_Implicit((Object)(object)_lastAimTarget)) ? null : _lastAimTarget.GetName());
		DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.BattleEnd, targetName);
	}

	private void OnServerSideBattleEnd(Notify msg)
	{
		string targetName = ((!Object.op_Implicit((Object)(object)_lastAimTarget)) ? null : _lastAimTarget.GetName());
		DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.BattleEnd, targetName);
		CombatMode = false;
		Controller.IsInServerSideBattle = false;
		EndLeavingBattle();
		InteractionGroupHelper.ShowInteractionButtons("Battle", show: true);
	}

	private void OnBattleTargetChangedNoDelay(Notify msg)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = null;
		GameObject val2 = null;
		MessagePackObject val3 = default(MessagePackObject);
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("attacker_id"), ref val3))
		{
			ulong id = ((MessagePackObject)(ref val3)).AsUInt64();
			val = KSingleton<ObjectManager>.Instance().FindObject(id);
		}
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("target_id"), ref val3))
		{
			ulong id2 = ((MessagePackObject)(ref val3)).AsUInt64();
			val2 = KSingleton<ObjectManager>.Instance().FindObject(id2);
		}
		if ((Object)(object)val != (Object)null && (Object)(object)val2 != (Object)null)
		{
			PlayTargetChangedEffect(val, val2);
		}
	}

	private void OnBattleTargetChanged(Notify msg)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (CombatMode)
		{
			GameObject val = null;
			GameObject val2 = null;
			MessagePackObject val3 = default(MessagePackObject);
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("attacker_id"), ref val3))
			{
				ulong id = ((MessagePackObject)(ref val3)).AsUInt64();
				val = KSingleton<ObjectManager>.Instance().FindObject(id);
			}
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("target_id"), ref val3))
			{
				ulong id2 = ((MessagePackObject)(ref val3)).AsUInt64();
				val2 = KSingleton<ObjectManager>.Instance().FindObject(id2);
			}
			if ((Object)(object)val != (Object)null && (Object)(object)val2 != (Object)null)
			{
				SelectTarget(val, val2);
			}
		}
	}

	private void OnServerSideMove(Notify msg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		if (CombatMode)
		{
			MessagePackObject val = msg.Data[MessagePackObject.op_Implicit("pos")];
			IList<MessagePackObject> list = ((MessagePackObject)(ref val)).AsList();
			MessagePackObject val2 = list[0];
			float num = ((MessagePackObject)(ref val2)).AsSingle();
			MessagePackObject val3 = list[1];
			Vector3 pos = TerrainA6.WorldPositionToClientPosition(new Vector2(num, ((MessagePackObject)(ref val3)).AsSingle()));
			MessagePackObject val4 = default(MessagePackObject);
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("move_speed"), ref val4))
			{
				Controller.MoveSpeed = ((MessagePackObject)(ref val4)).AsSingle();
			}
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("move_type"), ref val4))
			{
				PlayerBehavior.LocalPlayer.RunState = (PlayerBehavior.RunStateEnum)(int)Enum.Parse(typeof(PlayerBehavior.RunStateEnum), ((MessagePackObject)(ref val4)).AsString(), ignoreCase: true);
			}
			float distanceThresh = 0f;
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("distance"), ref val4))
			{
				distanceThresh = ((MessagePackObject)(ref val4)).AsSingle();
			}
			Controller.MoveToTarget(pos, delegate
			{
				Controller.RotateToTarget();
			}, distanceThresh);
		}
	}

	private void OnServerSideMotion(Notify msg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		if (CombatMode)
		{
			MessagePackObject val = msg.Data[MessagePackObject.op_Implicit("pos")];
			IList<MessagePackObject> list = ((MessagePackObject)(ref val)).AsList();
			MessagePackObject val2 = list[0];
			float num = ((MessagePackObject)(ref val2)).AsSingle();
			MessagePackObject val3 = list[1];
			Vector3 pos = TerrainA6.WorldPositionToClientPosition(new Vector2(num, ((MessagePackObject)(ref val3)).AsSingle()));
			string motionState = null;
			float time = 0f;
			float playbackRate = 1f;
			string text = null;
			MessagePackObject val4 = default(MessagePackObject);
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("motion"), ref val4))
			{
				motionState = ((MessagePackObject)(ref val4)).AsString();
			}
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("time"), ref val4))
			{
				time = ((MessagePackObject)(ref val4)).AsSingle();
			}
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("playback_rate"), ref val4))
			{
				playbackRate = ((MessagePackObject)(ref val4)).AsSingle();
			}
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("equip"), ref val4))
			{
				text = ((MessagePackObject)(ref val4)).AsString();
			}
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("move_type"), ref val4))
			{
				PlayerBehavior.LocalPlayer.RunState = (PlayerBehavior.RunStateEnum)(int)Enum.Parse(typeof(PlayerBehavior.RunStateEnum), ((MessagePackObject)(ref val4)).AsString(), ignoreCase: true);
			}
			Controller.StopMove();
			Controller.RotateToPosition(pos, bSnap: true);
			PlayerController controller = Controller;
			string equip = text;
			controller.Motion(motionState, time, playbackRate, forceTransition: false, equip);
		}
	}

	private void OnReceiveBattleActionsQueue(Notify msg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		MessagePackObject val = default(MessagePackObject);
		if (CombatMode && !msg.Data.TryGetValue(MessagePackObject.op_Implicit("actions_queue"), ref val))
		{
		}
	}

	private void OnReciveActiveActionUpdate(Notify msg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		MessagePackObject val = default(MessagePackObject);
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("actions_list"), ref val))
		{
			UpdateActionStates(((MessagePackObject)(ref val)).AsList());
		}
	}

	public bool HasTimer(string key)
	{
		bool result = false;
		int i = 0;
		for (int count = _actionTimers.Count; i < count; i++)
		{
			if (_actionTimers[i] != null && _actionTimers[i].Subject == key)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void StopAllActionTimers()
	{
		int count = _actionTimers.Count;
		for (int i = 0; i < count; i++)
		{
			if (_actionTimers[i] != null)
			{
				_actionTimers[i].Stop();
			}
		}
		_actionTimers.Clear();
		actionReservedTimer = null;
	}

	private void StopAllActionTimers(float delay)
	{
		int count = _actionTimers.Count;
		for (int i = 0; i < count; i++)
		{
			if (_actionTimers[i] != null)
			{
				_actionTimers[i].Stop(delay);
			}
		}
		_actionTimers.Clear();
	}

	private void StopActionTimer(string subject)
	{
		int count = _actionTimers.Count;
		for (int num = count - 1; num >= count; num--)
		{
			if (_actionTimers[num] != null && subject == _actionTimers[num].Subject)
			{
				_actionTimers[num].Stop();
				_actionTimers.RemoveAt(num);
			}
		}
	}

	private void SetUntilTimeToAllActionTimers(float untilTime)
	{
		int count = _actionTimers.Count;
		for (int i = 0; i < count; i++)
		{
			if (_actionTimers[i] != null)
			{
				_actionTimers[i].Until = untilTime;
			}
		}
	}

	private TimerData.Timer GetActionTimer(string subject)
	{
		int count = _actionTimers.Count;
		for (int i = 0; i < count; i++)
		{
			if (_actionTimers[i] != null && subject == _actionTimers[i].Subject)
			{
				return _actionTimers[i];
			}
		}
		return null;
	}

	private void OnActiveActionCanceled(ActiveActionCanceled msg, PacketHeader header)
	{
		if (_actionTimers.Count > 0)
		{
			float num = (float)(msg.CanceledAt - Connections.Frontend.GetBufferedServerTime_Enhanced());
			SetUntilTimeToAllActionTimers(Time.time + num);
		}
	}

	private void OnActiveActionUsed(ActiveActionUsed msg, PacketHeader header)
	{
		TimerData.Timer actionTimer = GetActionTimer(msg.ActionSetId);
		if (actionTimer != null)
		{
			float delay = (float)(msg.UsedAt - Connections.Frontend.GetBufferedServerTime_Enhanced());
			actionTimer.Stop(delay);
		}
	}

	private void OnReactiveActionStandby(ReactiveActionStandby msg, PacketHeader header)
	{
		StopAllActionTimers();
		CreateOrUpdateTimer(msg.Since, msg.Until, msg.ActionSetId);
	}

	private void OnReactiveActionactivated(ReactiveActionActivated msg, PacketHeader header)
	{
		float delay = (float)(msg.ActivatedAt - Connections.Frontend.GetBufferedServerTime_Enhanced());
		StopAllActionTimers(delay);
	}

	private void OnAttackCoolTimeUpdated(AttackCoolTimeUpdated msg, PacketHeader header)
	{
		ActionButton actionButton = ActionButtonContainer.FindAutoActionButton();
		if (Object.op_Implicit((Object)(object)actionButton))
		{
			actionButton.UpdateAttackCoolTime(msg.Until);
		}
		if (actionReservedTimer != null && !actionReservedTimer.IsDefensiveAction)
		{
			double bufferedServerTime_Enhanced = Connections.Frontend.GetBufferedServerTime_Enhanced();
			float until = (float)(msg.Until - bufferedServerTime_Enhanced) + Time.time;
			actionReservedTimer.Set(actionReservedTimer.Since, until, InterruptCondition.Dead);
		}
	}

	private static void OnColosseumReplyUpdated(ColosseumReplyUpdated msg, PacketHeader header)
	{
		if (RequestedColosseumTimer.TryGetValue(msg.RequestKey, out var value))
		{
			float duration = (float)(msg.ScheduledAt - Connections.Frontend.GetBufferedServerTime_Enhanced());
			value.Set(duration, InterruptCondition.Dead);
		}
	}

	private void OnActionBegun(ActionBegun msg, PacketHeader header)
	{
		ClearAllColosseumTimers();
		ActionButtonContainer.ReserveAutoAction(msg.Until);
	}

	private void OnWaitForAttack(WaitForAttack msg, PacketHeader header)
	{
		WaitForAttack();
	}

	private void OnActionGaugeFinished(TimerData.Timer timer)
	{
		StopAllActionTimers();
	}

	private void OnAttackAlert(Notify msg)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		if (!EnableAttackAlert)
		{
			return;
		}
		AttackAlert obj = default(AttackAlert);
		MessagePackObject val = default(MessagePackObject);
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("center"), ref val))
		{
			IList<MessagePackObject> list = ((MessagePackObject)(ref val)).AsList();
			if (list != null && list.Count >= 2)
			{
				MessagePackObject val2 = list[0];
				Vector2 worldPosition = default(Vector2);
				worldPosition.x = ((MessagePackObject)(ref val2)).AsSingle();
				MessagePackObject val3 = list[1];
				worldPosition.y = ((MessagePackObject)(ref val3)).AsSingle();
				obj.Center = TerrainA6.WorldPositionToClientPosition(worldPosition);
			}
		}
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("yaw"), ref val))
		{
			obj.Yaw = ((MessagePackObject)(ref val)).AsInt32();
		}
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("radius"), ref val))
		{
			obj.Radius = ((MessagePackObject)(ref val)).AsInt32();
		}
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("rect_size_halves"), ref val))
		{
			IList<MessagePackObject> list2 = ((MessagePackObject)(ref val)).AsList();
			if (list2 != null && list2.Count >= 2)
			{
				ref Vector2 rectSizeHalves = ref obj.RectSizeHalves;
				MessagePackObject val4 = list2[0];
				rectSizeHalves.x = ((MessagePackObject)(ref val4)).AsSingle();
				ref Vector2 rectSizeHalves2 = ref obj.RectSizeHalves;
				MessagePackObject val5 = list2[1];
				rectSizeHalves2.y = ((MessagePackObject)(ref val5)).AsSingle();
			}
		}
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("attack_time"), ref val))
		{
			obj.At = ((MessagePackObject)(ref val)).AsDouble();
		}
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("angle"), ref val))
		{
			IList<MessagePackObject> list3 = ((MessagePackObject)(ref val)).AsList();
			if (list3 != null && list3.Count >= 2)
			{
				ref Point2 angle = ref obj.Angle;
				MessagePackObject val6 = list3[0];
				angle.x = ((MessagePackObject)(ref val6)).AsInt32();
				ref Point2 angle2 = ref obj.Angle;
				MessagePackObject val7 = list3[1];
				angle2.y = ((MessagePackObject)(ref val7)).AsInt32();
			}
		}
		if (this.AttackAlerted != null)
		{
			this.AttackAlerted(obj);
		}
	}

	public void SelectPolicy(int index)
	{
		if (!CombatMode)
		{
			return;
		}
		int num = ((CurrentCombatPolicies != null) ? CurrentCombatPolicies.Length : 0);
		if (index >= 0 && index < num)
		{
			string id = CurrentCombatPolicies[index].Id;
			PolicyButton policyButton = UIManager.FindScript<PolicyButtonGroup>().FindPolicyButton(id);
			if ((Object)(object)policyButton != (Object)null)
			{
				RequestCombatInputReply(id, ChangeCombatPolicy(id), ((Component)policyButton).gameObject);
			}
		}
	}

	public void SelectTargetFront()
	{
	}

	public void SelectAction(int index)
	{
		if (CombatMode)
		{
			int count = _currentActiveActions.Count;
			if (index >= 0 && index < count)
			{
				string id = _currentActiveActions[index].Id;
				UIManager.FindScript<ActionButtonGroup>().OnClickActionButton(id);
			}
		}
	}

	private void OnRunawayTimerStarted(Notify msg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		MessagePackObject val = default(MessagePackObject);
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("entity_id"), ref val))
		{
		}
		double num = 0.0;
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("time_remains"), ref val))
		{
			num = ((MessagePackObject)(ref val)).AsDouble();
		}
		float remainTime = 0f;
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("event_at"), ref val))
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			remainTime = (float)(((MessagePackObject)(ref val)).AsDouble() + num - predictedServerTime);
		}
		UIManager.SystemMsg(LocalizeSystem.Format("#battle_msg_runaway_timer_started", remainTime.ToString("N1", CultureInfo.CurrentCulture)));
		BeginLeavingBattle(remainTime);
	}

	private void OnRunawaySucceeded(Notify msg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		MessagePackObject val = default(MessagePackObject);
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("entity_id"), ref val))
		{
		}
		UIManager.SystemMsg(LocalizeSystem.Format("#battle_msg_runaway_succeeded"));
		EndLeavingBattle();
	}

	private void WaitForAttack()
	{
		if (!CombatMode)
		{
			CombatState = State.Waiting;
			InteractionButtonGroup.IsProhibitInteraction = true;
			UIManager.FindScript<ToDoListGroup>().HideToDoList();
			InteractionGroupHelper.ShowInteractionButtons("Battle", show: false);
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
			if (WaitForAttackStarted != null)
			{
				WaitForAttackStarted();
			}
		}
	}

	private void BeginLeavingBattle(float remainTime)
	{
		CombatState = State.Leaving;
		InteractionButtonGroup.IsProhibitInteraction = true;
		UIManager.FindScript<ToDoListGroup>().HideToDoList();
		InteractionGroupHelper.ShowInteractionButtons("LeavingBattle", show: false);
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		if (LeavingBattleStarted != null)
		{
			LeavingBattleStarted(remainTime);
		}
	}

	private void EndLeavingBattle()
	{
		CombatState = State.None;
		InteractionButtonGroup.IsProhibitInteraction = false;
		UIManager.FindScript<ToDoListGroup>().RestoreToDoList();
		InteractionGroupHelper.ShowInteractionButtons("LeavingBattle", show: true);
	}

	private void OnActionReservedNoDelay(Notify msg)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		EfxType efxType = EfxType.Invalid;
		string text = null;
		string text2 = null;
		double since = 0.0;
		double until = 0.0;
		MessagePackObject val = default(MessagePackObject);
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("action_set_id"), ref val))
		{
			text = ((MessagePackObject)(ref val)).AsString();
		}
		ActionButton actionButton = ActionButtonContainer.FindActionButton(text);
		if (!Object.op_Implicit((Object)(object)actionButton) || !actionButton.Reserved)
		{
			ActionButtonContainer.ReserveAction(text);
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("efx_type"), ref val))
			{
				efxType = (EfxType)(int)Enum.Parse(typeof(EfxType), ((MessagePackObject)(ref val)).AsString(), ignoreCase: true);
			}
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("event_at"), ref val))
			{
				since = ((MessagePackObject)(ref val)).AsDouble();
			}
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("until"), ref val))
			{
				until = ((MessagePackObject)(ref val)).AsDouble();
			}
			switch (efxType)
			{
			case EfxType.Attack:
				KSingleton<PlayerController>.Instance().ParticleEffect("Particle/FX_SkillActivated_Common_01.prefab");
				break;
			case EfxType.Defense:
			case EfxType.Util:
				KSingleton<PlayerController>.Instance().ParticleEffect("Particle/FX_SkillActivated_Common_02.prefab");
				break;
			case EfxType.Invalid:
				return;
			}
			SoundManager.Play("Sound/Effect/Action/Action_SkillActivate_01.wav");
			CreateOrUpdateTimer(since, until, text);
		}
	}

	private void CreateOrUpdateTimer(double since, double until, string actionSetId)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		float since2 = (float)(since - predictedServerTime) + Time.time;
		float until2 = (float)(until - predictedServerTime) + Time.time;
		if (actionReservedTimer != null && !actionReservedTimer.IsStop && actionReservedTimer.EntityId == GameManager.PlayerId && actionReservedTimer.Subject == actionSetId)
		{
			if (!actionReservedTimer.IsDefensiveAction)
			{
				actionReservedTimer.Set(actionReservedTimer.Since, until2, InterruptCondition.Dead);
			}
			return;
		}
		StopAllActionTimers();
		actionReservedTimer = new ActionReservedTimer();
		actionReservedTimer.Set(GameManager.PlayerId, actionSetId, since2, until2, InterruptCondition.Dead);
		actionReservedTimer.Finished += OnActionGaugeFinished;
		ActionProgressGauge actionProgressGauge = TimerData.Timer.Play<ActionProgressGauge>(actionReservedTimer);
		CombatData.Action action = GetAction(actionSetId);
		actionReservedTimer.IsDefensiveAction = action.ActionGroup == ActionGroup.Guard;
		actionProgressGauge.Set(action);
		_actionTimers.Add(actionReservedTimer);
	}

	private void OnTensionChanged(Notify msg)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		AnimalBehavior animalBehavior = null;
		MessagePackObject val = default(MessagePackObject);
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("entity_id"), ref val))
		{
			ulong id = ((MessagePackObject)(ref val)).AsUInt64();
			animalBehavior = KSingleton<AnimalManager>.Instance().GetAnimal(id);
		}
		if ((Object)(object)animalBehavior == (Object)null)
		{
			return;
		}
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("tension_name"), ref val))
		{
			string comment = T._(LocalizeSystem.Get(((MessagePackObject)(ref val)).AsString()), animalBehavior.GetName());
			UIManager.SystemMsg(comment, 4f);
		}
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("render_effects"), ref val))
		{
			MessagePackObjectDictionary val2 = ((MessagePackObject)(ref val)).AsDictionary();
			if (val2.TryGetValue(MessagePackObject.op_Implicit("color"), ref val))
			{
				Color color = KUtility.ToColor(((MessagePackObject)(ref val)).AsString());
				animalBehavior.ApplyTensionColor(color);
			}
		}
	}

	public void OnPolicyChanged(Notify msg)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		string arg = null;
		MessagePackObject val = default(MessagePackObject);
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("policy_name"), ref val))
		{
			arg = ((MessagePackObject)(ref val)).AsString();
		}
		RunAwayNow = false;
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("runaway"), ref val))
		{
			RunAwayNow = ((MessagePackObject)(ref val)).AsBoolean();
		}
		bool arg2 = false;
		if (msg.Data.TryGetValue(MessagePackObject.op_Implicit("use_direction"), ref val))
		{
			arg2 = ((MessagePackObject)(ref val)).AsBoolean();
		}
		if (this.PolicyChanged != null)
		{
			this.PolicyChanged(arg, arg2);
		}
	}

	public void OnDirectionSelected(Notify msg)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		List<DamageDirection> list = new List<DamageDirection>();
		MessagePackObject val = default(MessagePackObject);
		for (int i = 0; i <= 3; i++)
		{
			DamageDirection damageDirection = (DamageDirection)i;
			if (msg.Data.TryGetValue(MessagePackObject.op_Implicit(damageDirection.ToString().ToLower()), ref val) && ((MessagePackObject)(ref val)).AsBoolean())
			{
				list.Add(damageDirection);
			}
		}
		if (this.DirectionSelected != null)
		{
			this.DirectionSelected(list);
		}
	}
}
