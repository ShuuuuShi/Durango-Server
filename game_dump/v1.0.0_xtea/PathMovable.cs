using System;
using System.Collections.Generic;
using Messages;
using NetworkEnums;
using UnityEngine;

public class PathMovable
{
	public struct LocationClient
	{
		public Vector3 ClientPosition;

		public byte Floor;

		public Vector2 Direction;

		public float Yaw;

		public double Time;
	}

	private readonly CharacterBehavior _owner;

	private readonly List<Movement> _movementQueue = new List<Movement>();

	private float _rotInterpReserveTime;

	private Vector3 _initialPos;

	private double _initialTime;

	private MotionOption _curMotionOption;

	private string _lastMotionName;

	public List<LocationClient> PathBuffer { get; private set; }

	public event Action<Movement> MovementProcessed;

	public PathMovable(CharacterBehavior owner)
	{
		_owner = owner;
		PathBuffer = new List<LocationClient>();
	}

	private static double GetServerTime()
	{
		return Connections.Frontend.GetBufferedServerTime_Enhanced();
	}

	public void HandleMoveMsg(Move msg)
	{
		Movement[] movements = msg.Movements;
		if (movements == null || movements.Length == 0)
		{
			return;
		}
		double time = movements[0].Path[0].Time;
		RemoveExpiredMovements(time);
		int num = movements.Length;
		for (int i = 0; i < num; i++)
		{
			Movement movement = movements[i];
			if (movement.Path != null && movement.Path.Length != 0)
			{
				Location location = movement.Path[movement.Path.Length - 1];
				if (!(location.Time < GetServerTime()) || i == num - 1)
				{
					_movementQueue.Add(movements[i]);
				}
			}
		}
	}

	public bool HasMovingPath()
	{
		if (_movementQueue.Count > 0)
		{
			return true;
		}
		if (PathBuffer.Count == 1 && PathBuffer[0].Time < GetServerTime())
		{
			return false;
		}
		return PathBuffer.Count >= 1;
	}

	public void RemoveExpiredMovements(double beginTime = -1.0)
	{
		if (beginTime < 0.0)
		{
			beginTime = GetServerTime();
		}
		while (true)
		{
			int count = _movementQueue.Count;
			if (count > 0 && _movementQueue[count - 1].Path[0].Time >= beginTime)
			{
				_movementQueue.RemoveAt(count - 1);
				continue;
			}
			break;
		}
	}

