using UnityEngine;

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/Tween/Tween Shape")]
public class TweenShape : UITweener
{
	public UIWidget from;

	public UIWidget to;

	public bool parentWhenFinished;

	private UIWidget _mWidget;

	private Vector2 _fromWidgetSize;

	private Vector3 _fromLocalPos;

	private Vector3 _fromPivotOffsetSize;

	public UIWidget CachedWidget
	{
		get
		{
			if (_mWidget == null)
			{
				_mWidget = GetComponent<UIWidget>();
			}
			return _mWidget;
		}
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		if (to != null && from != null)
		{
			if (from == null)
			{
				from = CachedWidget;
				_fromWidgetSize = from.localSize;
				_fromLocalPos = from.transform.localPosition;
				_fromPivotOffsetSize = from.PivotOffsetSize;
			}
			if (from != null)
			{
				Vector2 vector = Vector3.Scale(GetNormalizedWidgetSize(from) * (1f - factor) + GetNormalizedWidgetSize(to) * factor, GetSizeOffset(CachedWidget));
				CachedWidget.SetDimensions((int)vector.x, (int)vector.y);
				CachedWidget.transform.localPosition = (from.transform.localPosition - Vector3.Scale(from.PivotOffsetSize, from.transform.localScale)) * (1f - factor) + (to.transform.localPosition - Vector3.Scale(to.PivotOffsetSize, to.transform.localScale)) * factor + Vector3.Scale(CachedWidget.PivotOffsetSize, CachedWidget.transform.localScale);
			}
			else
			{
				Vector2 vector2 = _fromWidgetSize * (1f - factor) + to.localSize * factor;
				CachedWidget.SetDimensions((int)vector2.x, (int)vector2.y);
				CachedWidget.transform.localPosition = (_fromLocalPos - _fromPivotOffsetSize) * (1f - factor) + (to.transform.localPosition - (Vector3)to.PivotOffsetSize) * factor + (Vector3)CachedWidget.PivotOffsetSize;
			}
			if (parentWhenFinished && isFinished)
			{
				CachedWidget.transform.parent = to.transform;
			}
		}
	}

	public virtual Vector2 GetNormalizedWidgetSize(UIWidget target)
	{
		return Vector2.Scale(target.localSize, new Vector2(target.transform.localScale.x, target.transform.localScale.y));
	}

	private Vector3 GetSizeOffset(UIWidget widget)
	{
		Vector3 localScale = widget.cachedTransform.localScale;
		return new Vector3(1f / localScale.x, 1f / localScale.y, 1f / localScale.z);
	}

	public static TweenShape Begin(GameObject go, float duration, UIWidget to)
	{
		return Begin(go, duration, null, to);
	}

	public static TweenShape Begin(GameObject go, float duration, UIWidget from, UIWidget to)
	{
		TweenShape tweenShape = UITweener.Begin<TweenShape>(go, duration);
		tweenShape.from = from;
		tweenShape.to = to;
		if (duration <= 0f)
		{
			tweenShape.Sample(1f, isFinished: true);
			tweenShape.enabled = false;
		}
		return tweenShape;
	}
}
