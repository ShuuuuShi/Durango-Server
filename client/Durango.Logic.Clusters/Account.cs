using System.Collections.Generic;
using Durango.Utils.Extensions;
using Newtonsoft.Json;

namespace Durango.Logic.Clusters;

public class Account
{
	[JsonProperty(PropertyName = "players")]
	public List<PlayerInfo> Players;

	[JsonProperty(PropertyName = "max_player_slot_count")]
	public int MaxPlayerSlotCount;

	[JsonProperty(PropertyName = "player_slot_count")]
	public int PlayerSlotCount;

	private Pair<string, int> _recommendedPlayerIdCache;

	public Pair<string, int> GetRecommendedPlayer()
	{
		if (KUtility.GetSize(Players) == 0)
		{
			return new Pair<string, int>(string.Empty, 0);
		}
		if (_recommendedPlayerIdCache.Item1 == string.Empty)
		{
			return _recommendedPlayerIdCache;
		}
		if (_recommendedPlayerIdCache.Item1 != null && Players.Exists((PlayerInfo elem) => elem.PlayerEntityId == _recommendedPlayerIdCache.Item1))
		{
			return _recommendedPlayerIdCache;
		}
		PlayerInfo lastConnected = Players.MaxBy((PlayerInfo elem) => elem.DisconnectedAt);
		int item = Players.FindIndex((PlayerInfo elem) => elem == lastConnected);
		return ApplyRecommendedPlayer(new Pair<string, int>(lastConnected.PlayerEntityId, item));
	}

	public Pair<string, int> ApplyRecommendedPlayer(Pair<string, int> idToSlot)
	{
		_recommendedPlayerIdCache = idToSlot;
		return idToSlot;
	}

	public string GetPlayerInfoText(string id)
	{
		int i = 0;
		for (int size = KUtility.GetSize(Players); i < size; i++)
		{
			PlayerInfo playerInfo = Players[i];
			if (playerInfo.PlayerEntityId == id)
			{
				return playerInfo.GetPlayerInfoText();
			}
		}
		return null;
	}
}
