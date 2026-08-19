using System;
using System.Collections;
using Durango.Render.Screen;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class TeleportLoadingCurtain : LoadingCurtainBase
{
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
		if (_captureTexture.mainTexture == null)
		{
			yield return null;
		}
		if (_onTeleport != null)
		{
			_onTeleport();
		}
		_onTeleport = null;
		float remainTime = 1f;
		while (!Singleton<TerrainBase>.Instance().IsChunkLoading && remainTime > 0f)
		{
			remainTime -= Time.deltaTime;
			yield return null;
		}
		while (Singleton<TerrainBase>.Instance().IsChunkLoading)
		{
			yield return null;
		}
		SetState(LoadingState.Closing);
		yield return Fadeout();
		SetState(LoadingState.Closed);
		_captureTexture.mainTexture = null;
	}
}
