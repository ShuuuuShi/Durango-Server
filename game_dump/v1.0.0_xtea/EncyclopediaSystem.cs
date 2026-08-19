using System;
using System.Collections;
using System.Collections.Generic;
using EncyclopediaData;
using JetBrains.Annotations;
using K1Network;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class EncyclopediaSystem : GameSystem<EncyclopediaSystem>
{
	public const string StorageKey = "encyclopedia";

	private EncyclopediaStorage _storage;

	private static bool _isMemoRangeInit;

	private static List<KeyValuePair<MemoType, int>> _memosRange;

	public event Action<MemoType, int> MemoCollected;

	private void Awake()
	{
		Connections.Frontend.On<MemoCollected>(OnMemoCollect);
		KSingleton<GameManager>.Instance().StorageLoaded += InitStorage;
		KSingleton<GameManager>.Instance().Ready += delegate
		{
			Connections.Frontend.Send(default(GetMemos)).On<Memos>(OnMemos);
		};
	}

	private void InitStorage(Dictionary<string, byte[]> storage)
	{
		byte[] array = storage?.Get("encyclopedia");
		_storage = ((array != null) ? KUtility.ParseJson<EncyclopediaStorage>(array) : default(EncyclopediaStorage));
		RemoveOverMaxIndex();
	}

	private void SaveStorage()
	{
		KSingleton<GameManager>.Instance().AddOnReady(OnSaveStorage);
	}

	private void OnSaveStorage()
	{
		SetStorageItem msg = default(SetStorageItem);
		msg.Key = "encyclopedia";
		msg.Value = KUtility.SerializeJsonToBytes(_storage);
		Connections.Frontend.Send(msg);
	}

	[NotNull]
	public List<KeyValuePair<MemoType, List<int>>> GetAvailableMemoList()
	{
		if (_storage.Memo.Memos == null)
		{
			_storage.Memo.Memos = new List<KeyValuePair<MemoType, List<int>>>();
		}
		return _storage.Memo.Memos;
	}

	[NotNull]
	public List<int> GetAvailableMemoList(MemoType type)
	{
		List<KeyValuePair<MemoType, List<int>>> availableMemoList = GetAvailableMemoList();
		List<int> list = null;
		int i = 0;
		for (int count = availableMemoList.Count; i < count; i++)
		{
			if (availableMemoList[i].Key == type)
			{
				list = availableMemoList[i].Value;
				break;
			}
		}
		if (list == null)
		{
			list = new List<int>();
			availableMemoList.Add(new KeyValuePair<MemoType, List<int>>(type, list));
		}
		return list;
	}

	private void _SetMemoAvailable(MemoType type, int index)
	{
		int lastMemoIndex = GetLastMemoIndex(type);
		if (index > lastMemoIndex)
		{
			return;
		}
		List<int> availableMemoList = GetAvailableMemoList(type);
		if (!availableMemoList.Contains(index))
		{
			availableMemoList.Add(index);
			availableMemoList.Sort();
			SaveStorage();
			if (this.MemoCollected != null)
			{
				this.MemoCollected(type, index);
			}
		}
	}

	private void OnMemos(Memos msg, PacketHeader header)
	{
		int count = SingletonDict<int, Memo>.Instance.Count;
		List<KeyValuePair<MemoType, int>> memosRange = _memosRange;
		int i = 0;
		for (int num = memosRange?.Count ?? 0; i < num; i++)
		{
			KeyValuePair<MemoType, int> keyValuePair = memosRange[i];
			if (keyValuePair.Key == MemoType.Submemo)
			{
				memosRange[i] = new KeyValuePair<MemoType, int>(keyValuePair.Key, count);
				break;
			}
		}
		BitArray bitArray = new BitArray(msg._Memos);
		List<int> availableMemoList = GetAvailableMemoList(MemoType.Submemo);
		availableMemoList.Clear();
		int j = 0;
		for (int length = bitArray.Length; j < length && j < count; j++)
		{
			if (bitArray[j])
			{
				availableMemoList.Add(j + 1);
			}
		}
		SaveStorage();
	}

	private void OnMemoCollect(MemoCollected msg, PacketHeader header)
	{
		_SetMemoAvailable(MemoType.Submemo, msg.Number);
	}

	private void RemoveOverMaxIndex()
	{
		List<KeyValuePair<MemoType, List<int>>> availableMemoList = GetAvailableMemoList();
		int i = 0;
		for (int count = availableMemoList.Count; i < count; i++)
		{
			MemoType key = availableMemoList[i].Key;
			int num = FindMemoMaxIndex(key);
			for (int num2 = availableMemoList[i].Value.Count - 1; num2 >= 0; num2--)
			{
				if (availableMemoList[i].Value[num2] > num)
				{
					availableMemoList[i].Value.RemoveAt(num2);
				}
			}
		}
	}

	private static void InitMemosRange()
	{
		if (!_isMemoRangeInit)
		{
			_isMemoRangeInit = true;
			_memosRange = new List<KeyValuePair<MemoType, int>>();
			Array values = Enum.GetValues(typeof(MemoType));
			int i = 0;
			for (int length = values.Length; i < length; i++)
			{
				MemoType memoType = (MemoType)(int)values.GetValue(i);
				int value = FindMemoMaxIndex(memoType);
				_memosRange.Add(new KeyValuePair<MemoType, int>(memoType, value));
			}
		}
	}

	private static int FindMemoMaxIndex(MemoType type)
	{
		int num = 0;
		if (!IsServerMemo(type))
		{
			string text = $"#{GetLocalizePostfix(type)}";
			List<string> sequenceKeys = LocalizeSystem.GetSequenceKeys(text, numberOnly: true);
			int num2 = text.Length + 1;
			int i = 0;
			for (int count = sequenceKeys.Count; i < count; i++)
			{
				string text2 = sequenceKeys[i];
				if (num2 < text2.Length && int.TryParse(text2.Substring(num2), out var result))
				{
					num = Mathf.Max(num, result);
				}
			}
		}
		return num;
	}

	public static void SetMemoAvailable(MemoType type, int index)
	{
		if (GameSystem<EncyclopediaSystem>.HasInstance() && !IsServerMemo(type))
		{
			GameSystem<EncyclopediaSystem>.Instance()._SetMemoAvailable(type, index);
		}
	}

	public static string GetMemoFullText(MemoType type, int index)
	{
		string memoTitle = GetMemoTitle(type, index);
		string memoText = GetMemoText(type, index);
		return $"[c29e73FF]{memoTitle}[-]\n[9f9282FF]{memoText}[-]";
	}

	public static string GetMemoTitle(MemoType type, int index)
	{
		if (IsServerMemo(type))
		{
			return SingletonDict<int, Memo>.Get(index)?.name ?? ((Gettext)null);
		}
		string format = $"#{GetLocalizePostfix(type)}_title_format";
		return LocalizeSystem.Format(format, index.ToString());
	}

	public static string GetMemoText(MemoType type, int index)
	{
		if (IsServerMemo(type))
		{
			return SingletonDict<int, Memo>.Get(index)?.content ?? ((Gettext)null);
		}
		string text = $"#{GetLocalizePostfix(type)}_{index}";
		string text2 = LocalizeSystem.Get(text);
		return (!(text == text2)) ? text2 : null;
	}

	private static bool IsServerMemo(MemoType type)
	{
		if (type == MemoType.Submemo)
		{
			return true;
		}
		return false;
	}

	private static string GetLocalizePostfix(MemoType type)
	{
		return type switch
		{
			MemoType.Fiction => "fiction", 
			MemoType.Tooltip => "shortcut_tooltip", 
			MemoType.Survival => "survival_memo", 
			MemoType.Submemo => "submemo", 
			_ => null, 
		};
	}

	public static int RandomMemoGet(MemoType type, bool save = true)
	{
		int lastMemoIndex = GetLastMemoIndex(type);
		if (lastMemoIndex == 0)
		{
			return -1;
		}
		for (int i = 0; i < 100; i++)
		{
			int num = Random.Range(1, lastMemoIndex);
			string memoText = GetMemoText(type, num);
			if (!string.IsNullOrEmpty(memoText))
			{
				if (save)
				{
					SetMemoAvailable(type, num);
				}
				return num;
			}
		}
		return -1;
	}

	public static int GetLastMemoIndex(MemoType type)
	{
		InitMemosRange();
		int result = 0;
		List<KeyValuePair<MemoType, int>> memosRange = _memosRange;
		int i = 0;
		for (int num = memosRange?.Count ?? 0; i < num; i++)
		{
			KeyValuePair<MemoType, int> keyValuePair = memosRange[i];
			if (keyValuePair.Key == type)
			{
				result = keyValuePair.Value;
				break;
			}
		}
		return result;
	}
}
