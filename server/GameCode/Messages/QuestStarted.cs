using MsgPack;

namespace Messages;

public struct QuestStarted
{
	public const uint TypeCode = 879361u;

	public string Category;

	public QuestToDo[] Quests;

	public static void Pack(Packer packer, QuestStarted val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(879361u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Category == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Category);
		}
		if (val.Quests == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Quests.Length);
		for (int i = 0; i < val.Quests.Length; i++)
		{
			QuestToDo.Pack(packer, val.Quests[i]);
		}
	}

	public static QuestStarted Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		QuestStarted result = default(QuestStarted);
		result.Category = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Quests = new QuestToDo[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref QuestToDo reference = ref result.Quests[i];
			reference = QuestToDo.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<QuestStarted Category={Category} Quests={Quests}>";
	}
}
