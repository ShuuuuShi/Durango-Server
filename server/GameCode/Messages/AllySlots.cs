using MsgPack;

namespace Messages;

public struct AllySlots
{
	public const uint TypeCode = 9138746u;

	public AllySlot[] Slots;

	public static void Pack(Packer packer, AllySlots val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(9138746u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Slots == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Slots.Length);
		for (int i = 0; i < val.Slots.Length; i++)
		{
			AllySlot.Pack(packer, val.Slots[i]);
		}
	}

	public static AllySlots Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AllySlots result = default(AllySlots);
		result.Slots = new AllySlot[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref AllySlot reference = ref result.Slots[i];
			reference = AllySlot.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AllySlots Slots={Slots}>";
	}
}
