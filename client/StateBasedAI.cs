using System;
using System.Collections;
using System.Collections.Generic;
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
		yield break;
	}

	protected virtual IEnumerator OnBeforeDoingState()
	{
		yield break;
	}

	protected virtual IEnumerator OnAfterDoingState()
	{
		yield break;
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
		yield return StartCoroutine(OnStart());
		while (!IsAIEnded())
		{
			IsInterrupted = false;
			yield return StartCoroutine(OnBeforeDoingState());
			StateElem state = _states.Get(CurState);
			if (state != null)
			{
				if (state.Doing == null)
				{
					while (!IsInterrupted)
					{
						yield return null;
					}
				}
				else
				{
					yield return StartCoroutine(state.Doing());
				}
			}
			yield return StartCoroutine(OnAfterDoingState());
			yield return null;
		}
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
