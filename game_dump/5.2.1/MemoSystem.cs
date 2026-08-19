using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Clusters;
using Durango.Logic.Encyclopedia;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Memo;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class MemoSystem : GameSystem<MemoSystem>
{
	public struct EncyclopediaStorage
	{
		public MemoStorage Memo;
	}

	public struct MemoStorage
	{
		public List<KeyValuePair<Durango.Logic.Encyclopedia.MemoType, List<int>>> Memos;
	}

	public const string StorageKey = "encyclopedia";

	private readonly Dictionary<Durango.Logic.Encyclopedia.MemoType, BitArray> _activeMemoFlags = new Dictionary<Durango.Logic.Encyclopedia.MemoType, BitArray>(default(Durango.Logic.Encyclopedia.MemoTypeComparer));

	private readonly Dictionary<Durango.Logic.Encyclopedia.MemoType, List<Submemo>> _subMemoGroup = new Dictionary<Durango.Logic.Encyclopedia.MemoType, List<Submemo>>();

	public event Action<Durango.Logic.Encyclopedia.MemoType, int> MemoCollected;

	private void Awake()
	{
		Connections.Frontend.On<MemoCollected>(OnMemoCollect);
		Durango.Utils.Singleton<GameManager>.Instance().YamlLoaded += InitSubMemos;
		Durango.Utils.Singleton<GameManager>.Instance().WelcomeReceived += OnWelcome;
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			if (GameManager.ClusterMode == Mode.Online)
			{
				Connections.Frontend.Send(default(GetMemos)).On<Memos>(OnMemos);
			}
			else
			{
				MemosYaml instance = Yaml.Util.Singleton<MemosYaml>.Instance;
				Durango.Logic.Encyclopedia.MemoType[] memoTypes = MemosYaml.MemoTypes;
				foreach (Durango.Logic.Encyclopedia.MemoType memoType in memoTypes)
				{
					Dictionary<int, MemoInfo> subMemoFromType = instance.GetSubMemoFromType(memoType);
					if (subMemoFromType != null)
					{
						int length = subMemoFromType.Keys.Max();
						_activeMemoFlags[memoType] = new BitArray(length, defaultValue: true);
					}
				}
			}
		});
	}

	private Dictionary<Durango.Logic.Encyclopedia.MemoType, int> GetMemoRange()
	{
		Dictionary<Durango.Logic.Encyclopedia.MemoType, int> dictionary = new Dictionary<Durango.Logic.Encyclopedia.MemoType, int>();
		Array values = Enum.GetValues(typeof(Durango.Logic.Encyclopedia.MemoType));
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			Durango.Logic.Encyclopedia.MemoType memoType = (Durango.Logic.Encyclopedia.MemoType)values.GetValue(i);
			if (!IsServerMemo(memoType) && memoType != Durango.Logic.Encyclopedia.MemoType.Invalid)
			{
				int value = FindLastMemoIndex(memoType);
				dictionary[memoType] = value;
			}
		}
		return dictionary;
	}

	private void InitSubMemos()
	{
		MemosYaml instance = Yaml.Util.Singleton<MemosYaml>.Instance;
		Durango.Logic.Encyclopedia.MemoType[] memoTypes = MemosYaml.MemoTypes;
		foreach (Durango.Logic.Encyclopedia.MemoType memoType in memoTypes)
		{
			Dictionary<int, MemoInfo> subMemoFromType = instance.GetSubMemoFromType(memoType);
			if (subMemoFromType == null)
			{
				continue;
			}
			Dictionary<string, List<KeyValuePair<int, MemoInfo>>> dictionary = new Dictionary<string, List<KeyValuePair<int, MemoInfo>>>();
			foreach (KeyValuePair<int, MemoInfo> item2 in subMemoFromType)
			{
				string text = item2.Value.name;
				if (!string.IsNullOrEmpty(text) && text.Length >= 3)
				{
					int length = ((!char.IsDigit(text, text.Length - 2)) ? (text.Length - 2) : (text.Length - 3));
					text = text.Substring(0, length).TrimEnd();
					List<KeyValuePair<int, MemoInfo>> list = dictionary.Get(text);
					if (list == null)
					{
						list = new List<KeyValuePair<int, MemoInfo>>();
					}
					list.Add(item2);
					dictionary[text] = list;
				}
			}
			foreach (KeyValuePair<string, List<KeyValuePair<int, MemoInfo>>> item3 in dictionary)
			{
				List<int> list2 = new List<int>();
				List<float> list3 = new List<float>();
				List<KeyValuePair<int, MemoInfo>> value = item3.Value;
				for (int j = 0; j < value.Count; j++)
				{
					string text2 = value[j].Value.name;
					int num = ((!char.IsDigit(text2, text2.Length - 2)) ? (text2.Length - 2) : (text2.Length - 3));
					if (float.TryParse((num != -1) ? text2.Substring(num + 1) : string.Empty, out var result))
					{
						list3.Add(result);
						list2.Add(value[j].Key - 1);
					}
				}
				Submemo submemo = default(Submemo);
				submemo.Title = item3.Key;
				submemo.Indexes = list2.ToArray();
				submemo.Numbers = list3.ToArray();
				Submemo item = submemo;
				Array.Sort(item.Numbers, item.Indexes);
				if (!_subMemoGroup.ContainsKey(memoType))
				{
					_subMemoGroup[memoType] = new List<Submemo>();
				}
				_subMemoGroup[memoType].Add(item);
			}
		}
	}

	private void OnMemos(Memos msg, PacketHeader header)
	{
		foreach (KeyValuePair<Shared.Memo.MemoType, BitArray> collectedMemo in msg.CollectedMemos)
		{
			Durango.Logic.Encyclopedia.MemoType key = MemosYaml.ToClientMemoType(collectedMemo.Key);
			_activeMemoFlags[key] = new BitArray(collectedMemo.Value);
		}
	}

	private void OnWelcome(Welcome welcome)
	{
		byte[] array = welcome.Storage.Data?.Get("encyclopedia");
		if (array == null)
		{
			return;
		}
		List<KeyValuePair<Durango.Logic.Encyclopedia.MemoType, List<int>>> memos = Json.Read<EncyclopediaStorage>(array).Memo.Memos;
		Dictionary<Durango.Logic.Encyclopedia.MemoType, int> memoRange = GetMemoRange();
		int i = 0;
		for (int size = KUtility.GetSize(memos); i < size; i++)
		{
			int num = memoRange[memos[i].Key];
			BitArray bitArray = new BitArray(num + 1);
			int j = 0;
			for (int size2 = KUtility.GetSize(memos[i].Value); j < size2; j++)
			{
				int num2 = memos[i].Value[j];
				if (num2 >= 0 && num2 <= num)
				{
					bitArray[num2] = true;
				}
			}
			_activeMemoFlags[memos[i].Key] = bitArray;
		}
	}

	private void SaveStorage()
	{
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(OnSaveStorage);
	}

	private void OnSaveStorage()
	{
		SetStorageItem msg = default(SetStorageItem);
		msg.Key = "encyclopedia";
		EncyclopediaStorage data = default(EncyclopediaStorage);
		data.Memo.Memos = new List<KeyValuePair<Durango.Logic.Encyclopedia.MemoType, List<int>>>();
		foreach (KeyValuePair<Durango.Logic.Encyclopedia.MemoType, BitArray> activeMemoFlag in _activeMemoFlags)
		{
			Durango.Logic.Encyclopedia.MemoType key = activeMemoFlag.Key;
			if (IsServerMemo(key))
			{
				continue;
			}
			List<int> list = new List<int>();
			for (int i = 0; i < activeMemoFlag.Value.Length; i++)
			{
				if (activeMemoFlag.Value[i])
				{
					list.Add(i);
				}
			}
			data.Memo.Memos.Add(new KeyValuePair<Durango.Logic.Encyclopedia.MemoType, List<int>>(key, list));
		}
		msg.Value = Json.WriteToBytes(data);
		Connections.Frontend.Send(msg);
	}

	[CanBeNull]
	public List<Submemo> GetSubMemos(Durango.Logic.Encyclopedia.MemoType type)
	{
		return _subMemoGroup.Get(type);
	}

	[NotNull]
	public BitArray GetActiveMemoFlags(Durango.Logic.Encyclopedia.MemoType type)
	{
		if (!_activeMemoFlags.ContainsKey(type))
		{
			int num = FindLastMemoIndex(type);
			_activeMemoFlags[type] = new BitArray(num + 1);
		}
		return _activeMemoFlags[type];
	}

	private void UpdateMemoList(Durango.Logic.Encyclopedia.MemoType type, int index)
	{
		int num = FindLastMemoIndex(type);
		if (index > num)
		{
			ExpandMemoList(type, index);
		}
		GetActiveMemoFlags(type)[index] = true;
	}

	private void ExpandMemoList(Durango.Logic.Encyclopedia.MemoType type, int expandedSize)
	{
		BitArray bitArray = new BitArray(expandedSize + 1);
		if (!_activeMemoFlags.ContainsKey(type))
		{
			_activeMemoFlags.Add(type, bitArray);
			return;
		}
		for (int i = 0; i < _activeMemoFlags[type].Count; i++)
		{
			bitArray[i] = _activeMemoFlags[type][i];
		}
		_activeMemoFlags[type] = bitArray;
	}

	public int SubMemoIndexOf(Durango.Logic.Encyclopedia.MemoType type, int memoId)
	{
		List<Submemo> subMemos = GetSubMemos(type);
		if (subMemos == null)
		{
			return -1;
		}
		for (int i = 0; i < subMemos.Count; i++)
		{
			Submemo submemo = subMemos[i];
			for (int j = 0; j < submemo.Indexes.Length; j++)
			{
				if (submemo.Indexes[j] == memoId)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private void OnMemoCollect(MemoCollected msg, PacketHeader header)
	{
		int num = msg.Number - 1;
		Durango.Logic.Encyclopedia.MemoType memoType = MemosYaml.ToClientMemoType(msg.MemoType);
		UpdateMemoList(memoType, num);
		if (this.MemoCollected != null)
		{
			this.MemoCollected(memoType, num);
		}
	}

	private int FindLastMemoIndex(Durango.Logic.Encyclopedia.MemoType type)
	{
		if (_activeMemoFlags.ContainsKey(type))
		{
			return _activeMemoFlags[type].Count - 1;
		}
		int num = 0;
		if (!IsServerMemo(type))
		{
			string text = "#" + GetLocalizePostfix(type);
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

	public static void SetMemoAvailable(Durango.Logic.Encyclopedia.MemoType type, int index)
	{
		if (GameSystem<MemoSystem>.HasInstance() && !IsServerMemo(type))
		{
			MemoSystem memoSystem = GameSystem<MemoSystem>.Instance();
			memoSystem.UpdateMemoList(type, index);
			memoSystem.SaveStorage();
			if (memoSystem.MemoCollected != null)
			{
				memoSystem.MemoCollected(type, index);
			}
		}
	}

	public static string GetMemoFullText(Durango.Logic.Encyclopedia.MemoType type, int index)
	{
		string memoTitle = GetMemoTitle(type, index);
		string memoText = GetMemoText(type, index);
		return "[size=40][c29e73FF]" + memoTitle + "[-][/size]\n[9f9282FF]" + memoText + "[-]";
	}

	public static string GetMemoTitle(Durango.Logic.Encyclopedia.MemoType type, int zeroIndex)
	{
		if (IsServerMemo(type))
		{
			return GetTitleByIndex(type, zeroIndex);
		}
		return T._(type.GetName(), zeroIndex);
	}

	public static string GetMemoText(Durango.Logic.Encyclopedia.MemoType type, int zeroIndex)
	{
		if (IsServerMemo(type))
		{
			return GetContentByIndex(type, zeroIndex);
		}
		string text = $"#{GetLocalizePostfix(type)}_{zeroIndex}";
		string text2 = LocalizeSystem.Get(text);
		if (text == text2)
		{
			return string.Empty;
		}
		return text2;
	}

	private static int IndexToKey(Durango.Logic.Encyclopedia.MemoType type, int index)
	{
		if (type == Durango.Logic.Encyclopedia.MemoType.Collect || type == Durango.Logic.Encyclopedia.MemoType.Faction)
		{
			return index + 1;
		}
		return index;
	}

	private static string GetTitleByIndex(Durango.Logic.Encyclopedia.MemoType type, int zeroIndex)
	{
		Dictionary<int, MemoInfo> subMemoFromType = Yaml.Util.Singleton<MemosYaml>.Instance.GetSubMemoFromType(type);
		if (subMemoFromType == null)
		{
			return string.Empty;
		}
		int key = IndexToKey(type, zeroIndex);
		MemoInfo memoInfo = subMemoFromType.Get(key);
		if (memoInfo != null)
		{
			return memoInfo.name;
		}
		return string.Empty;
	}

	private static string GetContentByIndex(Durango.Logic.Encyclopedia.MemoType type, int zeroIndex)
	{
		Dictionary<int, MemoInfo> subMemoFromType = Yaml.Util.Singleton<MemosYaml>.Instance.GetSubMemoFromType(type);
		if (subMemoFromType == null)
		{
			return string.Empty;
		}
		int key = IndexToKey(type, zeroIndex);
		MemoInfo memoInfo = subMemoFromType.Get(key);
		if (memoInfo != null)
		{
			return memoInfo.content;
		}
		return string.Empty;
	}

	public static bool IsServerMemo(Durango.Logic.Encyclopedia.MemoType type)
	{
		return type switch
		{
			Durango.Logic.Encyclopedia.MemoType.Collect => true, 
			Durango.Logic.Encyclopedia.MemoType.Faction => true, 
			_ => false, 
		};
	}

	private static string GetLocalizePostfix(Durango.Logic.Encyclopedia.MemoType type)
	{
		return type switch
		{
			Durango.Logic.Encyclopedia.MemoType.Fiction => "fiction", 
			Durango.Logic.Encyclopedia.MemoType.Tooltip => "shortcut_tooltip", 
			Durango.Logic.Encyclopedia.MemoType.Survival => "survival_memo", 
			Durango.Logic.Encyclopedia.MemoType.Collect => "collect", 
			Durango.Logic.Encyclopedia.MemoType.Faction => "faction", 
			_ => null, 
		};
	}

	public static int GetRandomMemo(Durango.Logic.Encyclopedia.MemoType type, bool save = true)
	{
		if (!GameSystem<MemoSystem>.HasInstance())
		{
			return -1;
		}
		int num = GameSystem<MemoSystem>.Instance().FindLastMemoIndex(type);
		if (num == 0)
		{
			return -1;
		}
		for (int i = 0; i < 100; i++)
		{
			int num2 = UnityEngine.Random.Range(0, num);
			if (!string.IsNullOrEmpty(GetMemoText(type, num2)))
			{
				if (save)
				{
					SetMemoAvailable(type, num2);
				}
				return num2;
			}
		}
		return -1;
	}
}
