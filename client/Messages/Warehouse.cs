using MsgPack;

namespace Messages;

public struct Warehouse
{
	public const uint TypeCode = 3684u;

	public string EntityId;

	public InventorySectionInfos[] SectionInfos;

	public static void Pack(Packer packer, Warehouse val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3684u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.SectionInfos == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.SectionInfos.Length);
		for (int i = 0; i < val.SectionInfos.Length; i++)
		{
			InventorySectionInfos.Pack(packer, val.SectionInfos[i]);
		}
	}

	public static Warehouse Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Warehouse result = default(Warehouse);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.SectionInfos = new InventorySectionInfos[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref InventorySectionInfos reference = ref result.SectionInfos[i];
			reference = InventorySectionInfos.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Warehouse EntityId={EntityId} SectionInfos={SectionInfos}>";
	}
}
