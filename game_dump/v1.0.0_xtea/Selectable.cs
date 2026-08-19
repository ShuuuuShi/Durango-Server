using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using K1Network;
using UnityEngine;

public abstract class Selectable : MonoBehaviour
{
	public enum State
	{
		Invalid,
		Normal,
		Wait,
		Success,
		Fail,
		Timeout
	}

	public static Selectable Current;

	public static Packet Packet;

	public Action Clicked;

	private bool _initialized;

	private List<ulong> _replyTypeCodes;

	private Action<State> _onResponse;

	private float _stateTimer;

	private bool _isPlayStateRoutine;

	private bool _disable;

	private bool _isSelect;

	public State AsyncState { get; private set; }

	public bool Disable
	{
		get
		{
			return _disable;
		}
		set
		{
			_disable = value;
			OnSelectDisable(value);
		}
	}

	public bool Select
	{
		get
		{
			return _isSelect;
		}
		set
		{
			_isSelect = value;
			OnSelected(value);
			if (this.Selected != null)
			{
				Current = this;
				this.Selected(_isSelect);
				Current = null;
			}
		}
	}

	public event Action<bool> Selected;

	private void Awake()
	{
		Init();
	}

	public void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			OnInit();
		}
	}

	protected virtual void OnEnable()
	{
		SetState(State.Normal);
	}

	protected virtual void OnDisable()
	{
		SetState(State.Normal);
	}

	protected abstract void OnInit();

	protected virtual void OnSelectDisable(bool disable)
	{
		Refresh();
	}

	protected virtual void OnSelected(bool select)
	{
		Refresh();
	}

	protected abstract void Refresh(bool select);

	protected void Refresh()
	{
		Refresh(Select);
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (AsyncState != State.Normal)
		{
			return;
		}
		Connection frontend = Connections.Frontend;
		ulong num = frontend.CurrentSeq();
		Current = this;
		if (Clicked != null)
		{
			Clicked();
		}
		Current = null;
		ulong num2 = frontend.CurrentSeq();
		if (num != num2)
		{
			Connections.Frontend.PacketReceived += OnPacketHandler;
			if (_replyTypeCodes == null)
			{
				_replyTypeCodes = new List<ulong>();
			}
			for (ulong num3 = num; num3 < num2; num3++)
			{
				_replyTypeCodes.Add(num3);
			}
			OnRequest();
		}
	}

	private void OnRequest()
	{
		SetState(State.Wait);
	}

	private void SetStateTimer(float period)
	{
		_stateTimer = Time.time + period;
		if (!_isPlayStateRoutine)
		{
			if (period > 0f)
			{
				((MonoBehaviour)this).StartCoroutine(CoStateRoutine());
			}
			else
			{
				StateChanged();
			}
		}
	}

	private IEnumerator CoStateRoutine()
	{
		_isPlayStateRoutine = true;
		while (_stateTimer > Time.time)
		{
			yield return null;
		}
		StateChanged();
		_isPlayStateRoutine = false;
	}

	private void StateChanged()
	{
		switch (AsyncState)
		{
		case State.Normal:
			break;
		case State.Wait:
			SetState(State.Timeout);
			break;
		case State.Success:
		case State.Fail:
		case State.Timeout:
			SetState(State.Normal);
			break;
		}
	}

	protected void SetState(State state)
	{
		if (AsyncState == state)
		{
			return;
		}
		State asyncState = AsyncState;
		AsyncState = state;
		float num = 0f;
		switch (state)
		{
		case State.Normal:
			OnNormalState();
			break;
		case State.Wait:
			num = OnWaitState();
			if (num <= 0f)
			{
				SetState(State.Normal);
				return;
			}
			break;
		case State.Success:
			num = OnSuccessState();
			break;
		case State.Fail:
			num = OnFailState();
			break;
		case State.Timeout:
			num = OnTimeoutState();
			break;
		}
		switch (state)
		{
		case State.Success:
		case State.Fail:
		case State.Timeout:
			if (_onResponse != null)
			{
				_onResponse(state);
				_onResponse = null;
			}
			break;
		}
		if (asyncState == State.Wait)
		{
			if (_replyTypeCodes != null)
			{
				_replyTypeCodes.Clear();
			}
			Connections.Frontend.PacketReceived -= OnPacketHandler;
		}
		SetStateTimer(num);
	}

	public void WaitResponse(Action<State> callback)
	{
		_onResponse = (Action<State>)Delegate.Combine(_onResponse, callback);
	}

	protected virtual void OnNormalState()
	{
	}

	protected virtual float OnWaitState()
	{
		return 0f;
	}

	protected virtual float OnSuccessState()
	{
		return 0f;
	}

	protected virtual float OnFailState()
	{
		return 0f;
	}

	protected virtual float OnTimeoutState()
	{
		return 0f;
	}

	private void OnPacketHandler(Packet packet)
	{
		if (AsyncState == State.Wait && _replyTypeCodes != null && _replyTypeCodes.Contains(packet.Header.ReplyOf))
		{
			bool flag = packet.Header.TypeCode == 1022;
			Packet = packet;
			SetState((!flag) ? State.Success : State.Fail);
			Packet = null;
		}
	}
}
