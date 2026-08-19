using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Durango.Utils;

public class DelayedFunction
{
	[CompilerGenerated]
	private sealed class _003CCoRoutine_003Ed__5 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public DelayedFunction _003C_003E4__this;

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
		public _003CCoRoutine_003Ed__5(int _003C_003E1__state)
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
			DelayedFunction delayedFunction = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = delayedFunction._yield;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				delayedFunction._func();
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

	private readonly Action _func;

	private readonly YieldInstruction _yield;

	private int _callFrame;

	public DelayedFunction(Action func, YieldInstruction yi = null)
	{
		_func = func;
		_yield = yi;
	}

	public void Call(MonoBehaviour parent)
	{
		if (parent.gameObject.activeInHierarchy)
		{
			int frameCount = Time.frameCount;
			if (frameCount != _callFrame)
			{
				_callFrame = frameCount;
				parent.StartCoroutine(CoRoutine());
			}
		}
		else
		{
			_func();
		}
	}

	private IEnumerator CoRoutine()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoRoutine_003Ed__5(0)
		{
			_003C_003E4__this = this
		};
	}
}
