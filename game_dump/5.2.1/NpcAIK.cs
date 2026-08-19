using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Model;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

public class NpcAIK : StateBasedAI<NpcAIK.State>
{
	public enum State
	{
		Invalid = -1,
		Normal,
		Chase,
		Run,
		Count
	}

	[CompilerGenerated]
	private sealed class _003CChaseDoing_003Ed__26 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIK _003C_003E4__this;

		private float _003CprevTime_003E5__2;

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
		public _003CChaseDoing_003Ed__26(int _003C_003E1__state)
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
			NpcAIK npcAIK = _003C_003E4__this;
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
				npcAIK.TargetAnimal.CrossFade(npcAIK._moveMotion, 0.1f);
				_003CprevTime_003E5__2 = Time.time;
			}
			if (null == npcAIK._victim || npcAIK.IsInterrupted)
			{
				return false;
			}
			float num2 = Time.time - _003CprevTime_003E5__2;
			_003CprevTime_003E5__2 = Time.time;
			Vector3 vector = Maths.Make2D(npcAIK._victim.transform.position - npcAIK.transform.position);
			if (!(vector.magnitude <= npcAIK._engageDistance))
			{
				float yaw = Maths.CalcYawWithTarget(npcAIK._victim.transform.position, npcAIK.transform.position);
				npcAIK.TargetAnimal.TurnToYaw(yaw, bSnap: false);
				Vector3 vector2 = vector.normalized * npcAIK._moveSpeed;
				npcAIK.TargetAnimal.CurrentPosition += vector2 * num2;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			npcAIK.CurState = State.Normal;
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
	private sealed class _003CNormalDoing_003Ed__23 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIK _003C_003E4__this;

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
		public _003CNormalDoing_003Ed__23(int _003C_003E1__state)
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
			NpcAIK npcAIK = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (null != npcAIK._victim && (npcAIK._victim.transform.position - npcAIK.transform.position).magnitude > npcAIK._engageDistance)
				{
					npcAIK.CurState = State.Chase;
				}
				_003C_003E2__current = new WaitForSeconds(0.3f);
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
	private sealed class _003COnAfterDoingState_003Ed__18 : IEnumerator<object>, IDisposable, IEnumerator
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
		public _003COnAfterDoingState_003Ed__18(int _003C_003E1__state)
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
	private sealed class _003COnBeforeDoingState_003Ed__17 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIK _003C_003E4__this;

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
		public _003COnBeforeDoingState_003Ed__17(int _003C_003E1__state)
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
			NpcAIK npcAIK = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				BoneLookAtTarget component = npcAIK.GetComponent<BoneLookAtTarget>();
				npcAIK._victim = PlayerBehavior.LocalPlayer.gameObject;
				if (npcAIK._victim == null)
				{
					_003C_003E2__current = new WaitForSeconds(1f);
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
	private sealed class _003COnStart_003Ed__16 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIK _003C_003E4__this;

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
		public _003COnStart_003Ed__16(int _003C_003E1__state)
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
			NpcAIK npcAIK = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				npcAIK.TargetAnimal.EntityId = "666";
				npcAIK.CurState = State.Normal;
				npcAIK.GetComponent<BoneLookAtTarget>().AutoChangeTarget = false;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (!TerrainBase.IsPlayerInitialized)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			Vector3 vector = Util.WorldPositionToClientPosition(new Vector3(512f, 512f));
			npcAIK._initialPos = PlayerBehavior.LocalPlayer.CurrentPosition + (vector - PlayerBehavior.LocalPlayer.CurrentPosition).normalized * npcAIK._appearDiatanceFromPlayer;
			npcAIK._initialPos.y = 0f;
			npcAIK.TargetAnimal.CurrentPosition = npcAIK._initialPos;
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
	private sealed class _003CRunDoing_003Ed__29 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NpcAIK _003C_003E4__this;

		private float _003CprevTime_003E5__2;

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
		public _003CRunDoing_003Ed__29(int _003C_003E1__state)
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
			NpcAIK npcAIK = _003C_003E4__this;
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
				npcAIK.TargetAnimal.CrossFade(npcAIK._moveMotion, 0.1f);
				_003CprevTime_003E5__2 = Time.time;
			}
			if (null == npcAIK._victim || npcAIK.IsInterrupted)
			{
				return false;
			}
			float num2 = Time.time - _003CprevTime_003E5__2;
			_003CprevTime_003E5__2 = Time.time;
			Vector3 vector = Maths.Make2D(npcAIK._initialPos - npcAIK.transform.position);
			if (!(vector.magnitude <= npcAIK._engageDistance))
			{
				float yaw = Maths.CalcYawWithTarget(npcAIK._initialPos, npcAIK.transform.position);
				npcAIK.TargetAnimal.TurnToYaw(yaw, bSnap: false);
				Vector3 vector2 = vector.normalized * npcAIK._moveSpeed;
				npcAIK.TargetAnimal.CurrentPosition += vector2 * num2;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			UnityEngine.Object.Destroy(npcAIK.gameObject);
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

	[SerializeField]
	private string _standMotion = "F_Barehand_Stand";

	[SerializeField]
	private string _moveMotion = "F_Barehand_Run";

	[SerializeField]
	private float _engageDistance = 450f;

	[SerializeField]
	private float _moveSpeed = 500f;

	[SerializeField]
	private float _appearDiatanceFromPlayer = 1000f;

	private Vector3 _initialPos;

	private GameObject _victim;

	private AnimalBehavior _targetAnimal;

	protected override State InvalidState => State.Invalid;

	protected override int StateEnumCount => 3;

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

	protected override void DefineStates()
	{
		AddState(State.Normal, new StateElem
		{
			Entered = NormalEntered,
			Doing = NormalDoing,
			Exited = NormalExited
		});
		AddState(State.Chase, new StateElem
		{
			Entered = ChaseEntered,
			Doing = ChaseDoing,
			Exited = ChaseExited
		});
		AddState(State.Run, new StateElem
		{
			Entered = RunEntered,
			Doing = RunDoing,
			Exited = RunExited
		});
	}

	protected override IEnumerator OnStart()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnStart_003Ed__16(0)
		{
			_003C_003E4__this = this
		};
	}

	protected override IEnumerator OnBeforeDoingState()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnBeforeDoingState_003Ed__17(0)
		{
			_003C_003E4__this = this
		};
	}

	protected override IEnumerator OnAfterDoingState()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnAfterDoingState_003Ed__18(0);
	}

	protected override bool IsAIEnded()
	{
		return false;
	}

	protected override bool IsTerminalState(State state)
	{
		return false;
	}

	private void NormalEntered()
	{
		TargetAnimal.CrossFade(_standMotion, 0.1f);
	}

	private void NormalExited()
	{
	}

	private IEnumerator NormalDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CNormalDoing_003Ed__23(0)
		{
			_003C_003E4__this = this
		};
	}

	private void ChaseEntered()
	{
	}

	private void ChaseExited()
	{
	}

	private IEnumerator ChaseDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CChaseDoing_003Ed__26(0)
		{
			_003C_003E4__this = this
		};
	}

	private void RunEntered()
	{
	}

	private void RunExited()
	{
	}

	private IEnumerator RunDoing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRunDoing_003Ed__29(0)
		{
			_003C_003E4__this = this
		};
	}

	public void EventRun()
	{
		base.CurState = State.Run;
	}
}
