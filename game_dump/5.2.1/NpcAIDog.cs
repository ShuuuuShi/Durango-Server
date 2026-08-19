using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using Algorithms;
using Durango.Logic.Map;
using Durango.Model;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using Shared.Battle;
using UnityEngine;

public class NpcAIDog : StateBasedAI<NpcAIDog.State>
{
	public enum State
	{
		Invalid = -1,
		FirstIntroStates = 0,
		PrepareIntroToMMO = 0,
		IntroToMMO = 1,
		AfterCure = 2,
		IntroduceDog = 3,
		LastIntroStates = 3,
		Normal = 4,
		Chase = 5,
		MoveToPOI = 6,
		Aggress = 7,
		Bark = 8,
		Happy = 9,
		Idle = 10,
		Farewell = 11,
		Count = 12
	}

	public class StateCandidate : WeightedCandidate
	{
		public State NextState;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass105_0
	{
		public Vector3 destPos;

		internal Vector3 _003CHappyDoing_003Eb__0()
		{
			return destPos;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass106_0
	{
		public Vector3 destPos;

		internal Vector3 _003CIdleDoing_003Eb__0()
		{
			return destPos;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass107_0
	{
		public Vector3 goodByePos;

		public NpcAIDog _003C_003E4__this;

		internal Vector3 _003CFarewellDoing_003Eb__0()
		{
			return goodByePos;
		}

		internal Vector3 _003CFarewellDoing_003Eb__1()
		{
			return _003C_003E4__this.POIClientPosition;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass107_1
	{
		public Vector3 destPos;

		internal Vector3 _003CFarewellDoing_003Eb__2()
		{
			return destPos;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass107_2
	{
		public Vector3 lastGoodByePos;

		internal Vector3 _003CFarewellDoing_003Eb__3()
		{
			return lastGoodByePos;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass85_0
	{
		public Vector3 destPos;

		internal Vector3 _003CAfterCureDoing_003Eb__0()
		{
			return destPos;
		}
	}

	[CompilerGenerated]
	private sealed class _003CAfterCureDoing_003Ed__85 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		private _003C_003Ec__DisplayClass85_0 _003C_003E8__1;

		object IEnumerator<object>.Current
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
		public _003CAfterCureDoing_003Ed__85(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E8__1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E8__1 = new _003C_003Ec__DisplayClass85_0();
				npcAIDog.TargetAnimal.PlayAndFitLocation(npcAIDog._introSitEndMotion, loop: false);
				_003C_003E2__current = new WaitForSeconds(npcAIDog.TargetAnimal.CurAnimState.length - 0.15f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				npcAIDog.CrossFadeAndFitLocation(npcAIDog._standMotion, 0.1f);
				_003C_003E2__current = new WaitForSeconds(2f);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E8__1.destPos = npcAIDog.transform.position + npcAIDog._locationOffsetAfterCure;
				_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoMoveTo(() => _003C_003E8__1.destPos, null, npcAIDog._walkMotion, npcAIDog._walkSpeed, endAtReached: true, 0.2f));
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoTurnAndCrossFadeMotion(npcAIDog._standMotion, 0.2f));
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				break;
			case 5:
				_003C_003E1__state = -1;
				break;
			}
			if (!npcAIDog.IsInterrupted)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 5;
				return true;
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
	}

	[CompilerGenerated]
	private sealed class _003CAggressDoing_003Ed__101 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		object IEnumerator<object>.Current
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
		public _003CAggressDoing_003Ed__101(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog CS_0024_003C_003E8__locals0 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = CS_0024_003C_003E8__locals0.StartCoroutine(CS_0024_003C_003E8__locals0.CoMoveTo(() => CS_0024_003C_003E8__locals0.MasterPos, CS_0024_003C_003E8__locals0.AggressTransitions, CS_0024_003C_003E8__locals0._runMotion, CS_0024_003C_003E8__locals0._runSpeed));
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
			}
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
	}

	[CompilerGenerated]
	private sealed class _003CBarkDoing_003Ed__103 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		private float _003Cduration_003E5__2;

		private float _003CendTime_003E5__3;

		object IEnumerator<object>.Current
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
		public _003CBarkDoing_003Ed__103(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				float ang = Maths.CalcYawWithTarget(npcAIDog.MasterPos, npcAIDog.transform.position);
				_003Cduration_003E5__2 = UnityEngine.Random.Range(npcAIDog._barkDurationMin, npcAIDog._barkDurationMax);
				if (Maths.DistanceAngDeg(ang, npcAIDog.TargetAnimal.CurrentYaw) > npcAIDog._turnMotionAcivateAngle)
				{
					_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoTurnAndCrossFadeMotion(npcAIDog._barkMotion, 0.1f));
					_003C_003E1__state = 1;
					return true;
				}
				npcAIDog.CrossFadeAndFitLocation(npcAIDog._barkMotion, 0.1f);
				goto IL_00c6;
			}
			case 1:
				_003C_003E1__state = -1;
				goto IL_00c6;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_00c6:
				_003CendTime_003E5__3 = Time.time + _003Cduration_003E5__2;
				break;
			}
			if (Time.time < _003CendTime_003E5__3)
			{
				if (npcAIDog.IsMasterMoreCloseToPOI)
				{
					npcAIDog.CurState = State.MoveToPOI;
					return false;
				}
				if (npcAIDog.IsInterrupted)
				{
					return false;
				}
				float ang = Maths.CalcYawWithTarget(npcAIDog.MasterPos, npcAIDog.transform.position);
				npcAIDog.TargetAnimal.TurnToYaw(ang, bSnap: false);
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
			npcAIDog.CurState = State.Normal;
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
	}

	[CompilerGenerated]
	private sealed class _003CBarkToPlayer_003Ed__108 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		public float duration;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
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
		public _003CBarkToPlayer_003Ed__108(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				float ang = Maths.CalcYawWithTarget(npcAIDog.MasterPos, npcAIDog.transform.position);
				if (Maths.DistanceAngDeg(ang, npcAIDog.TargetAnimal.CurrentYaw) > npcAIDog._turnMotionAcivateAngle)
				{
					_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoTurnAndCrossFadeMotion(npcAIDog._barkMotion, 0.1f));
					_003C_003E1__state = 1;
					return true;
				}
				npcAIDog.CrossFadeAndFitLocation(npcAIDog._barkMotion, 0.1f);
				goto IL_00af;
			}
			case 1:
				_003C_003E1__state = -1;
				goto IL_00af;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_00af:
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			}
			if (Time.time < _003CendTime_003E5__2)
			{
				float ang = Maths.CalcYawWithTarget(npcAIDog.MasterPos, npcAIDog.transform.position);
				npcAIDog.TargetAnimal.TurnToYaw(ang, bSnap: false);
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
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
	}

	[CompilerGenerated]
	private sealed class _003CChaseDoing_003Ed__95 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		object IEnumerator<object>.Current
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
		public _003CChaseDoing_003Ed__95(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog CS_0024_003C_003E8__locals0 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = CS_0024_003C_003E8__locals0.StartCoroutine(CS_0024_003C_003E8__locals0.CoMoveTo(() => CS_0024_003C_003E8__locals0.MasterPos, CS_0024_003C_003E8__locals0.ChaseTransitions, CS_0024_003C_003E8__locals0._runMotion, CS_0024_003C_003E8__locals0._runSpeed));
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
			}
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
	}

	[CompilerGenerated]
	private sealed class _003CCoMoveTo_003Ed__109 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Func<Vector3> funcTargetPos;

		public NpcAIDog _003C_003E4__this;

		public string moveMotion;

		public float fadeInTime;

		public bool endAtReached;

		public float moveSpeed;

		public Func<bool> funcTransition;

		private bool _003CisMoving_003E5__2;

		private float _003CprevTime_003E5__3;

		object IEnumerator<object>.Current
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
		public _003CCoMoveTo_003Ed__109(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			float ang;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CisMoving_003E5__2 = false;
				_003CprevTime_003E5__3 = Time.time;
				goto IL_01c8;
			case 1:
				_003C_003E1__state = -1;
				_003CisMoving_003E5__2 = true;
				_003CprevTime_003E5__3 = Time.time;
				goto IL_00f6;
			case 2:
				{
					_003C_003E1__state = -1;
					goto IL_01c8;
				}
				IL_01c8:
				if (null == npcAIDog.Master || (funcTransition != null && funcTransition()))
				{
					break;
				}
				ang = Maths.CalcYawWithTarget(funcTargetPos(), npcAIDog.transform.position);
				if (!_003CisMoving_003E5__2 && Maths.DistanceAngDeg(ang, npcAIDog.TargetAnimal.CurrentYaw) > npcAIDog._turnMotionAcivateAngle)
				{
					_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoTurnAndCrossFadeMotion(moveMotion, fadeInTime));
					_003C_003E1__state = 1;
					return true;
				}
				if (!_003CisMoving_003E5__2)
				{
					npcAIDog.CrossFadeAndFitLocation(moveMotion, fadeInTime);
					_003CisMoving_003E5__2 = true;
				}
				goto IL_00f6;
				IL_00f6:
				if (!npcAIDog.IsInterrupted)
				{
					float num2 = Time.time - _003CprevTime_003E5__3;
					_003CprevTime_003E5__3 = Time.time;
					Vector3 vector = Maths.Make2D(funcTargetPos() - npcAIDog.transform.position);
					if (!endAtReached || !(vector.magnitude < 100f))
					{
						float yaw = Maths.CalcYawWithTarget(funcTargetPos(), npcAIDog.transform.position);
						npcAIDog.TargetAnimal.TurnToYaw(yaw, bSnap: false);
						Vector3 vector2 = vector.normalized * moveSpeed;
						npcAIDog.TargetAnimal.CurrentPosition += vector2 * num2;
						_003C_003E2__current = null;
						_003C_003E1__state = 2;
						return true;
					}
				}
				break;
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
	}

	[CompilerGenerated]
	private sealed class _003CCoMoveToWithPathFind_003Ed__110 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		public Func<Vector3> funcTargetPos;

		public Func<bool, bool> funcCheckWalk;

		public string runMotion;

		public string walkMotion;

		public float runSpeed;

		public float walkSpeed;

		public Func<bool> funcTransition;

		private float _003CprevTime_003E5__2;

		private List<Vector3> _003Cpaths_003E5__3;

		private bool _003CisMoving_003E5__4;

		private bool _003CwasLastMoveWalk_003E5__5;

		private Vector3 _003CdestPos_003E5__6;

		private float _003CmoveSpeed_003E5__7;

		object IEnumerator<object>.Current
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
		public _003CCoMoveToWithPathFind_003Ed__110(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cpaths_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			bool flag;
			bool flag2;
			string text;
			float ang;
			float num2;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				_003CprevTime_003E5__2 = Time.time;
				Vector2 vector = Util.ClientPositionToTilePosition(npcAIDog.transform.position);
				Vector2 vector2 = Util.ClientPositionToTilePosition(funcTargetPos());
				List<PathFinderNode> list = npcAIDog._pathFinder.FindPath(new Point((int)vector.x, (int)vector.y), new Point((int)vector2.x, (int)vector2.y));
				if (list == null)
				{
					npcAIDog.CurState = State.Chase;
					return false;
				}
				_003Cpaths_003E5__3 = new List<Vector3>();
				foreach (PathFinderNode item2 in list)
				{
					Vector3 item = Util.TilePositionToClientPosition(new Vector2(item2.X, item2.Y));
					_003Cpaths_003E5__3.Add(item);
				}
				_003Cpaths_003E5__3.Reverse();
				_003Cpaths_003E5__3.RemoveAt(0);
				_003Cpaths_003E5__3.Add(funcTargetPos());
				_003CisMoving_003E5__4 = false;
				_003CwasLastMoveWalk_003E5__5 = false;
				_003CdestPos_003E5__6 = _003Cpaths_003E5__3[0];
				_003Cpaths_003E5__3.RemoveAt(0);
				goto IL_0365;
			}
			case 1:
				_003C_003E1__state = -1;
				_003CisMoving_003E5__4 = true;
				_003CprevTime_003E5__2 = Time.time;
				goto IL_0262;
			case 2:
				{
					_003C_003E1__state = -1;
					goto IL_0365;
				}
				IL_0365:
				if (null == npcAIDog.Master)
				{
					break;
				}
				flag = funcCheckWalk?.Invoke(_003CwasLastMoveWalk_003E5__5) ?? false;
				flag2 = _003CwasLastMoveWalk_003E5__5 != flag;
				_003CwasLastMoveWalk_003E5__5 = flag;
				text = ((!flag) ? runMotion : walkMotion);
				_003CmoveSpeed_003E5__7 = ((!flag) ? runSpeed : walkSpeed);
				ang = Maths.CalcYawWithTarget(_003CdestPos_003E5__6, npcAIDog.transform.position);
				if (!_003CisMoving_003E5__4 && Maths.DistanceAngDeg(ang, npcAIDog.TargetAnimal.CurrentYaw) > npcAIDog._turnMotionAcivateAngle)
				{
					_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoTurnAndCrossFadeMotion(text, 0.1f));
					_003C_003E1__state = 1;
					return true;
				}
				if (!_003CisMoving_003E5__4 || flag2)
				{
					npcAIDog.CrossFadeAndFitLocation(text, 0.1f);
					_003CisMoving_003E5__4 = true;
				}
				goto IL_0262;
				IL_0262:
				if (npcAIDog.IsInterrupted)
				{
					break;
				}
				num2 = Time.time - _003CprevTime_003E5__2;
				_003CprevTime_003E5__2 = Time.time;
				if (funcTransition == null || !funcTransition())
				{
					float yaw = Maths.CalcYawWithTarget(_003CdestPos_003E5__6, npcAIDog.transform.position);
					npcAIDog.TargetAnimal.TurnToYaw(yaw, bSnap: false);
					Vector3 vector3 = Maths.Make2D(_003CdestPos_003E5__6 - npcAIDog.transform.position);
					if (vector3.magnitude < 200f && _003Cpaths_003E5__3.Count > 0)
					{
						_003CdestPos_003E5__6 = _003Cpaths_003E5__3[0];
						_003Cpaths_003E5__3.RemoveAt(0);
					}
					Vector3 vector4 = vector3.normalized * _003CmoveSpeed_003E5__7;
					npcAIDog.TargetAnimal.CurrentPosition += vector4 * num2;
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				break;
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
	}

