using System;
using System.Collections;
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
		_loadingIcon.gameObject.SetActive(value: true);
		yield return WaitForChunkLoading();
		if (!LoadingCurtainBase.IsChunkLoadFailed)
		{
			_loadingIcon.gameObject.SetActive(value: false);
			SetState(LoadingState.Closing);
			yield return Fadeout();
			SetState(LoadingState.Closed);
		}
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
