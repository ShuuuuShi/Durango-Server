using System.Collections.Generic;
using Durango.Logic.Encyclopedia;
using JetBrains.Annotations;
using Shared.Memo;
using Yaml.Util;

namespace Yaml;

public class MemosYaml : Singleton<MemosYaml>
{
	public MemoGroupDictionary memos;

	public static Durango.Logic.Encyclopedia.MemoType[] MemoTypes = new Durango.Logic.Encyclopedia.MemoType[2]
	{
		Durango.Logic.Encyclopedia.MemoType.Collect,
		Durango.Logic.Encyclopedia.MemoType.Faction
	};

	public static Durango.Logic.Encyclopedia.MemoType ToClientMemoType(Shared.Memo.MemoType memoType)
	{
		return memoType switch
		{
			Shared.Memo.MemoType.Collect => Durango.Logic.Encyclopedia.MemoType.Collect, 
			Shared.Memo.MemoType.Faction => Durango.Logic.Encyclopedia.MemoType.Faction, 
			_ => Durango.Logic.Encyclopedia.MemoType.Invalid, 
		};
	}

	public static Shared.Memo.MemoType ToServerMemoType(Durango.Logic.Encyclopedia.MemoType memoType)
	{
		return memoType switch
		{
			Durango.Logic.Encyclopedia.MemoType.Collect => Shared.Memo.MemoType.Collect, 
			Durango.Logic.Encyclopedia.MemoType.Faction => Shared.Memo.MemoType.Faction, 
			_ => Shared.Memo.MemoType.Invalid, 
		};
	}

	[CanBeNull]
	public Dictionary<int, MemoInfo> GetSubMemoFromType(Shared.Memo.MemoType type)
	{
		if (memos != null)
		{
			return memos.Get(type);
		}
		return null;
	}

	[CanBeNull]
	public Dictionary<int, MemoInfo> GetSubMemoFromType(Durango.Logic.Encyclopedia.MemoType type)
	{
		return GetSubMemoFromType(ToServerMemoType(type));
	}
}
