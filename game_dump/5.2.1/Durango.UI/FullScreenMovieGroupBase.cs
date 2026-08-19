using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.System.Config;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class FullScreenMovieGroupBase : UIBase
{
	[CompilerGenerated]
	private sealed class _003CCoEnd_003Ed__19 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public FullScreenMovieGroupBase _003C_003E4__this;

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
		public _003CCoEnd_003Ed__19(int _003C_003E1__state)
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
			FullScreenMovieGroupBase fullScreenMovieGroupBase = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForEndOfFrame();
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (fullScreenMovieGroupBase._once)
				{
					fullScreenMovieGroupBase._mediaPlayer.Destroy();
				}
				fullScreenMovieGroupBase._finished = true;
				fullScreenMovieGroupBase.Close();
				if (fullScreenMovieGroupBase._onFinished != null)
				{
					fullScreenMovieGroupBase._onFinished();
					fullScreenMovieGroupBase._onFinished = null;
				}
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

	private const string HideKey = "FullScreenMovieGroup";

	[SerializeField]
	protected UILabel _skipLabel;

	[SerializeField]
	protected TweenAlpha _skipLabelTweener;

	[SerializeField]
	protected TweenerPlayer _skipHoldGauge;

	protected float SkipLabelFadeTime = -1f;

	[SerializeField]
	private GameObject _loadingIcon;

	[SerializeField]
	private MediaPlayerCtrl _mediaPlayer;

	[SerializeField]
	private GameObject _back;

	private bool _holdingPress;

	private bool _finished;

	private bool _once;

	private Action _onFinished;

	protected virtual void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_back);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPressBack));
		base.TryClose();
	}

	protected virtual void OnPressBack(GameObject go, bool press)
	{
		if (press)
		{
			if (_skipLabel.alpha > 0f && _skipHoldGauge != null && !_skipHoldGauge.gameObject.activeSelf && !_holdingPress)
			{
				_skipHoldGauge.gameObject.SetActive(value: true);
				Vector2 vector = NGUIMath.ScreenToPixels(UICamera.lastEventPosition, base.transform);
				_skipHoldGauge.transform.localPosition = vector;
				_skipHoldGauge.Play(Stop);
				_holdingPress = true;
			}
		}
		else if (_holdingPress)
		{
			_skipHoldGauge.gameObject.SetActive(value: false);
			_holdingPress = false;
		}
		if (press)
		{
			SkipLabelFadeTime = -1f;
		}
		else
		{
			PlayLabelTween();
		}
	}

	protected void Update()
	{
		if (_mediaPlayer.GetCurrentState() != 0)
		{
			_loadingIcon.SetActive(value: false);
		}
		if (Time.time >= SkipLabelFadeTime && SkipLabelFadeTime >= 0f)
		{
			_skipLabelTweener.from = _skipLabel.alpha;
			_skipLabelTweener.to = 0f;
			_skipLabelTweener.style = UITweener.Style.Once;
			_skipLabelTweener.onFinished.Clear();
			_skipLabelTweener.PlayForward();
			SkipLabelFadeTime = -1f;
		}
	}

	protected override bool TryClose()
	{
		if (_finished)
		{
			return base.TryClose();
		}
		return false;
	}

	public static void Play(string url, bool once = false, Action onFinished = null)
	{
		FullScreenMovieGroupBase fullScreenMovieGroupBase = UIManager.FindScript<FullScreenMovieGroupBase>();
		if ((bool)fullScreenMovieGroupBase)
		{
			fullScreenMovieGroupBase.Open(url, once, onFinished);
		}
		else
		{
			onFinished?.Invoke();
		}
	}

	private void Open(string url, bool once, Action onFinished)
	{
		Open();
		_finished = false;
		_once = once;
		_skipLabel.alpha = 0f;
		SkipLabelFadeTime = -1f;
		_skipLabelTweener.enabled = false;
		_skipHoldGauge.gameObject.SetActive(value: false);
		_holdingPress = false;
		base.VisibleController.HideExceptForMe(hide: true, "FullScreenMovieGroup");
		ConfigInstance.MuteAll();
		_mediaPlayer.OnEnd = Stop;
		_mediaPlayer.OnVideoError = MediaPlayer_VideoError;
		_onFinished = onFinished;
		_mediaPlayer.Load(url);
	}

	protected void Stop()
	{
		_mediaPlayer.OnEnd = null;
		_mediaPlayer.UnLoad();
		ConfigInstance.UnMuteAll();
		base.VisibleController.HideExceptForMe(hide: false, "FullScreenMovieGroup");
		StartCoroutine(CoEnd());
	}

	private IEnumerator CoEnd()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoEnd_003Ed__19(0)
		{
			_003C_003E4__this = this
		};
	}

	private void MediaPlayer_VideoError(MediaPlayerCtrl.MEDIAPLAYER_ERROR mediaplayerError, MediaPlayerCtrl.MEDIAPLAYER_ERROR error)
	{
		Stop();
	}

	protected void PlayLabelTween()
	{
		_skipLabelTweener.from = 0f;
		_skipLabelTweener.to = 1f;
		_skipLabelTweener.style = UITweener.Style.Once;
		_skipLabelTweener.SetOnFinished(SkipLabelTweener_OnFinished);
		_skipLabelTweener.PlayForward();
		_skipLabelTweener.ResetToBeginning();
		SkipLabelFadeTime = Time.time + 6f;
	}

	private void SkipLabelTweener_OnFinished()
	{
		_skipLabelTweener.from = 0.4f;
		_skipLabelTweener.to = 1f;
		_skipLabelTweener.style = UITweener.Style.PingPong;
		_skipLabelTweener.onFinished.Clear();
		_skipLabelTweener.PlayForward();
	}
}
