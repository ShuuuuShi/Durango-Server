using MsgPack;

namespace Messages;

public struct Equip
{
	public const uint TypeCode = 10u;

	public string SlotName;

	public ulong ItemId;

	public string Action;

	public static void Pack(Packer packer, Equip val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(10u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.SlotName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SlotName);
		}
		packer.Pack(val.ItemId);
		if (val.Action == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Action);
		}
	}

	public static Equip Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Equip result = default(Equip);
		result.SlotName = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ItemId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Action = ((MessagePackObject)(ref lastReadData3)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Equip SlotName={SlotName} ItemId={ItemId} Action={Action}>";
	}
}
