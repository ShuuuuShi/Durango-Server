using UnityEngine;

public class FadeInOutContainer : MonoBehaviour
{
	[SerializeField]
	private float _fadeInDelay;

	[SerializeField]
	private float _fadeOutDelay;

	private UIWidget _uiWidget;

	private TweenAlpha _tweenAlpha;

	public virtual void Init()
	{
		_uiWidget = ((Component)this).GetComponent<UIWidget>();
		_tweenAlpha = ((Component)this).GetComponent<TweenAlpha>();
		_tweenAlpha.SetOnFinished(OnFinishedTweenAlpha);
	}

	public void Show(bool show, bool instant = false)
	{
		if (instant)
		{
			_tweenAlpha.ResetToBeginning();
			((Behaviour)_tweenAlpha).enabled = false;
			_uiWidget.alpha = ((!show) ? 0f : 1f);
			((Component)this).gameObject.SetActive(show);
		}
		else if (show)
		{
			((Component)this).gameObject.SetActive(true);
			_tweenAlpha.delay = _fadeInDelay;
			_tweenAlpha.tweenFactor = 0f;
			_tweenAlpha.PlayForward();
		}
		else
		{
			_tweenAlpha.delay = _fadeOutDelay;
			_tweenAlpha.tweenFactor = 1f;
			_tweenAlpha.PlayReverse();
		}
	}

	private void OnFinishedTweenAlpha()
	{
		if (_uiWidget.alpha == 0f)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
