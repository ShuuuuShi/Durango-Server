using UnityEngine;

namespace Durango.UI;

public class PlayGuideHelperGroup : PlayGuideHelperGroupBase
{
	[SerializeField]
	private UISprite _clickTargetHand;

	[SerializeField]
	private TweenScale _clickTargeTweenScale;

	[SerializeField]
	private TweenAlpha _clickTargetTweenAlpha;

	private float _clickTargetPrevScale;

	private void OnEnable()
	{
		_clickTargetTweenAlpha.enabled = false;
		_clickTargetPrevScale = _clickTargetHand.transform.localScale.x;
	}

	protected new void LateUpdate()
	{
		base.LateUpdate();
		float x = _clickTargetHand.transform.localScale.x;
		bool flag = _clickTargetPrevScale > x;
		_clickTargetHand.spriteName = ((!flag) ? "guide_hand_normal" : "guide_hand_touched");
		_clickTargetPrevScale = x;
	}

	protected override void OnBeginVisible()
	{
		base.OnBeginVisible();
		_clickTargetTweenAlpha.ResetToBeginning();
		_clickTargetTweenAlpha.PlayForward();
		_clickTargeTweenScale.PlayForward();
	}
}
