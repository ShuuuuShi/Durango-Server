using System;
using Holoville.HOTween;
using Shared.Battle;
using UnityEngine;

public class ActionButton : MonoBehaviour
{
	public static Color InvalidColor = new Color(1f, 1f, 1f, 0f);

	private UIEventListener _eventListener;

	private UIWidget _widget;

	private float _alpha;

	private Color _borderColor;

	private Color _iconColor;

	private Color _bgColor;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _border;

	[SerializeField]
	private UISprite _cooltime;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UISprite _attackCoolTimeGauge;

	[SerializeField]
	private int _iconSize = 64;

	[SerializeField]
	private GameObject _sealedIcon;

	[SerializeField]
	private GameObject _cooltimeEndEffect;

	[SerializeField]
	private GameObject _executeEffect;

	[SerializeField]
	private GameObject _reservedEffect;

	[SerializeField]
	private GameObject _autoActionEffect;

	private UISprite _autoActionEffectBg;

	[SerializeField]
	private float _colorTweenDuration = 0.3f;

	[SerializeField]
	private Color _notLearnedIconColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	private Color _notLearnedBorderColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	private Color _notLearnedBgColor = new Color(0f, 0f, 0f, 1f);

	[SerializeField]
	private Color _grayedIconColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	private Color _grayedBorderColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	private Color _grayedBgColor = new Color(0f, 0f, 0f, 1f);

	[SerializeField]
	private Color _cooltimedIconColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	private Color _cooltimedBorderColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	private Color _cooltimedBgColor = new Color(0f, 0f, 0f, 1f);

	[SerializeField]
	private Color _activatedIconColor = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private Color _activatedBorderColor = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private Color _activatedBgColor = new Color(0f, 0f, 0f, 1f);

	[SerializeField]
	private Color _reservedIconColor = new Color(1f, 1f, 0.5f, 0.5f);

	[SerializeField]
	private Color _reservedBorderColor = new Color(1f, 1f, 0.5f, 0.5f);

	[SerializeField]
	private Color _reservedBgColor = new Color(0f, 0f, 0f, 1f);

	[SerializeField]
	private Color _waitingIconColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	private Color _waitingBorderColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	private Color _waitingBgColor = new Color(0f, 0f, 0f, 1f);

	[SerializeField]
	private Color _normalActionColor = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private Color _guardActionColor = new Color(0.8f, 1f, 0.8f, 1f);

	[SerializeField]
	private Color _activeActionColor = new Color(1f, 1f, 0.8f, 1f);

	[SerializeField]
	private Color _tackleActionColor = new Color(0.8f, 0.8f, 1f, 1f);

	[SerializeField]
	private Color _additionalActionColor = new Color(0.8f, 0.8f, 1f, 1f);

	[SerializeField]
	private GlitteringDots _dotsEffect;

	private int _enableFrame;

	private ActionState _prevTickState;

	private double _deactiveTimeSince;

	private TweenScale _scaleTweener;

	private TweenAlpha _alphaTweener;

	private ReservedTweenScale _reservedScaleTweener;

	private ActionState _curState = ActionState.Invalid;

	private bool _isCooltime;

	private double _attackCoolTimeBeginAt = -1.0;

	private double _attackCoolTimeEndAt = -1.0;

	private bool _reserved;

	private float _autoActionRotationIconPeriod = 3f;

	private TweenColor _iconTweenColor;

	private TweenColor _borderTweenColor;

	private TweenColor _bgTweenColor;

	private PressColorChange _pressColorChange;

	private TweenParms borderColorParms = new TweenParms();

	private TweenParms iconColorParms = new TweenParms();

	private TweenParms bgColorParms = new TweenParms();

	[SerializeField]
	private string _timerGaugeIcon;

	[SerializeField]
	private Vector3 _timerGaugePosOffset = new Vector3(0f, -50f, 0f);

