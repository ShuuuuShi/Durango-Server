using System;
using System.Collections.Generic;
using System.Text;
using Durango.Player;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerInfoManager : Singleton<PlayerInfoManager>
{
	public static readonly PlayerInfo EmptyPlayer = new PlayerInfo();

	private AsyncCachedDictionary<string, PlayerInfo> _cachedDict;

	private readonly List<string> _requestConnectedIds = new List<string>();

	private readonly List<KeyValuePair<string, Action<PlayerConnected>>> _itemCallbackList = new List<KeyValuePair<string, Action<PlayerConnected>>>();

	private readonly Dictionary<string, KeyValuePair<PlayerConnected, float>> _playerConnectedCache = new Dictionary<string, KeyValuePair<PlayerConnected, float>>();

	private DelayedFunction _requestConnected;

	public string CurrentKey => _cachedDict.CurrentKey;

	protected override void OnAwake()
	{
		_cachedDict = new AsyncCachedDictionary<string, PlayerInfo>(RequestFunc);
		_cachedDict.InvalidKey = string.Empty;
		_cachedDict.EmptyValue = EmptyPlayer;
		_cachedDict.OnPreRequest = OnPreRequest;
		_requestConnected = new DelayedFunction(RequestPlayersConnected);
	}

	private void Start()
	{
		if (Singleton<PlayerManager>.HasInstance())
		{
			Singleton<PlayerManager>.Instance().PlayerAppeared += PlayerManager_Updated;
			Singleton<PlayerManager>.Instance().DisplayUpdated += PlayerManager_Updated;
			Singleton<PlayerManager>.Instance().PlayerClanChanged += PlayerManager_PlayerClanChanged;
		}
	}

	private void PlayerManager_Updated([NotNull] PlayerBehavior player)
	{
		if (_cachedDict.TryGetCachedValue(player.EntityId, out var value))
		{
			value.Display = player.Display;
			value.Level = player.Level;
			value.ClanId = player.ClanId;
			value.ClanName = player.Clan.ClanName;
		}
	}

	private void PlayerManager_PlayerClanChanged([NotNull] PlayerBehavior player)
	{
		if (_cachedDict.TryGetCachedValue(player.EntityId, out var value))
		{
			value.ClanId = player.ClanId;
			value.ClanName = player.Clan.ClanName;
		}
	}

	private static void RequestFunc(string key, PlayerInfo cachedInfo, Action<string, PlayerInfo> onResult)
	{
		Http.RequestYml(GameManager.GatewayUrl + "/players/" + key, delegate(PlayerInfoJson json)
		{
			PlayerInfo playerInfo = cachedInfo;
			if (string.IsNullOrEmpty(json.EntityId))
			{
				playerInfo = EmptyPlayer;
			}
			else
			{
				if (playerInfo == null)
				{
					playerInfo = new PlayerInfo();
				}
				playerInfo.Set(json);
			}
			if (Singleton<PlayerManager>.HasInstance())
			{
				PlayerBehavior playerIncludeLocalPlayer = Singleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(key);
				if (playerIncludeLocalPlayer != null)
				{
					playerInfo.Display = playerIncludeLocalPlayer.Display;
					playerInfo.Level = playerIncludeLocalPlayer.Level;
					playerInfo.ClanId = playerIncludeLocalPlayer.ClanId;
					playerInfo.ClanName = playerIncludeLocalPlayer.Clan.ClanName;
				}
			}
			onResult(key, playerInfo);
		});
	}

	private bool OnPreRequest(string id, out PlayerInfo info)
	{
		if (string.IsNullOrEmpty(id) || id == "1000")
		{
			info = EmptyPlayer;
			return true;
		}
		info = null;
		return false;
	}

	public void RefreshPlayerInfos(IList<string> entityIds)
	{
		_cachedDict.Refresh(entityIds);
	}

	public void RequestPlayerInfos(IList<string> entityIds, [NotNull] Action<PlayerInfo[]> response)
	{
		_cachedDict.Request(entityIds, response);
	}

	public void RequestNewPlayerInfos(IList<string> entityIds, [NotNull] Action<PlayerInfo[]> response)
	{
		_cachedDict.Request(entityIds, response, refresh: true);
	}

	public void RequestPlayerInfo([CanBeNull] string entityId, [NotNull] Action<PlayerInfo> response)
	{
		_cachedDict.Request(entityId, response);
	}

	public void RequestNewPlayerInfo([CanBeNull] string entityId, [NotNull] Action<PlayerInfo> response)
	{
		_cachedDict.Request(entityId, response, refresh: true);
	}

	[NotNull]
	public PlayerInfo GetCachedPlayerInfoOrEmpty(string entityId)
	{
		if (_cachedDict.TryGetCachedValue(entityId, out var value))
		{
			return value;
		}
		return EmptyPlayer;
	}

	public void SearchPlayerInfos(string searchKey, string searchFreq, Action<FoundPlayerInfo[]> response)
	{
		if (response != null && !string.IsNullOrEmpty(searchKey))
		{
			string text = WWW.EscapeURL(searchKey.Trim());
			string text2 = ((searchFreq != null) ? WWW.EscapeURL(searchFreq.Trim()) : string.Empty);
			Http.RequestYml(GameManager.GatewayUrl + "/players?name=" + text + "&freq=" + text2, delegate(FoundPlayersJson json)
			{
				response(json.Players);
			});
		}
	}

	private void RequestPlayersConnected()
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		value.Append(GameManager.GatewayUrl);
		value.Append("/online_statuses?");
		int num = 0;
		int i = 0;
		for (int size = KUtility.GetSize(_requestConnectedIds); i < size; i++)
		{
			string value2 = _requestConnectedIds[i];
			if (!string.IsNullOrEmpty(value2))
			{
				if (num > 0)
				{
					value.Append('&');
				}
				value.Append("entity_id=");
				value.Append(value2);
				num++;
			}
		}
		_requestConnectedIds.Clear();
		if (num > 0)
		{
			Http.RequestYml<Dictionary<string, PlayerConnected>>(value.ToString(), OnConnectedInfo);
		}
	}

	private void OnConnectedInfo(Dictionary<string, PlayerConnected> data)
	{
		if (data != null)
		{
			float time = Time.time;
			foreach (KeyValuePair<string, PlayerConnected> datum in data)
			{
				_playerConnectedCache[datum.Key] = new KeyValuePair<PlayerConnected, float>(datum.Value, time);
			}
		}
		int i = 0;
		for (int count = _itemCallbackList.Count; i < count; i++)
		{
			KeyValuePair<string, Action<PlayerConnected>> keyValuePair = _itemCallbackList[i];
			keyValuePair.Value(data?.Get(keyValuePair.Key) ?? default(PlayerConnected));
		}
		_itemCallbackList.Clear();
	}

	public void GetPlayerConnected([CanBeNull] string entityId, [NotNull] Action<PlayerConnected> onResult)
	{
		if (string.IsNullOrEmpty(entityId))
		{
			onResult(new PlayerConnected
			{
				Online = false
			});
			return;
		}
		if (entityId == GameManager.PlayerId)
		{
			onResult(new PlayerConnected
			{
				Online = true
			});
			return;
		}
		if (_playerConnectedCache.TryGetValue(entityId, out var value))
		{
			float time = Time.time;
			if (value.Value + 60f > time)
			{
				onResult(value.Key);
				return;
			}
		}
		_requestConnectedIds.Add(entityId);
		_itemCallbackList.Add(new KeyValuePair<string, Action<PlayerConnected>>(entityId, onResult));
		_requestConnected.Call(this);
	}
}
