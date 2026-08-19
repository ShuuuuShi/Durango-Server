using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Durango.UI;

public class BlankLoadingCurtain : LoadingCurtainBase
{
	[CompilerGenerated]
	private sealed class _003CCoShowRoutine_003Ed__1 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public BlankLoadingCurtain _003C_003E4__this;

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
		public _003CCoShowRoutine_003Ed__1(int _003C_003E1__state)
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
			BlankLoadingCurtain blankLoadingCurtain = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = blankLoadingCurtain.Fadein();
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0062;
			case 2:
				_003C_003E1__state = -1;
				goto IL_0062;
			case 3:
				{
					_003C_003E1__state = -1;
					blankLoadingCurtain.SetState(LoadingState.Closed);
					return false;
				}
				IL_0062:
				if (blankLoadingCurtain.State == LoadingState.Open)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003E2__current = blankLoadingCurtain.Fadeout();
				_003C_003E1__state = 3;
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

	private void OnEnable()
	{
		SetState(LoadingState.Open);
		StartCoroutine(CoShowRoutine());
	}

	private IEnumerator CoShowRoutine()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoShowRoutine_003Ed__1(0)
		{
			_003C_003E4__this = this
		};
	}

	public void Close()
	{
		SetState(LoadingState.Closing);
	}
}
