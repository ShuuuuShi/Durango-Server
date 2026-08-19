using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Durango.Prologue;

public class TriggerActivateTarget : TriggerOnce
{
	[CompilerGenerated]
	private sealed class _003CcoTriggerBegin_003Ed__4 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float delay;

		public TriggerActivateTarget _003C_003E4__this;

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
		public _003CcoTriggerBegin_003Ed__4(int _003C_003E1__state)
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
			TriggerActivateTarget triggerActivateTarget = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(delay);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				int count = triggerActivateTarget._targetObjects.Count;
				for (int i = 0; i < count; i++)
				{
					if ((bool)triggerActivateTarget._targetObjects[i])
					{
						triggerActivateTarget._targetObjects[i].SetActive(triggerActivateTarget._activateTarget);
					}
				}
				return false;
			}
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

	public List<GameObject> _targetObjects = new List<GameObject>();

	public bool _activateTarget = true;

	public float _delay;

	protected override bool TriggerEntered(Collider other)
	{
		if (_targetObjects.Count > 0)
		{
			StartCoroutine(coTriggerBegin(_delay));
		}
		return true;
	}

	private IEnumerator coTriggerBegin(float delay)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CcoTriggerBegin_003Ed__4(0)
		{
			_003C_003E4__this = this,
			delay = delay
		};
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}
}
