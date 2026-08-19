using UnityEngine;

namespace Durango.UI;

public class GatheringProgressGauge : ProgressGauge
{
	[SerializeField]
	private UISprite _gaugeSprite;

	private void OnEnable()
	{
		base.Widget.alpha = 0f;
	}

	protected override void InitGauge()
	{
		SetTarget(null);
		_gaugeSprite.fillAmount = 0f;
	}

	protected override void DrawGauge(float ratio)
	{
		_gaugeSprite.fillAmount = ratio;
	}

	protected override bool EndedGauge(float timer)
	{
		base.Widget.alpha = 0f;
		return true;
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.Timer = null;
	}
}
