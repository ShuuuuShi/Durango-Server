using UnityEngine;

[AddComponentMenu("NGUI/Tween/Tween Float")]
public class TweenFloat : UITweener
{
	public delegate void TweenCallback(float current, bool isFinished);

	public float from = 1f;

	public float to = 1f;

	private TweenCallback _callback;

	public float value { get; private set; }

	protected override void OnUpdate(float factor, bool isFinished)
	{
		value = Mathf.Lerp(from, to, factor);
		if (_callback != null)
		{
			_callback(value, isFinished);
		}
	}

	public void Begin(float dst)
	{
		Begin(base.gameObject, duration, dst);
	}

	public static TweenFloat Begin(GameObject go, float duration, float dst)
	{
		TweenFloat tweenFloat = UITweener.Begin<TweenFloat>(go, duration);
		tweenFloat.from = tweenFloat.value;
		tweenFloat.to = dst;
		if (duration <= 0f)
		{
			tweenFloat.Sample(1f, isFinished: true);
			tweenFloat.enabled = false;
		}
		return tweenFloat;
	}

	public override void SetStartToCurrentValue()
	{
		from = value;
	}

	public override void SetEndToCurrentValue()
	{
		to = value;
	}

	public void SetCallback(TweenCallback action)
	{
		_callback = action;
	}
}
