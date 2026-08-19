using MsgPack;

namespace Messages;

public struct Route
{
	public ulong RegionId;

	public Price? Price;

	public static void Pack(Packer packer, Route val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.RegionId);
		if (!val.Price.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Price.Pack(packer, val.Price.Value);
		}
	}

	public static Route Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Route result = default(Route);
		result.RegionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.Price = null;
		}
		else
		{
			Price value = Messages.Price.Unpack(unpacker);
			result.Price = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Route RegionId={RegionId} Price={Price}>";
	}
}
