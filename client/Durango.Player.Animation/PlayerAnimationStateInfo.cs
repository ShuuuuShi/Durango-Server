using System.Collections.Generic;

namespace Durango.Player.Animation;

public class PlayerAnimationStateInfo
{
	public string State;

	public List<PlayerAnimationStateClip> Clips;

	public void Init()
	{
		int i = 0;
		for (int size = KUtility.GetSize(Clips); i < size; i++)
		{
			Clips[i].Init();
			Clips[i].SetParent(this);
		}
	}

	public PlayerAnimationStateClip Get(PlayerAnimationConditionArguments arguments)
	{
		PlayerAnimationStateClip result = null;
		int size = KUtility.GetSize(Clips);
		float num = float.MinValue;
		for (int i = 0; i < size; i++)
		{
			PlayerAnimationStateClip playerAnimationStateClip = Clips[i];
			float conditionValue = playerAnimationStateClip.GetConditionValue(arguments);
			if (conditionValue > num)
			{
				num = conditionValue;
				result = playerAnimationStateClip;
			}
		}
		return result;
	}
}
