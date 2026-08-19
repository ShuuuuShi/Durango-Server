using UnityEngine;

[AddComponentMenu("NGUI/Tween/Tween Fill")]
[RequireComponent(typeof(UIBasicSprite))]
public class TweenFill : UITweener
{
	[Range(0f, 1f)]
	public float from = 1f;

	[Range(0f, 1f)]
	public float to = 1f;

	private bool mCached;

	private UIBasicSprite mSprite;

	public float value
	{
		get
		{
			if (!mCached)
			{
				Cache();
			}
			if ((Object)(object)mSprite != (Object)null)
			{
				return mSprite.fillAmount;
			}
			return 0f;
		}
		set
		{
			if (!mCached)
			{
				Cache();
			}
			if ((Object)(object)mSprite != (Object)null)
			{
				mSprite.fillAmount = value;
			}
		}
	}

	private void Cache()
	{
		mCached = true;
		mSprite = ((Component)this).GetComponent<UISprite>();
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		value = Mathf.Lerp(from, to, factor);
	}

	public static TweenFill Begin(GameObject go, float duration, float fill)
	{
		TweenFill tweenFill = UITweener.Begin<TweenFill>(go, duration);
		tweenFill.from = tweenFill.value;
		tweenFill.to = fill;
		if (duration <= 0f)
		{
			tweenFill.Sample(1f, isFinished: true);
			((Behaviour)tweenFill).enabled = false;
		}
		return tweenFill;
	}

	public override void SetStartToCurrentValue()
	{
		from = value;
	}

	public override void SetEndToCurrentValue()
	{
		to = value;
	}
}
