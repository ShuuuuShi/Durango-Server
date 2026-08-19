using System.Collections.Generic;

public class PlayerAnimationStateInfo
{
	public string State;

	public List<PlayerAnimationStateClip> Clips;

	public List<PlayerAnimationClipTrasitionInfo> StateTransitions;

	public void Init()
	{
		int i = 0;
		for (int num = ((Clips != null) ? Clips.Count : 0); i < num; i++)
		{
			Clips[i].Init();
			Clips[i].SetParent(this);
		}
		int j = 0;
		for (int num2 = ((StateTransitions != null) ? StateTransitions.Count : 0); j < num2; j++)
		{
			StateTransitions[j].Init<StateClipCondition>();
		}
	}
}
