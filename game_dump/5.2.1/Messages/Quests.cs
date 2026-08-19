using MsgPack;

namespace Messages;

public struct Quests
{
	public const uint TypeCode = 237919u;

	public string Category;

	public QuestToDo[] Todos;

	public static void Pack(Packer packer, Quests val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(237919u);
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
		if (val.Todos == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Todos.Length);
		for (int i = 0; i < val.Todos.Length; i++)
		{
			QuestToDo.Pack(packer, val.Todos[i]);
		}
	}

	public static Quests Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Quests result = default(Quests);
		result.Category = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Todos = new QuestToDo[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref QuestToDo reference = ref result.Todos[i];
			reference = QuestToDo.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Quests Category={Category} Todos={Todos}>";
	}
}
