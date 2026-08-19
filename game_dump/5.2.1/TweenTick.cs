using UnityEngine;

public class TweenTick : UITweener
{
	public delegate void TickCallback(float factor, bool isFinished);

	[Range(0f, 1f)]
	private float from;

	[Range(0f, 1f)]
	private float to = 1f;

	private TickCallback _callback;

	private float _value;

	protected override void OnUpdate(float factor, bool isFinished)
	{
		if (_callback != null)
		{
			_callback(factor, isFinished);
		}
	}

	public static TweenTick Begin(GameObject go, float duration, TickCallback callback)
	{
		TweenTick tweenTick = UITweener.Begin<TweenTick>(go, duration);
		tweenTick._callback = callback;
		tweenTick.from = 0f;
		tweenTick.to = 1f;
		if (duration <= 0f)
		{
			tweenTick.Sample(1f, isFinished: true);
			tweenTick.enabled = false;
		}
		return tweenTick;
	}
}
