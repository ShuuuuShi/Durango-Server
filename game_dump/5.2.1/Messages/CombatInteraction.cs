using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct CombatInteraction
{
	public const uint TypeCode = 23u;

	public string EntityId;

	public string TargetId;

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
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.TargetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TargetId);
		}
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
		unpacker.Read();
		CombatInteraction result = default(CombatInteraction);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TargetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Details = new Dictionary<string, long>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			long value = unpacker.LastReadData.AsInt64();
			result.Details.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CombatInteraction EntityId={EntityId} TargetId={TargetId} Details={Details}>";
	}
}
