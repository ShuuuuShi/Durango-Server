using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Player.Animation;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Shared.Region;
using UnityEngine;

public class LocalMoveOperator
{
	private bool _completeMoveTarget;

	private Vector3? _lastSentPosition;

	private Vector3? _lastSafePosition;

	private float _lastSentYaw = -999f;

	private byte? _lastSentFloor;

	private Vector3 _prevSliding = Vector3.zero;

	private float _slidingKeepLengthCounter;

	private readonly MoveMsgGenerator _moveMsgGenerator = new MoveMsgGenerator();

	private LocalMotionUpdater _motionUpdater;

	private LocalUnitPushArea _unitPushArea;

	private KeyValuePair<string, AnimationState> _cachedAnimationState;

	[NotNull]
	private static PlayerBehavior Player => PlayerBehavior.LocalPlayer;

	[NotNull]
	private static PlayerController Controller => Singleton<PlayerController>.Instance();

	public Vector3 LastSentPosition
	{
		get
		{
			return _lastSentPosition.GetValueOrDefault(Player.CurrentPosition);
		}
		private set
		{
			_lastSentPosition = value;
		}
	}

	public byte LastSentFloor
	{
		get
		{
			return _lastSentFloor.GetValueOrDefault(Player.Floor);
		}
		private set
		{
			_lastSentFloor = value;
		}
	}

	public bool MovedByManual { get; private set; }

	public float LastSentYaw
	{
		get
		{
			if (Mathf.Approximately(_lastSentYaw, -999f))
			{
				return Player.CurrentYaw;
			}
			return _lastSentYaw;
		}
		private set
		{
			_lastSentYaw = value;
		}
	}

	public event Action MovingBlocked;

	public event Action<float> YawUpdated;

	public event Action TargetYawReached;

	public event Action<Vector3> CollisionSlideOccurred;

	public LocalMoveOperator()
	{
		_unitPushArea = new LocalUnitPushArea();
	}

	public void SetMotionUpdater(LocalMotionUpdater motionUpdater)
	{
		_motionUpdater = motionUpdater;
	}

	public void UpdateLastSent(Vector3 position, float height, float yaw, byte floor)
	{
		position.y = height;
		LastSentPosition = position;
		LastSentYaw = yaw;
		LastSentFloor = floor;
	}

