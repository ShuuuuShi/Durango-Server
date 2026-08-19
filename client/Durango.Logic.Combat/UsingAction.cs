using System;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.UI;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.Logic.Combat;

public class UsingAction
{
	public enum State
	{
		None,
		Prepare,
		Ready,
		Using,
		End
	}

	[CanBeNull]
	private BattleAction _action;

	[CanBeNull]
	private ItemData _tamingItem;

	[CanBeNull]
	private DamageableEntity _target;

	private float? _radius;

	private State _state;

	private float _stateFinishTime;

	private readonly UsingActionAlert _actionAlert = new UsingActionAlert();

	private readonly Observable<double> _tamingBeginAt = new Observable<double>();

	private PredictTimer _tamingTimer;

	public PredictTimer TamingTimer
	{
		get
		{
			if (_tamingTimer == null)
			{
				_tamingTimer = new PredictTimer(GameManager.PlayerId, "Taming");
				_tamingTimer.InterruptCondition = ~InterruptCondition.TakeDamage;
			}
			return _tamingTimer;
		}
	}

	public Observable<double> TamingBeginAt => _tamingBeginAt;

	public event Action<BattleAction, DamageableEntity> UsedAction;

	public event Action<BattleAction> ActionCanceled;

	public event Action<BattleAction> ActionFinished;

	public void Set([NotNull] BattleAction action, DamageableEntity target)
	{
		Clear();
		_action = action;
		_tamingItem = null;
		_state = State.None;
		_target = ((KUtility.GetSize(_action.Data.AttackInfo) <= 0) ? null : target);
		if (_target != null && _action.Data.Meta.UseRange > 0f)
		{
			_radius = ObjectManager.GetBoundRadius(_target.GetEntityTypeId()) * target.GameObject.transform.localScale.x + _action.Data.Meta.UseRange;
		}
		else
		{
			_radius = null;
		}
		Singleton<PlayerController>.Instance().IgnoreSimilarMoveDirection();
		_actionAlert.Set(action);
	}

	public void SetTamingAction([NotNull] ItemData tamingItem, DamageableEntity target)
	{
		if (!(target == null))
		{
			Clear();
			_action = null;
			_tamingItem = tamingItem;
			_state = State.None;
			_target = target;
			_radius = ObjectManager.GetBoundRadius(_target.GetEntityTypeId()) * target.GameObject.transform.localScale.x + 200f;
			Singleton<PlayerController>.Instance().IgnoreSimilarMoveDirection();
			_actionAlert.Set(null);
		}
	}

	public bool HasValue()
	{
		if (_action == null)
		{
			return _tamingItem != null;
		}
		return true;
	}

	public State GetState()
	{
		return _state;
	}

	public void Clear()
	{
		if (HasValue())
		{
			GameSystem<global::InputSystem>.Instance().MoveLock = false;
			if (_action != null)
			{
				switch (_state)
				{
				case State.Using:
					if (this.ActionCanceled != null)
					{
						this.ActionCanceled(_action);
					}
					break;
				case State.End:
					if (this.ActionFinished != null)
					{
						this.ActionFinished(_action);
					}
					break;
				}
			}
			_actionAlert.Set(null);
		}
		_action = null;
		_tamingItem = null;
		_state = State.None;
	}

