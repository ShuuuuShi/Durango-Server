using System;
using UnityEngine;

namespace Durango.UI;

public class TimerProgressGauge : ProgressGauge
{
	private const int AlertTime = 10;

	[SerializeField]
	private UISprite _upperSprite;

	[SerializeField]
	private Transform _tickArrow;

	[SerializeField]
	private UILabel _timeLabel;

	private int _prevRemainTick;

	protected override void InitGauge()
	{
		if (_upperSprite != null)
		{
			_upperSprite.fillAmount = 1f;
		}
		if (_tickArrow != null)
		{
			_tickArrow.localEulerAngles = Vector3.zero;
		}
		_prevRemainTick = -1;
	}

	protected override void DrawGauge(float ratio)
	{
		float num = RemainTime();
		int num2 = Mathf.CeilToInt(num);
		if (num2 != _prevRemainTick)
		{
			if (_timeLabel != null)
			{
				_timeLabel.text = GetLabelText(num2);
			}
			_prevRemainTick = num2;
		}
		if (_upperSprite != null)
		{
			_upperSprite.fillAmount = 1f - ratio;
		}
		if (_tickArrow != null)
		{
			_tickArrow.localEulerAngles = Vector3.back * 360f * ratio;
		}
		if (num2 < 10)
		{
			float alpha = Mathf.Cos((10f - num) * (float)Math.PI * 2f) * 0.25f + 0.75f;
			base.Widget.alpha = alpha;
		}
		else
		{
			base.Widget.alpha = 1f;
		}
	}

	protected override bool EndedGauge(float timer)
	{
		if (timer < 0.5f)
		{
			base.Widget.alpha = 1f - timer / 0.5f;
			return false;
		}
		return true;
	}

	protected virtual string GetLabelText(double remainTick)
	{
		return TimedeltaFormatter.Format(remainTick);
	}
}
