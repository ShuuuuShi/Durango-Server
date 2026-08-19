using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Durango.Network;
using Durango.Utils;
using Messages;
using UnityEngine;

public class MoveMsgGenerator
{
	[CompilerGenerated]
	private sealed class _003CCompactMovement_003Ed__28 : IEnumerable<Movement>, IEnumerable, IEnumerator<Movement>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private Movement _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public MoveMsgGenerator _003C_003E4__this;

		private int _003Ci_003E5__2;

		Movement IEnumerator<Movement>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCompactMovement_003Ed__28(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			MoveMsgGenerator moveMsgGenerator = _003C_003E4__this;
			int num2;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				num2 = _003Ci_003E5__2;
				goto IL_00fd;
			}
			_003C_003E1__state = -1;
			num2 = -1;
			_003Ci_003E5__2 = 0;
			goto IL_010f;
			IL_00fd:
			_003Ci_003E5__2++;
			goto IL_010f;
			IL_010f:
			if (_003Ci_003E5__2 <= moveMsgGenerator._movements.Count)
			{
				if (num2 == -1)
				{
					num2 = _003Ci_003E5__2;
				}
				else if (_003Ci_003E5__2 >= moveMsgGenerator._movements.Count || !moveMsgGenerator.IsSimilarMovement(moveMsgGenerator._movements[_003Ci_003E5__2], moveMsgGenerator._movements[num2]))
				{
					IEnumerable<Location> enumerable = null;
					for (int i = num2; i < _003Ci_003E5__2; i++)
					{
						Location[] path = moveMsgGenerator._movements[i].Path;
						if (path != null)
						{
							IEnumerable<Location> enumerable3;
							if (enumerable == null)
							{
								IEnumerable<Location> enumerable2 = path;
								enumerable3 = enumerable2;
							}
							else
							{
								enumerable3 = enumerable.Concat(path);
							}
							enumerable = enumerable3;
						}
					}
					Movement movement = moveMsgGenerator._movements[num2];
					movement.Path = moveMsgGenerator.CompactPath(enumerable).ToArray();
					_003C_003E2__current = movement;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_00fd;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Movement> IEnumerable<Movement>.GetEnumerator()
		{
			_003CCompactMovement_003Ed__28 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CCompactMovement_003Ed__28(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Movement>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCompactPath_003Ed__27 : IEnumerable<Location>, IEnumerable, IEnumerator<Location>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private Location _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<Location> path;

		public IEnumerable<Location> _003C_003E3__path;

		public MoveMsgGenerator _003C_003E4__this;

		private Location? _003CbaseLoc_003E5__2;

		private Location? _003Cprev_003E5__3;

		private Vector2 _003CposVelocity_003E5__4;

		private float _003CyawVelocity_003E5__5;

		private float _003CheightVelocity_003E5__6;

		private IEnumerator<Location> _003C_003E7__wrap6;

		private Location _003Cloc_003E5__8;

		private Location _003Cbl_003E5__9;

		Location IEnumerator<Location>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCompactPath_003Ed__27(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || (uint)(num - 1) <= 3u)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap6 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				MoveMsgGenerator moveMsgGenerator = _003C_003E4__this;
				Vector2 deltaPos;
				float deltaYaw;
				float deltaHeight;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003CbaseLoc_003E5__2 = null;
					_003Cprev_003E5__3 = null;
					_003CposVelocity_003E5__4 = default(Vector2);
					_003CyawVelocity_003E5__5 = 0f;
					_003CheightVelocity_003E5__6 = 0f;
					_003C_003E7__wrap6 = path.GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_024d;
				case 1:
					_003C_003E1__state = -3;
					goto IL_024d;
				case 2:
					_003C_003E1__state = -3;
					goto IL_0151;
				case 3:
					_003C_003E1__state = -3;
					goto IL_0190;
				case 4:
					_003C_003E1__state = -3;
					goto IL_024d;
				case 5:
					{
						_003C_003E1__state = -1;
						break;
					}
					IL_024d:
					if (_003C_003E7__wrap6.MoveNext())
					{
						_003Cloc_003E5__8 = _003C_003E7__wrap6.Current;
						if (!_003CbaseLoc_003E5__2.HasValue)
						{
							_003CbaseLoc_003E5__2 = _003Cloc_003E5__8;
							_003Cprev_003E5__3 = null;
							_003C_003E2__current = _003Cloc_003E5__8;
							_003C_003E1__state = 1;
							return true;
						}
						_003Cbl_003E5__9 = _003CbaseLoc_003E5__2.Value;
						if (_003Cbl_003E5__9.Floor != _003Cloc_003E5__8.Floor)
						{
							if (_003Cprev_003E5__3.HasValue)
							{
								_003C_003E2__current = _003Cprev_003E5__3.Value;
								_003C_003E1__state = 2;
								return true;
							}
							goto IL_0151;
						}
						goto IL_0190;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap6 = null;
					if (_003Cprev_003E5__3.HasValue)
					{
						_003C_003E2__current = _003Cprev_003E5__3.Value;
						_003C_003E1__state = 5;
						return true;
					}
					break;
					IL_0190:
					moveMsgGenerator.GetLocationPathArguments(_003Cbl_003E5__9, _003Cloc_003E5__8, out deltaPos, out deltaYaw, out deltaHeight);
					if (!_003Cprev_003E5__3.HasValue)
					{
						_003CposVelocity_003E5__4 = deltaPos;
						_003CyawVelocity_003E5__5 = deltaYaw;
						_003CheightVelocity_003E5__6 = deltaHeight;
						_003Cprev_003E5__3 = _003Cloc_003E5__8;
					}
					else
					{
						if (!moveMsgGenerator.IsSimilarPathArguments(_003CposVelocity_003E5__4, deltaPos, _003CyawVelocity_003E5__5, deltaYaw, _003CheightVelocity_003E5__6, deltaHeight))
						{
							_003CbaseLoc_003E5__2 = _003Cloc_003E5__8;
							_003Cprev_003E5__3 = null;
							_003C_003E2__current = _003Cloc_003E5__8;
							_003C_003E1__state = 4;
							return true;
						}
						_003Cprev_003E5__3 = _003Cloc_003E5__8;
					}
					goto IL_024d;
					IL_0151:
					_003CbaseLoc_003E5__2 = _003Cloc_003E5__8;
					_003Cprev_003E5__3 = null;
					_003C_003E2__current = _003Cloc_003E5__8;
					_003C_003E1__state = 3;
					return true;
				}
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap6 != null)
			{
				_003C_003E7__wrap6.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Location> IEnumerable<Location>.GetEnumerator()
		{
			_003CCompactPath_003Ed__27 _003CCompactPath_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CCompactPath_003Ed__ = this;
			}
			else
			{
				_003CCompactPath_003Ed__ = new _003CCompactPath_003Ed__27(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CCompactPath_003Ed__.path = _003C_003E3__path;
			return _003CCompactPath_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Location>)this).GetEnumerator();
		}
	}

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
		if (Mathf.Abs(yawVelocity2 - yawVelocity1) > 1f)
		{
			return false;
		}
		if (Mathf.Abs(heightVelocity2 - heightVelocity1) > 1f)
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCompactPath_003Ed__27(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__path = path
		};
	}

	private IEnumerable<Movement> CompactMovement()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCompactMovement_003Ed__28(-2)
		{
			_003C_003E4__this = this
		};
	}
}
