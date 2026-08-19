using System;
using Durango.Utils;
using UnityEngine;

public class LocalMoveNavigator
{
	public enum MoveType
	{
		Stop,
		MoveInDirection,
		MoveToPosition,
		MoveToTarget
	}

	public enum YawType
	{
		None,
		TargetYaw,
		TargetPosition
	}

	public class MoveParam
	{
		public MoveType MoveType;

		public float DistanceThreshold;

		public Vector3 TargetPos;

		public Vector3 MovingDir;

		public bool HasGoal;

		public GameObject TargetObj;

		public Action OnComplete;

		public bool CompleteIfBlocked;

		public void Reset()
		{
			MoveType = MoveType.Stop;
			HasGoal = false;
			TargetObj = null;
			DistanceThreshold = 0f;
			TargetPos = default(Vector3);
			MovingDir = default(Vector3);
			OnComplete = null;
			CompleteIfBlocked = false;
		}
	}

	public class YawParam
	{
		public YawType YawType;

		public float TargetYaw = -999f;

		public Vector3 TargetPosition;

		public bool IsSnapYaw;

		public void Reset()
		{
			YawType = YawType.None;
			TargetYaw = -999f;
			TargetPosition = Vector3.zero;
			IsSnapYaw = false;
		}
	}

	public const float InvalidYaw = -999f;

	private readonly MoveParam _moveParam = new MoveParam();

	private readonly YawParam _yawParam = new YawParam();

	private static PlayerBehavior Player => PlayerBehavior.LocalPlayer;

	public void MoveOperator_MovingBlocked()
	{
		_yawParam.Reset();
		_moveParam.Reset();
	}

	public void MoveOperator_TargetYawReached()
	{
		_yawParam.Reset();
	}

	public void MoveOperator_CollisionSlideOccurred(Vector3 slidingDelta)
	{
		_yawParam.YawType = YawType.TargetYaw;
		_yawParam.TargetYaw = Maths.CalcYaw(slidingDelta);
	}

	public MoveParam GetMoveParam()
	{
		return _moveParam;
	}

	public YawParam GetYawParam()
	{
		return _yawParam;
	}

	public void UpdateTargetPosition(Vector3 lastPos)
	{
		if (_moveParam.MoveType == MoveType.MoveToTarget && _yawParam.YawType != YawType.TargetYaw && (bool)_moveParam.TargetObj)
		{
			Vector3 interactionPosition = InteractionObject.GetInteractionPosition(_moveParam.TargetObj);
			if (Vector3.SqrMagnitude(interactionPosition - _moveParam.TargetPos) > Mathf.Epsilon)
			{
				_moveParam.TargetPos = interactionPosition;
				RotateToPosition(_moveParam.TargetPos);
			}
		}
		if (_yawParam.YawType == YawType.TargetPosition)
		{
			_yawParam.YawType = YawType.TargetYaw;
			_yawParam.TargetYaw = Maths.CalcYawWithTarget(_yawParam.TargetPosition, lastPos);
		}
	}

	public void Stop()
	{
		if (Player.LookAtController != null)
		{
			Player.LookAtController.SetLookTarget(null);
		}
		_moveParam.Reset();
		if (!_yawParam.IsSnapYaw)
		{
			_yawParam.Reset();
		}
	}

	public void MoveInDirection(Vector3 dir)
	{
		_moveParam.TargetObj = null;
		if (Player.LookAtController != null)
		{
			Player.LookAtController.SetLookTarget(null);
		}
		_moveParam.Reset();
		_moveParam.MoveType = MoveType.MoveInDirection;
		_moveParam.MovingDir = dir;
		SetTargetYaw(Maths.CalcYaw(dir));
	}

	public void MoveToPosition(Vector3 pos, Action onComplete, float distanceThreshold, bool completeIfBlocked)
	{
		if (Player.IsAlive && !GameSystem<InputSystem>.Instance().MoveLock)
		{
			FillMoveTargetParam(pos, distanceThreshold, null, onComplete, completeIfBlocked);
			_moveParam.MoveType = MoveType.MoveToPosition;
		}
	}

	public void MoveToTarget(GameObject targetObj, Action onComplete, float distanceThreshold, bool completeIfBlocked)
	{
		if (Player.IsAlive && !GameSystem<InputSystem>.Instance().MoveLock && !(targetObj == null))
		{
			Vector3 interactionPosition = InteractionObject.GetInteractionPosition(targetObj);
			FillMoveTargetParam(interactionPosition, distanceThreshold, targetObj, onComplete, completeIfBlocked);
			_moveParam.MoveType = MoveType.MoveToTarget;
		}
	}

	private void FillMoveTargetParam(Vector3 targetPos, float distanceThreshold, GameObject targetObj, Action onComplete, bool completeIfBlocked)
	{
		_moveParam.HasGoal = true;
		_moveParam.TargetPos = targetPos;
		_moveParam.MovingDir = default(Vector3);
		_moveParam.DistanceThreshold = distanceThreshold;
		_moveParam.TargetObj = targetObj;
		_moveParam.OnComplete = onComplete;
		_moveParam.CompleteIfBlocked = completeIfBlocked;
		if (Player.LookAtController != null)
		{
			Player.LookAtController.SetLookTarget(targetObj);
		}
		RotateToPosition(_moveParam.TargetPos);
	}

	public void RotateToObject(GameObject target, bool snap = false)
	{
		if (!(target == null))
		{
			Vector3 interactionPosition = InteractionObject.GetInteractionPosition(target);
			RotateToPosition(interactionPosition, snap);
		}
	}

	public void RotateToPosition(Vector3 pos, bool snap = false)
	{
		_yawParam.YawType = YawType.TargetPosition;
		_yawParam.TargetPosition = pos;
		_yawParam.IsSnapYaw = snap;
	}

	public void SetTargetYaw(float yaw, bool snap = false)
	{
		_yawParam.YawType = YawType.TargetYaw;
		_yawParam.TargetYaw = yaw;
		_yawParam.IsSnapYaw = snap;
	}
}
