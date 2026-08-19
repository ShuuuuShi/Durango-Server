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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		OfferCompletedEffect result = default(OfferCompletedEffect);
		if (num < 0 || 9 < num)
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
		return $"<OfferCompletedEffect Type={Type} Offer={Offer} NewOffer={NewOffer}>";
	}
}
