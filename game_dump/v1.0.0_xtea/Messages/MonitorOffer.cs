using MsgPack;
using Shared.Guide;

namespace Messages;

public struct MonitorOffer
{
	public const uint TypeCode = 3504u;

	public OfferType OfferType;

	public bool Monitoring;

	public static void Pack(Packer packer, MonitorOffer val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3504u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.OfferType);
		packer.Pack(val.Monitoring);
	}

	public static MonitorOffer Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		MonitorOffer result = default(MonitorOffer);
		if (num < 0 || 2 < num)
		{
			result.OfferType = OfferType.Invalid;
		}
		else
		{
			result.OfferType = (OfferType)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Monitoring = ((MessagePackObject)(ref lastReadData2)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<MonitorOffer OfferType={OfferType} Monitoring={Monitoring}>";
	}
}
