using System.Collections.Generic;

namespace Durango.Player.Animation;

public class PlayerAnimationClipTrasitionInfo
{
	public string State { get; set; }

	public string Clip { get; set; }

	public List<PlayerAnimationCondition> Conditions { get; set; }

	public void Init<T>()
	{
		int i = 0;
		for (int size = KUtility.GetSize(Conditions); i < size; i++)
		{
			Conditions[i].Init<T>();
		}
	}
}
