using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render.Screen;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ReconnectLoadingCurtain : LoadingCurtainBase
{
	[Serializable]
	private struct StatusInfo
	{
		public UIWidget Parent;

		public UISprite Bg;

		public UILabel Label;
	}

	[CompilerGenerated]
	private sealed class _003CCoShowRoutine_003Ed__7 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ReconnectLoadingCurtain _003C_003E4__this;

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
			ReconnectLoadingCurtain reconnectLoadingCurtain = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				reconnectLoadingCurtain._loadingIcon.gameObject.SetActive(value: true);
				_003C_003E2__current = reconnectLoadingCurtain.WaitForChunkLoading();
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (!LoadingCurtainBase.IsChunkLoadFailed)
				{
					reconnectLoadingCurtain._loadingIcon.gameObject.SetActive(value: false);
					reconnectLoadingCurtain.SetState(LoadingState.Closing);
					_003C_003E2__current = reconnectLoadingCurtain.Fadeout();
					_003C_003E1__state = 2;
					return true;
				}
				break;
			case 2:
				_003C_003E1__state = -1;
				reconnectLoadingCurtain.SetState(LoadingState.Closed);
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

	[SerializeField]
	private UITexture _captureTexture;

	[SerializeField]
	private StatusInfo _statusBar;

	[SerializeField]
	private GameObject _loadingIcon;

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
		_statusBar.Bg.color = PresetColor.TryConnectColor;
		SetStatusBar(T._("게임 서버와 연결 중 입니다"), PresetColor.ConnectingColor, tween: true);
	}

	private void OnDisable()
	{
		_captureTexture.mainTexture = null;
	}

	public void Connected()
	{
		SetStatusBar(T._("게임 서버와 연결 되었습니다"), PresetColor.ConnectedColor, tween: true);
		StartCoroutine(CoShowRoutine());
	}

	private IEnumerator CoShowRoutine()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoShowRoutine_003Ed__7(0)
		{
			_003C_003E4__this = this
		};
	}

	private void SetStatusBar(string text, Color color, bool tween)
	{
		_statusBar.Label.text = text;
		if (tween)
		{
			TweenColor.Begin(_statusBar.Bg.gameObject, 0.5f, color);
		}
		else
		{
			_statusBar.Bg.color = color;
		}
	}
}
