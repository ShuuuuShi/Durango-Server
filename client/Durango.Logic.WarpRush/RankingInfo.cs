using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Season2;

namespace Durango.Logic.WarpRush;

public class RankingInfo
{
	[JsonProperty(PropertyName = "at")]
	public double At;

	[JsonProperty(PropertyName = "high_scores")]
	public List<Record> HighScores;

	[JsonProperty(PropertyName = "revision")]
	public string RevisionId;

	[JsonProperty(PropertyName = "my_rank")]
	public MyRecord MyRecord;

	public static string ScoreToText(int[] scores, bool isEmphatic)
	{
		if (scores == null || scores.Length != 3)
		{
			return string.Empty;
		}
		return string.Format("[size=26][icon={0}][/size] {1}      [size=26][icon={2}][/size] {3}", WarpRushSystem.GetResourceIcon(ResourceType.AlphaStone), (!isEmphatic) ? scores[1].ToString("N0") : $"<em>{scores[1]:N0}</em>", WarpRushSystem.GetResourceIcon(ResourceType.BravoStone), (!isEmphatic) ? scores[0].ToString("N0") : $"<em>{scores[0]:N0}</em>");
	}
}
