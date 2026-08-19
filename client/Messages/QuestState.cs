using System.Collections.Generic;
using MsgPack;
using Shared.Quest;

namespace Messages;

public struct QuestState
{
	public const uint TypeCode = 398133u;

	public Dictionary<string, Shared.Quest.QuestState> States;

	public static void Pack(Packer packer, QuestState val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(398133u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.States == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.States.Count);
		foreach (KeyValuePair<string, Shared.Quest.QuestState> state in val.States)
		{
			if (state.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(state.Key);
			}
			packer.Pack((int)state.Value);
		}
	}

	public static QuestState Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		QuestState result = default(QuestState);
		result.States = new Dictionary<string, Shared.Quest.QuestState>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Shared.Quest.QuestState value = ((num2 >= 0 && 3 >= num2) ? ((Shared.Quest.QuestState)num2) : Shared.Quest.QuestState.Invalid);
			result.States.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<QuestState States={States}>";
	}
}
