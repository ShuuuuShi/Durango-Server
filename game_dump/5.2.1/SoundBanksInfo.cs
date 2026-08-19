using System;
using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public class SoundBanksInfo
{
	public class SoundBanksInfoJson
	{
		public class Info
		{
			public class Bank
			{
				public class Event
				{
					public string Name;

					public string ObjectPath;
				}

				public string Language;

				public string ShortName;

				public string Path;

				public Event[] IncludedEvents;
			}

			public Bank[] SoundBanks;
		}

		public Info SoundBanksInfo;
	}

	private static readonly char[] Seperators = new char[1] { '\\' };

	private const string NonLanguage = "SFX";

	private readonly List<string> _eventIncludedBankPaths = new List<string>();

	private readonly Dictionary<string, string> _eventNameToMediaIncludedBankPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private int _version;

	public IList<string> EventIncludedBankPaths => _eventIncludedBankPaths;

	public void Initialize(string soundBanksInfoFilePath, Action<bool> onReply)
	{
		Clear();
		int requested = _version;
		Singleton<AssetBundleManager>.Instance().RequestAsset(soundBanksInfoFilePath, typeof(TextAsset), delegate(UnityEngine.Object asset)
		{
			if (requested == _version)
			{
				TextAsset textAsset = asset as TextAsset;
				if (textAsset == null)
				{
					onReply(obj: false);
				}
				else
				{
					try
					{
						ParseSoundBanksinfo(textAsset.text);
					}
					catch (Exception)
					{
						onReply(obj: false);
						return;
					}
					onReply(obj: true);
				}
			}
		});
	}

	public void Clear()
	{
		_version++;
		_eventIncludedBankPaths.Clear();
		_eventNameToMediaIncludedBankPath.Clear();
	}

	public bool ContainsEvent(string eventName)
	{
		return _eventNameToMediaIncludedBankPath.ContainsKey(eventName);
	}

	[CanBeNull]
	public string GetMediaBankPathByEventName(string eventName)
	{
		return _eventNameToMediaIncludedBankPath.Get(eventName);
	}

	private void ParseSoundBanksinfo(string text)
	{
		SoundBanksInfoJson soundBanksInfoJson = Json.Read<SoundBanksInfoJson>(text);
		if (soundBanksInfoJson == null || soundBanksInfoJson.SoundBanksInfo == null || soundBanksInfoJson.SoundBanksInfo.SoundBanks == null)
		{
			throw new Exception("파일 정보가 비어 있습니다.");
		}
		Dictionary<string, string> bankPathDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		List<SoundBanksInfoJson.Info.Bank> eventIncludedBanks = new List<SoundBanksInfoJson.Info.Bank>();
		CollectSoundBanks(soundBanksInfoJson.SoundBanksInfo.SoundBanks, eventIncludedBanks, bankPathDictionary);
		CollectEventIncludedBankPaths(eventIncludedBanks, bankPathDictionary);
	}

	private static void CollectSoundBanks(SoundBanksInfoJson.Info.Bank[] banks, List<SoundBanksInfoJson.Info.Bank> eventIncludedBanks, Dictionary<string, string> bankPathDictionary)
	{
		foreach (SoundBanksInfoJson.Info.Bank bank in banks)
		{
			if (!(bank.Language != "SFX") || !(bank.Language != LocalizeSystem.VoiceLocale))
			{
				if (bank.IncludedEvents != null)
				{
					eventIncludedBanks.Add(bank);
				}
				bankPathDictionary[bank.ShortName] = bank.Path;
			}
		}
	}

	private void CollectEventIncludedBankPaths(List<SoundBanksInfoJson.Info.Bank> eventIncludedBanks, Dictionary<string, string> bankPathDictionary)
	{
		for (int i = 0; i < eventIncludedBanks.Count; i++)
		{
			SoundBanksInfoJson.Info.Bank bank = eventIncludedBanks[i];
			_eventIncludedBankPaths.Add(bank.Path);
			CollectEventNameToMediaBankPath(bank.IncludedEvents, bankPathDictionary);
		}
	}

	private void CollectEventNameToMediaBankPath(SoundBanksInfoJson.Info.Bank.Event[] includedEvents, Dictionary<string, string> bankPathDictionary)
	{
		foreach (SoundBanksInfoJson.Info.Bank.Event @event in includedEvents)
		{
			string bankPathFromObjectPath = GetBankPathFromObjectPath(@event.ObjectPath, bankPathDictionary);
			if (bankPathFromObjectPath == null)
			{
				throw new Exception("이벤트 경로명으로부터 뱅크 경로명을 찾을 수 없습니다: " + @event.ObjectPath);
			}
			_eventNameToMediaIncludedBankPath[@event.Name] = bankPathFromObjectPath;
		}
	}

	private static string GetBankPathFromObjectPath(string objectPath, Dictionary<string, string> bankPathDictionary)
	{
		string[] array = objectPath.Split(Seperators, StringSplitOptions.None);
		for (int num = array.Length - 2; num >= 0; num--)
		{
			string text = bankPathDictionary.Get(array[num]);
			if (text != null)
			{
				return text;
			}
		}
		return null;
	}
}
