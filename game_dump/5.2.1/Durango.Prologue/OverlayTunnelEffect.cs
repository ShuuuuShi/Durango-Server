using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.UI.Prologue;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

[ExecuteInEditMode]
public class OverlayTunnelEffect : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CBeginEffects_003Ed__6 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public OverlayTunnelEffect _003C_003E4__this;

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
		public _003CBeginEffects_003Ed__6(int _003C_003E1__state)
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
			OverlayTunnelEffect overlayTunnelEffect = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				overlayTunnelEffect._targetBlackPanelTexture.transform.position = overlayTunnelEffect._beginPos;
				overlayTunnelEffect._targetWhitePanelTexture.transform.position = overlayTunnelEffect._beginPos;
				overlayTunnelEffect._targetBlackPanelTexture.alpha = Singleton<PrologueTunnelController>.Instance()._maxAlphaBlack;
				overlayTunnelEffect._targetWhitePanelTexture.alpha = Singleton<PrologueTunnelController>.Instance()._maxAlphaWhite;
				_003C_003E2__current = new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._preDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = overlayTunnelEffect.StartCoroutine(overlayTunnelEffect.TunnelStart());
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = overlayTunnelEffect.StartCoroutine(overlayTunnelEffect.TunnelStartFadeOut());
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._tunnelLeavingDelay);
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003C_003E2__current = overlayTunnelEffect.StartCoroutine(overlayTunnelEffect.TunnelEnd());
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				_003C_003E2__current = overlayTunnelEffect.StartCoroutine(overlayTunnelEffect.TunnelEndFadeOut());
				_003C_003E1__state = 6;
				return true;
			case 6:
				_003C_003E1__state = -1;
				overlayTunnelEffect.OnFinish();
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
	private sealed class _003CTunnelEnd_003Ed__9 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public OverlayTunnelEffect _003C_003E4__this;

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
		public _003CTunnelEnd_003Ed__9(int _003C_003E1__state)
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
			OverlayTunnelEffect overlayTunnelEffect = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				TweenPosition.Begin(overlayTunnelEffect._targetWhitePanelTexture.gameObject, Singleton<PrologueTunnelController>.Instance()._tunnelLeavingDuration, overlayTunnelEffect._endPos).PlayForward();
				_003C_003E2__current = new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._tunnelLeavingDuration);
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
	private sealed class _003CTunnelEndFadeOut_003Ed__10 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public OverlayTunnelEffect _003C_003E4__this;

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
		public _003CTunnelEndFadeOut_003Ed__10(int _003C_003E1__state)
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
			OverlayTunnelEffect overlayTunnelEffect = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				TweenAlpha tweenAlpha = TweenAlpha.Begin(overlayTunnelEffect._targetWhitePanelTexture.gameObject, Singleton<PrologueTunnelController>.Instance()._tunnelLeavingFadeOut, 0f);
				tweenAlpha.method = UITweener.Method.EaseOut;
				tweenAlpha.PlayForward();
				_003C_003E2__current = new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._tunnelLeavingFadeOut);
				_003C_003E1__state = 1;
				return true;
			}
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
	private sealed class _003CTunnelStart_003Ed__7 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public OverlayTunnelEffect _003C_003E4__this;

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
		public _003CTunnelStart_003Ed__7(int _003C_003E1__state)
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
			OverlayTunnelEffect overlayTunnelEffect = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				TweenPosition.Begin(overlayTunnelEffect._targetBlackPanelTexture.gameObject, Singleton<PrologueTunnelController>.Instance()._tunnelEnteringDuration, overlayTunnelEffect._endPos).PlayForward();
				_003C_003E2__current = new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._tunnelEnteringDuration);
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
	private sealed class _003CTunnelStartFadeOut_003Ed__8 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public OverlayTunnelEffect _003C_003E4__this;

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
		public _003CTunnelStartFadeOut_003Ed__8(int _003C_003E1__state)
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
			OverlayTunnelEffect overlayTunnelEffect = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				TweenAlpha tweenAlpha = TweenAlpha.Begin(overlayTunnelEffect._targetBlackPanelTexture.gameObject, Singleton<PrologueTunnelController>.Instance()._tunnelEnteringFadeOut, 0f);
				tweenAlpha.method = UITweener.Method.EaseOut;
				tweenAlpha.PlayForward();
				_003C_003E2__current = new WaitForSeconds(Singleton<PrologueTunnelController>.Instance()._tunnelEnteringFadeOut);
				_003C_003E1__state = 1;
				return true;
			}
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

	public Vector3 _beginPos;

	public Vector3 _endPos;

	public PrologueOverlayGroup _prologueOverlayGroup;

	public UITexture _targetBlackPanelTexture;

	public UITexture _targetWhitePanelTexture;

	private void OnEnable()
	{
		StartCoroutine(BeginEffects());
	}

	private IEnumerator BeginEffects()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBeginEffects_003Ed__6(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator TunnelStart()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTunnelStart_003Ed__7(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator TunnelStartFadeOut()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTunnelStartFadeOut_003Ed__8(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator TunnelEnd()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTunnelEnd_003Ed__9(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator TunnelEndFadeOut()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTunnelEndFadeOut_003Ed__10(0)
		{
			_003C_003E4__this = this
		};
	}

	private void OnFinish()
	{
		base.gameObject.SetActive(value: false);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.ReturnToSeat);
	}
}
