using System;
using AnimationOrTween;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class TitleMessageBox_PC : TitleMessageBoxBase
{
	[SerializeField]
	private TweenAlpha _tween;

	[SerializeField]
	private BlurTexture _blur;

	protected override void Awake()
	{
		base.Awake();
		_blur.Init();
	}

	public override void Show(string title, string message, Action onClick, Action onCancel = null, string okButtonLabel = null, string cancelButtonLabel = null)
	{
		base.Show(title, message, onClick, onCancel, okButtonLabel, cancelButtonLabel);
		_blur.Show(show: true);
		_tween.AddOnFinished(delegate
		{
			if (_tween.direction == Direction.Reverse)
			{
				base.gameObject.SetActive(value: false);
				_blur.Show(show: false);
			}
		});
		_tween.PlayForward();
	}

	private void Update()
	{
		if (_blur != null && _tween != null && _tween.enabled)
		{
			_blur.SetParameters(Mathf.Lerp(0f, 3f, _tween.value), Mathf.Lerp(0f, 0.3f, _tween.value), _blur.TintColor);
		}
	}

	public override void Close()
	{
		_tween.PlayReverse();
	}
}
