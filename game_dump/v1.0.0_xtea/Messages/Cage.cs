using MsgPack;

namespace Messages;

public struct Cage
{
	public const uint TypeCode = 811u;

	public byte CageSize;

	public byte CageRemainSize;

	public Item[] CagedReins;

	public static void Pack(Packer packer, Cage val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(811u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.CageSize);
		packer.Pack(val.CageRemainSize);
		if (val.CagedReins == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.CagedReins.Length);
		for (int i = 0; i < val.CagedReins.Length; i++)
		{
			Item.Pack(packer, val.CagedReins[i]);
		}
	}

	public static Cage Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Cage result = default(Cage);
		result.CageSize = ((MessagePackObject)(ref lastReadData)).AsByte();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.CageRemainSize = ((MessagePackObject)(ref lastReadData2)).AsByte();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.CagedReins = new Item[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.CagedReins[i];
			reference = Item.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Cage CageSize={CageSize} CageRemainSize={CageRemainSize} CagedReins={CagedReins}>";
	}
}
