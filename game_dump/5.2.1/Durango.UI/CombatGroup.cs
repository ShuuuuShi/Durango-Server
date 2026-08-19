using System;
using Durango.Logic.Combat;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.UI.Control;
using Durango.Utils;
using InteractionData;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Battle;
using UnityEngine;

namespace Durango.UI;

public class CombatGroup : UIBase
{
	public enum BattleViewMode
	{
		Normal,
		Battle,
		Mount
	}

	private const string BattleModeLockKey = "BattleModeLock";

	private const float BattleMinZoom = 0.252f;

	private const float BattleMaxZoom = 1.32f;

	[SerializeField]
	private CombatTargetWidget _targetWidget;

	[SerializeField]
	private CatapultStateWidget _cataplutStateWidget;

	[SerializeField]
	private CombatInspectors _inspectors;

	[SerializeField]
	private NestedPrefabLinker _actionButtonsLinker;

	[SerializeField]
	private ParticleType _selectEffect;

	[SerializeField]
	private ParticleType _dodgeEffect;

	private int _targetParticleId;

	private DamageableEntity _target;

	private BattleActionButtons _actionButtons;

	private InjuryEffectEmitter _injuryEffectEmitter;

	private Durango.Logic.Timer.Timer _actionCastingTimer;

	private float _battleZoom;

	private float _prevZoom;

	private readonly Observable<bool> _battleModeLock = new Observable<bool>();

	private BattleActionButton _actionButton;

	public Observable<bool> BattleModeLock => _battleModeLock;

	public BattleViewMode BattleView { get; private set; }

	public event Action<BattleViewMode> BattleViewChanged;

	private void TargetChaged(DamageableEntity newTarget)
	{
		if ((bool)_target)
		{
			EntitySelectEffect(_target.GameObject, selected: false);
		}
		string target;
		if (newTarget == null)
		{
			ParticleManager.Stop(_targetParticleId);
			_targetParticleId = 0;
			_target = null;
			target = null;
		}
		else
		{
			_target = newTarget;
			EntitySelectEffect(_target.GameObject, selected: true, Color.red, 4f);
			int num = ParticleManager.EmitFollow(_selectEffect, Vector3.zero, Quaternion.identity, _target.Transform);
			Singleton<ParticleManager>.Instance().RegisterAction(num, delegate(GameObject obj)
			{
				obj.GetComponent<ParticleSystem>();
			});
			if (_targetParticleId != 0)
			{
				ParticleManager.Stop(_targetParticleId);
			}
			_targetParticleId = num;
			target = _target.GetEntityId();
		}
		RefreshTargetWidget();
		_injuryEffectEmitter.SetTarget(target);
	}

	private void OnDamage(Damaged damaged)
	{
		bool num = damaged.AttackerId == PlayerBehavior.LocalPlayer.EntityId;
		bool flag = damaged.VictimId == PlayerBehavior.LocalPlayer.EntityId;
		Damage damage = damaged.Damage;
		if (num && damage.Value > 0)
		{
			bool num2 = (damage.Effects & DamageEffects.Critical) > DamageEffects.None;
			float num3 = ((!num2) ? 0.5f : 1f);
			float shakeScaleU = UnityEngine.Random.Range(50f, 80f) * num3;
			float shakeScaleV = UnityEngine.Random.Range(50f, 80f) * num3;
			float value = ((!num2) ? 0.5f : 1f);
			Singleton<CameraShaker>.Instance().Shake(shakeScaleU, shakeScaleV, null, value);
		}
		if (flag)
		{
			if (BattleView == BattleViewMode.Battle && _target == null)
			{
				GameSystem<CombatSystem>.Instance().SelectTarget(damaged.AttackerId);
			}
			DamageResult result = damage.Result;
			if (result == DamageResult.Dodged || result == DamageResult.Evaded || result == DamageResult.AutoDodged)
			{
				ParticleManager.EmitFollow(_dodgeEffect, Vector3.zero, Quaternion.identity, PlayerBehavior.LocalPlayer.Bip001Transform);
			}
		}
	}

