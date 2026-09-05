using MsgPack;

namespace Messages;

public struct CurrentRegionCoOpTodos
{
	public const uint TypeCode = 241001u;

	public RegionCoOpTodo[] Todos;

	public static void Pack(Packer packer, CurrentRegionCoOpTodos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(241001u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Todos == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Todos.Length);
		for (int i = 0; i < val.Todos.Length; i++)
		{
			RegionCoOpTodo.Pack(packer, val.Todos[i]);
		}
	}

	public static CurrentRegionCoOpTodos Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		CurrentRegionCoOpTodos result = default(CurrentRegionCoOpTodos);
		result.Todos = new RegionCoOpTodo[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref RegionCoOpTodo reference = ref result.Todos[i];
			reference = RegionCoOpTodo.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CurrentRegionCoOpTodos Todos={Todos}>";
	}
}
