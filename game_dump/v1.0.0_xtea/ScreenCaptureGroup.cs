using L10N;
using UnityEngine;

public class ScreenCaptureGroup : UIBase
{
	[SerializeField]
	private UITexture _renderTexture;

	[SerializeField]
	private AudioClipType _cameraSound;

	[SerializeField]
	private ScreenCapture.EffectEnum _effects;

	private void Awake()
	{
		UIEventListener.Get(((Component)_renderTexture).gameObject).onClick = delegate
		{
			Close();
		};
		SoundManager.Cache(_cameraSound);
		base.OnOpenSucceed += OnOpenSucceeded;
		base.OnCloseSucceed += OnCloseSucceeded;
		base.OnClose();
	}

	private void OnOpenSucceeded()
	{
		ScreenCapture.CaptureOption option = default(ScreenCapture.CaptureOption);
		option.Effect = _effects;
		option.Logo = true;
		option.NoUI = true;
		option.OnResult = SetInstagramShot;
		ScreenCapture.Capture(option);
		SoundManager.Play((string)_cameraSound, loop: false, default(SoundManager.PitchRange));
	}

	private void OnCloseSucceeded()
	{
		UIManager.SystemMsg(T._("스크린샷이 저장되었습니다."), 2f);
	}

	private void SetInstagramShot(Texture2D tex)
	{
		_renderTexture.mainTexture = (Texture)(object)tex;
		ScreenshotManager.SaveImage(tex);
	}
}
