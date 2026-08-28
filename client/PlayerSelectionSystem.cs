using System;
using System.Collections.Generic;
using BestHTTP;
using Durango.Logic.Clusters;
using Durango.Offline;
using Durango.Prologue;
using Durango.System;
using Durango.Utils;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerSelectionSystem : GameSystem<PlayerSelectionSystem>
{
	private class DeletePlayer
	{
		[JsonProperty(PropertyName = "deleted")]
		public bool Deleted;
	}

	private List<PlayerInfo> _players;

	public int EmptySlotCount { get; private set; }

	public int LockedSlotCount { get; private set; }

	public int PlayerSlotCount { get; private set; }

	public int PlayersCount => KUtility.GetSize(_players);

	public bool PlayerSlotExceeded { get; private set; }

	public event Action<List<PlayerInfo>> AccountsUpdated;

	// Character data is hosted by the separate character server.  In local
	// SingleMode GameManager.GatewayUrl can point at the gameplay gateway (8390),
	// which does not own /accounts or /players.
	public static string CharacterGatewayUrl
	{
		get
		{
			string gatewayUrl = GameManager.GatewayUrl;
			if (string.IsNullOrEmpty(gatewayUrl) || gatewayUrl.IndexOf(":8390", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "http://127.0.0.1:8190";
			}
			return gatewayUrl.TrimEnd('/');
		}
	}

	public void UpdateAccounts(Action updated = null)
	{
		Clusters.RequestAccounts(CharacterGatewayUrl, delegate(Account account)
		{
			if (account != null)
			{
				_players = account.Players ?? new List<PlayerInfo>();
				int size = KUtility.GetSize(_players);
				PlayerSlotExceeded = account.PlayerSlotCount < size;
				PlayerSlotCount = account.PlayerSlotCount;
				EmptySlotCount = Mathf.Max(account.PlayerSlotCount - size, 0);
				int num = Mathf.Max(account.PlayerSlotCount, size);
				LockedSlotCount = Mathf.Max(account.MaxPlayerSlotCount - num, 0);
				if (this.AccountsUpdated != null)
				{
					this.AccountsUpdated(_players);
				}
				if (updated != null)
				{
					updated();
				}
			}
		});
	}

	public PlayerInfo FindPlayerInfo(string entityId)
	{
		if (_players == null)
		{
			return null;
		}
		for (int i = 0; i < KUtility.GetSize(_players); i++)
		{
			PlayerInfo playerInfo = _players[i];
			if (playerInfo != null && playerInfo.PlayerEntityId == entityId)
			{
				return playerInfo;
			}
		}
		return null;
	}

	private int IndexOf(string entityId)
	{
		if (_players == null)
		{
			return -1;
		}
		for (int i = 0; i < KUtility.GetSize(_players); i++)
		{
			if (_players[i] != null && _players[i].PlayerEntityId == entityId)
			{
				return i;
			}
		}
		return -1;
	}

	public void ChangePlayer(string playerEntityId)
	{
		GameManager.IsPlayerIdSelected = true;
		GameManager.PlayerId = playerEntityId;
		GameManager.PlayerSlotIndex = IndexOf(playerEntityId);
		Singleton<GameManager>.Instance().MoveToTitle();
	}

	public void CreateNewPlayer(bool skipPrologue)
	{
		if (_players == null)
		{
			_players = new List<PlayerInfo>();
		}
		GameManager.IsPlayerIdSelected = true;
		GameManager.PlayerId = string.Empty;
		GameManager.PlayerSlotIndex = KUtility.GetSize(_players);
		PrologueManager.ToBeSkipped = skipPrologue;
		Singleton<GameManager>.Instance().MoveToTitle();
	}

	public void RequestDeletePlayer(PlayerInfo playerInfo, Action<bool> action)
	{
		RequestDeletePlayer(playerInfo.PlayerEntityId, delegate(bool success)
		{
			if (success && _players != null)
			{
				_players.RemoveAll((PlayerInfo player) => player != null && player.PlayerEntityId == playerInfo.PlayerEntityId);
			}
			UpdateAccounts();
			if (action != null)
			{
				action(success);
			}
		});
	}

	public void RequestCancelDeletion(PlayerInfo playerInfo, Action<bool> action)
	{
		RequestCancelDeletion(playerInfo.PlayerEntityId, delegate(bool isSuccess)
		{
			UpdateAccounts();
			if (action != null)
			{
				action(isSuccess);
			}
		});
	}

	public static void RequestDeletePlayer(string playerEntityId, Action<bool> callback)
	{
		string capturedNpsn = Platform.Instance.NPSN;
		string url = CharacterGatewayUrl + "/players/" + playerEntityId + "?account_id=" + Uri.EscapeDataString(capturedNpsn);
		Http.Request(url, delegate(byte[] result, HTTPResponse response)
		{
			if (capturedNpsn != Platform.Instance.NPSN || response == null || !response.IsSuccess)
			{
				if (callback != null)
				{
					callback(obj: false);
				}
			}
			else
			{
				DeletePlayer deletePlayer = Json.Read<DeletePlayer>(result);
				if (callback != null)
				{
					callback(deletePlayer != null && deletePlayer.Deleted);
				}
			}
		}, disableCache: true, addSession: true, null, HTTPMethods.Delete);
	}

	public static void RequestCancelDeletion(string playerEntityId, Action<bool> callback)
	{
		string url = $"{CharacterGatewayUrl}/players/{playerEntityId}/cancel_player_deletion";
		string capturedNpsn = Platform.Instance.NPSN;
		Http.Request(url, delegate(byte[] result, HTTPResponse response)
		{
			if (capturedNpsn != Platform.Instance.NPSN)
			{
				callback(obj: false);
			}
			else if (callback != null)
			{
				callback(response.IsSuccess);
			}
		}, disableCache: true, addSession: true, null, HTTPMethods.Post);
	}
}
