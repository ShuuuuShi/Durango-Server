using System.Collections.Generic;
using MsgPack;
using Shared.Guide;

namespace Messages;

public struct Offers
{
	public const uint TypeCode = 3499u;

	public string TargetTitleId;

	public KeyValuePair<int, int> Progress;

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
		packer.Pack(val.Progress.Key);
		packer.Pack(val.Progress.Value);
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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Offers result = default(Offers);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.TargetTitleId = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string targetTitleId = ((MessagePackObject)(ref lastReadData2)).AsString();
			result.TargetTitleId = targetTitleId;
		}
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int key = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int value = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.Progress = new KeyValuePair<int, int>(key, value);
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		result._Offers = new Dictionary<OfferType, TodoTemplate>(num, default(OfferTypeComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
			OfferType key2 = ((num2 >= 0 && 2 >= num2) ? ((OfferType)num2) : OfferType.Invalid);
			unpacker.Read();
			TodoTemplate value2 = TodoTemplate.Unpack(unpacker);
			result._Offers.Add(key2, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Offers TargetTitleId={TargetTitleId} Progress={Progress} _Offers={_Offers}>";
	}
}
