using MsgPack;

namespace Messages;

public struct GetQuestState
{
	public const uint TypeCode = 398132u;

	public string[] QuestIds;

	public static void Pack(Packer packer, GetQuestState val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(398132u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.QuestIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.QuestIds.Length);
		for (int i = 0; i < val.QuestIds.Length; i++)
		{
			if (val.QuestIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.QuestIds[i]);
			}
		}
	}

	public static GetQuestState Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GetQuestState result = default(GetQuestState);
		result.QuestIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.QuestIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		object[] questIds = QuestIds;
		return string.Format("<GetQuestState QuestIds={0}>", questIds);
	}
}
