using MsgPack;
using Shared.Economy;

namespace Messages;

public struct Route
{
	public string RegionId;

	public Money? Price;

	public static void Pack(Packer packer, Route val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		if (!val.Price.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.Price.Value.Amount);
		packer.Pack((int)val.Price.Value.Currency);
	}

	public static Route Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Route result = default(Route);
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Price = null;
		}
		else
		{
			unpacker.ReadInt32(out var result2);
			unpacker.ReadInt32(out var result3);
			Money value = new Money(result2, (Currency)result3);
			result.Price = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Route RegionId={RegionId} Price={Price}>";
	}
}
