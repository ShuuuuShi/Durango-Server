using UnityEngine;

[AddComponentMenu("NGUI/Tween/Tween Offset")]
public class TweenOffset : UITweener
{
	public Vector3 offset;

	private Pair<Vector3, Vector3>? _value;

	protected override void OnDisable()
	{
		base.OnDisable();
		_value = null;
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		Pair<Vector3, Vector3>? value = _value;
		if (!value.HasValue)
		{
			Vector3 localPosition = base.transform.localPosition;
			_value = new Pair<Vector3, Vector3>(localPosition + offset, localPosition);
		}
		Vector3 item = _value.Value.Item1;
		Vector3 item2 = _value.Value.Item2;
		base.transform.localPosition = item * (1f - factor) + item2 * factor;
	}

	public static TweenOffset Begin(GameObject go, float duration, Vector3 offset)
	{
		TweenOffset tweenOffset = UITweener.Begin<TweenOffset>(go, duration);
		tweenOffset.offset = offset;
		if (duration <= 0f)
		{
			tweenOffset.Sample(1f, isFinished: true);
			tweenOffset.enabled = false;
		}
		return tweenOffset;
	}

	[ContextMenu("Set 'From' to current value")]
	public override void SetStartToCurrentValue()
	{
	}

	[ContextMenu("Set 'To' to current value")]
	public override void SetEndToCurrentValue()
	{
	}

	[ContextMenu("Assume value of 'From'")]
	private void SetCurrentValueToStart()
	{
	}

	[ContextMenu("Assume value of 'To'")]
	private void SetCurrentValueToEnd()
	{
	}
}