	public void Update()
	{
		while (HasValue())
		{
			State state = _state;
			switch (_state)
			{
			case State.None:
				if (PlayerBehavior.LocalPlayer.IsRiding)
				{
					if (!PlayerBehavior.LocalPlayer.Driver.IsWaitForUnmountMotionFinish)
					{
						VehicleBase.RequestUnmountIfRiding(immediately: true);
					}
					Clear();
				}
				else
				{
					_state = ((!(_target == null)) ? State.Prepare : State.Ready);
				}
				break;
			case State.Prepare:
			{
				if (_target == null)
				{
					Clear();
					break;
				}
				PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
				Vector3 currentPosition = _target.GetCurrentPosition();
				Vector3 currentPosition2 = localPlayer.CurrentPosition;
				Vector3 vector = currentPosition - currentPosition2;
				if (_radius.HasValue)
				{
					float? radius = _radius;
					float? num;
					if (radius.HasValue)
					{
						float valueOrDefault = radius.GetValueOrDefault();
						float? radius2 = _radius;
						num = valueOrDefault * radius2.GetValueOrDefault();
					}
					else
					{
						num = null;
					}
					if (vector.sqrMagnitude > num)
					{
						Singleton<PlayerController>.Instance().MoveToPosition(currentPosition);
						break;
					}
				}
				float num2 = Maths.CalcYawWithTarget(currentPosition, currentPosition2);
				float num3 = Mathf.Abs(localPlayer.CurrentYaw - num2);
				num3 = Mathf.Min(num3, 360f - num3);
				if (num3 < 4f)
				{
					_state = State.Ready;
					if (localPlayer.CurrentPlayerClipInfo.Clip == GetActionMotion())
					{
						PlayerController.MotionUpdater.RefreshMotion(null, force: true, clearReservations: true);
						state = _state;
					}
				}
				else
				{
					Singleton<PlayerController>.Instance().TurnToYaw(num2, snap: true);
				}
				break;
			}
			case State.Ready:
			{
				float time = Time.time;
				string actionMotion = GetActionMotion();
				float num4 = 0f;
				if (_action != null)
				{
					Connections.Frontend.Send(new UseBattleAction
					{
						ActionId = _action.Data.Id,
						StartAt = Connections.Frontend.GetPredictedServerTime(),
						TargetEntityId = ((!(_target == null)) ? _target.GetEntityId() : null),
						TargetTile = ((!(_target == null)) ? _target.GetTile() : null)
					});
					num4 = _action.Data.Meta.ActionLength;
					LocalMotionUpdater motionUpdater = PlayerController.MotionUpdater;
					string motion = actionMotion;
					float playbackRate = _action.PlaybackRate;
					motionUpdater.Motion(motion, 0f, playbackRate);
					if (this.UsedAction != null)
					{
						this.UsedAction(_action, _target);
					}
				}
				else if (_tamingItem != null)
				{
					if (_target != null)
					{
						PredictTimer timer = TamingTimer;
						timer.Play(Connections.Frontend.Ping + 10f);
						Singleton<PlayerController>.Instance().RotateToPosition(_target.GetCurrentPosition(), snap: true);
						timer.SetMotion(actionMotion);
						Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer.Timer).AddIcon(_tamingItem.Icon);
						_tamingBeginAt.Value = Connections.Frontend.GetPredictedServerTime();
						Connections.Frontend.Send(new UseTamingAction
						{
							EntityId = _target.GetEntityId(),
							ToolItemId = _tamingItem.Id
						}).On(delegate(Messages.Timer msg, PacketHeader _)
						{
							timer.Play(msg.Duration);
						}).Rest(delegate
						{
							timer.Stop();
							_tamingBeginAt.Value = 0.0;
						});
					}
					num4 = 0.5f;
				}
				_stateFinishTime = time + num4;
				Singleton<PlayerController>.Instance().StopMove();
				GameSystem<global::InputSystem>.Instance().MoveLock = true;
				_state = State.Using;
				break;
			}
			case State.Using:
				if (_stateFinishTime < Time.time)
				{
					_state = State.End;
				}
				break;
			case State.End:
				Clear();
				break;
			default:
				Clear();
				break;
			}
			if (state == _state)
			{
				break;
			}
		}
		_actionAlert.Update();
	}

	private string GetActionMotion()
	{
		if (_action != null)
		{
			return _action.Motion;
		}
		if (_tamingItem != null)
		{
			return "Ride_Saddle";
		}
		return null;
	}
}