	private void OnBattleActionUsed(BattleAction action)
	{
		Pair<float, float> castingBar = action.Data.Meta.CastingBar;
		if (!(castingBar.Item1 >= castingBar.Item2))
		{
			if (_actionCastingTimer != null)
			{
				_actionCastingTimer.Stop();
			}
			float time = Time.time;
			_actionCastingTimer = new Durango.Logic.Timer.Timer(time + castingBar.Item1, time + castingBar.Item2);
			Durango.Logic.Timer.Timer.Play<ActionProgressGauge>(_actionCastingTimer).Set(action.Data.Meta.Icon);
		}
	}

	private void OnBattleActionCanceled(BattleAction action)
	{
		if (_actionCastingTimer != null)
		{
			_actionCastingTimer.Stop();
		}
	}

	private void OnChangeCombatMode(bool isCombat)
	{
		if (isCombat)
		{
			SetBattleView(BattleViewMode.Battle);
		}
		else if (!_battleModeLock && BattleView == BattleViewMode.Battle)
		{
			SetBattleView(BattleViewMode.Normal);
		}
	}

	public Transform FindActionButton(int index)
	{
		return _actionButtons.GetButton(index);
	}

	public virtual void SetBattleView(BattleViewMode mode)
	{
		if (BattleView == mode)
		{
			return;
		}
		BattleViewMode battleView = BattleView;
		BattleView = mode;
		bool flag = mode > BattleViewMode.Normal;
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		if (flag)
		{
			if (battleView == BattleViewMode.Normal)
			{
				GameCursorUtil.SetGameCursorLocked(isLocked: true);
			}
		}
		else
		{
			_inspectors.Hide();
			GameSystem<CombatSystem>.Instance().ClearTarget();
		}
		SetBattleViewZoom(battleView, mode);
		RefreshTargetWidget();
		_cataplutStateWidget.gameObject.SetActive(mode == BattleViewMode.Mount);
		if (flag)
		{
			UIBase.CloseAllUI();
		}
		VisibleController.Hide(VisibleType.HideOnBattle, flag, "Battle", (!flag) ? 0.2f : 0f);
		PlayerController.MotionUpdater.IsBattleStand.Value = mode == BattleViewMode.Battle;
		if (mode == BattleViewMode.Battle && PlayerController.MotionUpdater.IsState("Stand"))
		{
			PlayerController.MotionUpdater.Motion("Prepare");
		}
		if (this.BattleViewChanged != null)
		{
			this.BattleViewChanged(mode);
		}
	}

	private void SetBattleViewZoom(BattleViewMode prev, BattleViewMode mode)
	{
		if (!GameManager.IsPrologueMode)
		{
			switch (prev)
			{
			case BattleViewMode.Battle:
				_battleZoom = Singleton<MainCamera>.Instance().Zoom;
				break;
			case BattleViewMode.Normal:
				_prevZoom = Singleton<MainCamera>.Instance().Zoom;
				break;
			}
			switch (mode)
			{
			case BattleViewMode.Battle:
				Singleton<CameraController>.Instance().ZoomRange(0.252f, 1.32f, 0.5f).Zoom(_battleZoom, 0.5f);
				break;
			case BattleViewMode.Mount:
				Singleton<CameraController>.Instance().ZoomRange(0.252f, 0.252f, 0.5f).Zoom(0.252f, 0.5f);
				break;
			case BattleViewMode.Normal:
				Singleton<CameraController>.Instance().ZoomRange(0.42f, 2.2f, 0.3f).Zoom(_prevZoom, 0.3f);
				break;
			}
		}
	}

