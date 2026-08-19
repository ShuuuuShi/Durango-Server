using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render.Screen;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class TeleportLoadingCurtain : LoadingCurtainBase
{
	[CompilerGenerated]
	private sealed class _003CCoShowRoutine_003Ed__5 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TeleportLoadingCurtain _003C_003E4__this;

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
		public _003CCoShowRoutine_003Ed__5(int _003C_003E1__state)
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
			TeleportLoadingCurtain teleportLoadingCurtain = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (teleportLoadingCurtain._captureTexture.mainTexture == null)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_005b;
			case 1:
				_003C_003E1__state = -1;
				goto IL_005b;
			case 2:
				_003C_003E1__state = -1;
				goto IL_00ab;
			case 3:
				_003C_003E1__state = -1;
				goto IL_00dd;
			case 4:
				{
					_003C_003E1__state = -1;
					teleportLoadingCurtain.SetState(LoadingState.Closed);
					teleportLoadingCurtain._captureTexture.mainTexture = null;
					return false;
				}
				IL_00ab:
				if (!Singleton<TerrainBase>.Instance().IsChunkLoading && _003CremainTime_003E5__2 > 0f)
				{
					_003CremainTime_003E5__2 -= Time.deltaTime;
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				goto IL_00dd;
				IL_005b:
				if (teleportLoadingCurtain._onTeleport != null)
				{
					teleportLoadingCurtain._onTeleport();
				}
				teleportLoadingCurtain._onTeleport = null;
				_003CremainTime_003E5__2 = 1f;
				goto IL_00ab;
				IL_00dd:
				if (Singleton<TerrainBase>.Instance().IsChunkLoading)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 3;
					return true;
				}
				teleportLoadingCurtain.SetState(LoadingState.Closing);
				_003C_003E2__current = teleportLoadingCurtain.Fadeout();
				_003C_003E1__state = 4;
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
	private UITexture _captureTexture;

	private Action _onTeleport;

	private void OnEnable()
	{
		_captureTexture.mainTexture = null;
		base.Widget.alpha = 0f;
		ScreenCapture.CaptureOption option = default(ScreenCapture.CaptureOption);
		option.OnResult = delegate(Texture2D tex)
		{
			base.Widget.alpha = 1f;
			_captureTexture.mainTexture = tex;
		};
		ScreenCapture.Capture(option);
		SetState(LoadingState.Open);
		StartCoroutine(CoShowRoutine());
	}

	private void OnDisable()
	{
		_captureTexture.mainTexture = null;
	}

	public void SetReadyToTeleport(Action onTeleport)
	{
		_onTeleport = onTeleport;
	}

	private IEnumerator CoShowRoutine()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoShowRoutine_003Ed__5(0)
		{
			_003C_003E4__this = this
		};
	}
}
