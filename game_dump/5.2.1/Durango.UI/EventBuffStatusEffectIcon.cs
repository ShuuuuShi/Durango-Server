using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Durango.UI;

public class EventBuffStatusEffectIcon : StatusEffectIcon
{
	[CompilerGenerated]
	private sealed class _003CCoFadeIn_003Ed__4 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public EventBuffStatusEffectIcon _003C_003E4__this;

		private float _003CstartTime_003E5__2;

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
		public _003CCoFadeIn_003Ed__4(int _003C_003E1__state)
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
			EventBuffStatusEffectIcon eventBuffStatusEffectIcon = _003C_003E4__this;
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
				eventBuffStatusEffectIcon.IsPlayingEffect = true;
				_003CstartTime_003E5__2 = Time.time;
			}
			float num2 = Time.time - _003CstartTime_003E5__2;
			float f = num2 / 1f;
			eventBuffStatusEffectIcon.SetAlpha(Mathf.Pow(f, 3f));
			if (!(num2 > 1f))
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			eventBuffStatusEffectIcon.IsPlayingEffect = false;
			if (eventBuffStatusEffectIcon.Index < 0)
			{
				eventBuffStatusEffectIcon.PlayFadeOut();
			}
			else
			{
				eventBuffStatusEffectIcon.OnFinishFadeEffect();
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

	public override Vector3 Position
	{
		get
		{
			return base.transform.localPosition;
		}
		set
		{
			base.transform.localPosition = value;
		}
	}

	public override void PlayFadeIn(Vector3 targetPos)
	{
		if (!base.IsPlayingEffect)
		{
			base.transform.localPosition = targetPos;
			StartCoroutine(CoFadeIn());
		}
	}

	private IEnumerator CoFadeIn()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoFadeIn_003Ed__4(0)
		{
			_003C_003E4__this = this
		};
	}
}
