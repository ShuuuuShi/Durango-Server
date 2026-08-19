using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct FarmingEncyclopediaData
{
	public bool IsCropLocked;

	public int CurrentExp;

	public int CurrentLevel;

	public int CurrentLevelExpThreshold;

	public int NextLevelExpThreshold;

	public Dictionary<int, int> MasteryLevelToIndex;

	public static void Pack(Packer packer, FarmingEncyclopediaData val, bool hint = false)
	{
		packer.PackArrayHeader(6);
		packer.Pack(val.IsCropLocked);
		packer.Pack(val.CurrentExp);
		packer.Pack(val.CurrentLevel);
		packer.Pack(val.CurrentLevelExpThreshold);
		packer.Pack(val.NextLevelExpThreshold);
		if (val.MasteryLevelToIndex == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.MasteryLevelToIndex.Count);
		foreach (KeyValuePair<int, int> item in val.MasteryLevelToIndex)
		{
			packer.Pack(item.Key);
			packer.Pack(item.Value);
		}
	}

	public static FarmingEncyclopediaData Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		FarmingEncyclopediaData result = default(FarmingEncyclopediaData);
		result.IsCropLocked = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.CurrentExp = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.CurrentLevel = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.CurrentLevelExpThreshold = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.NextLevelExpThreshold = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.MasteryLevelToIndex = new Dictionary<int, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int key = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.MasteryLevelToIndex.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FarmingEncyclopediaData IsCropLocked={IsCropLocked} CurrentExp={CurrentExp} CurrentLevel={CurrentLevel} CurrentLevelExpThreshold={CurrentLevelExpThreshold} NextLevelExpThreshold={NextLevelExpThreshold} MasteryLevelToIndex={MasteryLevelToIndex}>";
	}
}
