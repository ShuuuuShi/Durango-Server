using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Prologue;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PrologueLoadingCurtain : LoadingCurtainBase
{
	[Serializable]
	private struct YearInfo
	{
		public UIWidget Parent;

		public UILabel Title;

		public UILabel Year;
	}

	[CompilerGenerated]
	private sealed class _003CCoShowRoutine_003Ed__7 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueLoadingCurtain _003C_003E4__this;

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
		public _003CCoShowRoutine_003Ed__7(int _003C_003E1__state)
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
			PrologueLoadingCurtain prologueLoadingCurtain = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				prologueLoadingCurtain.Widget.alpha = 1f;
				_003C_003E2__current = prologueLoadingCurtain.ShowYearInfo();
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (!GameManager.IsPrologueMode)
				{
					_003C_003E2__current = prologueLoadingCurtain.WaitForChunkLoading();
					_003C_003E1__state = 2;
					return true;
				}
				goto IL_0089;
			case 2:
				_003C_003E1__state = -1;
				if (LoadingCurtainBase.IsChunkLoadFailed)
				{
					return false;
				}
				goto IL_0089;
			case 3:
				_003C_003E1__state = -1;
				prologueLoadingCurtain.SetState(LoadingState.Closing);
				_003C_003E2__current = prologueLoadingCurtain.Fadeout();
				_003C_003E1__state = 4;
				return true;
			case 4:
				{
					_003C_003E1__state = -1;
					prologueLoadingCurtain.SetState(LoadingState.Closed);
					return false;
				}
				IL_0089:
				_003C_003E2__current = prologueLoadingCurtain.WaitForTap();
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

	[CompilerGenerated]
	private sealed class _003CShowYearInfo_003Ed__9 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueLoadingCurtain _003C_003E4__this;

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
		public _003CShowYearInfo_003Ed__9(int _003C_003E1__state)
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
			PrologueLoadingCurtain prologueLoadingCurtain = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				prologueLoadingCurtain._downloadWarning.gameObject.SetActive(value: false);
				prologueLoadingCurtain._yearInfo.Parent.gameObject.SetActive(value: true);
				bool isPrologueMode = GameManager.IsPrologueMode;
				prologueLoadingCurtain._yearInfo.Title.text = ((!isPrologueMode) ? T._("미지의 땅") : T._("지구"));
				prologueLoadingCurtain._yearInfo.Year.text = ((!isPrologueMode) ? T._("연도 불명") : ConditionalText.Format(T._("서기 {year}년")));
				_003C_003E2__current = new WaitForSeconds(2f);
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
	private sealed class _003CWaitForTap_003Ed__10 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueLoadingCurtain _003C_003E4__this;

		private float _003CremainTime_003E5__2;

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
		public _003CWaitForTap_003Ed__10(int _003C_003E1__state)
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
			PrologueLoadingCurtain prologueLoadingCurtain = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				prologueLoadingCurtain._isTap = false;
				_003CremainTime_003E5__2 = 3f;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (_003CremainTime_003E5__2 > 0f && !prologueLoadingCurtain._isTap)
			{
				_003CremainTime_003E5__2 -= Time.deltaTime;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
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
	private sealed class _003CWarnAboutDataNetwork_003Ed__8 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueLoadingCurtain _003C_003E4__this;

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
		public _003CWarnAboutDataNetwork_003Ed__8(int _003C_003E1__state)
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
			PrologueLoadingCurtain prologueLoadingCurtain = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				prologueLoadingCurtain._downloadWarning.gameObject.SetActive(value: true);
				prologueLoadingCurtain._yearInfo.Parent.gameObject.SetActive(value: false);
				_003C_003E2__current = new WaitForSeconds(2f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = prologueLoadingCurtain.WaitForTap();
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				break;
			case 3:
				_003C_003E1__state = -1;
				break;
			}
			if (prologueLoadingCurtain._downloadWarning.alpha > 0f)
			{
				prologueLoadingCurtain._downloadWarning.alpha -= Time.deltaTime;
				_003C_003E2__current = null;
				_003C_003E1__state = 3;
				return true;
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

	[SerializeField]
	private YearInfo _yearInfo;

	[SerializeField]
	private UIWidget _downloadWarning;

	private bool _isTap;

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouchScreen));
		_isTap = false;
		StartCoroutine(CoShowRoutine());
		SetState(LoadingState.Open);
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouchScreen));
	}

	private void OnTouchScreen(GameObject obj, bool press)
	{
		_isTap = true;
	}

	private IEnumerator CoShowRoutine()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoShowRoutine_003Ed__7(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator WarnAboutDataNetwork()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWarnAboutDataNetwork_003Ed__8(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator ShowYearInfo()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CShowYearInfo_003Ed__9(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator WaitForTap()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWaitForTap_003Ed__10(0)
		{
			_003C_003E4__this = this
		};
	}
}
