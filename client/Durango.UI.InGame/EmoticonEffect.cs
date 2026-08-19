using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.InGame;

public class EmoticonEffect : MonoBehaviour
{
	public Action<EmoticonEffect> Disabled;

	[SerializeField]
	private UISprite _sprite;

	[SerializeField]
	private TweenerPlayer _tweenPlayer;

	private UIWidget _widget;

	private AnimationWidget _animWidget;

	private float _hideAt;

	private Vector3 _offset;

	private string _soundSwitchName;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public AnimationWidget AnimWidget
	{
		get
		{
			if (_animWidget == null)
			{
				_animWidget = GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public Transform Target { get; private set; }

	private void Update()
	{
		if (Target != null)
		{
			base.transform.position = Target.position + _offset;
		}
		if (_hideAt < Time.time)
		{
			Hide();
		}
	}

	public void Set(Transform target, Vector3 offset, string sprite, string soundSwitchName)
	{
		Target = target;
		_offset = offset;
		_soundSwitchName = soundSwitchName;
		_sprite.spriteName = sprite;
		UIUtility.ResizeToSquare(_sprite, Widget.width);
	}

	public void Show(float duration)
	{
		base.gameObject.SetActive(value: true);
		Widget.alpha = 1f;
		_hideAt = Time.time + duration;
		SoundManager.PlayEvent("emoticon", SoundPosition.Fix(Target.position + _offset), SoundSwitch.Set("emoticon", _soundSwitchName));
		_tweenPlayer.Play();
	}

	public void Hide()
	{
		_hideAt = float.MaxValue;
		AnimWidget.Alpha = 0f;
	}

	private void OnDisable()
	{
		if (Disabled != null)
		{
			Disabled(this);
		}
	}
}
