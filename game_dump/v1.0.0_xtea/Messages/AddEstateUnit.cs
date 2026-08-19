using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct AddEstateUnit
{
	public const uint TypeCode = 2421u;

	public ulong EstateId;

	public KeyValuePair<int, int> Unit;

	public static void Pack(Packer packer, AddEstateUnit val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2421u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EstateId);
		packer.PackArrayHeader(2);
		packer.Pack(val.Unit.Key);
		packer.Pack(val.Unit.Value);
	}

	public static AddEstateUnit Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		AddEstateUnit result = default(AddEstateUnit);
		result.EstateId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int key = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int value = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.Unit = new KeyValuePair<int, int>(key, value);
		return result;
	}

	public override string ToString()
	{
		return $"<AddEstateUnit EstateId={EstateId} Unit={Unit}>";
	}
}
