using System.Collections.Generic;

public class PlayerAnimationStateClip : PlayerAnimationClipInfo
{
	private PlayerAnimationStateInfo _parent;

	public List<PlayerAnimationCondition> Conditions { get; set; }

	public void Copy(PlayerAnimationClipInfo obj)
	{
		if (obj != null)
		{
			base.IsLoop = obj.IsLoop;
			base.Length = obj.Length;
			base.Clip = obj.Clip;
			base.Tag = obj.Tag;
			base.FadeOutTime = obj.FadeOutTime;
			base.FadeInTime = obj.FadeInTime;
			base.EquipAnimation = obj.EquipAnimation;
		}
	}

	public override void Init()
	{
		base.Init();
		int i = 0;
		for (int num = ((Conditions != null) ? Conditions.Count : 0); i < num; i++)
		{
			Conditions[i].Init<StateClipCondition>();
		}
	}

	public void SetParent(PlayerAnimationStateInfo parent)
	{
		_parent = parent;
	}

	public PlayerAnimationStateInfo GetParent()
	{
		return _parent;
	}
}
