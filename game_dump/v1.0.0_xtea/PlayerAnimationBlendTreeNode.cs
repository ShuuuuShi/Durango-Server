using UnityEngine;

public class PlayerAnimationBlendTreeNode
{
	private AnimationState _linkAnim;

	public string Clip { get; set; }

	public float Param { get; set; }

	public float Weight { get; set; }

	public float Min { get; set; }

	public float Max { get; set; }

	public AnimationState GetLinkAnim()
	{
		return _linkAnim;
	}

	public void SetLinkAnim(AnimationState state)
	{
		_linkAnim = state;
	}
}
