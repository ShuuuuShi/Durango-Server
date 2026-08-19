using EnvironmentData;
using L10N;
using TimerData;
using UnityEngine;

public class MinimapFatigueWarningWidget : MonoBehaviour
{
	[SerializeField]
	private TweenScale _warningSprite;

	[SerializeField]
	private UILabel _survivalTimeLabel;

	[SerializeField]
	private Color[] _labelColors;

	[SerializeField]
	private float _warningAnimationRatio;

	private Fatigue _fatigue;

	private float _defaultPeriod;

	private Fatigue.State _state = Fatigue.State.None;

	private float _survivalLabellUpdateTime;

	private void Awake()
	{
		((Component)_warningSprite).gameObject.SetActive(false);
		((Component)_survivalTimeLabel).gameObject.SetActive(true);
		_survivalTimeLabel.text = string.Empty;
		_defaultPeriod = _warningSprite.duration;
	}

	private void Update()
	{
		UpdateFatige();
		if (_survivalLabellUpdateTime > 0f && _survivalLabellUpdateTime < Time.time)
		{
			UpdateSurvivalTimeLabel();
		}
	}

	private void UpdateFatige()
	{
		_fatigue = GameSystem<FatigueSystem>.Instance().Fatigue;
		if (_fatigue != null)
		{
			Fatigue.State state = _fatigue.GetState();
			if (_state != state)
			{
				SetState(state);
			}
		}
	}

	private void SetState(Fatigue.State state)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		_state = state;
		switch (state)
		{
		case Fatigue.State.Normal:
			((Component)_warningSprite).gameObject.SetActive(false);
			break;
		case Fatigue.State.Warning:
			((Component)_warningSprite).gameObject.SetActive(true);
			_warningSprite.duration = _defaultPeriod;
			break;
		case Fatigue.State.Danger:
			((Component)_warningSprite).gameObject.SetActive(true);
			_warningSprite.duration = _defaultPeriod * _warningAnimationRatio;
			break;
		}
		_survivalTimeLabel.color = _labelColors[(int)state];
		UpdateSurvivalTimeLabel();
	}

	private void UpdateSurvivalTimeLabel()
	{
		if (_fatigue == null)
		{
			return;
		}
		if (_state == Fatigue.State.Normal)
		{
			float num = 0f;
			if (_fatigue.Velocity > 0.01f)
			{
				num = _fatigue.Remain(_fatigue.Max);
			}
			if (num > 0f)
			{
				string text = TimerSystem.TimeToString(num, TimePeriod.Min, 1);
				_survivalTimeLabel.text = T._("{0} 활동 가능", text);
			}
			else
			{
				_survivalTimeLabel.text = T._("안전");
			}
			_survivalLabellUpdateTime = Time.time + 10f;
		}
		else
		{
			_survivalTimeLabel.text = _state.GetName();
			_survivalLabellUpdateTime = 0f;
		}
	}
}
