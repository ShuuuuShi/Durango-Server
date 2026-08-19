using System.Collections.Generic;

public class PlayerAnimationClipTrasitionInfo
{
	public string State { get; set; }

	public string Clip { get; set; }

	public List<PlayerAnimationCondition> Conditions { get; set; }

	public void Init<T>()
	{
		int i = 0;
		for (int num = ((Conditions != null) ? Conditions.Count : 0); i < num; i++)
		{
			Conditions[i].Init<T>();
		}
	}
}