	[CompilerGenerated]
	private sealed class _003CCoTurnAndCrossFadeMotion_003Ed__111 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		public float fadeTime;

		public string afterTurnMotionName;

		public bool loop;

		public float beginTime;

		public float playbackRate;

		object IEnumerator<object>.Current
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
		public _003CCoTurnAndCrossFadeMotion_003Ed__111(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				npcAIDog.TargetAnimal.CrossFade(npcAIDog._turnMotion, fadeTime, loop: false);
				_003C_003E2__current = new WaitForSeconds(npcAIDog.TargetAnimal.CurAnimState.length);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				npcAIDog.FixUpRootBoneAndCrossFadeMotion(afterTurnMotionName, fadeTime, loop, beginTime, playbackRate);
				return false;
			}
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
	}

	[CompilerGenerated]
	private sealed class _003CFarewellDoing_003Ed__107 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		private _003C_003Ec__DisplayClass107_1 _003C_003E8__1;

		private _003C_003Ec__DisplayClass107_2 _003C_003E8__2;

		private _003C_003Ec__DisplayClass107_0 _003C_003E8__3;

		private int _003Cj_003E5__2;

		object IEnumerator<object>.Current
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
		public _003CFarewellDoing_003Ed__107(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E8__1 = null;
			_003C_003E8__2 = null;
			_003C_003E8__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E8__3 = new _003C_003Ec__DisplayClass107_0();
				_003C_003E8__3._003C_003E4__this = _003C_003E4__this;
				_003C_003E8__3.goodByePos = npcAIDog.MasterPos + (npcAIDog.transform.position - npcAIDog.MasterPos).normalized * 200f;
				_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoMoveTo(() => _003C_003E8__3.goodByePos, null, npcAIDog._runMotion, npcAIDog._runSpeed, endAtReached: true));
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (npcAIDog._beacon != null)
				{
					npcAIDog._beacon.SetActive(value: false);
				}
				SoundManager.PlayEvent(npcAIDog._farewellSound, SoundPosition.Chase(npcAIDog.transform.gameObject));
				_003Cj_003E5__2 = 0;
				goto IL_01ca;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E8__1.destPos = npcAIDog.GetRandomMasterSurroundingPos(300f);
				_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoMoveTo(() => _003C_003E8__1.destPos, null, npcAIDog._walkMotion, npcAIDog._walkSpeed, endAtReached: true));
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E8__1 = null;
				_003Cj_003E5__2++;
				goto IL_01ca;
			case 4:
				_003C_003E1__state = -1;
				_003C_003E8__2.lastGoodByePos = npcAIDog.transform.position + (npcAIDog.POIClientPosition - npcAIDog.transform.position).normalized * 250f;
				_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoMoveTo(() => _003C_003E8__2.lastGoodByePos, null, npcAIDog._walkMotion, npcAIDog._walkSpeed, endAtReached: true));
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				_003C_003E8__2 = null;
				_003Cj_003E5__2++;
				goto IL_02d6;
			case 6:
				_003C_003E1__state = -1;
				_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoMoveTo(() => _003C_003E8__3._003C_003E4__this.POIClientPosition, null, npcAIDog._runMotion, npcAIDog._runSpeed, endAtReached: true));
				_003C_003E1__state = 7;
				return true;
			case 7:
				{
					_003C_003E1__state = -1;
					UnityEngine.Object.Destroy(npcAIDog.gameObject);
					return false;
				}
				IL_02d6:
				if (_003Cj_003E5__2 < 2)
				{
					_003C_003E8__2 = new _003C_003Ec__DisplayClass107_2();
					_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.BarkToPlayer(1.5f));
					_003C_003E1__state = 4;
					return true;
				}
				_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.BarkToPlayer(5f));
				_003C_003E1__state = 6;
				return true;
				IL_01ca:
				if (_003Cj_003E5__2 < 2)
				{
					_003C_003E8__1 = new _003C_003Ec__DisplayClass107_1();
					_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.BarkToPlayer(1.5f));
					_003C_003E1__state = 2;
					return true;
				}
				SoundManager.PlayEvent(npcAIDog._farewellSound, SoundPosition.Chase(npcAIDog.transform.gameObject));
				_003Cj_003E5__2 = 0;
				goto IL_02d6;
			}
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
	}

	[CompilerGenerated]
	private sealed class _003CHappyDoing_003Ed__105 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		object IEnumerator<object>.Current
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
		public _003CHappyDoing_003Ed__105(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (npcAIDog.DistanceToMaster > npcAIDog._reactDistance)
				{
					_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoMoveTo(new _003C_003Ec__DisplayClass105_0
					{
						destPos = npcAIDog.CalcMasterNearestPos(npcAIDog._reactDistance)
					}._003CHappyDoing_003Eb__0, null, npcAIDog._runMotion, npcAIDog._runSpeed, endAtReached: true));
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0090;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0090;
			case 2:
				{
					_003C_003E1__state = -1;
					npcAIDog.CurState = State.Normal;
					return false;
				}
				IL_0090:
				npcAIDog.TargetAnimal.CrossFade(npcAIDog._happyMotion, 0.1f, loop: false);
				_003C_003E2__current = new WaitForSeconds(npcAIDog.TargetAnimal.CurAnimState.length);
				_003C_003E1__state = 2;
				return true;
			}
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
	}

	[CompilerGenerated]
	private sealed class _003CIdleDoing_003Ed__106 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		object IEnumerator<object>.Current
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
		public _003CIdleDoing_003Ed__106(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (npcAIDog.DistanceToMaster > npcAIDog._reactDistance)
				{
					_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoMoveTo(new _003C_003Ec__DisplayClass106_0
					{
						destPos = npcAIDog.CalcMasterNearestPos(npcAIDog._reactDistance)
					}._003CIdleDoing_003Eb__0, null, npcAIDog._runMotion, npcAIDog._runSpeed, endAtReached: true));
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0090;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0090;
			case 2:
				{
					_003C_003E1__state = -1;
					npcAIDog.CurState = State.Normal;
					return false;
				}
				IL_0090:
				npcAIDog.TargetAnimal.CrossFade(npcAIDog._idleMotion, 0.1f, loop: false);
				_003C_003E2__current = new WaitForSeconds(npcAIDog.TargetAnimal.CurAnimState.length);
				_003C_003E1__state = 2;
				return true;
			}
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
	}

	[CompilerGenerated]
	private sealed class _003CIntroToMMODoing_003Ed__83 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		object IEnumerator<object>.Current
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
		public _003CIntroToMMODoing_003Ed__83(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				npcAIDog.TargetAnimal.Play(npcAIDog._introMotion, loop: false);
				_003C_003E2__current = new WaitForSeconds(npcAIDog.TargetAnimal.CurAnimState.length);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				npcAIDog.TargetAnimal.PlayAndFitLocation(npcAIDog._introSitMotion);
				_003C_003E2__current = new WaitForSeconds(npcAIDog._introSitDuringTime);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				npcAIDog.CurState = State.AfterCure;
				return false;
			}
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
	}

	[CompilerGenerated]
	private sealed class _003CIntroduceDogDoing_003Ed__87 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		object IEnumerator<object>.Current
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
		public _003CIntroduceDogDoing_003Ed__87(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			if (num != 0)
			{
				return false;
			}
			_003C_003E1__state = -1;
			npcAIDog.CurState = State.Normal;
			if (npcAIDog._beacon != null)
			{
				npcAIDog._beacon.SetActive(value: true);
			}
			npcAIDog.AddToMapIndicator();
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
	}

	[CompilerGenerated]
	private sealed class _003CMoveToPOIDoing_003Ed__98 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		object IEnumerator<object>.Current
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
		public _003CMoveToPOIDoing_003Ed__98(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoMoveToWithPathFind(npcAIDog.MoveToPOIDestPos, npcAIDog.MoveToPOITransitions, npcAIDog.CheckWalk, npcAIDog._runMotion, npcAIDog._runSpeed, npcAIDog._walkMotion, npcAIDog._walkSpeed));
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
			}
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
	}

	[CompilerGenerated]
	private sealed class _003CNormalDoing_003Ed__89 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		private float _003CnewYaw_003E5__2;

		object IEnumerator<object>.Current
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
		public _003CNormalDoing_003Ed__89(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (null == npcAIDog.Master)
				{
					return false;
				}
				if (npcAIDog.NeedToChaseMaster())
				{
					npcAIDog.CurState = State.Chase;
				}
				else if (npcAIDog.NeedToTransitionMoveToPOI())
				{
					npcAIDog.CurState = State.MoveToPOI;
				}
				else
				{
					if (_randomStateCandidatesAtNormal == null)
					{
						_randomStateCandidatesAtNormal = new StateCandidate[4]
						{
							new StateCandidate
							{
								Weight = npcAIDog._barkAtNormalProbability,
								NextState = State.Bark
							},
							new StateCandidate
							{
								Weight = npcAIDog._chaseAtNormalProbability,
								NextState = State.Chase
							},
							new StateCandidate
							{
								Weight = npcAIDog._idleAtNormalProbability,
								NextState = State.Idle
							},
							new StateCandidate
							{
								Weight = npcAIDog._standAtNormalProbability,
								NextState = State.Normal
							}
						};
					}
					StateCandidate stateCandidate = WeightedCandidate.Select(_randomStateCandidatesAtNormal);
					if (stateCandidate == null)
					{
						return false;
					}
					if (stateCandidate.NextState == State.Normal)
					{
						npcAIDog.TargetAnimal.CrossFade(npcAIDog._standMotion, 0.1f);
						_003CnewYaw_003E5__2 = Maths.CalcYawWithTarget(npcAIDog.MasterPos, npcAIDog.transform.position);
						if (Maths.DistanceAngDeg(_003CnewYaw_003E5__2, npcAIDog.TargetAnimal.CurrentYaw) > npcAIDog._turnMotionAcivateAngle)
						{
							_003C_003E2__current = npcAIDog.StartCoroutine(npcAIDog.CoTurnAndCrossFadeMotion(npcAIDog._standMotion, 0.1f));
							_003C_003E1__state = 1;
							return true;
						}
						npcAIDog.CrossFadeAndFitLocation(npcAIDog._standMotion, 0.1f);
						goto IL_01ae;
					}
					npcAIDog.CurState = stateCandidate.NextState;
				}
				goto IL_01ce;
			case 1:
				_003C_003E1__state = -1;
				goto IL_01ae;
			case 2:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_01ae:
				npcAIDog.TargetAnimal.TurnToYaw(_003CnewYaw_003E5__2, bSnap: false);
				goto IL_01ce;
				IL_01ce:
				_003C_003E2__current = WaitForOneSecond;
				_003C_003E1__state = 2;
				return true;
			}
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
	}

	[CompilerGenerated]
	private sealed class _003COnAfterDoingState_003Ed__64 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		object IEnumerator<object>.Current
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
		public _003COnAfterDoingState_003Ed__64(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			if (_003C_003E1__state != 0)
			{
				return false;
			}
			_003C_003E1__state = -1;
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
	}

	[CompilerGenerated]
	private sealed class _003COnBeforeDoingState_003Ed__63 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		object IEnumerator<object>.Current
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
		public _003COnBeforeDoingState_003Ed__63(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				BoneLookAtTarget component = npcAIDog.GetComponent<BoneLookAtTarget>();
				npcAIDog.Master = PlayerBehavior.LocalPlayer.gameObject;
				if (npcAIDog.Master == null)
				{
					_003C_003E2__current = WaitForOneSecond;
					_003C_003E1__state = 1;
					return true;
				}
				component.SetLookTarget(PlayerBehavior.LocalPlayer.gameObject, findHead: true);
				break;
			}
			case 1:
				_003C_003E1__state = -1;
				break;
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
	}

	[CompilerGenerated]
	private sealed class _003COnStart_003Ed__62 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIDog _003C_003E4__this;

		object IEnumerator<object>.Current
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
		public _003COnStart_003Ed__62(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NpcAIDog npcAIDog = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
			}
			else
			{
				_003C_003E1__state = -1;
				npcAIDog.TargetAnimal.EntityId = "666";
				if (npcAIDog.IsInIntroStates())
				{
					goto IL_00e0;
				}
				npcAIDog.CurState = State.Chase;
				npcAIDog.GetComponent<BoneLookAtTarget>().AutoChangeTarget = false;
			}
			if (!TerrainBase.IsPlayerInitialized)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			Vector3 vector = Util.WorldPositionToClientPosition(new Vector3(512f, 512f));
			npcAIDog._initialPos = PlayerBehavior.LocalPlayer.CurrentPosition + (vector - PlayerBehavior.LocalPlayer.CurrentPosition).normalized * npcAIDog._appearDiatanceFromPlayer;
			npcAIDog._initialPos.y = 0f;
			npcAIDog.TargetAnimal.CurrentPosition = npcAIDog._initialPos;
			npcAIDog.AddToMapIndicator();
			goto IL_00e0;
			IL_00e0:
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
	}

	[CompilerGenerated]
	private sealed class _003CPrepareIntroToMMODoing_003Ed__78 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		object IEnumerator<object>.Current
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
		public _003CPrepareIntroToMMODoing_003Ed__78(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
			}
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
	}

	private static readonly WaitForSeconds WaitForOneSecond = new WaitForSeconds(1f);

	private static StateCandidate[] _randomStateCandidatesAtNormal;

	private IPathFinder _pathFinder;

	private bool _isMapIndicatorAdded;

	[SerializeField]
	private string _bikeLickMotion = "Dog_Bike_Lick";

	[SerializeField]
	private Vector3 _introPosFromPlayer = new Vector3(81f, 0f, 65f);

	[SerializeField]
	private float _introYaw = 118f;

	[SerializeField]
	private string _introMotion = "Dog_Bike_Begin";

	[SerializeField]
	private string _introSitMotion = "Wolf_Sit_Looping_A";

	[SerializeField]
	private float _introSitDuringTime = 7f;

	[SerializeField]
	private string _introSitEndMotion = "Dog_Sit_End";

	[SerializeField]
	private Vector3 _locationOffsetAfterCure = new Vector3(-52f, 0f, 300f);

	[SerializeField]
	private string _standMotion = "Dog_Stand";

	[SerializeField]
	private string _runMotion = "Dog_Run";

	[SerializeField]
	private string _walkMotion = "Dog_Walk";

	[SerializeField]
	private string _barkMotion = "Dog_Bark";

	[SerializeField]
	private string _happyMotion = "Dog_Jump";

	[SerializeField]
	private float _reactDistance = 500f;

	[SerializeField]
	private string _turnMotion = "Dog_Turn";

	[SerializeField]
	private string _idleMotion = "Dog_Idle";

	[SerializeField]
	private float _followDistanceMin = 50f;

	[SerializeField]
	private float _followDistanceMax = 700f;

	[SerializeField]
	private float _walkToPOIDistance = 500f;

	[SerializeField]
	private float _runSpeed = 500f;

	[SerializeField]
	private float _walkSpeed = 200f;

	[SerializeField]
	private float _appearDiatanceFromPlayer = 1000f;

	[SerializeField]
	private float _distanceThreshould = 200f;

	[SerializeField]
	private Vector2 _poiTilePos = Vector3.zero;

	[SerializeField]
	private TextAsset _navigationGridAsset;

	[SerializeField]
	private string _beaconName = "Beacon";

	[SerializeField]
	private float _barkDurationMin = 1f;

	[SerializeField]
	private float _barkDurationMax = 3f;

	[SerializeField]
	private float _barkAtNormalProbability = 0.1f;

	[SerializeField]
	private float _chaseAtNormalProbability = 0.3f;

	[SerializeField]
	private float _idleAtNormalProbability = 0.1f;

	[SerializeField]
	private float _standAtNormalProbability = 0.5f;

	[SerializeField]
	private float _barkAfterChaseProbability = 0.3f;

	[SerializeField]
	private float _barkAfterAggressProbability = 0.3f;

	[SerializeField]
	private SoundEventType _farewellSound;

	[SerializeField]
	private float _turnMotionAcivateAngle = 120f;

	private Vector3 _initialPos;

	private AnimalBehavior _targetAnimal;

	private byte[,] _navGrid;

	private GameObject _beacon;

	private bool IsMasterMoreCloseToPOI
	{
		get
		{
			if (base.Master != null)
			{
				return DistanceMasterToPOI < DistanceToPOI;
			}
			return false;
		}
	}

	private float DistanceToPOI => (POIClientPosition - base.transform.position).magnitude;

	private float DistanceMasterToPOI => (POIClientPosition - base.MasterPos).magnitude;

	protected override State InvalidState => State.Invalid;

	protected override int StateEnumCount => 12;

	private AnimalBehavior TargetAnimal
	{
		get
		{
			if (null == _targetAnimal)
			{
				_targetAnimal = GetComponent<AnimalBehavior>();
			}
			return _targetAnimal;
		}
	}

	private Vector3 POIClientPosition => Util.WorldPositionToClientPosition(Util.TilePositionToWorldPosition(_poiTilePos));

	protected override void DefineStates()
	{
		AddState(State.FirstIntroStates, new StateElem
		{
			Entered = PrepareIntroToMMOEntered,
			Doing = PrepareIntroToMMODoing,
			Exited = PrepareIntroToMMOExited
		});
		AddState(State.IntroToMMO, new StateElem
		{
			Entered = IntroToMMOEntered,
			Doing = IntroToMMODoing,
			Exited = IntroToMMOExited
		});
		AddState(State.AfterCure, new StateElem
		{
			Doing = AfterCureDoing
		});
		AddState(State.IntroduceDog, new StateElem
		{
			Doing = IntroduceDogDoing
		});
		AddState(State.Normal, new StateElem
		{
			Entered = NormalEntered,
			Doing = NormalDoing
		});
		AddState(State.Chase, new StateElem
		{
			Doing = ChaseDoing
		});
		AddState(State.MoveToPOI, new StateElem
		{
			Doing = MoveToPOIDoing
		});
		AddState(State.Aggress, new StateElem
		{
			Doing = AggressDoing
		});
		AddState(State.Bark, new StateElem
		{
			Doing = BarkDoing
		});
		AddState(State.Happy, new StateElem
		{
			Doing = HappyDoing
		});
		AddState(State.Idle, new StateElem
		{
			Doing = IdleDoing
		});
		AddState(State.Farewell, new StateElem
		{
			Doing = FarewellDoing
		});
	}

	protected override void OnAwake()
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(_navigationGridAsset.bytes))
		{
			_navGrid = (byte[,])binaryFormatter.Deserialize(serializationStream);
		}
		_pathFinder = new PathFinderFast(_navGrid);
		TargetAnimal.SetActivateRootMotion(active: false);
		_beacon = KUtility.FindObjectByName(base.gameObject, _beaconName);
		SoundManager.PrepareEvent(_farewellSound);
	}

	protected override IEnumerator OnStart()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnStart_003Ed__62(0)
		{
			_003C_003E4__this = this
		};
	}

	protected override IEnumerator OnBeforeDoingState()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnBeforeDoingState_003Ed__63(0)
		{
			_003C_003E4__this = this
		};
	}

	protected override IEnumerator OnAfterDoingState()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnAfterDoingState_003Ed__64(0);
	}

	protected override bool IsAIEnded()
	{
		return false;
	}

	protected override bool IsTerminalState(State state)
	{
		return state == State.Farewell;
	}

	private void OnDisable()
	{
		MapIndicators.Remove(TargetAnimal.EntityId, IndicatorType.GuideDog);
	}

	private bool IsInIntroStates()
	{
		if (State.FirstIntroStates <= base.CurState)
		{
			return base.CurState <= State.IntroduceDog;
		}
		return false;
	}

	public void MoveCloseToPlayer()
	{
		if (!IsInIntroStates())
		{
			base.CurState = State.Chase;
		}
	}

	private void AddToMapIndicator()
	{
		if (!_isMapIndicatorAdded)
		{
			MapIconIndicator orAdd = MapIndicators.GetOrAdd<MapIconIndicator>(TargetAnimal.EntityId, IndicatorType.GuideDog);
			orAdd.SetTarget(TargetAnimal.gameObject);
			orAdd.SetIcon("icon_map_animal", PresetColor.UISkyBlue, 16, 30);
			_isMapIndicatorAdded = true;
		}
	}

	public void SetPOIPosTile(Vector2 tilePos)
	{
		AddToMapIndicator();
		_poiTilePos = tilePos;
		if (!IsInIntroStates())
		{
			base.CurState = ((!IsMasterMoreCloseToPOI) ? State.Chase : State.MoveToPOI);
		}
	}

	public Vector2 GetPOIPosTile()
	{
		return _poiTilePos;
	}

	public void SetPOIPos(Vector3 clientPos)
	{
		SetPOIPosTile(Util.ClientPositionToTilePosition(clientPos));
	}

	public void SetFarewellTile(Vector2 tilePos)
	{
		_poiTilePos = tilePos;
		base.CurState = State.Farewell;
	}

	public void PrepareIntroMMO()
	{
		base.CurState = State.FirstIntroStates;
	}

	private void PrepareIntroToMMOEntered()
	{
		TargetAnimal.Play(_bikeLickMotion);
		RepositionToIntro();
	}

	private void PrepareIntroToMMOExited()
	{
	}

	private IEnumerator PrepareIntroToMMODoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CPrepareIntroToMMODoing_003Ed__78(0);
	}

	[ExposedInEditor("Intro 위치 새로 잡기")]
	public void RepositionToIntro()
	{
		Vector3 position = PlayerBehavior.LocalPlayer.GetBodyPartTransform(BodyPart.Head).gameObject.transform.position + _introPosFromPlayer;
		position.y = 0f;
		base.transform.position = position;
		base.transform.rotation = Quaternion.Euler(0f, _introYaw, 0f);
	}

	public void PlayIntroAnim()
	{
		base.CurState = State.IntroToMMO;
	}

	private void IntroToMMOEntered()
	{
		if (_beacon != null)
		{
			_beacon.SetActive(value: false);
		}
		RepositionToIntro();
	}

	private void IntroToMMOExited()
	{
	}

	private IEnumerator IntroToMMODoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CIntroToMMODoing_003Ed__83(0)
		{
			_003C_003E4__this = this
		};
	}

	public void RestoreStandingKCutScene()
	{
		RepositionToIntro();
		base.CurState = State.AfterCure;
	}

	private IEnumerator AfterCureDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CAfterCureDoing_003Ed__85(0)
		{
			_003C_003E4__this = this
		};
	}

	public void Dog_Introduce()
	{
		base.CurState = State.IntroduceDog;
	}

	private IEnumerator IntroduceDogDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CIntroduceDogDoing_003Ed__87(0)
		{
			_003C_003E4__this = this
		};
	}

	public void NormalEntered()
	{
		TargetAnimal.CrossFade(_standMotion, 0.1f);
	}

	private IEnumerator NormalDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CNormalDoing_003Ed__89(0)
		{
			_003C_003E4__this = this
		};
	}

	private bool NeedToChaseMaster()
	{
		return base.DistanceToMaster > _followDistanceMax + _distanceThreshould;
	}

	private bool NeedToEndChaseMaster()
	{
		return base.DistanceToMaster < _followDistanceMin;
	}

	private bool NeedToTransitionMoveToPOI()
	{
		if (base.DistanceToMaster < _followDistanceMax)
		{
			return DistanceToPOI > 500f;
		}
		return false;
	}

	private bool NeedToEndMoveToPOI()
	{
		if (!(base.DistanceToMaster >= _followDistanceMax))
		{
			return DistanceToPOI < 100f;
		}
		return true;
	}

	private bool NeedToUnAgressToMaster()
	{
		return base.DistanceToMaster < _followDistanceMin;
	}

	private IEnumerator ChaseDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CChaseDoing_003Ed__95(0)
		{
			_003C_003E4__this = this
		};
	}

	private bool ChaseTransitions()
	{
		if (NeedToEndChaseMaster())
		{
			if (IsMasterMoreCloseToPOI)
			{
				base.CurState = State.MoveToPOI;
				return true;
			}
			base.CurState = ((!(UnityEngine.Random.value < _barkAfterChaseProbability)) ? State.Normal : State.Bark);
			return true;
		}
		return false;
	}

	private bool CheckWalk(bool wasLastMoveWalk)
	{
		float num = 100f;
		if (wasLastMoveWalk)
		{
			num = -100f;
		}
		if (DistanceMasterToPOI > DistanceToPOI && base.DistanceToMaster > _walkToPOIDistance + num)
		{
			return true;
		}
		return false;
	}

	private IEnumerator MoveToPOIDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CMoveToPOIDoing_003Ed__98(0)
		{
			_003C_003E4__this = this
		};
	}

	private Vector3 MoveToPOIDestPos()
	{
		return POIClientPosition;
	}

	private bool MoveToPOITransitions()
	{
		if (NeedToEndMoveToPOI())
		{
			base.CurState = State.Normal;
			return true;
		}
		if (NeedToChaseMaster())
		{
			base.CurState = State.Aggress;
			return true;
		}
		return false;
	}

	private IEnumerator AggressDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CAggressDoing_003Ed__101(0)
		{
			_003C_003E4__this = this
		};
	}

	private bool AggressTransitions()
	{
		if (NeedToUnAgressToMaster())
		{
			base.CurState = ((!(UnityEngine.Random.value < _barkAfterAggressProbability)) ? State.Normal : State.Bark);
			return true;
		}
		return false;
	}

	private IEnumerator BarkDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBarkDoing_003Ed__103(0)
		{
			_003C_003E4__this = this
		};
	}

	public void Dog_Happy()
	{
		base.CurState = State.Happy;
	}

	private IEnumerator HappyDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CHappyDoing_003Ed__105(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator IdleDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CIdleDoing_003Ed__106(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator FarewellDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFarewellDoing_003Ed__107(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator BarkToPlayer(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBarkToPlayer_003Ed__108(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	private IEnumerator CoMoveTo(Func<Vector3> funcTargetPos, Func<bool> funcTransition, string moveMotion, float moveSpeed, bool endAtReached = false, float fadeInTime = 0.1f)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoMoveTo_003Ed__109(0)
		{
			_003C_003E4__this = this,
			funcTargetPos = funcTargetPos,
			funcTransition = funcTransition,
			moveMotion = moveMotion,
			moveSpeed = moveSpeed,
			endAtReached = endAtReached,
			fadeInTime = fadeInTime
		};
	}

	private IEnumerator CoMoveToWithPathFind(Func<Vector3> funcTargetPos, Func<bool> funcTransition, Func<bool, bool> funcCheckWalk, string runMotion, float runSpeed, string walkMotion, float walkSpeed)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoMoveToWithPathFind_003Ed__110(0)
		{
			_003C_003E4__this = this,
			funcTargetPos = funcTargetPos,
			funcTransition = funcTransition,
			funcCheckWalk = funcCheckWalk,
			runMotion = runMotion,
			runSpeed = runSpeed,
			walkMotion = walkMotion,
			walkSpeed = walkSpeed
		};
	}

	private IEnumerator CoTurnAndCrossFadeMotion(string afterTurnMotionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoTurnAndCrossFadeMotion_003Ed__111(0)
		{
			_003C_003E4__this = this,
			afterTurnMotionName = afterTurnMotionName,
			fadeTime = fadeTime,
			loop = loop,
			beginTime = beginTime,
			playbackRate = playbackRate
		};
	}

	private void FixUpRootBoneAndCrossFadeMotion(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		Matrix4x4 localToWorldMatrix = TargetAnimal.Bip001Transform.localToWorldMatrix;
		TargetAnimal.Play(_standMotion);
		TargetAnimal.Anim.Sample();
		Matrix4x4 m = Matrix4x4.TRS(TargetAnimal.Bip001Transform.localPosition, TargetAnimal.Bip001Transform.localRotation, TargetAnimal.Bip001Transform.localScale);
		Maths.DecomposeMatrix(localToWorldMatrix * Matrix4x4.Inverse(m), out var position, out var rotation, out var _);
		TargetAnimal.TurnToYaw(rotation.eulerAngles.y, bSnap: true);
		TargetAnimal.CurrentPosition = position;
		TargetAnimal.CrossFade(motionName, fadeTime, loop, beginTime, playbackRate);
	}

	private void CrossFadeAndFitLocation(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		if (!loop || !(TargetAnimal.CurAnimState != null) || !(TargetAnimal.CurAnimState.name == motionName))
		{
			Vector3 position = TargetAnimal.Bip001Transform.position;
			TargetAnimal.CrossFade(motionName, fadeTime, loop, beginTime, playbackRate);
			TargetAnimal.Anim.Sample();
			Vector3 pos = position - TargetAnimal.Bip001Transform.position;
			TargetAnimal.CurrentPosition += Maths.Make2D(pos);
		}
	}
}
