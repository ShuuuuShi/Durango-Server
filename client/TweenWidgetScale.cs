using UnityEngine;

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/Tween/Tween Widget Scale")]
public class TweenWidgetScale : UITweener
{
	public Vector2 CachedTargetSize = new Vector2(100f, 100f);

	public Vector2 from = new Vector2(0.94f, 0.94f);

	public Vector2 to = Vector2.one;

	private UIWidget mWidget;

	public UIWidget CachedWidget
	{
		get
		{
			if (mWidget == null)
			{
				mWidget = GetComponent<UIWidget>();
			}
			return mWidget;
		}
	}

	public Vector2 value
	{
		get
		{
			return new Vector2(CachedTargetSize.x / (float)CachedWidget.width, CachedTargetSize.y / (float)CachedWidget.height);
		}
		set
		{
			CachedWidget.SetDimensions(Mathf.RoundToInt(CachedTargetSize.x * value.x), Mathf.RoundToInt(CachedTargetSize.y * value.y));
		}
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		float x = from.x * (1f - factor) + to.x * factor;
		float y = from.y * (1f - factor) + to.y * factor;
		value = new Vector2(x, y);
	}

	public static TweenWidgetScale Begin(UIWidget widget, float duration, Vector2 targetSize)
	{
		TweenWidgetScale tweenWidgetScale = UITweener.Begin<TweenWidgetScale>(widget.gameObject, duration);
		tweenWidgetScale.from = new Vector2(widget.width, widget.height);
		tweenWidgetScale.to = targetSize;
		if (duration <= 0f)
		{
			tweenWidgetScale.Sample(1f, isFinished: true);
			tweenWidgetScale.enabled = false;
		}
		return tweenWidgetScale;
	}

	[ContextMenu("Set 'From' to current value")]
	public override void SetStartToCurrentValue()
	{
		from = value;
	}

	[ContextMenu("Set 'To' to current value")]
	public override void SetEndToCurrentValue()
	{
		to = value;
	}

	[ContextMenu("Assume value of 'From'")]
	private void SetCurrentValueToStart()
	{
		value = from;
	}

	[ContextMenu("Assume value of 'To'")]
	private void SetCurrentValueToEnd()
	{
		value = to;
	}
}
