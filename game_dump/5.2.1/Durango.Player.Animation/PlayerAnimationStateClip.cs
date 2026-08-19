using System.Collections.Generic;

namespace Durango.Player.Animation;

public class PlayerAnimationStateClip : PlayerAnimationClipInfoBase
{
	private PlayerAnimationStateInfo _parent;

	public List<PlayerAnimationClipTrasitionInfo> Transitions { get; set; }

	public List<PlayerAnimationCondition> Conditions { get; set; }

	public override void Init()
	{
		base.Init();
		int i = 0;
		for (int size = KUtility.GetSize(Transitions); i < size; i++)
		{
			Transitions[i].Init<TransitionCondition>();
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(Conditions); j < size2; j++)
		{
			Conditions[j].Init<StateClipCondition>();
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

	public float GetConditionValue(PlayerAnimationConditionArguments arguments)
	{
		if (Conditions == null)
		{
			return 0f;
		}
		float num = 0f;
		int count = Conditions.Count;
		for (int i = 0; i < count; i++)
		{
			float contionValue = Conditions[i].GetContionValue(arguments);
			if (contionValue > 0f)
			{
				num += contionValue;
			}
			else if (contionValue < 0f)
			{
				num = -1f;
				break;
			}
		}
		return num;
	}
}
