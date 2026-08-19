using MsgPack;
using Shared.Guide;

namespace Messages;

public struct ReissueOffer
{
	public const uint TypeCode = 3502u;

	public OfferType OfferType;

	public static void Pack(Packer packer, ReissueOffer val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3502u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.OfferType);
	}

	public static ReissueOffer Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		ReissueOffer result = default(ReissueOffer);
		if (num < 0 || 2 < num)
		{
			result.OfferType = OfferType.Invalid;
		}
		else
		{
			result.OfferType = (OfferType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ReissueOffer OfferType={OfferType}>";
	}
}
