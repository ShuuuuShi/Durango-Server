using MsgPack;

namespace Messages;

public struct RemoveFromFavoriteProducts
{
	public const uint TypeCode = 423809u;

	public string ProductId;

	public static void Pack(Packer packer, RemoveFromFavoriteProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(423809u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ProductId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ProductId);
		}
	}

	public static RemoveFromFavoriteProducts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RemoveFromFavoriteProducts result = default(RemoveFromFavoriteProducts);
		result.ProductId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RemoveFromFavoriteProducts ProductId=" + ProductId + ">";
	}
}
