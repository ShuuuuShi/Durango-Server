using System;
using System.Collections.Generic;
using System.Linq;
using Messages;
using UnityEngine;

public class PathMovable
{
	private readonly CharacterBehavior _owner;

	private readonly List<Movement> _movements = new List<Movement>();

	private Movement _prevProcessed;

	public event Action<Movement> MovementProcessed;

	public PathMovable(CharacterBehavior owner)
	{
		_owner = owner;
	}

	public void Clear()
	{
		_movements.Clear();
		_prevProcessed = default(Movement);
	}

	public void HandleMoveMsg(Move msg)
	{
		Movement[] movements = msg.Movements;
		if (movements != null && movements.Length != 0)
		{
			Movement[] array = movements;
			foreach (Movement movement in array)
			{
				HandleMovement(movement);
			}
		}
	}

	public void HandleMovement(Movement movement)
	{
		_movements.Add(movement);
	}

	public bool HasMovingPath()
	{
		return _movements.Count > 0;
	}

	public void Process()
	{
		double moveServerTime = _owner.GetMoveServerTime();
		Process(moveServerTime);
	}

	public void Process(double at)
	{
		int num = -1;
		for (int num2 = _movements.Count - 1; num2 >= 0; num2--)
		{
			Movement movement = _movements[num2];
			if (KUtility.GetSize(movement.Path) != 0)
			{
				Location location = movement.Path[0];
				if (location.Time < at)
				{
					Movement? next = null;
					if (num2 < _movements.Count - 1)
					{
						next = _movements[num2 + 1];
					}
					ProcessMovement(movement, next, at);
					num = num2;
					break;
				}
			}
		}
		if (num == -1)
		{
			return;
		}
		if (num == _movements.Count - 1)
		{
			Movement movement2 = _movements[num];
			if (KUtility.GetSize(movement2.Path) == 0)
			{
				_movements.Clear();
				return;
			}
			if (movement2.Path[movement2.Path.Length - 1].Time + 0.5 < at)
			{
				_movements.Clear();
				return;
			}
		}
		if (num > 0)
		{
			_movements.RemoveRange(0, num);
		}
	}

	private bool IsNewMovement(Movement movement)
	{
		double num = ((KUtility.GetSize(_prevProcessed.Path) != 0) ? _prevProcessed.Path.First().Time : 0.0);
		double num2 = ((KUtility.GetSize(movement.Path) != 0) ? movement.Path.First().Time : 0.0);
		if (num == num2)
		{
			return false;
		}
		if ((movement.MotionOption & 0x60) > 0 && _prevProcessed.MotionName == movement.MotionName && _prevProcessed.PlaybackRate == movement.PlaybackRate)
		{
			return false;
		}
		return true;
	}

	private void ProcessMovement(Movement movement, Movement? next, double at)
	{
		Location[] path = movement.Path;
		if (IsNewMovement(movement))
		{
			_prevProcessed = movement;
			if (this.MovementProcessed != null)
			{
				this.MovementProcessed(movement);
			}
		}
		GetLocation(path, at, out var prev, out var next2);
		if (!next2.HasValue && next.HasValue)
		{
			GetLocation(next.Value.Path, at, out var _, out next2);
		}
		ApplyLocation(prev, next2, at);
	}

	private void ApplyLocation(Location? prev, Location? next, double at)
	{
		if (prev.HasValue || next.HasValue)
		{
			Location? location = null;
			if (!prev.HasValue)
			{
				location = next;
			}
			else if (!next.HasValue)
			{
				location = prev;
			}
			if (location.HasValue)
			{
				Location value = location.Value;
				_owner.Floor.Value = value.Floor;
				_owner.TurnToYaw(value.Yaw, bSnap: true);
				Vector3 vector = value.Position.ToClientPosition();
				float num = _owner.ProcessWaterDepth(vector);
				vector.y = (float)(value.Floor * 200) + value.Height + num;
				_owner.CurrentPosition = vector;
			}
			else
			{
				Location value2 = prev.Value;
				Location value3 = next.Value;
				float num2 = (float)(at - value2.Time);
				float num3 = num2 / (float)(value3.Time - value2.Time);
				Vector3 vector2 = Vector3.Lerp(value2.Position.ToClientPosition(), value3.Position.ToClientPosition(), num3);
				float num4 = _owner.ProcessWaterDepth(vector2);
				vector2.y = (float)(value2.Floor * 200) + Mathf.Lerp(value2.Height, value3.Height, num3) + num4;
				_owner.CurrentPosition = vector2;
				_owner.Floor.Value = value2.Floor;
				float num5 = Mathf.DeltaAngle(value2.Yaw, value3.Yaw);
				float yaw = value2.Yaw + num5 * num3;
				_owner.TurnToYaw(yaw, bSnap: true);
			}
		}
	}

	private static void GetLocation(Location[] path, double at, out Location? prev, out Location? next)
	{
		prev = null;
		next = null;
		if (path == null)
		{
			return;
		}
		for (int i = 0; i < path.Length; i++)
		{
			Location value = path[i];
			if (at < value.Time)
			{
				next = value;
				break;
			}
			prev = value;
		}
	}

	public static Location GetLocation(Move msg, double nowTime)
	{
		Location location = default(Location);
		Location location2 = default(Location);
		for (int i = 0; i < KUtility.GetSize(msg.Movements); i++)
		{
			for (int j = 0; j < KUtility.GetSize(msg.Movements[i].Path); j++)
			{
				Location location3 = msg.Movements[i].Path[j];
				if ((i == 0 && j == 0) || location3.Time <= nowTime)
				{
					location = location3;
					location2 = location3;
				}
				else if (location3.Time > nowTime)
				{
					location = location2;
					location2 = location3;
					break;
				}
			}
		}
		float t = 1f;
		if (!(Math.Abs(location.Time - location2.Time) < double.Epsilon))
		{
			double num = nowTime - location.Time;
			double num2 = location2.Time - location.Time;
			t = Mathf.Clamp((float)(num / num2), 0f, 1f);
		}
		Vector2 vector = Vector2.Lerp(location.Position.ToVector2(), location2.Position.ToVector2(), t);
		float yaw = Mathf.LerpAngle(location.Yaw, location2.Yaw, t);
		Location result = default(Location);
		result.Position = new WorldPosition
		{
			x = vector.x,
			y = vector.y
		};
		result.Yaw = yaw;
		result.Floor = location.Floor;
		result.Time = nowTime;
		result.Height = location.Height;
		return result;
	}

	public static string GetLastMotionName(Move msg)
	{
		int num = -1;
		for (int i = 0; i < KUtility.GetSize(msg.Movements); i++)
		{
			if (KUtility.GetSize(msg.Movements[i].Path) != 0)
			{
				num = i;
			}
		}
		return (num != -1) ? msg.Movements[num].MotionName : "Barehand_Stand";
	}

	public static string GetAppearMotionName(Move msg, double nowTime)
	{
		int num = -1;
		for (int i = 0; i < KUtility.GetSize(msg.Movements); i++)
		{
			if (KUtility.GetSize(msg.Movements[i].Path) != 0 && (i == 0 || nowTime < msg.Movements[i].Path[0].Time))
			{
				num = i;
			}
		}
		return (num != -1) ? msg.Movements[num].MotionName : "Barehand_Stand";
	}
}
