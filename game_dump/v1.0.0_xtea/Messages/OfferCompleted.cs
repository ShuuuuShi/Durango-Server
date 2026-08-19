using MsgPack;
using Shared.Guide;

namespace Messages;

public struct OfferCompleted
{
	public const uint TypeCode = 3501u;

	public OfferType OfferType;

	public TodoTemplate Offer;

	public TodoTemplate? NewOffer;

	public static void Pack(Packer packer, OfferCompleted val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3501u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.OfferType);
		TodoTemplate.Pack(packer, val.Offer);
		if (!val.NewOffer.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			TodoTemplate.Pack(packer, val.NewOffer.Value);
		}
	}

	public static OfferCompleted Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		OfferCompleted result = default(OfferCompleted);
		if (num < 0 || 2 < num)
		{
			result.OfferType = OfferType.Invalid;
		}
		else
		{
			result.OfferType = (OfferType)num;
		}
		unpacker.Read();
		result.Offer = TodoTemplate.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.NewOffer = null;
		}
		else
		{
			TodoTemplate value = TodoTemplate.Unpack(unpacker);
			result.NewOffer = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<OfferCompleted OfferType={OfferType} Offer={Offer} NewOffer={NewOffer}>";
	}
}