	private void RefreshTargetWidget()
	{
		DamageableEntity target = null;
		switch (BattleView)
		{
		case BattleViewMode.Battle:
			target = _target;
			break;
		case BattleViewMode.Normal:
		{
			InteractionObject target2 = GameSystem<InteractionSystem>.Instance().Target;
			if (target2 != null && InteractionSystem.CombatTargetObjectFilter(target2.Target) != null && !ObjectIdentifier.IsAlly(target2.Target))
			{
				target = GameSystem<CombatSystem>.Instance().DamageableEntities.Get(target2.EntityId);
			}
			break;
		}
		}
		_targetWidget.SetTarget(target);
	}

	private void HideAllUI()
	{
		_inspectors.Hide();
		_cataplutStateWidget.gameObject.SetActive(value: false);
	}

	private void InteractionSystem_InteractionTargetSelected(InteractionObject obj)
	{
		RefreshTargetWidget();
	}

	private void OnDamageableEntitiesUpdate()
	{
		DamageableEntities damageableEntities = GameSystem<CombatSystem>.Instance().DamageableEntities;
		_actionButtons.SetToggleButton(damageableEntities.HasEnemies());
		if (BattleView != 0)
		{
			UpdateCombatModeTargets();
		}
	}

	private void UpdateCombatModeTargets()
	{
		_inspectors.BeginSetting();
		DamageableEntities damageableEntities = GameSystem<CombatSystem>.Instance().DamageableEntities;
		foreach (DamageableEntity ally in damageableEntities.GetAllies())
		{
			_inspectors.AddAlly(ally);
		}
		foreach (DamageableEntity enemy in damageableEntities.GetEnemies())
		{
			_inspectors.AddEnemey(enemy);
		}
		_inspectors.EndSetting();
	}

	private void OnScreenTouch(GameObject obj)
	{
		if (BattleView != 0 && (!(obj != null) || !NGUITools.IsChild(Singleton<UIManager>.Instance().UIRoot.transform, obj.transform)))
		{
			switch (BattleView)
			{
			case BattleViewMode.Mount:
				TouchScreenInMount();
				break;
			case BattleViewMode.Battle:
				TouchScreenInBattle();
				break;
			}
		}
	}

	private void TouchScreenInBattle()
	{
		Vector2 vector = MainCamera.ScreenPosToNGUIPos(UICamera.currentTouch.pos);
		DamageableEntity damageableEntity = null;
		float num = 1000000f;
		GameObject gameObject = ((!(_target == null)) ? _target.GameObject : null);
		float num2 = Singleton<MainCamera>.Instance().Zoom * 1.5f;
		foreach (DamageableEntity enemy in GameSystem<CombatSystem>.Instance().DamageableEntities.GetEnemies())
		{
			if (enemy == null)
			{
				continue;
			}
			GameObject gameObject2 = enemy.GameObject;
			if (!(gameObject2 == gameObject) && !(gameObject2 == PlayerBehavior.LocalPlayer.gameObject))
			{
				Vector2 vector2 = MainCamera.WorldToNGUIPos(enemy.GetInteractionPosition());
				float num3 = ObjectManager.GetBoundRadius(enemy.GetEntityTypeId()) * enemy.Transform.localScale.x * num2;
				float num4 = num3 * num3;
				float sqrMagnitude = (vector2 - vector).sqrMagnitude;
				if (sqrMagnitude <= num4 && sqrMagnitude < num)
				{
					num = sqrMagnitude;
					damageableEntity = enemy;
				}
			}
		}
		if (!(damageableEntity == null))
		{
			GameSystem<CombatSystem>.Instance().SelectTarget(damageableEntity);
		}
	}

