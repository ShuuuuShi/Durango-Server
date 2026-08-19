using System.Collections;
using InteractionData;
using L10N;
using UnityEngine;

public class ContextActionButton : MonoBehaviour
{
	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UISprite _cooltime;

	private UITweener[] _effectTweeners;

	private UIWidget _widget;

	private double _deactiveAt;

	private double _reactiveAt;

	private bool _cooltimeRoutineFlag;

	private bool _isShow;

	private bool _isInit;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	public Interaction Action { get; private set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_effectTweeners = ((Component)this).GetComponents<UITweener>();
			Widget.alpha = 0f;
		}
	}

	private void Start()
	{
		Init();
	}

	private void OnDisable()
	{
		_cooltimeRoutineFlag = false;
		_cooltime.fillAmount = 1f;
	}

	public void Show(Interaction value)
	{
		Init();
		Set(value);
		if (!_isShow)
		{
			_isShow = true;
			for (int i = 0; i < _effectTweeners.Length; i++)
			{
				_effectTweeners[i].tweenFactor = 0f;
				_effectTweeners[i].PlayForward();
			}
		}
	}

	public void Hide()
	{
		if (_isShow)
		{
			_isShow = false;
			for (int i = 0; i < _effectTweeners.Length; i++)
			{
				((Behaviour)_effectTweeners[i]).enabled = false;
			}
			Widget.alpha = 0f;
		}
	}

	public void SetCooltime(double deactiveAt, double reactiveAt)
	{
		_deactiveAt = deactiveAt;
		_reactiveAt = reactiveAt;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (predictedServerTime >= _reactiveAt)
		{
			_cooltimeRoutineFlag = false;
		}
		else if (!_cooltimeRoutineFlag)
		{
			((MonoBehaviour)this).StartCoroutine(CoCooltimeRoutine());
		}
	}

	private IEnumerator CoCooltimeRoutine()
	{
		_cooltimeRoutineFlag = true;
		while (_cooltimeRoutineFlag)
		{
			double now = Connections.Frontend.GetPredictedServerTime();
			double r = (now - _deactiveAt) / (_reactiveAt - _deactiveAt);
			float ratio = Mathf.Clamp01((float)r);
			if (ratio < 1f)
			{
				_cooltime.fillAmount = ratio;
				yield return null;
				continue;
			}
			break;
		}
		_cooltime.fillAmount = 1f;
		_cooltimeRoutineFlag = false;
	}

	private void Set(Interaction value)
	{
		Action = value;
		string text = IconMap.Get(value);
		if (text == null)
		{
			_text.text = value.GetName();
			((Component)_icon).gameObject.SetActive(false);
			((Component)_text).gameObject.SetActive(true);
		}
		else
		{
			_icon.spriteName = text;
			UIUtility.ResizeToSquare(_icon);
			((Component)_icon).gameObject.SetActive(true);
			((Component)_text).gameObject.SetActive(false);
		}
	}
}
