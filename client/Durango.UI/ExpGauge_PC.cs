using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ExpGauge_PC : ExpGaugeBase
{
	[SerializeField]
	private TweenerPlayer _fillEffectTweenerPlayer;

	[SerializeField]
	private UISprite _expLabelBG;

	[SerializeField]
	private UILabel _expLabel;

	[SerializeField]
	private TweenAlpha _expLabelTweenAlpha;

	private bool _updated;

	protected override void Set(int level, float ratio, bool instant)
	{
		_beforeWidth = _currentWidth;
		_targetWidth = (int)((float)(_background.width - (_background.rightAnchor.absolute - _background.leftAnchor.absolute)) * ratio);
		if (instant)
		{
			_currentWidth = _targetWidth;
			_upper.width = _targetWidth;
			_fillEffectTweenerPlayer.ResetToLast();
			_expLabelBG.alpha = _expLabelTweenAlpha.to;
			_updated = true;
		}
		else
		{
			_fillEffectTweenerPlayer.ResetToFirst();
			_fillEffectTweenerPlayer.Play(0f, 0, Mathf.Max((float)(_targetWidth - _beforeWidth) / (float)_fillSpeed, 1f));
			_expLabelBG.alpha = _expLabelTweenAlpha.from;
			_updated = false;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Application.isPlaying)
		{
			return;
		}
		bool flag = false;
		if (_currentWidth < _targetWidth)
		{
			int num = _targetWidth - _currentWidth;
			int num2 = (int)((float)_fillSpeed * Time.deltaTime);
			if (num2 < num)
			{
				_currentWidth += num2;
			}
			else
			{
				_currentWidth = _targetWidth;
			}
			_upper.width = _currentWidth;
			flag = _currentWidth == _targetWidth;
			UpdateExp();
		}
		else if (_currentWidth > _targetWidth)
		{
			_currentWidth = _targetWidth;
			_upper.width = _currentWidth;
			flag = true;
			UpdateExp();
		}
		else if (!_updated)
		{
			flag = true;
			UpdateExp();
		}
		if (flag)
		{
			_expLabelTweenAlpha.ResetToBeginning();
			_expLabelTweenAlpha.PlayForward();
		}
		_updated = true;
	}

	private void UpdateExp()
	{
		_expLabel.text = T._("{0:p1}", Mathf.Clamp01((float)_currentWidth / (float)(_background.width - (_background.rightAnchor.absolute - _background.leftAnchor.absolute))));
		_expLabelBG.width = 6 + _expLabel.width + 6;
	}
}
