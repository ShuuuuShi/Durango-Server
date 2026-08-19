using System.Collections.Generic;
using MsgPack;
using Shared.Faction;

namespace Messages;

public struct SupportRequests
{
	public const uint TypeCode = 523413u;

	public SupportRequest[] Requests;

	public double EndAt;

	public Dictionary<FactionType, double> Untils;

	public FactionType[] FactionTypes;

	public int Level;

	public static void Pack(Packer packer, SupportRequests val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(523413u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.Requests == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Requests.Length);
			for (int i = 0; i < val.Requests.Length; i++)
			{
				SupportRequest.Pack(packer, val.Requests[i]);
			}
		}
		packer.Pack(val.EndAt);
		if (val.Untils == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Untils.Count);
			foreach (KeyValuePair<FactionType, double> until in val.Untils)
			{
				packer.Pack((int)until.Key);
				packer.Pack(until.Value);
			}
		}
		if (val.FactionTypes == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.FactionTypes.Length);
			for (int j = 0; j < val.FactionTypes.Length; j++)
			{
				packer.Pack((int)val.FactionTypes[j]);
			}
		}
		packer.Pack(val.Level);
	}

	public static SupportRequests Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SupportRequests result = default(SupportRequests);
		result.Requests = new SupportRequest[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref SupportRequest reference = ref result.Requests[i];
			reference = SupportRequest.Unpack(unpacker);
		}
		unpacker.Read();
		result.EndAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Untils = new Dictionary<FactionType, double>(num2, default(FactionTypeComparer));
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			FactionType key = ((num3 >= 0 && 101 >= num3) ? ((FactionType)num3) : FactionType.Invalid);
			unpacker.Read();
			double value = unpacker.LastReadData.AsDouble();
			result.Untils.Add(key, value);
		}
		unpacker.Read();
		int num4 = unpacker.LastReadData.AsInt32();
		result.FactionTypes = new FactionType[num4];
		for (int k = 0; k < num4; k++)
		{
			unpacker.Read();
			int num5 = unpacker.LastReadData.AsInt32();
			if (num5 < 0 || 101 < num5)
			{
				result.FactionTypes[k] = FactionType.Invalid;
			}
			else
			{
				result.FactionTypes[k] = (FactionType)num5;
			}
		}
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SupportRequests Requests={Requests} EndAt={EndAt} Untils={Untils} FactionTypes={FactionTypes} Level={Level}>";
	}
}
