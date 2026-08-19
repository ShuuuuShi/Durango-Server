using MsgPack;

namespace Messages;

public struct CraftTodo
{
	public const uint TypeCode = 3520u;

	public string RecipeId;

	public static void Pack(Packer packer, CraftTodo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3520u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.RecipeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RecipeId);
		}
	}

	public static CraftTodo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CraftTodo result = default(CraftTodo);
		result.RecipeId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<CraftTodo RecipeId=" + RecipeId + ">";
	}
}
