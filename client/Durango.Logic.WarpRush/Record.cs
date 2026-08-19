using Newtonsoft.Json;

namespace Durango.Logic.WarpRush;

public struct Record
{
	[JsonProperty(PropertyName = "entity_id")]
	public string EntityId;

	[JsonProperty(PropertyName = "freq")]
	public int Freq;

	[JsonProperty(PropertyName = "name")]
	public string Name;

	[JsonProperty(PropertyName = "score")]
	public int[] Scores;

	public string GetScoreText(bool isEmphatic = false)
	{
		return RankingInfo.ScoreToText(Scores, isEmphatic);
	}
}
