using UnityEngine;

public class GatheringProgressGauge : ProgressGauge
{
	[SerializeField]
	private UIWidget _spriteContiner;

	[SerializeField]
	private UISprite _gaugeSprite;

	protected override void InitGauge()
	{
		SetTarget(null);
		SetFadeInWidget(_spriteContiner);
		_gaugeSprite.fillAmount = 0f;
	}

	protected override void DrawGauge(float ratio)
	{
		_gaugeSprite.fillAmount = ratio;
	}

	protected override bool EndedGauge(float timer)
	{
		_spriteContiner.alpha = 0f;
		return true;
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.Timer = null;
	}

	protected override void Reposition()
	{
	}
}
