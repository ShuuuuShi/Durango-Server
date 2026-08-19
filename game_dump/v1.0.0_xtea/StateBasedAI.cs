using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBasedAI<T> : MonoBehaviour
{
	protected class StateElem
	{
		public Action Entered;

		public Func<IEnumerator> Doing;

		public Action Exited;
	}

	private readonly Dictionary<T, StateElem> _states = new Dictionary<T, StateElem>();

	private T _curState;

	private T _prevState;

	private bool _interrupted;

	public GameObject Master { get; protected set; }

	protected float DistanceToMaster
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = MasterPos - ((Component)this).transform.position;
			return ((Vector3)(ref val)).magnitude;
		}
	}

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

	public bool IsInterrupted
	{
		get
		{
			return _interrupted;
		}
		set
		{
			_interrupted = value;
		}
	}

	protected abstract T InvalidState { get; }

	protected abstract int StateEnumCount { get; }

	protected Vector3 MasterPos
	{
		get
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Master == (Object)null)
			{
				return Vector3.zero;
			}
			return Master.transform.position;
		}
	}

	protected void TransitionTo(T nextState, bool force = false)
	{
		if (!force && IsAIEnded())
		{
			return;
		}
		IsInterrupted = !IsTerminalState(_prevState);
		if (force || !IsTerminalState(_prevState))
		{
			_prevState = _curState;
			_curState = nextState;
			if (!_prevState.Equals(InvalidState) && _states.TryGetValue(_prevState, out var value) && value.Exited != null)
			{
				value.Exited();
			}
			if (!_curState.Equals(InvalidState) && _states.TryGetValue(_curState, out value) && value.Entered != null)
			{
				value.Entered();
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
		yield return ((MonoBehaviour)this).StartCoroutine(OnStart());
		while (!IsAIEnded())
		{
			IsInterrupted = false;
			yield return ((MonoBehaviour)this).StartCoroutine(OnBeforeDoingState());
			if (_states.TryGetValue(CurState, out var state))
			{
				yield return ((MonoBehaviour)this).StartCoroutine(state.Doing());
			}
			yield return ((MonoBehaviour)this).StartCoroutine(OnAfterDoingState());
			yield return null;
		}
	}

	protected void AddState(T state, StateElem stateElem)
	{
		_states.Add(state, stateElem);
	}

	protected Vector3 GetRandomMasterSurroundingPos(float radius)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return KMathUtil.GetRandomSurroundingPos(MasterPos, radius);
	}

	protected Vector3 CalcMasterNearestPos(float distance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - MasterPos;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		return KMathUtil.Make2D(MasterPos + normalized * distance);
	}
}
