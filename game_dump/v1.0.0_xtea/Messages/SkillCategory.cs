using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct SkillCategory
{
	public int Level;

	public int Exp;

	public KeyValuePair<double, double>? Researching;

	public Gauge ResearchSkipCost;

	public static void Pack(Packer packer, SkillCategory val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.Pack(val.Level);
		packer.Pack(val.Exp);
		if (!val.Researching.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.Researching.Value.Key);
			packer.Pack(val.Researching.Value.Value);
		}
		if (val.ResearchSkipCost == null)
		{
			packer.PackNull();
		}
		else
		{
			Gauge.PackTo(val.ResearchSkipCost, packer);
		}
	}

	public static SkillCategory Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SkillCategory result = default(SkillCategory);
		result.Level = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Exp = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.Researching = null;
		}
		else
		{
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			double key = ((MessagePackObject)(ref lastReadData4)).AsDouble();
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			double value = ((MessagePackObject)(ref lastReadData5)).AsDouble();
			KeyValuePair<double, double> value2 = new KeyValuePair<double, double>(key, value);
			result.Researching = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData6)).IsNil)
		{
			result.ResearchSkipCost = null;
		}
		else
		{
			Gauge researchSkipCost = Gauge.UnpackFrom(unpacker);
			result.ResearchSkipCost = researchSkipCost;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SkillCategory Level={Level} Exp={Exp} Researching={Researching} ResearchSkipCost={ResearchSkipCost}>";
	}
}
