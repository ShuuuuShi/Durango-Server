using System.Collections.Generic;
using MsgPack;
using Shared.Guide;

namespace Messages;

public struct Offers
{
	public const uint TypeCode = 3499u;

	public string TargetTitleId;

	public Pair<int, int> Progress;

	public Dictionary<OfferType, TodoTemplate> _Offers;

	public static void Pack(Packer packer, Offers val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3499u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.TargetTitleId == null)
		{
			packer.PackNull();
		}
		else if (val.TargetTitleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TargetTitleId);
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.Progress.Item1);
		packer.Pack(val.Progress.Item2);
		if (val._Offers == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val._Offers.Count);
		foreach (KeyValuePair<OfferType, TodoTemplate> offer in val._Offers)
		{
			packer.Pack((int)offer.Key);
			TodoTemplate.Pack(packer, offer.Value);
		}
	}

	public static Offers Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Offers result = default(Offers);
		if (unpacker.LastReadData.IsNil)
		{
			result.TargetTitleId = null;
		}
		else
		{
			string targetTitleId = unpacker.LastReadData.AsString();
			result.TargetTitleId = targetTitleId;
		}
		unpacker.Read();
		unpacker.Read();
		int item = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int item2 = unpacker.LastReadData.AsInt32();
		result.Progress = new Pair<int, int>(item, item2);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result._Offers = new Dictionary<OfferType, TodoTemplate>(num, default(OfferTypeComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			OfferType key = ((num2 >= 0 && 2 >= num2) ? ((OfferType)num2) : OfferType.Invalid);
			unpacker.Read();
			TodoTemplate value = TodoTemplate.Unpack(unpacker);
			result._Offers.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Offers TargetTitleId={TargetTitleId} Progress={Progress} _Offers={_Offers}>";
	}
}
