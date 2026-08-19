using MsgPack;
using Shared.System;

namespace Messages;

public struct OfferCompletedEffect
{
	public const uint TypeCode = 2066u;

	public Shared.System.RewardEffect Type;

	public TodoTemplate Offer;

	public TodoTemplate? NewOffer;

	public static void Pack(Packer packer, OfferCompletedEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2066u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Type);
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

	public static OfferCompletedEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		OfferCompletedEffect result = default(OfferCompletedEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.Offer = TodoTemplate.Unpack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
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
		return $"<OfferCompletedEffect Type={Type} Offer={Offer} NewOffer={NewOffer}>";
	}
}
