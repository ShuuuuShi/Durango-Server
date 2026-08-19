using Durango.Logic;
using UnityEngine;

namespace Durango.UI;

public class HudFatigueGauge : MonoBehaviour
{
	[SerializeField]
	private FatigueGaugeScrollSprite _scrollSprite;

	[SerializeField]
	private float _scollSpeedModifier;

	[SerializeField]
	private UISprite _upper;

	private TweenColor[] _upperTweenColors;

	[SerializeField]
	private Transform[] _fatigueLevelGuides;

	[SerializeField]
	private string[] _arrowSprites;

	[SerializeField]
	private Transform _currentArrow;

	[SerializeField]
	private bool _movableArrow = true;

	[SerializeField]
	private UISprite _arrowIcon;

	[SerializeField]
	private TweenScale _arrowIconChangeAnimation;

	private const int MaxFatigueLevel = 4;

	private int _arrowFatigueLevel;

	private OnBlurMasking _dangerEffect;

	private void Awake()
	{
		if (_upperTweenColors == null)
		{
			int num = 5;
			TweenColor[] components = _upper.GetComponents<TweenColor>();
			if (components != null && components.Length >= num)
			{
				_upperTweenColors = new TweenColor[num];
				TweenColor[] array = components;
				foreach (TweenColor tweenColor in array)
				{
					_upperTweenColors[tweenColor.tweenGroup] = tweenColor;
				}
			}
		}
		if (_fatigueLevelGuides == null || _fatigueLevelGuides.Length != 3)
		{
		}
		if (_arrowSprites == null || _arrowSprites.Length != 4)
		{
		}
		if (_dangerEffect == null)
		{
			_dangerEffect = GetComponent<OnBlurMasking>();
		}
		if (_dangerEffect != null)
		{
			_dangerEffect.OnBlur(enable: false);
		}
		GameSystem<FatigueSystem>.Instance().FatigueUpdated += UpdateFatigueGauge;
		UpdateFatigueGauge();
		GameSystem<FatigueSystem>.Instance().FatigueLevelChanged += OnFatigueLevelChanged;
		OnFatigueLevelChanged(GameSystem<FatigueSystem>.Instance().CurrentFatigueLevel);
	}

	private void Update()
	{
		UpdateFatigueGauge();
	}

	private void UpdateFatigueGauge()
	{
		_scrollSprite.Speed = GameSystem<FatigueSystem>.Instance().FatigueVelocity * _scollSpeedModifier;
		Fatigue fatigue = GameSystem<FatigueSystem>.Instance().Fatigue;
		float num = Mathf.Clamp01(fatigue.GetRatio(fatigue.Get()));
		_upper.fillAmount = num;
		for (int i = 0; i < _fatigueLevelGuides.Length; i++)
		{
			float fatigueLevelTerm = GameSystem<FatigueSystem>.Instance().GetFatigueLevelTerm((FatigueSystem.FatigueLevel)(i + 1));
			Vector3 localPosition = _fatigueLevelGuides[i].localPosition;
			localPosition.x = _upper.GetWidth() * fatigueLevelTerm;
			_fatigueLevelGuides[i].localPosition = localPosition;
			_fatigueLevelGuides[i].gameObject.SetActive(fatigueLevelTerm < 1f);
		}
		if (_movableArrow)
		{
			Vector3 localPosition2 = _currentArrow.localPosition;
			localPosition2.x = _upper.transform.localPosition.x + _upper.GetWidth() * num;
			_currentArrow.localPosition = localPosition2;
		}
	}

	private void OnFatigueLevelChanged(FatigueSystem.FatigueLevel level)
	{
		TweenColor[] upperTweenColors = _upperTweenColors;
		foreach (TweenColor tweenColor in upperTweenColors)
		{
			tweenColor.enabled = false;
			tweenColor.SetOnFinished((EventDelegate)null);
		}
		_upper.color = _upperTweenColors[_arrowFatigueLevel].to;
		_upperTweenColors[_arrowFatigueLevel].enabled = true;
		_upperTweenColors[_arrowFatigueLevel].SetOnFinished(OnFinishedUpperTweenColor);
		_upperTweenColors[_arrowFatigueLevel].ResetToEnd();
		_upperTweenColors[_arrowFatigueLevel].PlayReverse();
		if (_dangerEffect != null)
		{
			_dangerEffect.OnBlur(level == FatigueSystem.FatigueLevel.Danger);
		}
		_arrowFatigueLevel = (int)level;
		if (!_arrowIcon.spriteName.Equals(_arrowSprites[_arrowFatigueLevel]))
		{
			_arrowIcon.spriteName = _arrowSprites[_arrowFatigueLevel];
			_arrowIconChangeAnimation.ResetToBeginning();
			_arrowIconChangeAnimation.PlayForward();
		}
	}

	private void OnFinishedUpperTweenColor()
	{
		TweenColor[] upperTweenColors = _upperTweenColors;
		foreach (TweenColor tweenColor in upperTweenColors)
		{
			tweenColor.enabled = false;
			tweenColor.SetOnFinished((EventDelegate)null);
		}
		_upper.color = _upperTweenColors[_arrowFatigueLevel].from;
		_upperTweenColors[_arrowFatigueLevel].enabled = true;
		_upperTweenColors[_arrowFatigueLevel].ResetToBeginning();
		_upperTweenColors[_arrowFatigueLevel].PlayForward();
		if (_arrowFatigueLevel == 2)
		{
			_upperTweenColors[_arrowFatigueLevel].SetOnFinished(OnFinishedUpperBeforeDangerTweenColor);
		}
	}

	private void OnFinishedUpperBeforeDangerTweenColor()
	{
		_upper.color = _upperTweenColors[4].from;
		_upperTweenColors[4].enabled = true;
		_upperTweenColors[4].ResetToBeginning();
		_upperTweenColors[4].PlayForward();
	}
}