	private void TouchScreenInMount()
	{
		VehicleBase vehicle = PlayerBehavior.LocalPlayer.Driver.Vehicle;
		VehicleProp vehicleProp = ((!(vehicle == null)) ? (vehicle as VehicleProp) : null);
		if (vehicleProp == null)
		{
			return;
		}
		Artifact artifact = vehicleProp.GetArtifact();
		if (artifact == null)
		{
			return;
		}
		Catapult artifactComponent = artifact.GetArtifactComponent<Catapult>();
		if (artifactComponent == null)
		{
			return;
		}
		if (Connections.Frontend.GetPredictedServerTime() < artifactComponent.LastProjectile.CoolingUntil)
		{
			ShowCatapultWarning(T._("아직 발사할 수 없습니다."));
		}
		else if (artifact.ArtifactState.Catapult.HasValue)
		{
			CatapultState value = artifact.ArtifactState.Catapult.Value;
			Vector3 center = artifact.Center;
			Vector3 vector = MainCamera.ScreenPosToWorldPos(UICamera.currentTouch.pos);
			float magnitude = (vector - center).magnitude;
			if (magnitude < value.AtkRangeMin)
			{
				ShowCatapultWarning(T._("너무 가까워서 발사할 수 없습니다."));
				return;
			}
			if (magnitude > value.AtkRangeMax)
			{
				ShowCatapultWarning(T._("너무 멀어서 발사할 수 없습니다."));
				return;
			}
			WorldPosition targetPosition = new WorldPosition(0f, 0f);
			targetPosition.SetFromClientPosition(vector);
			Connections.Frontend.Send(new FireProjectileFromVehicle
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile,
				TargetPosition = targetPosition
			});
			SoundManager.PlayEvent("ui_catapult_point_click");
		}
	}

	private void ShowCatapultWarning(string comment)
	{
		UIManager.SystemMsg("CatapultWarning", comment);
		SoundManager.PlayEvent("ui_catapult_unavailable");
	}

	private static void EntitySelectEffect(GameObject obj, bool selected, Color outlineColor = default(Color), float outlineWidth = 0f)
	{
		if (!(obj != null) || !obj.activeSelf)
		{
			return;
		}
		ImmovableBase component = obj.GetComponent<ImmovableBase>();
		if (component != null)
		{
			component.Select(selected);
			return;
		}
		CharacterBehavior component2 = obj.GetComponent<CharacterBehavior>();
		if (component2 != null)
		{
			component2.Select(selected, outlineColor, outlineWidth);
		}
	}

	private void UseBattleAction(string id)
	{
		GameSystem<CombatSystem>.Instance().UseBattleAction(id);
	}

	private void UsePetAction(string id)
	{
		Singleton<PetManager>.Instance().UsePetActiveSkill(id);
	}

	private void UseTaming()
	{
		if (_target == null)
		{
			return;
		}
		ItemData itemData = null;
		foreach (ItemData playerItem in GameSystem<InventorySystem>.Instance().PlayerItemList)
		{
			if ((itemData == null || itemData.Level < playerItem.Level) && playerItem.IsCapturable())
			{
				itemData = playerItem;
			}
		}
		if (itemData != null)
		{
			PlayerController.MotionUpdater.Motion("Avatar_Dress");
			GameSystem<CombatSystem>.Instance().UseTamingAction(_target, itemData);
		}
	}

	private void OnReceiveBackMessage(InputCommandMessage message)
	{
		if (LeaveBattleView())
		{
			GameSystem<InputSystem>.Instance().StopPropagation();
		}
	}

	private bool LeaveBattleView()
	{
		switch (BattleView)
		{
		case BattleViewMode.Battle:
			if (GameSystem<CombatSystem>.Instance().CombatMode)
			{
				if ((float)GameSystem<CombatSystem>.Instance().BattleLeaveAvailableAt < Time.time)
				{
					CombatSystem.ExitBattle();
				}
			}
			else
			{
				SetBattleView(BattleViewMode.Normal);
			}
			return true;
		default:
			return false;
		case BattleViewMode.Mount:
			PetManager.UnmountVehicle();
			return true;
		}
	}

	private void Awake()
	{
		_actionButtons = _actionButtonsLinker.Object.GetComponent<BattleActionButtons>();
		_battleZoom = Preferences.GetFloat("BattleZoom", 1.188f);
	}

	private void Start()
	{
		_battleModeLock.Value = Preferences.GetBool("BattleModeLock");
		Observable<bool> battleModeLock = _battleModeLock;
		battleModeLock.Changed = (Action<bool>)Delegate.Combine(battleModeLock.Changed, (Action<bool>)delegate(bool value)
		{
			Preferences.SetBool("BattleModeLock", value);
		});
		ParticleManager.Cache(_selectEffect);
		HideAllUI();
		BattleActionButtons actionButtons = _actionButtons;
		actionButtons.ActionUsed = (Action<string>)Delegate.Combine(actionButtons.ActionUsed, new Action<string>(UseBattleAction));
		BattleActionButtons actionButtons2 = _actionButtons;
		actionButtons2.PetActionUsed = (Action<string>)Delegate.Combine(actionButtons2.PetActionUsed, new Action<string>(UsePetAction));
		BattleActionButtons actionButtons3 = _actionButtons;
		actionButtons3.TamingActionUsed = (Action)Delegate.Combine(actionButtons3.TamingActionUsed, new Action(UseTaming));
		_actionButtons.BattleLeaveClicked += delegate
		{
			if (!_actionButtons.enabled)
			{
				UIManager.SystemMsg("BattleAction", T._("일정 시간 피해를 받지 않으면 전투에서 벗어날 수 있습니다."));
			}
			else
			{
				LeaveBattleView();
			}
		};
		_injuryEffectEmitter = new InjuryEffectEmitter();
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += OnChangeCombatMode;
		GameSystem<CombatSystem>.Instance().DamagedProcesser.Damaged += OnDamage;
		GameSystem<CombatSystem>.Instance().BattleActionUsed += OnBattleActionUsed;
		GameSystem<CombatSystem>.Instance().BattleActionCanceled += OnBattleActionCanceled;
		DamageableEntities damageableEntities = GameSystem<CombatSystem>.Instance().DamageableEntities;
		damageableEntities.Updated = (Action)Delegate.Combine(damageableEntities.Updated, new Action(OnDamageableEntitiesUpdate));
		Observable<DamageableEntity> target2 = GameSystem<CombatSystem>.Instance().Target;
		target2.Changed = (Action<DamageableEntity>)Delegate.Combine(target2.Changed, new Action<DamageableEntity>(TargetChaged));
		PlayerBehavior.LocalPlayer.Died += delegate
		{
			SetBattleView(BattleViewMode.Normal);
		};
		_targetWidget.SetTarget(null);
		if (GameManager.IsPrologueMode)
		{
			_targetWidget.transform.localScale = Vector3.zero;
			_inspectors.transform.localScale = Vector3.zero;
			return;
		}
		GameSystem<InteractionSystem>.Instance().InteractionTargetSelected += InteractionSystem_InteractionTargetSelected;
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Attack, delegate(InteractionObject target)
		{
			SetBattleView(BattleViewMode.Battle);
			GameSystem<CombatSystem>.Instance().SelectTarget(target.EntityId);
		});
		GameSystem<InputSystem>.Instance().On(InputCommand.Back, OnReceiveBackMessage, InputSystem.Priority.Higher);
		ToDoListGroupBase toDoListGroupBase = UIManager.FindScript<ToDoListGroupBase>();
		if (toDoListGroupBase != null)
		{
			toDoListGroupBase.AddWidthOnChanged(delegate(float ratio)
			{
				UIRect component = _actionButtons.GetComponent<UIRect>();
				component.rightAnchor.absolute = (int)((ratio - 1f) * (float)ToDoListGroupBase.Width);
				UIUtility.UpdateAnchors(component.transform);
			});
		}
	}

	private void OnEnable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onClick, new UICamera.VoidDelegate(OnScreenTouch));
	}

	private void OnDisable()
	{
		Preferences.SetFloat("BattleZoom", _battleZoom);
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Remove(UICamera.onClick, new UICamera.VoidDelegate(OnScreenTouch));
	}
}
