using MsgPack;

namespace Messages;

public struct AddToFavoriteProducts
{
	public const uint TypeCode = 123482u;

	public string ProductId;

	public static void Pack(Packer packer, AddToFavoriteProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(123482u);
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

	public static AddToFavoriteProducts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AddToFavoriteProducts result = default(AddToFavoriteProducts);
		result.ProductId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<AddToFavoriteProducts ProductId={ProductId}>";
	}
}
