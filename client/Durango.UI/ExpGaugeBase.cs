using UnityEngine;

namespace Durango.UI;

public class ExpGaugeBase : UIWidget
{
	[SerializeField]
	protected UISprite _background;

	[SerializeField]
	protected UISprite _upper;

	[SerializeField]
	protected UISprite _fillEffect;

	[SerializeField]
	protected int _fillSpeed;

	[SerializeField]
	private RectLayout _layout;

	protected int _beforeWidth;

	protected int _currentWidth;

	protected int _targetWidth;

	protected float _fillEffectAlpha;

	protected bool _enableFillEffect;

	protected override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			_upper.width = 0;
			_currentWidth = -1;
			AddOnChange(OnChanged);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			GameSystem<StatisticsSystem>.Instance().StatisticsUpdated += OnUpdateStatistics;
			Refresh(instant: true);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			GameSystem<StatisticsSystem>.Instance().StatisticsUpdated -= OnUpdateStatistics;
		}
	}

	private void OnUpdateStatistics()
	{
		Refresh(instant: false);
	}

	private void Refresh(bool instant)
	{
		Set(GameSystem<StatisticsSystem>.Instance().Exp, instant || UIManager.IsLoadingCurtain);
	}

	private void Set(int exp, bool instant)
	{
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		GameSystem<StatisticsSystem>.Instance().GetExpRange(level, out var min, out var max);
		int num = exp - min;
		int num2 = max - min;
		Set(level, (num2 != 0) ? ((float)num / (float)num2) : 0f, instant);
	}

	protected virtual void Set(int level, float ratio, bool instant)
	{
		_beforeWidth = _currentWidth;
		_targetWidth = (int)((float)_background.width * ratio);
		if (instant)
		{
			_currentWidth = _targetWidth;
			_upper.width = _targetWidth;
			_upper.alpha = Mathf.Clamp01(_currentWidth);
			_fillEffect.gameObject.SetActive(value: false);
		}
		else
		{
			_enableFillEffect = true;
			_fillEffect.gameObject.SetActive(value: true);
		}
	}

	protected virtual void OnChanged()
	{
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		Set(GameSystem<StatisticsSystem>.Instance().Exp, instant: true);
	}
}
