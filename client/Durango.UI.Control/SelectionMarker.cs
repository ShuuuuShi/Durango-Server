using UnityEngine;

namespace Durango.UI.Control;

[RequireComponent(typeof(UIWidget))]
public class SelectionMarker : MonoBehaviour
{
	[SerializeField]
	private UITweener[] _tweens;

	[SerializeField]
	private TweenWidgetScale[] _tweenSize;

	private bool _isInitialized;

	private void OnDisable()
	{
		_isInitialized = false;
	}

	public void Set(UIWidget target, Vector3 offset = default(Vector3))
	{
		bool flag = false;
		if (!_isInitialized)
		{
			flag = true;
			_isInitialized = true;
		}
		Transform parent = base.transform.parent;
		base.transform.parent = target.transform;
		base.transform.localScale = Vector3.one;
		UIWidget component = GetComponent<UIWidget>();
		component.SetDimensions(target.width, target.height);
		component.ParentHasChanged();
		component.SetPosition(target.localCenter, 0.5f, 0.5f);
		if (!flag && parent == base.transform.parent)
		{
			flag = true;
		}
		UITweener[] tweens = _tweens;
		foreach (UITweener uITweener in tweens)
		{
			if (flag)
			{
				uITweener.enabled = false;
				uITweener.Sample(1f, isFinished: true);
			}
			else
			{
				uITweener.tweenFactor = 0f;
				uITweener.PlayForward();
			}
		}
		TweenWidgetScale[] tweenSize = _tweenSize;
		foreach (TweenWidgetScale tweenWidgetScale in tweenSize)
		{
			tweenWidgetScale.CachedTargetSize = new Vector2(target.width, target.height);
			if (flag)
			{
				tweenWidgetScale.enabled = false;
				tweenWidgetScale.Sample(1f, isFinished: true);
			}
			else
			{
				tweenWidgetScale.tweenFactor = 0f;
				tweenWidgetScale.PlayForward();
			}
		}
		base.gameObject.SetActive(value: true);
	}
}
