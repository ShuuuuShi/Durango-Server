using MsgPack;

namespace Messages;

public struct SetRecipeLike
{
	public const uint TypeCode = 3800u;

	public string RecipeId;

	public bool Like;

	public static void Pack(Packer packer, SetRecipeLike val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3800u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.RecipeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RecipeId);
		}
		packer.Pack(val.Like);
	}

	public static SetRecipeLike Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetRecipeLike result = default(SetRecipeLike);
		result.RecipeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Like = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<SetRecipeLike RecipeId={RecipeId} Like={Like}>";
	}
}
