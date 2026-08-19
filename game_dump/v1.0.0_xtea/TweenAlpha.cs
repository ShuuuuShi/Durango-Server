using System;
using UnityEngine;

[AddComponentMenu("NGUI/Tween/Tween Alpha")]
public class TweenAlpha : UITweener
{
	[Range(0f, 1f)]
	public float from = 1f;

	[Range(0f, 1f)]
	public float to = 1f;

	private bool mCached;

	private UIRect mRect;

	private Material mMat;

	private SpriteRenderer mSr;

	[Obsolete("Use 'value' instead")]
	public float alpha
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public float value
	{
		get
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			if (!mCached)
			{
				Cache();
			}
			if ((Object)(object)mRect != (Object)null)
			{
				return mRect.alpha;
			}
			if ((Object)(object)mSr != (Object)null)
			{
				return mSr.color.a;
			}
			return (!((Object)(object)mMat != (Object)null)) ? 1f : mMat.color.a;
		}
		set
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			if (!mCached)
			{
				Cache();
			}
			if ((Object)(object)mRect != (Object)null)
			{
				mRect.alpha = value;
			}
			else if ((Object)(object)mSr != (Object)null)
			{
				Color color = mSr.color;
				color.a = value;
				mSr.color = color;
			}
			else if ((Object)(object)mMat != (Object)null)
			{
				Color color2 = mMat.color;
				color2.a = value;
				mMat.color = color2;
			}
		}
	}

	private void Cache()
	{
		mCached = true;
		mRect = ((Component)this).GetComponent<UIRect>();
		mSr = ((Component)this).GetComponent<SpriteRenderer>();
		if ((Object)(object)mRect == (Object)null && (Object)(object)mSr == (Object)null)
		{
			Renderer component = ((Component)this).GetComponent<Renderer>();
			if ((Object)(object)component != (Object)null)
			{
				mMat = component.material;
			}
			if ((Object)(object)mMat == (Object)null)
			{
				mRect = ((Component)this).GetComponentInChildren<UIRect>();
			}
		}
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		value = Mathf.Lerp(from, to, factor);
	}

	public static TweenAlpha Begin(GameObject go, float duration, float alpha)
	{
		TweenAlpha tweenAlpha = UITweener.Begin<TweenAlpha>(go, duration);
		tweenAlpha.from = tweenAlpha.value;
		tweenAlpha.to = alpha;
		if (duration <= 0f)
		{
			tweenAlpha.Sample(1f, isFinished: true);
			((Behaviour)tweenAlpha).enabled = false;
		}
		return tweenAlpha;
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
