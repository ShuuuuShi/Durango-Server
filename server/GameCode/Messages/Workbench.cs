using MsgPack;

namespace Messages;

public struct Workbench
{
	public const uint TypeCode = 3000u;

	public string EntityId;

	public uint Capacity;

	public Crafting[] Craftings;

	public CraftedResult[] Crafteds;

	public static void Pack(Packer packer, Workbench val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3000u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.Capacity);
		if (val.Craftings == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Craftings.Length);
			for (int i = 0; i < val.Craftings.Length; i++)
			{
				Crafting.Pack(packer, val.Craftings[i]);
			}
		}
		if (val.Crafteds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Crafteds.Length);
		for (int j = 0; j < val.Crafteds.Length; j++)
		{
			CraftedResult.Pack(packer, val.Crafteds[j]);
		}
	}

	public static Workbench Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Workbench result = default(Workbench);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Capacity = unpacker.LastReadData.AsUInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Craftings = new Crafting[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Crafting reference = ref result.Craftings[i];
			reference = Crafting.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Crafteds = new CraftedResult[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref CraftedResult reference2 = ref result.Crafteds[j];
			reference2 = CraftedResult.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Workbench EntityId={EntityId} Capacity={Capacity} Craftings={Craftings} Crafteds={Crafteds}>";
	}
}
