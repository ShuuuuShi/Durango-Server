using System.Collections.Generic;

public class PlayerAnimationBlendTree
{
	public string Name { get; set; }

	public List<PlayerAnimationBlendTreeNode> Clips { get; set; }

	public string Parameter { get; set; }

	public float ParamMin { get; set; }

	public float ParamMax { get; set; }

	public void CalcParameter()
	{
		if (Clips == null)
		{
			return;
		}
		Clips.Sort((PlayerAnimationBlendTreeNode a, PlayerAnimationBlendTreeNode b) => (a.Param > b.Param) ? 1 : ((a.Param < b.Param) ? (-1) : 0));
		float num = ParamMax - ParamMin;
		int count = Clips.Count;
		for (int i = 0; i < Clips.Count; i++)
		{
			PlayerAnimationBlendTreeNode playerAnimationBlendTreeNode = Clips[i];
			playerAnimationBlendTreeNode.Weight = 0f;
			PlayerAnimationBlendTreeNode playerAnimationBlendTreeNode2 = ((i != 0) ? Clips[i - 1] : Clips[count - 1]);
			PlayerAnimationBlendTreeNode playerAnimationBlendTreeNode3 = ((i != count - 1) ? Clips[i + 1] : Clips[0]);
			if (playerAnimationBlendTreeNode.Param < playerAnimationBlendTreeNode2.Param)
			{
				playerAnimationBlendTreeNode.Min = playerAnimationBlendTreeNode2.Param - num;
			}
			else
			{
				playerAnimationBlendTreeNode.Min = playerAnimationBlendTreeNode2.Param;
			}
			if (playerAnimationBlendTreeNode.Param > playerAnimationBlendTreeNode3.Param)
			{
				playerAnimationBlendTreeNode.Max = playerAnimationBlendTreeNode3.Param + num;
			}
			else
			{
				playerAnimationBlendTreeNode.Max = playerAnimationBlendTreeNode3.Param;
			}
		}
	}
}
