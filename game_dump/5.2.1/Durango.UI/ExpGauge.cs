using L10N;
using UnityEngine;

namespace Durango.UI;

public class ExpGauge : ExpGaugeBase
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UIWidget _paddingWidget;

	[SerializeField]
	private UISprite _fill;

	protected override void Awake()
	{
		_fill.width = 0;
		base.Awake();
	}

	protected override void Set(int level, float ratio, bool instant)
	{
		base.Set(level, ratio, instant);
		_label.text = T._("[c][257C25]{1:lv:}[-][/c]   {0:p1}", ratio, level);
		_fill.width = _targetWidth;
		_fill.alpha = Mathf.Clamp01(_targetWidth);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Application.isPlaying)
		{
			return;
		}
		if (_currentWidth < _targetWidth)
		{
			int num = _targetWidth - _currentWidth;
			int num2 = (int)((float)_fillSpeed * Time.deltaTime);
			if (num < num2)
			{
				_currentWidth = _targetWidth;
			}
			else
			{
				_currentWidth += num2;
			}
			_upper.width = _currentWidth;
			_upper.alpha = Mathf.Clamp01(_currentWidth);
			_fillEffectAlpha += Time.deltaTime / 0.2f;
			_fillEffectAlpha = Mathf.Min(_fillEffectAlpha, 1f);
			_fillEffect.alpha = _fillEffectAlpha;
			UpdateActivatedFillEffect();
		}
		else if (_currentWidth > _targetWidth)
		{
			_currentWidth = _targetWidth;
			_upper.width = _currentWidth;
			_upper.alpha = Mathf.Clamp01(_currentWidth);
			DeactivateFillEffect();
		}
		else if (_enableFillEffect)
		{
			if (_fillEffectAlpha > 0f)
			{
				_fillEffectAlpha -= Time.deltaTime / 0.2f;
				_fillEffect.alpha = _fillEffectAlpha;
			}
			else
			{
				DeactivateFillEffect();
			}
		}
	}

	private void UpdateActivatedFillEffect()
	{
		float num = _currentWidth - _beforeWidth;
		float num2 = _targetWidth - _beforeWidth;
		float num3 = Easings.QuadraticEaseOut(num / num2);
		_fillEffect.width = _beforeWidth + Mathf.RoundToInt(num2 * num3);
	}

	private void DeactivateFillEffect()
	{
		_fillEffect.alpha = 0f;
		_fillEffectAlpha = 0f;
		_enableFillEffect = false;
		_fillEffect.gameObject.SetActive(value: false);
	}

	protected override void OnChanged()
	{
		if (UIManager.IsPortraitScreen)
		{
			_paddingWidget.gameObject.SetActive(value: true);
			_paddingWidget.width = (int)((float)UIManager.ScreenHeight * UIManager.SafeArea.y);
		}
		else
		{
			_paddingWidget.gameObject.SetActive(value: false);
		}
		base.OnChanged();
	}
}
