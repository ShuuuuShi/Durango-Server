using System;
using TimerData;
using UnityEngine;

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
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_upperSprite != (Object)null)
		{
			_upperSprite.fillAmount = 1f;
		}
		if ((Object)(object)_tickArrow != (Object)null)
		{
			_tickArrow.localEulerAngles = Vector3.zero;
		}
		_prevRemainTick = -1;
	}

	protected override void DrawGauge(float ratio)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		float num = RemainTime();
		int num2 = Mathf.CeilToInt(num);
		if (num2 != _prevRemainTick)
		{
			if ((Object)(object)_timeLabel != (Object)null)
			{
				_timeLabel.text = GetLabelText(num2);
			}
			_prevRemainTick = num2;
		}
		if ((Object)(object)_upperSprite != (Object)null)
		{
			_upperSprite.fillAmount = 1f - ratio;
		}
		if ((Object)(object)_tickArrow != (Object)null)
		{
			_tickArrow.localEulerAngles = Vector3.back * 360f * ratio;
		}
		if (num2 < 10)
		{
			float num3 = 10f - num;
			float alpha = Mathf.Cos(num3 * (float)Math.PI * 2f) * 0.25f + 0.75f;
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
		return TimerSystem.TimeToString(remainTick, TimePeriod.Sec, 2);
	}
}
