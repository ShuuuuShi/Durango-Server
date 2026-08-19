using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public abstract class StateBasedAI<T> : MonoBehaviour where T : IConvertible
{
	protected class StateElem
	{
		public Action Entered;

		public Func<IEnumerator> Doing;

		public Action Exited;
	}

	[CompilerGenerated]
	private sealed class _003COnAfterDoingState_003Ed__30 : IEnumerator<object>, IDisposable, IEnumerator
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
		public _003COnAfterDoingState_003Ed__30(int _003C_003E1__state)
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
	private sealed class _003COnBeforeDoingState_003Ed__29 : IEnumerator<object>, IDisposable, IEnumerator
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
		public _003COnBeforeDoingState_003Ed__29(int _003C_003E1__state)
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
	private sealed class _003COnStart_003Ed__28 : IEnumerator<object>, IDisposable, IEnumerator
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
		public _003COnStart_003Ed__28(int _003C_003E1__state)
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
	private sealed class _003CStart_003Ed__34 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public StateBasedAI<T> _003C_003E4__this;

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
		public _003CStart_003Ed__34(int _003C_003E1__state)
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
			StateBasedAI<T> stateBasedAI = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = stateBasedAI.StartCoroutine(stateBasedAI.OnStart());
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
			{
				_003C_003E1__state = -1;
				StateElem stateElem = stateBasedAI._states.Get(stateBasedAI.CurState);
				if (stateElem != null)
				{
					if (stateElem.Doing == null)
					{
						goto IL_00c0;
					}
					_003C_003E2__current = stateBasedAI.StartCoroutine(stateElem.Doing());
					_003C_003E1__state = 4;
					return true;
				}
				goto IL_00f1;
			}
			case 3:
				_003C_003E1__state = -1;
				goto IL_00c0;
			case 4:
				_003C_003E1__state = -1;
				goto IL_00f1;
			case 5:
				_003C_003E1__state = -1;
				_003C_003E2__current = null;
				_003C_003E1__state = 6;
				return true;
			case 6:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_00c0:
				if (!stateBasedAI.IsInterrupted)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 3;
					return true;
				}
				goto IL_00f1;
				IL_00f1:
				_003C_003E2__current = stateBasedAI.StartCoroutine(stateBasedAI.OnAfterDoingState());
				_003C_003E1__state = 5;
				return true;
			}
			if (!stateBasedAI.IsAIEnded())
			{
				stateBasedAI.IsInterrupted = false;
				_003C_003E2__current = stateBasedAI.StartCoroutine(stateBasedAI.OnBeforeDoingState());
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

	private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;

	private readonly Dictionary<T, StateElem> _states = new Dictionary<T, StateElem>();

	private T _curState;

	private T _prevState;

	private Vector3 _lastMasterPos = Vector3.zero;

	[CanBeNull]
	public GameObject Master { get; protected set; }

	protected float DistanceToMaster => (MasterPos - base.transform.position).magnitude;

	protected T CurState
	{
		get
		{
			return _curState;
		}
		set
		{
			TransitionTo(value);
		}
	}

	public bool IsInterrupted { get; set; }

	protected abstract T InvalidState { get; }

	protected abstract int StateEnumCount { get; }

	protected Vector3 MasterPos
	{
		get
		{
			if (Master != null)
			{
				_lastMasterPos = Master.transform.position;
			}
			return _lastMasterPos;
		}
	}

	protected void TransitionTo(T nextState, bool force = false)
	{
		if (!force && IsAIEnded())
		{
			return;
		}
		IsInterrupted = !IsTerminalState(_curState) || force;
		if (!force && IsTerminalState(_curState))
		{
			return;
		}
		_prevState = _curState;
		_curState = nextState;
		if (!Comparer.Equals(_prevState, InvalidState))
		{
			StateElem stateElem = _states.Get(_prevState);
			if (stateElem != null && stateElem.Exited != null)
			{
				stateElem.Exited();
			}
		}
		if (!Comparer.Equals(_curState, InvalidState))
		{
			StateElem stateElem2 = _states.Get(_curState);
			if (stateElem2 != null && stateElem2.Entered != null)
			{
				stateElem2.Entered();
			}
		}
	}

	protected abstract void DefineStates();

	protected virtual void OnAwake()
	{
	}

	protected virtual IEnumerator OnStart()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnStart_003Ed__28(0);
	}

	protected virtual IEnumerator OnBeforeDoingState()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnBeforeDoingState_003Ed__29(0);
	}

	protected virtual IEnumerator OnAfterDoingState()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnAfterDoingState_003Ed__30(0);
	}

	protected abstract bool IsAIEnded();

	protected abstract bool IsTerminalState(T state);

	private void Awake()
	{
		_curState = InvalidState;
		_prevState = InvalidState;
		DefineStates();
		OnAwake();
	}

	private IEnumerator Start()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStart_003Ed__34(0)
		{
			_003C_003E4__this = this
		};
	}

	protected void AddState(T state, StateElem stateElem)
	{
		_states.Add(state, stateElem);
	}

	protected Vector3 GetRandomMasterSurroundingPos(float radius)
	{
		return Maths.GetRandomSurroundingPos(MasterPos, radius);
	}

	protected Vector3 CalcMasterNearestPos(float distance)
	{
		Vector3 normalized = (base.transform.position - MasterPos).normalized;
		return Maths.Make2D(MasterPos + normalized * distance);
	}
}
