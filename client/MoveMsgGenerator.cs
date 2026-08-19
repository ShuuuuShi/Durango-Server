using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.Utils;
using Messages;
using UnityEngine;

public class MoveMsgGenerator
{
	private float _normalPathDelay;

	private float _battlePathDelay;

	private bool _isBattle;

	private readonly List<Location> _locations = new List<Location>();

	private readonly List<Movement> _movements = new List<Movement>();

	private float _nextSendMoveTime;

	private bool _sendMoveRequired;

	private string _curMotionName;

	private float _curPlaybackRate;

	private byte _curMotionOption;

	private bool _preMove;

	private Location _preLocation;

	private bool _preLocInitialized;

	private static PlayerBehavior Player => PlayerBehavior.LocalPlayer;

	private bool SendMoveRequired
	{
		get
		{
			return _sendMoveRequired;
		}
		set
		{
			if (_sendMoveRequired != value)
			{
				_sendMoveRequired = value;
				if (_sendMoveRequired)
				{
					_nextSendMoveTime = Time.time + ((!_isBattle) ? _normalPathDelay : _battlePathDelay);
				}
			}
		}
	}

	public MoveMsgGenerator()
	{
		GameSystem<OptionSystem>.Instance().AddOnChange("client_normal_path_delay", delegate(double value)
		{
			_normalPathDelay = (float)value;
		});
		GameSystem<OptionSystem>.Instance().AddOnChange("client_battle_path_delay", delegate(double value)
		{
			_battlePathDelay = (float)value;
		});
		_normalPathDelay = (float)OptionSystem.GetDouble("client_normal_path_delay", 0.5);
		_battlePathDelay = (float)OptionSystem.GetDouble("client_battle_path_delay", 0.5);
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += delegate(bool value)
		{
			_isBattle = value;
		};
	}

	public void UpdateCurrentLocation(bool addMove, Vector3 targetPosition, byte floor, float height, float targetYaw, bool movedByUserInput, bool force = false)
	{
		if (addMove != _preMove || addMove || force)
		{
			if (((_preMove && addMove && _locations.Count == 0) || (!_preMove && addMove)) && _preLocInitialized)
			{
				_locations.Add(_preLocation);
			}
			Location item = default(Location);
			item.Position.SetFromClientPosition(targetPosition);
			item.Floor = floor;
			item.Height = height;
			item.Yaw = targetYaw;
			item.Time = ((!GameManager.IsPrologueMode) ? Connections.Frontend.GetPredictedServerTime() : ((double)Time.time));
			_locations.Add(item);
			if (!_preMove && movedByUserInput && !GameManager.IsPrologueMode)
			{
				Connections.Frontend.Send(default(Depart));
			}
			SendMoveRequired = true;
			WriteCurrentMovement(_curMotionName, _curPlaybackRate, _curMotionOption);
		}
		else
		{
			_preLocation.Time = ((!GameManager.IsPrologueMode) ? Connections.Frontend.GetPredictedServerTime() : ((double)Time.time));
		}
		_preMove = addMove;
	}

	public void MotionChanged(string motionName, float playBackRate, byte motionOption, bool addMove)
	{
		WriteCurrentMovement(_curMotionName, _curPlaybackRate, _curMotionOption);
		_curMotionName = motionName;
		_curPlaybackRate = playBackRate;
		_curMotionOption = motionOption;
		WriteCurrentMovement(_curMotionName, _curPlaybackRate, _curMotionOption);
		SendMoveRequired = true;
	}

	private void WriteCurrentMovement(string motionName, float playBackRate, byte motionOption)
	{
		if (_locations.Count != 0)
		{
			Movement movement = default(Movement);
			movement.MotionName = motionName;
			movement.PlaybackRate = playBackRate;
			movement.RotSpeed = Singleton<PlayerController>.Instance().RotateSpeed;
			movement.MotionOption = motionOption;
			movement.Path = _locations.ToArray();
			_preLocation = _locations[_locations.Count - 1];
			_preLocInitialized = true;
			_locations.Clear();
			Player.PathMovable.HandleMovement(movement);
			_movements.Add(movement);
		}
	}