	public void RemoveClientSideMovements()
	{
		int count = _movementQueue.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			if ((_movementQueue[num].MotionOption & 0x20) > 0)
			{
				_movementQueue.RemoveAt(num);
			}
		}
	}

	public void ProcessMovementQueue()
	{
		if (_movementQueue.Count > 0 && DequeueMovement(out var movement))
		{
			AddMovement(movement);
			float num = 1f;
			if ((movement.MotionOption & 4) > 0)
			{
				num = -1f;
			}
			_rotInterpReserveTime = Time.time + num;
			if (this.MovementProcessed != null && (!(_lastMotionName == movement.MotionName) || (movement.MotionOption & 0x40) <= 0))
			{
				_lastMotionName = movement.MotionName;
				this.MovementProcessed(movement);
			}
		}
	}

	private bool DequeueMovement(out Movement movement)
	{
		double serverTime = GetServerTime();
		movement = _movementQueue[0];
		if (movement.Path.Length > 0 && serverTime < movement.Path[0].Time)
		{
			return false;
		}
		_movementQueue.RemoveAt(0);
		return true;
	}

	private void AddMovement(Movement movement)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		MotionOption motionOption = (MotionOption)movement.MotionOption;
		Location[] path = movement.Path;
		double time = path[0].Time;
		ForgetPastPath(GetServerTime());
		RemovePredictedPath(time);
		float yaw = path[0].Yaw;
		AddPathBuffer(path, motionOption, yaw);
		_initialPos = _owner.CurrentPosition;
		_initialTime = GetServerTime();
		_curMotionOption = (MotionOption)movement.MotionOption;
	}

	private void AddPathBuffer(Location[] path, MotionOption motionOption, float initialYaw)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		int num = path.Length;
		for (int i = 0; i < num; i++)
		{
			Location location = path[i];
			LocationClient item = default(LocationClient);
			item.ClientPosition = location.Position.ToClientPosition();
			item.Floor = location.Floor;
			item.Yaw = location.Yaw;
			if ((motionOption & MotionOption.ALIGN_TO_PATH) > MotionOption.NORMAL)
			{
			}
			if ((motionOption & MotionOption.USE_LOCAL_ROOT_YAW) > MotionOption.NORMAL)
			{
				item.Yaw = initialYaw;
			}
			item.Time = location.Time;
			PathBuffer.Add(item);
		}
	}

	private void ForgetPastPath(double at)
	{
		int count = PathBuffer.Count;
		if (count <= 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			if (PathBuffer[i].Time >= at)
			{
				if (i > 0)
				{
					PathBuffer.RemoveRange(0, i);
				}
				break;
			}
		}
	}

	private void RemovePredictedPath(double firstMoveTime)
	{
		if (PathBuffer.Count <= 0)
		{
			return;
		}
		int count = PathBuffer.Count;
		for (int i = 0; i < count; i++)
		{
			if (firstMoveTime <= PathBuffer[i].Time)
			{
				PathBuffer.RemoveRange(i, PathBuffer.Count - i);
				break;
			}
		}
	}

	public void ProcessMovements()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		double serverTime = GetServerTime();
		if (GetPathOrientation(PathBuffer, serverTime, out var pos, out var floor, out var curYaw, out var index, _owner.DebugPath, !_owner.IsPlayer))
		{
			pos.y = _owner.CurrentPosition.y;
			_owner.CurrentPosition = pos;
			_owner.Floor = floor;
			bool flag = Time.time < _rotInterpReserveTime;
			_owner.TurnToYaw(curYaw, !flag);
			PathBuffer.RemoveRange(0, index);
		}
	}

	public bool GetPathOrientation(List<LocationClient> pathBuffer, double time, out Vector3 pos, out byte floor, out float curYaw, out int index, bool debug = false, bool interpolateYaw = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		pos = Vector3.zero;
		floor = 0;
		curYaw = 0f;
		index = -1;
		int count = pathBuffer.Count;
		if (count <= 0)
		{
			return false;
		}
		for (int i = 0; i < count && !(time < pathBuffer[i].Time); i++)
		{
			index = i;
		}
		if (index == -1)
		{
			return false;
		}
		if (index == count - 1)
		{
			pos = pathBuffer[index].ClientPosition;
			floor = pathBuffer[index].Floor;
			curYaw = pathBuffer[index].Yaw;
		}
		else
		{
			LocationClient locationClient = pathBuffer[index];
			LocationClient locationClient2 = pathBuffer[index + 1];
			double num = time - locationClient.Time;
			double num2 = locationClient2.Time - locationClient.Time;
			float num3 = Mathf.Clamp((float)(num / num2), 0f, 1f);
			pos = Vector3.Lerp(locationClient.ClientPosition, locationClient2.ClientPosition, num3);
			floor = locationClient.Floor;
			curYaw = ((!interpolateYaw) ? locationClient.Yaw : Mathf.LerpAngle(locationClient.Yaw, locationClient2.Yaw, num3));
			if (!debug)
			{
			}
		}
		if ((_curMotionOption & MotionOption.PHYSICAL_FORCED) > MotionOption.NORMAL)
		{
			float num4 = Mathf.Clamp01((float)(time - _initialTime) / 0.5f);
			pos = Vector3.Lerp(_initialPos, pos, Mathf.Sqrt(num4));
		}
		return true;
	}

	public Vector3 GetPositionAt(double predictedServerTime)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)(predictedServerTime - GetServerTime());
		if (PathBuffer.Count <= 1)
		{
			return _owner.CurrentPosition + _owner.CurrentVelocity * num;
		}
		int num2 = -1;
		int count = PathBuffer.Count;
		for (int i = 0; i < count && !(predictedServerTime < PathBuffer[i].Time); i++)
		{
			num2 = i;
		}
		if (num2 == -1)
		{
			return _owner.CurrentPosition + _owner.CurrentVelocity * num;
		}
		if (num2 == count - 1)
		{
			return PathBuffer[num2].ClientPosition;
		}
		LocationClient locationClient = PathBuffer[num2];
		LocationClient locationClient2 = PathBuffer[num2 + 1];
		double num3 = predictedServerTime - locationClient.Time;
		double num4 = locationClient2.Time - locationClient.Time;
		float num5 = Mathf.Clamp((float)(num3 / num4), 0f, 1f);
		return Vector3.Lerp(locationClient.ClientPosition, locationClient2.ClientPosition, num5);
	}

	public void DebugShowPath(float yaw)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		int count = PathBuffer.Count;
		for (int i = 1; i < count; i++)
		{
			Debug.DrawLine(PathBuffer[i - 1].ClientPosition, PathBuffer[i].ClientPosition, Color.green, 0.5f);
			Debug.DrawLine(PathBuffer[i].ClientPosition, PathBuffer[i].ClientPosition + KMathUtil.CalcDirectionFromYaw(PathBuffer[i].Yaw) * 100f, Color.red, 0.5f);
		}
		Vector3 val = Quaternion.AngleAxis(yaw, Vector3.up) * Vector3.forward;
		Debug.DrawLine(_owner.CurrentPosition, _owner.CurrentPosition + val * 100f, Color.green, 0.1f);
	}

	public static Location GetLocation(Move msg, double time)
	{
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		if (msg.Movements == null || msg.Movements.Length == 0)
		{
			return default(Location);
		}
		Point2 point = -Point2.one;
		Point2 point2 = -Point2.one;
		for (int i = 0; i < msg.Movements.Length; i++)
		{
			for (int j = 0; j < msg.Movements[i].Path.Length; j++)
			{
				if (time < msg.Movements[i].Path[j].Time)
				{
					point2.x = i;
					point2.y = j;
					break;
				}
				point.x = i;
				point.y = j;
			}
		}
		if (point.x == -1)
		{
			return default(Location);
		}
		if (point2.x == -1)
		{
			return msg.Movements[point.x].Path[point.y];
		}
		Location location = msg.Movements[point.x].Path[point.y];
		Location location2 = msg.Movements[point2.x].Path[point2.y];
		double num = time - location.Time;
		double num2 = location2.Time - location.Time;
		float num3 = Mathf.Clamp((float)(num / num2), 0f, 1f);
		Vector2 val = Vector2.Lerp(location.Position.ToVector2(), location2.Position.ToVector2(), num3);
		float yaw = Mathf.LerpAngle(location.Yaw, location2.Yaw, num3);
		Location result = default(Location);
		result.Position = new WorldPosition
		{
			x = val.x,
			y = val.y
		};
		result.Yaw = yaw;
		result.Floor = location.Floor;
		result.Time = time;
		return result;
	}
}
