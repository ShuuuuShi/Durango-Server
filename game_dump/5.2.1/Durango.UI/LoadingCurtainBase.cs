using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.System;
using Durango.Terrain;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

public abstract class LoadingCurtainBase : MonoBehaviour
{
	public enum LoadingState
	{
		Open,
		Closing,
		Closed
	}

	[CompilerGenerated]
	private sealed class _003CFadein_003Ed__16 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public LoadingCurtainBase _003C_003E4__this;

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
		public _003CFadein_003Ed__16(int _003C_003E1__state)
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
			LoadingCurtainBase loadingCurtainBase = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CremainTime_003E5__2 = loadingCurtainBase.Duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (_003CremainTime_003E5__2 > 0f)
			{
				_003CremainTime_003E5__2 -= Time.deltaTime;
				loadingCurtainBase.Widget.alpha = Mathf.Clamp01(1f - _003CremainTime_003E5__2 / loadingCurtainBase.Duration);
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
	private sealed class _003CFadeout_003Ed__17 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public LoadingCurtainBase _003C_003E4__this;

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
		public _003CFadeout_003Ed__17(int _003C_003E1__state)
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
			LoadingCurtainBase loadingCurtainBase = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CremainTime_003E5__2 = loadingCurtainBase.Duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (_003CremainTime_003E5__2 > 0f)
			{
				_003CremainTime_003E5__2 -= Time.deltaTime;
				loadingCurtainBase.Widget.alpha = Mathf.Clamp01(_003CremainTime_003E5__2 / loadingCurtainBase.Duration);
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
	private sealed class _003CWaitForChunkLoading_003Ed__15 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private float _003CbeginTime_003E5__2;

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
		public _003CWaitForChunkLoading_003Ed__15(int _003C_003E1__state)
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
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				float num2 = 60f;
				if (Time.realtimeSinceStartup - _003CbeginTime_003E5__2 > num2)
				{
					IsChunkLoadFailed = true;
					string text = ((!TerrainBase.IsPlayerInitialized) ? T._("플레이어 정보를 불러오는데 실패하였습니다.") : T._("지형 정보를 불러오는데 실패하였습니다."));
					string text2 = T._("화면을 터치 후 다시 시도해 주세요.");
					GameManager.LastEvictedMsg = ((!Platform.Instance.UsePCUI) ? (text + "\n" + text2) : text);
					Singleton<GameManager>.Instance().MoveToTitle();
					goto IL_00c3;
				}
			}
			else
			{
				_003C_003E1__state = -1;
				IsChunkLoadFailed = false;
				_003CbeginTime_003E5__2 = Time.realtimeSinceStartup;
			}
			if (!Singleton<TerrainBase>.Instance().IsReady)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			goto IL_00c3;
			IL_00c3:
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

	protected static bool IsChunkLoadFailed;

	private UIWidget _widget;

	protected float Duration = 0.5f;

	public Action<LoadingState> StateChanged { get; set; }

	protected LoadingState State { get; private set; }

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	protected void SetState(LoadingState state)
	{
		State = state;
		if (StateChanged != null)
		{
			StateChanged(state);
		}
	}

	protected IEnumerator WaitForChunkLoading()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWaitForChunkLoading_003Ed__15(0);
	}

	protected IEnumerator Fadein()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFadein_003Ed__16(0)
		{
			_003C_003E4__this = this
		};
	}

	protected IEnumerator Fadeout()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFadeout_003Ed__17(0)
		{
			_003C_003E4__this = this
		};
	}
}
