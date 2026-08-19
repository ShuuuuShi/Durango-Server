using System;
using System.Collections;
using UnityEngine;

public class FullScreenMovieGroup : MonoBehaviour
{
	[SerializeField]
	private GameObject _loadingIcon;

	[SerializeField]
	private GameObject _back;

	[SerializeField]
	private UITexture _movieTexture;

	[SerializeField]
	private UILabel _skipLabel;

	[SerializeField]
	private TweenAlpha _skipLabelTweener;

	[SerializeField]
	private UISprite _skipHoldGauge;

	[SerializeField]
	private MediaPlayerCtrl _mediaPlayer;

	private float _skipLabelFadeTime = -1f;

	private float _skipHoldBeginTime = -1f;

	public event Action Finished;

	private void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_back);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(Back_OnPress));
		_skipLabel.alpha = 0f;
	}

	private void Back_OnPress(GameObject go, bool press)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (press)
		{
			if (_skipLabel.alpha > 0f)
			{
				_skipHoldBeginTime = Time.time;
				((Component)_skipHoldGauge).gameObject.SetActive(true);
				Vector2 val = NGUIMath.ScreenToPixels(UICamera.lastEventPosition, ((Component)this).transform);
				((Component)_skipHoldGauge).transform.localPosition = Vector2.op_Implicit(val);
			}
			_skipLabelFadeTime = -1f;
		}
		else
		{
			_skipLabelTweener.from = 0f;
			_skipLabelTweener.to = 1f;
			_skipLabelTweener.style = UITweener.Style.Once;
			_skipLabelTweener.SetOnFinished(SkipLabelTweener_OnFinished);
			_skipLabelTweener.PlayForward();
			_skipLabelTweener.ResetToBeginning();
			_skipLabelFadeTime = Time.time + 6f;
			_skipHoldBeginTime = -1f;
			((Component)_skipHoldGauge).gameObject.SetActive(false);
		}
	}

	private void SkipLabelTweener_OnFinished()
	{
		_skipLabelTweener.from = 0.4f;
		_skipLabelTweener.to = 1f;
		_skipLabelTweener.style = UITweener.Style.PingPong;
		_skipLabelTweener.onFinished.Clear();
		_skipLabelTweener.PlayForward();
	}

	private void LateUpdate()
	{
		_movieTexture.mainTexture = (Texture)(object)_mediaPlayer.GetVideoTexture();
		if (_skipHoldBeginTime >= 0f)
		{
			float num = Time.time - _skipHoldBeginTime;
			float num2 = Mathf.Min(1f, num / 2f);
			_skipHoldGauge.fillAmount = num2;
			if (num2 >= 1f)
			{
				Stop();
				_skipHoldBeginTime = -1f;
			}
		}
		if (Time.time >= _skipLabelFadeTime && _skipLabelFadeTime >= 0f)
		{
			_skipLabelTweener.from = _skipLabel.alpha;
			_skipLabelTweener.to = 0f;
			_skipLabelTweener.style = UITweener.Style.Once;
			_skipLabelTweener.onFinished.Clear();
			_skipLabelTweener.PlayForward();
			_skipLabelFadeTime = -1f;
		}
	}

	private void MediaPlayer_OnReady()
	{
		_loadingIcon.SetActive(false);
	}

	private void MediaPlayer_OnEnd()
	{
		((MonoBehaviour)this).StartCoroutine(CoEnd());
	}

	private IEnumerator CoEnd()
	{
		yield return (object)new WaitForEndOfFrame();
		_mediaPlayer.UnLoad();
		((Component)this).gameObject.SetActive(false);
		if (this.Finished != null)
		{
			this.Finished();
		}
	}

	private void MediaPlayer_VideoError(MediaPlayerCtrl.MEDIAPLAYER_ERROR mediaplayerError, MediaPlayerCtrl.MEDIAPLAYER_ERROR error)
	{
		Stop();
	}

	public void Play(string url)
	{
		((Component)this).gameObject.SetActive(true);
		_mediaPlayer.OnReady = MediaPlayer_OnReady;
		_mediaPlayer.OnEnd = MediaPlayer_OnEnd;
		_mediaPlayer.OnVideoError = MediaPlayer_VideoError;
		_mediaPlayer.Load(url);
	}

	public void Stop()
	{
		_mediaPlayer.Stop();
		MediaPlayer_OnEnd();
	}
}
