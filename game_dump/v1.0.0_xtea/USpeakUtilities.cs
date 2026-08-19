using System.Collections.Generic;
using UnityEngine;

public class USpeakUtilities
{
	public static string USpeakerPrefabPath = "USpeakerPrefab";

	public static void PlayerJoined(string PlayerID)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		GameObject val = (GameObject)Object.Instantiate(Resources.Load("USpeakerPrefab"));
		USpeakOwnerInfo uSpeakOwnerInfo = val.AddComponent<USpeakOwnerInfo>();
		USpeakPlayer uSpeakPlayer = new USpeakPlayer();
		uSpeakPlayer.PlayerID = PlayerID;
		uSpeakOwnerInfo.Init(uSpeakPlayer);
	}

	public static void PlayerLeft(string PlayerID)
	{
		USpeakOwnerInfo.FindPlayerByID(PlayerID).DeInit();
	}

	public static void ListPlayers(IEnumerable<string> PlayerIDs)
	{
		foreach (string PlayerID in PlayerIDs)
		{
			PlayerJoined(PlayerID);
		}
	}

	public static void Clear()
	{
		foreach (string key in USpeakOwnerInfo.USpeakPlayerMap.Keys)
		{
			USpeakOwnerInfo.USpeakPlayerMap[key].DeInit();
		}
	}
}
