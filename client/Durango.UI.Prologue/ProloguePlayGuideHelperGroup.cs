using Durango.System;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Prologue;

public class ProloguePlayGuideHelperGroup : UIBase
{
	[SerializeField]
	private PrologueArrowTargetWidget _arrowTarget;

	[SerializeField]
	private GameObject _clickTarget;

	[SerializeField]
	private UISprite _handSprite;

	private PrologueClickTargetLocator _clickTargetLocator;

	private PrologueClickTargetLocator _arrowTargetLocator;

	private float _clickTargetPrevScale;

	private TweenScale _clickTargeTweenScale;

	private TweenAlpha _clickTargetTweenAlpha;

	private void Awake()
	{
		_clickTargetTweenAlpha = _handSprite.GetComponent<TweenAlpha>();
		_clickTargeTweenScale = _handSprite.GetComponent<TweenScale>();
	}

	private void LateUpdate()
	{
		if (_arrowTarget.IsEnabled() && _arrowTarget.WithinScreen)
		{
			ProcessClickTarget(_arrowTargetLocator);
		}
		else
		{
			ProcessClickTarget(_clickTargetLocator);
		}
	}

	private void ProcessClickTarget(PrologueClickTargetLocator locator)
	{
		if (locator == null)
		{
			_clickTarget.SetActive(value: false);
			return;
		}
		locator.Process();
		_clickTarget.transform.localPosition = GetCurrentClickTargetPos(locator);
		_clickTarget.transform.localRotation = Quaternion.Euler(0f, 0f, locator.Rotate());
		bool activeSelf = _clickTarget.activeSelf;
		bool flag = !Platform.Instance.UsePCUI && locator.IsVisible();
		_clickTarget.SetActive(flag);
		if (!activeSelf && flag)
		{
			_clickTargetTweenAlpha.ResetToBeginning();
			_clickTargetTweenAlpha.PlayForward();
			_clickTargeTweenScale.PlayForward();
		}
		float x = _handSprite.transform.localScale.x;
		bool flag2 = _clickTargetPrevScale > x;
		_handSprite.spriteName = ((!flag2) ? "guide_hand_normal" : "guide_hand_touched");
		_clickTargetPrevScale = x;
	}

	public void EnableClickTarget([NotNull] PrologueClickTargetLocator locator)
	{
		_clickTargetLocator = locator;
		_clickTargetTweenAlpha.enabled = false;
		_clickTargetPrevScale = _handSprite.transform.localScale.x;
	}

	private static Vector3 GetCurrentClickTargetPos(PrologueClickTargetLocator locator)
	{
		Vector3 nGUIPosition = locator.GetNGUIPosition();
		Vector2 offset = locator.GetOffset();
		return nGUIPosition + new Vector3(offset.x, offset.y);
	}

	public void DisableClickTarget()
	{
		_clickTargetLocator = null;
	}

	public void ShowTargetIfEnabled(bool visible)
	{
		_arrowTarget.ShowTargetIfEnabled(visible);
	}

	public void SetTarget(Vector3 target)
	{
		_arrowTarget.SetTarget(target);
		_arrowTargetLocator = ((!_arrowTarget.IsEnabled()) ? null : new PrologueClickTargetLocator(null, target));
	}

	public void FinishTargetIf()
	{
		_arrowTarget.FinishTargetIf();
	}

	public void ClearTarget()
	{
		_arrowTarget.ClearTarget();
		_arrowTargetLocator = null;
	}
}