	public void TrySendMoveMessage()
	{
		if (GameManager.IsPrologueMode)
		{
			_movements.Clear();
		}
		else if (SendMoveRequired && !(Time.time < _nextSendMoveTime))
		{
			SendMoveMessage();
		}
	}

	private void SendMoveMessage()
	{
		if (_movements.Count != 0)
		{
			Move msg = default(Move);
			msg.EntityId = GameManager.PlayerId;
			msg.Movements = CompactMovement().ToArray();
			_movements.Clear();
			Connections.Frontend.Send(msg);
			SendMoveRequired = false;
		}
	}

	private bool IsSimilarMovement(Movement m1, Movement m2)
	{
		if (m1.MotionName == m2.MotionName && m1.MotionOption == m2.MotionOption && Mathf.Approximately(m1.PlaybackRate, m2.PlaybackRate) && Mathf.Approximately(m1.RotSpeed, m2.RotSpeed))
		{
			return true;
		}
		return false;
	}

	private bool IsSimilarPathArguments(Vector2 posVelocity1, Vector2 posVelocity2, float yawVelocity1, float yawVelocity2, float heightVelocity1, float heightVelocity2)
	{
		float f = yawVelocity2 - yawVelocity1;
		if (Mathf.Abs(f) > 1f)
		{
			return false;
		}
		float f2 = heightVelocity2 - heightVelocity1;
		if (Mathf.Abs(f2) > 1f)
		{
			return false;
		}
		Vector2 vector = posVelocity2 - posVelocity1;
		if (Mathf.Abs(vector.x) > 10f || Mathf.Abs(vector.y) > 10f)
		{
			return false;
		}
		return true;
	}

	private void GetLocationPathArguments(Location l1, Location l2, out Vector2 deltaPos, out float deltaYaw, out float deltaHeight)
	{
		float num = (float)(l2.Time - l1.Time);
		deltaPos = (l2.Position.ToVector2() - l1.Position.ToVector2()) / num;
		deltaYaw = (l2.Yaw - l1.Yaw) / num;
		deltaHeight = (l2.Height - l1.Height) / num;
	}

	private IEnumerable<Location> CompactPath(IEnumerable<Location> path)
	{
		Location? baseLoc = null;
		Location? prev = null;
		Vector2 posVelocity = default(Vector2);
		float yawVelocity = 0f;
		float heightVelocity = 0f;
		foreach (Location loc in path)
		{
			if (!baseLoc.HasValue)
			{
				baseLoc = loc;
				prev = null;
				yield return loc;
				continue;
			}
			Location bl = baseLoc.Value;
			if (bl.Floor != loc.Floor)
			{
				if (prev.HasValue)
				{
					yield return prev.Value;
				}
				baseLoc = loc;
				prev = null;
				yield return loc;
			}
			GetLocationPathArguments(bl, loc, out var pv, out var yv, out var hv);
			if (!prev.HasValue)
			{
				posVelocity = pv;
				yawVelocity = yv;
				heightVelocity = hv;
				prev = loc;
			}
			else if (IsSimilarPathArguments(posVelocity, pv, yawVelocity, yv, heightVelocity, hv))
			{
				prev = loc;
			}
			else
			{
				baseLoc = loc;
				prev = null;
				yield return loc;
			}
		}
		if (prev.HasValue)
		{
			yield return prev.Value;
		}
	}

	private IEnumerable<Movement> CompactMovement()
	{
		int prev = -1;
		for (int i = 0; i <= _movements.Count; i++)
		{
			if (prev == -1)
			{
				prev = i;
			}
			else
			{
				if (i < _movements.Count && IsSimilarMovement(_movements[i], _movements[prev]))
				{
					continue;
				}
				IEnumerable<Location> path = null;
				for (int j = prev; j < i; j++)
				{
					Location[] path2 = _movements[j].Path;
					if (path2 != null)
					{
						path = ((path != null) ? path.Concat(path2) : path2);
					}
				}
				Movement movement = _movements[prev];
				movement.Path = CompactPath(path).ToArray();
				yield return movement;
				prev = i;
			}
		}
	}
}
