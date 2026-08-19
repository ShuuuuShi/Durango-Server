using System.Collections.Generic;

public class PlayerAnimationClipInfo
{
	public string Clip { get; set; }

	public bool IsLoop { get; set; }

	public float Length { get; set; }

	public PlayerAnimationClipTag Tag { get; set; }

	public float FadeOutTime { get; set; }

	public float FadeInTime { get; set; }

	public string EquipAnimation { get; set; }

	public List<PlayerAnimationClipTrasitionInfo> Transitions { get; set; }

	public PlayerAnimationClipInfo()
	{
		FadeInTime = -1f;
		FadeOutTime = -1f;
	}

	public bool HasTag(PlayerAnimationClipTag tag)
	{
		return (Tag & tag) != 0;
	}

	public virtual void Init()
	{
		int i = 0;
		for (int num = ((Transitions != null) ? Transitions.Count : 0); i < num; i++)
		{
			Transitions[i].Init<TransitionCondition>();
		}
	}
}
