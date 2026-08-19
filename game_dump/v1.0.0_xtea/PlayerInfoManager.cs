using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using L10N;
using Player;
using UnityEngine;

public class PlayerInfoManager : KSingleton<PlayerInfoManager>
{
	public static readonly PlayerInfo EmptyPlayer = new PlayerInfo();

	private AsyncCachedDictionary<ulong, PlayerInfo> _cachedDict;

	private static PlayerInfo _systemChatInfo;

	private static PlayerInfo SystemChatInfo
	{
		get
		{
			if (_systemChatInfo == null)
			{
				PlayerInfoJson playerInfoJson = new PlayerInfoJson();
				playerInfoJson.entity_id = 0uL;
				playerInfoJson.name = T._("시스템");
				_systemChatInfo = new PlayerInfo();
				_systemChatInfo.Set(playerInfoJson);
			}
			return _systemChatInfo;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_cachedDict = new AsyncCachedDictionary<ulong, PlayerInfo>(RequestFunc);
		_cachedDict.InvalidKey = 0uL;
		_cachedDict.EmptyValue = EmptyPlayer;
	}

	private void RequestFunc(ulong key, PlayerInfo cachedInfo, Action<ulong, PlayerInfo> onResult)
	{
		string url = $"{KSingleton<GameManager>.Instance().GatewayUrl}players/{key}";
		KUtility.RequestYml(url, delegate(PlayerInfoJson json)
		{
			PlayerInfo playerInfo = cachedInfo;
			if (playerInfo == null)
			{
				playerInfo = new PlayerInfo();
			}
			if (json == null)
			{
				playerInfo.Set(key);
			}
			else
			{
				playerInfo.Set(json);
			}
			onResult(key, playerInfo);
		});
	}

	public void RequestPlayerInfos(IList<ulong> entityIds, [NotNull] Action<PlayerInfo[]> response, bool useOldCache = false)
	{
		_cachedDict.Request(entityIds, response, !useOldCache);
	}

	public void RequestPlayerInfo(ulong entityId, [NotNull] Action<PlayerInfo> response, bool useOldCache = false)
	{
		if (entityId == 0L)
		{
			response(SystemChatInfo);
		}
		else
		{
			_cachedDict.Request(entityId, response, !useOldCache);
		}
	}

	public void SearchPlayerInfos(string searchKey, Action<PlayerInfo[]> response)
	{
		if (response == null || string.IsNullOrEmpty(searchKey))
		{
			return;
		}
		string url = $"{KSingleton<GameManager>.Instance().GatewayUrl}players?name={WWW.EscapeURL(searchKey.Trim())}";
		KUtility.RequestYml(url, delegate(FoundPlayersJson json)
		{
			if (json != null && json.players != null)
			{
				PlayerInfo[] array = new PlayerInfo[json.players.Length];
				int i = 0;
				for (int num = array.Length; i < num; i++)
				{
					array[i] = new PlayerInfo();
					array[i].Set(json.players[i]);
				}
				response(array);
			}
		});
	}
}