	public UIEventListener Listener
	{
		get
		{
			if ((Object)(object)_eventListener == (Object)null)
			{
				_eventListener = UIEventListener.Get(((Component)this).gameObject);
			}
			return _eventListener;
		}
	}

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).gameObject.GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public float Alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			_alpha = value;
			WidgetAlphaTweener.to = _alpha;
			if (!((Behaviour)WidgetAlphaTweener).enabled || !(WidgetAlphaTweener.amountPerDelta > 0f))
			{
				((Behaviour)WidgetAlphaTweener).enabled = false;
				Widget.alpha = _alpha;
			}
		}
	}

	private Color ActionGroupColor
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			if (_curState == ActionState.NotLearned)
			{
				return Color.white;
			}
			switch (ActionGroup)
			{
			case ActionGroup.ClientSided:
			case ActionGroup.Normal:
			case ActionGroup.Counter:
				return _normalActionColor;
			case ActionGroup.Guard:
				return _guardActionColor;
			case ActionGroup.ActiveAction:
				return _activeActionColor;
			case ActionGroup.Tackle:
				return _tackleActionColor;
			case ActionGroup.Additional:
				return _additionalActionColor;
			default:
				return Color.white;
			}
		}
	}

	public Color BorderColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _borderColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			_borderColor = value;
			borderColorParms.NewProp("color", (object)value);
			borderColorParms.Ease((EaseType)0);
			HOTween.To((object)BorderSprite, _colorTweenDuration, borderColorParms);
		}
	}

	public Color IconColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _iconColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			_iconColor = value;
			iconColorParms.NewProp("color", (object)(value * ActionGroupColor));
			iconColorParms.Ease((EaseType)0);
			HOTween.To((object)IconSprite, _colorTweenDuration, iconColorParms);
			if ((Object)(object)_pressColorChange != (Object)null)
			{
				_pressColorChange.SetOriginColor(value * ActionGroupColor);
			}
		}
	}

	public Color BgColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _bgColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			_bgColor = value;
			bgColorParms.NewProp("color", (object)value);
			bgColorParms.Ease((EaseType)0);
			HOTween.To((object)BgSprite, _colorTweenDuration, bgColorParms);
		}
	}

	private double DeactiveTimeSince
	{
		get
		{
			if (_deactiveTimeSince < 0.0)
			{
				_deactiveTimeSince = GetServerTime();
			}
			return _deactiveTimeSince;
		}
	}

	private double DeactiveTimeUntil { get; set; }

	public string Text
	{
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				((Component)_text).gameObject.SetActive(false);
				return;
			}
			((Component)_text).gameObject.SetActive(true);
			((Component)IconSprite).gameObject.SetActive(false);
			_text.text = value;
		}
	}

	public string IconName
	{
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				((Component)IconSprite).gameObject.SetActive(false);
				return;
			}
			((Component)_text).gameObject.SetActive(false);
			((Component)IconSprite).gameObject.SetActive(true);
			IconSprite.spriteName = value;
			UISpriteData atlasSprite = IconSprite.GetAtlasSprite();
			if (atlasSprite == null)
			{
				Debug.LogError((object)("아이콘이 이름이 잘못 되었습니다: " + value));
			}
			else
			{
				UIUtility.ResizeToSquare(IconSprite, _iconSize);
			}
		}
	}

	public TweenScale AppearEffectScaleTweener
	{
		get
		{
			if ((Object)(object)_scaleTweener == (Object)null)
			{
				_scaleTweener = ((Component)this).GetComponent<TweenScale>();
				_scaleTweener.AddOnFinished(OnScaleTweenerFinished);
			}
			return _scaleTweener;
		}
	}

	public ReservedTweenScale ReservedScaleTweener
	{
		get
		{
			if ((Object)(object)_reservedScaleTweener == (Object)null)
			{
				_reservedScaleTweener = ((Component)this).GetComponent<ReservedTweenScale>();
			}
			return _reservedScaleTweener;
		}
	}

	public TweenAlpha WidgetAlphaTweener
	{
		get
		{
			if ((Object)(object)_alphaTweener == (Object)null)
			{
				_alphaTweener = ((Component)this).GetComponent<TweenAlpha>();
			}
			return _alphaTweener;
		}
	}

	public string Id { get; set; }

	public ActionGroup ActionGroup { get; set; }

	public ActionState CurState
	{
		get
		{
			return _curState;
		}
		set
		{
			ActionState curState = _curState;
			_curState = value;
			if (curState != _curState)
			{
				UpdateButtonState();
				OnChangeState(_curState);
			}
		}
	}

	public bool IsClickable => CurState == ActionState.Activated && !Reserved;

	public UISprite BorderSprite => _border;

	public UISprite IconSprite => _icon;

	public UISprite BgSprite => _background;

	public Vector3 TimerGaugePosOffset => _timerGaugePosOffset;

	public string TimerGaugeIcon => _timerGaugeIcon;

	public bool Reserved => _reserved;

	private UISprite AutoActionEffectBg
	{
		get
		{
			if ((Object)(object)_autoActionEffectBg == (Object)null)
			{
				_autoActionEffectBg = _autoActionEffect.GetComponentInChildren<UISprite>();
			}
			return _autoActionEffectBg;
		}
	}

	public bool IsAutoAction()
	{
		return ActionGroup == ActionGroup.Normal;
	}

	private void UpdateButtonState()
	{
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		switch (_curState)
		{
		case ActionState.Hidden:
			flag = false;
			break;
		case ActionState.Invalid:
			flag = false;
			break;
		case ActionState.Grayed:
			IconColor = _grayedIconColor;
			BorderColor = _grayedBorderColor;
			BgColor = _grayedBgColor;
			_sealedIcon.gameObject.SetActive(false);
			Alpha = 1f;
			break;
		case ActionState.Cooling:
			IconColor = ((!IsAutoAction()) ? _cooltimedIconColor : _activatedIconColor);
			BorderColor = _cooltimedBorderColor;
			BgColor = _cooltimedBgColor;
			_sealedIcon.gameObject.SetActive(false);
			Alpha = 1f;
			break;
		case ActionState.Activated:
			IconColor = _activatedIconColor;
			BorderColor = _activatedBorderColor;
			BgColor = _activatedBgColor;
			_sealedIcon.gameObject.SetActive(false);
			Alpha = 1f;
			break;
		case ActionState.NotLearned:
			IconColor = _notLearnedIconColor;
			BorderColor = _notLearnedBorderColor;
			BgColor = _notLearnedBgColor;
			_sealedIcon.gameObject.SetActive(true);
			Alpha = 1f;
			break;
		}
		_autoActionEffect.SetActive(flag && IsAutoAction());
		((Component)this).gameObject.SetActive(flag);
		((Component)_attackCoolTimeGauge).gameObject.SetActive(IsAutoAction());
	}

	public void PlayExecutionEffect()
	{
		if (Object.op_Implicit((Object)(object)_executeEffect))
		{
			_executeEffect.SetActive(true);
		}
	}

	public void SetDeactiveTime(double since, double until)
	{
		double serverTime = GetServerTime();
		_isCooltime = since <= serverTime && serverTime <= until;
		_deactiveTimeSince = since;
		DeactiveTimeUntil = until;
	}

	private void OnScaleTweenerFinished()
	{
		if (AppearEffectScaleTweener.tweenFactor < 1f)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void Hide()
	{
		Id = string.Empty;
		Listener.onClick = null;
		if (NGUITools.GetActive((Behaviour)(object)this) && AppearEffectScaleTweener.tweenFactor > 0f)
		{
			AppearEffectScaleTweener.PlayReverse();
			WidgetAlphaTweener.PlayReverse();
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void OnChangeState(ActionState current)
	{
		if (((Component)this).gameObject.activeSelf && _enableFrame != Time.frameCount)
		{
			if (_prevTickState != ActionState.Activated && current == ActionState.Activated && Object.op_Implicit((Object)(object)_cooltimeEndEffect))
			{
				_cooltimeEndEffect.SetActive(true);
			}
			if (_prevTickState == ActionState.Activated && current == ActionState.Cooling)
			{
				PlayExecutionEffect();
			}
		}
	}

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)_cooltimeEndEffect))
		{
			_cooltimeEndEffect.gameObject.SetActive(false);
			UIExtendEventListener uIExtendEventListener = UIExtendEventListener.Get(_cooltimeEndEffect);
			uIExtendEventListener.onEnable = (UIEventListener.VoidDelegate)Delegate.Combine(uIExtendEventListener.onEnable, (UIEventListener.VoidDelegate)delegate
			{
				_dotsEffect.Play();
			});
		}
		if (Object.op_Implicit((Object)(object)_executeEffect))
		{
			_executeEffect.gameObject.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)_reservedEffect))
		{
			_reservedEffect.gameObject.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)_autoActionEffect))
		{
			_autoActionEffect.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		_enableFrame = Time.frameCount;
		_pressColorChange = ((Component)this).GetComponent<PressColorChange>();
		if ((Object)(object)_pressColorChange != (Object)null)
		{
			_pressColorChange.Press(press: false);
		}
	}

	private void OnDisable()
	{
		_curState = ActionState.Invalid;
	}

	private double GetServerTime()
	{
		return Connections.Frontend.GetPredictedServerTime();
	}

	public void PlayOrStopReservedAnim(bool reserved)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (reserved)
		{
			ReservedScaleTweener.PlayForward();
			IconColor = ((!IsAutoAction()) ? _reservedIconColor : _activatedIconColor);
			BorderColor = _reservedBorderColor;
			BgColor = _reservedBgColor;
			if (IsAutoAction())
			{
				AutoActionEffectBg.color = _reservedBorderColor;
				AutoActionEffectBg.alpha = 0.5f;
			}
			if (Object.op_Implicit((Object)(object)_reservedEffect))
			{
				_reservedEffect.SetActive(true);
				GlitteringDots componentInChildren = _reservedEffect.GetComponentInChildren<GlitteringDots>();
				if (Object.op_Implicit((Object)(object)componentInChildren))
				{
					componentInChildren.Play();
				}
			}
		}
		else
		{
			ReservedScaleTweener.PlayReverse();
			UpdateButtonState();
			if (IsAutoAction())
			{
				AutoActionEffectBg.color = _activatedBorderColor;
				AutoActionEffectBg.alpha = 0.5f;
			}
			if (Object.op_Implicit((Object)(object)_reservedEffect))
			{
				_reservedEffect.SetActive(false);
			}
		}
		_reserved = reserved;
	}

	private void Update()
	{
		if (CurState == ActionState.NotLearned)
		{
			((Component)_cooltime).gameObject.SetActive(false);
		}
		else
		{
			double serverTime = GetServerTime();
			double deactiveTimeSince = DeactiveTimeSince;
			double deactiveTimeUntil = DeactiveTimeUntil;
			float num;
			if (CurState == ActionState.Grayed)
			{
				num = 0f;
			}
			else if (!(deactiveTimeUntil <= serverTime))
			{
				num = ((!(deactiveTimeSince >= serverTime)) ? ((float)(serverTime - deactiveTimeSince) / (float)(deactiveTimeUntil - deactiveTimeSince)) : 0f);
			}
			else
			{
				if (_isCooltime)
				{
					_isCooltime = false;
					CurState = ActionState.Activated;
				}
				num = 1f;
			}
			((Component)_cooltime).gameObject.SetActive(num < 1f);
			_cooltime.fillAmount = num;
		}
		if (IsAutoAction())
		{
			ProcessAttackCoolTime();
		}
		_prevTickState = CurState;
	}

	public bool IsAttackCoolingDown()
	{
		return _attackCoolTimeBeginAt > 0.0 && _attackCoolTimeEndAt > 0.0;
	}

	public void UpdateAttackCoolTime(double until)
	{
		double bufferedServerTime_Enhanced = Connections.Frontend.GetBufferedServerTime_Enhanced();
		if (_attackCoolTimeBeginAt < 0.0 || bufferedServerTime_Enhanced >= _attackCoolTimeEndAt)
		{
			_attackCoolTimeBeginAt = Connections.Frontend.GetBufferedServerTime_Enhanced();
		}
		_attackCoolTimeEndAt = until;
	}

	private void ProcessAttackCoolTime()
	{
		double bufferedServerTime_Enhanced = Connections.Frontend.GetBufferedServerTime_Enhanced();
		if (_attackCoolTimeBeginAt < 0.0 || _attackCoolTimeEndAt < 0.0 || _attackCoolTimeBeginAt > bufferedServerTime_Enhanced)
		{
			_attackCoolTimeGauge.fillAmount = 0f;
			return;
		}
		float num = (float)(bufferedServerTime_Enhanced - _attackCoolTimeBeginAt) / (float)(_attackCoolTimeEndAt - _attackCoolTimeBeginAt);
		if (num > 1.1f)
		{
			_attackCoolTimeBeginAt = -1.0;
			_attackCoolTimeEndAt = -1.0;
			_attackCoolTimeGauge.fillAmount = 0f;
		}
		else
		{
			_attackCoolTimeGauge.fillAmount = Mathf.Clamp01(num);
		}
	}
}
