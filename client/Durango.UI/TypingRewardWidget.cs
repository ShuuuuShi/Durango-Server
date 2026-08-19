using Durango.Render.Particle;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class TypingRewardWidget : AlarmRewardWidget
{
	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private ParticleType _particle;

	[SerializeField]
	private float _typingSpeed;

	[SerializeField]
	private float _scrollUpSpeed;

	[SerializeField]
	private float _hideDelay;

	private TypeWriterEffect _typeWriter;

	private TweenPosition _positionTweener;

	private AnimationWidget _animWidget;

	private float _hideAt;

	private bool _isShow;

	protected override void OnInit()
	{
		base.OnInit();
		_animWidget = GetComponent<AnimationWidget>();
		_positionTweener = _widget.gameObject.AddMissingComponent<TweenPosition>();
		_typeWriter = _subLabel.gameObject.AddMissingComponent<TypeWriterEffect>();
		_typeWriter.SetInterval(_typingSpeed);
		_typeWriter.Finished += OnFinishTypeWriter;
		ParticleManager.Cache(_particle);
	}

	protected override void UpdateLayout()
	{
		base.UpdateLayout();
		Vector2 printedSize = _mainLabel.printedSize;
		Vector2 printedSize2 = _subLabel.printedSize;
		int w = (int)(Mathf.Max(printedSize.x, printedSize2.x) + _mainLabel.transform.localPosition.x * 2f);
		int h = (int)(_mainLabel.GetPosition(0f, 1f).y - _subLabel.GetPosition(0f, 0f).y);
		_widget.SetDimensions(w, h);
		UIUtility.UpdateAnchors(_widget.transform);
	}

	protected override void Play()
	{
		base.Play();
		_typeWriter.Reset();
		_typeWriter.enabled = true;
		_typeWriter.SetInterval(_typingSpeed);
		_isShow = true;
		_hideAt = 0f;
		_animWidget.Widget.alpha = 0f;
		_animWidget.Alpha = 1f;
		_positionTweener.from = Vector3.zero;
		_positionTweener.to = _widget.height * Vector3.up * 0.5f;
		_positionTweener.duration = 0.2f;
		_positionTweener.tweenFactor = 0f;
		_positionTweener.enabled = true;
		ParticleManager.EmitFollow(_particle, Vector3.left * 300f, Quaternion.identity, base.transform);
	}

	private void Hide()
	{
		_isShow = false;
		_animWidget.Alpha = 0f;
	}

	private void Update()
	{
		if (!_positionTweener.enabled)
		{
			_widget.transform.localPosition += Vector3.up * _scrollUpSpeed * Time.deltaTime;
		}
		if (_isShow && _hideAt > 0f && _hideAt < Time.time)
		{
			Hide();
		}
	}

	private void OnFinishTypeWriter()
	{
		_hideAt = Time.time + _hideDelay;
	}
}
