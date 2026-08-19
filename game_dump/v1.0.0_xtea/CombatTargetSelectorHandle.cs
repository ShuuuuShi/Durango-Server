using System;
using UnityEngine;

public class CombatTargetSelectorHandle : MonoBehaviour
{
	[SerializeField]
	private UIWidget _uiWidget;

	[SerializeField]
	private TweenAlpha _tweenAlpha;

	public event Action Clicked;

	private void Awake()
	{
		_tweenAlpha.AddOnFinished(OnFinishedTweenAlpha);
		UIEventListener.Get(((Component)this).gameObject).onClick = OnClickGameObject;
	}

	public void Show(bool show)
	{
		if (_uiWidget.alpha != ((!show) ? 0f : 1f))
		{
			((Component)this).gameObject.SetActive(true);
			_tweenAlpha.tweenFactor = _uiWidget.alpha;
			if (show)
			{
				_tweenAlpha.PlayForward();
			}
			else
			{
				_tweenAlpha.PlayReverse();
			}
		}
	}

	private void OnFinishedTweenAlpha()
	{
		if (_uiWidget.alpha == 0f)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void OnClickGameObject(GameObject obj)
	{
		if (this.Clicked != null)
		{
			this.Clicked();
		}
	}
}
