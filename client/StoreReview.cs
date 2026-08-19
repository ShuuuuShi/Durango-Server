using System.Collections.Generic;
using Durango.Network;
using Durango.System;
using Durango.Utils;
using Messages;
using UnityEngine;

public static class StoreReview
{
	private static bool _isReviewed;

	private const string StorageKey = "store_review";

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void InstallEvent()
	{
		GameManager.Started += delegate
		{
			Singleton<GameManager>.Instance().WelcomeReceived += delegate(Welcome welcome)
			{
				LoadStorage(welcome.Storage.Data);
			};
		};
	}

	public static void LoadStorage(Dictionary<string, byte[]> storage)
	{
		byte[] collection = storage.Get("store_review");
		_isReviewed = KUtility.GetSize(collection) > 0;
	}

	private static void SetReviewed()
	{
		_isReviewed = true;
		Preferences.SetBool("store_review", value: true);
		SetStorageItem msg = default(SetStorageItem);
		msg.Key = "store_review";
		msg.Value = new byte[1];
		Connections.Frontend.Send(msg);
	}

	private static bool IsAlreadyReviewed()
	{
		return _isReviewed || Preferences.GetBool("store_review");
	}

	public static void Request()
	{
		if (!IsAlreadyReviewed() && Preferences.CheckTimePassed("last_store_review", 86400))
		{
			GameSystem<PlayGuideSystem>.Instance().ReloadFlow("store_review", FlowFinished);
			GameSystem<PlayGuideSystem>.Instance().BeginFlow("store_review");
		}
	}

	private static void FlowFinished()
	{
		if (GameSystem<PlayGuideSystem>.Instance().LastQuizAnswer == 1)
		{
			GoToRateUrl();
		}
	}

	private static void GoToRateUrl()
	{
		SetReviewed();
		string url = ((Application.platform != RuntimePlatform.Android) ? "https://apps.apple.com/app/id1281442514?action=write-review" : $"https://play.google.com/store/apps/details?id={Platform.Instance.AppBundleId}");
		Application.OpenURL(url);
	}
}
