using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct CombatInteraction
{
	public const uint TypeCode = 23u;

	public ulong EntityId;

	public ulong TargetId;

	public Dictionary<string, long> Details;

	public static void Pack(Packer packer, CombatInteraction val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(23u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.TargetId);
		if (val.Details == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Details.Count);
		foreach (KeyValuePair<string, long> detail in val.Details)
		{
			if (detail.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(detail.Key);
			}
			packer.Pack(detail.Value);
		}
	}

	public static CombatInteraction Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		CombatInteraction result = default(CombatInteraction);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.TargetId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.Details = new Dictionary<string, long>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData4)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			long value = ((MessagePackObject)(ref lastReadData5)).AsInt64();
			result.Details.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CombatInteraction EntityId={EntityId} TargetId={TargetId} Details={Details}>";
	}
}
