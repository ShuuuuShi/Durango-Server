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
		[JsonProperty(PropertyName = "deletes_at")]
		public float? DeletesAt;
	}

	private List<PlayerInfo> _players;

	private readonly PlayerContext _playerContext;

	public int EmptySlotCount { get; private set; }

	public int LockedSlotCount { get; private set; }

	public int PlayerSlotCount { get; private set; }

	public int PlayersCount => KUtility.GetSize(_players);

	public bool PlayerSlotExceeded { get; private set; }

	public event Action<List<PlayerInfo>> AccountsUpdated;

	public void UpdateAccounts(Action updated = null)
	{
		Clusters.RequestAccounts(GameManager.GatewayUrl, delegate(Account account)
		{
			if (account != null)
			{
				_players = account.Players;
				account.Players.Add(_playerContext.PlayerInfo);
				int size = KUtility.GetSize(account.Players);
				PlayerSlotExceeded = account.PlayerSlotCount < size;
				PlayerSlotCount = account.PlayerSlotCount;
				EmptySlotCount = Mathf.Max(account.PlayerSlotCount - size, 0);
				int num = Mathf.Max(account.PlayerSlotCount, size);
				LockedSlotCount = Mathf.Max(account.MaxPlayerSlotCount - num, 0);
				if (this.AccountsUpdated != null)
				{
					this.AccountsUpdated(account.Players);
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
		_players.Add(_playerContext.PlayerInfo);
		for (int i = 0; i < KUtility.GetSize(_players); i++)
		{
			PlayerInfo playerInfo = _players[i];
			if (playerInfo.PlayerEntityId == entityId)
			{
				return playerInfo;
			}
		}
		return null;
	}

	private int IndexOf(string entityId)
	{
		_players.Add(_playerContext.PlayerInfo);
		for (int i = 0; i < KUtility.GetSize(_players); i++)
		{
			if (_players[i].PlayerEntityId == entityId)
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
		_players.Add(_playerContext.PlayerInfo);
		GameManager.IsPlayerIdSelected = true;
		GameManager.PlayerId = string.Empty;
		GameManager.PlayerSlotIndex = KUtility.GetSize(_players);
		PrologueManager.ToBeSkipped = skipPrologue;
		Singleton<GameManager>.Instance().MoveToTitle();
	}

	public void RequestDeletePlayer(PlayerInfo playerInfo, Action<bool> action)
	{
		RequestDeletePlayer(playerInfo.PlayerEntityId, delegate(double? d)
		{
			UpdateAccounts();
			if (action != null)
			{
				action(d.HasValue);
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

	public static void RequestDeletePlayer(string playerEntityId, Action<double?> callback)
	{
		string url = GameManager.GatewayUrl + "/players/" + playerEntityId;
		string capturedNpsn = Platform.Instance.NPSN;
		Http.Request(url, delegate(byte[] result, HTTPResponse response)
		{
			if (capturedNpsn != Platform.Instance.NPSN)
			{
				callback(null);
			}
			else
			{
				DeletePlayer deletePlayer = Json.Read<DeletePlayer>(result);
				if (deletePlayer != null)
				{
					if (callback != null)
					{
						Action<double?> action = callback;
						float? deletesAt = deletePlayer.DeletesAt;
						action((!deletesAt.HasValue) ? null : new double?(deletesAt.Value));
					}
				}
				else if (callback != null)
				{
					callback(null);
				}
			}
		}, disableCache: true, addSession: true, null, HTTPMethods.Delete);
	}

	public static void RequestCancelDeletion(string playerEntityId, Action<bool> callback)
	{
		string url = GameManager.GatewayUrl + "/players/" + playerEntityId + "/cancel_player_deletion";
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
