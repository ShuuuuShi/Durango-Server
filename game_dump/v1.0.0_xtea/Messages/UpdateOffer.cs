using MsgPack;
using Shared.Guide;

namespace Messages;

public struct UpdateOffer
{
	public const uint TypeCode = 3500u;

	public OfferType OfferType;

	public TodoTemplate Offer;

	public static void Pack(Packer packer, UpdateOffer val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3500u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.OfferType);
		TodoTemplate.Pack(packer, val.Offer);
	}

	public static UpdateOffer Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		UpdateOffer result = default(UpdateOffer);
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
		return result;
	}

	public override string ToString()
	{
		return $"<UpdateOffer OfferType={OfferType} Offer={Offer}>";
	}
}
