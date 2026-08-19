using System;
using UnityEngine;

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

	private Transform _transform;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public Transform Target { get; private set; }

	public Vector3 Offset { get; private set; }

	public string Sound { get; private set; }

	private void Awake()
	{
		_transform = ((Component)this).transform;
	}

	private void Update()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Target != (Object)null)
		{
			_transform.position = Target.position + Offset;
		}
		if (_hideAt < Time.time)
		{
			Hide();
		}
	}

	public void Set(Transform target, Vector3 offset, string sprite, string sound)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Target = target;
		Offset = offset;
		Sound = sound;
		_sprite.spriteName = sprite;
		UIUtility.ResizeToSquare(_sprite, Widget.width);
	}

	public void Show(float duration)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		Widget.alpha = 1f;
		_hideAt = Time.time + duration;
		SoundManager.Play(Sound, Target.localPosition + Offset);
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
