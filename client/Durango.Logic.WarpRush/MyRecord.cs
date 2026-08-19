using Newtonsoft.Json;
using Shared.Season2;

namespace Durango.Logic.WarpRush;

public class MyRecord
{
	[JsonProperty(PropertyName = "rank")]
	public int Rank;

	[JsonProperty(PropertyName = "score")]
	public int[] Scores;

	public int GetResource(ResourceType type)
	{
		if (Scores.Length < 3)
		{
			return 0;
		}
		return type switch
		{
			ResourceType.AlphaStone => Scores[1], 
			ResourceType.BravoStone => Scores[0], 
			_ => 0, 
		};
	}

	public string GetScoreText()
	{
		return RankingInfo.ScoreToText(Scores, isEmphatic: true);
	}
}