	private bool ProcessYaw(LocalMoveNavigator.YawParam yawParam, ref float yaw, ref bool isSnapYaw)
	{
		if (yawParam.YawType == LocalMoveNavigator.YawType.None)
		{
			return false;
		}
		float targetYaw = yawParam.TargetYaw;
		isSnapYaw = yawParam.IsSnapYaw;
		if ((Maths.DistanceAngDeg(yaw, targetYaw) <= 1f) | isSnapYaw)
		{
			yaw = targetYaw;
			isSnapYaw = true;
			if (this.TargetYawReached != null)
			{
				this.TargetYawReached();
			}
		}
		else
		{
			yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, Controller.RotateSpeed * Time.deltaTime);
			if (this.YawUpdated != null)
			{
				this.YawUpdated(yaw);
			}
		}
		return true;
	}

	public void Teleport(Vector3 pos, byte floor)
	{
		if (!(LastSentPosition == pos) || LastSentFloor != floor)
		{
			_lastSafePosition = Vector3.zero;
			float height = pos.y - (float)(floor * 200);
			UpdateLastSent(pos, height, LastSentYaw, floor);
			_moveMsgGenerator.UpdateCurrentLocation(addMove: false, pos, floor, height, LastSentYaw, movedByUserInput: true, force: true);
			_motionUpdater.ConditionChanged();
			_moveMsgGenerator.TrySendMoveMessage();
			Player.PathMovable.Process();
		}
	}

	public void ProcessLocalPlayerMovements(LocalMoveNavigator.MoveParam moveParam, LocalMoveNavigator.YawParam yawParam)
	{
		MovedByManual = false;
		if (!Controller.CutScenePlayMode)
		{
			Vector3 lastSentPosition = LastSentPosition;
			float height = lastSentPosition.y;
			lastSentPosition.y = 0f;
			byte floor = LastSentFloor;
			_motionUpdater.ProcessMotionTimer();
			float yaw = LastSentYaw;
			bool isSnapYaw = false;
			Vector3 delta = Vector3.zero;
			bool flag = false;
			PlayerAnimationClipInfo lastPlayerClipInfo = Player.LastPlayerClipInfo;
			if (lastPlayerClipInfo == null || !lastPlayerClipInfo.HasAnimTag(PlayerAnimationClipTag.Irrevocable))
			{
				delta = ProcessLocomotionLocalPlayer(lastSentPosition, moveParam);
				bool flag3 = (MovedByManual = delta.sqrMagnitude > 0f);
				flag = flag3;
			}
			if (!MovedByManual && TryParseRootMotionDelta(lastPlayerClipInfo, out delta))
			{
				delta = Player.MainTransform.localToWorldMatrix.MultiplyVector(delta);
				flag = true;
			}
			else
			{
				flag |= ProcessYaw(yawParam, ref yaw, ref isSnapYaw);
			}
			Vector3 vector = ProcessWaterFlowLocalPlayer(lastSentPosition, ref flag);
			_motionUpdater.IsWaterCarried = vector != Vector3.zero;
			delta += vector;
			Vector3 newPos = ((!CancelMoveIfNotLoaded(lastSentPosition, delta)) ? ProcessCollisionWithSliding(lastSentPosition, floor, height, moveParam, delta, ref flag, isRootMotionDelta: false) : lastSentPosition);
			ProcessHeight(lastSentPosition, ref newPos, ref floor, ref height);
			_motionUpdater.UpdateMovingCondition(MovedByManual);
			string currentMotionClip;
			bool currentMotionClip2 = _motionUpdater.GetCurrentMotionClip(out currentMotionClip);
			if (currentMotionClip2)
			{
				MotionOption currentMotionOption = _motionUpdater.GetCurrentMotionOption(isSnapYaw);
				_motionUpdater.ReserveMotionEquipment();
				_moveMsgGenerator.MotionChanged(currentMotionClip, _motionUpdater.PlaybackRate, (byte)currentMotionOption, flag);
			}
			bool force = !flag && currentMotionClip2;
			_moveMsgGenerator.UpdateCurrentLocation(flag, newPos, floor, height, yaw, MovedByManual, force);
			if (!flag)
			{
				_prevSliding = Vector3.zero;
			}
			UpdateLastSent(newPos, height, yaw, floor);
			if (_completeMoveTarget)
			{
				OnMoveFinished(moveParam.OnComplete);
				_completeMoveTarget = false;
			}
			_moveMsgGenerator.TrySendMoveMessage();
		}
	}

	private void OnMoveFinished(Action onComplete)
	{
		if (this.MovingBlocked != null)
		{
			this.MovingBlocked();
		}
		onComplete?.Invoke();
	}

	private static bool CancelMoveIfNotLoaded(Vector3 curPos, Vector3 delta)
	{
		if (GameManager.IsPrologueMode)
		{
			return false;
		}
		Vector3 vector = curPos + delta;
		if (Singleton<TerrainBase>.Instance().GetTileBiome(Util.ClientPositionToWorldPosition(vector)) == Biome.Invalid)
		{
			return true;
		}
		int tileCount = TerrainMeta.TileCount;
		Vector2 vector2 = Util.ClientPositionToTilePosition(vector);
		if (vector2.x >= (float)tileCount || vector2.y >= (float)tileCount || vector2.x < 0f || vector2.y < 0f)
		{
			return true;
		}
		return false;
	}

	private bool TryParseRootMotionDelta(PlayerAnimationClipInfo clip, out Vector3 delta)
	{
		delta = Vector3.zero;
		if (clip != null && clip.HasAnimTag(PlayerAnimationClipTag.RootMotion))
		{
			if (_cachedAnimationState.Key != clip.Clip)
			{
				_cachedAnimationState = new KeyValuePair<string, AnimationState>(clip.Clip, Player.Anim[Player.MotionPrefix + clip.Clip]);
			}
			AnimationState value = _cachedAnimationState.Value;
			if (value != null)
			{
				PlayerRootMotionPath path = clip.GetPath(Player.IsMale);
				float num = value.time;
				float num2 = Mathf.Max(0f, num - Time.deltaTime);
				if (clip.IsLoop)
				{
					num2 %= clip.Length;
					num %= clip.Length;
				}
				delta = path?.GetDelta(num2, num) ?? Vector3.zero;
				return true;
			}
		}
		return false;
	}

	private Vector3 ProcessLocomotionLocalPlayer(Vector3 curPos, LocalMoveNavigator.MoveParam moveParam)
	{
		Vector3 vector = Vector3.zero;
		if (moveParam.MoveType == LocalMoveNavigator.MoveType.Stop)
		{
			return vector;
		}
		float currentMoveSpeed = Controller.GetCurrentMoveSpeed();
		if (moveParam.MoveType == LocalMoveNavigator.MoveType.MoveToPosition || moveParam.MoveType == LocalMoveNavigator.MoveType.MoveToTarget)
		{
			Vector3 vector2 = moveParam.TargetPos - curPos;
			vector2.y = 0f;
			float magnitude = vector2.magnitude;
			if (magnitude < moveParam.DistanceThreshold || magnitude < Time.deltaTime * currentMoveSpeed)
			{
				_completeMoveTarget = true;
				return Vector3.zero;
			}
			vector = vector2.normalized;
		}
		else if (moveParam.MoveType == LocalMoveNavigator.MoveType.MoveInDirection)
		{
			vector = moveParam.MovingDir;
		}
		return vector * Time.deltaTime * currentMoveSpeed;
	}

	private Vector3 ProcessWaterFlowLocalPlayer(Vector3 curPos, ref bool addMove)
	{
		if (GameManager.IsPrologueMode || !TerrainBase.IsPlayerInitialized)
		{
			return Vector3.zero;
		}
		if (Player.GetBiome() == Biome.Lava)
		{
			return Vector3.zero;
		}
		if ((TerrainWater.WaterDepthLevel)Player.WaterDepthLevel <= TerrainWater.WaterDepthLevel.Foot)
		{
			return Vector3.zero;
		}
		if (_motionUpdater.IsWaterResist || Controller.WaterFlowRegisterSet.Count > 0)
		{
			return Vector3.zero;
		}
		if (Player.IsRiding && Player.Driver.Vehicle != null && Player.Driver.Vehicle.IgnoreWaterFlow)
		{
			return Vector3.zero;
		}
		Vector3 worldPosition = Util.ClientPositionToWorldPosition(curPos);
		Vector2 waterFlow = Singleton<TerrainBase>.Instance().GetWaterFlow(worldPosition);
		if (waterFlow == Vector2.zero)
		{
			return Vector3.zero;
		}
		addMove = true;
		return Time.deltaTime * new Vector3(waterFlow.x, 0f, waterFlow.y) * TerrainWater.RiverSpeed * Player.WaterDepth;
	}

	public static float? GetWorldHeight(Vector3 pos, byte floor, float height)
	{
		pos.y = (float)(floor * 200) + height;
		Ray ray = new Ray(pos + Vector3.up * 200f, Vector3.down);
		float? result = null;
		float num = float.MaxValue;
		int count;
		RaycastHit[] array = Collisions.RayCast(ray, 380f, LayerHelper.PropMask, out count);
		for (int i = 0; i < count; i++)
		{
			RaycastHit raycastHit = array[i];
			if (!(num < raycastHit.distance) && raycastHit.collider.CompareTag("Steppable"))
			{
				result = raycastHit.point.y;
				num = raycastHit.distance;
			}
		}
		return result;
	}

	private void ProcessHeight(Vector3 curPos, ref Vector3 newPos, ref byte floor, ref float height)
	{
		if (GameManager.IsPrologueMode || !TerrainBase.IsPlayerInitialized)
		{
			floor = 0;
			height = 0f;
		}
		else
		{
			if (curPos == newPos)
			{
				return;
			}
			Vector2 vector = Util.ClientPositionToTilePosition(newPos);
			float depth = Singleton<TerrainBase>.Instance().GetTileDepth(vector);
			if (TerrainWater.IsTooDeepToSwim(depth, Player.SwimmableDepthRatio))
			{
				newPos = curPos;
				if (!Player.Driver.IsHovering)
				{
					NoticeDeepWater();
				}
				return;
			}
			float? worldHeight = GetWorldHeight(newPos, floor, height);
			float num = height + (float)(floor * 200);
			byte b = floor;
			int? num2 = Singleton<TerrainBase>.Instance().GetTileObject(new Point2(vector))?.Stories;
			if (!num2.HasValue)
			{
				b = 0;
			}
			else if (floor >= num2.Value)
			{
				b = (byte)(num2.Value - 1);
			}
			if (Mathf.Abs(worldHeight.GetValueOrDefault(b * 200) - num) > 40f)
			{
				newPos = curPos;
				vector = Util.ClientPositionToTilePosition(newPos);
				num2 = Singleton<TerrainBase>.Instance().GetTileObject(new Point2(vector))?.Stories;
				floor = (byte)Mathf.Min(floor, Mathf.Max(0, num2.GetValueOrDefault() - 1));
				float? worldHeight2 = GetWorldHeight(curPos, floor, height);
				height = (worldHeight2.HasValue ? (worldHeight2.Value - (float)(floor * 200)) : 0f);
				return;
			}
			float num3;
			if (num2.HasValue)
			{
				num3 = (worldHeight.HasValue ? (worldHeight.Value - (float)(floor * 200)) : 0f);
				if (Mathf.Abs(num3) > 120.00001f)
				{
					if (num3 > 0f)
					{
						if (b + 1 < num2.Value)
						{
							b++;
							num3 -= 200f;
						}
					}
					else if (num3 < 0f && b > 0)
					{
						b--;
						num3 += 200f;
					}
				}
			}
			else
			{
				num3 = worldHeight.GetValueOrDefault();
			}
			floor = b;
			height = num3;
			if (floor > 0)
			{
				depth = 0f;
			}
			Controller.WaterRetardingSpeedRatio = TerrainWater.GetRelativeSpeed(depth);
		}
	}

	private static void NoticeDeepWater()
	{
		UIManager.SystemMsg("NoticeDeepWater", T._("깊어서 더이상 이동할 수 없습니다."), 2f);
	}

	private Vector3 ProcessUnitPushArea(Vector3 curPos, Vector3 delta, ref bool addMove)
	{
		if (_unitPushArea.ProcessUnitPush(curPos + delta * 0.5f, out var dir, out var power))
		{
			delta = delta * (1f - power) + dir * Time.deltaTime * 500f * 0.5f * power;
			addMove = true;
		}
		return delta;
	}

	private Vector3 ProcessCollisionWithSliding(Vector3 curPos, int floor, float height, LocalMoveNavigator.MoveParam moveParam, Vector3 delta, ref bool addMove, bool isRootMotionDelta)
	{
		if (Player.Driver.IsHovering)
		{
			return curPos + delta;
		}
		delta = ProcessUnitPushArea(curPos + ((float)(floor * 200) + height) * Vector3.up, delta, ref addMove);
		if (delta == Vector3.zero)
		{
			return curPos;
		}
		CollisionParam param = Collisions.CreateCollisionParam(curPos + ((float)(floor * 200) + height) * Vector3.up, delta);
		delta = ((!isRootMotionDelta && !GameSystem<InputSystem>.Instance().MovedByManual()) ? ProcessPathFindSliding(moveParam, param, ref addMove) : Collisions.ProcessSimpleSliding(param));
		if (GameManager.IsPrologueMode)
		{
			if (param.Distance < 0.3f && this.MovingBlocked != null)
			{
				this.MovingBlocked();
			}
			if (Collisions.TryCapsuleCast(Collisions.CreateCollisionParam(curPos, delta), out var _) == RayCastResult.Pass)
			{
				_lastSafePosition = curPos;
			}
			Vector3? lastSafePosition = _lastSafePosition;
			if (lastSafePosition.HasValue && delta.magnitude <= Mathf.Epsilon)
			{
				return _lastSafePosition.Value;
			}
		}
		return curPos + delta;
	}

	private bool CheckCollision(LocalMoveNavigator.MoveParam moveParam, CollisionParam param, bool collideOnOverlapped, out RaycastHit hit)
	{
		if (!Collisions.CheckCollision(param, collideOnOverlapped, out hit))
		{
			return false;
		}
		if (moveParam.MoveType != LocalMoveNavigator.MoveType.MoveToTarget || moveParam.TargetObj == null)
		{
			return true;
		}
		ImmovableBase componentInParent = moveParam.TargetObj.GetComponentInParent<ImmovableBase>();
		if (componentInParent != null)
		{
			GameObject gameObject = hit.collider.gameObject;
			if (gameObject.CompareTag("Interactable") && gameObject.GetComponentInParent<ImmovableBase>() == componentInParent)
			{
				_completeMoveTarget = true;
			}
		}
		VehicleBase componentInChildren = moveParam.TargetObj.GetComponentInChildren<VehicleBase>();
		if (componentInChildren != null && hit.collider.gameObject.GetComponentInParent<VehicleBase>() == componentInChildren)
		{
			_completeMoveTarget = true;
		}
		return true;
	}

	private Vector3 ProcessPathFindSliding(LocalMoveNavigator.MoveParam moveParam, CollisionParam param, ref bool addMove)
	{
		Vector3 vector = param.Direction * param.Distance;
		if (CheckCollision(moveParam, param, collideOnOverlapped: false, out var hit))
		{
			if (_prevSliding == Vector3.zero)
			{
				float num = Mathf.Atan2(param.Direction.z, param.Direction.x);
				float num2 = Mathf.Atan2(hit.normal.z, hit.normal.x);
				float f = num - num2;
				int num3 = ((Mathf.Abs(f) < (float)Math.PI) ? 1 : (-1));
				float f2 = num2 + Mathf.Sign(f) * (float)num3 * (float)Math.PI / 2f;
				_prevSliding.x = Mathf.Cos(f2);
				_prevSliding.z = Mathf.Sin(f2);
				if (this.CollisionSlideOccurred != null)
				{
					this.CollisionSlideOccurred(_prevSliding);
				}
				addMove = true;
				_slidingKeepLengthCounter = 50f;
			}
			param.Direction = _prevSliding;
			if (CheckCollision(moveParam, param, collideOnOverlapped: true, out hit))
			{
				_prevSliding = Vector3.zero;
				OnMoveFinished((!moveParam.CompleteIfBlocked) ? null : moveParam.OnComplete);
				return Vector3.zero;
			}
			vector = _prevSliding * param.Distance * 1f;
		}
		else
		{
			if (_prevSliding == Vector3.zero)
			{
				return vector;
			}
			if (_slidingKeepLengthCounter < 0f)
			{
				_prevSliding = Vector3.zero;
				if (this.CollisionSlideOccurred != null)
				{
					this.CollisionSlideOccurred(vector);
				}
				addMove = true;
			}
			else
			{
				param.Direction = _prevSliding;
				if (!CheckCollision(moveParam, param, collideOnOverlapped: true, out hit))
				{
					vector = _prevSliding * param.Distance;
					_slidingKeepLengthCounter -= param.Distance;
				}
			}
		}
		return vector;
	}
}
