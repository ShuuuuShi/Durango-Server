using System;
using System.Collections;
using Durango.Render.Screen;
using UnityEngine;

namespace Durango.UI;

public class TransitionCurtain : LoadingCurtainBase
{
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
		SetState(LoadingState.Open);
		base.Widget.alpha = 0f;
		while (_transitionCurtain.mainTexture == null)
		{
			yield return null;
		}
		Duration = fadeIn;
		yield return Fadein();
		callback?.Invoke();
		Duration = fadeOut;
		SetState(LoadingState.Closing);
		yield return Fadeout();
		SetState(LoadingState.Closed);
		_transitionCurtain.mainTexture = null;
	}
}
