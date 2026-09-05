using MsgPack;

namespace Messages;

public struct RegisterProduct
{
	public const uint TypeCode = 2069u;

	public string ItemId;

	public long Price;

	public float Duration;

	public static void Pack(Packer packer, RegisterProduct val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2069u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
		packer.Pack(val.Price);
		packer.Pack(val.Duration);
	}

	public static RegisterProduct Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RegisterProduct result = default(RegisterProduct);
		result.ItemId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Price = unpacker.LastReadData.AsInt64();
		unpacker.Read();
		result.Duration = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<RegisterProduct ItemId={ItemId} Price={Price} Duration={Duration}>";
	}
}
