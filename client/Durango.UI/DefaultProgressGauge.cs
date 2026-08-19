using UnityEngine;

namespace Durango.UI;

public class DefaultProgressGauge : ProgressGauge
{
	[SerializeField]
	private UIWidget _background;

	[SerializeField]
	private UIWidget _gaugeBar;

	[SerializeField]
	private UIWidget _finishEffect;

	[SerializeField]
	private float _glitterTime;

	[SerializeField]
	private float _hideTime;

	private float _width;

	protected override void InitGauge()
	{
		_width = _background.width;
		_gaugeBar.alpha = 1f;
		_background.alpha = 1f;
		_finishEffect.alpha = 0f;
	}

	protected override void DrawGauge(float ratio)
	{
		_gaugeBar.width = (int)(_width * ratio);
	}

	protected override bool EndedGauge(float timer)
	{
		if (timer < _glitterTime)
		{
			_gaugeBar.alpha = 1f - timer / _glitterTime;
			_finishEffect.alpha = timer / _glitterTime;
			return false;
		}
		timer -= _glitterTime;
		if (timer < _hideTime)
		{
			base.Widget.alpha = Mathf.Clamp01(1f - timer / _hideTime);
			return false;
		}
		return true;
	}
}
