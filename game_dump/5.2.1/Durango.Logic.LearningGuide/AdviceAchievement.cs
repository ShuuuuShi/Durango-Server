namespace Durango.Logic.LearningGuide;

public class AdviceAchievement
{
	public float Ratio { get; set; }

	public bool Achieved => Ratio >= 1f;

	public bool CanReward { get; set; }

	public bool Complete
	{
		get
		{
			if (Achieved)
			{
				return !CanReward;
			}
			return false;
		}
	}
}
