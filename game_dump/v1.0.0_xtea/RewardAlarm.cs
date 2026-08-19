using System;
using UnityEngine;

public class RewardAlarm : MonoBehaviour
{
	public Action<RewardAlarm> Disabled;

	public Action<RewardAlarm> OnHide;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private UISprite _lineSprite;

	[SerializeField]
	private UISpriteLabel _commentLabel;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private float _typingSpeed;

	[SerializeField]
	private float _minVisibleTime;

	[SerializeField]
	private float _maxVisibleTime;

	[SerializeField]
	private float _scrollUpSpeed;

	private TypeWriterEffect _typeWriter;

	private UIWidget _widget;

	private AnimationWidget _animWidget;

	private TweenPosition _positionTweener;

	private float _hideReadyTime;

	private float _hideAt = -1f;

	private bool _isHiding;

	private bool _isInit;

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

	public bool ReadyToHide => _hideReadyTime > 0f && _hideReadyTime < Time.time;

	public int TitleFontSize
	{
		get
		{
			return _titleLabel.Label.fontSize;
		}
		set
		{
			_titleLabel.Label.fontSize = value;
		}
	}

	public int Priority { get; set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_positionTweener = AnimWidget.GetTweener<TweenPosition>();
			_typeWriter = ((Component)_commentLabel).gameObject.AddComponent<TypeWriterEffect>();
			_typeWriter.TypingSpeed = _typingSpeed;
		}
	}

	public void Set(string title, string comment)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Init();
		_titleLabel.text = title;
		_commentLabel.text = comment;
		if (string.IsNullOrEmpty(comment))
		{
			((Component)_lineSprite).gameObject.SetActive(false);
			Widget.height = _titleLabel.Label.fontSize - (int)(((Component)_titleLabel).transform.localPosition.y + (float)_titleLabel.Label.fontSize) * 2;
		}
		else
		{
			((Component)_lineSprite).gameObject.SetActive(true);
			Widget.height = -(int)((Component)_commentLabel).transform.localPosition.y + _commentLabel.Label.height;
		}
		_typeWriter.Reset();
		((Behaviour)_typeWriter).enabled = true;
		_background.ResetAndUpdateAnchors();
	}

	public void Show()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Show(Vector3.zero);
	}

	public void Show(Vector3 initPos)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		_hideReadyTime = -1f;
		_hideAt = -1f;
		_isHiding = false;
		((Component)this).gameObject.SetActive(true);
		Widget.alpha = 0f;
		AnimWidget.Alpha = 1f;
		((Component)AnimWidget).transform.localPosition = initPos;
		AnimWidget.Position = (float)Widget.height * Vector3.up * 0.5f;
	}

	public void Hide()
	{
		_isHiding = true;
		AnimWidget.Alpha = 0f;
		_hideAt = -1f;
		_hideReadyTime = 0f;
		if (OnHide != null)
		{
			OnHide(this);
		}
	}

	private void Update()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)_positionTweener).enabled)
		{
			Transform transform = ((Component)AnimWidget).transform;
			transform.localPosition += Vector3.up * _scrollUpSpeed * Time.deltaTime;
		}
		if (!_isHiding && !((Behaviour)_typeWriter).enabled)
		{
			float time = Time.time;
			if (_hideAt < 0f)
			{
				_hideAt = time + _maxVisibleTime;
				_hideReadyTime = time + _minVisibleTime;
			}
			else if (time > _hideAt)
			{
				Hide();
			}
		}
	}

	private void OnDisable()
	{
		if (Disabled != null)
		{
			Disabled(this);
		}
	}
}
