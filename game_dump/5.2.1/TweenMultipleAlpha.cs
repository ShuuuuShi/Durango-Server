using UnityEngine;

public class TweenMultipleAlpha : UITweener
{
	[Range(0f, 1f)]
	public float from = 1f;

	[Range(0f, 1f)]
	public float to = 1f;

	private bool mCached;

	private Material[] mMats;

	public float value
	{
		get
		{
			if (!mCached)
			{
				Cache();
			}
			if (mMats != null)
			{
				return mMats[0].color.a;
			}
			return 1f;
		}
		set
		{
			if (!mCached)
			{
				Cache();
			}
			if (mMats != null)
			{
				for (int i = 0; i < mMats.Length; i++)
				{
					Color color = mMats[i].color;
					color.a = value;
					mMats[i].color = color;
				}
			}
		}
	}

	private void Cache()
	{
		mCached = true;
		Renderer component = GetComponent<Renderer>();
		if (component != null)
		{
			mMats = component.materials;
		}
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		value = Mathf.Lerp(from, to, factor);
	}

	public static TweenMultipleAlpha Begin(GameObject go, float duration, float alpha)
	{
		TweenMultipleAlpha tweenMultipleAlpha = UITweener.Begin<TweenMultipleAlpha>(go, duration);
		tweenMultipleAlpha.from = tweenMultipleAlpha.value;
		tweenMultipleAlpha.to = alpha;
		if (duration <= 0f)
		{
			tweenMultipleAlpha.Sample(1f, isFinished: true);
			tweenMultipleAlpha.enabled = false;
		}
		return tweenMultipleAlpha;
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
