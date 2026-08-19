using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Combat;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Player.Animation;
using Durango.Terrain;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Battle;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class CombatSystem : GameSystem<CombatSystem>
{
	private bool _combatMode;

	private readonly UsingAction _usingAction = new UsingAction();

	private readonly DamagedProcesser _damagedProcesser = new DamagedProcesser();

	private readonly DamageableEntities _damageableEntities = new DamageableEntities();

	private readonly Dictionary<string, BattleAction> _currentActions = new Dictionary<string, BattleAction>();

	private bool _isDirtyPlaceholderActions = true;

	private readonly Dictionary<int, PlayerAction> _currentPlaceholderActions = new Dictionary<int, PlayerAction>();

	private readonly Observable<DamageableEntity> _target = new Observable<DamageableEntity>();

	private readonly Observable<float> _battleLeaveAvailableAt = new Observable<float>();

	private float? _actionSlotsDirtyAt;

	public BattleAction[] ActionSlots { get; private set; }

	public UsingAction UsingAction => _usingAction;

	public DamagedProcesser DamagedProcesser => _damagedProcesser;

	public DamageableEntities DamageableEntities => _damageableEntities;

	[ExposedInEditor(null)]
	public bool CombatMode
	{
		get
		{
			return _combatMode;
		}
		set
		{
			bool num = _combatMode != value;
			_combatMode = value;
			if (num)
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

	public Observable<DamageableEntity> Target => _target;

	public Observable<float> BattleLeaveAvailableAt => _battleLeaveAvailableAt;

	public static bool AttackAlertEnabled { get; set; }

	public event Action<bool> ChangedCombatMode;

	public event Action<AttackAlerted> AttackAlerted;

	public event Action<VehicleProjectileFired> VehicleProjectileFired;

	public event Action PlayerActionsUpdated;

	public event Action<BattleAction> BattleActionUsed;

	public event Action<BattleAction> BattleActionCanceled;

	public event Action<TargetChanged> TargetChanged;

	private void Awake()
	{
		Connections.Frontend.On<BattleBegun>(OnBattleBegin);
		Connections.Frontend.On<Damaged>(OnDamaged);
		Connections.Frontend.On<BattleEnded>(OnBattleEnded);
		Connections.Frontend.On<AttackAlerted>(OnAttackAlert);
		Connections.Frontend.On<TensionChanged>(OnTensionChanged);
		Connections.Frontend.On<Actions>(OnActions);
		Connections.Frontend.On<TargetChanged>(OnTargetChanged);
		Connections.Frontend.On(delegate(BattleScenario msg, PacketHeader header)
		{
			Move[] moves = msg.Moves;
			foreach (Move move in moves)
			{
				Connections.Frontend.Handle(4u, move, default(PacketHeader));
			}
			Damaged[] damages = msg.Damages;
			foreach (Damaged damaged2 in damages)
			{
				Connections.Frontend.Handle(12u, damaged2, default(PacketHeader));
			}
		});
		Connections.Frontend.On<VehicleProjectileFired>(OnVehicleProjectileFired);
		Durango.Utils.Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			PlayerBehavior.LocalPlayer.Died += delegate
			{
				_usingAction.Clear();
				CombatMode = false;
			};
			Durango.Utils.Singleton<PlayerController>.Instance().MoveStarted += delegate
			{
				_usingAction.Clear();
			};
			_damageableEntities.SetEnabled(enabled: true);
		};
		GameSystem<EquipSystem>.Instance().EquipmentsUpdated += CheckCurrentEquipments;
		_usingAction.UsedAction += OnUseAction;
		_usingAction.ActionFinished += delegate
		{
			_actionSlotsDirtyAt = 0f;
		};
		_usingAction.ActionCanceled += OnBattleActionCanceled;
		_damagedProcesser.ControllLost += delegate
		{
			_actionSlotsDirtyAt = 0f;
			_usingAction.Clear();
		};
		_damagedProcesser.Damaged += delegate(Damaged damaged)
		{
			bool num = damaged.AttackerId == PlayerBehavior.LocalPlayer.EntityId;
			bool flag = damaged.VictimId == PlayerBehavior.LocalPlayer.EntityId;
			if (num || flag)
			{
				_battleLeaveAvailableAt.Value = Mathf.Max(_battleLeaveAvailableAt, Time.time + Yaml.Util.Singleton<PlayerEntities>.Instance.player.battle_retreat_time);
			}
		};
		Observable<DamageableEntity> target2 = _target;
		target2.Changed = (Action<DamageableEntity>)Delegate.Combine(target2.Changed, (Action<DamageableEntity>)delegate(DamageableEntity target)
		{
			SelectBattleTarget msg2 = default(SelectBattleTarget);
			if (target != null)
			{
				msg2.EntityId = target.GetEntityId();
				msg2.Tile = target.GetTile();
			}
			Connections.Frontend.Send(msg2);
		});
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(OnReady);
	}

	private void Update()
	{
		_damageableEntities.Update();
		_damagedProcesser.Update();
		_usingAction.Update();
		if (_actionSlotsDirtyAt.HasValue && _actionSlotsDirtyAt.Value < Time.time)
		{
			UpdateActionSlots();
		}
	}

	private void OnReady()
	{
		Connections.Frontend.Send(default(GetActions));
	}

	private void OnEnterCombatMode()
	{
		OrientationController.LockRotation(OrientationController.RotationLock.Battle);
		_battleLeaveAvailableAt.Value = Time.time + Yaml.Util.Singleton<PlayerEntities>.Instance.player.battle_retreat_time;
	}

	private void OnExitCombatMode()
	{
		OrientationController.UnlockRotation(OrientationController.RotationLock.Battle);
		_damageableEntities.ClearTargets();
		PlayerBehavior.LocalPlayer.ProjectileController.ForceRemoveUnfiredArrow();
		_battleLeaveAvailableAt.Value = 0f;
	}

	public IEnumerable<BattleAction> GetCurrentBattleActions()
	{
		return _currentActions.Values;
	}

	public BattleAction GetBattleAction(string id)
	{
		return _currentActions.Get(id);
	}

	private void OnUseAction(BattleAction usedAction, DamageableEntity target)
	{
		_actionSlotsDirtyAt = 0f;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (usedAction.Cooldown > 0f)
		{
			usedAction.CooldownSince = predictedServerTime;
			usedAction.CooldownUntil = predictedServerTime + (double)usedAction.Cooldown;
		}
		if (usedAction.ActivatedUntil.HasValue)
		{
			usedAction.ActivatedUntil = 0.0;
		}
		Dictionary<ProhibitType, float> prohibitedTime = usedAction.Data.Meta.ProhibitedTime;
		foreach (BattleAction value in _currentActions.Values)
		{
			if (prohibitedTime != null)
			{
				float num = prohibitedTime.Get(value.Data.Meta.ProhibitType, 0f);
				if (num > 0f)
				{
					value.ProhibitedUntil = predictedServerTime + (double)num;
				}
			}
			ActionActiveCondition activeCondition = value.Data.Meta.ActiveCondition;
			if (activeCondition != null && activeCondition.ActiveType == ActiveType.UseAction && !(activeCondition.Value != usedAction.Data.Id))
			{
				value.ActivatedUntil = predictedServerTime + (double)activeCondition.Duration;
			}
		}
		if (this.BattleActionUsed != null)
		{
			this.BattleActionUsed(usedAction);
		}
	}

	private void OnBattleActionCanceled(BattleAction action)
	{
		if (this.BattleActionCanceled != null)
		{
			this.BattleActionCanceled(action);
		}
	}

	private void OnTargetChanged(TargetChanged msg, PacketHeader header)
	{
		_damageableEntities.TargetChange(msg);
		if (this.TargetChanged != null)
		{
			this.TargetChanged(msg);
		}
	}

	private void OnActions(Actions msg, PacketHeader header)
	{
		using Reusable<List<BattleAction>> reusable = ReusableList<BattleAction>.Pop();
		List<BattleAction> value = reusable.Value;
		if (msg.BattleActions != null)
		{
			ActionStatus[] battleActions = msg.BattleActions;
			for (int i = 0; i < battleActions.Length; i++)
			{
				ActionStatus actionStatus = battleActions[i];
				string id = actionStatus.Id;
				if (!_currentActions.TryGetValue(id, out var value2))
				{
					PlayerAction playerAction = SingletonDict<string, PlayerAction>.Get(actionStatus.Id);
					if (playerAction == null)
					{
						continue;
					}
					value2 = new BattleAction(playerAction);
				}
				value2.Cooldown = actionStatus.Cooltime;
				value2.Stamina = actionStatus.Stamina;
				value.Add(value2);
			}
		}
		SetCurrentBattleActions(value);
	}

	public void SetCurrentBattleActions(IEnumerable<BattleAction> actions)
	{
		_isDirtyPlaceholderActions = true;
		_currentActions.Clear();
		if (actions == null)
		{
			return;
		}
		foreach (BattleAction action in actions)
		{
			_currentActions[action.Data.Id] = action;
		}
		UpdateActionSlots();
	}

	private void UpdateActionSlots()
	{
		_actionSlotsDirtyAt = null;
		int? num = null;
		foreach (BattleAction value in _currentActions.Values)
		{
			PlayerActionSlot slot = value.Data.Meta.Slot;
			if (slot != null)
			{
				num = Mathf.Max(num.GetValueOrDefault(0), slot.Id);
			}
		}
		double? num2 = null;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (!num.HasValue)
		{
			ActionSlots = null;
		}
		else
		{
			ActionSlots = new BattleAction[num.Value + 1];
			foreach (BattleAction value2 in _currentActions.Values)
			{
				if (value2.ActivatedUntil.HasValue && value2.ActivatedUntil.Value < predictedServerTime)
				{
					continue;
				}
				PlayerActionSlot slot2 = value2.Data.Meta.Slot;
				if (slot2 != null && slot2.Id >= 0)
				{
					BattleAction battleAction = ActionSlots[slot2.Id];
					if (battleAction == null || battleAction.Data.Meta.Slot.Order < slot2.Order)
					{
						ActionSlots[slot2.Id] = value2;
					}
				}
			}
			BattleAction[] actionSlots = ActionSlots;
			foreach (BattleAction battleAction2 in actionSlots)
			{
				if (battleAction2 != null)
				{
					double? deactivateUntil = battleAction2.GetDeactivateUntil();
					if (num2.GetValueOrDefault(double.MaxValue) > deactivateUntil.GetValueOrDefault(double.MaxValue))
					{
						num2 = deactivateUntil.GetValueOrDefault();
					}
					if (num2.GetValueOrDefault(double.MaxValue) > battleAction2.ActivatedUntil.GetValueOrDefault(double.MaxValue))
					{
						num2 = battleAction2.ActivatedUntil.GetValueOrDefault();
					}
				}
			}
		}
		if (num2.HasValue && num2.Value > predictedServerTime)
		{
			_actionSlotsDirtyAt = Times.UnixTimeToUnityTime(num2.Value);
		}
		if (this.PlayerActionsUpdated != null)
		{
			this.PlayerActionsUpdated();
		}
	}

	private void CheckCurrentEquipments()
	{
		_isDirtyPlaceholderActions = true;
	}

	public void ClearTarget()
	{
		SelectTarget((DamageableEntity)null);
	}

	public void SelectTarget(string entityId)
	{
		if (string.IsNullOrEmpty(entityId))
		{
			ClearTarget();
			return;
		}
		bool isAlly;
		DamageableEntity target = DamageableEntities.Get(entityId, out isAlly);
		if (isAlly)
		{
			ClearTarget();
		}
		else
		{
			SelectTarget(target);
		}
	}

	public void SelectTarget(DamageableEntity target)
	{
		_target.Value = target;
	}

	public bool IsUsableTamingAction()
	{
		if (GameSystem<InputSystem>.Instance().MoveLock)
		{
			return false;
		}
		if (_usingAction.GetState() >= UsingAction.State.Ready)
		{
			return false;
		}
		if ((TerrainWater.WaterDepthLevel)PlayerBehavior.LocalPlayer.WaterDepthLevel >= TerrainWater.WaterDepthLevel.Waist)
		{
			return false;
		}
		PlayerAnimationClipInfo playerAnimationClipInfo = Durango.Utils.Singleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationClipInfo("Ride_Saddle");
		if (playerAnimationClipInfo != null && !PlayerController.MotionUpdater.IsPlayableMotion(playerAnimationClipInfo))
		{
			return false;
		}
		return true;
	}

	public bool IsUsableAction(BattleAction action)
	{
		if (action == null)
		{
			return false;
		}
		if (_damagedProcesser.ControllLostUntil.HasValue && Time.time < _damagedProcesser.ControllLostUntil.Value)
		{
			return false;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (predictedServerTime < action.CooldownUntil && predictedServerTime > action.CooldownSince)
		{
			return false;
		}
		if (predictedServerTime < action.ProhibitedUntil)
		{
			return false;
		}
		if (action.ActivatedUntil.HasValue && predictedServerTime > action.ActivatedUntil.Value)
		{
			return false;
		}
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if ((TerrainWater.WaterDepthLevel)localPlayer.WaterDepthLevel >= TerrainWater.WaterDepthLevel.Waist)
		{
			return false;
		}
		if (action.Stamina > 0f)
		{
			Gauge stamina = localPlayer.Stamina;
			if (stamina != null && stamina.Get() < action.Stamina)
			{
				return false;
			}
		}
		if (!localPlayer.IsRiding && !string.IsNullOrEmpty(action.Motion))
		{
			PlayerAnimationClipInfo playerAnimationClipInfo = Durango.Utils.Singleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationClipInfo(action.Motion);
			if (playerAnimationClipInfo != null && !PlayerController.MotionUpdater.IsPlayableMotion(playerAnimationClipInfo))
			{
				return false;
			}
		}
		return true;
	}

	public void UseBattleAction(string id)
	{
		UseBattleAction(id, Target, targetSelect: true);
	}

	public void UseBattleAction(string id, DamageableEntity target, bool targetSelect)
	{
		BattleAction battleAction = GetBattleAction(id);
		if (!IsUsableAction(battleAction))
		{
			return;
		}
		if (KUtility.GetSize(battleAction.Data.AttackInfo) > 0 && (target == null || !target.IsAlive))
		{
			float? num = null;
			target = null;
			Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
			foreach (DamageableEntity enemy in DamageableEntities.GetEnemies())
			{
				if (!(enemy == null) && enemy.IsAlive)
				{
					float sqrMagnitude = (enemy.GetCurrentPosition() - currentPosition).sqrMagnitude;
					if (sqrMagnitude < num.GetValueOrDefault(float.MaxValue))
					{
						num = sqrMagnitude;
						target = enemy;
					}
				}
			}
		}
		if (targetSelect && target != null)
		{
			SelectTarget(target);
		}
		_usingAction.Set(battleAction, target);
	}

	private void OnBattleBegin(BattleBegun msg, PacketHeader header)
	{
		if (msg.EntityId != GameManager.PlayerId)
		{
			PetAI petObject = Durango.Utils.Singleton<PetManager>.Instance().GetPetObject(msg.EntityId);
			if (petObject != null)
			{
				petObject.BattleBegin();
			}
		}
		else
		{
			CombatMode = true;
		}
	}

	private void OnDamaged(Damaged msg, PacketHeader header)
	{
		_damagedProcesser.Add(msg);
	}

	private void OnBattleEnded(BattleEnded msg, PacketHeader header)
	{
		if (msg.EntityId != PlayerBehavior.LocalPlayer.EntityId)
		{
			PetAI petObject = Durango.Utils.Singleton<PetManager>.Instance().GetPetObject(msg.EntityId);
			if (petObject != null)
			{
				petObject.BattleEnd();
			}
		}
		else
		{
			CombatMode = false;
		}
	}

	private void OnAttackAlert(AttackAlerted msg, PacketHeader header)
	{
		if (this.AttackAlerted != null)
		{
			this.AttackAlerted(msg);
		}
	}

	private void OnVehicleProjectileFired(VehicleProjectileFired msg, PacketHeader header)
	{
		Artifact artifact = Durango.Utils.Singleton<ArtifactManager>.Instance().Find(msg.EntityId);
		if (artifact != null)
		{
			artifact.GetArtifactComponent<Catapult>()?.FireProjectile(msg);
		}
		if (this.VehicleProjectileFired != null)
		{
			this.VehicleProjectileFired(msg);
		}
	}

	private void OnTensionChanged(TensionChanged msg, PacketHeader header)
	{
		AnimalBehavior animal = Durango.Utils.Singleton<AnimalManager>.Instance().GetAnimal(msg.EntityId);
		if (!(animal == null))
		{
			UIManager.SystemMsg(msg.TensionChangedText, 4f);
			if (msg.RenderEffects.TryGetValue("color", out var value))
			{
				Color color = value.ToColor();
				animal.ApplyTensionColor(color);
			}
		}
	}

	public PlayerAction GetPlaceholderAction(int index)
	{
		if (_isDirtyPlaceholderActions)
		{
			FillPlaceholderActions();
		}
		return _currentPlaceholderActions.Get(index);
	}

	private void FillPlaceholderActions()
	{
		_isDirtyPlaceholderActions = false;
		_currentPlaceholderActions.Clear();
		EquipSystem.EquipPreset equipPreset = GameSystem<EquipSystem>.Instance().GetEquipPreset(GameSystem<EquipSystem>.Instance().CurrentEquipPreset);
		if (equipPreset == null)
		{
			return;
		}
		using Reusable<HashSet<string>> reusable = ReusableHashSet<string>.Pop();
		using Reusable<HashSet<string>> reusable2 = ReusableHashSet<string>.Pop();
		foreach (string value2 in equipPreset.SlotItems.Values)
		{
			ItemData itemData = GameSystem<InventorySystem>.Instance().FindItem(value2);
			if (itemData == null)
			{
				continue;
			}
			foreach (TagData tag in itemData.Tags)
			{
				reusable.Value.Add(tag.Id);
			}
		}
		foreach (string item in reusable.Value)
		{
			TagAllowAction tagAllowAction = SingletonDict<string, TagAllowAction>.Get(item);
			if (tagAllowAction != null)
			{
				if (tagAllowAction.DefaultActions != null)
				{
					reusable2.Value.AddRange(tagAllowAction.DefaultActions);
				}
				if (tagAllowAction.SkillActions != null)
				{
					reusable2.Value.AddRange(tagAllowAction.SkillActions);
				}
			}
		}
		foreach (string item2 in reusable2.Value)
		{
			PlayerAction playerAction = SingletonDict<string, PlayerAction>.Get(item2);
			if (playerAction != null)
			{
				int id = playerAction.Meta.Slot.Id;
				if (!_currentPlaceholderActions.TryGetValue(id, out var value) || value.Meta.Slot.Order >= playerAction.Meta.Slot.Order)
				{
					_currentPlaceholderActions[id] = playerAction;
				}
			}
		}
	}

	public void UseTamingAction([NotNull] DamageableEntity target, [NotNull] ItemData tool)
	{
		if (IsUsableTamingAction())
		{
			_usingAction.SetTamingAction(tool, target);
		}
	}

	public static void ExitBattle()
	{
		Connections.Frontend.Send(default(ExitBattle));
	}

	public static bool IsPvPEnabled()
	{
		if (GameManager.Region.Role() == Role.Outpost || GameManager.Region.IsPvpIsland())
		{
			return OptionSystem.GetBattlePvPEnabled();
		}
		return false;
	}

	public static bool IsHostilePlayer([NotNull] PlayerBehavior otherPlayer)
	{
		if (PlayerBehavior.LocalPlayer.EntityId == otherPlayer.EntityId)
		{
			return false;
		}
		if (IsPvPEnabled() && !GameSystem<PartySystem>.Instance().IsInParty(otherPlayer.EntityId))
		{
			return !ClanSystem.IsMyClanOrAlliance(otherPlayer);
		}
		return false;
	}

	public static void ToggleAutoAction(bool on)
	{
	}
}
