using System;
using UnityEngine;

namespace Durango.UI;

public class ActionProgressGauge : ProgressGauge
{
	[SerializeField]
	private UISprite _upperSprite;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private float _fadeTime;

	private float _enableTime;

	public void Set(string icon)
	{
		_iconSprite.spriteName = icon;
	}

	protected override void InitGauge()
	{
		base.Widget.alpha = 0f;
		_enableTime = Time.time;
	}

	protected override void DrawGauge(float ratio)
	{
		float alpha = Mathf.Clamp01((Time.time - _enableTime) / _fadeTime);
		base.Widget.alpha = alpha;
		_upperSprite.fillAmount = 1f - ratio;
	}

	protected override bool EndedGauge(float timer)
	{
		float num = 1f - Mathf.Clamp01(timer / _fadeTime);
		base.Widget.alpha = num;
		return Math.Abs(num) < float.Epsilon;
	}
}
