using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render.Screen;
using UnityEngine;

namespace Durango.UI;

public class TransitionCurtain : LoadingCurtainBase
{
	[CompilerGenerated]
	private sealed class _003CCoShowRoutine_003Ed__3 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TransitionCurtain _003C_003E4__this;

		public float fadeIn;

		public Action callback;

		public float fadeOut;

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
		public _003CCoShowRoutine_003Ed__3(int _003C_003E1__state)
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
			TransitionCurtain transitionCurtain = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				transitionCurtain.SetState(LoadingState.Open);
				transitionCurtain.Widget.alpha = 0f;
				goto IL_005d;
			case 1:
				_003C_003E1__state = -1;
				goto IL_005d;
			case 2:
				_003C_003E1__state = -1;
				callback?.Invoke();
				transitionCurtain.Duration = fadeOut;
				transitionCurtain.SetState(LoadingState.Closing);
				_003C_003E2__current = transitionCurtain.Fadeout();
				_003C_003E1__state = 3;
				return true;
			case 3:
				{
					_003C_003E1__state = -1;
					transitionCurtain.SetState(LoadingState.Closed);
					transitionCurtain._transitionCurtain.mainTexture = null;
					return false;
				}
				IL_005d:
				if (transitionCurtain._transitionCurtain.mainTexture == null)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				transitionCurtain.Duration = fadeIn;
				_003C_003E2__current = transitionCurtain.Fadein();
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

	[SerializeField]
	private UITexture _transitionCurtain;

	public void PlayColorRoutine(float fadeIn, float fadeOut, Color curtainColor, Action callback)
	{
		_transitionCurtain.mainTexture = Texture2D.whiteTexture;
		_transitionCurtain.color = curtainColor;
		StopAllCoroutines();
		StartCoroutine(CoShowRoutine(fadeIn, fadeOut, callback));
	}

	public void PlayCaptureRoutine(float fadeIn, float fadeOut, Action callback)
	{
		_transitionCurtain.mainTexture = null;
		_transitionCurtain.color = Color.white;
		ScreenCapture.CaptureOption option = default(ScreenCapture.CaptureOption);
		option.OnResult = delegate(Texture2D tex)
		{
			base.Widget.alpha = 1f;
			_transitionCurtain.mainTexture = tex;
		};
		ScreenCapture.Capture(option);
		StartCoroutine(CoShowRoutine(fadeIn, fadeOut, callback));
	}

	private IEnumerator CoShowRoutine(float fadeIn, float fadeOut, Action callback)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoShowRoutine_003Ed__3(0)
		{
			_003C_003E4__this = this,
			fadeIn = fadeIn,
			fadeOut = fadeOut,
			callback = callback
		};
	}
}
